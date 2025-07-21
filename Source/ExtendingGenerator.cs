// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>The source generator responsible for generating disjoint union functionality to annotated types.</summary>
[Generator]
public sealed class ExtendingGenerator : IIncrementalGenerator
{
    /// <summary>The name of the <see cref="Choice"/> attribute.</summary>
    public const string Choice = nameof(Choice);

    /// <summary>The minimum number of members required to generate a disjoint union.</summary>
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
    /// <param name="fullyPolyfillAttributes">
    /// Determines whether to generate private attributes required to make dot-declaration syntax work. Set this
    /// to <see langword="null"/> if you are not using dot-declaration syntax, causing no attributes to be generated.
    /// Set this to <see langword="false"/> if you have annotated the parameter <paramref name="named"/>
    /// using the unqualified form, directly mentioning <c>[Choice]</c>. Set this to <see langword="true"/>
    /// if the annotation is fully qualified as <c>[Emik.Choice]</c>. An example of the dot declaration syntax would be
    /// <c>[Choice.Public.Foo&lt;Bar&gt;.Baz&lt;Qux&gt;]</c> and not <c>[Choice(typeof(true, (Bar Foo, Qux Baz)))]</c>.
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
        bool? fullyPolyfillAttributes = null,
        params MemberSymbol[] members
    ) =>
        named is { IsTupleType: false } &&
        members.Length >= MinimumMembers &&
        (named, AsImmutableArray(members), publiclyMutable, fullyPolyfillAttributes) is var raw &&
        HasSufficientMembers(raw)
            ? new Scaffolder(raw).Result
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
    static void Generate(SourceProductionContext context, Raw raw)
#if LOG_PERFORMANCE
    {
        var sw = Stopwatch.StartNew();
        AddSource(context, new Scaffolder(raw).Result);
        sw.Elapsed.ToConciseString().Debug(x => (raw.Named.GetFullyQualifiedMetadataName(), x));
    }
#else
        =>
            AddSource(context, new Scaffolder(raw).Result);
#endif
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
    static bool HasSufficientMembers(Raw x) => x is ({ IsStatic: false }, { Length: >= MinimumMembers }, _, _);

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
                    Left: IdentifierNameSyntax { Identifier.Text: Choice } or
                    QualifiedNameSyntax
                    {
                        Left: IdentifierNameSyntax { Identifier.Text: nameof(Emik) },
                        Right: IdentifierNameSyntax { Identifier.Text: Choice },
                    } or
                    QualifiedNameSyntax
                    {
                        Left: IdentifierNameSyntax { Identifier.Text: Choice } or QualifiedNameSyntax,
                        Right: IdentifierNameSyntax or GenericNameSyntax,
                    },
                    Right: IdentifierNameSyntax or GenericNameSyntax,
                },
                Right: IdentifierNameSyntax or GenericNameSyntax,
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

        var fields = ImmutableArray.CreateBuilder<MemberSymbol>();
        bool? mutablePublicly = null;
        var e = false;

        for (var n = attribute.Name; n is QualifiedNameSyntax q; n = q.Left)
        {
            token.ThrowIfCancellationRequested();

            if (q.Right is IdentifierNameSyntax { Identifier.Text: var right })
                switch (right)
                {
                    case Choice when q.Left is IdentifierNameSyntax { Identifier.Text: nameof(Emik) }:
                        e = true;
                        goto Done;
                    case nameof(Accessibility.Private) or nameof(Accessibility.Public) when
                        q.Left is IdentifierNameSyntax { Identifier.Text: Choice }:
                        mutablePublicly = right is nameof(Accessibility.Public);
                        goto Done;
                }

            if (MemberSymbol.From(q, context.SemanticModel, token) is not { } member)
                return default;

            fields.Add(member);
        }

    Done:
        fields.Reverse();
        return (named, fields.DrainToImmutable(), mutablePublicly, e);
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
           .FirstOrDefault(x => x.Ancestors().FirstOrDefault() is BaseTypeDeclarationSyntax)
          ?.Parameters
           .Select(x => context.SemanticModel.GetDeclaredSymbolSafe(x, token))
           .Filter()
           .Select(x => new MemberSymbol(x))
           .ToImmutableArray();

        var fields = symbolSet switch
        {
            { IsTupleType: true, TupleElements: { Length: >= MinimumMembers } e } => Scaffolder.Decouple(e),
            _ when MemberSymbol.IsSystemTuple(symbolSet) => Scaffolder.Instances(symbolSet),
            null when primaryConstructorParameters is { } p => p,
            null => Scaffolder.Instances(target),
            _ => [],
        };

        return (target, fields, mutablePublicly, null);
    }
}
