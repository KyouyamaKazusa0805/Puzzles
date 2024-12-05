namespace Puzzles.Hamiltonian.Concepts;

/// <summary>
/// Represents a coordinate.
/// </summary>
/// <param name="RowIndex">Indicates the row label.</param>
/// <param name="ColumnIndex">Indicates the column label.</param>
public readonly record struct Coordinate(int RowIndex, int ColumnIndex) :
	IEqualityOperators<Coordinate, Coordinate, bool>,
	ISubtractionOperators<Coordinate, Coordinate, Direction>
{
	/// <summary>
	/// Indicates the left cell.
	/// </summary>
	public Coordinate Up => new(RowIndex - 1, ColumnIndex);

	/// <summary>
	/// Indicates the right cell.
	/// </summary>
	public Coordinate Down => new(RowIndex + 1, ColumnIndex);

	/// <summary>
	/// Indicates the up cell.
	/// </summary>
	public Coordinate Left => new(RowIndex, ColumnIndex - 1);

	/// <summary>
	/// Indicates the down cell.
	/// </summary>
	public Coordinate Right => new(RowIndex, ColumnIndex + 1);


	/// <summary>
	/// Indicates whether the coordinate is out of bound.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool IsOutOfBound(Graph graph)
	{
		var rows = graph.RowsLength;
		var columns = graph.ColumnsLength;
		return RowIndex < 0 || RowIndex >= rows || ColumnIndex < 0 || ColumnIndex >= columns;
	}

	/// <summary>
	/// Converts the current coordinate into an absolute index.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The absolute index.</returns>
	public int ToIndex(Graph graph) => RowIndex * graph.ColumnsLength + ColumnIndex;

	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"{nameof(RowIndex)} = {RowIndex}, {nameof(ColumnIndex)} = {ColumnIndex}");
		return true;
	}


	/// <inheritdoc/>
	/// <exception cref="InvalidOperationException">
	/// Throws when the two coordinates has a gap between them, or they cannot see each other in their own direction.
	/// </exception>
	public static Direction operator -(Coordinate left, Coordinate right)
	{
		if (left == right)
		{
			return Direction.None;
		}
		else if (left.RowIndex - right.RowIndex == -1 && left.ColumnIndex == right.ColumnIndex)
		{
			return Direction.Up;
		}
		else if (left.RowIndex - right.RowIndex == 1 && left.ColumnIndex == right.ColumnIndex)
		{
			return Direction.Down;
		}
		else if (left.RowIndex == right.RowIndex && left.ColumnIndex - right.ColumnIndex == -1)
		{
			return Direction.Left;
		}
		else if (left.RowIndex == right.RowIndex && left.ColumnIndex - right.ColumnIndex == 1)
		{
			return Direction.Right;
		}
		throw new InvalidOperationException();
	}

	/// <summary>
	/// Moves the coordinate one step forward to the next coordinate by the specified direction.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	/// <param name="arrow">The direction.</param>
	/// <returns>The new coordinate.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="arrow"/> is out of range.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Coordinate operator >>(Coordinate coordinate, char arrow)
		=> arrow switch
		{
			'↑' => coordinate.Up,
			'↓' => coordinate.Down,
			'←' => coordinate.Left,
			'→' => coordinate.Right,
			_ => throw new ArgumentOutOfRangeException(nameof(arrow))
		};

	/// <summary>
	/// Moves the coordinate one step forward to the next coordinate by the specified direction.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	/// <param name="direction">The direction.</param>
	/// <returns>The new coordinate.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="direction"/> is out of range.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Coordinate operator >>(Coordinate coordinate, Direction direction) => coordinate >> direction.GetArrow();
}
