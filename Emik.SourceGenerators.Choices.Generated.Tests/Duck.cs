// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices.Generated.Tests;
#pragma warning disable
[Choice(true)]
ref partial struct SpanEncodings
{
    Span<byte> _utf8;

    Span<char> _utf16;
}

[Choice(typeof((int Integer, float Floating, ValueTuple Unknown)), false)]
partial class Number;

[Choice]
readonly partial record struct Result<TOk, TErr>
{
    readonly TOk? _ok;

    readonly TErr? _err;
}

[Choice(typeof((AuditFlags Audits, BindingFlags Bindings, SocketFlags Sockets, ObjectAceFlags ObjectAces)))]
readonly partial struct Enums;
