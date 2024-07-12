// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

sealed class RawEqualityComparer : IEqualityComparer<Raw>
{
    private RawEqualityComparer() { }

    public static RawEqualityComparer Instance { get; } = new();

    [Pure]
    public bool Equals(Raw x, Raw y)
    {
        if (x.MutablePublicly != y.MutablePublicly || x.Fields.Count != y.Fields.Count)
            return false;

        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < x.Fields.Count; i++)
            if (!x.Fields[i].Equals(y.Fields[i]))
                return false;

        if (!Equal(x.Named, y.Named))
            return false;

        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < x.Fields.Count; i++)
            if (!Equal(x.Fields[i].Type, y.Fields[i].Type))
                return false;

        return true;
    }

    [Pure]
    public int GetHashCode(Raw x)
    {
        var hash = BetterHashCode(x.MutablePublicly) ^ BetterHashCode(x.Named);

        for (var i = 0; i < x.Fields.Count; i++)
            hash ^= x.Fields[i].GetHashCode() * Primes.Index(i);

        return hash;
    }

    [Pure]
    static bool Equal(ITypeSymbol x, ITypeSymbol y) =>
        x.SpecialType is not SpecialType.None
            ? x.SpecialType == y.SpecialType
            : x.TypeKind == y.TypeKind &&
            // We skip properties covered by SpecialType, TypeKind, in NamedTypeSymbolComparer, or ones always false.
            x.DeclaredAccessibility == y.DeclaredAccessibility &&
            x.IsRefLikeType == y.IsRefLikeType &&
            x.IsUnmanagedType == y.IsUnmanagedType &&
            x.IsReadOnly == y.IsReadOnly &&
            x.IsRecord == y.IsRecord &&
            TypeSymbolComparer.Equal(x, y) &&
            Equal(x.BaseType, y.BaseType) &&
            x.AllInterfaces.SequenceEqual(y.AllInterfaces, Equal) &&
            x.GetMembers().SequenceEqual(y.GetMembers(), SymbolComparer.Default);

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

    [Pure]
    static int BetterHashCode(ITypeSymbol x) =>
        (int)x.SpecialType * Primes.Int16[^1] ^
        (int)x.TypeKind * Primes.Int16[^2] ^
        (int)x.DeclaredAccessibility * Primes.Int16[^3] ^
        x.IsRefLikeType.ToByte() * Primes.Int16[^4] ^
        x.IsUnmanagedType.ToByte() * Primes.Int16[^5] ^
        x.IsReadOnly.ToByte() * Primes.Int16[^6] ^
        x.IsRecord.ToByte() * Primes.Int16[^7] ^
        TypeSymbolComparer.GetHashCode(x) * Primes.Int16[^8];
}
