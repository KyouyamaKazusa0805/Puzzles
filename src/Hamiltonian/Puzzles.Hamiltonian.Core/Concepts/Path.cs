namespace Puzzles.Hamiltonian.Concepts;

/// <summary>
/// Provides a path of a Hamiltonian graph.
/// </summary>
[CollectionBuilder(typeof(Path), nameof(Create))]
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class Path :
	IEquatable<Path>,
	IEqualityOperators<Path, Path, bool>,
	IFormattable,
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
	[EquatableMember]
	public ReadOnlySpan<Coordinate> Span => _coordinates;

	/// <summary>
	/// Returns the backing elements, integrated as a <see cref="ReadOnlySpan{T}"/>, in reversed order.
	/// </summary>
	public ReadOnlySpan<Coordinate> SpanReversed
	{
		get
		{
			var result = (Coordinate[])_coordinates.Clone();
			result.Reverse();
			return result;
		}
	}

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
	public override string ToString() => ToString(null);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider)
		=> (formatProvider as PathFormatInfo ?? new DirectionPathFormatInfo()).FormatCore(this);

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<Coordinate> GetEnumerator() => new(_coordinates);

	/// <summary>
	/// Enumerates all coordinates.
	/// </summary>
	/// <returns>An enumerator that can iterate on each coordinate.</returns>
	public AnonymousSpanEnumerator<Coordinate> Enumerate() => GetEnumerator();

	/// <summary>
	/// Enumerates all coordinates in reversed order.
	/// </summary>
	/// <returns>An enumerator that can iterate on each coordinate.</returns>
	public AnonymousSpanEnumerator<Coordinate> EnumerateReversed()
	{
		var pathCloned = (Coordinate[])_coordinates.Clone();
		return new(pathCloned.Reverse());
	}

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
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _coordinates.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Coordinate> IEnumerable<Coordinate>.GetEnumerator() => _coordinates.AsEnumerable().GetEnumerator();


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(string str, [NotNullWhen(true)] out Path? result) => TryParse(str, null, out result);

	/// <inheritdoc/>
	public static bool TryParse(string? s, IFormatProvider? provider, [NotNullWhen(true)] out Path? result)
	{
		if (s is null)
		{
			result = null;
			return false;
		}
		else
		{
			try
			{
				result = Parse(s, provider);
				return true;
			}
			catch (FormatException)
			{
				result = null;
				return false;
			}
		}
	}

	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(ReadOnlySpan<char> str, [NotNullWhen(true)] out Path? result) => TryParse(str.ToString(), out result);

	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [NotNullWhen(true)] out Path? result)
		=> TryParse(s.ToString(), provider, out result);

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static Path Parse(string str) => Parse(str, new DirectionPathFormatInfo());

	/// <inheritdoc/>
	public static Path Parse(string s, IFormatProvider? provider)
		=> (provider as PathFormatInfo ?? new DirectionPathFormatInfo()).ParseCore(s);

	/// <inheritdoc cref="Parse(string)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path Parse(ReadOnlySpan<char> str) => Parse(str.ToString());

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path Parse(ReadOnlySpan<char> str, IFormatProvider? provider) => Parse(str.ToString(), provider);

	/// <summary>
	/// Creates a <see cref="Path"/> instance via the specified coordinates.
	/// </summary>
	/// <param name="coordinates">A list of coordinates.</param>
	/// <returns>A <see cref="Path"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Path Create(ReadOnlySpan<Coordinate> coordinates) => new([.. coordinates]);
}
