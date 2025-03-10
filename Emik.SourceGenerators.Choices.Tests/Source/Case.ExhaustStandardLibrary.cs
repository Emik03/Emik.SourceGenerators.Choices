// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Tests;

public partial class Case
{
    public sealed class ExhaustStandardLibrary(ITestOutputHelper output)
    {
        const int UpdateMeEvery = 1000;

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

        static ImmutableArray<string> Structures { get; } = [DotPattern, Fields, PrimaryConstructors, Properties];

        static ImmutableArray<string> TypeKeywords { get; } = ["class", "record", "record struct", "struct"];

        static ImmutableArray<INamedTypeSymbol> AccessibleTypes { get; } =
        [
            ..CSharpCompilation.Create("tmp")
               .AddReferences(Net90.References.All.Where(x => x.Display?.Contains("System") is true))
               .GlobalNamespace
               .GetMembers()
               .SelectMany(GetMembers)
               .OfType<INamedTypeSymbol>()
               .Where(IsNonGenericInstance)
               .Where(x => GetBaseTypes(x).All(x => x.DeclaredAccessibility is Accessibility.Public)),
        ];

        [Fact]
        public Task Run1Async() => EnumerateAsync(1);

        [Fact]
        public Task Run2Async() => EnumerateAsync(2);

        [Fact]
        public Task Run4Async() => EnumerateAsync(4);

        [Fact]
        public Task Run8Async() => EnumerateAsync(8);

        [Fact]
        public Task RunAllAsync() => EnumerateAsync(int.MaxValue);

        async Task EnumerateAsync(int size)
        {
            static int FlattenedLength(int source, int size) =>
                source / size * size * size +
                source % size * (source % size);

            static IEnumerable<(string, Verify)> Query(INamedTypeSymbol[] array) =>
                from second in array
                from first in array
                from typeKeyword in TypeKeywords
                from structure in Structures
                let testCode = string.Format(structure, typeKeyword, first, second)
                select (testCode, new Verify
                {
                    TestCode = Wrap(testCode),
                    TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
                });

            var length = FlattenedLength(AccessibleTypes.Length, size);
            var code = "";
            var i = 1;

            try
            {
                foreach (var (source, verify) in AccessibleTypes.Chunk(size).SelectMany(Query))
                {
                    i++;
                    code = source;
                    await verify.RunAsync();

                    if (i % UpdateMeEvery is 0)
                        output.WriteLine($"Successfully ran micro-test {i}/{length}.");
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Micro-test {i}/{length} caused invalid codegen:\n{code}", e);
            }
        }

        static bool IsNonGenericInstance(INamedTypeSymbol? x) =>
            x is null ||
            x is
            {
                IsRefLikeType: false, IsStatic: false, SpecialType: not SpecialType.System_Void, TypeParameters: [],
            } &&
            IsNonGenericInstance(x.ContainingType) &&
            (x.BaseType is not null || x.GetMembers().All(x => !x.IsStatic));

        static IEnumerable<ITypeSymbol> GetBaseTypes(ITypeSymbol? x) =>
            x is null ? [] : GetBaseTypes(x.BaseType).Prepend(x);

        static IEnumerable<ISymbol> GetMembers(INamespaceOrTypeSymbol x) =>
            x.GetMembers().OfType<INamespaceOrTypeSymbol>().SelectMany(GetMembers).Prepend(x);
    }
}
