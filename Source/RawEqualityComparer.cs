// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

sealed class RawEqualityComparer : IEqualityComparer<Raw>
{
    private RawEqualityComparer() { }

    public static RawEqualityComparer Instance { get; } = new();

    [Pure]
    public bool Equals(Raw x, Raw y) =>
        x.PolyfillAttributes == y.PolyfillAttributes &&
        x.MutablePublicly == y.MutablePublicly &&
        x.Fields.Count == y.Fields.Count &&
        MemberSymbol.Equal(x.Named, y.Named) &&
        x.Fields.Equals(y.Fields);

    [Pure]
    public int GetHashCode(Raw x)
    {
        var hash = x.PolyfillAttributes.GetHashCode() << 2 ^
            BetterHashCode(x.MutablePublicly) ^
            MemberSymbol.Hash(x.Named);

        for (var i = 0; i < x.Fields.Count; i++)
            hash ^= unchecked(x.Fields[i].GetHashCode() * Primes.Index(^(i + 1)));

        return hash;
    }

    // Rust knows the best memory layout. Therefore, this is guaranteed to be the blazingly fastest implementation.
    // [src/main.rs:4] unsafe { mem::transmute::<Option<bool>, u8>(Some(false)) } = 0
    // [src/main.rs:5] unsafe { mem::transmute::<Option<bool>, u8>(Some(true)) } = 1
    // [src/main.rs:6] unsafe { mem::transmute::<Option<bool>, u8>(None) } = 2
    [Pure]
    static int BetterHashCode(bool? x) =>
        x switch
        {
            false => 0,
            true => 1,
            null => 2,
        };
}
