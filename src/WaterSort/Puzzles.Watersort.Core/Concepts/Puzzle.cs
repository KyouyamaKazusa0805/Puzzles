namespace WaterSort.Concepts;

/// <summary>
/// Represents a puzzle.
/// </summary>
/// <param name="tubes">Indicates the tubes.</param>
public sealed partial class Puzzle(params Tube[] tubes) :
	ICloneable,
	IEquatable<Puzzle>,
	IEqualityOperators<Puzzle, Puzzle, bool>,
	IReadOnlyCollection<Tube>,
	IReadOnlyList<Tube>
{
	/// <summary>
	/// Indicates whether the puzzle is finished.
	/// </summary>
	public bool IsSolved
	{
		get
		{
			foreach (var tube in Tubes)
			{
				if (!tube.IsEmpty && !tube.IsSolved)
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>
	/// Indicates the number of the tubes.
	/// </summary>
	public int Length => Tubes.Length;

	/// <summary>
	/// Indicates the tubes.
	/// </summary>
	public ReadOnlySpan<Tube> Tubes => tubes;

	/// <summary>
	/// Returns a <see cref="FrozenDictionary{TKey, TValue}"/> that describes the color counting result.
	/// </summary>
	public FrozenDictionary<int, int> ColorCounting
	{
		get
		{
			var result = new Dictionary<int, int>();
			foreach (var tube in Tubes)
			{
				var count = tube.ColorsCount;
				if (!result.TryAdd(count, 1))
				{
					result[count]++;
				}
			}
			return result.ToFrozenDictionary();
		}
	}

	/// <summary>
	/// Returns a <see cref="FrozenDictionary{TKey, TValue}"/> that describes the colors,
	/// and the tubes containing such color.
	/// </summary>
	public FrozenDictionary<Color, ReadOnlyMemory<Tube>> ColorDistribution
	{
		get
		{
			var result = new Dictionary<Color, List<Tube>>();
			foreach (var tube in Tubes)
			{
				foreach (var color in tube)
				{
					if (!result.TryAdd(color, [tube]))
					{
						result[color].Add(tube);
					}
				}
			}
			return result.ToFrozenDictionary(
				static kvp => kvp.Key,
				static kvp => (ReadOnlyMemory<Tube>)kvp.Value.ToArray()
			);
		}
	}

	/// <inheritdoc/>
	int IReadOnlyCollection<Tube>.Count => Length;


	/// <summary>
	/// Gets the tube at the specified index.
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>A <see cref="Tube"/> instance.</returns>
	public Tube this[int index] => Tubes[index];


	/// <inheritdoc/>
	public override bool Equals(object? obj) => Equals(obj as Puzzle);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] Puzzle? other) => other is not null && Tubes.SequenceEqual(other.Tubes);

	/// <summary>
	/// Determine whether all tubes satisfy the specified condition.
	/// </summary>
	/// <param name="predicate">The condition.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool TrueForAll(Func<Tube, bool> predicate)
	{
		foreach (var tube in Tubes)
		{
			if (!predicate(tube))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Determine whether at least one tube satisfies the specified condition.
	/// </summary>
	/// <param name="predicate">The condition.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Exists(Func<Tube, bool> predicate)
	{
		foreach (var tube in Tubes)
		{
			if (predicate(tube))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var hashCode = new HashCode();
		foreach (var tube in Tubes)
		{
			hashCode.Add(tube);
		}
		return hashCode.ToHashCode();
	}

	/// <inheritdoc/>
	public override string ToString() => $"[{string.Join(", ", from tube in Tubes select tube.ToString())}]";

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public Enumerator GetEnumerator() => new(Tubes);

	/// <inheritdoc cref="ICloneable.Clone"/>
	public Puzzle Clone() => new([.. from tube in Tubes select tube.Clone()]);

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Tubes.ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Tube> IEnumerable<Tube>.GetEnumerator() => Tubes.ToArray().AsEnumerable().GetEnumerator();


	/// <inheritdoc cref="op_False(Puzzle)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !(Puzzle value) => !value.IsSolved;

	/// <summary>
	/// Returns property <see cref="IsSolved"/>.
	/// </summary>
	/// <param name="value">The object.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator true(Puzzle value) => value.IsSolved;

	/// <summary>
	/// Negates property <see cref="IsSolved"/>.
	/// </summary>
	/// <param name="value">The object.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator false(Puzzle value) => !value.IsSolved;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Puzzle? left, Puzzle? right)
		=> (left, right) switch { (not null, not null) => left.Equals(right), (null, null) => true, _ => false };

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Puzzle? left, Puzzle? right) => !(left == right);
}
