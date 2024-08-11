// SPDX-License-Identifier: MPL-2.0

global using Extract = (Microsoft.CodeAnalysis.ISymbol Symbol,
    Microsoft.CodeAnalysis.RefKind Kind,
    Emik.Morsels.SmallList<string?> InterfaceDeclarations);
global using Fold = (Microsoft.CodeAnalysis.INamedTypeSymbol Named,
    Microsoft.CodeAnalysis.INamedTypeSymbol? SymbolSet,
    bool? MutablePublicly);
global using Raw = (Microsoft.CodeAnalysis.INamedTypeSymbol Named,
    Emik.Morsels.SmallList<Emik.SourceGenerators.Choices.MemberSymbol> Fields,
    bool? MutablePublicly,
    bool PolyfillAttributes);
