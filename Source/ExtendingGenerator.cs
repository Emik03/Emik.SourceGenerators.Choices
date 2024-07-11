// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>The source generator that implements implicit operators.</summary>
[Generator]
public sealed class ExtendingGenerator : IIncrementalGenerator
{
    const int MinimumFields = 2;

    [Pure]
    public static GeneratedSource? Transform(
        INamedTypeSymbol named,
        bool? publiclyMutable = true,
        bool polyfillAttributes = false,
        params MemberSymbol[] fields
    ) =>
        !named.IsTupleType &&
        fields.Length >= MinimumFields &&
        (named, fields.ToSmallList(), publiclyMutable, polyfillAttributes) is var raw &&
        HasSufficientFields(raw)
            ? ((Scaffolder)raw).Result
            : null;

    /// <inheritdoc />
    // ReSharper disable once ArrangeAttributes
    // [Choice.A<int>.B<int>]
    // [Choice(typeof((int A, int B)))]
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dot = context.SyntaxProvider.CreateSyntaxProvider(IsHeavilyNested, DiscoverTypeParameters);
        var org = context.SyntaxProvider.ForAttributeWithMetadataName(Of<AttributeGenerator>(), IsExtendable, Target);
        Register(context, [dot, org]);
    }

    static void Generate(SourceProductionContext context, Raw raw) => AddSource(context, ((Scaffolder)raw).Result);

    static void Register(
        in IncrementalGeneratorInitializationContext context,
        scoped in ReadOnlySpan<IncrementalValuesProvider<Raw>> providers
    )
    {
        foreach (var provider in providers)
            context.RegisterSourceOutput(
                provider.WithComparer(RawEqualityComparer.Instance)
                   .WithTrackingName(nameof(Raw))
                   .Where(HasSufficientFields),
                Generate
            );
    }

    [Pure]
    static bool HasAnnotatedCorrectly(Fold x) =>
        x is ({ IsTupleType: false }, null or { IsTupleType: true, TupleElements.Length: >= MinimumFields }, _);

    [Pure]
    static bool HasSufficientFields(Raw x) => x is ({ IsStatic: false }, { Count: >= MinimumFields }, _, _);

    [Pure]
    static bool IsExtendable(SyntaxNode x, CancellationToken _ = default) =>
        x is TypeDeclarationSyntax { AttributeLists.Count: >= 1, Modifiers: var modifiers } and
            not InterfaceDeclarationSyntax and
            not DelegateDeclarationSyntax &&
        modifiers.Any(SyntaxKind.PartialKeyword);

    [Pure]
    static bool IsHeavilyNested(SyntaxNode x, CancellationToken _ = default) =>
        // Guarantees that at the minimum it involves: [Rest.Property<T1>.Property<T2>]
        x is AttributeSyntax
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

        return (type, fields, mutablePublicly, false);
    }

    // ReSharper disable once CognitiveComplexity
    static Raw DiscoverTypeParameters(GeneratorSyntaxContext context, CancellationToken token)
    {
        if (context.Node is not AttributeSyntax attribute ||
            attribute.TypeDeclaration() is not { } typeDeclaration ||
            context.SemanticModel.GetSymbolInfo(typeDeclaration, token).Symbol is not INamedTypeSymbol named)
            return default;

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
                    default: return default;
                }

            if (qualifiedName.Left is IdentifierNameSyntax leftIdentifier)
                if (leftIdentifier.Identifier.Text is "Choice")
                    break;
                else
                    return default;

            if (mutablePublicly is not null ||
                qualifiedName.Right is not GenericNameSyntax genericName ||
                genericName.TypeArgumentList.Arguments is not [var typeArgument] ||
                context.SemanticModel.GetSymbolInfo(typeArgument, token).Symbol is not ITypeSymbol type)
                return default;

            fields.Add(new(type, genericName.Identifier.Text));
        }

        return (named, fields, mutablePublicly, true);
    }

    [Pure]
    static Raw Target(GeneratorAttributeSyntaxContext context, CancellationToken token)
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

        return ((INamedTypeSymbol)context.TargetSymbol, symbolSet, mutablePublicly) is var fold &&
            HasAnnotatedCorrectly(fold)
                ? DiscoverFields(fold, token)
                : default;
    }
}
