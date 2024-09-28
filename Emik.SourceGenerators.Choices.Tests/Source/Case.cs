// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Tests;

public abstract class Case([StringSyntax("C#")] string? source)
{
    public sealed class Nothing() : Case(null);

    public sealed class Color() : Case(
        """
        [Choice]
        partial class Color(Color.OrRef<int>[]? rgba, Color.OrRef<Number>[]? gradient)
        {
            public sealed class OrRef<T>;
        }

        public partial struct Number;
        """
    );

    public sealed class ConditionDescription() : Case(
        """
        [Choice.And<BinaryCondition>.Or<BinaryCondition>.Included<InclusionCondition>.Excluded<InclusionCondition>]
        partial class ConditionDescription
        {
            public record struct BinaryCondition(ConditionDescription Left, ConditionDescription Right);

            public record struct InclusionCondition(
                Color.OrRef<JsonElement> Left,
                Color.OrRef<Color.OrRef<JsonElement>[]> Right
            );

            public partial class Color(Color.OrRef<int>[]? rgba, Color.OrRef<byte>[]? gradient)
            {
                public sealed class OrRef<T>;
            }
        }
        """
    );

    public sealed class DotFruit()
        : Case("[Choice.Apple<byte>.Pear<int>.Orange<BindingFlags>] sealed partial class DotFruit;");

    public sealed class DotKMModule() : Case(
        "[Choice.Regular<KMBombModule>.Needy<KMNeedyModule>.Other<UnityEngine.Component>] partial class DotKMModule;"
    );

    public sealed class Enums() : Case(
        """
        [Choice(typeof((AuditFlags Audits, BindingFlags Bindings, SocketFlags Sockets, ObjectAceFlags ObjectAces)))]
        readonly partial struct Enums;
        """
    );

    public sealed class KMModule() : Case(
        """
        [Choice(typeof((KMBombModule Regular, KMNeedyModule Needy)))]
        partial class KMModule
        {
            readonly byte _discriminator;
        }
        """
    );

    public sealed class Number()
        : Case("[Choice(typeof((int Integer, float Floating, ValueTuple Unknown)), false)] partial class Number;");

    public sealed class Option1() : Case(
        """
        [Choice]
        readonly partial struct Option<T>
            where T : class
        {
            public T? Some { get; }

            ValueTuple None => default;
        }
        """
    );

    public sealed class Pointers()
        : Case("[Choice] unsafe partial struct Pointers(byte* bytes, char* chars, nint native);");

    public sealed class Result2() : Case(
        """
        [Choice]
        readonly partial record struct Result<TOk, TErr>
        {
            readonly TOk? _ok;

            readonly TErr? _err;
        }
        """
    );

    public sealed class SpanEncodings() : Case(
        """
        [Choice(true)]
        ref partial struct SpanEncodings
        {
            Span<byte> _utf8;

            Span<char> _utf16;
        }
        """
    );

    public sealed class SpanEncodingsDot()
        : Case("[Choice.Public.Utf8<Span<byte>>.Utf16<Span<char>>] ref partial struct SpanEncodingsDot;");

    public sealed class SuperTask()
        : Case("[Choice(typeof((Task Left, Task Right)), false)] partial record SuperTask;");

    public sealed class Tasks()
        : Case("[Choice(typeof((Task Referenced, ValueTask Valued)), true)] partial struct Tasks;");

    [Fact]
    public async Task RunAsync()
    {
        Verify verify = new()
        {
            TestCode = Wrap(source),
            TestState = { GeneratedSources = { new AttributeGenerator().Source } },
        };

        if (source is null)
        {
            await verify.RunAsync();
            return;
        }

        var memberName = GetType().Name;
        var directory = Environment.CurrentDirectory;
        var generics = memberName is [.., >= '1' and <= '9' and var c] ? c - '0' : 0;

        while (Path.Join(directory, "Expected") is var expected && !Directory.Exists(expected))
            directory = Path.GetDirectoryName(directory ?? throw new FileNotFoundException(null, memberName));

        var name = $"{typeof(ExtendingGenerator).Namespace}/{typeof(ExtendingGenerator)}/{nameof(Emik)
        }.{typeof(Verify).Namespace}.{(generics is 0 ? memberName : $"{memberName[..^1]}`{generics}")}.g.cs";

        var absolute = Path.Join(directory, "Expected", $"{memberName}.csx");
        var text = await File.ReadAllTextAsync(absolute);
        verify.TestState.GeneratedSources.Add((name, SourceText.From(text, Encoding.UTF8)));
        await verify.RunAsync();
    }

    static string Wrap(string? source) => // language=c#
        $$"""
          using System;
          using System.Text.Json;
          using Emik;

          namespace Emik.SourceGenerators.Choices.Tests
          {
          {{source}}
          }

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
}
