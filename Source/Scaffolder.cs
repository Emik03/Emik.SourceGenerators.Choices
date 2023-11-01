// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

sealed partial record Scaffolder(INamedTypeSymbol Named, SmallList<FieldOrProperty> Symbols, bool? MutablePublicly)
{
    const int MinimumBoxedSize = 2, MinimumExplicitStructSize = 2, TupleGenericLimit = 8;

    [StringSyntax("C#")]
    const string
        Action = "global::System.Action",
        AggressiveInlining = "[global::System.Runtime.CompilerServices.MethodImpl(256)]",
        DiscriminatorField = "_discriminator",
        DiscriminatorProperty = "Discriminator",
        Func = "global::System.Func",
        Pure = "[global::System.Diagnostics.Contracts.PureAttribute]",
        ReadOnly = "readonly ",
        ReferenceField = "_reference",
        ResultGeneric = "TMappingResult",
        Throw = "throw new global::System.InvalidOperationException()",
        UnmanagedField = "_unmanaged";

    string? _discriminator, _source;

    [Pure]
    public GeneratedSource Result => (HintName, Source);

    [Pure]
    bool CanOverlapReferenceMemorySpace => Reference.Omit(Members.Contains).Skip(MinimumBoxedSize - 1).Any();

    [Pure]
    bool CanOverlapUnmanagedMemorySpace => Unmanaged.Omit(Members.Contains).Skip(MinimumExplicitStructSize - 1).Any();

    [Pure]
    string AutoIfStruct =>
        Named.IsValueType && Named.GetAttributes().All(x => x.AttributeClass?.Name is not nameof(StructLayoutAttribute))
            ? CSharp(
                "[global::System.Runtime.InteropServices.StructLayoutAttribute(global::System.Runtime.InteropServices.LayoutKind.Auto)]\n"
            )
            : "";

    [Pure]
    string PrivatelyReadOnly { get; } = MutablePublicly is null ? ReadOnly : "";

    [Pure]
    string ReadOnlyIfStruct { get; } = Named.IsValueType ? ReadOnly : "";

    [Pure]
    string ReadOnlyIfImmutableStruct { get; } = Named.IsValueType && MutablePublicly is null ? ReadOnly : "";

    [Pure]
    string ReadOnlyIfMutableStruct { get; } = Named.IsValueType && MutablePublicly is not null ? ReadOnly : "";

    [Pure]
    string DeclareType =>
        CSharp(
            $$"""
              {{AutoIfStruct}}partial {{Named.Keyword()}} {{Named.Name
              }}{{(Named.TypeArguments is [] ? "" : $"<{Named.TypeArguments.Conjoin()}>")
              }}{{DeclareInterfaces}}
              {
              {{DeclareExplicitStruct
              }}{{Symbols.Select(DeclareDelegate).Conjoin("")
              }}{{DeclareDiscriminator}}

              {{DeclareUnmanagedFields
              }}{{DeclareReferencedFields
              }}{{Rest.Select(DeclareField).Conjoin("")
              }}{{Symbols.Select(DeclareConstructor).Conjoin("")
              }}{{Symbols.Select(DeclareCheck).Conjoin("")
              }}{{Symbols.Select(DeclareProperty).Conjoin("")
              }}{{Symbols.Select(DeclareOperators).Conjoin("")
              }}{{Symbols.Select(DeclareFactory).Conjoin("")
              }}{{DeclareInterfaceImplementations
              }}{{DeclareMappers
              }}{{DeclareForwarders}}
              }
              """
        );

    string DeclareInterfaces =>
        Named.IsRefLikeType
            ? ""
            : CSharp(
                $"""
                  :
                     global::System.IComparable,
                     global::System.IComparable<object>,
                     global::System.IComparable<{Name}>,
                 #if NET7_0_OR_GREATER
                     global::System.Numerics.IComparisonOperators<{Name}, {Name}, bool>,
                 #endif
                     global::System.IEquatable<object>,
                     global::System.IEquatable<{Name}>{DeclareAdditionalInterfaces}
                 """
            );

    [Pure]
    string DeclareAdditionalInterfaces =>
        new IntersectedInterfaces(Symbols, Named.IsReadOnly).Set.Select(x => $",\n    {x}").Conjoin("");

