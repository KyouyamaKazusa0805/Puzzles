namespace Puzzles.WaterSort.Concepts;

/// <summary>
/// Represents a tube.
/// </summary>
/// <param name="_items">Indicates the items.</param>
[CollectionBuilder(typeof(Tube), nameof(Create))]
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class Tube(Stack<Color> _items) :
	ICloneable,
	IEquatable<Tube>,
	IEqualityOperators<Tube, Tube, bool>,
	IReadOnlyCollection<Color>
{
	/// <summary>
	/// Indicates whether all items are same-colored in the tube.
	/// </summary>
	public bool IsSolved => Colors.Length == 1;

	/// <summary>
	/// Indicates whether the tube is empty.
	/// </summary>
	public bool IsEmpty => _items.Count == 0;

	/// <summary>
	/// Determine whether the tube is mono-colored (only one color).
	/// </summary>
	public bool IsMonocolored
	{
		get
		{
			var originalColor = Color.MaxValue;
			foreach (var c in _items)
			{
				if (originalColor == Color.MaxValue)
				{
					originalColor = c;
					continue;
				}
				else if (originalColor != c)
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>
	/// Indicates the number of tube items.
	/// </summary>
	public int Length => Items.Length;

	/// <summary>
	/// Indicates the number of same color element above the top.
	/// </summary>
	public int TopColorSpannedCount => GetSpannedCount(TopColor);

	/// <summary>
	/// Indicates the number of colors.
	/// </summary>
	public int ColorsCount => ColorDistribution.Keys.Length;

	/// <summary>
	/// Indicates the top element; return <see cref="Color.MaxValue"/> if the tube is empty.
	/// </summary>
	public Color TopColor => _items.Count == 0 ? Color.MaxValue : _items.Peek();

	/// <summary>
	/// Indicates all colors in the tube.
	/// </summary>
	public ReadOnlySpan<Color> Colors => new HashSet<Color>(_items).ToArray();

	/// <summary>
	/// Indicates the items.
	/// </summary>
	[EquatableMember]
	public ReadOnlySpan<Color> Items => _items.ToArray();

	/// <summary>
	/// Returns a <see cref="FrozenDictionary{TKey, TValue}"/> that describes the spanning of water among different colors
	/// inside the current tube.
	/// </summary>
	public FrozenDictionary<Color, int> SpanningDistribution
	{
		get
		{
			var result = new Dictionary<Color, int>();
			foreach (var color in Colors)
			{
				var internalSpan = StackEntry<Color>.GetArray(_items).AsSpan()[.._items.Count];
				var firstPos = internalSpan.IndexOf(color);
				int lastPos;
				for (lastPos = firstPos + 1; lastPos < _items.Count && internalSpan[lastPos] != color; lastPos++) ;
				result.Add(color, lastPos - firstPos);
			}
			return result.ToFrozenDictionary();
		}
	}

	/// <summary>
	/// Returns a <see cref="FrozenDictionary{TKey, TValue}"/> that describes the appeared colors and the times of appearing.
	/// </summary>
	public FrozenDictionary<Color, int> ColorDistribution
	{
		get
		{
			var result = new Dictionary<Color, int>();
			foreach (var color in _items)
			{
				if (!result.TryAdd(color, 1))
				{
					result[color]++;
				}
			}
			return result.ToFrozenDictionary();
		}
	}

	/// <inheritdoc/>
	int IReadOnlyCollection<Color>.Count => Length;


	/// <summary>
	/// Pushes the specified color into the tube, with the specified number of times.
	/// </summary>
	/// <param name="color">The color.</param>
	/// <param name="count">The number of times pushed.</param>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the argument <paramref name="color"/> is invalid.</exception>
	public void Push(Color color, int count)
	{
		ArgumentOutOfRangeException.ThrowIfEqual(color, Color.MaxValue);

		// Push the color into the tube.
		for (var i = 0; i < count; i++)
		{
			_items.Push(color);
		}
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var result = new HashCode();
		foreach (var element in Items)
		{
			result.Add(element);
		}
		return result.ToHashCode();
	}

	/// <summary>
	/// Gets the number of spanned water elements of the specified color in the current tube.
	/// </summary>
	/// <param name="color">The color.</param>
	/// <returns>The number of spanning.</returns>
	public int GetSpannedCount(Color color)
	{
		if (IsEmpty)
		{
			return 0;
		}

		var result = 0;
		var internalSpan = StackEntry<Color>.GetArray(_items).AsSpan()[.._items.Count];
		var startIndex = -1;
		for (var i = internalSpan.Length - 1; i >= 0; i--)
		{
			if (internalSpan[i] == color)
			{
				startIndex = i;
				break;
			}
		}
		if (startIndex == -1)
		{
			return 0;
		}

		for (var i = startIndex; i >= 0; i--)
		{
			if (internalSpan[i] != color)
			{
				break;
			}
			result++;
		}
		return result;
	}

	/// <inheritdoc/>
	public override string ToString() => $"[{string.Join(", ", _items)}]";

	/// <summary>
	/// Pops the color from the top with the specified number of units.
	/// </summary>
	/// <param name="count">The desired number of popped color units.</param>
	/// <returns>The color popped; or throws <see cref="InvalidOperationException"/> if failed to pour out.</returns>
	/// <exception cref="InvalidOperationException">Throws when the operation is invalid.</exception>
	public Color Pop(int count)
	{
		if (_items.Count == 0)
		{
			throw new InvalidOperationException();
		}

		var color = _items.Pop();
		while (--count > 0)
		{
			_items.TryPeek(out var c);
			if (c == color)
			{
				_items.Pop();
				continue;
			}
			throw new InvalidOperationException();
		}
		return color;
	}

	/// <summary>
	/// Pops the color from the top; if the color occupies multiple units of the tube, all will be popped.
	/// </summary>
	/// <param name="count">The number of popped color units.</param>
	/// <returns>The color popped.</returns>
	public Color Pop(out int count)
	{
		if (_items.Count == 0)
		{
			count = 0;
			return Color.MaxValue;
		}

		count = 1;
		var color = _items.Pop();
		while (_items.TryPeek(out var c) && c == color)
		{
			count++;
			_items.Pop();
		}
		return color;
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public Enumerator GetEnumerator() => new(Items);

	/// <inheritdoc cref="ICloneable.Clone"/>
	public Tube Clone()
	{
		var items = new Stack<Color>(_items.Count);
		var auxiliaryItems = new Stack<Color>(_items.Count);

		foreach (var element in _items)
		{
			auxiliaryItems.Push(element);
		}
		foreach (var element in auxiliaryItems)
		{
			items.Push(element);
		}

		return new(items);
	}

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Color> IEnumerable<Color>.GetEnumerator() => _items.AsEnumerable().GetEnumerator();


	/// <summary>
	/// Creates a <see cref="Tube"/> object via a list of <see cref="Color"/> instances.
	/// </summary>
	/// <param name="colors">The colors.</param>
	/// <returns>The tube created, with FILO structure.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Tube Create(ReadOnlySpan<Color> colors)
	{
		var result = new Stack<Color>();
		foreach (var color in colors)
		{
			result.Push(color);
		}
		return new(result);
	}


	/// <summary>
	/// Returns <see cref="IsSolved"/> property.
	/// </summary>
	/// <param name="value">The object.</param>
	/// <returns>The value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator true(Tube value) => value.IsSolved;

	/// <summary>
	/// Negates the property value <see cref="IsSolved"/>.
	/// </summary>
	/// <param name="value">The object.</param>
	/// <returns>The value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator false(Tube value) => !value.IsSolved;
}

file static class StackEntry<T>
{
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_array")]
	public static extern ref T[] GetArray(Stack<T> stack);
}
