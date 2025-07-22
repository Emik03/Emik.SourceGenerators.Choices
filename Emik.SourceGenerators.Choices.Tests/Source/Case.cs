// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Tests;
#pragma warning disable CA1716, CA1724
public partial class Case()
#pragma warning restore CA1716
{
    readonly string? _source;

    private protected Case([StringSyntax("C#")] string source)
        : this() =>
        _source = source;

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
        [Choice(
            typeof((System.Reflection.AssemblyNameFlags AssemblyNames,
                System.Reflection.BindingFlags Bindings,
                System.Net.Sockets.SocketFlags Sockets,
                System.Threading.Tasks.Sources.ValueTaskSourceOnCompletedFlags ValueTaskSources))
        )]
        readonly partial struct Enums;
        """
    );

    public sealed class Inheritance()
        : Case("[Choice.First<ComponentConverter>.Second<CollectionConverter>] partial struct Inheritance;");

    public sealed class Ints() : Case(
        """
        [Choice]
        readonly partial struct Ints
        {
            public int First { get; }

            public int Second { get; }
        }
        """
    );

    public sealed class KMModule() : Case(
        """
        [Choice(typeof((KMBombModule Regular, KMNeedyModule Needy)))]
        partial class KMModule
        {
            readonly byte _discriminator;

            public override string ToString() => name;
        }
        """
    );

    public sealed class Logic() : Case(
        """
        [Choice]
        public sealed partial class Logic(string? one, (Logic Left, Logic Right) or, (Logic left, Logic right) and);
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

    public sealed class SelfRecursive()
        : Case("[Choice] partial class SelfRecursive(SelfRecursive recursion, int hope);");

    public sealed class SpanEncodings() : Case(
        """
        [Choice(true)]
        ref partial struct SpanEncodings
        {
            Span<byte> _utf8;

            Span<char> _utf16;

            public static bool operator ==(SpanEncodings l, SpanEncodings r)
                => l._utf8.SequenceEqual(r._utf8) && l._utf16.SequenceEqual(r._utf16);

            public static bool operator >(SpanEncodings l, SpanEncodings r)
                => l._discriminator > r._discriminator ||
                    l._utf8.SequenceCompareTo(r._utf8) > 0 ||
                    l._utf16.SequenceCompareTo(r._utf16) > 0;

            public static bool operator >=(SpanEncodings l, SpanEncodings r)
                => l._discriminator >= r._discriminator ||
                    l._utf8.SequenceCompareTo(r._utf8) >= 0 ||
                    l._utf16.SequenceCompareTo(r._utf16) >= 0;
        }
        """
    );

    public sealed class SpanEncodingsDot()
        : Case("[Choice.Public.Utf8<Span<byte>>.Utf16<Span<char>>] ref partial struct SpanEncodingsDot;");

    public sealed class SuperTask()
        : Case("[Choice(typeof((Task Left, Task Right)), false)] partial record SuperTask;");

    public sealed class Tasks()
        : Case("[Choice(typeof((Task Referenced, ValueTask Valued)), true)] partial struct Tasks;");

    public sealed class ThinEnum() : Case(
        "[Choice.Maybe<ValueTuple>.False.Null<ValueTuple>.True] readonly partial struct ThinEnum;"
    );

    public sealed class TuplePacking() : Case(
        "[Choice.Public.Unary<TuplePacking>.Binary<(TuplePacking, TuplePacking)>] sealed partial class TuplePacking;"
    );

    public sealed class UnderlyingInt()
        : Case(
            """
            [Choice]
            partial struct UnderlyingInt
            {
                public int Left { get; }

                public ValueTuple Center => default;

                public int Right { get; }
            }
            """
        );

    [Fact]
    public async Task RunAsync()
    {
        Verify verify = new()
        {
            TestCode = Wrap(_source),
            TestState = { GeneratedSources = { new AttributeGenerator().Source } },
        };

        if (_source is null)
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
        await using var file = File.Open(absolute, FileMode.OpenOrCreate, FileAccess.Read);
        using StreamReader reader = new(file);
        verify.TestState.GeneratedSources.Add((name, SourceText.From(await reader.ReadToEndAsync(), Encoding.UTF8)));
        await verify.RunAsync();
    }

    [MustUseReturnValue]
    static string Wrap(string? source) => // language=c#
        $$"""
          using System;
          using System.ComponentModel;
          using System.Reflection;
          using System.Text.Json;
          using System.Threading.Tasks;
          using Emik;
          #pragma warning disable SYSLIB5003
          namespace Emik.SourceGenerators.Choices.Tests
          {
          {{string.Join('\n', source?.Split('\n').Select(x => $"    {x}") ?? [])}}
          }
          #pragma warning restore SYSLIB5003
          """;
}
