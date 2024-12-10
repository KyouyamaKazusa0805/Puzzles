namespace Puzzles.Matching.Concepts;

/// <summary>
/// Represents a grid of match.
/// </summary>
/// <remarks>
/// The grid will use an array of <see cref="ItemIndex"/> values to describe internal items.
/// Different elements will be represented as its <see cref="ItemIndex"/> value.
/// Using 255 (i.e. <see cref="ItemIndex.MaxValue"/> to describe an empty cell).
/// </remarks>
public sealed partial class Grid :
	ICloneable,
	IEnumerable<ItemIndex>,
	IEquatable<Grid>,
	IEqualityOperators<Grid, Grid, bool>,
	IFormattable,
	IParsable<Grid>,
	IReadOnlyCollection<ItemIndex>,
	IReadOnlyList<ItemIndex>
{
	/// <summary>
	/// Indicates the empty cell value.
	/// </summary>
	public const ItemIndex EmptyKey = ItemIndex.MaxValue;


	/// <summary>
	/// Indicates the backing grid.
	/// </summary>
	private readonly ItemIndex[] _array;


	/// <summary>
	/// Initializes a <see cref="Grid"/> instance via the specified array and its number of rows and columns.
	/// </summary>
	/// <param name="array">Indicates the array.</param>
	/// <param name="columns">Indicates the number of rows.</param>
	/// <param name="rows">Indicates the number of columns.</param>
	public Grid(ItemIndex[] array, int rows, int columns) => (_array, RowsLength, ColumnsLength) = (array, rows, columns);

	/// <inheritdoc cref="Grid(ItemIndex[], int, int)"/>
	public Grid(ItemIndex[,] array) : this(array.Flat(), array.GetLength(0), array.GetLength(1))
	{
	}


	/// <summary>
	/// Indicates whether the grid is empty.
	/// </summary>
	public bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			// To check whether a puzzle is empty, just check all values are equal to 255 ('EmptyKey').
			var sequence = new ItemIndex[RowsLength * ColumnsLength].AsSpan();
			sequence.Fill(EmptyKey);
			return _array.AsSpan().SequenceEqual(sequence);
		}
	}

	/// <summary>
	/// Indicates whether the grid pattern is valid to be checked.
	/// </summary>
	public bool IsValid => ItemsSet.Values.All(static value => (value.Count & 1) == 0);

	/// <summary>
	/// Indicates the number of rows used.
	/// </summary>
	public int RowsLength { get; }

	/// <summary>
	/// Indicates the number of columns used.
	/// </summary>
	public int ColumnsLength { get; }

	/// <summary>
	/// Indicates the number of elements.
	/// </summary>
	public int Length => _array.Length;

	/// <summary>
	/// Indicates the last items in the grid.
	/// </summary>
	public ReadOnlySpan<ItemIndex> LastItems => ItemsSet.Keys.ToArray();

	/// <summary>
	/// Represents a dictionary of items that their appearing positions (in coordinates).
	/// </summary>
	public FrozenDictionary<ItemIndex, IReadOnlySet<Coordinate>> ItemsSet
	{
		get
		{
			var result = new Dictionary<ItemIndex, List<Coordinate>>();
			for (var i = 0; i < RowsLength; i++)
			{
				for (var j = 0; j < ColumnsLength; j++)
				{
					var item = this[i, j];
					var coordinate = new Coordinate(i, j);
					if (!result.TryAdd(item, [coordinate]))
					{
						result[item].Add(coordinate);
					}
				}
			}
			return result.ToFrozenDictionary(static kvp => kvp.Key, static kvp => (IReadOnlySet<Coordinate>)kvp.Value.ToHashSet());
		}
	}

	/// <inheritdoc/>
	int IReadOnlyCollection<ItemIndex>.Count => Length;


	/// <summary>
	/// Represents an empty instance.
	/// </summary>
	public static Grid Empty => new([], 0, 0);


	/// <summary>
	/// Gets the element at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The item.</returns>
	public ref ItemIndex this[int index] => ref _array[index];

	/// <summary>
	/// Gets the element at the specified row and the specified column.
	/// </summary>
	/// <param name="row">The desired row index.</param>
	/// <param name="column">The desired column index.</param>
	/// <returns>The item.</returns>
	public ref ItemIndex this[int row, int column] => ref _array[row * ColumnsLength + column];

	/// <summary>
	/// Gets the element at the specified coordinate.
	/// </summary>
	/// <param name="coordinate">The desired coordinate.</param>
	/// <returns>The item.</returns>
	public ref ItemIndex this[Coordinate coordinate] => ref this[coordinate.X, coordinate.Y];

	/// <inheritdoc/>
	ItemIndex IReadOnlyList<ItemIndex>.this[int index] => this[index];


	[GeneratedRegex("""\d{1,3}""", RegexOptions.Compiled)]
	private static partial Regex Pattern { get; }


	/// <summary>
	/// Apply a match, removing the pair of cells from the grid, making those two cells empty.
	/// </summary>
	/// <param name="match">The match to be applied.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Apply(ItemMatch match) => this[match.Start] = this[match.End] = EmptyKey;

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as Grid);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] Grid? other) => other is not null && _array.AsSpan().SequenceEqual(other._array);

	/// <summary>
	/// Try to find the next match. If the grid has no match, <see langword="false"/> will be returned.
	/// </summary>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result indicating whether a new match can be found.</returns>
	public bool TryGetMatch([NotNullWhen(true)] out ItemMatch? result)
	{
		foreach (var (key, coordinates) in ItemsSet)
		{
			if (key == EmptyKey)
			{
				continue;
			}

			foreach (var pair in coordinates.ToArray().AsReadOnlySpan().GetSubsets(2))
			{
				if (TryPair(pair[0], pair[1], out result))
				{
					return true;
				}
			}
		}

		result = null;
		return false;
	}

	/// <summary>
	/// Determine whether two values are paired under the matching rule;
	/// if so, return <see langword="true"/> and return an <see cref="ItemMatch"/> object
	/// to parameter <paramref name="result"/> indicating the result details.
	/// </summary>
	/// <param name="coordinate1">Indicates the first coordinate.</param>
	/// <param name="coordinate2">Indicates the second coordinate.</param>
	/// <param name="result">Indicates the result match.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <exception cref="InvalidOperationException">Throws when the grid is too small (lower than 2x2).</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryPair(Coordinate coordinate1, Coordinate coordinate2, [NotNullWhen(true)] out ItemMatch? result)
	{
		if (RowsLength < 2 || ColumnsLength < 2)
		{
			throw new InvalidOperationException("The grid is too small.");
		}

		if (IsSameRowPaired(coordinate1, coordinate2))
		{
			result = new(coordinate1, coordinate2);
			return true;
		}
		if (IsSameColumnPaired(coordinate1, coordinate2))
		{
			result = new(coordinate1, coordinate2);
			return true;
		}
		if (IsTurningOncePaired(coordinate1, coordinate2, out var interim))
		{
			result = new(coordinate1, coordinate2, interim);
			return true;
		}
		if (IsTurningTwicePaired(coordinate1, coordinate2, out var interimPair))
		{
			result = new(coordinate1, coordinate2, interimPair);
			return true;
		}

		result = null;
		return false;
	}

	/// <summary>
	/// Determine whether two coordinates is in same row, and can be paired.
	/// </summary>
	/// <param name="coordinate1">Indicates the first coordinate.</param>
	/// <param name="coordinate2">Indicates the second coordinate.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool IsSameRowPaired(Coordinate coordinate1, Coordinate coordinate2)
	{
		if (coordinate1 == coordinate2)
		{
			return false;
		}

		if (coordinate1.X != coordinate2.X)
		{
			return false;
		}

		var c1 = coordinate1.Y;
		var c2 = coordinate2.Y;
		var start = Math.Min(c1, c2);
		var end = Math.Max(c1, c2);
		for (var i = start + 1; i < end; i++)
		{
			if (Blocks(coordinate1.X, i))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Determine whether two coordinates is in same column, and can be paired.
	/// </summary>
	/// <param name="coordinate1">Indicates the first coordinate.</param>
	/// <param name="coordinate2">Indicates the second coordinate.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool IsSameColumnPaired(Coordinate coordinate1, Coordinate coordinate2)
	{
		if (coordinate1 == coordinate2)
		{
			return false;
		}

		if (coordinate1.Y != coordinate2.Y)
		{
			return false;
		}

		var r1 = coordinate1.X;
		var r2 = coordinate2.X;
		var start = Math.Min(r1, r2);
		var end = Math.Max(r1, r2);
		for (var i = start + 1; i < end; i++)
		{
			if (Blocks(i, coordinate1.Y))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Determine whether two coordinates can be paired with one-time turning.
	/// </summary>
	/// <param name="coordinate1">Indicates the first coordinate.</param>
	/// <param name="coordinate2">Indicates the second coordinate.</param>
	/// <param name="interim">Indicates the interim on turning.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsTurningOncePaired(Coordinate coordinate1, Coordinate coordinate2, out Coordinate interim)
	{
		if (coordinate1 == coordinate2)
		{
			interim = default;
			return false;
		}

		var (cx, dy) = coordinate1;
		var (dx, cy) = coordinate2;
		if (!Blocks(cx, cy))
		{
			if (IsSameRowPaired(coordinate1, new(cx, cy)) && IsSameColumnPaired(new(cx, cy), coordinate2))
			{
				interim = new(cx, cy);
				return true;
			}
		}
		if (!Blocks(dx, dy))
		{
			if (IsSameColumnPaired(coordinate1, new(dx, dy)) && IsSameRowPaired(new(dx, dy), coordinate2))
			{
				interim = new(dx, dy);
				return true;
			}
		}

		interim = default;
		return false;
	}

	/// <summary>
	/// Determine whether two coordinates can be paired with two-time turning.
	/// </summary>
	/// <param name="coordinate1">Indicates the first coordinate.</param>
	/// <param name="coordinate2">Indicates the second coordinate.</param>
	/// <param name="interims">Indicates the interim coordinates on turning.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool IsTurningTwicePaired(Coordinate coordinate1, Coordinate coordinate2, [NotNullWhen(true)] out Coordinate[]? interims)
	{
		if (coordinate1 == coordinate2)
		{
			interims = null;
			return false;
		}

		interims = [new(-2, -2), new(-2, -2)];
		for (var i = -1; i <= RowsLength; i++)
		{
			for (var j = -1; j <= ColumnsLength; j++)
			{
				if (i == coordinate1.X && j == coordinate1.Y || i == coordinate2.X && j == coordinate2.Y)
				{
					continue;
				}

				if (Blocks(i, j))
				{
					continue;
				}

				if (checkType1(coordinate1, new(i, j), coordinate2, out var temp0, out var temp1))
				{
					if (temp0 == default && temp1 == default)
					{
						interims = [temp0, temp1];
					}
					else
					{
						var path0 = new ItemMatch(coordinate1, coordinate2, interims[0], interims[1]).Distance;
						var path1 = new ItemMatch(coordinate1, coordinate2, temp0, temp1).Distance;
						if (path1 < path0)
						{
							interims = [temp0, temp1];
						}
					}
				}

				if (checkType2(coordinate1, new(i, j), coordinate2, out temp0, out temp1))
				{
					if (temp0 == default && temp1 == default)
					{
						interims = [temp0, temp1];
					}
					else
					{
						var path0 = new ItemMatch(coordinate1, coordinate2, interims[0], interims[1]).Distance;
						var path1 = new ItemMatch(coordinate1, coordinate2, temp0, temp1).Distance;
						if (path1 < path0)
						{
							interims = [temp0, temp1];
						}
					}
				}
			}
		}

		if (interims is [(-2, -2), (-2, -2)])
		{
			interims = null;
			return false;
		}
		return true;


		bool checkType1(
			Coordinate coordinate1,
			Coordinate interim,
			Coordinate coordinate2,
			out Coordinate resultInterim1,
			out Coordinate resultInterim2
		)
		{
			if (IsTurningOncePaired(coordinate1, interim, out resultInterim1)
				&& (IsSameRowPaired(interim, coordinate2) || IsSameColumnPaired(interim, coordinate2)))
			{
				resultInterim2 = interim;
				return true;
			}

			(resultInterim1, resultInterim2) = (default, default);
			return false;
		}

		bool checkType2(
			Coordinate coordinate1,
			Coordinate interim,
			Coordinate coordinate2,
			out Coordinate resultInterim1,
			out Coordinate resultInterim2
		)
		{
			if (IsTurningOncePaired(interim, coordinate2, out resultInterim2)
				&& (IsSameRowPaired(coordinate1, interim) || IsSameColumnPaired(coordinate1, interim)))
			{
				resultInterim1 = interim;
				return true;
			}

			(resultInterim1, resultInterim2) = (default, default);
			return false;
		}
	}

	/// <summary>
	/// Determine whether the grid has blocked the specified coordinate.
	/// </summary>
	/// <param name="x">The row index.</param>
	/// <param name="y">The column index.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Blocks(int x, int y) => Blocks(new(x, y));

	/// <summary>
	/// Determine whether the grid has blocked the specified coordinate.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Blocks(Coordinate coordinate) => !IsCoordinateOutOfBound(coordinate) && this[coordinate] != EmptyKey;

	/// <summary>
	/// Determine whether the current coordinate is out of bound.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsCoordinateOutOfBound(Coordinate coordinate)
	{
		var (x, y) = coordinate;
		return x < 0 || x >= RowsLength || y < 0 || y >= ColumnsLength;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var hashCode = new HashCode();
		hashCode.AddBytes(_array);
		return hashCode.ToHashCode();
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	/// <remarks>
	/// All formats:
	/// <list type="table">
	/// <listheader>
	/// <term>Format</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><c>"a"</c></term>
	/// <description>Array-like format, output like an 2D-array sequence, keeping new line characters if line ends</description>
	/// </item>
	/// <item>
	/// <term><c>"f"</c></term>
	/// <description>Sequence-like format, output like an 1D-aray flatten each element by removing new line characters</description>
	/// </item>
	/// <item>
	/// <term><c>"f+1"</c></term>
	/// <description>
	/// Just like <see cref="ToString(string?)"/> with format <c>"f"</c>, but with each element added with 1
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	public string ToString(string? format)
	{
		var maxValue = _array.Max();
		var array = (ItemIndex[,])this;
		return format switch
		{
			null or "a" => array.ToArrayString(numeralConverter),
			"f" => array.Flat().ToArrayString(),
			"f+1" => array.Flat().ToArrayString(static v => (v + 1).ToString()),
			_ => throw new FormatException()
		};


		string? numeralConverter(byte e) => e.ToString(maxValue switch { >= 0 and < 10 => "0", >= 10 and < 100 => "00", _ => "000" });
	}

	/// <summary>
	/// Try to find all possible steps appeared in the grid; if no steps found, an empty array will be returned.
	/// </summary>
	/// <returns>All matched items.</returns>
	public ReadOnlySpan<ItemMatch> GetAllMatches()
	{
		var result = new List<ItemMatch>();
		foreach (var (key, coordinates) in ItemsSet)
		{
			if (key == EmptyKey)
			{
				continue;
			}

			foreach (var pair in coordinates.ToArray().AsReadOnlySpan().GetSubsets(2))
			{
				if (TryPair(pair[0], pair[1], out var r))
				{
					result.Add(r);
				}
			}
		}
		return result.AsSpan();
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public Enumerator GetEnumerator() => new(_array);

	/// <inheritdoc cref="ICloneable.Clone"/>
	public Grid Clone() => new((byte[])_array.Clone(), RowsLength, ColumnsLength);

	/// <summary>
	/// Try to get the next match; or throws <see cref="InvalidOperationException"/> if a match is not found.
	/// </summary>
	/// <returns>The next match.</returns>
	/// <exception cref="InvalidOperationException">Throws when the grid is invalid.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ItemMatch GetMatch() => TryGetMatch(out var result) ? result : throw new InvalidOperationException();

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(format);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<ItemIndex> IEnumerable<ItemIndex>.GetEnumerator() => _array.AsEnumerable().GetEnumerator();


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(string s, [NotNullWhen(true)] out Grid? result)
	{
		try
		{
			result = Parse(s);
			return true;
		}
		catch (FormatException)
		{
			result = null;
			return false;
		}
	}

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static Grid Parse(string s)
	{
		// Format:
		// <rows>:<columns>:<data>

		var split = s.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		var rows = int.Parse(split[0]);
		var columns = int.Parse(split[1]);
		var array = new ItemIndex[rows * columns];
		var i = 0;
		var dataSpan = split[2].AsSpan();
		foreach (var match in Pattern.EnumerateMatches(dataSpan))
		{
			var index = match.Index;
			array[i++] = match.Length switch
			{
				1 => (ItemIndex)(dataSpan[index] - '0'),
				2 => (ItemIndex)((dataSpan[index] - '0') * 10 + dataSpan[index + 1] - '0'),
				_ => (ItemIndex)((dataSpan[index] - '0') * 100 + (dataSpan[index + 1] - '0') * 10 + dataSpan[index + 2] - '0')
			};
		}
		return new(array, rows, columns);
	}

	/// <inheritdoc/>
	static bool IParsable<Grid>.TryParse(string? s, IFormatProvider? provider, [NotNullWhen(true)] out Grid? result)
	{
		if (s is null)
		{
			result = null;
			return false;
		}
		return TryParse(s, out result);
	}

	/// <inheritdoc/>
	static Grid IParsable<Grid>.Parse(string s, IFormatProvider? provider) => Parse(s);


	/// <inheritdoc/>
	public static bool operator ==(Grid? left, Grid? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(Grid? left, Grid? right) => !(left == right);


	/// <summary>
	/// Implicit cast from <see cref="Grid"/> into a <see cref="ItemIndex"/>[,].
	/// </summary>
	/// <param name="grid">The grid.</param>
	public static implicit operator ItemIndex[,](Grid grid)
	{
		var result = new ItemIndex[grid.RowsLength, grid.ColumnsLength];
		for (var i = 0; i < grid.RowsLength; i++)
		{
			for (var j = 0; j < grid.ColumnsLength; j++)
			{
				result[i, j] = grid[i, j];
			}
		}
		return result;
	}

	/// <summary>
	/// Explicit cast from <see cref="ItemIndex"/>[,] into a <see cref="Grid"/>.
	/// </summary>
	/// <param name="array">An array.</param>
	public static explicit operator Grid(ItemIndex[,] array) => new(array);
}
