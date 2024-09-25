// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>The source generator responsible for generating disjoint union functionality to annotated types.</summary>
[Generator]
public sealed class ExtendingGenerator : IIncrementalGenerator
{
    const int MinimumMembers = 2;

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
        members.Length >= MinimumMembers &&
        (named, members.ToSmallList(), publiclyMutable, polyfillAttributes) is var raw &&
        HasSufficientMembers(raw)
            ? Scaffolder.From(raw).Result
            : null;

    /// <inheritdoc />
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dot = context.SyntaxProvider.CreateSyntaxProvider(IsHeavilyNested, DiscoverDotDeclaration);
        var org = context.SyntaxProvider.ForAttributeWithMetadataName(Of<AttributeGenerator>(), IsExtendable, Discover);
        Register(context, dot);
        Register(context, org);
    }

    /// <summary>Creates the generated source to the <see cref="SourceProductionContext"/>.</summary>
    /// <param name="context">The context to register source code.</param>
    /// <param name="raw">The values to base the source generation from.</param>
    static void Generate(SourceProductionContext context, Raw raw) => AddSource(context, Scaffolder.From(raw).Result);

    /// <summary>
    /// Registers the provider to the generator, which also includes
    /// injecting the <see cref="RawEqualityComparer"/> and explicit filter.
    /// </summary>
    /// <param name="context">The context to register source code.</param>
    /// <param name="values">The provider for generating source code.</param>
    static void Register(
        in IncrementalGeneratorInitializationContext context,
        in IncrementalValuesProvider<Raw> values
    ) =>
        context.RegisterSourceOutput(
            values.WithComparer(RawEqualityComparer.Instance).WithTrackingName(nameof(Raw)).Where(HasSufficientMembers),
            Generate
        );

    /// <summary>Determines whether the parameter <paramref name="x"/> is annotated correctly.</summary>
    /// <param name="x">The parameter to check.</param>
    /// <returns>
    /// The value <see langword="true"/> if the parameter <paramref name="x"/>
    /// is annotated correctly; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    static bool HasSufficientMembers(Raw x) => x is ({ IsStatic: false }, { Count: >= MinimumMembers }, _, _);

    /// <summary>Determines whether the parameter <paramref name="x"/> is extendable.</summary>
    /// <param name="x">The parameter to check.</param>
    /// <param name="_">The cancellation token, which is unused.</param>
    /// <returns>
    /// The value <see langword="true"/> if the parameter <paramref name="x"/>
    /// is extendable; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    static bool IsExtendable(SyntaxNode x, CancellationToken _ = default) =>
        x is TypeDeclarationSyntax { AttributeLists.Count: >= 1, Modifiers: var modifiers } and
            not InterfaceDeclarationSyntax &&
        modifiers.Any(SyntaxKind.PartialKeyword) &&
        !modifiers.Any(SyntaxKind.StaticKeyword);

    /// <summary>
    /// Determines whether the parameter <paramref name="x"/> is heavily nested.
    /// This refers to whether it is a candidate in dot-declaration syntax.
    /// </summary>
    /// <param name="x">The parameter to check.</param>
    /// <param name="_">The cancellation token, which is unused.</param>
    /// <returns>
    /// The value <see langword="true"/> if the parameter <paramref name="x"/> is
    /// a candidate in dot-declaration syntax; otherwise, <see langword="false"/>.
    /// </returns>
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

    /// <summary>Extracts the members from the node's <see cref="AttributeSyntax"/>.</summary>
    /// <param name="context">The syntax context.</param>
    /// <param name="token">
    /// The cancellation token used for cancelling the iteration over <see cref="QuailifiedNameSyntax"/> instances.
    /// </param>
    /// <returns>The extracted result, or <see langword="default"/> if the input is invalid.</returns>
    // ReSharper disable once CognitiveComplexity
    static Raw DiscoverDotDeclaration(GeneratorSyntaxContext context, CancellationToken token)
    {
        if (context.Node is not AttributeSyntax attribute ||
            attribute.TypeDeclaration() is not { } typeDeclaration ||
            context.SemanticModel.GetDeclaredSymbol(typeDeclaration, token) is not { IsStatic: false } named)
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

    /// <summary>Extracts the members from the node.</summary>
    /// <param name="context">The context to check the node from.</param>
    /// <param name="token">The cancellation token used for interrupting the iteration of constructor arguments.</param>
    /// <returns>The extracted result, or <see langword="default"/> if the input is invalid.</returns>
    [Pure]
    static Raw Discover(GeneratorAttributeSyntaxContext context, CancellationToken token)
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

        if (context.TargetSymbol is not INamedTypeSymbol { IsTupleType: false } target ||
            symbolSet is not null and not { IsTupleType: true, TupleElements.Length: >= MinimumMembers })
            return default;

        var primaryConstructorParameters = target
           .DeclaringSyntaxReferences
           .SelectMany(x => x.GetSyntax(token).DescendantNodes())
           .OfType<ParameterListSyntax>()
           .Filter()
           .FirstOrDefault()
          ?.Parameters
           .Select(x => context.SemanticModel.GetDeclaredSymbol(x, token))
           .Filter()
           .Select(x => new MemberSymbol(x))
           .ToSmallList();

        var fields = symbolSet switch
        {
            { IsTupleType: true, TupleElements: { Length: > 1 } e } => Scaffolder.Decouple(e),
            _ when Scaffolder.IsSystemTuple(symbolSet) => Scaffolder.Instances(symbolSet),
            null when primaryConstructorParameters is { } p => p,
            null => Scaffolder.Instances(target),
            _ => default,
        };

        return (target, fields, mutablePublicly, false);
    }
}
