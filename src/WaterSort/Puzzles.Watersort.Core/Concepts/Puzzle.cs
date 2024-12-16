namespace Puzzles.WaterSort.Concepts;

/// <summary>
/// Represents a puzzle.
/// </summary>
/// <param name="tubes">Indicates the tubes.</param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
[CollectionBuilder(typeof(Puzzle), nameof(Create))]
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

			var depth = Depth;
			return TrueForAll(tube => tube.Length == depth || tube.Length == 0);
		}
	}

	/// <summary>
	/// Determine whether the current puzzle is at initial state.
	/// </summary>
	public bool IsInitial
	{
		get
		{
			var depth = Depth;
			var colorsCount = ColorDistribution.Keys.Length;
			foreach (var (index, tube) in Tubes.Index())
			{
				if (!tube.IsEmpty && tube.Length != depth)
				{
					return false;
				}

				if (tube.IsEmpty && index < colorsCount)
				{
					// The game requires all empty tubes should be at the last positions at initial state.
					// e.g. [[0, 0], [1, 1], [2, 2], [], []]
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
	/// Indicates the depth of the puzzle.
	/// </summary>
	public int Depth => Tubes.Max(static tube => tube.Length);

	/// <summary>
	/// Indicates the tubes.
	/// </summary>
	[EquatableMember]
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

	/// <summary>
	/// Determine whether the current puzzle can accommodate the specified color into the specified tube.
	/// </summary>
	/// <param name="color">The color.</param>
	/// <param name="tubeIndex">The index of the tube.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	/// <exception cref="InvalidOperationException">Throws when the color value is invalid.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CanAccommodateColor(Color color, int tubeIndex)
		=> color != Color.MaxValue
			? Tubes[tubeIndex].Length < Depth
			: throw new InvalidOperationException("The color here cannot be empty color.");

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


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(string? s, [NotNullWhen(true)] out Puzzle? result)
	{
		try
		{
			result = s is null ? throw new FormatException() : Parse(s);
			return true;
		}
		catch (FormatException)
		{
			result = null;
			return false;
		}
	}

	/// <summary>
	/// Create a <see cref="Puzzle"/> with tubes.
	/// </summary>
	/// <param name="tubes">The tubes.</param>
	/// <returns>A <see cref="Puzzle"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Puzzle Create(ReadOnlySpan<Tube> tubes) => new([.. tubes]);

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static Puzzle Parse(string s)
	{
		var tubes = new List<Tube>();
		foreach (var textRange in TubePattern.EnumerateMatches(s))
		{
			var values = textRange.MatchString(s);
			var tube = new List<Color>();
			foreach (var digitRange in DigitPattern.EnumerateMatches(values))
			{
				tube.Add(Color.Parse(digitRange.MatchString(values)));
			}
			tubes.Add([.. tube]);
		}
		return [.. tubes];
	}


	[GeneratedRegex("""\[(|\d+(,\s*\d+)*)\]""", RegexOptions.Compiled)]
	private static partial Regex TubePattern { get; }

	[GeneratedRegex("""\d+""", RegexOptions.Compiled)]
	private static partial Regex DigitPattern { get; }
}
