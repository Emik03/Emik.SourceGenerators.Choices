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
///                 <see cref="First"/> <see langword="as"/> <see cref="System.Collections.CollectionBase"/>
///                 <list type="bullet">
///                     <item>
///                         <description>
///                             <see cref="IsFirst"/>
///                         </description>
///                     </item>
///                     <item>
///                         <description>
///                             <see cref="OfFirst(System.Collections.CollectionBase)"/>
///                         </description>
///                     </item>
///                 </list>
///             </term>
///         </item>
///         <item>
///             <term>
///                 <see cref="Second"/> <see langword="as"/> <see cref="System.Collections.CollectionBase"/>
///                 <list type="bullet">
///                     <item>
///                         <description>
///                             <see cref="IsSecond"/>
///                         </description>
///                     </item>
///                     <item>
///                         <description>
///                             <see cref="OfSecond(System.Collections.CollectionBase)"/>
///                         </description>
///                     </item>
///                 </list>
///             </term>
///         </item>
///     </list>
/// </remarks>
partial class Test :
    global::System.IComparable,
    global::System.IComparable<object>,
    global::System.IComparable<global::Test>,
#if NET7_0_OR_GREATER
    global::System.Numerics.IComparisonOperators<global::Test, global::Test, bool>,
#endif
    global::System.IEquatable<object>,
    global::System.IEquatable<global::Test>,
    System.Collections.IList,
    System.Collections.ICollection,
    System.Collections.IEnumerable
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    private static class Choice
    {
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
        internal static class First<TFirstDiscard>
        {
            [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
            internal sealed class Second<TSecondDiscard> : global::System.Attribute
            {
            }
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    private readonly byte _discriminator;

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    private readonly System.Collections.CollectionBase? _reference;

    /// <summary>
    /// Initializes a new instance of the <see cref="Test"/> class with the variant <see cref="First"/> of type <see cref="System.Collections.CollectionBase"/>.
    /// </summary>
    /// <param name="first">The variant.</param>
    /// <param name="x">The discriminator.</param>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    private Test(System.Collections.CollectionBase first, byte x)
    {
        _discriminator = x;
        _reference = first;
    }

    /// <summary>
    /// Gets the value determining if the <see cref="Test"/> is the variant <see cref="First"/> of type <see cref="System.Collections.CollectionBase"/>.
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
    /// Gets the value determining if the <see cref="Test"/> is the variant <see cref="Second"/> of type <see cref="System.Collections.CollectionBase"/>.
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
    /// Gets the <see cref="System.Collections.CollectionBase"/> variant.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public System.Collections.CollectionBase? First
    {
        [global::System.Diagnostics.Contracts.PureAttribute]
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator is 0 ? (System.Collections.CollectionBase)_reference! : default;
    }

    /// <summary>
    /// Gets the <see cref="System.Collections.CollectionBase"/> variant.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public System.Collections.CollectionBase? Second
    {
        [global::System.Diagnostics.Contracts.PureAttribute]
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator is 1 ? (System.Collections.CollectionBase)_reference! : default;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Test"/> class with the variant <see cref="First"/> of type <see cref="System.Collections.CollectionBase"/>.
    /// </summary>
    /// <param name="first">The value to pass into the type.</param>
    /// <returns>The union containing the parameter <paramref name="first"/>.</returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static global::Test OfFirst(System.Collections.CollectionBase first)
        => new global::Test(first, 0);

    /// <summary>
    /// Creates a new instance of the <see cref="Test"/> class with the variant <see cref="Second"/> of type <see cref="System.Collections.CollectionBase"/>.
    /// </summary>
    /// <param name="second">The value to pass into the type.</param>
    /// <returns>The union containing the parameter <paramref name="second"/>.</returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public static global::Test OfSecond(System.Collections.CollectionBase second)
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
                0 => global::System.Collections.Generic.EqualityComparer<System.Collections.CollectionBase>.Default.Equals(((System.Collections.CollectionBase)left._reference!), ((System.Collections.CollectionBase)right._reference!)),
                _ => global::System.Collections.Generic.EqualityComparer<System.Collections.CollectionBase>.Default.Equals(((System.Collections.CollectionBase)left._reference!), ((System.Collections.CollectionBase)right._reference!)),
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
                0 => global::System.Collections.Generic.Comparer<System.Collections.CollectionBase>.Default.Compare(((System.Collections.CollectionBase)left._reference!), ((System.Collections.CollectionBase)right._reference!)) > 0,
                _ => global::System.Collections.Generic.Comparer<System.Collections.CollectionBase>.Default.Compare(((System.Collections.CollectionBase)left._reference!), ((System.Collections.CollectionBase)right._reference!)) > 0,
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
            0 => ((System.Collections.CollectionBase)_reference!).GetHashCode(),
            _ => ((System.Collections.CollectionBase)_reference!).GetHashCode(),
        });

    /// <inheritdoc />
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Diagnostics.Contracts.PureAttribute]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public override string ToString()
        => _discriminator switch
        {
            0 => $"{nameof(First)}({((System.Collections.CollectionBase)_reference!)})",
            _ => $"{nameof(Second)}({((System.Collections.CollectionBase)_reference!)})",
        };

    /// <summary>
    /// Invokes the callback based on current variance.
    /// </summary>
    /// <param name="onFirst">The callback to use when the contract of the <see cref="Test"/> class with the variant <see cref="First"/> of type <see cref="System.Collections.CollectionBase"/> is held.</param>
    /// <param name="onSecond">The callback to use when the contract of the <see cref="Test"/> class with the variant <see cref="Second"/> of type <see cref="System.Collections.CollectionBase"/> is held.</param>
    /// <returns>Itself.</returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public Test? Map(
        global::System.Action<System.Collections.CollectionBase>? onFirst = null,
        global::System.Action<System.Collections.CollectionBase>? onSecond = null
    )
    {
        switch (_discriminator)
        {
            case 0:
                onFirst?.Invoke(((System.Collections.CollectionBase)_reference!));
                return this;
            default:
                onSecond?.Invoke(((System.Collections.CollectionBase)_reference!));
                return this;
        }
    }

    /// <summary>
    /// Maps each variant to <typeparamref name="TMappingResult"/>.
    /// </summary>
    /// <typeparam name="TMappingResult">The resulting type from the mapping.</typeparam>
    /// <param name="onFirst">The callback to use when the contract of the <see cref="Test"/> class with the variant <see cref="First"/> of type <see cref="System.Collections.CollectionBase"/> is held.</param>
    /// <param name="onSecond">The callback to use when the contract of the <see cref="Test"/> class with the variant <see cref="Second"/> of type <see cref="System.Collections.CollectionBase"/> is held.</param>
    /// <returns>
    /// The resulting value from one of the parameters based on the current state of the object.
    /// </returns>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public TMappingResult Map<TMappingResult>(
        global::System.Func<System.Collections.CollectionBase, TMappingResult> onFirst,
        global::System.Func<System.Collections.CollectionBase, TMappingResult> onSecond
    )
        => _discriminator switch
        {
            0 => onFirst(((System.Collections.CollectionBase)_reference!)),
            _ => onSecond(((System.Collections.CollectionBase)_reference!)),
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
    public System.Collections.CollectionBase GetUnderlyingValue()
        => (System.Collections.CollectionBase)_reference!;

    /// <inheritdoc cref="System.Collections.CollectionBase.Clear()"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public void Clear()
    {
        switch (_discriminator)
        {
            case 0:
                ((System.Collections.CollectionBase)_reference!).Clear();
                break;
            default:
                ((System.Collections.CollectionBase)_reference!).Clear();
                break;
        }
    }

    /// <inheritdoc cref="System.Collections.CollectionBase.GetEnumerator()"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public System.Collections.IEnumerator GetEnumerator()
        => _discriminator switch
        {
            0 => ((System.Collections.CollectionBase)_reference!).GetEnumerator(),
            _ => ((System.Collections.CollectionBase)_reference!).GetEnumerator(),
        };

    /// <inheritdoc cref="System.Collections.CollectionBase.RemoveAt(int)"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public void RemoveAt(int index)
    {
        switch (_discriminator)
        {
            case 0:
                ((System.Collections.CollectionBase)_reference!).RemoveAt(index);
                break;
            default:
                ((System.Collections.CollectionBase)_reference!).RemoveAt(index);
                break;
        }
    }

    /// <inheritdoc cref="System.Collections.CollectionBase.Capacity"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public int Capacity
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator switch
        {
            0 => ((System.Collections.CollectionBase)_reference!).Capacity,
            _ => ((System.Collections.CollectionBase)_reference!).Capacity,
        };
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        set => _ = _discriminator switch
        {
            0 => ((System.Collections.CollectionBase)_reference!).Capacity = value,
            _ => ((System.Collections.CollectionBase)_reference!).Capacity = value,
        };
    }

    /// <inheritdoc cref="System.Collections.CollectionBase.Count"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public int Count
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator switch
        {
            0 => ((System.Collections.CollectionBase)_reference!).Count,
            _ => ((System.Collections.CollectionBase)_reference!).Count,
        };
    }

    /// <inheritdoc cref="System.Collections.IList.Add(object?)"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public int Add(object? value)
        => _discriminator switch
        {
            0 => ((System.Collections.IList)_reference!).Add(value),
            _ => ((System.Collections.IList)_reference!).Add(value),
        };

    /// <inheritdoc cref="System.Collections.IList.Contains(object?)"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public bool Contains(object? value)
        => _discriminator switch
        {
            0 => ((System.Collections.IList)_reference!).Contains(value),
            _ => ((System.Collections.IList)_reference!).Contains(value),
        };

    /// <inheritdoc cref="System.Collections.IList.IndexOf(object?)"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public int IndexOf(object? value)
        => _discriminator switch
        {
            0 => ((System.Collections.IList)_reference!).IndexOf(value),
            _ => ((System.Collections.IList)_reference!).IndexOf(value),
        };

    /// <inheritdoc cref="System.Collections.IList.Insert(int, object?)"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public void Insert(int index, object? value)
    {
        switch (_discriminator)
        {
            case 0:
                ((System.Collections.IList)_reference!).Insert(index, value);
                break;
            default:
                ((System.Collections.IList)_reference!).Insert(index, value);
                break;
        }
    }

    /// <inheritdoc cref="System.Collections.IList.Remove(object?)"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public void Remove(object? value)
    {
        switch (_discriminator)
        {
            case 0:
                ((System.Collections.IList)_reference!).Remove(value);
                break;
            default:
                ((System.Collections.IList)_reference!).Remove(value);
                break;
        }
    }

    /// <inheritdoc cref="System.Collections.IList.IsFixedSize"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public bool IsFixedSize
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator switch
        {
            0 => ((System.Collections.IList)_reference!).IsFixedSize,
            _ => ((System.Collections.IList)_reference!).IsFixedSize,
        };
    }

    /// <inheritdoc cref="System.Collections.IList.IsReadOnly"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public bool IsReadOnly
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator switch
        {
            0 => ((System.Collections.IList)_reference!).IsReadOnly,
            _ => ((System.Collections.IList)_reference!).IsReadOnly,
        };
    }

    /// <inheritdoc cref="System.Collections.IList.this[int]"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public object? this[int index]
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator switch
        {
            0 => ((System.Collections.IList)_reference!)[index],
            _ => ((System.Collections.IList)_reference!)[index],
        };
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        set => _ = _discriminator switch
        {
            0 => ((System.Collections.IList)_reference!)[index] = value,
            _ => ((System.Collections.IList)_reference!)[index] = value,
        };
    }

    /// <inheritdoc cref="System.Collections.ICollection.CopyTo(System.Array, int)"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
    public void CopyTo(global::System.Array array, int index)
    {
        switch (_discriminator)
        {
            case 0:
                ((System.Collections.ICollection)_reference!).CopyTo(array, index);
                break;
            default:
                ((System.Collections.ICollection)_reference!).CopyTo(array, index);
                break;
        }
    }

    /// <inheritdoc cref="System.Collections.ICollection.IsSynchronized"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public bool IsSynchronized
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator switch
        {
            0 => ((System.Collections.ICollection)_reference!).IsSynchronized,
            _ => ((System.Collections.ICollection)_reference!).IsSynchronized,
        };
    }

    /// <inheritdoc cref="System.Collections.ICollection.SyncRoot"/>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Emik.SourceGenerators.Choices", "1.4.2.0")]
    public object SyncRoot
    {
        [global::System.Runtime.CompilerServices.MethodImplAttribute(256)]
        get => _discriminator switch
        {
            0 => ((System.Collections.ICollection)_reference!).SyncRoot,
            _ => ((System.Collections.ICollection)_reference!).SyncRoot,
        };
    }
}
