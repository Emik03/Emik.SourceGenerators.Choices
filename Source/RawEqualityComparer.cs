// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>Provides an <see cref="IEqualityComparer{T}"/> for <see cref="Raw"/> instances.</summary>
sealed class RawEqualityComparer : IEqualityComparer<Raw>
{
    /// <summary>Initializes a new instance of the <see cref="RawEqualityComparer"/> class.</summary>
    RawEqualityComparer() { }

    /// <summary>Gets the singleton instance of the <see cref="RawEqualityComparer"/> class.</summary>
    public static RawEqualityComparer Instance { get; } = new();

    /// <inheritdoc />
    [Pure]
    public bool Equals(Raw x, Raw y) =>
        x.PolyfillAttributes == y.PolyfillAttributes &&
        x.MutablePublicly == y.MutablePublicly &&
        x.Fields.Length == y.Fields.Length &&
        MemberSymbol.Equal(x.Named, y.Named) &&
        x.Fields.Equals(y.Fields);

    /// <inheritdoc />
    [Pure]
    public int GetHashCode(Raw x)
    {
        var hash = x.PolyfillAttributes.ToByte() << 2 ^
            BetterHashCode(x.MutablePublicly) ^
            RoslynComparer.Signature.GetHashCode(x.Named);

        for (var i = 0; i < x.Fields.Length; i++)
            hash ^= unchecked(x.Fields[i].GetHashCode() * Primes.Index(^(i + 1)));

        return hash;
    }

    /// <summary>Converts a nullable boolean to an integer hash code.</summary>
    /// <remarks><para>
    /// The default hash code implementation has a conflict with <see langword="null"/> and <see langword="false"/>,
    /// both being <c>0</c>. This function separates the two aforementioned values with different return values.
    /// </para></remarks>
    /// <param name="x">The boolean to convert.</param>
    /// <returns>The hash code of <paramref name="x"/>.</returns>
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
