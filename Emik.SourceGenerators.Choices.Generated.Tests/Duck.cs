// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Generated.Tests;
#pragma warning disable 169, IDE0044, MA0008, MA0094
using Attribute = System.Attribute;
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

[Choice(typeof((AuditFlags Audits, BindingFlags Bindings, SocketFlags Sockets, ObjectAceFlags ObjectAces)))]
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