    [Pure]
    string DeclareDiscriminator =>
        Discriminator is DiscriminatorProperty
            ? CSharp(
                $$"""
                      {{Annotation}}
                      private {{ReadOnlyIfStruct}}byte {{Discriminator}}
                      {
                          {{Pure}}
                          {{AggressiveInlining}}
                          get => {{ReferenceField}} switch
                          {
                              {{Symbols
                                 .Select((x, i) => (x, i))
                                 .OrderByDescending(Inheritance)
                                 .Select(x => $"{x.x.Type} => {x.i},")
                                 .Conjoin("\n            ")}}
                              _ => {{Throw}},
                          };
                          {{Pure}}
                          {{AggressiveInlining}}
                          set { }
                      }
                  """
            )
            : CSharp(
                $"""
                     {Annotation}
                     private {PrivatelyReadOnly}byte {Discriminator};
                 """
            );

    [Pure]
    public string DeclareEqualityOperators =>
        Named.IsRecord
            ? ""
            : CSharp(
                $$"""
                      /// <summary>
                      /// Determines whether the left-hand side is equal to the right.
                      /// </summary>
                      /// <param name="left">The left-hand side.</param>
                      /// <param name="right">The right-hand side.</param>
                      /// <returns>
                      /// The value determining whether the parameter <paramref name="left"/>
                      /// is equal to the parameter <paramref name="right"/>.
                      /// </returns>
                      {{Annotation}}
                      {{Pure}}
                      {{AggressiveInlining}}
                      public static bool operator ==({{NullableName}} left, {{NullableName}} right)
                          =>{{(Named.IsReferenceType ? " left is null ? right is null : right is not null &&" : "")}} (left.{{
                              Discriminator}} == right.{{Discriminator}}) && (left.{{Discriminator}}
                          switch
                          {
                              {{Symbols
                                 .Select((x, i) => $"{i} => {Equality(x)},")
                                 .Conjoin("\n            ")}}
                              _ => {{Throw}},
                          });
                      
                      /// <summary>
                      /// Determines whether the left-hand side is unequal to the right.
                      /// </summary>
                      /// <param name="left">The left-hand side.</param>
                      /// <param name="right">The right-hand side.</param>
                      /// <returns>
                      /// The value determining whether the parameter <paramref name="left"/>
                      /// is unequal to the parameter <paramref name="right"/>.
                      /// </returns>
                      {{Annotation}}
                      {{Pure}}
                      {{AggressiveInlining}}
                      public static bool operator !=({{NullableName}} left, {{NullableName}} right)
                          => !(left == right);


                  """
            );

    [Pure]
    string DeclareExplicitStruct =>
        CanOverlapUnmanagedMemorySpace
            ? CSharp(
                $$"""
                      /// <summary>
                      /// Compact representation of all unmanaged memory within the union {{XmlName}}.
                      /// </summary>
                      {{Annotation}}
                      [global::System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
                      partial struct Unmanaged
                      {
                  {{Unmanaged.Select(DeclareFieldWithExplicitOffset).Conjoin("\n\n")}}
                      }


                  """
            )
            : "";

    string DeclareObjectEquals =>
        Named.IsRecord
            ? ""
            : CSharp(
                $"""
                 
                 
                     /// <inheritdoc cref="object.Equals(object)"/>
                     {Annotation}
                     {Pure}
                     {AggressiveInlining}
                     public override bool Equals(object? obj)
                         => {(Named.IsRefLikeType ? "false" : $"obj is {Name} x && Equals(x)")};
                 """
            );

    [Pure]
    string DeclareReferencedFields =>
        CanOverlapReferenceMemorySpace
            ? CSharp(
                $"""
                     {Annotation}
                     private {PrivatelyReadOnly}object? {ReferenceField};


                 """
            )
            : Reference.Select(DeclareField).Conjoin("");

    [Pure]
    string DeclareUnmanagedFields =>
        CanOverlapUnmanagedMemorySpace
            ? CSharp(
                $"""
                     {Annotation}
                     private {PrivatelyReadOnly}Unmanaged {UnmanagedField};


                 """
            )
            : Unmanaged.Select(DeclareField).Conjoin("");

