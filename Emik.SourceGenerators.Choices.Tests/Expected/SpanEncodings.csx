// <auto-generated/>
// ReSharper disable RedundantNameQualifier
// ReSharper disable once CheckNamespace
#nullable enable
#pragma warning disable
namespace Emik
{
    namespace SourceGenerators
    {
        namespace Choices
        {
            namespace Tests
            {
                /// <inheritdoc cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/>
                /// <remarks>
                ///     <para>
                ///         The type <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/>
                ///         is a mutable disjoint union representing the following variants:
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
                ///                             Factories
                ///                         </description>
                ///                     </item>
                ///                 </list>
                ///             </term>
                ///         </listheader>
                ///         <item>
                ///             <term>
                ///                 <see cref="Utf8"/> <see langword="as"/> <see cref="System.Span{T}"/>
                ///                 <list type="bullet">
                ///                     <item>
                ///                         <description>
                ///                             <see cref="IsUtf8"/>
                ///                         </description>
                ///                     </item>
                ///                     <item>
                ///                         <description>
                ///                             <list type="number">
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="OfUtf8(System.Span{byte})"/>
                ///                                     </description>
                ///                                 </item>
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings(System.Span{byte})"/>
                ///                                     </description>
                ///                                 </item>
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="op_Implicit(System.Span{byte})"/>
                ///                                     </description>
                ///                                 </item>
                ///                             </list>
                ///                         </description>
                ///                     </item>
                ///                 </list>
                ///             </term>
                ///         </item>
                ///         <item>
                ///             <term>
                ///                 <see cref="Utf16"/> <see langword="as"/> <see cref="System.Span{T}"/>
                ///                 <list type="bullet">
                ///                     <item>
                ///                         <description>
                ///                             <see cref="IsUtf16"/>
                ///                         </description>
                ///                     </item>
                ///                     <item>
                ///                         <description>
                ///                             <list type="number">
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="OfUtf16(System.Span{char})"/>
                ///                                     </description>
                ///                                 </item>
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings(System.Span{char})"/>
                ///                                     </description>
                ///                                 </item>
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="op_Implicit(System.Span{char})"/>
                ///                                     </description>
                ///                                 </item>
                ///                             </list>
                ///                         </description>
                ///                     </item>
                ///                 </list>
                ///             </term>
                ///         </item>
                ///     </list>
                /// </remarks>
                [global::System.Runtime.InteropServices.StructLayoutAttribute(global::System.Runtime.InteropServices.LayoutKind.Auto)]
                partial struct SpanEncodings
                {
                    /// <summary>
                    /// Explicit side effect delegate for the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf8"/> of type <see cref="System.Span{T}"/> due to it being a by-ref like type.
                    /// </summary>
                    /// <param name="utf8">The referenced value.</param>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public delegate void Utf8Handler(System.Span<byte> utf8);

                    /// <summary>
                    /// Explicit mapper delegate for the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf8"/> of type <see cref="System.Span{T}"/> due to it being a by-ref like type.
                    /// </summary>
                    /// <typeparam name="TMappingResult">The type of value to return.</typeparam>
                    /// <param name="utf8">The referenced value.</param>
                    /// <returns>The result of the mapping.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public delegate TMappingResult Utf8Handler<out TMappingResult>(System.Span<byte> utf8);

                    /// <summary>
                    /// Explicit side effect delegate for the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf16"/> of type <see cref="System.Span{T}"/> due to it being a by-ref like type.
                    /// </summary>
                    /// <param name="utf16">The referenced value.</param>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public delegate void Utf16Handler(System.Span<char> utf16);

                    /// <summary>
                    /// Explicit mapper delegate for the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf16"/> of type <see cref="System.Span{T}"/> due to it being a by-ref like type.
                    /// </summary>
                    /// <typeparam name="TMappingResult">The type of value to return.</typeparam>
                    /// <param name="utf16">The referenced value.</param>
                    /// <returns>The result of the mapping.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public delegate TMappingResult Utf16Handler<out TMappingResult>(System.Span<char> utf16);

                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    private byte _discriminator;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf8"/> of type <see cref="System.Span{T}"/>.
                    /// </summary>
                    /// <param name="utf8">The variant.</param>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public SpanEncodings(System.Span<byte> utf8)
                    {
                        _discriminator = 0;
                        _utf8 = utf8;
                    }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf16"/> of type <see cref="System.Span{T}"/>.
                    /// </summary>
                    /// <param name="utf16">The variant.</param>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public SpanEncodings(System.Span<char> utf16)
                    {
                        _discriminator = 1;
                        _utf16 = utf16;
                    }

                    /// <summary>
                    /// Gets the value determining if the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> is the variant <see cref="Utf8"/> of type <see cref="System.Span{T}"/>.
                    /// </summary>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public readonly bool IsUtf8
                    {
                        [global::System.Diagnostics.Contracts.PureAttribute]
                        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(true, "Utf8")]
                        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(false, "Utf16")]
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        get => _discriminator is 0;
                    }

