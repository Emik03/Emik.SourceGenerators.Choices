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
    [Pure]
    public bool IsStatic => Symbol is { IsStatic: true };

    /// <summary>Compares two <see cref="ITypeSymbol"/> instances.</summary>
    /// <remarks><para>
    /// As opposed to <see cref="TypeSymbolComparer.Equal"/>, this method also checks for the members
    /// of <see cref="ITypeSymbol"/> instances if they are declared in source, since both instances
    /// could have the same metadata name but come from different iterations of source code.
    /// </para></remarks>
    /// <param name="x">The first <see cref="ITypeSymbol"/> to compare.</param>
    /// <param name="y">The second <see cref="ITypeSymbol"/> to compare.</param>
    /// <returns>Whether the two <see cref="ITypeSymbol"/> instances are equal.</returns>
    [Pure]
    public static bool Equal(ITypeSymbol? x, ITypeSymbol? y)
    {
        static bool Generic(ITypeParameterSymbol x, ITypeParameterSymbol y) =>
            x.ConstraintTypes.Length == y.ConstraintTypes.Length &&
            x.MetadataName == y.MetadataName &&
            NamespaceSymbolComparer.Equal(x.ContainingNamespace, y.ContainingNamespace);

        // Type symbols in source may vary in subtle but breaking ways.
        // We need to extensively make sure that everything remains the same.
        static bool ColdPath(ITypeSymbol x, ITypeSymbol y) =>
            x is ITypeParameterSymbol genericX && y is ITypeParameterSymbol genericY ? Generic(genericX, genericY) :
            x.SpecialType is not SpecialType.None ? x.SpecialType == y.SpecialType : x.TypeKind == y.TypeKind &&
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

    /// <summary>Hashes an <see cref="ITypeSymbol"/> instance.</summary>
    /// <remarks><para>See remarks in <see cref="Equal"/> for more details.</para></remarks>
    /// <param name="x">The <see cref="ITypeSymbol"/> to hash.</param>
    /// <returns>The computed hash code.</returns>
    [Pure]
    public static int Hash(ITypeSymbol? x) =>
        x is null
            ? -1
            : HashCode.Combine(
                x.SpecialType,
                x.TypeKind,
                x.DeclaredAccessibility,
                x.IsRefLikeType,
                x.IsUnmanagedType,
                x.IsReadOnly,
                x.IsRecord,
                TypeSymbolComparer.GetHashCode(x)
            );

    /// <summary>Creates a new instance of the <see cref="MemberSymbol"/> struct from the underlying symbol.</summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to create the <see cref="MemberSymbol"/> from.</param>
    /// <returns>The new <see cref="MemberSymbol"/> instance.</returns>
    public static MemberSymbol? From(ISymbol symbol) =>
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

    /// <summary>Checks for deep equality between two <see cref="MemberSymbol"/> instances.</summary>
    /// <remarks><para>See remarks in <see cref="Equal"/> for more details.</para></remarks>
    /// <param name="other">The other <see cref="MemberSymbol"/> to compare.</param>
    /// <returns>
    /// The value <see langword="true"/> if the two <see cref="MemberSymbol"/>
    /// instances are equal; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    public bool DeepEquals(MemberSymbol other) => Name == other.Name && Equal(Type, other.Type);

    /// <inheritdoc />
    public override int GetHashCode() =>
        Symbol switch
        {
            IFieldSymbol field => FieldSymbolComparer.Default.GetHashCode(field) * Prime(),
            IPropertySymbol property => PropertySymbolComparer.Default.GetHashCode(property) * Prime(),
            _ => TypeSymbolComparer.GetHashCode(Type) ^ StringComparer.Ordinal.GetHashCode(Name) * Prime(),
        };

    /// <inheritdoc />
    public override string ToString() =>
        Symbol switch
        {
            IFieldSymbol => $"{Type} {Name};",
            IPropertySymbol => $"{Type} {Name} {{ get; }}",
            _ => $"{Name}<{Type}>",
        };
}
