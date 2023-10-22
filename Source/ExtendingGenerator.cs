// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>The source generator that implements implicit operators.</summary>
[Generator]
public sealed class ExtendingGenerator : IIncrementalGenerator
{
    const int MinimumFields = 2;

    /// <inheritdoc />
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context
           .SyntaxProvider
           .ForAttributeWithMetadataName(Of<AttributeGenerator>(), AnnotatedAndIs<BaseTypeDeclarationSyntax>, Target)
           .Where(HasAnnotatedCorrectly)
           .Select(DiscoverFields)
           .Where(HasSufficientFields)
           .Select(GenerateSource);

        context.RegisterSourceOutput(provider, AddSource);
    }

    [Pure]
    static bool HasAnnotatedCorrectly((INamedTypeSymbol, INamedTypeSymbol?, bool?) x) =>
        x is ({ IsTupleType: false }, null or { IsTupleType: true, TupleElements.Length: >= MinimumFields }, _);

    [Pure]
    static bool HasSufficientFields((INamedTypeSymbol, SmallList<FieldOrProperty>, bool?) x) =>
        x is (_, { Count: >= MinimumFields }, _);

    [Pure]
    static (string, string) GenerateSource(
        (INamedTypeSymbol, SmallList<FieldOrProperty>, bool?) x,
        CancellationToken _
    )
    {
        var scaffolder = Scaffolder.From(x);
        return (scaffolder.HintName, scaffolder.Source);
    }

    [Pure]
    static (INamedTypeSymbol, SmallList<FieldOrProperty>, bool?) DiscoverFields(
        (INamedTypeSymbol, INamedTypeSymbol?, bool?) x,
        CancellationToken _
    )
    {
        var (type, named, isPubliclyMutable) = x;

        var fields = (named switch
        {
            { IsTupleType: true, TupleElements: { Length: > 1 } e } => Scaffolder.Decouple(e),
            null => Scaffolder.Instances(type),
            _ when Scaffolder.IsSystemTuple(named) => Scaffolder.Instances(named),
            _ => Enumerable.Empty<FieldOrProperty>(),
        }).ToSmallList();

        return (type, fields, isPubliclyMutable);
    }

    [Pure]
    static (INamedTypeSymbol, INamedTypeSymbol?, bool?) Target(
        GeneratorAttributeSyntaxContext context,
        CancellationToken _
    )
    {
        var values = context
           .Attributes[0]
           .ConstructorArguments
           .Select(x => x.Value)
           .ToSmallList();

        return (
            (INamedTypeSymbol)context.TargetSymbol,
            values.OfType<INamedTypeSymbol>().FirstOrDefault(),
            values.OfType<bool>().Cast<bool?>().FirstOrDefault()
        );
    }
}