                    /// <summary>
                    /// Gets the value determining if the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> is the variant <see cref="Utf16"/> of type <see cref="System.Span{T}"/>.
                    /// </summary>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public readonly bool IsUtf16
                    {
                        [global::System.Diagnostics.Contracts.PureAttribute]
                        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(true, "Utf16")]
                        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(false, "Utf8")]
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        get => _discriminator is 1;
                    }

                    /// <summary>
                    /// Gets or sets the <see cref="System.Span{T}"/> variant.
                    /// </summary>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public System.Span<byte> Utf8
                    {
                        [global::System.Diagnostics.Contracts.PureAttribute]
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        readonly get => _discriminator is 0 ? _utf8 : default;
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        set
                        {
                            _discriminator = 0;
                            _utf8 = value;
                        }
                    }

                    /// <summary>
                    /// Gets or sets the <see cref="System.Span{T}"/> variant.
                    /// </summary>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public System.Span<char> Utf16
                    {
                        [global::System.Diagnostics.Contracts.PureAttribute]
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        readonly get => _discriminator is 1 ? _utf16 : default;
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        set
                        {
                            _discriminator = 1;
                            _utf16 = value;
                        }
                    }

                    /// <summary>
                    /// Implicitly converts the <see cref="System.Span{T}"/> parameter to the union.
                    /// </summary>
                    /// <param name="utf8">The parameter to pass onto the constructor.</param>
                    /// <returns>The union containing the parameter <paramref name="utf8"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static implicit operator global::Emik.SourceGenerators.Choices.Tests.SpanEncodings(System.Span<byte> utf8)
                        => new global::Emik.SourceGenerators.Choices.Tests.SpanEncodings(utf8);

                    /// <summary>
                    /// Explicitly converts the union to the target type <see cref="System.Span{T}"/>.
                    /// </summary>
                    /// <param name="x">The union to access its property.</param>
                    /// <returns>The getter of the union <paramref name="x"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static explicit operator System.Span<byte>(global::Emik.SourceGenerators.Choices.Tests.SpanEncodings x)
                        => x.Utf8;

                    /// <summary>
                    /// Implicitly converts the <see cref="System.Span{T}"/> parameter to the union.
                    /// </summary>
                    /// <param name="utf16">The parameter to pass onto the constructor.</param>
                    /// <returns>The union containing the parameter <paramref name="utf16"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static implicit operator global::Emik.SourceGenerators.Choices.Tests.SpanEncodings(System.Span<char> utf16)
                        => new global::Emik.SourceGenerators.Choices.Tests.SpanEncodings(utf16);

                    /// <summary>
                    /// Explicitly converts the union to the target type <see cref="System.Span{T}"/>.
                    /// </summary>
                    /// <param name="x">The union to access its property.</param>
                    /// <returns>The getter of the union <paramref name="x"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static explicit operator System.Span<char>(global::Emik.SourceGenerators.Choices.Tests.SpanEncodings x)
                        => x.Utf16;

                    /// <summary>
                    /// Creates a new instance of the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf8"/> of type <see cref="System.Span{T}"/>.
                    /// </summary>
                    /// <param name="utf8">The value to pass into the type.</param>
                    /// <returns>The union containing the parameter <paramref name="utf8"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static global::Emik.SourceGenerators.Choices.Tests.SpanEncodings OfUtf8(System.Span<byte> utf8)
                        => new global::Emik.SourceGenerators.Choices.Tests.SpanEncodings(utf8);

                    /// <summary>
                    /// Creates a new instance of the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf16"/> of type <see cref="System.Span{T}"/>.
                    /// </summary>
                    /// <param name="utf16">The value to pass into the type.</param>
                    /// <returns>The union containing the parameter <paramref name="utf16"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static global::Emik.SourceGenerators.Choices.Tests.SpanEncodings OfUtf16(System.Span<char> utf16)
                        => new global::Emik.SourceGenerators.Choices.Tests.SpanEncodings(utf16);

