// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

/// <summary>Represents a named member, typically a field or property.</summary>
/// <param name="Type">The type of the member.</param>
/// <param name="Name">The name of the member.</param>
/// <param name="Symbol">The underlying symbol, if any.</param>
public readonly record struct MemberSymbol(ITypeSymbol Type, string Name, ISymbol? Symbol = null)
{
    /// <summary>Initializes a new instance of the <see cref="MemberSymbol"/> struct.</summary>
    /// <param name="field">The field.</param>
    public MemberSymbol(IFieldSymbol field)
        : this(field.Type, field.Name, field) { }

    /// <summary>Initializes a new instance of the <see cref="MemberSymbol"/> struct.</summary>
    /// <param name="property">The property.</param>
    public MemberSymbol(IPropertySymbol property)
        : this(property.Type, property.Name, property) { }

    /// <summary>Gets a value indicating whether the member is static.</summary>
    public bool IsStatic => Symbol is { IsStatic: true };

    /// <summary>Creates a new instance of the <see cref="MemberSymbol"/> struct from the underlying symbol.</summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to create the <see cref="MemberSymbol"/> from.</param>
    /// <returns>The new <see cref="MemberSymbol"/> instance.</returns>
    public static MemberSymbol? DeconstructFrom(ISymbol symbol) =>
        symbol switch
        {
            IFieldSymbol x => new(x),
            IPropertySymbol x => new(x),
            _ => null,
        };

    /// <inheritdoc />
    public bool Equals(MemberSymbol other) =>
        Symbol switch
        {
            IFieldSymbol field => FieldSymbolComparer.Equal(field, other.Symbol as IFieldSymbol),
            IPropertySymbol property => PropertySymbolComparer.Equal(property, other.Symbol as IPropertySymbol),
            _ => TypeSymbolComparer.Equal(Type, other.Type) && Name == other.Name,
        };

    /// <inheritdoc />
    public override int GetHashCode() =>
        Symbol switch
        {
            IFieldSymbol field => FieldSymbolComparer.Default.GetHashCode(field) * 3,
            IPropertySymbol property => PropertySymbolComparer.Default.GetHashCode(property) * 2,
            _ => TypeSymbolComparer.GetHashCode(Type) ^ StringComparer.Ordinal.GetHashCode(Name),
        };

    public override string ToString() =>
        Symbol switch
        {
            IFieldSymbol => $"{Type} {Name};",
            IPropertySymbol => $"{Type} {Name} {{ get; }}",
            _ => $"{Name}<{Type}>",
        };
}
