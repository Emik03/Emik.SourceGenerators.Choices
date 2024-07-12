// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>The source generator responsible for generating disjoint union functionality to annotated types.</summary>
[Generator]
public sealed class ExtendingGenerator : IIncrementalGenerator
{
    const int MinimumFields = 2;

    /// <summary>Executes this source generation based on given input.</summary>
    /// <remarks><para>
    /// This API is unused within the library, but can be used by other libraries that reference
    /// this library to integrate this source generator however they wish. Do note that the API
    /// is subject to change, but the general premise of the function will remain the same.
    /// </para></remarks>
    /// <param name="named">The type to generate for.</param>
    /// <param name="publiclyMutable">
    /// If <see langword="null"/>, the fields and properties generated will be immutable.
    /// If <see langword="false"/>, the fields and properties generated will be mutable, but only from within the type.
    /// If <see langword="true"/>, the fields and properties generated will be publicly mutable.
    /// </param>
    /// <param name="polyfillAttributes">
    /// Determines whether to generate private attributes required to make dot-declaration
    /// syntax work. Set this to <see langword="true"/> if you have annotated the parameter
    /// <paramref name="named"/> using the dot-declaration feature; nested class attributes such as
    /// <c>[Choice.Public.Foo&lt;Bar&gt;.Baz&lt;Qux&gt;]</c> versus <c>[Choice(true, typeof((Bar Foo, Qux Baz)))]</c>.
    /// </param>
    /// <param name="members">
    /// The variants of the parameter <paramref name="named"/>. Variants created from a <see cref="IFieldSymbol"/> or
    /// <see cref="IPropertySymbol"/> will not create the respective members in the source generation, since this
    /// indicates that the member already exists, and would cause a compiler error if a second member was generated.
    /// </param>
    /// <returns>
    /// The generated source. This contains the hint name, which indicates the name of the file, and the source,
    /// representing the contents of said file. This function will return <see langword="null"/> under a few
    /// circumstances. This includes the parameter <paramref name="named"/> being <see langword="null"/>, being a tuple
    /// type regardless of if it is a polyfill or not, not being annotated with <c>[Choice]</c> or annotated correctly,
    /// not being fully <see langword="partial"/> which includes any types that contain the parameter
    /// <paramref name="named"/>, the parameter <see cref="members"/> being less than 2 elements long.
    /// </returns>
    [Pure]
    public static GeneratedSource? Transform(
        INamedTypeSymbol? named,
        bool? publiclyMutable = null,
        bool polyfillAttributes = false,
        params MemberSymbol[] members
    ) =>
        named is { IsTupleType: false } &&
        members.Length >= MinimumFields &&
        (named, members.ToSmallList(), publiclyMutable, polyfillAttributes) is var raw &&
        HasSufficientFields(raw)
            ? ((Scaffolder)raw).Result
            : null;

    /// <inheritdoc />
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dot = context.SyntaxProvider.CreateSyntaxProvider(IsHeavilyNested, DiscoverTypeParameters);
        var org = context.SyntaxProvider.ForAttributeWithMetadataName(Of<AttributeGenerator>(), IsExtendable, Target);
        Register(context, dot);
        Register(context, org);
    }

    static void Generate(SourceProductionContext context, Raw raw) => AddSource(context, ((Scaffolder)raw).Result);

    static void Register(
        in IncrementalGeneratorInitializationContext context,
        in IncrementalValuesProvider<Raw> values
    ) =>
        context.RegisterSourceOutput(
            values.WithComparer(RawEqualityComparer.Instance).WithTrackingName(nameof(Raw)).Where(HasSufficientFields),
            Generate
        );

    [Pure]
    static bool HasAnnotatedCorrectly(Fold x) =>
        x is ({ IsTupleType: false }, null or { IsTupleType: true, TupleElements.Length: >= MinimumFields }, _);

    [Pure]
    static bool HasSufficientFields(Raw x) => x is ({ IsStatic: false }, { Count: >= MinimumFields }, _, _);

    [Pure]
    static bool IsExtendable(SyntaxNode x, CancellationToken _ = default) =>
        x is TypeDeclarationSyntax { AttributeLists.Count: >= 1, Modifiers: var modifiers } and
            not InterfaceDeclarationSyntax &&
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
            context.SemanticModel.GetDeclaredSymbol(typeDeclaration, token) is not { } named)
            return default;

        SmallList<MemberSymbol> fields = [];
        bool? mutablePublicly = null;

        for (var name = attribute.Name; name is QualifiedNameSyntax qualifiedName; name = qualifiedName.Left)
        {
            token.ThrowIfCancellationRequested();

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

            if (qualifiedName is { Left: IdentifierNameSyntax leftIdentifier, Right: not GenericNameSyntax })
                if (leftIdentifier.Identifier.Text is "Choice")
                    break;
                else
                    return default;

            if (mutablePublicly is not null ||
                qualifiedName.Right is not GenericNameSyntax genericName ||
                genericName.TypeArgumentList.Arguments is not [var typeArgument] ||
                (context.SemanticModel.GetDeclaredSymbolSafe(typeArgument, token) ??
                    context.SemanticModel.GetSymbolSafe(typeArgument, token)) is not ITypeSymbol type)
                return default;

            fields.Add(new(type, genericName.Identifier.Text));
        }

        fields.Reverse();
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
