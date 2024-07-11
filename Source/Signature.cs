// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

// ReSharper disable SuggestBaseTypeForParameter
readonly record struct Signature(
    string Name,
    ImmutableArray<IParameterSymbol> Parameters,
    ITypeSymbol Type,
    ImmutableArray<ITypeParameterSymbol> TypeParameters
)
{
    static readonly IEqualityComparer<Extract> s_extracts =
        Equating<Extract>(
            (x, y) => From(x.Symbol) is var l &&
                From(y.Symbol) is var r &&
                (l is null ? r is null : r is not null && l.Value.Equivalent(r.Value))
        );

    static readonly IEqualityComparer<Signature> s_signatures = Equating<Signature>((x, y) => x.Equivalent(y));

    public Signature(
        ISymbol symbol,
        ImmutableArray<IParameterSymbol> parameters = default,
        ImmutableArray<ITypeParameterSymbol> typeParameters = default
    )
        : this(
            symbol.GetFullyQualifiedName(),
            parameters.OrEmpty(),
            symbol.ToUnderlying(),
            typeParameters.OrEmpty()
        ) { }

    public static IEnumerable<Extract> FindForwarders(
        in SmallList<MemberSymbol> symbols,
        INamedTypeSymbol named,
        ISet<MemberSymbol>? except = null
    )
    {
        var assembly = named.ContainingAssembly;

        bool IsValid(Extract next) =>
            From(next.Symbol, assembly) is not null &&
            (except is null || MemberSymbol.DeconstructFrom(next.Symbol) is not { } x || !except.Contains(x));

        var signatures = ToSelf(except, assembly);
        var forwarders = Add(symbols, assembly, signatures);

        forwarders.UnionWith(new IntersectedInterfaces(symbols, named.IsReadOnly).Members);
        forwarders.UnionWith(FindCommonBaseTypes(symbols));
        forwarders.ExceptWith(forwarders.SelectMany(GeneratedMethods).Filter());

        return forwarders.Where(IsValid);
    }

    [Pure]
    public static RefKind Kind(in MemberSymbol x) => Kind(x.Symbol);

    [Pure]
    public static RefKind Kind(in ISymbol x) =>
        x switch
        {
            IFieldSymbol { RefKind: var ret } => ret,
            IMethodSymbol { RefKind: var ret } => ret,
            IPropertySymbol { RefKind: var ret } => ret,
            _ => throw Unreachable,
        };

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

    [Pure]
    public static Signature? From(ISymbol symbol, IAssemblySymbol assembly) =>
        symbol.CanBeAccessedFrom(assembly) ? From(symbol) : null;

    [Pure]
    public bool Equivalent(in Signature other) =>
        Name.AsSpan().SplitOn('.').Last.SequenceEqual(other.Name.AsSpan().SplitOn('.').Last) &&
        SameType(Type, other.Type) &&
        Parameters.GuardedSequenceEqual(other.Parameters, SameType) &&
        TypeParameters.GuardedSequenceEqual(other.TypeParameters, Complies);

    [Pure]
    public bool IsIn(in MemberSymbol union, IAssemblySymbol assembly, out string? interfaceDeclaration)
    {
        foreach (var member in union.Type.GetMembers())
            if (From(member, assembly) is { } signature && Equivalent(signature))
            {
                interfaceDeclaration = InterfaceDeclaration(member);
                return true;
            }

        interfaceDeclaration = null;
        return false;
    }

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
            hash = hash * Prime ^ TypeSymbolComparer.GetHashCode(Type);
            hash = hash * Prime ^ Parameters.Length;
            return hash * Prime ^ TypeParameters.Length;
        }
    }

    void Next(
        in SmallList<MemberSymbol> symbols,
        HashSet<Extract> set,
        HashSet<Signature> exists,
        IAssemblySymbol assembly,
        ISymbol member
    )
    {
        SmallList<string?> small = InterfaceDeclaration(member);
        var kind = Kind(symbols.First);

        for (var i = 1; i < symbols.Count; i++)
        {
            var current = symbols[i];

            if (!IsIn(current, assembly, out var interfaceDeclaration))
                break;

            kind = Min(kind, Kind(current));
            small.Add(interfaceDeclaration);

            if (i == symbols.Count - 1 && !exists.Contains(this))
                set.Add((member, kind, small));
        }
    }

    [Pure]
    static bool AnyBaseType(ITypeSymbol x, ITypeSymbol y) =>
        x.FindSmallPathToNull(x => x.BaseType).Any(x => TypeSymbolComparer.Equal(x, y));

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

    [Pure]
    static bool SameType(IParameterSymbol left, IParameterSymbol right) => SameType(left.Type, right.Type);

    [Pure]
    static bool SameType(ITypeSymbol left, ITypeSymbol right) =>
        TypeSymbolComparer.Equal(left, right) &&
        TypeSymbolComparer.Equal(left.ToUnderlying(), right.ToUnderlying()) &&
        left.ContainingType is null == right.ContainingType is null &&
        (left.ContainingType is null || SameType(left.ContainingType, right.ContainingType));

    [Pure]
    static string? InterfaceDeclaration(ISymbol x) => x.Name.LastIndexOf('.') is not -1 and var i ? x.Name[..i] : null;

    [Pure]
    static Extract AsDirectExtract(ISymbol x) => (x, Kind(x), default);

    [Pure]
    static Extract AsDirectExtract(ISymbol x, int count, string? element = null) =>
        (x, Kind(x), Enumerable.Repeat(element, count).ToSmallList());

    [Pure]
    static HashSet<Extract> Add(
        in SmallList<MemberSymbol> symbols,
        IAssemblySymbol assembly,
        HashSet<Signature> exists
    )
    {
        HashSet<Extract> set = new(s_extracts);

        foreach (var member in symbols.First.Type.GetMembers())
            if (From(member, assembly) is { } signature)
                signature.Next(symbols, set, exists, assembly, member);

        return set;
    }

    [Pure]
    static IEnumerable<Extract> FindCommonBaseTypes(in SmallList<MemberSymbol> symbols)
    {
        var smalls = symbols
           .Select(x => x.Type.BaseType.FindPathToNull(x => x.BaseType).Reverse().ToListLazily())
           .ToSmallList();

        var min = smalls.Min(x => x.Count);
        var count = symbols.Count;

        for (var i = 0; i < min; i++)
            for (var j = 1; j < smalls.Count; j++)
            {
                if (!SameType(smalls.First[i], smalls[j][i]))
                    break;

                if (j == smalls.Count - 1)
                    return smalls.First.Skip(i).SelectMany(x => x.GetMembers()).Select(x => AsDirectExtract(x, count));
            }

        return [];
    }

    [Pure]
    static IEnumerable<Extract> GeneratedMethods(Extract symbol) =>
        (symbol switch
        {
            (IEventSymbol x, _, _) => SmallList.Create(x.AddMethod, x.RaiseMethod, x.RemoveMethod),
            (IPropertySymbol x, _, _) => SmallList.Create(x.GetMethod, x.SetMethod),
            _ => default,
        })
       .Filter()
       .Select(AsDirectExtract);

    [Pure]
    static RefKind Min(RefKind left, RefKind right) =>
        left is RefKind.None || right is RefKind.None ? RefKind.None :
        left is RefKind.Ref || right is RefKind.Ref ? RefKind.Ref : RefKind.RefReadOnly;

    [Pure]
    static HashSet<Signature> ToSelf(IEnumerable<MemberSymbol>? except, IAssemblySymbol assembly) =>
        except.OrEmpty().Select(x => From(x.Type, assembly)).Filter().ToSet(s_signatures);
}
