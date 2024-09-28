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
                /// <inheritdoc cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/>
                /// <remarks>
                ///     <para>
                ///         The type <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/>
                ///         is an immutable disjoint union representing the following variants:
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
                ///                 <see cref="Ok"/> <see langword="as"/> <typeparamref name="TOk"/>
                ///                 <list type="bullet">
                ///                     <item>
                ///                         <description>
                ///                             <see cref="IsOk"/>
                ///                         </description>
                ///                     </item>
                ///                     <item>
                ///                         <description>
                ///                             <list type="number">
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="OfOk(TOk?)"/>
                ///                                     </description>
                ///                                 </item>
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}(TOk?)"/>
                ///                                     </description>
                ///                                 </item>
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="op_Implicit(TOk?)"/>
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
                ///                 <see cref="Err"/> <see langword="as"/> <typeparamref name="TErr"/>
                ///                 <list type="bullet">
                ///                     <item>
                ///                         <description>
                ///                             <see cref="IsErr"/>
                ///                         </description>
                ///                     </item>
                ///                     <item>
                ///                         <description>
                ///                             <list type="number">
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="OfErr(TErr?)"/>
                ///                                     </description>
                ///                                 </item>
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}(TErr?)"/>
                ///                                     </description>
                ///                                 </item>
                ///                                 <item>
                ///                                     <description>
                ///                                         <see cref="op_Implicit(TErr?)"/>
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
                partial record struct Result<TOk, TErr> :
                    global::System.IComparable,
                    global::System.IComparable<object>,
                    global::System.IComparable<global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr>>,
#if NET7_0_OR_GREATER
                    global::System.Numerics.IComparisonOperators<global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr>, global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr>, bool>,
