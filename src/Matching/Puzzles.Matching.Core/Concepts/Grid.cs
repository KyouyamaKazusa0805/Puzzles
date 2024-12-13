namespace Puzzles.Matching.Concepts;

/// <summary>
/// Represents a grid of match.
/// </summary>
/// <remarks>
/// The grid will use an array of <see cref="ItemIndex"/> values to describe internal items.
/// Different elements will be represented as its <see cref="ItemIndex"/> value.
/// Using 255 (i.e. <see cref="ItemIndex.MaxValue"/> to describe an empty cell).
/// </remarks>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class Grid :
	IBoard,
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
	int IBoard.Rows => RowsLength;

	/// <inheritdoc/>
	int IBoard.Columns => ColumnsLength;

	/// <inheritdoc/>
	int IReadOnlyCollection<ItemIndex>.Count => Length;

	[EquatableMember]
	private ReadOnlySpan<ItemIndex> ArraySpan => _array;


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

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public Enumerator GetEnumerator() => new(_array);

	/// <inheritdoc cref="ICloneable.Clone"/>
	public Grid Clone() => new((byte[])_array.Clone(), RowsLength, ColumnsLength);

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
