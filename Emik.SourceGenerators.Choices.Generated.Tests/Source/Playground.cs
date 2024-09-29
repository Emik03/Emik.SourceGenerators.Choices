// SPDX-License-Identifier: MPL-2.0
// ReSharper disable RedundantNameQualifier
// ReSharper disable once CheckNamespace EmptyNamespace
namespace Emik.SourceGenerators.Choices.Tests; // ReSharper disable once RedundantNameQualifier
#pragma warning disable 169
extern alias unity;
using Component = UnityEngine.Component;
using Task = System.Threading.Tasks.Task;

[Choice]
readonly partial record struct Result<TOk, TErr>
{
    readonly TOk? _ok;

    readonly TErr? _err;
}

[Choice(true)]
ref partial struct SpanEncodings
{
    Span<byte> _utf8;

    Span<char> _utf16;
}

[Choice(typeof((int Integer, float Floating, ValueTuple Unknown)), false)]
partial class Number;

[Choice(
    typeof((System.Reflection.AssemblyNameFlags AssemblyNames,
        System.Reflection.BindingFlags Bindings,
        System.Net.Sockets.SocketFlags Sockets,
        System.Threading.Tasks.Sources.ValueTaskSourceOnCompletedFlags ValueTaskSources))
)]
readonly partial struct Enums;

[Choice(typeof((Task Referenced, ValueTask Valued)), true)]
partial struct Tasks;

[Choice(typeof((Task Left, Task Right)), false)]
partial record SuperTask;

[Choice(typeof((KMBombModule Regular, KMNeedyModule Needy)))]
partial class KMModule
{
    readonly byte _discriminator;
}

[Choice.Regular<KMBombModule>.Needy<KMNeedyModule>.Other<Component>]
partial class DotKMModule;

[Choice.Apple<byte>.Pear<int>.Orange<BindingFlags>]
sealed partial class DotFruit;

[Choice]
readonly partial struct Option<T>
    where T : class
{
    public T? Some { get; } // ReSharper disable once UnusedMember.Local

    ValueTuple None => default;
}

[Choice.Public.Utf8<Span<byte>>.Utf16<Span<char>>]
ref partial struct SpanEncodingsDot;

[Choice] // ReSharper disable ParameterTypeCanBeEnumerable.Local
partial class Color(Color.OrRef<int>[]? rgba, Color.OrRef<Number>[]? gradient)
{
    public sealed class OrRef<[unity::JetBrains.Annotations.UsedImplicitlyAttribute] T>;
}

[Choice
    .And<BinaryCondition>
    .Or<BinaryCondition>
    .Included<InclusionCondition>
    .Excluded<InclusionCondition>]
partial class ConditionDescription
{
    public record struct BinaryCondition(ConditionDescription Left, ConditionDescription Right);

    public record struct InclusionCondition(
        Color.OrRef<JsonElement> Left,
        Color.OrRef<Color.OrRef<JsonElement>[]> Right
    );
}