    [Pure]
    string DeclareInterfaceImplementations =>
        CSharp(
            $$"""
              {{DeclareEqualityOperators}}    /// <summary>
                  /// Determines whether the left-hand side is greater than the right.
                  /// </summary>
                  /// <param name="left">The left-hand side.</param>
                  /// <param name="right">The right-hand side.</param>
                  /// <returns>
                  /// The value determining whether the parameter <paramref name="left"/>
                  /// is greater than the parameter <paramref name="right"/>.
                  /// </returns>
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public static bool operator >({{NullableName}} left, {{NullableName}} right)
                      =>{{(Named.IsReferenceType ? " left is null ? right is null : right is not null &&" : "")}} (left.{{
                          Discriminator}} == right.{{Discriminator}}) && (left.{{Discriminator}}
                      switch
                      {
                          {{Symbols
                             .Select((x, i) => $"{i} => {Comparison(x)},")
                             .Conjoin("\n            ")}}
                          _ => {{Throw}},
                      });
              
                  /// <summary>
                  /// Determines whether the left-hand side is greater than or equal to the right.
                  /// </summary>
                  /// <param name="left">The left-hand side.</param>
                  /// <param name="right">The right-hand side.</param>
                  /// <returns>
                  /// The value determining whether the parameter <paramref name="left"/>
                  /// is greater than or equal to the parameter <paramref name="right"/>.
                  /// </returns>
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public static bool operator >=({{NullableName}} left, {{NullableName}} right)
                      => left == right || left > right;
              
                  /// <summary>
                  /// Determines whether the left-hand side is less than the right.
                  /// </summary>
                  /// <param name="left">The left-hand side.</param>
                  /// <param name="right">The right-hand side.</param>
                  /// <returns>
                  /// The value determining whether the parameter <paramref name="left"/>
                  /// is less than the parameter <paramref name="right"/>.
                  /// </returns>
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public static bool operator <({{NullableName}} left, {{NullableName}} right)
                      => right > left;
              
                  /// <summary>
                  /// Determines whether the left-hand side is less than or equal to the right.
                  /// </summary>
                  /// <param name="left">The left-hand side.</param>
                  /// <param name="right">The right-hand side.</param>
                  /// <returns>
                  /// The value determining whether the parameter <paramref name="left"/>
                  /// is less than or equal to the parameter <paramref name="right"/>.
                  /// </returns>
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public static bool operator <=({{NullableName}} left, {{NullableName}} right)
                      => right >= left;{{DeclareObjectEquals}}
              
                  /// <inheritdoc />
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public {{ReadOnlyIfStruct}}bool Equals({{NullableName}} other)
                      => this == other;
              
                  /// <inheritdoc cref="IComparable.CompareTo(object)"/>
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public {{ReadOnlyIfStruct}}int CompareTo(object? obj)
                      => {{(Named.IsRefLikeType ? "-1" : $"obj is null ? 1 : obj is {Name} x ? CompareTo(x) : -1")}};
              
                  /// <inheritdoc />
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public {{ReadOnlyIfStruct}}int CompareTo({{NullableName}} other)
                      =>{{(Named.IsReferenceType ? " other is null ? 1 :" : "")}} Equals(other) ? 0 : -1;
              
                  /// <inheritdoc />
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public {{ReadOnlyIfStruct}}override int GetHashCode()
                      => unchecked({{Discriminator}} * {{Primes.ElementAt(Xor(Name.GetDjb2HashCode()))}}) ^
                      ({{Discriminator}} switch
                      {
                          {{Symbols
                             .Select((x, i) => $"{i} => {
                                 (IsEmpty(x) || x.Type.IsRefLikeType && x.Type.GetMembers().All(x => IsUnoriginalMethod(x, nameof(GetHashCode)))
                                     ? "0"
                                     : $"{PropertyName(x)}{NullableSuppression(x)}.GetHashCode()")},")
                             .Conjoin("\n            ")}}
                          _ => {{Throw}},
                      });
              
                  /// <inheritdoc />
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public {{ReadOnlyIfStruct}}override string ToString()
                      => {{Discriminator}} switch
                      {
                          {{Symbols.Select((x, i) => $"{i} => {ToStringCase(x)},").Conjoin("\n            ")}}
                          _ => {{Throw}},
                      };
              """
        );

