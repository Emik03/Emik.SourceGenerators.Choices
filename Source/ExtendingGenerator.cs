// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>The source generator that implements implicit operators.</summary>
[Generator]
public sealed class ExtendingGenerator : IIncrementalGenerator
{
    const int MinimumFields = 2;

    static readonly IEqualityComparer<Fold> s_folds = Equating(
        (Fold x, Fold y) => Same(x, y) && NamedTypeSymbolComparer.Equal(x.SymbolSet, y.SymbolSet)
    );

    static readonly IEqualityComparer<Raw> s_raws = Equating(
        (Raw x, Raw y) => Same(x, y) && SameMembers(x.Fields, y.Fields)
    );

    static readonly IEqualityComparer<Scaffolder> s_comparer = Equating<Scaffolder, Raw>(
        x => x is null ? default : (x.Named, x.Symbols, x.MutablePublicly),
        s_raws
    );

    /// <inheritdoc />
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
           .Where(HasSufficientFields)
           .Select(Scaffolder.From)
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
    static bool IsExtendable(SyntaxNode node, CancellationToken token)
    {
        if (node is not TypeDeclarationSyntax { AttributeLists.Count: >= 1 } type)
            return false;

        if (type is InterfaceDeclarationSyntax)
            return false;

        foreach (var modifier in type.Modifiers)
        {
            token.ThrowIfCancellationRequested();

            if (modifier.IsKind(SyntaxKind.PartialKeyword))
                return true;
        }

        return false;
    }

    [Pure]
    static bool Same<T>(
        in (INamedTypeSymbol Named, T _, bool? MutablePublicly) x,
        in (INamedTypeSymbol Named, T _, bool? MutablePublicly) y
    ) =>
        x.MutablePublicly == y.MutablePublicly &&
        SameMetadataNames(x.Named, y.Named);

    [Pure]
    static bool SameMembers(in SmallList<FieldOrProperty> xs, in SmallList<FieldOrProperty> ys)
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

        while (true)
        {
            if (x is null)
                return y is null;

            if (y is null || x.MetadataName != y.MetadataName)
                return false;

            (x, y) = (x.ContainingSymbol, y.ContainingSymbol);
        }
    }

    [Pure]
    static Raw DiscoverFields(Fold x, CancellationToken _)
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
    static Fold Target(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        bool? mutablePublicly = null;
        INamedTypeSymbol? symbolSet = null;

        foreach (var arg in context.Attributes[0].ConstructorArguments)
            switch (arg.Value)
            {
                case bool x:
                    mutablePublicly = x;
                    break;
                case INamedTypeSymbol x:
                    symbolSet = x;
                    break;
            }

        return ((INamedTypeSymbol)context.TargetSymbol, symbolSet, mutablePublicly);
    }
}
