// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

// ReSharper disable SuggestBaseTypeForParameter
/// <summary>Represents the signature of a member.</summary>
/// <param name="Name">The name of the member.</param>
/// <param name="Parameters">The parameters of the member, for indexers and methods.</param>
/// <param name="Type">The return type of the member.</param>
/// <param name="TypeParameters">The type parameters of the member, for methods.</param>
/// <param name="Has">Defines the set of functions that the member is able to do.</param>
readonly record struct Signature(
    string Name,
    ITypeSymbol Type,
    ImmutableArray<IParameterSymbol> Parameters,
    ImmutableArray<ITypeParameterSymbol> TypeParameters,
    (bool Getter, bool Setter, bool Adder, bool Remover) Has
)
{
    /// <summary>The default <see cref="IEqualityComparer{T}"/> for <see cref="Extract"/> instances.</summary>
    static readonly IEqualityComparer<Extract> s_extracts =
        Equating<Extract>(
            (x, y) => From(x.Symbol) is var l &&
                From(y.Symbol) is var r &&
                (l is null ? r is null : r is not null && l.Value.Equivalent(r.Value))
        );

    /// <summary>The default <see cref="IEqualityComparer{T}"/> for <see cref="Signature"/> instances.</summary>
    static readonly IEqualityComparer<Signature> s_signatures = Equating<Signature>((x, y) => x.Equivalent(y));

    /// <summary>Initializes a new instance of the <see cref="Signature"/> struct.</summary>
    /// <param name="symbol">The symbol of the member.</param>
    /// <param name="parameters">The parameters of the member.</param>
    /// <param name="typeParameters">The type parameters of the member.</param>
    public Signature(
        ISymbol symbol,
        ImmutableArray<IParameterSymbol> parameters = default,
        ImmutableArray<ITypeParameterSymbol> typeParameters = default
    )
        : this(
            symbol.GetFullyQualifiedName(),
            symbol.ToUnderlying(),
            parameters.OrEmpty(),
            typeParameters.OrEmpty(),
            (symbol is IFieldSymbol or IPropertySymbol { GetMethod: not null },
                symbol is IFieldSymbol { IsReadOnly: false } or IPropertySymbol { SetMethod: not null },
                symbol is IEventSymbol { AddMethod: not null },
                symbol is IEventSymbol { RemoveMethod: not null })
        ) { }

    /// <summary>Extracts the forwarders from the set of <see cref="MemberSymbol"/> instances.</summary>
    /// <param name="symbols">The set of <see cref="MemberSymbol"/> instances.</param>
    /// <param name="named">The type that contains the members.</param>
    /// <param name="except">The set of <see cref="MemberSymbol"/> instances to exclude.</param>
    /// <returns>The set of forwarders.</returns>
    public static IEnumerable<Extract> FindForwarders(
        ImmutableArray<MemberSymbol> symbols,
        INamedTypeSymbol named,
        ISet<MemberSymbol>? except = null
    )
    {
        [Pure]
        static IEnumerable<Extract> GeneratedMethods(Extract symbol) =>
            (symbol switch
            {
                (IEventSymbol x, _, _) => [x.AddMethod, x.RaiseMethod, x.RemoveMethod],
                (IPropertySymbol x, _, _) => [x.GetMethod, x.SetMethod],
                _ => ImmutableArray<IMethodSymbol?>.Empty,
            })
           .Filter()
           .Select(AsDirectExtract);

        [Pure]
        static HashSet<Signature> ToSelf(IEnumerable<MemberSymbol>? except, IAssemblySymbol assembly) =>
            except.OrEmpty().Select(x => From(x.Type, assembly)).Filter().ToSet(s_signatures);

        var assembly = named.ContainingAssembly;

        [Pure]
        bool IsValid(Extract next) =>
            next.Symbol.Name != named.Name &&
            From(next.Symbol, assembly) is not null &&
            (except is null || MemberSymbol.From(next.Symbol) is not { } x || !except.Contains(x));

        [Pure]
        Extract ToExplicitInterfaceSymbolWhenRequired(Extract x) =>
            From(x.Symbol, assembly) is null &&
            x.Symbol.ContainingType.IsInterface() &&
            x.InterfaceDeclarations is var id &&
            id.Any(x => x is not null) &&
            x.Symbol.ExplicitInterfaceSymbols() is [var single] &&
            single.ContainingType.IsInterface()
                ? (single, x.Kind, ImmutableArray.CreateRange(id, string? (_) => $"{single.ContainingType}"))
                : x;

        [Pure]
        IEnumerable<Extract> FindCommonBaseMembers(ImmutableArray<MemberSymbol> s) =>
            FindCommonBaseTypes(s)
               .SelectMany(x => x.GetMembers())
               .Omit(x => x is ITypeSymbol || symbols.Any(y => y.CanConflict(x.Name)))
               .Select(x => AsDirectExtract(x, s.Length));

        var signatures = ToSelf(except, assembly);
        var forwarders = Add(symbols, assembly, signatures);

        forwarders.UnionWith(new IntersectedInterfaces(symbols, named.IsReadOnly, named.IsRecord).Members);
        forwarders.UnionWith(FindCommonBaseMembers(symbols));
        forwarders.ExceptWith(forwarders.ToArray().SelectMany(GeneratedMethods));

        return forwarders.Select(ToExplicitInterfaceSymbolWhenRequired).Where(IsValid);
    }

    /// <summary>Finds all common base types for the set of <see cref="MemberSymbol"/> instances.</summary>
    /// <param name="symbols">The set of <see cref="MemberSymbol"/> instances.</param>
    /// <returns>All common base types in descending order of specificity.</returns>
    [Pure]
    public static IEnumerable<ITypeSymbol> FindCommonBaseTypes(ImmutableArray<MemberSymbol> symbols) =>
        symbols.Skip(1).All(symbols[0].TypeEquals) ? [symbols[0].Type] :
        symbols.Any(x => x.Type is { TypeKind: TypeKind.Pointer } or { IsRefLikeType: true }) ? [] : symbols
           .Select(x => Inheritance(x.Type).ToSet(RoslynComparer.Signature))
           .Aggregate(IntersectWith)
           .OrderBy(x => x.SpecialType is SpecialType.System_Object)
           .ThenBy(x => x.SpecialType is SpecialType.System_ValueType)
           .ThenBy(x => x.IsInterface())
           .ThenByDescending(x => Inheritance(x).Count())
           .ThenByDescending(IsStandardLibrary)
           .ThenBy(x => x.GetFullyQualifiedMetadataName(), StringComparer.Ordinal);

    /// <summary>Gets the <see cref="RefKind"/> of the <see cref="ISymbol"/>.</summary>
    /// <param name="x">The <see cref="ISymbol"/> to get the <see cref="RefKind"/> of.</param>
    /// <returns>The <see cref="RefKind"/> of the <see cref="ISymbol"/>.</returns>
    [Pure]
    public static RefKind Kind(in ISymbol? x) =>
        x switch
        {
            IFieldSymbol { RefKind: var ret } => ret,
            IMethodSymbol { RefKind: var ret } => ret,
            IPropertySymbol { RefKind: var ret } => ret,
            IParameterSymbol { RefKind: var ret } => ret,
            IEventSymbol or null => RefKind.None,
            _ => throw Unreachable,
        };

    /// <summary>Gets the <see cref="Signature"/> of the <see cref="ISymbol"/>.</summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to get the <see cref="Signature"/> of.</param>
    /// <returns>
    /// The <see cref="Signature"/> of the <see cref="ISymbol"/>, or <see langword="null"/>
    /// if the <see cref="ISymbol"/> lacks a signature.
    /// </returns>
    [Pure]
    public static Signature? From(ISymbol symbol) =>
        symbol switch
        {
            { IsStatic: true } or
                { DeclaredAccessibility: Accessibility.ProtectedAndInternal or Accessibility.Protected } or
                IMethodSymbol { TypeArguments: [] } and
                ({ MethodKind: MethodKind.Constructor or MethodKind.Destructor } or
                {
                    Name: nameof(ToString) or [.., '.', 'T', 'o', 'S', 't', 'r', 'i', 'n', 'g'],
                    ReturnType.SpecialType: SpecialType.System_String,
                    Parameters: [],
                } or
                {
                    Name: nameof(GetHashCode) or [.., '.', 'G', 'e', 't', 'H', 'a', 's', 'h', 'C', 'o', 'd', 'e'],
                    ReturnType.SpecialType: SpecialType.System_Int32,
                    Parameters: [],
                } or
                {
                    Name: nameof(Equals) or [.., '.', 'E', 'q', 'u', 'a', 'l', 's'],
                    ReturnType.SpecialType: SpecialType.System_Boolean,
                    Parameters: [{ Type.SpecialType: SpecialType.System_Object }],
                } or
                {
                    Name: nameof(IComparable.CompareTo) or [.., '.', 'C', 'o', 'm', 'p', 'a', 'r', 'e', 'T', 'o'],
                    ReturnType.SpecialType: SpecialType.System_Int32,
                    Parameters: [{ Type.SpecialType: SpecialType.System_Object }],
                }) => null,
            IEventSymbol or IFieldSymbol { AssociatedSymbol: null } => new(symbol),
            IMethodSymbol { AssociatedSymbol: null, Parameters: var x, TypeParameters: var y } => new(symbol, x, y),
            IPropertySymbol { Parameters: var x } => new(symbol, x),
            _ => null,
        };

    /// <summary>Gets the <see cref="Signature"/> of the <see cref="ISymbol"/>.</summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to get the <see cref="Signature"/> of.</param>
    /// <param name="assembly">
    /// The <see cref="IAssemblySymbol"/> to check accessibility of the <see cref="ISymbol"/> for.
    /// </param>
    /// <returns>
    /// The <see cref="Signature"/> of the <see cref="ISymbol"/>, or <see langword="null"/>
    /// if the <see cref="ISymbol"/> lacks a signature.
    /// </returns>
    [Pure]
    public static Signature? From(ISymbol symbol, IAssemblySymbol assembly) =>
        symbol.CanBeAccessedFrom(assembly) ? From(symbol) : null;

    /// <summary>Checks if two <see cref="Signature"/>s are equivalent.</summary>
    /// <param name="other">The other <see cref="Signature"/> to compare.</param>
    /// <returns>
    /// The value <see langword="true"/> if the two <see cref="Signature"/>
    /// instances are equivalent; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    public bool Equivalent(in Signature other) =>
        Has == other.Has &&
        Name.AsSpan().SplitOn('.').Last.SequenceEqual(other.Name.AsSpan().SplitOn('.').Last) &&
        SameType(Type, other.Type) &&
        Parameters.GuardedSequenceEqual(other.Parameters, SameType) &&
        TypeParameters.GuardedSequenceEqual(other.TypeParameters, Complies);

    /// <summary>Checks if a <see cref="MemberSymbol"/> is in an <see cref="IAssemblySymbol"/>.</summary>
    /// <param name="symbol">The <see cref="MemberSymbol"/> to check.</param>
    /// <param name="assembly">The <see cref="IAssemblySymbol"/> to provide accessibility context.</param>
    /// <param name="interfaceDeclaration">The interface declaration of the <see cref="MemberSymbol"/>.</param>
    /// <returns>
    /// The value <see langword="true"/> if an equivalent signature was found; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    public bool IsIn(in MemberSymbol symbol, IAssemblySymbol assembly, out string? interfaceDeclaration)
    {
        foreach (var member in symbol.Type.GetMembers())
            if (From(member, assembly) is { } signature && Equivalent(signature))
            {
                interfaceDeclaration = InterfaceDeclaration(member);
                return true;
            }

        interfaceDeclaration = null;
        return false;
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        const int
            Prime = 16777619,
            StartingPrime = unchecked((int)2166136261);

        var hash = StartingPrime;

        unchecked
        {
            hash = hash * Prime ^ Name.AsSpan().SplitOn('.').Last.GetDjb2HashCode();
            hash = hash * Prime ^ RoslynComparer.Signature.GetHashCode(Type);
            hash = hash * Prime ^ Parameters.Length;
            return hash * Prime ^ TypeParameters.Length;
        }
    }

    /// <summary>Determines if any base type of <paramref name="x"/> is <paramref name="y"/>.</summary>
    /// <param name="x">The <see cref="ITypeSymbol"/> to check all base types of.</param>
    /// <param name="y">The <see cref="ITypeSymbol"/> to compare against.</param>
    /// <returns>
    /// The value <see langword="true"/> if any base type of <paramref name="x"/>
    /// is <paramref name="y"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    static bool AnyBaseType(ITypeSymbol? x, ITypeSymbol y)
    {
        for (; x is not null; x = x.BaseType)
            if (RoslynComparer.Signature.Equals(x, y))
                return true;

        return false;
    }

    /// <summary>
    /// Determines whether the <see cref="ITypeParameterSymbol"/> will always comply
    /// with the constraints of the other <see cref="ITypeParameterSymbol"/>.
    /// </summary>
    /// <param name="x">The <see cref="ITypeParameterSymbol"/> to check.</param>
    /// <param name="y">The <see cref="ITypeParameterSymbol"/> to compare against.</param>
    /// <returns>
    /// The value <see langword="true"/> if the parameter <paramref name="x"/> complies
    /// with the constraints of <paramref name="y"/>; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    static bool Complies(ITypeParameterSymbol x, ITypeParameterSymbol y) =>
        (x.HasConstructorConstraint || !y.HasConstructorConstraint) &&
        (x.HasNotNullConstraint || !y.HasNotNullConstraint) &&
        (x.HasReferenceTypeConstraint || !y.HasReferenceTypeConstraint) &&
        (x.HasUnmanagedTypeConstraint || !y.HasUnmanagedTypeConstraint) &&
        (x.HasValueTypeConstraint || !y.HasValueTypeConstraint) &&
        x.ConstraintNullableAnnotations.GuardedSequenceEqual(y.ConstraintNullableAnnotations) &&
        x.ConstraintTypes.GuardedSequenceEqual(y.ConstraintTypes, AnyBaseType) &&
        x.ReferenceTypeConstraintNullableAnnotation == y.ReferenceTypeConstraintNullableAnnotation;

    /// <summary>Determines if <paramref name="x"/> is a standard library type.</summary>
    /// <param name="x">The <see cref="ITypeSymbol"/> to check.</param>
    /// <returns>
    /// The value <see langword="true"/> if the parameter <paramref name="x"/>
    /// comes from the <see cref="System"/> namespace.
    /// </returns>
    [Pure]
    static bool IsStandardLibrary(ITypeSymbol x)
    {
        for (var name = x.ContainingNamespace; name.ContainingNamespace is { } containingName; name = containingName)
            if (name is { Name: nameof(System) } && containingName.IsGlobalNamespace)
                return true;

        return false;
    }

    /// <summary>Determines if both parameters represent the same type.</summary>
    /// <param name="left">The left-hand side.</param>
    /// <param name="right">The right-hand side.</param>
    /// <returns>The value <see langword="true"/> if both instances have the same type.</returns>
    [Pure]
    static bool SameType(IParameterSymbol left, IParameterSymbol right) => SameType(left.Type, right.Type);

    /// <summary>Determines if both types represent the same type.</summary>
    /// <param name="left">The left-hand side.</param>
    /// <param name="right">The right-hand side.</param>
    /// <returns>The value <see langword="true"/> if both instances have the same type.</returns>
    [Pure]
    static bool SameType(ITypeSymbol left, ITypeSymbol right) =>
        RoslynComparer.Signature.Equals(left, right) &&
        left.ContainingType is null == right.ContainingType is null &&
        (left.ContainingType is null || SameType(left.ContainingType, right.ContainingType));

    /// <summary>Gets the interface prefix for the member of the given <paramref name="x"/>.</summary>
    /// <param name="x">The symbol to extract.</param>
    /// <returns>The interface prefix of the parameter <paramref name="x"/>.</returns>
    [Pure]
    static string? InterfaceDeclaration(ISymbol x) => x.Name.LastIndexOf('.') is not -1 and var i ? x.Name[..i] : null;

    /// <summary>Converts the <see cref="ISymbol"/> to the <see cref="Extract"/>.</summary>
    /// <param name="x">The <see cref="ISymbol"/> to convert.</param>
    /// <returns>The <see cref="Extract"/> of the parameter <paramref name="x"/>.</returns>
    [Pure]
    static Extract AsDirectExtract(ISymbol x) => (x, Kind(x), default);

    /// <summary>Converts the <see cref="ISymbol"/> to the <see cref="Extract"/>.</summary>
    /// <param name="x">The <see cref="ISymbol"/> to convert.</param>
    /// <param name="count">The number of symbols.</param>
    /// <param name="element">The interface prefix.</param>
    /// <returns>The <see cref="Extract"/> of the parameter <paramref name="x"/>.</returns>
    [Pure]
    static Extract AsDirectExtract(ISymbol x, int count, string? element = null)
    {
        var interfaceDeclarations = new string?[count];
        interfaceDeclarations.AsSpan().Fill(element);
        return (x, Kind(x), AsImmutableArray(interfaceDeclarations));
    }

    /// <summary>Gets the set of <see cref="Extract"/> from the <paramref name="symbols"/>.</summary>
    /// <param name="symbols">The list of symbols.</param>
    /// <param name="assembly">The <see cref="IAssemblySymbol"/> to provide accessibility context.</param>
    /// <param name="exists">The set of signatures that already exist.</param>
    /// <returns>The set of <see cref="Extract"/> from the parameter <paramref name="symbols"/>.</returns>
    [Pure]
    static HashSet<Extract> Add(
        ImmutableArray<MemberSymbol> symbols,
        IAssemblySymbol assembly,
        HashSet<Signature> exists
    )
    {
        HashSet<Extract> set = new(s_extracts);

        foreach (var member in symbols[0].Type.GetMembers())
            if (symbols.All(x => !x.CanConflict(member.Name)) && From(member, assembly) is { } signature)
                signature.Next(symbols, set, exists, assembly, member);

        return set;
    }

    /// <summary>Intersects the first set of <see cref="ITypeSymbol"/> instances with the second.</summary>
    /// <param name="x">The first set of <see cref="ITypeSymbol"/> instances to mutate.</param>
    /// <param name="y">The second set of <see cref="ITypeSymbol"/> instances to enumerate.</param>
    /// <returns>The parameter <paramref name="x"/> after being intersected with <paramref name="y"/>.</returns>
    static HashSet<ITypeSymbol> IntersectWith(HashSet<ITypeSymbol> x, HashSet<ITypeSymbol> y)
    {
        x.IntersectWith(y);
        return x;
    }

    /// <summary>Gets all the base types and interfaces at all levels, including itself.</summary>
    /// <param name="x">The <see cref="ITypeSymbol"/> to get the types of.</param>
    /// <returns>The enumeration of all base types and interfaces of the parameter <paramref name="x"/>.</returns>
    [Pure]
    static IEnumerable<ITypeSymbol> Inheritance(ITypeSymbol x) =>
        x.FindPathToNull(x => x.BaseType).Concat(x.AllInterfaces);

    /// <summary>Steps through the next <see cref="ISymbol"/>.</summary>
    /// <param name="symbols">The set of symbols to compare.</param>
    /// <param name="set">The set of extracted signatures.</param>
    /// <param name="exists">The set of existing signatures.</param>
    /// <param name="assembly">The <see cref="IAssemblySymbol"/> to provide accessibility context.</param>
    /// <param name="member">The next <see cref="ISymbol"/> to process.</param>
    void Next(
        ImmutableArray<MemberSymbol> symbols,
        HashSet<Extract> set,
        HashSet<Signature> exists,
        IAssemblySymbol assembly,
        ISymbol member
    )
    {
        [Pure]
        static RefKind Min(RefKind left, RefKind right) =>
            left is RefKind.None || right is RefKind.None ? RefKind.None :
            left is RefKind.Ref || right is RefKind.Ref ? RefKind.Ref : RefKind.RefReadOnly;

        var small = ImmutableArray.CreateBuilder<string?>(symbols.Length);
        small.Add(InterfaceDeclaration(member));
        var kind = Kind(symbols[0].Symbol);

        for (var i = 1; i < symbols.Length && symbols[i] is var current; i++)
        {
            if (!IsIn(current, assembly, out var interfaceDeclaration))
                break;

            kind = Min(kind, Kind(current.Symbol));
            small.Add(interfaceDeclaration);

            if (i == symbols.Length - 1 && !exists.Contains(this))
                set.Add((member, kind, small.DrainToImmutable()));
        }
    }
}
