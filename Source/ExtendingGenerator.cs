// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>The source generator that implements implicit operators.</summary>
[Generator]
public sealed class ExtendingGenerator : IIncrementalGenerator
{
    const int MinimumFields = 2;

    static readonly IEqualityComparer<Fold> s_folds = Equating(
        (Fold x, Fold y) => Same(x, y) && NamedTypeSymbolComparer.Equal(x.SymbolSet, y.SymbolSet),
        Hash
    );

    static readonly IEqualityComparer<Raw> s_raws = Equating(
        (Raw x, Raw y) => Same(x, y) && SameMembers(x.Fields, y.Fields),
        Hash
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
           .Where(HasSufficientFields);

        context.RegisterSourceOutput(provider, Generate);
    }

    static void Generate(SourceProductionContext context, Raw x) => AddSource(context, Scaffolder.From(x).Result);

    [Pure]
    static bool HasAnnotatedCorrectly(Fold x) =>
        x is ({ IsTupleType: false }, null or { IsTupleType: true, TupleElements.Length: >= MinimumFields }, _);

    [Pure]
    static bool HasSufficientFields((INamedTypeSymbol, SmallList<FieldOrProperty>, bool?) x) =>
        x is (_, { Count: >= MinimumFields }, _);

    [Pure]
    static bool IsExtendable(SyntaxNode node, CancellationToken token)
    {
        if (node is not TypeDeclarationSyntax { AttributeLists.Count: >= 1 } type || type is InterfaceDeclarationSyntax)
            return false;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var modifier in type.Modifiers)
            if (modifier.IsKind(SyntaxKind.PartialKeyword))
                return true;

        return false;
    }

    [Pure]
    static bool Same<T>(
        in (INamedTypeSymbol Named, T _, bool? MutablePublicly) x,
        in (INamedTypeSymbol Named, T _, bool? MutablePublicly) y
    ) =>
        x.MutablePublicly == y.MutablePublicly && SameMetadataNames(x.Named, y.Named);

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
        (BetterHashCode(x.MutablePublicly) * 42061 ^ StringComparer.Ordinal.GetHashCode(x.Named.MetadataName)) * 42071;

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
