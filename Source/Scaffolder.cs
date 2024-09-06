// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

sealed partial record Scaffolder(
    INamedTypeSymbol Named,
    SmallList<MemberSymbol> Symbols,
    bool? MutablePublicly,
    bool PolyfillAttributes
)
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
        Suppression = "#pragma warning disable\n",
        Throw = "throw new global::System.InvalidOperationException()",
        UnmanagedField = "_unmanaged";

    static readonly ConcurrentDictionary<string, int> s_nameCounter = new(StringComparer.Ordinal);

    [ValueRange(Primes.Min, Primes.MaxInt16)]
    readonly short _hash =
        Primes.Index(s_nameCounter.GetOrAdd(Named.GetFullyQualifiedName(), _ => s_nameCounter.Count + 1));

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
    string VirtualIfNonSealedRecordClass { get; } =
        Named is { IsRecord: true, IsSealed: false, IsValueType: false } ? CSharp("virtual ") : "";

    [Pure]
    string DeclareType =>
        CSharp(
            $$"""
              /// {{XmlTypeName(Named, "inheritdoc")}}
              /// <remarks>
              ///     <para>
              ///         The type {{XmlName}}
              ///         is {{MutablePublicly switch {
                  true => "a mutable",
                  false => "a",
                  null => "an immutable",
              }}} disjoint union representing the following variants:
              ///     </para>
              ///     <list type="table">
              ///         <listheader>
              ///             <term>
              ///                 <c>Name</c> <see langword="as"/> <c>Type</c>
              ///                 <list type="bullet">
              ///                     <item>
              ///                         <description>
              ///                             Predicate
              ///                         </description>
              ///                     </item>
              ///                     <item>
              ///                         <description>
              ///                             {{(Symbols.All(SkipOperator) ? "Factory" : "Factories")}}
              ///                         </description>
              ///                     </item>
              ///                 </list>
              ///             </term>
              ///         </listheader>
              {{Symbols
                 .Select(x =>
                      CSharp(
                          $"""
                           ///         <item>
                           ///             <term>
                           ///                 {See(x)} <see langword="as"/> {XmlTypeName(x.Type)}
                           ///                 <list type="bullet">
                           ///                     <item>
                           ///                         <description>
                           ///                             <see cref="Is{PropertyName(x)}"/>
                           ///                         </description>
                           ///                     </item>
                           ///                     <item>
                           ///                         <description>
                           {(SkipOperator(x) ?
                               CSharp(
                                   $"""///                             <see cref="Of{PropertyName(x)}({XmlEscape(x.Type)})"/>"""
                               ) : CSharp(
                                   $"""
                                    ///                             <list type="number">
                                    ///                                 <item>
                                    ///                                     <description>
                                    ///                                         <see cref="Of{PropertyName(x)}({XmlEscape(x.Type, true)})"/>
                                    ///                                     </description>
                                    ///                                 </item>
                                    ///                                 <item>
                                    ///                                     <description>
                                    ///                                         <see cref="{XmlEscape(Named)}({XmlEscape(x.Type, true)})"/>
                                    ///                                     </description>
                                    ///                                 </item>
                                    ///                                 <item>
                                    ///                                     <description>
                                    ///                                         <see cref="op_Implicit({XmlEscape(x.Type, true)})"/>
                                    ///                                     </description>
                                    ///                                 </item>
                                    ///                             </list>
                                    """))}
                           ///                         </description>
                           ///                     </item>
                           ///                 </list>
                           ///             </term>
                           ///         </item>
                           """))
                 .Conjoin("\n")}}
              ///     </list>
              /// </remarks>
              {{AutoIfStruct}}partial {{Named.Keyword()}} {{Named.Name
              }}{{(Named.TypeArguments is [] ? "" : $"<{Named.TypeArguments.Conjoin()}>")
              }}{{DeclareInterfaces}}
              {
              {{DeclarePolyfillAttributes
              }}{{DeclareExplicitStruct
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

    [Pure]
    string DeclarePolyfillAttributes =>
        PolyfillAttributes
            ? CSharp(
                $"""
                     {Annotation}
                 {(MutablePublicly switch
                 {
                     true => nameof(Accessibility.Public),
                     false => nameof(Accessibility.Private),
                     null => null,
                 }).YieldValued()
                .Concat(Symbols.Select(x => x.Name))
                .Prepend("Choice")
                .Reverse()
                .Index()
                .Aggregate("", DeclareNestedClass)}


                 """
            )
            : "";

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
        Members.Any(x => x.Name == Discriminator) ? "" :
        Discriminator is DiscriminatorProperty ? CSharp(
            $$"""
                  {{Annotation}}
                  private {{ReadOnlyIfStruct}}byte {{Discriminator}}
                  {
                      {{Pure}}
                      {{AggressiveInlining}}
                      get => {{ReferenceField}} switch
                      {
                          {{Symbols
                             .Index()
                             .OrderByDescending(Inheritance)
                             .Select(x => $"{(x.Item.IsEmpty ? "null" : x.Item.Type)} => {x.Index},")
                             .Conjoin("\n            ")}}{{(Symbols.Any(x => x.IsEmpty) ? "" : $"\n_ => {Throw},")}}
                      };
                      {{Pure}}
                      {{AggressiveInlining}}
                      set { }
                  }
              """
        ) : CSharp(
            $"""
                 {Annotation}
                 private {PrivatelyReadOnly}byte {Discriminator};
             """
        );

    [Pure]
    string DeclareEqualityOperators =>
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
                     private {PrivatelyReadOnly}{new MemberSymbol(Common, "").NullableAnnotated} {ReferenceField};


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
                  public {{VirtualIfNonSealedRecordClass}}{{ReadOnlyIfStruct}}bool Equals({{NullableName}} other)
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
                      => unchecked({{Discriminator}} * {{_hash}}) ^
                      ({{Discriminator}} switch
                      {
                          {{Symbols
                             .Select((x, i) => $"{i} => {
                                 (x.IsEmpty || x.Type.IsRefLikeType && x.Type.GetMembers().All(x => IsUnoriginalMethod(x, nameof(GetHashCode)))
                                     ? "0"
                                     : $"{PrefixCast(x)}.GetHashCode()")},")
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
    string DeclareMappers =>
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
                              .Select((x, i) => $"case {i}:\n                on{PropertyName(x)
                              }?.Invoke({(x.IsEmpty ? "" : PrefixCast(x))});\n                return this;")
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
                              .Select((x, i) => $"{i} => on{PropertyName(x)}({(x.IsEmpty ? "" : PrefixCast(x))}),")
                              .Conjoin("\n            ")}}
                          _ => {{Throw}},
                      };

              {{DeclareUnderlyingValue}}
              """
        );

    string DeclareUnderlyingValue =>
        Common is null
            ? ""
            : CSharp(
                $"""
                     /// <summary>
                     /// Gets the underlying value.
                     /// </summary>
                     /// <returns>
                     /// The underlying value from this instance.
                     /// </returns>
                     {Annotation}
                     {Pure}
                     {AggressiveInlining}
                     public {ReadOnlyIfStruct}{Common} GetUnderlyingValue()
                         => {(Symbols.Count == Reference.Count
                             ? $"{(Common.SpecialType is SpecialType.System_Object
                                 ? ""
                                 : $"({Common})")}{ReferenceField}!"
                             : CSharp(
                                 $$"""
                                   {{Discriminator}}
                                   switch
                                   {
                                       {{Symbols
                                          .Select((x, i) => $"{i} => {PrefixCast(x)},")
                                          .Conjoin("\n            ")}}
                                       _ => {{Throw}},
                                   }
                                   """
                             ))};
                 """
            );

    [Pure]
    string Discriminator =>
        _discriminator ??= Symbols.Count != Reference.Count ||
            Symbols.Omit(x => x.IsEmpty).Any(Members.Contains) ||
            Symbols.Select(x => x.Type).GroupDuplicates(TypeSymbolComparer.Default).Any()
                ? DiscriminatorField
                : DiscriminatorProperty;

    [Pure]
    string HintName { get; } = Named.HintName();

    [Pure]
    string Name { get; } = Named.GetFullyQualifiedName();

    [Pure]
    string NullableName { get; } = new MemberSymbol(Named, "").NullableAnnotated;

    [Pure]
    string Source =>
        _source ??= $"{Header}{Suppression}{Named
           .ContainingWithoutGlobal()
           .FindSmallPathToNull(x => x.ContainingWithoutGlobal())
           .Aggregate(DeclareType, WrapNamespace)}\n";

    [Pure]
    string XmlName { get; } = XmlTypeName(Named);

    [Pure]
    HashSet<MemberSymbol> Members { get; } = Named.GetMembers().Select(MemberSymbol.From).Filter().ToSet();

    [Pure]
    ITypeSymbol? Common { get; } = Signature
       .FindCommonBaseTypes(Symbols.Except(SingleEmpty(Symbols)).ToSmallList())
       .FirstOrDefault();

    [Pure]
    SmallList<MemberSymbol> Reference { get; } =
        Symbols.Omit(x => x.IsUnmanaged).Where(x => x.IsReference).Concat(SingleEmpty(Symbols)).ToSmallList();

    [Pure]
    SmallList<MemberSymbol> Rest { get; } =
        Symbols.Omit(x => x.IsUnmanaged).Omit(x => x.IsReference).Except(SingleEmpty(Symbols)).ToSmallList();

    [Pure]
    SmallList<MemberSymbol> Unmanaged { get; } = Symbols.Where(x => x.IsUnmanaged).Omit(x => x.IsEmpty).ToSmallList();

    [Pure]
    public static bool IsSystemTuple([NotNullWhen(true)] ITypeSymbol? symbol) =>
        symbol is INamedTypeSymbol
        {
            Name: nameof(Tuple),
            TypeArguments.Length: > 1,
            ContainingNamespace: { Name: nameof(System), ContainingNamespace.IsGlobalNamespace: true },
        };

    [Pure]
    public static Scaffolder From(Raw raw) => new(raw.Named, raw.Fields, raw.MutablePublicly, raw.PolyfillAttributes);

    [Pure]
    public static SmallList<MemberSymbol> Decouple(ImmutableArray<IFieldSymbol> fields) =>
        (fields.Length is TupleGenericLimit &&
            fields[^1].Type is INamedTypeSymbol { IsTupleType: true, IsValueType: true, TupleElements: var tuple }
                ? fields.Take(TupleGenericLimit - 1).Select(x => new MemberSymbol(x)).Concat(Decouple(tuple))
                : fields.Select(x => new MemberSymbol(x))).ToSmallList();

    [Pure]
    public static SmallList<MemberSymbol> Instances(INamespaceOrTypeSymbol x) =>
        x.GetMembers()
           .Select(MemberSymbol.From)
           .Filter()
           .Omit(x => x.IsStatic || x.Symbol is IPropertySymbol { ExplicitInterfaceImplementations: not [] } || x.IsEq)
           .ToSmallList();

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

    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    static string CSharp([StringSyntax("C#")] string x) => x;

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
    static string XmlEscape(ISymbol x, bool allowTypeSubstitution = false) =>
        (x is INamedTypeSymbol { IsTupleType: true, TypeArguments: { Length: > 1 } args }
            ? $"{nameof(System)}.{nameof(ValueTuple)}{{{args.Length.For(i => $"T{i + 1}").Conjoin()}}}"
            : $"{(allowTypeSubstitution ? x : x.OriginalDefinition)}")
       .Replace('<', '{')
       .Replace('>', '}')
       .Replace("scoped ", "");

    [Pure]
    static string XmlTypeName(ISymbol x, string tag = "see")
    {
        var genericTag = x is ITypeParameterSymbol ? "typeparamref" : tag;
        var attribute = x is ITypeParameterSymbol ? "name" : "cref";
        return $"<{genericTag} {attribute}=\"{XmlEscape(x)}\"/>";
    }

    [Pure]
    static SmallList<MemberSymbol> SingleEmpty(SmallList<MemberSymbol> symbols)
    {
        MemberSymbol single = default;

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var symbol in symbols)
            if (symbol.IsEmpty)
                if (single.Type is null)
                    single = symbol;
                else
                    return [];

        return single.Type is null ? [] : single;
    }

    [Pure]
    bool HasConflict(MemberSymbol x) => Symbols.Where(y => TypeSymbolComparer.Equal(x.Type, y.Type)).Skip(1).Any();

    [Pure]
    bool IsNoninitial(MemberSymbol x) =>
        Symbols.Where(y => TypeSymbolComparer.Equal(x.Type, y.Type)).Skip(1).Contains(x);

    [Pure]
    bool SkipOperator(MemberSymbol x) => x.Type.BaseType is null || HasConflict(x) || IsNoninitial(x);

    [Pure]
    int Inheritance((int Index, MemberSymbol Item) tuple) =>
        Rest.Count(
            x => tuple.Item.Type.FindSmallPathToNull(x => x.BaseType).Contains(x.Type, TypeSymbolComparer.Default) ||
                tuple.Item.Type.AllInterfaces.Contains(x.Type, TypeSymbolComparer.Default)
        );

    [Pure]
    string Comparison(MemberSymbol x) =>
        x.IsEmpty ? CSharp("true") :
        x.IsOperatorComparable ? CSharp($"{PrefixCast(x, "left.")} > {PrefixCast(x, "right.")}") :
        x.IsInterfaceComparable ? CSharp($"{PrefixCast(x, "left.")}.CompareTo({PrefixCast(x, "right.")}) > 0") :
        CSharp("false");

    [Pure]
    string DeclareAlternativeConstructor(MemberSymbol x, bool conflict) =>
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
    string DeclareAlternativeFactory(MemberSymbol x, string discriminator)
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
    string DeclareCheck(MemberSymbol x, int i) =>
        CSharp(
            $$"""
                  /// <summary>
                  /// Gets the value determining if the {{XmlName}} is the variant {{See(x)}} of type {{XmlTypeName(x.Type)}}.
                  /// </summary>
                  {{Annotation}}
                  public {{ReadOnlyIfStruct}}bool Is{{PropertyName(x)}}
                  {
                      {{Pure}}{{(x.IsEmpty
                          ? Opposite(x)
                          : CSharp($"\n        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, \"{PropertyName(x)}\")]{Opposite(x)}"))}}
                      {{AggressiveInlining}}
                      get => {{Discriminator}} is {{i}};
                  }


              """
        );

    [Pure]
    string DeclareConstructor(MemberSymbol x, int i) =>
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
                      {{AggressiveInlining}}
                      {{(conflict ? "private" : "public")}} {{Named.Name}}({{x.Type}} {{ParameterName(x)}}{{(conflict ? ", byte x" : x.IsEmpty ? " = default" : "")}})
                      {
                          {{Discriminator}} = {{(conflict ? "x" : i)}};{{(x.IsEmpty ? "" : CSharp($"\n        {Prefix(x)} = {ParameterName(x)};"))}}
                      }

                  {{DeclareAlternativeConstructor(x, conflict)}}
                  """
            );

    [Pure]
    string DeclareDelegate(MemberSymbol x) =>
        x.Type.IsRefLikeType && !x.IsEmpty
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
    string DeclareExplicitOperator(MemberSymbol x) =>
        x.IsEmpty
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
                     public static explicit operator {x.NullableAnnotated}({Name} x)
                         => x.{PropertyName(x)};


                 """
            );

    [Pure]
    string DeclareFactory(MemberSymbol x, int i)
    {
        var discriminator = HasConflict(x) ? $", {i}" : "";
        var fallback = x.IsEmpty ? " = default" : "";

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
    string DeclareField(MemberSymbol x) =>
        x.IsEmpty || Members.Contains(x)
            ? ""
            : CSharp(
                $"""
                     private {PrivatelyReadOnly}{x.NullableAnnotated} {FieldName(x)};


                 """
            );

    [Pure]
    string DeclareOperators(MemberSymbol x) =>
        SkipOperator(x)
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
    string DeclareProperty(MemberSymbol x, int i)
    {
        if (x.IsEmpty)
            return "";

        var prefix = Prefix(x);
        var cast = prefix is ReferenceField ? $"({x.Type})" : "";
        var suppression = prefix is ReferenceField ? x.NullableSuppression : "";

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
                  public {{ReadOnlyIfImmutableStruct}}{{x.NullableAnnotated}} {{PropertyName(x)}}
                  {
                      {{Pure}}
                      {{AggressiveInlining}}
                      {{ReadOnlyIfMutableStruct}}get => {{Discriminator}} is {{i}} ? {{cast}}{{prefix}}{{suppression}} : default;{{setter}}
                  }


              """
        );
    }

    [Pure]
    string DelegateTypeName(MemberSymbol x, bool hasGenericReturn) =>
        $"{(x.Type.IsRefLikeType && !x.IsEmpty ? $"{PropertyName(x)}Handler" : hasGenericReturn ? Func : Action)}{(
            x.Type.IsRefLikeType || x.IsEmpty
                ? hasGenericReturn ? $"<{ResultGeneric}>" : ""
                : hasGenericReturn ? $"<{x.Type}, {ResultGeneric}>" : $"<{x.Type}>")}";

    [Pure]
    string DeclareNestedClass(string x, (int Index, string Item) y)
    {
        var choiceIndex = Symbols.Count + (MutablePublicly is not null).ToByte();
        var isChoiceClass = y.Index == choiceIndex;
        var isVariantClass = !isChoiceClass && (MutablePublicly is null || y.Index != choiceIndex - 1);

        return CSharp(
            $$"""
                  [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
                  {{(isChoiceClass ? "private" : "internal")}} {{(y.Index is 0 ? "sealed" : "static")
                  }} class {{y.Item}}{{(isVariantClass ? $"<T{y.Item}Discard>" : "")
                  }}{{(y.Index is 0 ? " : global::System.Attribute" : "")}}
                  {{{(y.Index is 0 ? "" : $"\n{x.SplitLines().Select(x => $"    {x}").Conjoin("\n")}")}}
                  }
              """
        );
    }

    [Pure]
    string DeclareFieldWithExplicitOffset(MemberSymbol x) =>
        CSharp(
            $"""
                     [global::System.Runtime.InteropServices.FieldOffsetAttribute(0)]
                     internal {x.Type} {FieldName(x)};
             """
        );

    [Pure]
    string Describe(MemberSymbol x) =>
        $"the {XmlName} {Named.Keyword()} with the variant {See(x)} of type {XmlTypeName(x.Type)}";

    [Pure]
    string Equality(MemberSymbol x) =>
        x.IsEmpty ? CSharp("true") :
        x.IsOperatorEquatable ? CSharp($"{PrefixCast(x, "left.")} == {PrefixCast(x, "right.")}") :
        x.IsInterfaceEquatable ? CSharp($"{PrefixCast(x, "left.")}.Equals({PrefixCast(x, "right.")})") :
        CSharp("false");

    [Pure]
    string FieldName(MemberSymbol x) =>
        Members.Contains(x) ? x.Name : $"_{x.Name.Nth(0)?.ToLower()}{x.Name.Nth(1..)}";

    [Pure]
    string Opposite(MemberSymbol x) =>
        Symbols.TrySingle(y => x != y, out var other) && !other.IsEmpty
            ? CSharp(
                $"""

                         [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, "{PropertyName(other)}")]
                 """
            )
            : "";

    [Pure]
    string PrefixCast(MemberSymbol x, string memberAccess = "") =>
        x.IsEmpty ? SingleEmpty(Symbols).Contains(x) ? CSharp("default") : CSharp($"default({x.Type})") :
        Prefix(x) is not ReferenceField and var prefix ? $"{memberAccess}{prefix}{x.NullableSuppression}" :
        $"(({x.Type}){memberAccess}{ReferenceField}{x.NullableSuppression})";

    [Pure]
    string Prefix(MemberSymbol x) =>
        Members.Contains(x) ? x.Name :
        CanOverlapUnmanagedMemorySpace && Unmanaged.Contains(x) ? $"{UnmanagedField}.{FieldName(x)}" :
        CanOverlapReferenceMemorySpace && Reference.Contains(x) ? ReferenceField : FieldName(x);

    [Pure]
    string ParameterName(MemberSymbol x) => $"{FieldName(x).Nth(1..)}";

    [Pure]
    string PropertyName(MemberSymbol x) =>
        Members.Contains(x) && x.Name.TrimStart('_') is var trim
            ? $"{trim.Nth(0)?.ToUpper()}{trim.Nth(1..)}"
            : x.Name;

    [Pure]
    string See(MemberSymbol x) => x.IsEmpty ? PropertyName(x) : $"<see cref=\"{PropertyName(x)}\"/>";

    [Pure]
    string ToStringCase(MemberSymbol x) =>
        x.IsEmpty || x.Type.IsRefLikeType && x.Type.GetMembers().All(x => IsUnoriginalMethod(x, nameof(ToString)))
            ? $"\"{PropertyName(x)}\""
            : CSharp(
                $$"""
                  $"{nameof({{PropertyName(x)}})}({{{PrefixCast(x)}}{{(x.Type.IsRefLikeType ? ".ToString()" : "")}}})"
                  """
            );
}
