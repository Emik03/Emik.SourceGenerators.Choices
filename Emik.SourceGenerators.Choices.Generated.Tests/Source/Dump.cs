
/// <inheritdoc cref="Test"/>
/// <remarks>
///     <para>
///         The type <see cref="Test"/>
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
///                             Factory
///                         </description>
///                     </item>
///                 </list>
///             </term>
///         </listheader>
///         <item>
///             <term>
///                 <see cref="First"/> <see langword="as"/> <see cref="System.Buffers.OperationStatus"/>
///                 <list type="bullet">
///                     <item>
///                         <description>
///                             <see cref="IsFirst"/>
///                         </description>
///                     </item>
///                     <item>
///                         <description>
///                             <see cref="OfFirst(System.Buffers.OperationStatus)"/>
///                         </description>
///                     </item>
///                 </list>
///             </term>
///         </item>
///         <item>
///             <term>
///                 <see cref="Second"/> <see langword="as"/> <see cref="System.Buffers.OperationStatus"/>
///                 <list type="bullet">
///                     <item>
///                         <description>
///                             <see cref="IsSecond"/>
///                         </description>
///                     </item>
///                     <item>
///                         <description>
///                             <see cref="OfSecond(System.Buffers.OperationStatus)"/>
///                         </description>
///                     </item>
///                 </list>
///             </term>
///         </item>
///     </list>
/// </remarks>
class Test(System.Buffers.OperationStatus first, System.Buffers.OperationStatus second) :
    global::System.IComparable,
    global::System.IComparable<object>,
    global::System.IComparable<global::Test>,
#if NET7_0_OR_GREATER
    global::System.Numerics.IComparisonOperators<global::Test, global::Test, bool>,
