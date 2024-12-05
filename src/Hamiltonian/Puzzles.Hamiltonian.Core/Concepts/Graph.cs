namespace Puzzles.Hamiltonian.Concepts;

/// <summary>
/// Represents a Hamiltonian graph.
/// </summary>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class Graph :
	ICloneable,
	IEquatable<Graph>,
	IEqualityOperators<Graph, Graph, bool>,
	IFormattable,
	IParsable<Graph>,
	IReadOnlyCollection<bool>,
	IReadOnlyList<bool>
{
	/// <summary>
	/// Indicates the sequence of the graph.
	/// </summary>
	[EquatableMember]
	private readonly BitArray _sequence;


	/// <summary>
	/// Initializes a <see cref="Graph"/> instance via the specified number of rows and columns,
	/// treating all cells as "off" state. 
	/// </summary>
	/// <param name="rows">Indicates the number of rows.</param>
	/// <param name="columns">Indicates the number of columns.</param>
	public Graph(int rows, int columns)
	{
		(RowsLength, ColumnsLength, _sequence) = (rows, columns, new(rows * columns));
		_sequence.SetAll(false);
	}

	/// <summary>
	/// Initializes a <see cref="Graph"/> instance via an 1D array of <see cref="bool"/> values indicating the status of each cell.
	/// </summary>
	/// <param name="array">An 1D array of <see cref="bool"/> values.</param>
	/// <param name="rows">Indicates the number of rows.</param>
	/// <param name="columns">Indicates the number of columns.</param>
	public Graph(bool[] array, int rows, int columns)
	{
		(RowsLength, ColumnsLength) = (rows, columns);
		_sequence = new(array);
	}

	/// <summary>
	/// Initializes a <see cref="Graph"/> instance via a 2D array of <see cref="bool"/> values indicating the status of each cell.
	/// </summary>
	/// <param name="array">A 2D array of <see cref="bool"/> values.</param>
	public Graph(bool[,] array) : this(array.Flat(), array.GetLength(0), array.GetLength(1))
	{
	}

	/// <summary>
	/// Copies a list of bits from the specified bit array.
	/// </summary>
	/// <param name="bitArray">The bit array.</param>
	/// <param name="rows">Indicates the number of rows.</param>
	/// <param name="columns">Indicates the number of columns.</param>
	private Graph(BitArray bitArray, int rows, int columns)
	{
		(RowsLength, ColumnsLength) = (rows, columns);
		_sequence = (BitArray)bitArray.Clone();
	}


	/// <summary>
	/// Indicates whether the graph is empty.
	/// </summary>
	public bool IsEmpty => Length == 0;

	/// <summary>
	/// Indicates whether the graph is square.
	/// </summary>
	public bool IsSquare => RowsLength == ColumnsLength;

	/// <summary>
	/// Indicates the number of cells used.
	/// </summary>
	public int Length => _sequence.GetCardinality();

	/// <summary>
	/// Indicates the size of the graph (the number of cells used).
	/// </summary>
	public int Size => _sequence.Count;

	/// <summary>
	/// Indicates the number of rows used.
	/// </summary>
	public int RowsLength { get; }

	/// <summary>
	/// Indicates the number of columns used.
	/// </summary>
	public int ColumnsLength { get; }

	/// <inheritdoc/>
	int IReadOnlyCollection<bool>.Count => Length;


	/// <summary>
	/// Gets or sets the state at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The boolean state to get or set.</returns>
	public bool this[int index]
	{
		get => _sequence[index];

		set => _sequence[index] = value;
	}

	/// <inheritdoc cref="this[int]"/>
	public bool this[Coordinate index]
	{
		get => _sequence[index.ToIndex(this)];

		set => _sequence[index.ToIndex(this)] = value;
	}


	/// <summary>
	/// Converts the current graph into a <see cref="bool"/> 2D table indicating which cells are used.
	/// </summary>
	/// <returns>A 2D array of <see cref="bool"/> values.</returns>
	public bool[,] ToBooleanArray()
	{
		var result = new bool[RowsLength, ColumnsLength];
		for (var i = 0; i < RowsLength; i++)
		{
			for (var j = 0; j < ColumnsLength; j++)
			{
				result[i, j] = _sequence[i * ColumnsLength + j];
			}
		}
		return result;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var length = (_sequence.Count & 7) == 0 ? _sequence.Count >> 3 : (_sequence.Count >> 3) + 1;
		var byteSequence = new byte[length];
		_sequence.CopyTo(byteSequence, 0);

		var hashCode = new HashCode();
		hashCode.AddBytes(byteSequence);
		return hashCode.ToHashCode();
	}

	/// <summary>
	/// Gets the degree value at the specified coordinate.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	/// <returns>The degree value. The return value must be 2, 3 or 4.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetDegreeAt(Coordinate coordinate)
	{
		var result = 0;
		if (!coordinate.Up.IsOutOfBound(this) && this[coordinate.Up])
		{
			result++;
		}
		if (!coordinate.Down.IsOutOfBound(this) && this[coordinate.Down])
		{
			result++;
		}
		if (!coordinate.Left.IsOutOfBound(this) && this[coordinate.Left])
		{
			result++;
		}
		if (!coordinate.Right.IsOutOfBound(this) && this[coordinate.Right])
		{
			result++;
		}
		return result;
	}

	/// <inheritdoc/>
	public override string ToString() => ToString("bs");

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(string? format)
	{
		GraphFormatInfo formatter = format switch
		{
			null or "bs" => new OnOffGraphFormatInfo(),
			"b" => new OnOffGraphFormatInfo { WithSize = false },
			"c" => new TableGraphFormatInfo(),
			_ => throw new FormatException()
		};
		return formatter.FormatCore(this);
	}

	/// <summary>
	/// Slices a row, and integrates the values into a sequence of <see cref="bool"/> values.
	/// </summary>
	/// <param name="row">The desired row label.</param>
	/// <returns>A list of <see cref="bool"/> result sliced.</returns>
	public ReadOnlySpan<bool> SliceRow(int row)
	{
		var result = new bool[ColumnsLength];
		var startIndex = row * ColumnsLength;
		for (var i = 0; i < ColumnsLength; i++)
		{
			result[i] = _sequence[startIndex++];
		}
		return result;
	}

	/// <summary>
	/// Slices a column, and integrates the values into a sequence of <see cref="bool"/> values.
	/// </summary>
	/// <param name="column">The desired column label.</param>
	/// <returns>A list of <see cref="bool"/> result sliced.</returns>
	public ReadOnlySpan<bool> SliceColumn(int column)
	{
		var result = new bool[RowsLength];
		var startIndex = column;
		for (var i = 0; i < RowsLength; i++, startIndex += ColumnsLength)
		{
			result[i] = _sequence[startIndex];
		}
		return result;
	}

	/// <summary>
	/// Returns an enumerator that iterates on each bits of the sequence.
	/// </summary>
	/// <returns>An enumerator that iterates on each bits of the sequence.</returns>
	public Enumerator GetEnumerator() => new(_sequence);

	/// <summary>
	/// Returns an enumerator that iterates on each coordinates of cells used.
	/// </summary>
	/// <returns>An enumerator that iterates on each coordinates of cells used.</returns>
	public CoordinateEnumerator EnumerateCoordinates() => new(_sequence, ColumnsLength);

	/// <inheritdoc cref="ICloneable.Clone"/>
	public Graph Clone() => new(_sequence, RowsLength, ColumnsLength);

	/// <summary>
	/// Returns a read-only bit array.
	/// </summary>
	/// <returns>The bit array.</returns>
	public BitArray ToBitArray() => (BitArray)_sequence.Clone();

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(format);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<bool>)this).GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
	{
		for (var i = 0; i < Size; i++)
		{
			yield return _sequence[i];
		}
	}


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(string? s, [NotNullWhen(true)] out Graph? result) => TryParse(s, null, out result);

	/// <inheritdoc/>
	public static bool TryParse(string? s, IFormatProvider? provider, [NotNullWhen(true)] out Graph? result)
	{
		try
		{
			if (s is null)
			{
				throw new FormatException();
			}
			result = Parse(s, provider);
			return true;
		}
		catch (FormatException)
		{
			result = null;
			return false;
		}
	}

	/// <summary>
	/// Generates an empty graph with the specified number of rows and columns.
	/// </summary>
	/// <param name="rows">The rows.</param>
	/// <param name="columns">The columns.</param>
	/// <returns>A graph.</returns>
	public static Graph Empty(int rows, int columns) => Parse($"{rows}:{columns}");

	/// <inheritdoc cref="Parse(string)"/>
	public static Graph Parse(ReadOnlySpan<char> s) => Parse(s.ToString());

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	/// <remarks>
	/// <para>
	/// Match format:
	/// <code><![CDATA[<row>:<column>:<data>]]></code>
	/// </para>
	/// <para>
	/// Meaning:
	/// <list type="bullet">
	/// <item><c>row</c>: The number of rows is <c>row</c>.</item>
	/// <item><c>column</c>: The number of columns is <c>column</c>.</item>
	/// <item><c>data</c>: Detail of on/off graph sequence (Use characters <c>'0'</c> and <c>'1'</c>; 0 - used, 1 - unused).</item>
	/// <item><c>colon token ':'</c>: Separator.</item>
	/// </list>
	/// </para>
	/// <para><c>data</c> can be omitted. If <c>data</c> is omitted, the generated graph will set all cells "off".</para>
	/// </remarks>
	public static Graph Parse(string s) => Parse(s, null);

	/// <inheritdoc/>
	public static Graph Parse(string s, IFormatProvider? provider)
		=> provider switch { GraphFormatInfo g => g.ParseCore(s), _ => new OnOffGraphFormatInfo().ParseCore(s) };


	/// <summary>
	/// Adds a new coordinate into the graph.
	/// </summary>
	/// <param name="base">The base graph.</param>
	/// <param name="coordinate">The coordinate.</param>
	/// <returns>The new graph with the specified coordinate added.</returns>
	public static Graph operator +(Graph @base, Coordinate coordinate)
	{
		var result = @base.Clone();
		result[coordinate] = true;
		return result;
	}

	/// <summary>
	/// Removes a coordinate from the graph.
	/// </summary>
	/// <param name="base">The base graph.</param>
	/// <param name="coordinate">The coordinate.</param>
	/// <returns>The new graph with the specified coordinate removed.</returns>
	public static Graph operator -(Graph @base, Coordinate coordinate)
	{
		var result = @base.Clone();
		result[coordinate] = false;
		return result;
	}


	/// <summary>
	/// Implicit cast from a <see cref="Graph"/> instance into a <see cref="bool"/>[].
	/// </summary>
	/// <param name="graph">A graph.</param>
	public static implicit operator bool[](Graph graph) => graph.ToBooleanArray().Flat();

	/// <summary>
	/// Implicit cast from a <see cref="Graph"/> instance into a <see cref="bool"/>[,].
	/// </summary>
	/// <param name="graph">A graph.</param>
	public static implicit operator bool[,](Graph graph) => graph.ToBooleanArray();

	/// <summary>
	/// Implicit cast from a <see cref="Graph"/> instance into a <see cref="BitArray"/>.
	/// </summary>
	/// <param name="graph">A graph.</param>
	public static implicit operator BitArray(Graph graph) => graph.ToBitArray();

	/// <summary>
	/// Explicit cast from a <see cref="bool"/>[,] instance into a <see cref="Graph"/>.
	/// </summary>
	/// <param name="array">An array.</param>
	public static explicit operator Graph(bool[,] array) => new(array);
}