    [Pure]
    public string DeclareMappers =>
        CSharp(
            $$"""


                  /// <summary>
                  /// Invokes the callback based on current variance.
                  /// </summary>
                  /// {{Symbols
                     .Select(x => $"""<param name="on{PropertyName(x)}">The callback to use when the contract of {Describe(x)} is held.</param>""")
                     .Conjoin("\n    /// ")
                  }}
                  /// <returns>Itself.</returns>
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public {{ReadOnlyIfStruct}}{{NullableName}} Map(
                      {{Symbols
                          .Select(x => $"{DelegateTypeName(x, false)}? on{PropertyName(x)} = null")
                          .Conjoin(",\n        ")}}
                  )
                  {
                      switch ({{Discriminator}})
                      {
                          {{Symbols
                              .Select((x, i) => $"case {i}:\n                on{PropertyName(x)}?.Invoke({(IsEmpty(x) ? "" : PropertyName(x))}{NullableSuppression(x)});\n                return this;")
                              .Conjoin("\n            ")}}
                          default: {{Throw}};
                      }
                  }

                  /// <summary>
                  /// Maps each variant to <typeparamref name="{{ResultGeneric}}"/>.
                  /// </summary>
                  /// <typeparam name="{{ResultGeneric}}">The resulting type from the mapping.</typeparam>
              {{Symbols
                      .Select(x => $"""    /// <param name="on{PropertyName(x)}">The callback to use when the contract of {Describe(x)} is held.</param>""")
                      .Conjoin("\n")
              }}
                  /// <returns>
                  /// The resulting value from one of the parameters based on the current state of the object.
                  /// </returns>
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public {{ReadOnlyIfStruct}}{{ResultGeneric}} Map<{{ResultGeneric}}>(
                      {{Symbols
                          .Select(x => $"{DelegateTypeName(x, true)} on{PropertyName(x)}")
                          .Conjoin(",\n        ")}}
                  )
                      => {{Discriminator}}
                      switch
                      {
                          {{Symbols
                              .Select((x, i) => $"{i} => on{PropertyName(x)}({(IsEmpty(x) ? "" : PropertyName(x))}{NullableSuppression(x)}),")
                              .Conjoin("\n            ")}}
                          _ => {{Throw}},
                      };
              """
        );

    [Pure]
    string Discriminator =>
        _discriminator ??= Reference.Count != Symbols.Count ||
            Symbols.Select(x => x.Type).GroupDuplicates(TypeSymbolComparer.Default).Any()
                ? DiscriminatorField
                : DiscriminatorProperty;

    [Pure]
    string HintName { get; } = Named.HintName();

    [Pure]
    string Name { get; } = Named.GetFullyQualifiedName();

    [Pure]
    string NullableName { get; } = $"{Named.WithNullableAnnotation(NullableAnnotation.Annotated)}";

    [Pure]
    string Source =>
        _source ??= $"{Header}{Named
           .ContainingWithoutGlobal()
           .FindSmallPathToNull(x => x.ContainingWithoutGlobal())
           .Aggregate(DeclareType, WrapNamespace)}\n";

    [Pure]
    string XmlName { get; } = XmlTypeName(Named);

    [Pure]
    HashSet<FieldOrProperty> Members { get; } = Named
       .GetMembers()
       .Select(x => FieldOrProperty.TryCreate(x, out var res) ? res : (FieldOrProperty?)null)
       .Filter()
       .ToSet();

    [Pure]
    SmallList<FieldOrProperty> Reference { get; } = Symbols.Omit(IsUnmanaged).Where(IsReference).ToSmallList();

    [Pure]
    SmallList<FieldOrProperty> Rest { get; } = Symbols.Omit(IsUnmanaged).Omit(IsReference).ToSmallList();

    [Pure]
    SmallList<FieldOrProperty> Unmanaged { get; } = Symbols.Where(IsUnmanaged).Omit(IsEmpty).ToSmallList();

    [Pure]
    public static bool IsSystemTuple([NotNullWhen(true)] ITypeSymbol? symbol) =>
        symbol is INamedTypeSymbol
        {
            Name: nameof(Tuple),
            TypeArguments.Length: > 1,
            ContainingNamespace: { Name: nameof(System), ContainingNamespace.IsGlobalNamespace: true },
        };

    [Pure]
    public static SmallList<FieldOrProperty> Decouple(ImmutableArray<IFieldSymbol> fields) =>
        (fields.Length is TupleGenericLimit &&
            fields[^1].Type is INamedTypeSymbol { IsTupleType: true, IsValueType: true, TupleElements: var tuple }
                ? fields.Take(TupleGenericLimit - 1).Select(x => new FieldOrProperty(x)).Concat(Decouple(tuple))
                : fields.Select(x => new FieldOrProperty(x))).ToSmallList();

    [Pure]
    public static Scaffolder From(Raw x) => new(x.Named, x.Fields, x.MutablePublicly);

    [Pure]
#pragma warning disable MA0040
    public static Scaffolder From(Raw x, CancellationToken _) => From(x);
#pragma warning restore MA0040
    [Pure]
    public static SmallList<FieldOrProperty> Instances(INamespaceOrTypeSymbol x) =>
        x.GetMembers()
           .Select(x => FieldOrProperty.TryCreate(x, out var res) ? res : (FieldOrProperty?)null)
           .Filter()
           .Omit(x => x.IsStatic || x.Symbol is IPropertySymbol { ExplicitInterfaceImplementations: not [] })
           .ToSmallList();

    [Pure]
    static bool IsEmpty(FieldOrProperty x) =>
        x.Type is { BaseType.SpecialType: not SpecialType.System_Enum, IsValueType: true } type &&
        !type.IsUnmanagedPrimitive() &&
        type.GetMembers().All(IsEmpty);

    [Pure]
    static bool IsEmpty(ISymbol x) =>
        x is { IsStatic: true } or
            not IFieldSymbol and
            not IMethodSymbol { MethodKind: MethodKind.Constructor, Parameters: not [] };

    [Pure]
    static bool IsInterfaceComparable(FieldOrProperty x) =>
        x.Type.GetMembers().Any(x => IsSingleSelf(x, nameof(IComparable.CompareTo)));

    [Pure]
    static bool IsInterfaceEquatable(FieldOrProperty x) =>
        x.Type.GetMembers().Any(x => IsSingleSelf(x, nameof(Equals)));

    [Pure]
    static bool IsOperatorComparable(FieldOrProperty x) =>
        x.Type.BaseType?.SpecialType is SpecialType.System_Enum ||
        x.Type.IsUnmanagedPrimitive() ||
        x.Type.GetMembers().Any(x => IsOperator(x, "op_GreaterThan"));

    [Pure]
    static bool IsOperator(ISymbol symbol, string expect) =>
        symbol is IMethodSymbol
        {
            IsStatic: true,
            Name: var name,
            DeclaredAccessibility: Accessibility.Public,
            MethodKind: MethodKind.BuiltinOperator,
        } &&
        expect == name;

    [Pure]
    static bool IsOperatorEquatable(FieldOrProperty x) =>
        x.Type.BaseType?.SpecialType is SpecialType.System_Enum ||
        x.Type.IsUnmanagedPrimitive() ||
        x.Type.GetMembers().Any(x => IsOperator(x, "op_Equality"));

    [Pure]
    static bool IsReference(FieldOrProperty x) => x.Type.IsReferenceType;

    [Pure]
    static bool IsSingleSelf(ISymbol x, string expect) =>
        x is IMethodSymbol
        {
            Name: var name,
            IsStatic: false,
            ContainingType: { } type,
            Parameters: [{ Type: INamedTypeSymbol other }],
        } &&
        expect == name &&
        NamedTypeSymbolComparer.Equal(type, other);

    [Pure]
    static bool IsUnmanaged(FieldOrProperty x) => x.Type.IsUnmanagedType && x.Type is not ITypeParameterSymbol;

    [Pure]
    static bool IsUnoriginalMethod(ISymbol x, string name) =>
        x is not IMethodSymbol
        {
            Name: var methodName,
            IsVirtual: false,
            Parameters: [],
            TypeParameters: [],
        } ||
        methodName != name;

    [Pure]
    static ushort Xor(int i) => (ushort)((ushort)(i >> sizeof(ushort) * BitsInByte) ^ (ushort)i);

    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    static string CSharp([StringSyntax("C#")] string x) => x;

    [Pure]
    static string NullableAnnotated(FieldOrProperty x) =>
        $"{x.Type.WithNullableAnnotation(NullableAnnotation.Annotated)}";

    [Pure]
    static string NullableSuppression(FieldOrProperty x) => x.Type.IsValueType ? "" : "!";

    [Pure]
    static string WrapNamespace(string acc, ISymbol next) =>
        CSharp(
            $$"""
              {{(next is ITypeSymbol type ? $"partial {type.Keyword()} {next.Name}" : $"namespace {next.Name}")}}
              {
              {{acc.Split('\r', '\n').Select(x => x is "" or ['#', ..] ? x : $"    {x}").Conjoin("\n")}}
              }
              """
        );

    [Pure]
    static string XmlTypeName(ISymbol x, string tag = "see")
    {
        [Pure]
        static string Inner(ISymbol x) =>
            (x is INamedTypeSymbol { IsTupleType: true, TypeArguments: { Length: > 1 } args }
                ? $"{nameof(System)}.{nameof(ValueTuple)}{{{args.Select((_, i) => $"T{i + 1}").Conjoin()}}}"
                : $"{x.OriginalDefinition}")
           .Replace('<', '{')
           .Replace('>', '}')
           .Replace("scoped ", "");

        var genericTag = x is ITypeParameterSymbol ? "typeparamref" : tag;
        var attribute = x is ITypeParameterSymbol ? "name" : "cref";
        return $"<{genericTag} {attribute}=\"{Inner(x)}\"/>";
    }

    [Pure]
    bool HasConflict(FieldOrProperty x) => Symbols.Where(y => TypeSymbolComparer.Equal(x.Type, y.Type)).Skip(1).Any();

    [Pure]
    bool IsNoninitial(FieldOrProperty x) =>
        Symbols.Where(y => TypeSymbolComparer.Equal(x.Type, y.Type)).Skip(1).Contains(x);

    [Pure]
    int Inheritance((FieldOrProperty Field, int Index) tuple) =>
        Rest.Count( // ReSharper disable once NullableWarningSuppressionIsUsed
            x => tuple.Field.Type.FindSmallPathToNull(x => x.BaseType).Contains(x.Type, TypeSymbolComparer.Default!) ||
                tuple.Field.Type.AllInterfaces.Contains(x.Type, TypeSymbolComparer.Default)
        );

    [Pure]
    string Comparison(FieldOrProperty x) =>
        IsEmpty(x) ? CSharp("true") :
        IsOperatorComparable(x) ? CSharp($"left.{PropertyName(x)} > right.{PropertyName(x)}") :
        IsInterfaceComparable(x) ? CSharp($"left.{PropertyName(x)}{NullableSuppression(x)}.CompareTo(right.{PropertyName(x)}) > 0") :
        CSharp("false");

    [Pure]
    string DeclareAlternativeConstructor(FieldOrProperty x, bool conflict) =>
        !conflict &&
        x.Type switch
        {
            INamedTypeSymbol { IsTupleType: true, TupleElements: { Length: > 1 } e } => (true, true, Decouple(e)),
            INamedTypeSymbol type when IsSystemTuple(type) => (true, false, Instances(type)),
            _ => default,
        } is (true, var isValue, var enumerable) &&
        enumerable.ToSmallList() is var parameters
            ? CSharp(
                $$"""
                      /// <summary>
                      /// Initializes a new instance of {{Describe(x)}}.
                      /// </summary>
                  {{parameters
                     .Select(x => $"    /// <param name=\"{ParameterName(x)}\">The {ParameterName(x)} item within the variant.</param>")
                     .Conjoin("\n")}}
                      {{Annotation}}
                      {{Pure}}
                      {{AggressiveInlining}}
                      public {{Named.Name}}({{parameters.Select(x => $"{x.Type} {ParameterName(x)}").Conjoin()}})
                          : this({{(isValue ? nameof(ValueTuple) : nameof(Tuple))}}.{{nameof(Tuple.Create)}}({{
                              parameters.Select(ParameterName).Conjoin()}})) { }


                  """
            )
            : "";

    [Pure]
    string DeclareAlternativeFactory(FieldOrProperty x, string discriminator)
    {
        var parameters = x.Type switch
        {
            INamedTypeSymbol { IsTupleType: true, TupleElements: var e, TypeArguments.Length: > 1 } => Decouple(e),
            INamedTypeSymbol t when IsSystemTuple(t) => Instances(t),
            _ => default,
        };

        if (parameters is [])
            return "";

        return CSharp(
            $"""
                 /// <summary>
                 /// Creates a new instance of {Describe(x)}.
                 /// </summary>
             {parameters
                .Select(x => $"    /// <param name=\"{ParameterName(x)}\">The {ParameterName(x)} item within the value to pass into the type.</param>")
                .Conjoin("\n")
             }
                 /// <returns>The union containing the parameters.</returns>
                 {Annotation}
                 {Pure}
                 {AggressiveInlining}
                 public static {Name} Of{PropertyName(x)}({parameters.Select(x => $"{x.Type} {ParameterName(x)}").Conjoin()})
                     => new {Name}({parameters.Select(ParameterName).Conjoin()}{discriminator});


             """
        );
    }

    [Pure]
    string DeclareCheck(FieldOrProperty x, int i) =>
        CSharp(
            $$"""
                  /// <summary>
                  /// Gets the value determining if the {{XmlName}} is the variant {{See(x)}} of type {{XmlTypeName(x.Type)}}.
                  /// </summary>
                  {{Annotation}}
                  public {{ReadOnlyIfStruct}}bool Is{{PropertyName(x)}}
                  {
                      {{Pure}}{{(IsEmpty(x)
                          ? ""
                          : CSharp($"\n        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, \"{PropertyName(x)}\")]{Opposite(x)}"))}}
                      {{AggressiveInlining}}
                      get => {{Discriminator}} is {{i}};
                  }


              """
        );

    [Pure]
    string DeclareConstructor(FieldOrProperty x, int i) =>
        HasConflict(x) is var conflict && conflict && IsNoninitial(x)
            ? ""
            : CSharp(
                $$"""
                      /// <summary>
                      /// Initializes a new instance of {{Describe(x)}}.
                      /// </summary>
                      /// <param name="{{ParameterName(x)}}">The variant.</param>{{
                          (conflict ? "\n    /// <param name=\"x\">The discriminator.</param>" : "")}}
                      {{Annotation}}
                      {{Pure}}
                      {{AggressiveInlining}}
                      {{(conflict ? "private" : "public")}} {{Named.Name}}({{x.Type}} {{ParameterName(x)}}{{(conflict ? ", byte x" : IsEmpty(x) ? " = default" : "")}})
                      {
                          {{Discriminator}} = {{(conflict ? "x" : i)}};{{(IsEmpty(x) ? "" : CSharp($"\n        {Prefix(x)} = {ParameterName(x)};"))}}
                      }

                  {{DeclareAlternativeConstructor(x, conflict)}}
                  """
            );

    [Pure]
    string DeclareDelegate(FieldOrProperty x) =>
        x.Type.IsRefLikeType && !IsEmpty(x)
            ? CSharp(
                $"""
                     /// <summary>
                     /// Explicit side effect delegate for {Describe(x)} due to it being a by-ref like type.
                     /// </summary>
                     /// <param name="{ParameterName(x)}">The referenced value.</param>
                     {Annotation}
                     public delegate void {PropertyName(x)}Handler({x.Type} {ParameterName(x)});
                 
                     /// <summary>
                     /// Explicit mapper delegate for {Describe(x)} due to it being a by-ref like type.
                     /// </summary>
                     /// <typeparam name="{ResultGeneric}">The type of value to return.</typeparam>
                     /// <param name="{ParameterName(x)}">The referenced value.</param>
                     /// <returns>The result of the mapping.</returns>
                     {Annotation}
                     public delegate {ResultGeneric} {PropertyName(x)}Handler<out {ResultGeneric}>({x.Type} {ParameterName(x)});


                 """
            )
            : "";

    [Pure]
    string DeclareExplicitOperator(FieldOrProperty x) =>
        IsEmpty(x)
            ? ""
            : CSharp(
                $"""
                     /// <summary>
                     /// Explicitly converts the union to the target type {XmlTypeName(x.Type)}.
                     /// </summary>
                     /// <param name="x">The union to access its property.</param>
                     /// <returns>The getter of the union <paramref name="x"/>.</returns>
                     {Annotation}
                     {Pure}
                     {AggressiveInlining}
                     public static explicit operator {NullableAnnotated(x)}({Name} x)
                         => x.{PropertyName(x)};


                 """
            );

    [Pure]
    string DeclareFactory(FieldOrProperty x, int i)
    {
        var discriminator = HasConflict(x) ? $", {i}" : "";
        var fallback = IsEmpty(x) ? " = default" : "";

        return CSharp(
            $"""
                 /// <summary>
                 /// Creates a new instance of {Describe(x)}.
                 /// </summary>
                 /// <param name="{ParameterName(x)}">The value to pass into the type.</param>
                 /// <returns>The union containing the parameter <paramref name="{ParameterName(x)}"/>.</returns>
                 {Annotation}
                 {Pure}
                 {AggressiveInlining}
                 public static {Name} Of{PropertyName(x)}({x.Type} {ParameterName(x)}{fallback})
                     => new {Name}({ParameterName(x)}{discriminator});

             {DeclareAlternativeFactory(x, discriminator)}
             """
        );
    }

    [Pure]
    string DeclareField(FieldOrProperty x) =>
        Members.Contains(x)
            ? ""
            : CSharp(
                $"""
                     private {PrivatelyReadOnly}{NullableAnnotated(x)} {FieldName(x)};


                 """
            );

    [Pure]
    string DeclareOperators(FieldOrProperty x) =>
        x.Type.BaseType is null || HasConflict(x) || IsNoninitial(x)
            ? ""
            : CSharp(
                $"""
                     /// <summary>
                     /// Implicitly converts the {XmlTypeName(x.Type)} parameter to the union.
                     /// </summary>
                     /// <param name="{ParameterName(x)}">The parameter to pass onto the constructor.</param>
                     /// <returns>The union containing the parameter <paramref name="{ParameterName(x)}"/>.</returns>
                     {Annotation}
                     {Pure}
                     {AggressiveInlining}
                     public static implicit operator {Name}({x.Type} {ParameterName(x)})
                         => new {Name}({ParameterName(x)});

                 {DeclareExplicitOperator(x)}
                 """
            );

    [Pure]
    string DeclareProperty(FieldOrProperty x, int i)
    {
        if (IsEmpty(x))
            return "";

        var prefix = Prefix(x);
        var cast = prefix is ReferenceField ? $"({x.Type})" : "";
        var suppression = prefix is ReferenceField ? NullableSuppression(x) : "";

        var setter = MutablePublicly is null
            ? ""
            : CSharp(
                $$"""

                          {{AggressiveInlining}}
                          {{(MutablePublicly is false ? "private " : "")}}set
                          {
                              {{Discriminator}} = {{i}};
                              {{prefix}} = value;
                          }
                  """
            );

        return CSharp(
            $$"""
                  /// <summary>
                  /// Gets{{(MutablePublicly is true ? " or sets" : "")}} the {{XmlTypeName(x.Type)}} variant.
                  /// </summary>
                  {{Annotation}}
                  public {{ReadOnlyIfImmutableStruct}}{{NullableAnnotated(x)}} {{PropertyName(x)}}
                  {
                      {{Pure}}
                      {{AggressiveInlining}}
                      {{ReadOnlyIfMutableStruct}}get => {{Discriminator}} is {{i}} ? {{cast}}{{prefix}}{{suppression}} : default;{{setter}}
                  }


              """
        );
    }

    [Pure]
    string DelegateTypeName(FieldOrProperty x, bool hasGenericReturn) =>
        $"{(x.Type.IsRefLikeType && !IsEmpty(x) ? $"{PropertyName(x)}Handler" : hasGenericReturn ? Func : Action)}{(
            x.Type.IsRefLikeType || IsEmpty(x)
                ? hasGenericReturn ? $"<{ResultGeneric}>" : ""
                : hasGenericReturn ? $"<{x.Type}, {ResultGeneric}>" : $"<{x.Type}>")}";

    [Pure]
    string DeclareFieldWithExplicitOffset(FieldOrProperty x) =>
        CSharp(
            $"""
                     [global::System.Runtime.InteropServices.FieldOffsetAttribute(0)]
                     internal {x.Type} {FieldName(x)};
             """
        );

    [Pure]
    string Describe(FieldOrProperty x) =>
        $"the {XmlName} {Named.Keyword()} with the variant {See(x)} of type {XmlTypeName(x.Type)}";

    [Pure]
    string Equality(FieldOrProperty x) =>
        IsEmpty(x) ? CSharp("true") :
        IsOperatorEquatable(x) ? CSharp($"left.{PropertyName(x)} == right.{PropertyName(x)}") :
        IsInterfaceEquatable(x) ? CSharp($"left.{PropertyName(x)}{NullableSuppression(x)}.Equals(right.{PropertyName(x)})") :
        CSharp("false");

    [Pure]
    string FieldName(FieldOrProperty x) =>
        Members.Contains(x) ? x.Name : $"_{x.Name.Nth(0)?.ToLower()}{x.Name.Nth(1..)}";

    [Pure]
    string Opposite(FieldOrProperty x) =>
        Symbols.TrySingle(y => x != y, out var other)
            ? CSharp(
                $"""

                         [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, "{PropertyName(other)}")]
                 """
            )
            : "";

    [Pure]
    string Prefix(FieldOrProperty x) =>
        Members.Contains(x) ? x.Name :
        CanOverlapUnmanagedMemorySpace && Unmanaged.Contains(x) ? $"{UnmanagedField}.{FieldName(x)}" :
        CanOverlapReferenceMemorySpace && Reference.Contains(x) ? ReferenceField : FieldName(x);

    [Pure]
    string ParameterName(FieldOrProperty x) => $"{FieldName(x).Nth(1..)}";

    [Pure]
    string PropertyName(FieldOrProperty x) =>
        Members.Contains(x) && x.Name.TrimStart('_') is var trim
            ? $"{trim.Nth(0)?.ToUpper()}{trim.Nth(1..)}"
            : x.Name;

    [Pure]
    string See(FieldOrProperty x) => IsEmpty(x) ? PropertyName(x) : $"<see cref=\"{PropertyName(x)}\"/>";

    [Pure]
    string ToStringCase(FieldOrProperty x) =>
        IsEmpty(x) || x.Type.IsRefLikeType && x.Type.GetMembers().All(x => IsUnoriginalMethod(x, nameof(ToString)))
            ? $"\"{PropertyName(x)}\""
            : CSharp(
                $$"""
                  $"{nameof({{PropertyName(x)}})}({{{PropertyName(x)}}{{(x.Type.IsRefLikeType ? ".ToString()" : "")}}})"
                  """
            );
}
