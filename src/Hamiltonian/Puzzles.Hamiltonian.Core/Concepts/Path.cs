namespace Puzzles.Hamiltonian.Concepts;

/// <summary>
/// Provides a path of a Hamiltonian graph.
/// </summary>
[CollectionBuilder(typeof(Path), nameof(Create))]
public sealed class Path :
	IEquatable<Path>,
	IEqualityOperators<Path, Path, bool>,
	IParsable<Path>,
	IReadOnlyCollection<Coordinate>,
	IReadOnlyList<Coordinate>
{
	/// <summary>
	/// Indicates the coordinates.
	/// </summary>
	private readonly Coordinate[] _coordinates;


	/// <summary>
	/// Initializes a <see cref="Path"/> instance via the specified path.
	/// </summary>
	/// <param name="coordinates">The coordinates.</param>
	public Path(params Coordinate[] coordinates) => _coordinates = coordinates;

	/// <summary>
	/// Initializes a <see cref="Path"/> instance via the specified node as the last node.
	/// </summary>
	/// <param name="lastNode">The last node.</param>
	internal Path(CoordinateNode lastNode)
	{
		var nodes = new Stack<Coordinate>();
		for (var node = lastNode; node is not null; node = node.Parent)
		{
			nodes.Push(node.Coordinate);
		}
		_coordinates = [.. nodes];
	}


	/// <summary>
	/// Indicates the length of the coordinates used.
	/// </summary>
	public int Length => _coordinates.Length;

	/// <summary>
	/// Indicates the directions of the path.
	/// </summary>
	public ReadOnlySpan<Direction> Directions
	{
		get
		{
			var result = new List<Direction>();
			for (var i = 0; i < Length - 1; i++)
			{
				result.Add(_coordinates[i + 1] - _coordinates[i]);
			}
			return result.AsSpan();
		}
	}

	/// <summary>
	/// Returns the backing elements, integrated as a <see cref="ReadOnlySpan{T}"/>.
	/// </summary>
	public ReadOnlySpan<Coordinate> Span => _coordinates;

	/// <inheritdoc/>
	int IReadOnlyCollection<Coordinate>.Count => Length;


	/// <summary>
	/// Gets the coordinate at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The coordinate.</returns>
	/// <exception cref="IndexOutOfRangeException">Throws when the index is out of range.</exception>
	public Coordinate this[int index] => _coordinates[index];


	/// <inheritdoc/>
	public override bool Equals(object? obj) => Equals(obj as Path);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] Path? other)
		=> other is not null && _coordinates.AsSpan().SequenceEqual(other._coordinates);

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var result = new HashCode();
		foreach (var element in _coordinates)
		{
			result.Add(element);
		}
		return result.ToHashCode();
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(" -> ");

	/// <summary>
	/// Converts the current instance into a <see cref="string"/> result.
	/// </summary>
	/// <param name="separator">The separator.</param>
	/// <returns>The string result.</returns>
	public string ToString(string separator)
	{
		var sb = new StringBuilder();
		foreach (var element in _coordinates)
		{
			sb.Append(element.ToString());
			sb.Append(separator);
		}
		return sb.RemoveFrom(^separator.Length).ToString();
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<Coordinate> GetEnumerator() => new(_coordinates);

	/// <summary>
	/// Converts the current path object into a <see cref="Graph"/> instance.
	/// </summary>
	/// <param name="rows">The number of rows.</param>
	/// <param name="columns">The number of columns.</param>
	/// <returns>A <see cref="Graph"/> result casted.</returns>
	public Graph AsGraph(int rows, int columns)
	{
		var result = new Graph(rows, columns);
		foreach (var coordinate in _coordinates)
		{
			result[coordinate] = true;
		}
		return result;
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _coordinates.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Coordinate> IEnumerable<Coordinate>.GetEnumerator() => _coordinates.AsEnumerable().GetEnumerator();


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(string str, [NotNullWhen(true)] out Path? result)
	{
		try
		{
			result = Parse(str);
			return true;
		}
		catch (FormatException)
		{
			result = null;
			return false;
		}
	}

	/// <inheritdoc cref="Parse(string)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path Parse(ReadOnlySpan<char> str) => Parse(str);

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	/// <remarks>
	/// Format:
	/// <code><![CDATA[
	/// <start-row>:<start-column>:<direction-arrows>
	/// ]]></code>
	/// Example:
	/// <code><![CDATA[
	/// 0:0:↓↓↓→→→↓↓←↑←←↓↓→↓←↓→→↑→↓→→↑↑←↑→→↑↑←←↑↑→↑←←↓←
	/// ]]></code>
	/// </remarks>
	public static Path Parse(string str)
	{
		var split = str.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		var startCoordinate = new Coordinate(int.Parse(split[0]) - '0', int.Parse(split[1]) - '0');
		var coordinates = new List<Coordinate> { startCoordinate };
		var currentCoordinate = startCoordinate;
		foreach (var character in split[2])
		{
			currentCoordinate >>= character;
			coordinates.Add(currentCoordinate);
		}
		return [.. coordinates];
	}

	/// <summary>
	/// Creates a <see cref="Path"/> instance via the specified coordinates.
	/// </summary>
	/// <param name="coordinates">A list of coordinates.</param>
	/// <returns>A <see cref="Path"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Path Create(ReadOnlySpan<Coordinate> coordinates) => new([.. coordinates]);

	/// <inheritdoc/>
	static Path IParsable<Path>.Parse(string s, IFormatProvider? provider) => Parse(s);

	/// <inheritdoc/>
	static bool IParsable<Path>.TryParse(string? s, IFormatProvider? provider, [NotNullWhen(true)] out Path? result)
	{
		if (s is null)
		{
			result = null;
			return false;
		}
		else
		{
			return TryParse(s, out result);
		}
	}


	/// <inheritdoc/>
	public static bool operator ==(Path? left, Path? right)
		=> (left, right) switch
		{
			(null, null) => true,
			(not null, not null) => left.Equals(right),
			_ => false
		};

	/// <inheritdoc/>
	public static bool operator !=(Path? left, Path? right) => !(left == right);
}
