// SPDX-License-Identifier: MPL-2.0

// Code style insists that this be the format. Not my fault.
global using Extract =
    (Microsoft.CodeAnalysis.ISymbol Symbol, Microsoft.CodeAnalysis.RefKind Kind, Emik.Morsels.SmallList<string?>
    InterfaceDeclarations);
global using Fold =
    (Microsoft.CodeAnalysis.INamedTypeSymbol Named, Microsoft.CodeAnalysis.INamedTypeSymbol? SymbolSet, bool?
    PubliclyMutable);
global using Raw =
    (Microsoft.CodeAnalysis.INamedTypeSymbol Named, Emik.Morsels.SmallList<Gu.Roslyn.AnalyzerExtensions.FieldOrProperty>
    Fields, bool? PubliclyMutable);

[assembly: NullGuard(ValidationFlags.None)]
