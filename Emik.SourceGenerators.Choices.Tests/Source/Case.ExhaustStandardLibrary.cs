// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Tests;

public partial class Case
{
    public sealed class ExhaustStandardLibrary
    {
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

        static ImmutableArray<Type> Types { get; } = [..typeof(int).Assembly.GetTypes().Where(CanBeGeneric)];

        [Fact]
        public async Task RunAsync()
        {
            var tests = from second in Types
                from first in Types
                from typeKeyword in TypeKeywords
                from structure in Structures
                let testCode = string.Format(structure, typeKeyword, first.FullName, second.FullName)
                select (testCode, new Verify
                {
                    TestCode = Wrap(testCode),
                    TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck,
                });

            var fail = "";
            var i = 0;

            try
            {
                foreach (var (source, verify) in tests)
                {
                    i++;
                    fail = source;
                    await verify.RunAsync();
                }
            }
            catch (Exception e)
            {
                var length = Types.Length * Types.Length * TypeKeywords.Length * Structures.Length;
                throw new InvalidOperationException($"Micro-test {i + 1}/{length} causes invalid codegen:\n{fail}", e);
            }
        }

        static bool CanBeGeneric(Type x) =>
            x is { IsByRefLike: false, IsGenericType: false } and
                ({ IsAbstract: false } or { IsSealed: false }) and
                ({ IsNestedPublic: true } or { IsPublic: true });
    }
}
