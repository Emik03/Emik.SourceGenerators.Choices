// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>Represents a named member, typically a field or property.</summary>
/// <param name="Type">The type of the member.</param>
/// <param name="Name">The name of the member.</param>
/// <param name="Symbol">The underlying symbol, if any.</param>
public readonly record struct MemberSymbol(ITypeSymbol Type, string Name, ISymbol? Symbol = null)
{
    [StringSyntax("C#")]
    const string
        Action = "global::System.Action",
        Func = "global::System.Func";

    /// <summary>Initializes a new instance of the <see cref="MemberSymbol"/> struct.</summary>
    /// <param name="field">The field.</param>
    public MemberSymbol(IFieldSymbol field)
        : this(field.Type, field.Name, field) { }

    /// <summary>Initializes a new instance of the <see cref="MemberSymbol"/> struct.</summary>
    /// <param name="parameter">The parameter.</param>
    public MemberSymbol(IParameterSymbol parameter)
        : this(parameter.Type, parameter.Name, parameter) { }

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

    [Pure]
    public string UnmanagedFieldDeclaration =>
        $"""
                 [global::System.Runtime.InteropServices.FieldOffsetAttribute(0)]
                 internal {Type} {FieldName};
         """;

    /// <summary>Gets the name of the field that corresponds to this <see cref="MemberSymbol"/>.</summary>
    [Pure]
    public string FieldName => $"_{ParameterName}";

    /// <summary>Gets the name of the parameter that corresponds to this <see cref="MemberSymbol"/>.</summary>
    [Pure]
    public string ParameterName => $"{FirstCharName?.ToLower()}{RestName}";

    /// <summary>Gets the name of the property that corresponds to this <see cref="MemberSymbol"/>.</summary>
    [Pure]
    public string PropertyName => $"{FirstCharName?.ToUpper()}{RestName}";

    /// <summary>Gets the XML name that corresponds to this <see cref="MemberSymbol"/>.</summary>
    [Pure]
    public string XmlName => IsEmpty ? $"<c>{PropertyName}</c>" : $"<see cref=\"{PropertyName}\"/>";

    /// <summary>
    /// Gets the first character of the name of the parameter that corresponds to this <see cref="MemberSymbol"/>.
    /// </summary>
    [Pure]
    char? FirstCharName => Name.Nth((Name is ['_', ..]).ToByte());

    /// <summary>
    /// Gets the rest of the name of the parameter that corresponds to this <see cref="MemberSymbol"/>.
    /// </summary>
    [Pure]
    ReadOnlySpan<char> RestName => Name.AsSpan().Nth(((Name is ['_', ..]).ToByte() + 1)..);

    /// <summary>Compares two <see cref="INamespaceOrTypeSymbol"/> instances.</summary>
    /// <remarks><para>
    /// As opposed to <see cref="RoslynComparer.Instance"/>, this method also checks for the members
    /// of <see cref="INamespaceOrTypeSymbol"/> instances if they are declared in source, since both instances
    /// could have the same metadata name but come from different iterations of source code.
    /// </para></remarks>
    /// <param name="x">The first <see cref="INamespaceOrTypeSymbol"/> to compare.</param>
    /// <param name="y">The second <see cref="INamespaceOrTypeSymbol"/> to compare.</param>
    /// <returns>Whether the two <see cref="INamespaceOrTypeSymbol"/> instances are equal.</returns>
    [Pure]
    public static bool Equal(INamespaceOrTypeSymbol? x, INamespaceOrTypeSymbol? y) =>
        ReferenceEquals(x, y) ||
        x is not null && y is not null && (SourcedEquals(x, y) || UnsourcedEquals(x, y));

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
        RoslynComparer.Eq(type, other);

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

    /// <summary>Gets the name of the delegate type.</summary>
    /// <param name="hasGenericReturn">
    /// The value <see langword="true"/> if the delegate has a generic return type; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The name of the delegate type.</returns>
    [Pure]
    public string DelegateTypeName(bool hasGenericReturn) =>
        $"{(Type.IsRefLikeType && !IsEmpty ? $"{PropertyName}Handler" : hasGenericReturn ? Func : Action)}{(
            Type.IsRefLikeType || IsEmpty
                ? hasGenericReturn ? $"<{Scaffolder.ResultGeneric}>" : ""
                : hasGenericReturn ? $"<{Type}, {Scaffolder.ResultGeneric}>" : $"<{Type}>")}";

    /// <summary>Creates a new instance of the <see cref="MemberSymbol"/> struct from the underlying symbol.</summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to create the <see cref="MemberSymbol"/> from.</param>
    /// <returns>The new <see cref="MemberSymbol"/> instance.</returns>
    [Pure]
    public static MemberSymbol? From(ISymbol symbol) =>
        symbol switch
        {
            IFieldSymbol { CanBeReferencedByName: true } x => new(x),
            IPropertySymbol { CanBeReferencedByName: true } x => new(x),
            IParameterSymbol { CanBeReferencedByName: true } x => new(x),
            _ => null,
        };

    /// <inheritdoc />
    [Pure]
    public bool Equals(MemberSymbol other) =>
        Symbol?.Kind == other.Symbol?.Kind && Name == other.Name && Equal(Type, other.Type);

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode() =>
        Symbol switch
        {
            IFieldSymbol field => RoslynComparer.Hash(field) * Prime(),
            IPropertySymbol property => RoslynComparer.Hash(property) * Prime(),
            IParameterSymbol parameter => RoslynComparer.Hash(parameter) * Prime(),
            _ => RoslynComparer.Hash(Type) ^ StringComparer.Ordinal.GetHashCode(Name) * Prime(),
        };

    /// <inheritdoc />
    [Pure]
    public override string ToString() =>
        Symbol switch
        {
            IFieldSymbol => $"{Type} {Name};",
            IParameterSymbol => $"{Type} {Name}",
            IPropertySymbol => $"{Type} {Name} {{ get; }}",
            _ => $"{Name}<{Type}>",
        };

    /// <summary>Determines if both symbols are equal and in source.</summary>
    /// <param name="x">The first <see cref="INamespaceOrTypeSymbol"/> to compare.</param>
    /// <param name="y">The second <see cref="INamespaceOrTypeSymbol"/> to compare.</param>
    /// <returns>Whether the two <see cref="INamespaceOrTypeSymbol"/> instances are equal.</returns>
    static bool SourcedEquals(INamespaceOrTypeSymbol x, INamespaceOrTypeSymbol y) =>
        y.IsInSource() &&
        RoslynComparer.Eq(x, y) &&
        x.GetMembers().GuardedSequenceEqual(y.GetMembers(), RoslynComparer.Instance) &&
        x.GetTypeMembers().GuardedSequenceEqual(y.GetTypeMembers(), RoslynComparer.Instance);

    /// <summary>Determines if both symbols are equal but both not in source.</summary>
    /// <param name="x">The first <see cref="INamespaceOrTypeSymbol"/> to compare.</param>
    /// <param name="y">The second <see cref="INamespaceOrTypeSymbol"/> to compare.</param>
    /// <returns>Whether the two <see cref="INamespaceOrTypeSymbol"/> instances are equal.</returns>
    static bool UnsourcedEquals(ISymbol x, ISymbol y) => !y.IsInSource() && RoslynComparer.Eq(x, y);
}
