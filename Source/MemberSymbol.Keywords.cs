// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <inheritdoc />
public readonly partial record struct MemberSymbol
{
    /// <summary>Gets all reserved keywords.</summary>
    static readonly HashSet<string> s_keywords = new(
        [
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while",
        ],
        StringComparer.Ordinal
    );

    /// <summary>Determines whether the provided <see cref="string"/> is a reserved keyword.</summary>
    /// <param name="keyword">The <see cref="string"/> to check.</param>
    /// <returns>
    /// The value <see langword="true"/> if the parameter <paramref name="keyword"/>
    /// is a reserved keyword; otherwise, <see keyword="false"/>.
    /// </returns>
    public static bool IsKeyword(string keyword) => s_keywords.Contains(keyword);
}
