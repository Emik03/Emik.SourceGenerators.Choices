// SPDX-License-Identifier: MPL-2.0
global using Extract = (Microsoft.CodeAnalysis.ISymbol Symbol,
    Microsoft.CodeAnalysis.RefKind Kind,
    System.Collections.Immutable.ImmutableArray<string?> InterfaceDeclarations);
global using ForwarderAggregate = System.Collections.Generic.List<(Microsoft.CodeAnalysis.ISymbol Symbol, string Code)>;
global using Raw = (Microsoft.CodeAnalysis.INamedTypeSymbol Named,
    System.Collections.Immutable.ImmutableArray<Emik.SourceGenerators.Choices.MemberSymbol> Fields,
    bool? MutablePublicly,
    bool? FullyPolyfillAttributes);
global using static System.Runtime.InteropServices.ImmutableCollectionsMarshal;
