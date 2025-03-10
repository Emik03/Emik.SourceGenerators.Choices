// SPDX-License-Identifier: MPL-2.0
namespace Emik.SourceGenerators.Choices;

sealed partial record Scaffolder(
    INamedTypeSymbol Named,
    ImmutableArray<MemberSymbol> Symbols,
    ImmutableArray<MemberSymbol> SingleEmpty,
    bool? MutablePublicly,
    bool? FullyPolyfillAttributes
)
{
    [StringSyntax("C#")]
    public const string ResultGeneric = "TMappingResult";

    const int MinimumBoxedSize = 2, MinimumExplicitStructSize = 2, TupleGenericLimit = 8;

    [StringSyntax("C#")]
    const string
        AggressiveInlining = "[global::System.Runtime.CompilerServices.MethodImplAttribute(256)]",
        DiscriminatorField = "_discriminator",
        DiscriminatorProperty = "Discriminator",
        HideFromEditor =
            "[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]",
        Pure = "[global::System.Diagnostics.Contracts.PureAttribute]",
        ReadOnly = "readonly ",
        ReferenceField = "_reference",
        Suppression = "#pragma warning disable\n",
        UnmanagedField = "_unmanaged",
        UsedImplicitly = nameof(UsedImplicitly);

    static readonly ConcurrentDictionary<string, short> s_names = new(StringComparer.Ordinal);

    [ValueRange(Primes.Min, Primes.MaxInt16)]
    readonly short _hash = s_names.GetOrAdd(Named.GetFullyQualifiedName(), x => Primes.Index(x.GetDjb2HashCode()));

    public Scaffolder(
        INamedTypeSymbol named,
        ImmutableArray<MemberSymbol> symbols,
        bool? mutablePublicly,
        bool? fullyPolyfillAttributes
    )
        : this(named, symbols, FindSingleEmpty(symbols), mutablePublicly, fullyPolyfillAttributes) { }

    public Scaffolder(Raw raw)
        : this(raw.Named, raw.Fields, raw.MutablePublicly, raw.FullyPolyfillAttributes) { }

    [Pure]
    public GeneratedSource Result => (HintName, Source);

    [Pure]
    bool CanOverlapReferenceMemorySpace =>
        CanReserveNull || Reference.Omit(x => Members.Any(x.ReferenceEquals)).Skip(MinimumBoxedSize - 1).Any();

    [Pure]
    bool CanOverlapUnmanagedMemorySpace =>
        Unmanaged.Omit(x => Members.Any(x.ReferenceEquals)).Skip(MinimumExplicitStructSize - 1).Any();

    [Pure]
    bool CanReserveNull =>
        Symbols is [{ IsEmpty: true }, { IsReference: true }] or [{ IsReference: true }, { IsEmpty: true }];

    [Pure]
    bool UsesPrimaryConstructor => Symbols is [{ Symbol: IParameterSymbol }, ..];

    [Pure]
    string AutoIfStruct =>
        Named.IsValueType && Named.GetAttributes().All(x => x.AttributeClass?.Name is not nameof(StructLayoutAttribute))
            ? CSharp($"[global::{typeof(StructLayoutAttribute)}(global::{typeof(LayoutKind)}.Auto)]\n")
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
    string SymbolsUnsafe { get; } = Symbols.FirstOrDefault(x => !string.IsNullOrEmpty(x.Unsafe)).Unsafe;

    [Pure]
    string VirtualIfNonSealedRecordClass { get; } =
        Named is { IsRecord: true, IsSealed: false, IsValueType: false } ? CSharp("virtual ") : "";

    [Pure]
    string DeclareType =>
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
                  $"""
                   ///         <item>
                   ///             <term>
                   ///                 {x.XmlName} <see langword="as"/> {XmlTypeName(x.Type)}
                   ///                 <list type="bullet">
                   ///                     <item>
                   ///                         <description>
                   ///                             <see cref="Is{x.PropertyName}"/>
                   ///                         </description>
                   ///                     </item>
                   ///                     <item>
                   ///                         <description>
                   {(SkipOperator(x) ?
                       $"""///                             <see cref="Of{x.PropertyName}({XmlEscape(x.Type)})"/>""" : $"""
                            ///                             <list type="number">
                            ///                                 <item>
                            ///                                     <description>
                            ///                                         <see cref="Of{x.PropertyName}({XmlEscape(x.Type, true)})"/>
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
                            """)}
                   ///                         </description>
                   ///                     </item>
                   ///                 </list>
                   ///             </term>
                   ///         </item>
                   """)
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
          }}{{DeclareImplicitlyUsingParametersProperty
          }}{{Symbols.Select(DeclareOperators).Conjoin("")
          }}{{Symbols.Select(DeclareFactory).Conjoin("")
          }}{{DeclareInterfaceImplementations
          }}{{DeclareMappers
          }}{{DeclareForwarders}}
          }
          """;

    [Pure]
    string DeclarePolyfillAttributes =>
        FullyPolyfillAttributes is not null
            ? CSharp(
                $"""
                     {Annotation}
                 {(FullyPolyfillAttributes is true ? [nameof(Emik)] : Array.Empty<string>())
                    .Append(ExtendingGenerator.Choice)
                    .Concat(MutablePublicly switch
                     {
                         true => nameof(Accessibility.Public), false => nameof(Accessibility.Private), null => null,
                     } is { } mutablePublicly ? [mutablePublicly] : [])
                    .Select((x, i) => ((bool?)null, x, i is 0))
                    .Concat(Symbols.Select(x => ((bool?)x.Type.IsRefLikeType, x.Name, false)))
                    .Reverse()
                    .Index()
                    .Aggregate("", DeclareNestedClass)}


                 """
            )
            : "";

    [Pure]
    public string DeclareImplicitlyUsingParametersProperty =>
        UsesPrimaryConstructor && Members.All(x => x.Name is not UsedImplicitly)
            ? CSharp(
                $"""
                     /// <summary>This property exists solely to suppress lints regarding unused parameters.</summary>
                     {HideFromEditor}
                     {SymbolsUnsafe}bool {UsedImplicitly} => {Symbols.Skip(1).Select(x => $"{x.ParameterName} is var _").Conjoin(" && ")};


                 """
            )
            : "";

    [Pure]
    string DeclareInterfaces =>
        Named.IsRefLikeType
            ? ""
            : CSharp(
                $"""
                  :
                     global::{typeof(IComparable)},
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
        new IntersectedInterfaces(Symbols, Named.IsReadOnly, Named.IsRecord).Set.Select(x => $",\n    {x}").Conjoin("");

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
                             .Index()
                             .Select(x => $"{(x.Index == Symbols.Length - 1 ? "_" :
                                 x.Item.Item.IsEmpty ? CSharp("null") :
                                 x.Item.Item.NotNullableAnnotated)} => {x.Item.Index},")
                             .Conjoin("\n            ")}}
                      };
                      {{Pure}}
                      {{AggressiveInlining}}
                      set { /* Intentionally left blank. */ }
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
                      public static {{SymbolsUnsafe}}bool operator ==({{NullableName}} left, {{NullableName}} right)
                          => {{(Named.IsReferenceType ? "left is null ? right is null : right is not null &&\n            " : "")
                          }}left.{{Discriminator}} == right.{{Discriminator}} &&
                              left.{{Discriminator}} switch
                              {
                                  {{Symbols
                                     .Select((x, i) => $"{(i == Symbols.Length - 1 ? "_" : i)} => {Equality(x)},")
                                     .Conjoin("\n                ")}}
                              };

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
                      [global::{{typeof(StructLayoutAttribute)}}(global::{{typeof(LayoutKind)}}.Explicit)]
                      partial struct Unmanaged
                      {
                  {{Unmanaged.Select(x => x.UnmanagedFieldDeclaration).Conjoin("\n\n")}}
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
                     private {PrivatelyReadOnly}{(Common is null ? "object" : new MemberSymbol(Common, "").NullableAnnotated)
                     } {ReferenceField}{(UsesPrimaryConstructor && !Symbols[0].IsEmpty && Reference.Contains(Symbols[0]) ? CSharp($" = {Symbols[0].ParameterName}") : "")};


                 """
            )
            : Reference.Select(DeclareField).Conjoin("");

    [Pure]
    string DeclareUnmanagedFields =>
        CanOverlapUnmanagedMemorySpace
            ? CSharp(
                $"""
                     {Annotation}
                     private {PrivatelyReadOnly}{(UsesPrimaryConstructor && Unmanaged.Contains(Symbols[0]) ? Symbols[0].Unsafe : "")
                     }Unmanaged {UnmanagedField}{(UsesPrimaryConstructor && !Symbols[0].IsEmpty ? CSharp($" = new Unmanaged() {{ {Symbols[0].FieldName
                     } = {Symbols[0].ParameterName} }}") : "")};


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
                  public static {{SymbolsUnsafe}}bool operator >({{NullableName}} left, {{NullableName}} right)
                      => {{(Named.IsReferenceType ? "left is not null &&\n            (right is null ||\n            " : "")
                      }}left.{{Discriminator}} > right.{{Discriminator}} ||
                          left.{{Discriminator}} == right.{{Discriminator}} &&
                          left.{{Discriminator}} switch
                          {
                              {{Symbols
                                 .Select((x, i) => $"{(i == Symbols.Length - 1 ? "_" : i)} => {Comparison(x)},")
                                 .Conjoin("\n                ")}}
                          }{{(Named.IsReferenceType ? ")" : "")}};

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
                  public {{ReadOnlyIfStruct}}{{SymbolsUnsafe}}override int GetHashCode()
                      => unchecked({{Discriminator}} * {{_hash}}) ^
                      ({{Discriminator}} switch
                      {
                          {{Symbols
                             .Select((x, i) => $"{(i == Symbols.Length - 1 ? "_" : i)} => {
                                 (x.IsEmpty || x.Type.IsRefLikeType && x.Type.GetMembers().All(x => IsUnoriginalMethod(x, nameof(GetHashCode)))
                                     ? "0" : x.Type is IPointerTypeSymbol ? $"(int)(nint){PrefixCast(x)}" : $"{PrefixCast(x)}.GetHashCode()")},")
                             .Conjoin("\n            ")}}
                      });

                  /// <inheritdoc />
                  {{Annotation}}
                  {{Pure}}
                  {{AggressiveInlining}}
                  public {{ReadOnlyIfStruct}}{{SymbolsUnsafe}}override string ToString()
                      => {{Discriminator}} switch
                      {
                          {{Symbols.Select((x, i) => $"{(i == Symbols.Length - 1 ? "_" : i)} => {ToStringCase(x)},").Conjoin("\n            ")}}
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
                     .Select(x => $"""<param name="on{x.PropertyName}">The callback to use when the contract of {Describe(x)} is held.</param>""")
                     .Conjoin("\n    /// ")
                  }}
                  /// <returns>Itself.</returns>
                  {{Annotation}}
                  {{AggressiveInlining}}
                  public {{ReadOnlyIfStruct}}{{SymbolsUnsafe}}{{NullableName}} Map(
                      {{Symbols
                         .Select(x => $"{x.DelegateTypeName(false)}? on{x.PropertyName} = null")
                         .Conjoin(",\n        ")}}
                  )
                  {
                      switch ({{Discriminator}})
                      {
                          {{Symbols
                             .Select((x, i) => $"{(i == Symbols.Length - 1 ? "default" : $"case {i}")}:\n                on{x.PropertyName
                             }?.Invoke({(x.IsEmpty ? "" : PrefixCast(x))});\n                return this;")
                             .Conjoin("\n            ")}}
                      }
                  }

                  /// <summary>
                  /// Maps each variant to <typeparamref name="{{ResultGeneric}}"/>.
                  /// </summary>
                  /// <typeparam name="{{ResultGeneric}}">The resulting type from the mapping.</typeparam>
              {{Symbols
                 .Select(x => $"""    /// <param name="on{x.PropertyName}">The callback to use when the contract of {Describe(x)} is held.</param>""")
                 .Conjoin("\n")
              }}
                  /// <returns>
                  /// The resulting value from one of the parameters based on the current state of the object.
                  /// </returns>
                  {{Annotation}}
                  {{AggressiveInlining}}
                  public {{ReadOnlyIfStruct}}{{SymbolsUnsafe}}{{ResultGeneric}} Map<{{ResultGeneric}}>(
                      {{Symbols.Select(x => $"{x.DelegateTypeName(true)} on{x.PropertyName}").Conjoin(",\n        ")}}
                  )
                      => {{Discriminator}} switch
                      {
                          {{Symbols
                             .Select((x, i) => $"{(i == Symbols.Length - 1 ? "_" : i)} => on{x.PropertyName}({(x.IsEmpty ? "" : PrefixCast(x))}),")
                             .Conjoin("\n            ")}}
                      };{{DeclareUnderlyingValue}}
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
                     public {ReadOnlyIfStruct}{SymbolsUnsafe}{Common} GetUnderlyingValue()
                         => {(CanOverlapReferenceMemorySpace && Symbols.Length == Reference.Length
                             ? $"{(Common.SpecialType is SpecialType.System_Object ? "" : $"({Common})")}{ReferenceField}!"
                             : CSharp(
                                 $$"""
                                   {{Discriminator}} switch
                                           {
                                   {{Symbols
                                      .Select((x, i) => $"            {(i == Symbols.Length - 1 ? "_" : i)} => {PrefixCast(x)},")
                                      .Conjoin("\n")}}
                                           }
                                   """
                             ))};
                 """
            );

    [Pure]
    [field: MaybeNull]
    string Discriminator =>
        field ??= Symbols.Length != Reference.Length ||
            !CanReserveNull && Symbols.Any(x => Members.Any(x.ReferenceEquals)) ||
            Symbols.Select(x => x.Type).GroupDuplicates(RoslynComparer.Signature).Any()
                ? DiscriminatorField
                : DiscriminatorProperty;

    [Pure]
    string HintName { get; } = Named.HintName();

    [Pure]
    string Name { get; } = Named.GetFullyQualifiedName();

    [Pure]
    string NullableName { get; } = new MemberSymbol(Named, "").NullableAnnotated is var name &&
        name.StartsWith(nameof(Emik)) &&
        FullyPolyfillAttributes is true
            ? CSharp($"global::{name}")
            : name;

    [Pure]
    [field: MaybeNull]
    string Source =>
        field ??= $"{Header}{Suppression}{Named
           .ContainingWithoutGlobal()
           .FindSmallPathToNull(x => x.ContainingWithoutGlobal())
           .Aggregate(DeclareType, WrapNamespaceOrType)}\n";

    [Pure]
    string XmlName { get; } = XmlTypeName(Named);

    [Pure]
    HashSet<MemberSymbol> Members { get; } = Named.GetMembers().Select(MemberSymbol.From).Filter().ToSet();

    [Pure]
    ITypeSymbol? Common { get; } = Signature.FindCommonBaseTypes(Symbols.RemoveRange(SingleEmpty)).FirstOrDefault();

    [Pure]
    ImmutableArray<MemberSymbol> Reference { get; } =
        Symbols.Omit(x => x.IsUnmanaged).Where(x => x.IsReference).Concat(SingleEmpty).ToImmutableArray();

    [Pure]
    ImmutableArray<MemberSymbol> Rest { get; } =
        Symbols.Omit(x => x.IsUnmanaged).Omit(x => x.IsReference).Except(SingleEmpty).ToImmutableArray();

    [Pure]
    ImmutableArray<MemberSymbol> Unmanaged { get; } =
        Symbols.Where(x => x.IsUnmanaged).Omit(x => x.IsEmpty).ToImmutableArray();

    [Pure]
    public static ImmutableArray<MemberSymbol> Decouple(ImmutableArray<IFieldSymbol> fields) =>
        (fields.Length is TupleGenericLimit &&
            fields[^1].Type is INamedTypeSymbol { IsTupleType: true, IsValueType: true, TupleElements: var tuple }
                ? fields.Take(TupleGenericLimit - 1).Select(x => new MemberSymbol(x)).Concat(Decouple(tuple))
                : fields.Select(x => new MemberSymbol(x))).ToImmutableArray();

    [Pure]
    public static ImmutableArray<MemberSymbol> Instances(INamespaceOrTypeSymbol x) =>
        x.GetMembers()
           .Select(MemberSymbol.From)
           .Filter()
           .Omit(x => x.IsStatic || x.Symbol is IPropertySymbol { ExplicitInterfaceImplementations: not [] } || x.IsEq)
           .ToImmutableArray();

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
    static string DeclareNestedClass(string x, (int Index, (bool? IsRefLikeType, string Name, bool IsFirst) Item) y) =>
        CSharp(
            $$"""
                  {{HideFromEditor}}
                  {{(y.Item.IsFirst ? "private" : "internal")
                  }} {{(y.Index is 0 ? "sealed" : "static")}} class {{y.Item.Name
                  }}{{(y.Item.IsRefLikeType is not null ? $"<T{y.Item.Name}Discard>" : "")
                  }}{{(y.Index is 0 ? $" : global::{typeof(Attribute)}" : "")
                  }}{{(y.Item.IsRefLikeType is true ? CSharp($"\n        where T{y.Item.Name}Discard : allows ref struct") : "")
                  }}
                  {{{(y.Index is 0 ? "" : $"\n{x.SplitLines().Select(x => $"    {x}").Conjoin("\n")}")}}
                  }
              """
        );

    [Pure]
    static string WrapNamespaceOrType(string acc, ISymbol next) =>
        CSharp(
            $$"""
              {{(next is ITypeSymbol type ? $"partial {type.Keyword()} {next.Name
              }{(type is INamedTypeSymbol { TypeParameters: var generics and not [] } ? $"<{generics.Conjoin()}>" : "")
              }" : CSharp($"namespace {next.Name}"))}}
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
       .ToBuilder()
       .Replace('<', '{')
       .Replace('>', '}')
       .Replace("scoped ", "")
       .ToString();

    [Pure]
    static string XmlTypeName(ISymbol x, string tag = "see")
    {
        var genericTag = x is ITypeParameterSymbol ? "typeparamref" : tag;
        var attribute = x is ITypeParameterSymbol ? "name" : "cref";
        return $"<{genericTag} {attribute}=\"{XmlEscape(x)}\"/>";
    }

    [Pure]
    static ImmutableArray<MemberSymbol> FindSingleEmpty(ImmutableArray<MemberSymbol> symbols)
    {
        MemberSymbol single = default;

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var symbol in symbols)
            if (symbol.IsEmpty)
                if (single.Type is null)
                    single = symbol;
                else
                    return ImmutableArray<MemberSymbol>.Empty;

        return single.Type is null ? ImmutableArray<MemberSymbol>.Empty : ImmutableArray.Create(single);
    }

    [Pure]
    bool HasConflict(MemberSymbol x) => Symbols.Omit(y => MemberSymbol.Referential.Equals(x, y)).Any(x.TypeEquals);

    [Pure]
    bool IsNoninitial(MemberSymbol x) => Symbols.Where(x.TypeEquals).Skip(1).Any(x.ReferenceEquals);

    [Pure]
    bool SkipOperator(MemberSymbol x) =>
        x.Type is not IPointerTypeSymbol &&
        (x.Type is not ITypeParameterSymbol and { BaseType: null } ||
            x.Type.SpecialType is SpecialType.System_ValueType ||
            HasConflict(x) ||
            IsNoninitial(x));

    [Pure]
    int Inheritance((int Index, MemberSymbol Item) tuple) =>
        Symbols.Count(
            y => RoslynComparer.Signature.Equals(tuple.Item.Type, y.Type) ||
                tuple.Item.Type.AllInterfaces.Contains(y.Type, RoslynComparer.Signature) ||
                tuple.Item.Type.FindSmallPathToNull(x => x.BaseType).Contains(y.Type, RoslynComparer.Signature)
        );

    [Pure]
    string Comparison(MemberSymbol x) =>
        x.IsEmpty ? CSharp("true") :
        x.IsOperatorComparable ? CSharp($"{PrefixCast(x, "left.")} > {PrefixCast(x, "right.")}") :
        x.IsInterfaceComparable ? CSharp($"{PrefixCast(x, "left.")}.CompareTo({PrefixCast(x, "right.")}) > 0") :
        !x.Type.CanBeGeneric() ? CSharp("false") :
        CSharp(
            $"global::System.Collections.Generic.Comparer<{x.Type}>.Default.Compare({PrefixCast(x, "left.")}, {PrefixCast(x, "right.")}) > 0"
        );

    [Pure]
    string DeclareAlternativeConstructor(MemberSymbol x, bool conflict) =>
        !conflict &&
        x.Type switch
        {
            INamedTypeSymbol { IsTupleType: true, TupleElements: { Length: > 1 } e } => (true, true, Decouple(e)),
            INamedTypeSymbol type when MemberSymbol.IsSystemTuple(type) => (true, false, Instances(type)),
            _ => default,
        } is (true, var isValue, { IsDefault: false } parameters)
            ? CSharp(
                $$"""
                      /// <summary>
                      /// Initializes a new instance of {{Describe(x)}}.
                      /// </summary>
                  {{parameters
                     .Select(x => $"    /// <param name=\"{x.ParameterName}\">The {x.ParameterName} item within the variant.</param>")
                     .Conjoin("\n")}}
                      {{Annotation}}
                      {{Pure}}
                      {{AggressiveInlining}}
                      public {{x.Unsafe}}{{Named.Name}}({{parameters.Select(x => $"{x.Type} {x.ParameterName}").Conjoin()}})
                          : this({{(isValue ? nameof(ValueTuple) : nameof(Tuple))}}.{{nameof(Tuple.Create)}}({{
                              parameters.Select(x => x.ParameterName).Conjoin()}})) { }


                  """
            )
            : "";

    [Pure]
    string DeclareAlternativeFactory(MemberSymbol x, string discriminator) =>
        x.Type switch
        {
            INamedTypeSymbol { IsTupleType: true, TupleElements: var e, TypeArguments.Length: > 1 } => Decouple(e),
            INamedTypeSymbol t when MemberSymbol.IsSystemTuple(t) => Instances(t),
            _ => ImmutableArray<MemberSymbol>.Empty,
        } is not [] and var parameters
            ? CSharp(
                $"""
                     /// <summary>
                     /// Creates a new instance of {Describe(x)}.
                     /// </summary>
                 {parameters
                    .Select(x => $"    /// <param name=\"{x.ParameterName}\">The {x.ParameterName} item within the value to pass into the type.</param>")
                    .Conjoin("\n")
                 }
                     /// <returns>The union containing the parameters.</returns>
                     {Annotation}
                     {Pure}
                     {AggressiveInlining}
                     public static {Name} Of{x.PropertyName}({parameters.Select(x => $"{x.Type} {x.ParameterName}").Conjoin()})
                         => new {Name}({parameters.Select(x => x.ParameterName).Conjoin()}{discriminator});


                 """
            )
            : "";

    [Pure]
    string DeclareCheck(MemberSymbol x, int i) =>
        CSharp(
            $$"""
                  /// <summary>
                  /// Gets the value determining if the {{XmlName}} is the variant {{x.XmlName}} of type {{XmlTypeName(x.Type)}}.
                  /// </summary>
                  {{Annotation}}
                  public {{ReadOnlyIfStruct}}bool Is{{x.PropertyName}}
                  {
                      {{Pure}}{{(x.IsEmpty
                          ? Opposite(x)
                          : CSharp($"\n        [global::{typeof(MemberNotNullWhenAttribute)}(true, \"{x.PropertyName}\")]{Opposite(x)}"))}}
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
                      /// <param name="{{x.ParameterName}}">The variant.</param>{{
                          (conflict ? CSharp("\n    /// <param name=\"x\">The discriminator.</param>") : "")}}
                      {{Annotation}}
                      {{AggressiveInlining}}
                      {{(conflict ? "private" : "public")}} {{SymbolsUnsafe}}{{Named.Name}}({{x.Type}} {{x.ParameterName
                      }}{{(conflict ? ", byte x" : x.IsEmpty ? " = default" : "")
                      }}){{(UsesPrimaryConstructor ? $"\n        : this({i.For(i => $"default({Symbols[i].Type}), ").Conjoin("")
                      }{x.ParameterName}{(Symbols.Length - i - 1).For(j => $", default({Symbols[i + j + 1].Type})").Conjoin("")
                      })" : "")}}
                      {
                          {{Discriminator}} = {{(conflict ? "x" : i)}};{{(x.IsEmpty ? "" : conflict && Symbols.Where(x.TypeEquals)
                             .Aggregate((true, x), (a, x) => (a.Item1 && x.Symbol is IPropertySymbol, x)) is (true, var l)
                              ? $"\n\n        _ = {Symbols.Select((y, i) => y.ReferenceEquals(l) ? $"{y.PropertyName} = {x.ParameterName};"
                                  : y.TypeEquals(x) ? $"x is {i} ? {Prefix(y)} = {x.ParameterName} :\n            " : "").Conjoin("")}"
                              : $"\n        {Prefix(x)} = {x.ParameterName};")}}
                      }

                  {{DeclareAlternativeConstructor(x, conflict)}}
                  """
            );

    [Pure]
    string DeclareDelegate(MemberSymbol x) =>
        x is { IsEmpty: false, Type: IPointerTypeSymbol or { IsRefLikeType: true } }
            ? CSharp(
                $"""
                     /// <summary>
                     /// Explicit side effect delegate for {Describe(x)} due to it being a {(x.Type is IPointerTypeSymbol ? "pointer" : "by-ref like")} type.
                     /// </summary>
                     /// <param name="{x.ParameterName}">The referenced value.</param>
                     {Annotation}
                     public {x.Unsafe}delegate void {x.PropertyName}Handler({x.Type} {x.ParameterName});

                     /// <summary>
                     /// Explicit mapper delegate for {Describe(x)} due to it being a {(x.Type is IPointerTypeSymbol ? "pointer" : "by-ref like")} type.
                     /// </summary>
                     /// <typeparam name="{ResultGeneric}">The type of value to return.</typeparam>
                     /// <param name="{x.ParameterName}">The referenced value.</param>
                     /// <returns>The result of the mapping.</returns>
                     {Annotation}
                     public {x.Unsafe}delegate {ResultGeneric} {x.PropertyName}Handler<out {ResultGeneric}>({x.Type} {x.ParameterName});


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
                     public static {x.Unsafe}explicit operator {x.NullableAnnotated}({Name} x)
                         => x.{x.PropertyName};


                 """
            );

    [Pure]
    string DeclareFactory(MemberSymbol x, int i)
    {
        var discriminator = HasConflict(x) ? $", (byte){i}" : "";

        return CSharp(
            $"""
                 /// <summary>
                 /// Creates a new instance of {Describe(x)}.
                 /// </summary>
                 /// <param name="{x.ParameterName}">The value to pass into the type.</param>
                 /// <returns>The union containing the parameter <paramref name="{x.ParameterName}"/>.</returns>
                 {Annotation}
                 {Pure}
                 {AggressiveInlining}
                 public static {x.Unsafe}{Name} Of{x.PropertyName}({x.Type} {x.ParameterName}{(x.IsEmpty ? " = default" : "")})
                     => new {Name}({x.ParameterName}{discriminator});

             {DeclareAlternativeFactory(x, discriminator)}
             """
        );
    }

    [Pure]
    string DeclareField(MemberSymbol x) =>
        x.IsEmpty || Members.Any(x.ReferenceEquals) || IsNoninitial(x)
            ? ""
            : CSharp(
                $"""
                     private {x.Unsafe}{PrivatelyReadOnly}{x.NullableAnnotated} {
                         x.FieldName}{(UsesPrimaryConstructor && Symbols.Except(Unmanaged).Any(x.Equals)
                             ? CSharp($" = {x.ParameterName}") : "")};


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
                     /// <param name="{x.ParameterName}">The parameter to pass onto the constructor.</param>
                     /// <returns>The union containing the parameter <paramref name="{x.ParameterName}"/>.</returns>
                     {Annotation}
                     {Pure}
                     {AggressiveInlining}
                     public static {x.Unsafe}implicit operator {Name}({x.NotNullableAnnotated} {x.ParameterName})
                         => new {Name}({x.ParameterName});

                 {DeclareExplicitOperator(x)}
                 """
            );

    [Pure]
    string DeclareProperty(MemberSymbol x, int i)
    {
        if (x is { IsEmpty: true } or { Symbol: IPropertySymbol })
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
                  public {{ReadOnlyIfImmutableStruct}}{{x.Unsafe}}{{x.NullableAnnotated}} {{x.PropertyName}}
                  {
                      {{Pure}}
                      {{AggressiveInlining}}
                      {{ReadOnlyIfMutableStruct}}get => {{Discriminator}} is {{i}} ? {{cast}}{{prefix}}{{suppression}} : default;{{setter}}
                  }


              """
        );
    }

    [Pure]
    string Describe(MemberSymbol x) =>
        $"the {XmlName} {Named.Keyword()} with the variant {x.XmlName} of type {XmlTypeName(x.Type)}";

    [Pure]
    string Equality(MemberSymbol x) =>
        x.IsEmpty ? CSharp("true") :
        x.IsOperatorEquatable ? CSharp($"{PrefixCast(x, "left.")} == {PrefixCast(x, "right.")}") :
        x.IsInterfaceEquatable ? CSharp($"{PrefixCast(x, "left.")}.Equals({PrefixCast(x, "right.")})") :
        !x.Type.CanBeGeneric() ? CSharp("false") :
        CSharp(
            $"global::System.Collections.Generic.EqualityComparer<{x.Type}>.Default.Equals({PrefixCast(x, "left.")}, {PrefixCast(x, "right.")})"
        );

    [Pure]
    string Opposite(MemberSymbol x) =>
        Symbols.TrySingle(y => x != y, out var other) && !other.IsEmpty
            ? CSharp(
                $"""

                         [global::{typeof(MemberNotNullWhenAttribute)}(false, "{other.PropertyName}")]
                 """
            )
            : "";

    [Pure]
    string PrefixCast(MemberSymbol x, string memberAccess = "") =>
        x.IsEmpty ? SingleEmpty.Any(x.ReferenceEquals) ? CSharp("default") : CSharp($"default({x.Type})") :
        Prefix(x) is not ReferenceField and var prefix ? $"{memberAccess}{prefix}{x.NullableSuppression}" :
        $"(({x.Type}){memberAccess}{ReferenceField}{x.NullableSuppression})";

    [Pure]
    string Prefix(MemberSymbol x) =>
        x.Symbol is IPropertySymbol { Name: var name } ? name :
        Symbols.First(x.TypeEquals) is var y && Members.Any(y.ReferenceEquals) ? y.Name :
        CanOverlapUnmanagedMemorySpace && Unmanaged.Any(y.ReferenceEquals) ? $"{UnmanagedField}.{y.FieldName}" :
        CanOverlapReferenceMemorySpace && Reference.Any(y.ReferenceEquals) ? ReferenceField : y.FieldName;

    [Pure]
    string ToStringCase(MemberSymbol x) =>
        x.IsEmpty || x.Type.IsRefLikeType && x.Type.GetMembers().All(x => IsUnoriginalMethod(x, nameof(ToString)))
            ? $"\"{x.PropertyName}\""
            : CSharp(
                $$"""
                  $"{nameof({{x.PropertyName}})}({{{(x.Type is IPointerTypeSymbol ? "(nint)" : "")}}{{PrefixCast(x)
                  }}{{(x.Type.IsRefLikeType ? ".ToString()" : "")}}})"
                  """
            );
}
