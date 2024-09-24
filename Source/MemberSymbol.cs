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

    [Pure]
    public bool IsEmpty =>
        Type is { BaseType.SpecialType: not SpecialType.System_Enum, IsValueType: true } type &&
        !type.IsUnmanagedPrimitive() &&
        type.GetMembers().All(IsSymbolEmpty);

    /// <summary>Gets a value indicating whether the member is an equality contract generated by record types.</summary>
    [Pure]
    public bool IsEq =>
        Symbol is IPropertySymbol
        {
            ContainingType.IsRecord: true,
            Name: "EqualityContract",
            Type:
            {
                ContainingNamespace: { ContainingNamespace.IsGlobalNamespace: true, Name: nameof(System) },
                Name: nameof(Type),
            },
        };

    /// <summary>Gets a value indicating whether the member has <see cref="IComparable{T}.CompareTo"/>.</summary>
    [Pure]
    public bool IsInterfaceComparable => Type.GetMembers().Any(x => IsSingleSelf(x, nameof(IComparable.CompareTo)));

    /// <summary>Gets a value indicating whether the member has <see cref="IEquatable{T}.Equals"/>.</summary>
    [Pure]
    public bool IsInterfaceEquatable => Type.GetMembers().Any(x => IsSingleSelf(x, nameof(Equals)));

    /// <summary>Gets a value indicating whether the member has an operator comparison method.</summary>
    [Pure]
    public bool IsOperatorComparable =>
        Type.BaseType?.SpecialType is SpecialType.System_Enum ||
        Type.IsUnmanagedPrimitive() ||
        Type.GetMembers().Any(x => IsOperator(x, "op_GreaterThan"));

    /// <summary>Gets a value indicating whether the member has an operator equality method.</summary>
    [Pure]
    public bool IsOperatorEquatable =>
        Type.BaseType?.SpecialType is SpecialType.System_Enum ||
        Type.IsUnmanagedPrimitive() ||
        Type.GetMembers().Any(x => IsOperator(x, "op_Equality"));

    /// <summary>Gets a value indicating whether the type is a reference type.</summary>
    [Pure]
    public bool IsReference =>
        Type is { IsReferenceType: true } or
            ITypeParameterSymbol { ConstraintTypes: not [] } or
            ITypeParameterSymbol { HasReferenceTypeConstraint: true };

    /// <summary>Gets a value indicating whether the member is static.</summary>
    [Pure]
    public bool IsStatic => Symbol is { IsStatic: true };

    /// <summary>Gets a value indicating whether the type is unmanaged.</summary>
    [Pure]
    public bool IsUnmanaged => Type.IsUnmanagedType && Type is not ITypeParameterSymbol;

    /// <summary>Gets a string representation with the <see cref="NullableAnnotation.Annotated"/> annotation.</summary>
    [Pure]
    public string NullableAnnotated => $"{Type.WithNullableAnnotation(NullableAnnotation.Annotated)}";

    /// <summary>Gets a string suffix for nullable suppression.</summary>
    [Pure]
    public string NullableSuppression => Type.IsValueType ? "" : "!";

    /// <summary>Gets the name of the parameter that corresponds to this <see cref="MemberSymbol"/>.</summary>
    [Pure]
    public string ParameterName => $"{Name.Nth(0)?.ToLower()}{Name.AsSpan().Nth(1..)}";

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

    /// <summary>Determines if the <see cref="ISymbol"/> is an operator.</summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to check.</param>
    /// <param name="expect">The expected name of the operator.</param>
    /// <returns>
    /// The value <see langword="true"/> if the <see cref="ISymbol"/>
    /// is an operator; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    public static bool IsOperator(ISymbol symbol, string expect) =>
        symbol is IMethodSymbol
        {
            IsStatic: true,
            Name: var name,
            DeclaredAccessibility: Accessibility.Public,
            MethodKind: MethodKind.BuiltinOperator,
        } &&
        expect == name;

    /// <summary>Determines if the <see cref="ISymbol"/> is a method that has one parameter of itself.</summary>
    /// <param name="x">The <see cref="ISymbol"/> to check.</param>
    /// <param name="expect">The expected name of the parameter.</param>
    /// <returns>
    /// The value <see langword="true"/> if the <see cref="ISymbol"/> is a method
    /// that has one parameter of itself; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    public static bool IsSingleSelf(ISymbol x, string expect) =>
        x is IMethodSymbol
        {
            Name: var name,
            IsStatic: false,
            ContainingType: { } type,
            Parameters: [{ Type: INamedTypeSymbol other }],
        } &&
        expect == name &&
        NamedTypeSymbolComparer.Equal(type, other);

    /// <summary>Determines if the <see cref="ISymbol"/> could fit within a unit type.</summary>
    /// <param name="x">The symbol to check.</param>
    /// <returns>
    /// The value <see langword="true"/> if the <see cref="ISymbol"/> could
    /// fit within a unit type; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    public static bool IsSymbolEmpty(ISymbol x) =>
        x is { IsStatic: true } or
            not IFieldSymbol and not IMethodSymbol { MethodKind: MethodKind.Constructor, Parameters: not [] };

    /// <summary>Creates a new instance of the <see cref="MemberSymbol"/> struct from the underlying symbol.</summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to create the <see cref="MemberSymbol"/> from.</param>
    /// <returns>The new <see cref="MemberSymbol"/> instance.</returns>
    public static MemberSymbol? From(ISymbol symbol) =>
        symbol switch
        {
            IFieldSymbol { CanBeReferencedByName: true } x => new(x),
            IPropertySymbol { CanBeReferencedByName: true } x => new(x),
            _ => null,
        };

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
    public bool Equals(MemberSymbol other) =>
        Symbol switch
        {
            IFieldSymbol => other.Symbol is IFieldSymbol,
            IPropertySymbol => other.Symbol is IPropertySymbol,
            _ => other.Symbol is not IFieldSymbol and not IPropertySymbol,
        } &&
        Name == other.Name &&
        Equal(Type, other.Type);

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
