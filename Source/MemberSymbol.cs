// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>Represents a named member, typically a field or property.</summary>
/// <param name="Type">The type of the member.</param>
/// <param name="Name">The name of the member.</param>
/// <param name="Symbol">The underlying symbol, if any.</param>
public readonly record struct MemberSymbol(ITypeSymbol Type, string Name, ISymbol? Symbol = null)
{
    /// <summary>Initializes a new instance of the <see cref="MemberSymbol"/> struct.</summary>
    /// <param name="field">The field.</param>
    public MemberSymbol(IFieldSymbol field)
        : this(field.Type, field.Name, field) { }

    /// <summary>Initializes a new instance of the <see cref="MemberSymbol"/> struct.</summary>
    /// <param name="property">The property.</param>
    public MemberSymbol(IPropertySymbol property)
        : this(property.Type, property.Name, property) { }

    /// <summary>Gets a value indicating whether the member is static.</summary>
    public bool IsStatic => Symbol is { IsStatic: true };

    [Pure]
    public static bool Equal(ITypeSymbol? x, ITypeSymbol? y)
    {
        // Type symbols in source may vary in subtle but breaking ways.
        // We need to extensively make sure that everything remains the same.
        static bool ColdPath(ITypeSymbol x, ITypeSymbol y) =>
            x.SpecialType is not SpecialType.None
                ? x.SpecialType == y.SpecialType
                : x.TypeKind == y.TypeKind &&
                // We skip properties covered by SpecialType, TypeKind, in NamedTypeSymbolComparer, or always false.
                x.DeclaredAccessibility == y.DeclaredAccessibility &&
                x.IsRefLikeType == y.IsRefLikeType &&
                x.IsUnmanagedType == y.IsUnmanagedType &&
                x.IsReadOnly == y.IsReadOnly &&
                x.IsRecord == y.IsRecord &&
                TypeSymbolComparer.Equal(x, y) &&
                Equal(x.BaseType, y.BaseType) &&
                x.AllInterfaces.SequenceEqual(y.AllInterfaces, Equal) &&
                x.GetMembers().SequenceEqual(y.GetMembers(), SymbolComparer.Default);

        // A metadata name check is enough to determine if two type symbols are equal assuming they're compiled.
        static bool DifferentReferences(ITypeSymbol x, ITypeSymbol y) =>
            !x.IsInSource() ? !y.IsInSource() && TypeSymbolComparer.Equal(x, y) : y.IsInSource() && ColdPath(x, y);

        return x is null ? y is null : ReferenceEquals(x, y) || y is not null && DifferentReferences(x, y);
    }

    [Pure]
    public static int Hash(ITypeSymbol? x) =>
        x is null
            ? -1
            : unchecked((int)x.SpecialType * Primes.Int16[^1] ^
                (int)x.TypeKind * Primes.Int16[^2] ^
                (int)x.DeclaredAccessibility * Primes.Int16[^3] ^
                x.IsRefLikeType.ToByte() * Primes.Int16[^4] ^
                x.IsUnmanagedType.ToByte() * Primes.Int16[^5] ^
                x.IsReadOnly.ToByte() * Primes.Int16[^6] ^
                x.IsRecord.ToByte() * Primes.Int16[^7] ^
                TypeSymbolComparer.GetHashCode(x) * Primes.Int16[^8]);

    /// <summary>Creates a new instance of the <see cref="MemberSymbol"/> struct from the underlying symbol.</summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to create the <see cref="MemberSymbol"/> from.</param>
    /// <returns>The new <see cref="MemberSymbol"/> instance.</returns>
    public static MemberSymbol? DeconstructFrom(ISymbol symbol) =>
        symbol switch
        {
            IFieldSymbol x => new(x),
            IPropertySymbol x => new(x),
            _ => null,
        };

    /// <inheritdoc />
    public bool Equals(MemberSymbol other) =>
        Symbol switch
        {
            IFieldSymbol => other.Symbol is IFieldSymbol,
            IPropertySymbol => other.Symbol is IPropertySymbol,
            _ => other.Symbol is not IFieldSymbol and not IPropertySymbol,
        } &&
        Name == other.Name &&
        Equal(Type, other.Type);

    [Pure]
    public bool DeepEquals(MemberSymbol other) => Name == other.Name && Equal(Type, other.Type);

    /// <inheritdoc />
    public override int GetHashCode() =>
        Symbol switch
        {
            IFieldSymbol field => FieldSymbolComparer.Default.GetHashCode(field) * 3,
            IPropertySymbol property => PropertySymbolComparer.Default.GetHashCode(property) * 2,
            _ => TypeSymbolComparer.GetHashCode(Type) ^ StringComparer.Ordinal.GetHashCode(Name),
        };

    public override string ToString() =>
        Symbol switch
        {
            IFieldSymbol => $"{Type} {Name};",
            IPropertySymbol => $"{Type} {Name} {{ get; }}",
            _ => $"{Name}<{Type}>",
        };
}
