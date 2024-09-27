// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Tests;

using Task = System.Threading.Tasks.Task;

[CollectionDefinition(nameof(Tester), DisableParallelization = true)]
public class Tester
{
    [Fact]
    public Task Nothing() =>
        new Verifier { TestCode = CSharp(""), TestState = { GeneratedSources = { Gen() } } }.RunAsync();

    [Fact]
    public Task Result() =>
        new Verifier
        {
            TestCode = CSharp(
                """
                namespace Emik.SourceGenerators.Choices.Tests
                {
                    [Emik.Choice]
                    readonly partial record struct Result<TOk, TErr>
                    {
                        readonly TOk? _ok;
                    
                        readonly TErr? _err;
                    }
                }
                """
            ),
            TestState = { GeneratedSources = { Gen(), Read(2) } },
        }.RunAsync();

    [Fact]
    public Task SpanEncodings() =>
        new Verifier
        {
            TestCode = CSharp(
                """
                namespace Emik.SourceGenerators.Choices.Tests
                {
                    [Emik.Choice(true)]
                    ref partial struct SpanEncodings
                    {
                        Span<byte> _utf8;
                    
                        Span<char> _utf16;
                    }
                }
                """
            ),
            TestState = { GeneratedSources = { Gen(), Read() } },
        }.RunAsync();

    static string CSharp([StringSyntax("C#")] string x) => // language=c#
        $$"""
        using System;

        {{x}}

        namespace System.Diagnostics.CodeAnalysis
        {
            /// <summary>
            /// Specifies that the method or property will ensure that the listed field and property members
            /// have not-null values when returning with the specified return value condition.
            /// </summary>
            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
            sealed partial class MemberNotNullWhenAttribute : Attribute
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="MemberNotNullWhenAttribute"/> class
                /// with the specified return value condition and a field or property member.
                /// </summary>
                /// <param name="returnValue">
                /// The return value condition. If the method returns this value, the associated parameter will not be null.
                /// </param>
                /// <param name="member">The field or property member that is promised to be not-null.</param>
                public MemberNotNullWhenAttribute(bool returnValue, string member)
                {
                    ReturnValue = returnValue;
                    Members = [member];
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="MemberNotNullWhenAttribute"/> class
                /// with the specified return value condition and list of field and property members.
                /// </summary>
                /// <param name="returnValue">
                /// The return value condition. If the method returns this value, the associated parameter will not be null.
                /// </param>
                /// <param name="members">
                /// The list of field and property members that are promised to be not-null.
                /// </param>
                public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
                {
                    ReturnValue = returnValue;
                    Members = members;
                }

                /// <summary>
                /// Gets a value indicating whether the return value condition
                /// is <see langword="true"/> or <see langword="false"/>.
                /// </summary>
                public bool ReturnValue { get; }

                /// <summary>Gets field or property member names.</summary>
                public string[] Members { get; }
            }
        }
        """;

    static (string, SourceText) Gen()
    {
        var (name, text) = new AttributeGenerator();

        return ($"{typeof(AttributeGenerator).Namespace}/{typeof(AttributeGenerator)}/{name}",
            SourceText.From(text, Encoding.UTF8));
    }

    static (string, SourceText) Read(
        int generics = 0,
        string namespaces = "Emik.SourceGenerators.Choices.Tests",
        [CallerMemberName] string memberName = ""
    )
    {
        var name = $"{typeof(ExtendingGenerator).Namespace}/{typeof(ExtendingGenerator)
        }/Emik.{(namespaces is "" ? "" : $"{namespaces}.")}{memberName}{(generics is 0 ? "" : $"`{generics}")}.g.cs";

        var directory = Environment.CurrentDirectory;

        while (Path.Join(directory, "expected") is var expected && !Directory.Exists(expected))
            directory = Path.GetDirectoryName(directory ?? throw new FileNotFoundException(null, memberName));

        var absolute = Path.Join(directory, "expected", $"{memberName}.csx");
        var text = File.ReadAllText(absolute);
        return (name, SourceText.From(text, Encoding.UTF8));
    }
}
