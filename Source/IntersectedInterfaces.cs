// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>
/// Responsible for extracting the members and interfaces common with the set of <see cref="MemberSymbol"/> instances.
/// </summary>
/// <param name="Symbols">The set of <see cref="MemberSymbol"/> instances.</param>
/// <param name="IsReadOnly">Whether the type is immutable, which normally restricts some implementations.</param>
/// <param name="IsRecord">Whether the type is a record, which restricts members named <c>Clone</c>.</param>
// ReSharper disable NullableWarningSuppressionIsUsed
sealed record IntersectedInterfaces(ImmutableArray<MemberSymbol> Symbols, bool IsReadOnly, bool IsRecord)
{
    /// <summary>Gets the set of interfaces that are in common with every member of <see cref="Symbols"/>.</summary>
    [Pure] // ReSharper disable once ReturnTypeCanBeEnumerable.Global
    public HashSet<INamedTypeSymbol> Set =>
        Symbols.Skip(1).Aggregate(UnimplementedInterfaces(Symbols[0]).ToSet(RoslynComparer.Signature), Intersect);

    /// <summary>Gets the members that are in common with every member of <see cref="Symbols"/>.</summary>
    [Pure]
    public IEnumerable<Extract> Members =>
        Symbols[0]
           .Type
           .AllInterfaces
           .Select(GroupOriginalDefinitions)
           .Where(x => x.Length == Symbols.Length)
           .SelectMany(DelegateImplementors);

    /// <summary>
    /// Determines if <paramref name="symbol"/> is an
    /// <see cref="IComparable"/> or <see cref="IEquatable{T}"/> interface.
    /// </summary>
    /// <param name="symbol">The <see cref="INamedTypeSymbol"/> to check.</param>
    /// <returns>
    /// The value <see langword="true"/> if <paramref name="symbol"/> is an <see cref="IComparable"/>
    /// or <see cref="IEquatable{T}"/> interface; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    static bool IsImplementedInterface([NotNullWhen(true)] INamedTypeSymbol? symbol) =>
        symbol is
        {
            Name: nameof(IComparable) or nameof(IEquatable<>),
            TypeArguments: [] or [{ SpecialType: SpecialType.System_Object }],
            ContainingNamespace: { ContainingNamespace.IsGlobalNamespace: true, Name: nameof(System) },
        };

    /// <summary>Determines whether the symbol is mutable.</summary>
    /// <param name="symbol">The symbol to check.</param>
    /// <returns>
    /// The value <see langword="true"/> if <paramref name="symbol"/> is mutable; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    static bool IsMutable(ISymbol symbol) => symbol is IEventSymbol or IPropertySymbol { SetMethod: not null };

    [Pure]
    bool IsIncompatible(INamedTypeSymbol x)
    {
        bool Test(INamespaceOrTypeSymbol x) =>
            x.GetMembers().Any(x => x.IsStatic || x.Name is "Clone" && IsRecord) ||
            IsReadOnly && x.GetMembers().Any(IsMutable);

        return Test(x) || x.AllInterfaces.Any(Test);
    }

    /// <summary>Extracts the members that are in common with every member of <see cref="Symbols"/>.</summary>
    /// <param name="interfaces">The interfaces to try.</param>
    /// <returns>The members that are in common with every member of <see cref="Symbols"/>.</returns>
    [Pure]
    IEnumerable<Extract> DelegateImplementors(ImmutableArray<INamedTypeSymbol> interfaces)
    {
        var first = interfaces[0];

        static ImmutableArray<IParameterSymbol> Parameters(ISymbol symbol) =>
            symbol switch
            {
                IMethodSymbol { Parameters: var parameters } => parameters,
                IPropertySymbol { Parameters: var parameters } => parameters,
                _ => [],
            };

        bool IsEqual(MemberSymbol union) => union.Type.AllInterfaces.Contains(first, RoslynComparer.Signature);

        bool CanReturnTypeBeIncluded(ISymbol symbol) =>
            !first.TypeParameters.Contains(symbol.OriginalDefinition.ToUnderlying(), RoslynComparer.Signature);

        bool CanParameterBeIncluded(IParameterSymbol symbol) =>
            !first.TypeParameters.Contains(symbol.OriginalDefinition.Type, RoslynComparer.Signature);

        var interfacesEqual = Symbols.All(IsEqual);

        bool CanBeIncluded(ISymbol symbol) =>
            interfacesEqual || CanReturnTypeBeIncluded(symbol) && Parameters(symbol).All(CanParameterBeIncluded);

        var interfaceDeclarations = ImmutableArray.CreateRange(interfaces, string? (x) => $"{x}");

        return first.GetMembers().Where(CanBeIncluded).Select(x => (x, Signature.Kind(x), interfaceDeclarations));
    }

    /// <summary>
    /// The accumulator function that intersects the set of interfaces with ones incompatible of the next member.
    /// </summary>
    /// <param name="acc">The accumulator.</param>
    /// <param name="next">The next member.</param>
    /// <returns>
    /// The parameter <paramref name="acc"/>, intersected with the
    /// incompatible interfaces of <paramref name="next"/>, if any.
    /// </returns>
    HashSet<INamedTypeSymbol> Intersect(HashSet<INamedTypeSymbol> acc, MemberSymbol next)
    {
        acc.IntersectWith(UnimplementedInterfaces(next));
        return acc;
    }

    /// <summary>Gets the set of interfaces that are contenders for type forwarding.</summary>
    /// <param name="next">The next member.</param>
    /// <returns>The set of interfaces that are contenders for type forwarding.</returns>
    [Pure]
    IEnumerable<INamedTypeSymbol> UnimplementedInterfaces(MemberSymbol next) =>
        next
           .Type
           .AllInterfaces
           .Omit(IsImplementedInterface)
           .Omit(IsIncompatible);

    /// <summary>Creates the list of original definitions of <paramref name="interfaceFromFirst"/>.</summary>
    /// <param name="interfaceFromFirst">The first member.</param>
    /// <returns>The list of original definitions of <paramref name="interfaceFromFirst"/>.</returns>
    [Pure]
    ImmutableArray<INamedTypeSymbol> GroupOriginalDefinitions(INamedTypeSymbol interfaceFromFirst) =>
    [
        ..Symbols
           .Skip(1)
           .Select(x => x.Type.AllInterfaces)
           .Select(x => x.FirstOrDefault(x => RoslynComparer.Signature.Equals(x, interfaceFromFirst)))
           .Filter()
           .Prepend(interfaceFromFirst),
    ];
}
