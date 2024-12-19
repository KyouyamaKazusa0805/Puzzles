namespace Puzzles.Flow.Concepts;

/// <summary>
/// Represents a path of a flow.
/// </summary>
/// <param name="flow">Indicates the flow.</param>
/// <param name="coordinates">Indiactes the coordinates used as path.</param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class Path([Property] FlowPosition flow, [Field] params Coordinate[] coordinates) :
	IEquatable<Path>,
	IEqualityOperators<Path, Path, bool>,
	IReadOnlyList<Coordinate>
{
	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	public int Length => _coordinates.Length;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	[EquatableMember]
	public ReadOnlySpan<Coordinate> Coordinates => _coordinates;

	/// <summary>
	/// Indicates the directions used.
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

	/// <inheritdoc/>
	int IReadOnlyCollection<Coordinate>.Count => Length;

	[EquatableMember]
	private FlowPosition FlowEntry => Flow;


	/// <inheritdoc/>
	public Coordinate this[int index] => _coordinates[index];


	/// <summary>
	/// Check whether two <see cref="Path"/> overlap with each other.
	/// </summary>
	/// <param name="other">The other instance to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Overlaps(Path other) => _coordinates.Overlaps(other._coordinates);

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var hashCode = new HashCode();
		hashCode.Add(Flow);
		foreach (var coordinate in _coordinates)
		{
			hashCode.Add(coordinate);
		}
		return hashCode.ToHashCode();
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		var coordinateStrings = from coordinate in _coordinates select (coordinate.X, coordinate.Y).ToString();
		return $"Color #{Flow.Color + 1}: {string.Join(" -> ", coordinateStrings)}";
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<Coordinate> GetEnumerator() => new(_coordinates);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _coordinates.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Coordinate> IEnumerable<Coordinate>.GetEnumerator() => _coordinates.AsEnumerable().GetEnumerator();
}
