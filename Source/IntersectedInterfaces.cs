// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

// ReSharper disable NullableWarningSuppressionIsUsed
sealed record IntersectedInterfaces(SmallList<FieldOrProperty> Symbols, bool IsReadOnly)
{
    [Pure] // ReSharper disable once ReturnTypeCanBeEnumerable.Global
    public HashSet<INamedTypeSymbol> Set =>
        Symbols
           .Skip(1)
           .Aggregate(UnimplementedInterfaces(Symbols.First).ToSet(NamedTypeSymbolComparer.Default), Intersect);

    [Pure]
    public IEnumerable<Extract> Members =>
        Symbols
           .First
           .Type
           .AllInterfaces
           .Select(GroupOriginalDefinitions)
           .Where(x => x.Count == Symbols.Count)
           .SelectMany(DelegateImplementors);

    [Pure]
    static bool IsImplementedInterface(INamedTypeSymbol namedTypeSymbol) =>
        namedTypeSymbol is
        {
            Name: nameof(IComparable) or nameof(IEquatable<int>),
            TypeArguments: [] or [{ SpecialType: SpecialType.System_Object }],
            ContainingNamespace: { ContainingNamespace.IsGlobalNamespace: true, Name: nameof(System) },
        };

    [Pure]
    IEnumerable<Extract> DelegateImplementors(SmallList<INamedTypeSymbol> interfaces)
    {
        var first = interfaces.First;

        static ImmutableArray<IParameterSymbol> Parameters(ISymbol symbol) =>
            symbol switch
            {
                IMethodSymbol { Parameters: var parameters } => parameters,
                IPropertySymbol { Parameters: var parameters } => parameters,
                _ => ImmutableArray<IParameterSymbol>.Empty,
            };

        bool IsEqual(FieldOrProperty union) =>
            union.Type.AllInterfaces.Contains(first, NamedTypeSymbolComparer.Default);

        bool CanReturnTypeBeIncluded(ISymbol symbol) =>
            !first.TypeParameters.Contains(symbol.OriginalDefinition.ToUnderlying(), TypeSymbolComparer.Default!);

        bool CanParameterBeIncluded(IParameterSymbol symbol) =>
            !first.TypeParameters.Contains(symbol.OriginalDefinition.Type, TypeSymbolComparer.Default);

        var interfacesEqual = Symbols.All(IsEqual);

        bool CanBeIncluded(ISymbol symbol) =>
            interfacesEqual || CanReturnTypeBeIncluded(symbol) && Parameters(symbol).All(CanParameterBeIncluded);

        var interfaceDeclarations = interfaces.Select(x => $"{x}").ItemCanBeNull().ToSmallList();

        return first
           .GetMembers()
           .Where(CanBeIncluded)
           .Select(x => (Extract)(x, Signature.Kind(x), interfaceDeclarations));
    }

    HashSet<INamedTypeSymbol> Intersect(HashSet<INamedTypeSymbol> acc, FieldOrProperty next)
    {
        acc.IntersectWith(UnimplementedInterfaces(next));
        return acc;
    }

    [Pure]
    IEnumerable<INamedTypeSymbol> UnimplementedInterfaces(FieldOrProperty next) =>
        next
           .Type
           .AllInterfaces
           .Omit(IsImplementedInterface)
           .Omit(x => x.GetMembers().Any(x => IsReadOnly && x is IEventSymbol || x.IsStatic));

    [Pure]
    SmallList<INamedTypeSymbol> GroupOriginalDefinitions(INamedTypeSymbol first)
    {
        [Pure]
        INamedTypeSymbol? FindComparableInterface(ImmutableArray<INamedTypeSymbol> all) =>
            all.FirstOrDefault(x => NamedTypeSymbolComparer.Equal(x.OriginalDefinition, first.OriginalDefinition));

        return Symbols
           .Skip(1)
           .Select(x => x.Type.AllInterfaces)
           .Select(FindComparableInterface)
           .Filter()
           .Prepend(first)
           .ToSmallList();
    }
}
