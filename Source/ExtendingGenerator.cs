// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>The source generator that implements implicit operators.</summary>
[Generator]
public sealed class ExtendingGenerator : IIncrementalGenerator
{
    const int MinimumFields = 2;

    static readonly IEqualityComparer<Fold> s_folds =
        Equating<Fold>((x, y) => Same(x, y) && NamedTypeSymbolComparer.Equal(x.SymbolSet, y.SymbolSet), Hash);

    static readonly IEqualityComparer<Raw> s_raws =
        Equating<Raw>((x, y) => Same(x, y) && SameMembers(x.Fields, y.Fields), Hash);

    [Pure]
    public static GeneratedSource? Transform(in Fold fold) =>
        HasAnnotatedCorrectly(fold) && DiscoverFields(fold) is var raw && HasSufficientFields(raw)
            ? Scaffolder.From(raw).Result
            : null;

    /// <inheritdoc />
    // ReSharper disable once ArrangeAttributes
    // [Choice.A<int>.B<int>]
    // [Choice(typeof((int A, int B)))]
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context
           .SyntaxProvider
           .ForAttributeWithMetadataName(Of<AttributeGenerator>(), IsExtendable, Target)
           .WithComparer(s_folds)
           .WithTrackingName(nameof(Fold))
           .Where(HasAnnotatedCorrectly)
           .Select(DiscoverFields)
           .WithComparer(s_raws)
           .WithTrackingName(nameof(Raw))
           .Where(HasSufficientFields);

        var a = context
           .SyntaxProvider
           .CreateSyntaxProvider(IsHeavilyNested, DiscoverTypeParameters);

        context.RegisterSourceOutput(provider, Generate);
    }

    static Raw? DiscoverTypeParameters(GeneratorSyntaxContext context, CancellationToken token)
    {
        if (context.Node is not AttributeSyntax attribute ||
            attribute.TypeDeclaration() is not { } typeDeclaration ||
            context.SemanticModel.GetSymbolInfo(typeDeclaration, token).Symbol is not INamedTypeSymbol named)
            return null;

        SmallList<MemberSymbol> fields = [];
        bool? mutablePublicly = null;

        for (var name = attribute.Name; name is QualifiedNameSyntax qualifiedName; name = qualifiedName.Left)
        {
            if (qualifiedName.Right is IdentifierNameSyntax rightIdentifier)
                switch (rightIdentifier.Identifier.Text)
                {
                    case nameof(Accessibility.Private):
                        mutablePublicly = false;
                        break;
                    case nameof(Accessibility.Public):
                        mutablePublicly = true;
                        break;
                    default: return null;
                }

            if (qualifiedName.Left is IdentifierNameSyntax leftIdentifier)
                if (leftIdentifier.Identifier.Text is "Choice")
                    break;
                else
                    return null;

            if (mutablePublicly is not null ||
                qualifiedName.Right is not GenericNameSyntax genericName ||
                genericName.TypeArgumentList.Arguments is not [var typeArgument] ||
                context.SemanticModel.GetSymbolInfo(typeArgument, token).Symbol is not ITypeSymbol type)
                return null;

            fields.Add(new(type, genericName.Identifier.Text));
        }

        return (named, fields, mutablePublicly);
    }

    static bool IsHeavilyNested(SyntaxNode node, CancellationToken _) =>
        // Guarantees that at the minimum it involves: [Rest.Property<T1>.Property<T2>]
        node is AttributeSyntax
        {
            Name: QualifiedNameSyntax
            {
                Left: QualifiedNameSyntax
                {
                    Left: IdentifierNameSyntax { Identifier.Text: "Choice" } or
                    QualifiedNameSyntax
                    {
                        Left: IdentifierNameSyntax { Identifier.Text: "Choice" } or QualifiedNameSyntax,
                        Right: IdentifierNameSyntax
                        {
                            Identifier.Text: nameof(Accessibility.Private) or nameof(Accessibility.Public),
                        } or
                        GenericNameSyntax,
                    },
                    Right: GenericNameSyntax,
                },
                Right: GenericNameSyntax,
            },
        };

    static void Generate(SourceProductionContext context, Raw x) => AddSource(context, Scaffolder.From(x).Result);

    [Pure]
    static bool HasAnnotatedCorrectly(Fold x) =>
        x is ({ IsTupleType: false }, null or { IsTupleType: true, TupleElements.Length: >= MinimumFields }, _);

    [Pure]
    static bool HasSufficientFields(Raw x) => x is (_, { Count: >= MinimumFields }, _);

    [Pure]
    static bool IsExtendable(SyntaxNode node, CancellationToken token) =>
        node is TypeDeclarationSyntax { AttributeLists.Count: >= 1, Modifiers: var modifiers } and
            not InterfaceDeclarationSyntax and
            not DelegateDeclarationSyntax &&
        modifiers.Any(SyntaxKind.PartialKeyword);

    [Pure]
    static bool Same<T>(
        in (INamedTypeSymbol Named, T _, bool? MutablePublicly) x,
        in (INamedTypeSymbol Named, T _, bool? MutablePublicly) y
    ) =>
        x.MutablePublicly == y.MutablePublicly &&
        x.Named.Keyword() == y.Named.Keyword() &&
        SameMetadataNames(x.Named, y.Named);

    [Pure]
    static bool SameMembers(in SmallList<MemberSymbol> xs, in SmallList<MemberSymbol> ys)
    {
        if (xs.Count != ys.Count)
            return false;

        for (var i = 0; i < xs.Count; i++)
            if (!xs[i].Equals(ys[i]))
                return false;

        return true;
    }

    [Pure]
    static bool SameMetadataNames(ISymbol? x, ISymbol? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        for (; x is not null; x = x.ContainingSymbol, y = y.ContainingSymbol)
            if (y is null || x.MetadataName != y.MetadataName)
                return false;

        return y is null;
    }

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
    static int Hash<T>((INamedTypeSymbol Named, T _, bool? MutablePublicly) x) =>
        (BetterHashCode(x.MutablePublicly) * 42061 ^ StringComparer.Ordinal.GetHashCode(x.Named.MetadataName)) * 42071 ^
        StringComparer.Ordinal.GetHashCode(x.Named.Keyword());

    [Pure]
    static Raw DiscoverFields(Fold x, CancellationToken _ = default)
    {
        var (type, named, mutablePublicly) = x;

        var fields = named switch
        {
            { IsTupleType: true, TupleElements: { Length: > 1 } e } => Scaffolder.Decouple(e),
            null => Scaffolder.Instances(type),
            _ when Scaffolder.IsSystemTuple(named) => Scaffolder.Instances(named),
            _ => default,
        };

        return (type, fields, mutablePublicly);
    }

    [Pure]
    static Fold Target(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        bool? mutablePublicly = null;
        INamedTypeSymbol? symbolSet = null;

        foreach (var arg in context.Attributes[0].ConstructorArguments)
        {
            token.ThrowIfCancellationRequested();

            _ = arg.Value switch
            {
                bool x => mutablePublicly = x,
                INamedTypeSymbol x => (symbolSet = x) is var _,
                _ => false,
            };
        }

        return ((INamedTypeSymbol)context.TargetSymbol, symbolSet, mutablePublicly);
    }
}
