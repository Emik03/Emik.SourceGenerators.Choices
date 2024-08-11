// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Generated.Tests;
#pragma warning disable
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
