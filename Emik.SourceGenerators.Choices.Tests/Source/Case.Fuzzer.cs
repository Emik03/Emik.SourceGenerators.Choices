// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Tests;

using Test = (string Structure, string TypeKeyword, INamedTypeSymbol First, INamedTypeSymbol Second, Verify Verify);

public partial class Case
{
    public sealed class Fuzzer(ITestOutputHelper output)
    {
        const string DotPattern = // language=cs
            """
            [Choice.First<{1}>.Second<{2}>, Obsolete]
            public partial {0} Test;
            """;

        const string PrimaryConstructors = // language=cs
            """
            [Choice, Obsolete]
            public partial {0} Test({1} first, {2} second);
            """;

        const string Fields = // language=cs
            """
            [Choice, Obsolete]
            public partial {0} Test
            {{
                {1} _first;
            
                {2} _second;
            }}
            """;

        const string Properties = // language=cs
            """
            [Choice, Obsolete]
            public partial {0} Test
            {{
                public {1} First {{ get; }}

                public {2} Second {{ get; }}
            }}
            """;

        static IEnumerable<MetadataReference> References { get; } =
            Net90.References.All.Where(x => x.Display?.Contains("System") is true);

        static ImmutableArray<string> Structures { get; } = [DotPattern, Fields, PrimaryConstructors, Properties];

        static ImmutableArray<string> TypeKeywords { get; } = ["class", "record", "record struct", "struct"];

        static ImmutableArray<INamedTypeSymbol> AccessibleTypes { get; } =
        [
            ..CSharpCompilation.Create("tmp")
               .AddReferences(References)
               .GlobalNamespace
               .GetMembers()
               .SelectMany(GetMembers)
               .OfType<INamedTypeSymbol>()
               .Where(IsNonGenericInstance)
               .Where(x => GetBaseTypes(x).All(x => x.DeclaredAccessibility is Accessibility.Public)),
        ];

        [StressfulFact]
        public Task RunSingleAsync() => EnumerateAsync(1);

        [StressfulFact]
        public Task RunDoubleAsync() => EnumerateAsync(2);

        [StressfulFact]
        public Task RunQuadrupleAsync() => EnumerateAsync(4);

        [StressfulFact]
        public Task RunOctupleAsync() => EnumerateAsync(8);

        [StressfulFact]
        public Task RunAllAsync() => EnumerateAsync(int.MaxValue);

        async Task EnumerateAsync(int size)
        {
            var pass = FlattenedLength(AccessibleTypes.Length, size);
            var length = pass * Structures.Length * TypeKeywords.Length;
            var milestone = pass;
            Test fail = default;
            var i = 0;

            async Task RunAsync(Test x)
            {
                try // ReSharper disable once AccessToModifiedClosure
                {
                    await x.Verify.RunAsync();
                    Interlocked.Increment(ref i);
                }
                catch (Exception)
                {
                    fail = x;
                    throw;
                }
            }

            var tasks = AccessibleTypes.Chunk(size)
               .SelectMany(Query)
               .Chunk(Environment.ProcessorCount)
               .Select(x => Task.WhenAll(x.Select(RunAsync)));

            output.WriteLine($"Running {length} micro-tests.");

            try
            {
                foreach (var task in tasks)
                {
                    await task;

                    if (i < milestone)
                        continue;

                    output.WriteLine($"Successfully ran {i}/{length} micro-tests. ({(float)i / length:P})");
                    milestone += pass;
                }

                output.WriteLine("All micro-tests passed. Yippee!");
            }
            catch (Exception e)
            {
                Throw(e, fail, i, length);
            }
        }

        static void Throw(Exception e, Test fail, int i, int length)
        {
            var (structure, typeKeyword, first, second, verify) = fail;

            bool MatchesKind(ISymbol x, int i) =>
                structure switch
                {
                    Fields => x is IFieldSymbol,
                    Properties => x is IPropertySymbol,
                    PrimaryConstructors => x is IParameterSymbol,
                    _ => i < 2,
                };

            var syntaxTrees = CSharpSyntaxTree.ParseText(Wrap(string.Format(structure, typeKeyword, first, second)));

            var type = CSharpCompilation.Create("tmp", [syntaxTrees])
               .AddReferences(References)
               .GlobalNamespace
               .GetNamespaceMembers()
               .Single(x => x.Name is nameof(Emik))
               .GetNamespaceMembers()
               .Single(x => x.Name is nameof(SourceGenerators))
               .GetNamespaceMembers()
               .Single(x => x.Name is nameof(Choices))
               .GetNamespaceMembers()
               .Single()
               .GetTypeMembers()
               .Single();

            var members = type.GetMembers()
               .Where(MatchesKind)
               .Select(MemberSymbol.From)
               .Where(x => x is not null) // ReSharper disable once NullableWarningSuppressionIsUsed
               .Select(x => x!.Value)
               .ToArray() is { Length: >= 2 } arr
                ? arr
                : [new(first, nameof(first)), new(second, nameof(second))];

            var source = ExtendingGenerator.Transform(type, members: members) is var (hintName, contents)
                ? $"{hintName}:\n{contents}"
                : "Failed to display source.";

            InvalidOperationException display = new($"{Display(verify.TestState.Sources)}\n// {source}");
            throw new AggregateException($"Micro-test {i}/{length} caused invalid codegen.", e, display);
        }

        static bool IsNonGenericInstance(INamedTypeSymbol? x) =>
            x is null ||
            x is
            {
                IsRefLikeType: false, IsStatic: false, SpecialType: not SpecialType.System_Void, TypeParameters: [],
            } &&
            IsNonGenericInstance(x.ContainingType) &&
            (x.BaseType is not null || x.GetMembers().All(x => !x.IsStatic));

        static int FlattenedLength(int source, int size) =>
            source / size * size * size + source % size * (source % size);

        static string Display(SourceFileCollection? sources) =>
            string.Join('\n', sources?.Select(x => $"// {x.filename}:\n{x.content}") ?? []);

        static IEnumerable<ISymbol> GetMembers(INamespaceOrTypeSymbol x) =>
            x.GetMembers().OfType<INamespaceOrTypeSymbol>().SelectMany(GetMembers).Prepend(x);

        static IEnumerable<Test> Query(INamedTypeSymbol[] array) =>
            from second in array
            from first in array
            from typeKeyword in TypeKeywords
            from structure in Structures
            select (structure, typeKeyword, first, second, new Verify
            {
                TestCode = Wrap(string.Format(structure, typeKeyword, first, second)),
                TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
            });

        static IEnumerable<ITypeSymbol> GetBaseTypes(ITypeSymbol? x) =>
            x is null ? [] : GetBaseTypes(x.BaseType).Prepend(x);
    }
}
