// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>The source generator that implements implicit operators.</summary>
[Generator]
public sealed class ExtendingGenerator : IIncrementalGenerator
{
    const int MinimumFields = 2;

    static readonly IEqualityComparer<Fold> s_folds = Equating((Fold x) => x.Named, NamedTypeSymbolComparer.Default);

    static readonly IEqualityComparer<Raw> s_raws = Equating((Raw x) => x.Named, NamedTypeSymbolComparer.Default);

    static readonly IEqualityComparer<Scaffolder> s_comparer = Equating(
        (Scaffolder? x) => x?.Named, // ReSharper disable once NullableWarningSuppressionIsUsed
        NamedTypeSymbolComparer.Default!
    );

    /// <inheritdoc />
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context
           .SyntaxProvider
           .ForAttributeWithMetadataName(Of<AttributeGenerator>(), AnnotatedAndIs<BaseTypeDeclarationSyntax>, Target)
           .WithComparer(s_folds)
           .WithTrackingName(nameof(Fold))
           .Where(HasAnnotatedCorrectly)
           .Select(DiscoverFields)
           .WithComparer(s_raws)
           .WithTrackingName(nameof(Raw))
           .Where(HasSufficientFields)
           .Select(ToScaffolder)
           .WithComparer(s_comparer)
           .WithTrackingName(nameof(Scaffolder));

        context.RegisterSourceOutput(provider, Generate);
    }

    static void Generate(SourceProductionContext context, Scaffolder x) => AddSource(context, x.Result);

    [Pure]
    static bool HasAnnotatedCorrectly(Fold x) =>
        x is ({ IsTupleType: false }, null or { IsTupleType: true, TupleElements.Length: >= MinimumFields }, _);

    [Pure]
    static bool HasSufficientFields((INamedTypeSymbol, SmallList<FieldOrProperty>, bool?) x) =>
        x is (_, { Count: >= MinimumFields }, _);

    [Pure]
    static Raw DiscoverFields(
        Fold x,
        CancellationToken _
    )
    {
        var (type, named, isPubliclyMutable) = x;

        var fields = named switch
        {
            { IsTupleType: true, TupleElements: { Length: > 1 } e } => Scaffolder.Decouple(e),
            null => Scaffolder.Instances(type),
            _ when Scaffolder.IsSystemTuple(named) => Scaffolder.Instances(named),
            _ => default,
        };

        return (type, fields, isPubliclyMutable);
    }

    [Pure]
    static Fold Target(
        GeneratorAttributeSyntaxContext context,
        CancellationToken _
    )
    {
        bool? publiclyMutable = null;
        INamedTypeSymbol? symbolSet = null;

        foreach (var arg in context.Attributes[0].ConstructorArguments)
            switch (arg.Value)
            {
                case bool x:
                    publiclyMutable = x;
                    break;
                case INamedTypeSymbol x:
                    symbolSet = x;
                    break;
            }

        return ((INamedTypeSymbol)context.TargetSymbol, symbolSet, publiclyMutable);
    }

    [Pure]
    static Scaffolder ToScaffolder((INamedTypeSymbol, SmallList<FieldOrProperty>, bool?) x, CancellationToken _) =>
        Scaffolder.From(x);
}