#endif
    global::System.IEquatable<object>,
    global::System.IEquatable<global::Test>,
    System.IConvertible,
    System.ISpanFormattable,
    System.IFormattable
{
    /// <summary>
    /// Compact representation of all unmanaged memory within the union <see cref="Test"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.InteropServices.StructLayoutAttribute(global::System.Runtime.InteropServices.LayoutKind.Explicit)]
    partial struct Unmanaged
    {
        [global::System.Runtime.InteropServices.FieldOffsetAttribute(0)]
        internal System.Buffers.OperationStatus _first;

        [global::System.Runtime.InteropServices.FieldOffsetAttribute(0)]
        internal System.Buffers.OperationStatus _second;
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    private readonly byte _discriminator;

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    private readonly Unmanaged _unmanaged = new Unmanaged() { _first = first };

    /// <summary>
    /// Initializes a new instance of the <see cref="Test"/> class with the variant <see cref="First"/> of type <see cref="System.Buffers.OperationStatus"/>.
    /// </summary>
    /// <param name="first">The variant.</param>
    /// <param name="x">The discriminator.</param>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    private Test(System.Buffers.OperationStatus first, byte x)
        : this(first, default(System.Buffers.OperationStatus))
    {
        _discriminator = x;
        _unmanaged._first = first;
    }

    /// <summary>
    /// Gets the value determining if the <see cref="Test"/> is the variant <see cref="First"/> of type <see cref="System.Buffers.OperationStatus"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public bool IsFirst
    {
        [global::System.Diagnostics.Contracts.PureAttribute]
        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(true, "First")]
        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(false, "Second")]
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator is 0;
    }

    /// <summary>
    /// Gets the value determining if the <see cref="Test"/> is the variant <see cref="Second"/> of type <see cref="System.Buffers.OperationStatus"/>.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public bool IsSecond
    {
        [global::System.Diagnostics.Contracts.PureAttribute]
        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(true, "Second")]
        [global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute(false, "First")]
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator is 1;
    }

    /// <summary>
    /// Gets the <see cref="System.Buffers.OperationStatus"/> variant.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public System.Buffers.OperationStatus First
    {
        [global::System.Diagnostics.Contracts.PureAttribute]
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator is 0 ? _unmanaged._first : default;
    }

    /// <summary>
    /// Gets the <see cref="System.Buffers.OperationStatus"/> variant.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public System.Buffers.OperationStatus Second
    {
        [global::System.Diagnostics.Contracts.PureAttribute]
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator is 1 ? _unmanaged._first : default;
    }

    /// <summary>This property exists solely to suppress lints regarding unused parameters.</summary>
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    bool UsedImplicitly => second is var _;

    /// <summary>
    /// Creates a new instance of the <see cref="Test"/> class with the variant <see cref="First"/> of type <see cref="System.Buffers.OperationStatus"/>.
    /// </summary>
    /// <param name="first">The value to pass into the type.</param>
    /// <returns>The union containing the parameter <paramref name="first"/>.</returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static global::Test OfFirst(System.Buffers.OperationStatus first)
        => new global::Test(first, 0);

    /// <summary>
    /// Creates a new instance of the <see cref="Test"/> class with the variant <see cref="Second"/> of type <see cref="System.Buffers.OperationStatus"/>.
    /// </summary>
    /// <param name="second">The value to pass into the type.</param>
    /// <returns>The union containing the parameter <paramref name="second"/>.</returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static global::Test OfSecond(System.Buffers.OperationStatus second)
        => new global::Test(second, 1);

    /// <summary>
    /// Determines whether the left-hand side is equal to the right.
    /// </summary>
    /// <param name="left">The left-hand side.</param>
    /// <param name="right">The right-hand side.</param>
    /// <returns>
    /// The value determining whether the parameter <paramref name="left"/>
    /// is equal to the parameter <paramref name="right"/>.
    /// </returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static bool operator ==(Test? left, Test? right)
        => left is null ? right is null : right is not null &&
            left._discriminator == right._discriminator &&
            left._discriminator switch
            {
                0 => left._unmanaged._first == right._unmanaged._first,
                _ => left._unmanaged._first == right._unmanaged._first,
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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static bool operator !=(Test? left, Test? right)
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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static bool operator >(Test? left, Test? right)
        => left is not null &&
            (right is null ||
            left._discriminator > right._discriminator ||
            left._discriminator == right._discriminator &&
            left._discriminator switch
            {
                0 => left._unmanaged._first > right._unmanaged._first,
                _ => left._unmanaged._first > right._unmanaged._first,
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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static bool operator >=(Test? left, Test? right)
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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static bool operator <(Test? left, Test? right)
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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static bool operator <=(Test? left, Test? right)
        => right >= left;

    /// <inheritdoc cref="object.Equals(object)"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public override bool Equals(object? obj)
        => obj is global::Test x && Equals(x);

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public bool Equals(Test? other)
        => this == other;

    /// <inheritdoc cref="IComparable.CompareTo(object)"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public int CompareTo(object? obj)
        => obj is null ? 1 : obj is global::Test x ? CompareTo(x) : -1;

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public int CompareTo(Test? other)
        => other is null ? 1 : Equals(other) ? 0 : -1;

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public override int GetHashCode()
        => unchecked(_discriminator * 28019) ^
        (_discriminator switch
        {
            0 => _unmanaged._first.GetHashCode(),
            _ => _unmanaged._first.GetHashCode(),
        });

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public override string ToString()
        => _discriminator switch
        {
            0 => $"{nameof(First)}({_unmanaged._first})",
            _ => $"{nameof(Second)}({_unmanaged._first})",
        };

    /// <summary>
    /// Invokes the callback based on current variance.
    /// </summary>
    /// <param name="onFirst">The callback to use when the contract of the <see cref="Test"/> class with the variant <see cref="First"/> of type <see cref="System.Buffers.OperationStatus"/> is held.</param>
    /// <param name="onSecond">The callback to use when the contract of the <see cref="Test"/> class with the variant <see cref="Second"/> of type <see cref="System.Buffers.OperationStatus"/> is held.</param>
    /// <returns>Itself.</returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public Test? Map(
        global::System.Action<System.Buffers.OperationStatus>? onFirst = null,
        global::System.Action<System.Buffers.OperationStatus>? onSecond = null
    )
    {
        switch (_discriminator)
        {
            case 0:
                onFirst?.Invoke(_unmanaged._first);
                return this;
            default:
                onSecond?.Invoke(_unmanaged._first);
                return this;
        }
    }

    /// <summary>
    /// Maps each variant to <typeparamref name="TMappingResult"/>.
    /// </summary>
    /// <typeparam name="TMappingResult">The resulting type from the mapping.</typeparam>
    /// <param name="onFirst">The callback to use when the contract of the <see cref="Test"/> class with the variant <see cref="First"/> of type <see cref="System.Buffers.OperationStatus"/> is held.</param>
    /// <param name="onSecond">The callback to use when the contract of the <see cref="Test"/> class with the variant <see cref="Second"/> of type <see cref="System.Buffers.OperationStatus"/> is held.</param>
    /// <returns>
    /// The resulting value from one of the parameters based on the current state of the object.
    /// </returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public TMappingResult Map<TMappingResult>(
        global::System.Func<System.Buffers.OperationStatus, TMappingResult> onFirst,
        global::System.Func<System.Buffers.OperationStatus, TMappingResult> onSecond
    )
        => _discriminator switch
        {
            0 => onFirst(_unmanaged._first),
            _ => onSecond(_unmanaged._first),
        };

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    /// <returns>
    /// The underlying value from this instance.
    /// </returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public System.Buffers.OperationStatus GetUnderlyingValue()
        => _discriminator switch
        {
            0 => _unmanaged._first,
            _ => _unmanaged._first,
        };

    /// <inheritdoc cref="System.IConvertible.GetTypeCode()"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public System.TypeCode GetTypeCode()
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).GetTypeCode(),
            _ => ((System.IConvertible)_unmanaged._first).GetTypeCode(),
        };

    /// <inheritdoc cref="System.IConvertible.ToBoolean(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public bool ToBoolean(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToBoolean(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToBoolean(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToByte(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public byte ToByte(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToByte(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToByte(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToChar(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public char ToChar(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToChar(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToChar(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToDateTime(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public System.DateTime ToDateTime(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToDateTime(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToDateTime(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToDecimal(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public decimal ToDecimal(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToDecimal(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToDecimal(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToDouble(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public double ToDouble(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToDouble(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToDouble(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToInt16(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public short ToInt16(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToInt16(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToInt16(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToInt32(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public int ToInt32(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToInt32(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToInt32(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToInt64(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public long ToInt64(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToInt64(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToInt64(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToSByte(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public sbyte ToSByte(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToSByte(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToSByte(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToSingle(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public float ToSingle(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToSingle(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToSingle(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToString(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public string ToString(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToString(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToString(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToType(System.Type, System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public object ToType(global::System.Type conversionType, global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToType(conversionType, provider),
            _ => ((System.IConvertible)_unmanaged._first).ToType(conversionType, provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToUInt16(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public ushort ToUInt16(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToUInt16(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToUInt16(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToUInt32(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public uint ToUInt32(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToUInt32(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToUInt32(provider),
        };

    /// <inheritdoc cref="System.IConvertible.ToUInt64(System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IConvertible"/>,
    /// <see cref="Second"/> as <see cref="System.IConvertible"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public ulong ToUInt64(global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.IConvertible)_unmanaged._first).ToUInt64(provider),
            _ => ((System.IConvertible)_unmanaged._first).ToUInt64(provider),
        };

    /// <inheritdoc cref="System.ISpanFormattable.TryFormat(System.Span{char}, out int, System.ReadOnlySpan{char}, System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.ISpanFormattable"/>,
    /// <see cref="Second"/> as <see cref="System.ISpanFormattable"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public bool TryFormat(global::System.Span<char> destination, scoped out int charsWritten, global::System.ReadOnlySpan<char> format, global::System.IFormatProvider? provider)
        => _discriminator switch
        {
            0 => ((System.ISpanFormattable)_unmanaged._first).TryFormat(destination, out charsWritten, format, provider),
            _ => ((System.ISpanFormattable)_unmanaged._first).TryFormat(destination, out charsWritten, format, provider),
        };

    /// <inheritdoc cref="System.IFormattable.ToString(string?, System.IFormatProvider?)"/>
    /// <remarks>
    /// Boxes when the current instance is
    /// <see cref="First"/> as <see cref="System.IFormattable"/>,
    /// <see cref="Second"/> as <see cref="System.IFormattable"/>.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public string ToString(string? format, global::System.IFormatProvider? formatProvider)
        => _discriminator switch
        {
            0 => ((System.IFormattable)_unmanaged._first).ToString(format, formatProvider),
            _ => ((System.IFormattable)_unmanaged._first).ToString(format, formatProvider),
        };
}
