// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;
#pragma warning disable MA0051
sealed partial record Scaffolder
{
    [Pure]
    string DeclareForwarders =>
        Signature.FindForwarders(Symbols, Named, Members).Select(DeclareForwarder).Conjoin("");

    [Pure]
    static string PrefixAnnotations(IParameterSymbol x) =>
        $"{(x.IsParams ? CSharp("params ") : "")}{(
            x.ScopedKind is ScopedKind.None ? "" : CSharp("scoped "))}{x.RefKind.KeywordInParameter()}";

    [Pure]
    static string SuffixAnnotations(IParameterSymbol x) =>
        x switch
        {
            { HasExplicitDefaultValue: false } => "",
            { ExplicitDefaultValue: { } value } => CSharp($" = {value}"),
            _ => CSharp(" = default"),
        };

    [Pure]
    bool IsIgnored(ISymbol symbol) =>
        Named.IsRefLikeType && symbol is IEventSymbol ||
        symbol is IEventSymbol { AddMethod: null, RemoveMethod: null } or
            IPropertySymbol { IsReadOnly: true, IsWriteOnly: true };

    [Pure] // ReSharper disable once CognitiveComplexity
    string DeclareForwarder(Extract extract)
    {
        var (symbol, kind, interfacesDeclared) = extract;

        if (IsIgnored(symbol))
            return "";

        var attributes = Attributes(symbol, '\n');

        StringBuilder builder = new(
            CSharp(
                $"""


                     /// {XmlTypeName(symbol, "inheritdoc")}{Remarks(interfacesDeclared)}
                     {Annotation}{(symbol is IMethodSymbol ? $"\n    {AggressiveInlining}" : "")}
                     {attributes}{(attributes is "" ? "" : "    ")}public {SymbolsUnsafe}
                 """
            )
        );

        string FullyQualified(IParameterSymbol x) =>
            $"{Attributes(x, ' ')}{PrefixAnnotations(x)
            }{x.Type.GetFullyQualifiedNameWithNullabilityAnnotations()} {x.Name
            }{SuffixAnnotations(x)}";

        StringBuilder AppendParameterSymbols(char begin, ImmutableArray<IParameterSymbol> all, char end, bool typed) =>
            builder.Append(begin).AppendMany(all.Select(typed ? FullyQualified : NameGetter())).Append(end);

        StringBuilder AppendParameters(StringBuilder builder) => AppendParametersTyped(builder, false);

        StringBuilder AppendParametersTyped(StringBuilder builder, bool typed = true) =>
            symbol switch
            {
                IMethodSymbol { Name: nameof(GetType), Parameters: [] } => builder,
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
                var isGetType = symbol is IMethodSymbol { Name: nameof(GetType), Parameters: [] };

                var value = isTypeKnown && isGetType
                    ? CSharp($"typeof({x.Type})")
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
            return builder.Append(isSwitchCase ? CSharp("        }") : CSharp("        };"));
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

        builder.Append(
            symbol switch
            {
                IMethodSymbol { Name: nameof(GetType), Parameters: [] } => $"{nameof(Enum.GetUnderlyingType)}()",
                IPropertySymbol { Parameters: not [] } => CSharp("this"),
                _ => symbol.GetFullyQualifiedName(),
            }
        );

        AppendParametersTyped(builder);

        switch (symbol)
        {
            case IMethodSymbol { ReturnType.SpecialType: SpecialType.System_Void }:
                return $"{AppendSwitchExpression(builder, "\n    {", isSwitchCase: true).Append("\n    }")}";
            case IMethodSymbol: return $"{AppendSwitchExpression(builder.Append("\n        "))}";
        }

        builder.Append("\n    {\n        ");
        var hasGetter = symbol is IFieldSymbol or IPropertySymbol { IsWriteOnly: false };
        var hasSetter = symbol is IFieldSymbol { IsReadOnly: false } or IPropertySymbol { IsReadOnly: false };

        if (hasGetter)
            AppendSwitchExpression(builder, Named.IsValueType && hasSetter ? "readonly get " : "get ");

        if (hasSetter)
        {
            if (hasGetter)
                builder.Append("\n        ");

            AppendSwitchExpression(builder, "set ", " = value");
        }

        var hasAdder = symbol is IEventSymbol { AddMethod: not null };

        if (hasAdder)
            AppendSwitchExpression(builder, "add ", " += value");

        if (symbol is not IEventSymbol { RemoveMethod: not null })
            return $"{builder.Append("\n    }")}";

        if (hasAdder)
            builder.Append("\n        ");

        AppendSwitchExpression(builder, "remove ", " -= value");
        return $"{builder.Append("\n    }")}";
    }

    [Pure]
    static bool IsGoodAttribute(AttributeData x) =>
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
                Name: nameof(MethodImplAttribute) or "NullableAttribute" or "NullableContextAttribute",
            };

    [Pure]
    static string Display(AttributeData x) => $"[{x.AttributeClass}{DisplayArguments(x)}]";

    [Pure]
    static string Display(KeyValuePair<string, TypedConstant> x) => $"{x.Key} = {Display(x.Value)}";

    [Pure]
    static string Display(TypedConstant x) =>
        x.Kind switch
        {
            _ when x.IsNull => "null",
            _ when x.Type?.SpecialType is SpecialType.System_Boolean ||
                x.Type is INamedTypeSymbol
                {
                    TypeArguments: [{ SpecialType: SpecialType.System_Boolean }],
                    SpecialType: SpecialType.System_Nullable_T,
                } => x.Value switch { true => "true", false => "false", _ => throw Unreachable },
            _ when x.Type?.SpecialType is SpecialType.System_String =>
                $"\"{x.Value?.ToString().SelectMany(Escape).Concat()}\"",
            TypedConstantKind.Error or TypedConstantKind.Primitive => $"{x.Value}",
            TypedConstantKind.Enum => $"({x.Type}){x.Value}",
            TypedConstantKind.Type => $"typeof({x.Type})",
            TypedConstantKind.Array => $"new{ObjectSuffix(x)}[] {{ {x.Values.Select(Display).Conjoin()} }}",
            _ => throw Unreachable,
        };

    [Pure]
    static string DisplayArguments(AttributeData x) =>
        x.ConstructorArguments.IsEmpty && x.NamedArguments.IsEmpty
            ? ""
            : $"({x.ConstructorArguments.Select(Display).Concat(x.NamedArguments.Select(Display)).Conjoin()})";

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
        };

    [Pure]
    static string ObjectSuffix(TypedConstant x) =>
        x.Type.ToUnderlying()?.SpecialType is SpecialType.System_Object ? " object" : "";

    [Pure]
    string Attributes(ISymbol symbol, char separator) =>
        symbol
           .GetAttributes()
           .Where(x => IsGoodAttribute(x) && x.AttributeConstructor.CanBeAccessedFrom(Named.ContainingAssembly))
           .Select(x => $"{Display(x)}{separator}")
           .Conjoin("");

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
    static Func<IParameterSymbol, string> NameGetter() => x => $"{x.RefKind.KeywordInParameter()}{x.Name}";
}
