// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;
#pragma warning disable CA1502, IDE0039, MA0051
sealed partial record Scaffolder
{
    [Pure]
    public string DeclareForwarders =>
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
        var name = (IParameterSymbol x) => $"{x.RefKind.KeywordInParameter()}{x.Name}";
        var (symbol, kind, interfacesDeclared) = extract;

        if (IsIgnored(symbol))
            return "";

        StringBuilder builder = new(
            CSharp(
                $"""


                     /// {XmlTypeName(symbol, "inheritdoc")}{Remarks(ref interfacesDeclared)}
                     {Annotation}{(symbol is IMethodSymbol ? $"\n    {AggressiveInlining}" : "")}
                     public{' '}
                 """
            )
        );

        static string FullyQualified(IParameterSymbol x) =>
            $"{PrefixAnnotations(x)}{x.Type.GetFullyQualifiedNameWithNullabilityAnnotations()} {x.Name}{SuffixAnnotations(x)}";

        StringBuilder AppendParameterSymbols(char begin, ImmutableArray<IParameterSymbol> all, char end, bool typed) =>
            builder.Append(begin).AppendMany(all.Select(typed ? FullyQualified : name)).Append(end);

        StringBuilder AppendParameters(StringBuilder builder) => AppendParametersTyped(builder, false);

        StringBuilder AppendParametersTyped(StringBuilder builder, bool typed = true) =>
            symbol switch
            {
                IMethodSymbol { Name: nameof(GetType), Parameters: [] } => builder.Append("()"),
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
                builder
                   .Append("    ")
                   .Append(x)
                   .Then(AppendParameters)
                   .Append(suffix)
                   .AppendLine(isSwitchCase ? CSharp(";\n                break;") : ",");

            var member = symbol is IPropertySymbol { Parameters: not [] } ? "" : $".{symbol.GetFullyQualifiedName()}";

            string Case(FieldOrProperty x, int i)
            {
                var instance = IsEmpty(x) ? CSharp($"default({x.Type})") : Prefix(x);

                var cast = interfacesDeclared[i] is { } to ? $"(({to}){instance})" :
                    instance is ReferenceField ? $"(({x.Type}){ReferenceField})" :
                    instance;

                if (isSwitchCase)
                    return CSharp(
                        $"""
                                 case {i}:
                                         {cast}{NullableSuppression(x)}{member}
                         """
                    );

                var value = (x.Type.IsValueType || x.Type.IsSealed) &&
                    symbol is IMethodSymbol { Name: nameof(GetType), Parameters: [] }
                        ? CSharp($"typeof({x.Type})")
                        : $"{kind.KeywordInReturn()}{cast}{NullableSuppression(x)}{member}";

                return CSharp($"        {i} => {value}");
            }

            var discard = suffix is "" ? "" : "_ =";

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

            return builder.Append(
                isSwitchCase
                    ? CSharp("            default: throw new global::System.InvalidOperationException();\n        }")
                    : CSharp("            _ => throw new global::System.InvalidOperationException(),\n        };")
            );
        }

        if (Named is { IsValueType: true } &&
            symbol is IFieldSymbol { IsReadOnly: true } or
                IMethodSymbol { IsReadOnly: true } or
                IPropertySymbol { IsReadOnly: true })
            builder.Append(CSharp("readonly "));

        if (symbol is IEventSymbol)
            builder.Append(CSharp("event "));

        builder.Append(kind.KeywordInReturn());
        builder.Append(symbol.ToUnderlying());
        builder.Append(' ');

        builder.Append(
            symbol switch
            {
                IMethodSymbol { Name: nameof(GetType), Parameters: [] } => "GetUnderlyingType()",
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

        var hasSetter = MutablePublicly is not null &&
            symbol is IFieldSymbol { IsReadOnly: false } or IPropertySymbol { IsReadOnly: false };

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
    string Remarks(ref SmallList<string?> interfaces) =>
        interfaces
           .Select(
                (x, i) => x is null || Symbols[i].Type.IsReferenceType
                    ? ((string, string)?)null
                    : (See(Symbols[i]), x.Replace('<', '{').Replace('>', '}'))
            )
           .Filter()
           .Select(x => $"""{x.Item1} as <see cref="{x.Item2}"/>""")
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
}
