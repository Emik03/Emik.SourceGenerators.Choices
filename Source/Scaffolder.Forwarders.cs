// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;
#pragma warning disable MA0051
sealed partial record Scaffolder
{
    [Pure]
    string DeclareForwarders =>
        Symbols.All(x => x.IsEmpty)
            ? ""
            : Signature.FindForwarders(Symbols, Named, Members)
               .Aggregate(new ForwarderAggregate(), DeclareForwarder)
               .Select(x => x.Code)
               .Conjoin("");

    [Pure]
    static string PrefixAnnotations(IParameterSymbol x) =>
        $"{(x.IsParams ? CSharp("params ") : "")}{(
            x.ScopedKind is ScopedKind.None ? "" : CSharp("scoped "))}{x.RefKind.KeywordInParameter()}";

    [Pure]
    static string SuffixAnnotations(IParameterSymbol x) =>
        x switch
        {
            { HasExplicitDefaultValue: false } => "",
            { ExplicitDefaultValue: true } => CSharp(" = true"),
            { ExplicitDefaultValue: false } => CSharp(" = false"),
            { ExplicitDefaultValue: string value } => CSharp($" = \"{value.SelectMany(Escape).Concat()}\""),
            _ => CSharp(" = default"),
        };

    [Pure]
    bool IsIgnored(ISymbol symbol) =>
        Named.IsRefLikeType && symbol is IEventSymbol ||
        symbol is IEventSymbol { AddMethod: null, RemoveMethod: null } or
            IPropertySymbol { IsReadOnly: true, IsWriteOnly: true } ||
        Named.IsRecord && symbol.Name is "Clone";

    [Pure] // ReSharper disable once CognitiveComplexity
    ForwarderAggregate DeclareForwarder(ForwarderAggregate list, Extract extract)
    {
        var (symbol, kind, interfacesDeclared) = extract;

        bool SameSignature((ISymbol Symbol, string) x) =>
            x.Symbol.Kind == symbol.Kind &&
            (x.Symbol as IMethodSymbol)?.TypeParameters.Length ==
            (symbol as IMethodSymbol)?.TypeParameters.Length &&
            x.Symbol.GetFullyQualifiedName() == symbol.GetFullyQualifiedName() &&
            ((x.Symbol as IMethodSymbol)?.Parameters ??
                (x.Symbol as IPropertySymbol)?.Parameters ?? [])
           .Select(x => x.Type)
           .SequenceEqual(
                ((symbol as IMethodSymbol)?.Parameters ??
                    (symbol as IPropertySymbol)?.Parameters ?? [])
               .Select(x => x.Type),
                Equating<ITypeSymbol>((x, y) => x.GetFullyQualifiedName() == y.GetFullyQualifiedName())
            );

        bool SameUnderlying((ISymbol Symbol, string) x) =>
            (x.Symbol is not IEventSymbol a ||
                symbol is not IEventSymbol b ||
                a.AddMethod is null == b.AddMethod is null &&
                a.RemoveMethod is null == b.RemoveMethod is null) &&
            (x.Symbol is not IPropertySymbol c ||
                symbol is not IPropertySymbol d ||
                c.GetMethod is null == d.GetMethod is null && c.SetMethod is null == d.SetMethod is null) &&
            RoslynComparer.Gu.Equals(x.Symbol.ToUnderlying(), symbol.ToUnderlying());

        if (IsIgnored(symbol))
            return list;

        var explicitDeclaration = list.Where(SameSignature).ToImmutableArray();

        if (explicitDeclaration.Any() && interfacesDeclared.All(x => x is null) ||
            explicitDeclaration.Any(x => interfacesDeclared.IsEmpty || SameUnderlying(x)))
            return list;

        var indexerName = symbol is IPropertySymbol { IsIndexer: true } &&
            !symbol.GetAttributes().Any(IsIndexerNameAttribute) &&
            FindIndexerName(list) is { } potential
                ? CSharp($"\n    [global::System.Runtime.CompilerServices.IndexerNameAttribute(\"{potential}\")]")
                : null;

        var attributes = Attributes(symbol, '\n');

        StringBuilder builder = new(
            CSharp(
                $"""


                     /// {XmlTypeName(symbol, "inheritdoc")}{Remarks(interfacesDeclared)}
                     {Annotation}{(symbol is IMethodSymbol ? $"\n    {AggressiveInlining}" : "")}{indexerName}
                     {attributes}{(attributes is "" ? "" : "    ")}{(explicitDeclaration.Any() && interfacesDeclared.Any() ? "" : "public ")
                     }{SymbolsUnsafe}
                 """
            )
        );

        string FullyQualified(IParameterSymbol x) =>
            $"{Attributes(x, ' ')}{PrefixAnnotations(x)
            }{x.Type.GetFullyQualifiedNameWithNullabilityAnnotations()} {x.GetFullyQualifiedName()
            }{SuffixAnnotations(x)}";

        StringBuilder AppendParameterSymbols(char begin, ImmutableArray<IParameterSymbol> all, char end, bool typed) =>
            builder.Append(begin).AppendMany(all.Select(typed ? FullyQualified : NameGetter())).Append(end);

        StringBuilder AppendParameters(StringBuilder builder) => AppendParametersTyped(builder, false);

        StringBuilder AppendParametersTyped(StringBuilder builder, bool typed = true) =>
            symbol switch
            {
                _ when Signature.IsGetType(symbol) => builder,
                IMethodSymbol { Parameters: var x } => AppendParameterSymbols('(', x, ')', typed),
                IPropertySymbol { Parameters: [_, ..] x } => AppendParameterSymbols('[', x, ']', typed),
                _ => builder,
            };

        StringBuilder AppendSwitchExpression(
            StringBuilder builder,
            [StringSyntax("C#")] string prefix = "",
            [StringSyntax("C#")] string suffix = "",
            bool isSwitchCase = false
        )
        {
            void AppendCase(string x) =>
                AppendParameters(builder.Append("    ").Append(x))
                   .Append(suffix)
                   .AppendLine(isSwitchCase ? CSharp(";\n                break;") : ",");

            var member = symbol is IPropertySymbol { Parameters: not [] } ? "" : $".{symbol.GetFullyQualifiedName()}";

            string Case(MemberSymbol x, int i)
            {
                var instance = x.IsEmpty ? CSharp($"default({x.Type})") : Prefix(x);

                var cast = interfacesDeclared[i] is { } to ? $"(({to}){instance}{x.NullableSuppression})" :
                    instance is ReferenceField ? $"(({x.Type}){ReferenceField}{x.NullableSuppression})" :
                    $"{instance}{x.NullableSuppression}";

                if (isSwitchCase)
                    return CSharp(
                        $"""
                                 {(i == Symbols.Length - 1 ? "default" : $"case {i}")}:
                                         {cast}{member}
                         """
                    );

                var isTypeKnown = x.Type.IsValueType || x.Type.IsSealed;
                var isGetType = Signature.IsGetType(symbol);

                var value = isTypeKnown && isGetType
                    ? CSharp($"typeof({x.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated)})")
                    : $"{kind.KeywordInReturn()}{cast}{member}";

                return CSharp(
                    $"        {(i == Symbols.Length - 1 ? "_" : i)} => {value}{(!isTypeKnown && isGetType ? "()" : "")}"
                );
            }

            var discard = suffix is "" ? "" : "_ = ";

            var arrow = isSwitchCase
                ? CSharp(
                    $$"""

                              switch ({{Discriminator}})
                              {

                      """
                )
                : CSharp(
                    $$"""
                      => {{discard}}{{Discriminator}} switch
                              {

                      """
                );

            if (symbol is not IMethodSymbol)
                builder.AppendLine(AggressiveInlining).Append(' ', 8);

            builder.Append(prefix).Append(arrow);
            Symbols.Select(Case).Lazily(AppendCase).Enumerate();
            return builder.Append(' ', 8).Append(isSwitchCase ? "}" : "};");
        }

        if (Named is { IsValueType: true } &&
            symbol is IFieldSymbol { IsReadOnly: true } or
                IMethodSymbol { IsReadOnly: true } or
                IPropertySymbol { IsReadOnly: true })
            builder.Append(CSharp("readonly "));

        switch (symbol)
        {
            case IFieldSymbol { Type: var fieldType } when fieldType.IsUnsafe():
            case IMethodSymbol { ReturnType: var returnType, Parameters: var methodParameters } when
                returnType.IsUnsafe() || methodParameters.Any(x => x.Type.IsUnsafe()):
            case IPropertySymbol { Type: var propertyType, Parameters: var propertyParameters } when
                propertyType.IsUnsafe() || propertyParameters.Any(x => x.Type.IsUnsafe()):
                builder.Append(CSharp("unsafe "));
                break;
            case IEventSymbol:
                builder.Append(CSharp("event "));
                break;
        }

        builder.Append(kind.KeywordInReturn());
        builder.Append(symbol.ToUnderlying());
        builder.Append(' ');

        if (explicitDeclaration.Any() && interfacesDeclared is [var first, ..])
            builder.Append(first).Append('.');

        builder.Append(
            symbol switch
            {
                _ when Signature.IsGetType(symbol) => $"{nameof(Enum.GetUnderlyingType)}()",
                IPropertySymbol { Parameters: not [] } => CSharp("this"),
                _ => symbol.GetFullyQualifiedName(),
            }
        );

        AppendParametersTyped(builder);

        if (symbol is IMethodSymbol { TypeParameters: var typeParameters } &&
            typeParameters.Select(IncludedSyntaxNodeRegistrant.Constraints).Filter().ToIList() is [_, ..] constraints)
            builder.AppendLine().Append(' ', 8).Append(constraints.Conjoin("\n        "));

        switch (symbol)
        {
            case IMethodSymbol { ReturnType.SpecialType: SpecialType.System_Void }:
                return list.AndAdd(
                    (symbol, $"{AppendSwitchExpression(builder, "\n    {", isSwitchCase: true).Append("\n    }")}")
                );
            case IMethodSymbol: return list.AndAdd((symbol, $"{AppendSwitchExpression(builder.Append("\n        "))}"));
        }

        builder.Append("\n    {\n        ");
        var isInterfaceImplementation = explicitDeclaration.Any() || interfacesDeclared.All(x => x is not null);

        var hasGetter = symbol is IFieldSymbol or IPropertySymbol { IsWriteOnly: false } &&
            (isInterfaceImplementation ||
                (symbol is IPropertySymbol { GetMethod: { } g } ? g : symbol)
               .CanBeAccessedFrom(Named.ContainingAssembly));

        var hasSetter = CanForwardSetters &&
            HasSetter(symbol) &&
            (isInterfaceImplementation ||
                (symbol is IPropertySymbol { SetMethod: { } s } ? s : symbol)
               .CanBeAccessedFrom(Named.ContainingAssembly)) &&
            Symbols.Select(x => x.Type.TryFindFirstMember(Finder(symbol), out var s) ? s : null).All(HasSetter);

        var hasAdder = symbol is IEventSymbol { AddMethod: { } a } &&
            (isInterfaceImplementation || a.CanBeAccessedFrom(Named.ContainingAssembly));

        var hasRemover = symbol is IEventSymbol { RemoveMethod: { } r } &&
            (isInterfaceImplementation || r.CanBeAccessedFrom(Named.ContainingAssembly));

        if (hasGetter)
            AppendSwitchExpression(builder, Named.IsValueType && hasSetter ? "readonly get " : "get ");

        if (hasGetter && hasSetter)
            builder.Append("\n        ");

        if (hasSetter)
            AppendSwitchExpression(builder, "set ", " = value");

        if (hasAdder)
            AppendSwitchExpression(builder, "add\n        {", " += value", true).Append("\n        }");

        if (hasAdder && hasRemover)
            builder.Append("\n        ");

        if (hasRemover)
            AppendSwitchExpression(builder, "remove\n        {", " -= value", true).Append("\n        }");

        return list.AndAdd((symbol, $"{builder.Append("\n    }")}"));
    }

    [Pure]
    static bool HasSetter([NotNullWhen(false)] ISymbol? symbol) =>
        symbol is null or
            IFieldSymbol { IsReadOnly: false } or
            IPropertySymbol { IsReadOnly: false, SetMethod: not { IsInitOnly: true } };

    [Pure]
    static bool IsConstantNegative(object? o) => o is sbyte and < 0 or short and < 0 or < 0 or long and < 0;

    [Pure]
    static bool IsGoodClass(AttributeData x) =>
        x.AttributeClass is not
            {
                ContainingNamespace:
                {
                    ContainingNamespace:
                    {
                        ContainingNamespace:
                        { ContainingNamespace.IsGlobalNamespace: true, Name: nameof(System) },
                        Name: nameof(System.CodeDom),
                    },
                    Name: nameof(System.CodeDom.Compiler),
                },
                Name: nameof(GeneratedCodeAttribute),
            } and
            not
            {
                ContainingNamespace:
                {
                    ContainingNamespace:
                    {
                        ContainingNamespace:
                        { ContainingNamespace.IsGlobalNamespace: true, Name: nameof(System) },
                        Name: nameof(System.Runtime),
                    },
                    Name: nameof(System.Runtime.CompilerServices),
                },
                Name: nameof(MethodImplAttribute) or
                nameof(TupleElementNamesAttribute) or
                "NullableAttribute" or
                "NullableContextAttribute",
            };

    [Pure]
    static bool IsIndexerNameAttribute(AttributeData arg) =>
        arg.AttributeClass is
        {
            ContainingNamespace:
            {
                ContainingNamespace:
                {
                    ContainingNamespace:
                    {
                        ContainingNamespace.IsGlobalNamespace: true,
                        Name: nameof(System),
                    },
                    Name: nameof(System.Runtime),
                },
                Name: nameof(System.Runtime.CompilerServices),
            },
            Name: nameof(IndexerNameAttribute),
        };

    [Pure]
    static bool IsUnscopedRefAttribute(ISymbol x) =>
        x is
        {
            ContainingNamespace:
            {
                ContainingNamespace:
                {
                    ContainingNamespace:
                    {
                        ContainingNamespace.IsGlobalNamespace: true,
                        Name: nameof(System),
                    },
                    Name: nameof(System.Diagnostics),
                },
                Name: nameof(System.Diagnostics.CodeAnalysis),
            },
            Name: nameof(UnscopedRefAttribute),
        };

    [Pure]
    static string Display(KeyValuePair<string, TypedConstant> x) => $"{x.Key} = {Display(x.Value)}";

    [Pure]
    static string Display(TypedConstant x) =>
        x.Kind switch
        {
            _ when x.IsNull => CSharp("null"),
            _ when x.Type?.SpecialType is SpecialType.System_Boolean ||
                x.Type is INamedTypeSymbol
                {
                    TypeArguments: [{ SpecialType: SpecialType.System_Boolean }],
                    SpecialType: SpecialType.System_Nullable_T,
                } => x.Value switch { true => CSharp("true"), false => CSharp("false"), _ => throw Unreachable },
            _ when x.Type?.SpecialType is SpecialType.System_String =>
                $"\"{x.Value?.ToString().SelectMany(Escape).Concat()}\"",
            TypedConstantKind.Error or TypedConstantKind.Primitive => $"{x.Value}",
            TypedConstantKind.Enum when IsConstantNegative(x.Value) is var isNegative =>
                $"({x.Type}){(isNegative ? "(" : "")}{x.Value}{(isNegative ? ")" : "")}",
            TypedConstantKind.Type => CSharp($"typeof({x.Type})"),
            TypedConstantKind.Array => $"new{ObjectSuffix(x)}[] {{ {x.Values.Select(Display).Conjoin()} }}",
            _ => throw Unreachable,
        };

    [Pure]
    static string Escape(char x) =>
        x switch
        {
            '\0' => @"\0",
            '\a' => @"\a",
            '\b' => @"\b",
            // Disabled for compatibility.
            // '\e' => @"\e",
            '\f' => @"\f",
            '\n' => @"\n",
            '\r' => @"\r",
            '\t' => @"\t",
            '\v' => @"\v",
            '\\' => @"\\",
            '\"' => @"\""",
            _ when char.IsControl(x) => @$"\u{(int)x:x4}",
            _ => $"{x}",
        }; // ReSharper disable once ParameterTypeCanBeEnumerable.Local SuggestBaseTypeForParameter

    static string? FindIndexerName(ForwarderAggregate list)
    {
        static bool HasConflict((ISymbol Symbol, string) x, string reserved) =>
            x.Symbol.Name == reserved &&
            (x.Symbol is not IPropertySymbol ||
                x.Symbol.GetAttributes()
                   .Where(IsIndexerNameAttribute)
                   .Any(x => x.ConstructorArguments.Any(x => x.Value as string == reserved)));

        var i = 1;

        for (; $"Item{(i is 1 ? "" : i)}" is var ret && list.Any(x => HasConflict(x, ret)); i++) { }

        return i is 1 ? null : $"Item{i}";
    }

    [Pure]
    static Func<IParameterSymbol, string> NameGetter() =>
        x => $"{x.RefKind.KeywordInParameter()}{x.GetFullyQualifiedName()}";

    [Pure]
    static Func<ISymbol, bool> Finder(ISymbol y) =>
        x => RoslynComparer.Signature.Equals(x, y) &&
            x.ExplicitInterfaceSymbols().GuardedSequenceEqual(y.ExplicitInterfaceSymbols(), RoslynComparer.Signature);

    [Pure]
    static string ObjectSuffix(TypedConstant x) =>
        x.Type.ToUnderlying()?.SpecialType is SpecialType.System_Object ? " object" : "";

    [Pure]
    bool IsValidAttribute(AttributeData x) =>
        IsGoodClass(x) &&
        (Named.IsValueType || !IsUnscopedRefAttribute(x.AttributeClass)) &&
        x.AttributeConstructor?.CanBeAccessedFrom(Named.ContainingAssembly) is true;

    [Pure]
    string Attributes(ISymbol symbol, char separator) =>
        symbol
           .GetAttributes()
           .Where(IsValidAttribute)
           .Select(x => $"{Display(x)}{separator}")
           .Conjoin("");

    [Pure]
    string Display(AttributeData x) => $"[{x.AttributeClass}{DisplayArguments(x)}]";

    [Pure]
    string DisplayArguments(AttributeData x) =>
        x.ConstructorArguments.IsEmpty && x.NamedArguments.IsEmpty
            ? ""
            : $"({x.ConstructorArguments.Select(Display).Concat(x.NamedArguments.Where(CanSet(x)).Select(Display)).Conjoin()})";

    [Pure]
    string Remarks(ImmutableArray<string?> interfaces) =>
        interfaces
           .Select(XmlMemberTypeNames)
           .Filter()
           .Select(x => $"""{x.Member} as <see cref="{x.Type}"/>""")
           .ToSmallList() is [_, ..] boxes
            ? CSharp(
                $"""

                     /// <remarks>
                     /// Boxes when the current instance is
                     /// {boxes.Conjoin(",\n    /// ")}.
                     /// </remarks>
                 """
            )
            : "";

    [Pure]
    (string Member, string Type)? XmlMemberTypeNames(string? x, int i) =>
        x is null || Symbols[i].Type.IsReferenceType
            ? null
            : (Symbols[i].XmlName, x.Replace('<', '{').Replace('>', '}'));

    [Pure]
    Func<KeyValuePair<string, TypedConstant>, bool> CanSet(AttributeData x) =>
        y => x.AttributeClass?.GetMembers().FirstOrDefault(x => x.Name == y.Key) is { } s &&
            (s is not IPropertySymbol p || p.SetMethod.CanBeAccessedFrom(Named.ContainingAssembly));
}