#endif
                    global::System.IEquatable<object>,
                    global::System.IEquatable<global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr>>
                {
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    private readonly byte _discriminator;

                    /// <summary>
                    /// Initializes a new instance of the <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/> record struct with the variant <see cref="Ok"/> of type <typeparamref name="TOk"/>.
                    /// </summary>
                    /// <param name="ok">The variant.</param>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public Result(TOk? ok)
                    {
                        _discriminator = 0;
                        _ok = ok;
                    }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/> record struct with the variant <see cref="Err"/> of type <typeparamref name="TErr"/>.
                    /// </summary>
                    /// <param name="err">The variant.</param>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public Result(TErr? err)
                    {
                        _discriminator = 1;
                        _err = err;
                    }

                    /// <summary>
                    /// Gets the value determining if the <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/> is the variant <see cref="Ok"/> of type <typeparamref name="TOk"/>.
                    /// </summary>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public readonly bool IsOk
                    {
                        [global::System.Diagnostics.Contracts.PureAttribute]
                        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(true, "Ok")]
                        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(false, "Err")]
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        get => _discriminator is 0;
                    }

                    /// <summary>
                    /// Gets the value determining if the <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/> is the variant <see cref="Err"/> of type <typeparamref name="TErr"/>.
                    /// </summary>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public readonly bool IsErr
                    {
                        [global::System.Diagnostics.Contracts.PureAttribute]
                        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(true, "Err")]
                        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(false, "Ok")]
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        get => _discriminator is 1;
                    }

                    /// <summary>
                    /// Gets the <typeparamref name="TOk"/> variant.
                    /// </summary>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public readonly TOk? Ok
                    {
                        [global::System.Diagnostics.Contracts.PureAttribute]
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        get => _discriminator is 0 ? _ok : default;
                    }

                    /// <summary>
                    /// Gets the <typeparamref name="TErr"/> variant.
                    /// </summary>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    public readonly TErr? Err
                    {
                        [global::System.Diagnostics.Contracts.PureAttribute]
                        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                        get => _discriminator is 1 ? _err : default;
                    }

                    /// <summary>
                    /// Implicitly converts the <typeparamref name="TOk"/> parameter to the union.
                    /// </summary>
                    /// <param name="ok">The parameter to pass onto the constructor.</param>
                    /// <returns>The union containing the parameter <paramref name="ok"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static implicit operator global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr>(TOk ok)
                        => new global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr>(ok);

                    /// <summary>
                    /// Explicitly converts the union to the target type <typeparamref name="TOk"/>.
                    /// </summary>
                    /// <param name="x">The union to access its property.</param>
                    /// <returns>The getter of the union <paramref name="x"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static explicit operator TOk?(global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> x)
                        => x.Ok;

                    /// <summary>
                    /// Implicitly converts the <typeparamref name="TErr"/> parameter to the union.
                    /// </summary>
                    /// <param name="err">The parameter to pass onto the constructor.</param>
                    /// <returns>The union containing the parameter <paramref name="err"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static implicit operator global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr>(TErr err)
                        => new global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr>(err);

                    /// <summary>
                    /// Explicitly converts the union to the target type <typeparamref name="TErr"/>.
                    /// </summary>
                    /// <param name="x">The union to access its property.</param>
                    /// <returns>The getter of the union <paramref name="x"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static explicit operator TErr?(global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> x)
                        => x.Err;

                    /// <summary>
                    /// Creates a new instance of the <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/> record struct with the variant <see cref="Ok"/> of type <typeparamref name="TOk"/>.
                    /// </summary>
                    /// <param name="ok">The value to pass into the type.</param>
                    /// <returns>The union containing the parameter <paramref name="ok"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> OfOk(TOk? ok)
                        => new global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr>(ok);

                    /// <summary>
                    /// Creates a new instance of the <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/> record struct with the variant <see cref="Err"/> of type <typeparamref name="TErr"/>.
                    /// </summary>
                    /// <param name="err">The value to pass into the type.</param>
                    /// <returns>The union containing the parameter <paramref name="err"/>.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public static global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> OfErr(TErr? err)
                        => new global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr>(err);

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
                    public static bool operator >(Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> left, Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> right)
                        => left._discriminator > right._discriminator ||
                            left._discriminator == right._discriminator &&
                            left._discriminator switch
                            {
                                0 => global::System.Collections.Generic.Comparer<TOk?>.Default.Compare(left._ok!, right._ok!) > 0,
                                _ => global::System.Collections.Generic.Comparer<TErr?>.Default.Compare(left._err!, right._err!) > 0,
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
                    public static bool operator >=(Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> left, Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> right)
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
                    public static bool operator <(Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> left, Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> right)
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
                    public static bool operator <=(Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> left, Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> right)
                        => right >= left;

                    /// <inheritdoc />
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly bool Equals(Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> other)
                        => this == other;

                    /// <inheritdoc cref="IComparable.CompareTo(object)"/>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly int CompareTo(object? obj)
                        => obj is null ? 1 : obj is global::Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> x ? CompareTo(x) : -1;

                    /// <inheritdoc />
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly int CompareTo(Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> other)
                        => Equals(other) ? 0 : -1;

                    /// <inheritdoc />
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly override int GetHashCode()
                        => unchecked(_discriminator * 17903) ^
                        (_discriminator switch
                        {
                            0 => _ok!.GetHashCode(),
                            _ => _err!.GetHashCode(),
                        });

                    /// <inheritdoc />
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Diagnostics.Contracts.PureAttribute]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly override string ToString()
                        => _discriminator switch
                        {
                            0 => $"{nameof(Ok)}({_ok!})",
                            _ => $"{nameof(Err)}({_err!})",
                        };

                    /// <summary>
                    /// Invokes the callback based on current variance.
                    /// </summary>
                    /// <param name="onOk">The callback to use when the contract of the <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/> record struct with the variant <see cref="Ok"/> of type <typeparamref name="TOk"/> is held.</param>
                    /// <param name="onErr">The callback to use when the contract of the <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/> record struct with the variant <see cref="Err"/> of type <typeparamref name="TErr"/> is held.</param>
                    /// <returns>Itself.</returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly Emik.SourceGenerators.Choices.Tests.Result<TOk, TErr> Map(
                        global::System.Action<TOk?>? onOk = null,
                        global::System.Action<TErr?>? onErr = null
                    )
                    {
                        switch (_discriminator)
                        {
                            case 0:
                                onOk?.Invoke(_ok!);
                                return this;
                            default:
                                onErr?.Invoke(_err!);
                                return this;
                        }
                    }

                    /// <summary>
                    /// Maps each variant to <typeparamref name="TMappingResult"/>.
                    /// </summary>
                    /// <typeparam name="TMappingResult">The resulting type from the mapping.</typeparam>
                    /// <param name="onOk">The callback to use when the contract of the <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/> record struct with the variant <see cref="Ok"/> of type <typeparamref name="TOk"/> is held.</param>
                    /// <param name="onErr">The callback to use when the contract of the <see cref="Emik.SourceGenerators.Choices.Tests.Result{TOk, TErr}"/> record struct with the variant <see cref="Err"/> of type <typeparamref name="TErr"/> is held.</param>
                    /// <returns>
                    /// The resulting value from one of the parameters based on the current state of the object.
                    /// </returns>
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.0.0")]
                    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
                    public readonly TMappingResult Map<TMappingResult>(
                        global::System.Func<TOk?, TMappingResult> onOk,
                        global::System.Func<TErr?, TMappingResult> onErr
                    )
                        => _discriminator switch
                        {
                            0 => onOk(_ok!),
                            _ => onErr(_err!),
                        };
                }
            }
        }
    }
}