                    /// <summary>
                    /// Determines whether the left-hand side is equal to the right.
                    /// </summary>
                    /// <param name="left">The left-hand side.</param>
                    /// <param name="right">The right-hand side.</param>
                    /// <returns>
                    /// The value determining whether the parameter <paramref name="left"/>
                    /// is equal to the parameter <paramref name="right"/>.
                    /// </returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static bool operator ==(Emik.SourceGenerators.Choices.Tests.SpanEncodings left, Emik.SourceGenerators.Choices.Tests.SpanEncodings right)
                        => left._discriminator == right._discriminator &&
                            left._discriminator switch
                            {
                                0 => false,
                                _ => false,
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
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static bool operator !=(Emik.SourceGenerators.Choices.Tests.SpanEncodings left, Emik.SourceGenerators.Choices.Tests.SpanEncodings right)
                        => !(left == right);

                    /// <summary>
                    /// Determines whether the left-hand side is greater than the right.
                    /// </summary>
                    /// <param name="left">The left-hand side.</param>
                    /// <param name="right">The right-hand side.</param>
                    /// <returns>
                    /// The value determining whether the parameter <paramref name="left"/>
                    /// is greater than the parameter <paramref name="right"/>.
                    /// </returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static bool operator >(Emik.SourceGenerators.Choices.Tests.SpanEncodings left, Emik.SourceGenerators.Choices.Tests.SpanEncodings right)
                        => left._discriminator > right._discriminator ||
                            left._discriminator == right._discriminator &&
                            left._discriminator switch
                            {
                                0 => false,
                                _ => false,
                            };

                    /// <summary>
                    /// Determines whether the left-hand side is greater than or equal to the right.
                    /// </summary>
                    /// <param name="left">The left-hand side.</param>
                    /// <param name="right">The right-hand side.</param>
                    /// <returns>
                    /// The value determining whether the parameter <paramref name="left"/>
                    /// is greater than or equal to the parameter <paramref name="right"/>.
                    /// </returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static bool operator >=(Emik.SourceGenerators.Choices.Tests.SpanEncodings left, Emik.SourceGenerators.Choices.Tests.SpanEncodings right)
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
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static bool operator <(Emik.SourceGenerators.Choices.Tests.SpanEncodings left, Emik.SourceGenerators.Choices.Tests.SpanEncodings right)
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
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static bool operator <=(Emik.SourceGenerators.Choices.Tests.SpanEncodings left, Emik.SourceGenerators.Choices.Tests.SpanEncodings right)
                        => right >= left;

                    /// <inheritdoc cref="object.Equals(object)"/>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public override bool Equals(object? obj)
                        => false;

                    /// <inheritdoc />
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly bool Equals(Emik.SourceGenerators.Choices.Tests.SpanEncodings other)
                        => this == other;

                    /// <inheritdoc cref="IComparable.CompareTo(object)"/>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly int CompareTo(object? obj)
                        => -1;

                    /// <inheritdoc />
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly int CompareTo(Emik.SourceGenerators.Choices.Tests.SpanEncodings other)
                        => Equals(other) ? 0 : -1;

                    /// <inheritdoc />
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly override int GetHashCode()
                        => unchecked(_discriminator * 13687) ^
                        (_discriminator switch
                        {
                            0 => _utf8.GetHashCode(),
                            _ => _utf16.GetHashCode(),
                        });

                    /// <inheritdoc />
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly override string ToString()
                        => _discriminator switch
                        {
                            0 => $"{nameof(Utf8)}({_utf8.ToString()})",
                            _ => $"{nameof(Utf16)}({_utf16.ToString()})",
                        };

                    /// <summary>
                    /// Invokes the callback based on current variance.
                    /// </summary>
                    /// <param name="onUtf8">The callback to use when the contract of the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf8"/> of type <see cref="System.Span{T}"/> is held.</param>
                    /// <param name="onUtf16">The callback to use when the contract of the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf16"/> of type <see cref="System.Span{T}"/> is held.</param>
                    /// <returns>Itself.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly Emik.SourceGenerators.Choices.Tests.SpanEncodings Map(
                        Utf8Handler? onUtf8 = null,
                        Utf16Handler? onUtf16 = null
                    )
                    {
                        switch (_discriminator)
                        {
                            case 0:
                                onUtf8?.Invoke(_utf8);
                                return this;
                            default:
                                onUtf16?.Invoke(_utf16);
                                return this;
                        }
                    }

                    /// <summary>
                    /// Maps each variant to <typeparamref name="TMappingResult"/>.
                    /// </summary>
                    /// <typeparam name="TMappingResult">The resulting type from the mapping.</typeparam>
                    /// <param name="onUtf8">The callback to use when the contract of the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf8"/> of type <see cref="System.Span{T}"/> is held.</param>
                    /// <param name="onUtf16">The callback to use when the contract of the <see cref="Emik.SourceGenerators.Choices.Tests.SpanEncodings"/> struct with the variant <see cref="Utf16"/> of type <see cref="System.Span{T}"/> is held.</param>
                    /// <returns>
                    /// The resulting value from one of the parameters based on the current state of the object.
                    /// </returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly TMappingResult Map<TMappingResult>(
                        Utf8Handler<TMappingResult> onUtf8,
                        Utf16Handler<TMappingResult> onUtf16
                    )
                        => _discriminator switch
                        {
                            0 => onUtf8(_utf8),
                            _ => onUtf16(_utf16),
                        };

                    /// <inheritdoc cref="System.Span{T}.Clear()"/>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly void Clear()
                    {
                        switch (_discriminator)
                        {
                            case 0:
                                _utf8.Clear();
                                break;
                            default:
                                _utf16.Clear();
                                break;
                        }
                    }

                    /// <inheritdoc cref="System.Span{T}.IsEmpty"/>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public readonly bool IsEmpty
                    {
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        get => _discriminator switch
                        {
                            0 => _utf8.IsEmpty,
                            _ => _utf16.IsEmpty,
                        };
                    }

                    /// <inheritdoc cref="System.Span{T}.Length"/>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public readonly int Length
                    {
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        get => _discriminator switch
                        {
                            0 => _utf8.Length,
                            _ => _utf16.Length,
                        };
                    }
                }
            }
        }
    }
}
