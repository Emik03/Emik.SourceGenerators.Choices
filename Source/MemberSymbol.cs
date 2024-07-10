// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

readonly record struct MemberSymbol(ITypeSymbol Type, string Name, ISymbol? Symbol = null)
{
    public MemberSymbol(IFieldSymbol field)
        : this(field.Type, field.Name, field) { }

    public MemberSymbol(IPropertySymbol field)
        : this(field.Type, field.Name, field) { }

    public bool IsStatic => Symbol is { IsStatic: true };

    public static MemberSymbol? DeconstructFrom(ISymbol symbol) =>
        symbol switch
        {
            IFieldSymbol x => new(x),
            IPropertySymbol x => new(x),
            _ => null,
        };

    public bool Equals(MemberSymbol other) =>
        Symbol switch
        {
            IFieldSymbol field => FieldSymbolComparer.Equal(field, other.Symbol as IFieldSymbol),
            IPropertySymbol property => PropertySymbolComparer.Equal(property, other.Symbol as IPropertySymbol),
            _ => TypeSymbolComparer.Equal(Type, other.Type) && Name == other.Name,
        };

    public override int GetHashCode() =>
        Symbol switch
        {
            IFieldSymbol field => FieldSymbolComparer.Default.GetHashCode(field) * 3,
            IPropertySymbol property => PropertySymbolComparer.Default.GetHashCode(property) * 2,
            _ => TypeSymbolComparer.GetHashCode(Type) ^ StringComparer.Ordinal.GetHashCode(Name),
        };
}
