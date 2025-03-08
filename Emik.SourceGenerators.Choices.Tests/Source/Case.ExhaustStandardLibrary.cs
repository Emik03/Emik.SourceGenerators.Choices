// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Tests;

public partial class Case
{
    public sealed class ExhaustStandardLibrary(ITestOutputHelper output)
    {
        const int UpdateMeEvery = 1000;

        const string DotPattern = // language=cs
            """
            [Choice.First<{1}>.Second<{2}>]
            public partial {0} Test;
            """;

        const string PrimaryConstructors = // language=cs
            """
            [Choice]
            public partial {0} Test({1} first, {2} second);
            """;

        const string Fields = // language=cs
            """
            [Choice]
            public partial {0} Test
            {{
                {1} _first;
            
                {2} _second;
            }}
            """;

        const string Properties = // language=cs
            """
            [Choice]
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
        public async Task RunAsync()
        {
            var tests = from second in AccessibleTypes
                from first in AccessibleTypes
                from typeKeyword in TypeKeywords
                from structure in Structures
                let testCode = string.Format(structure, typeKeyword, first, second)
                select (testCode, new Verify
                {
                    TestCode = Wrap(testCode),
                    TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
                });

            var length = AccessibleTypes.Length * AccessibleTypes.Length * TypeKeywords.Length * Structures.Length;
            var fail = "";
            var i = 0;

            try
            {
                foreach (var (source, verify) in tests)
                {
                    i++;
                    fail = source;
                    await verify.RunAsync();

                    if ((i + 1) % UpdateMeEvery is 0)
                        output.WriteLine($"Successfully ran micro-test {i + 1}/{length}.");
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Micro-test {i + 1}/{length} causes invalid codegen:\n{fail}", e);
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
