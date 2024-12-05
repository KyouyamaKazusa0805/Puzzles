namespace Puzzles.Hamiltonian.Concepts;

/// <summary>
/// Represents a coordinate.
/// </summary>
/// <param name="X">Indicates the row label.</param>
/// <param name="Y">Indicates the column label.</param>
public readonly record struct Coordinate(int X, int Y) :
	IEqualityOperators<Coordinate, Coordinate, bool>,
	ISubtractionOperators<Coordinate, Coordinate, Direction>
{
	/// <summary>
	/// Indicates the left cell.
	/// </summary>
	public Coordinate Up => new(X - 1, Y);

	/// <summary>
	/// Indicates the right cell.
	/// </summary>
	public Coordinate Down => new(X + 1, Y);

	/// <summary>
	/// Indicates the up cell.
	/// </summary>
	public Coordinate Left => new(X, Y - 1);

	/// <summary>
	/// Indicates the down cell.
	/// </summary>
	public Coordinate Right => new(X, Y + 1);


	/// <summary>
	/// Indicates whether the coordinate is out of bound.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool IsOutOfBound(Graph graph)
	{
		var rows = graph.RowsLength;
		var columns = graph.ColumnsLength;
		return X < 0 || X >= rows || Y < 0 || Y >= columns;
	}

	/// <summary>
	/// Converts the current coordinate into an absolute index.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The absolute index.</returns>
	public int ToIndex(Graph graph) => X * graph.ColumnsLength + Y;

	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"{nameof(X)} = {X}, {nameof(Y)} = {Y}");
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
		else if (left.X - right.X == -1 && left.Y == right.Y)
		{
			return Direction.Up;
		}
		else if (left.X - right.X == 1 && left.Y == right.Y)
		{
			return Direction.Down;
		}
		else if (left.X == right.X && left.Y - right.Y == -1)
		{
			return Direction.Left;
		}
		else if (left.X == right.X && left.Y - right.Y == 1)
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
	public static Coordinate operator >>(Coordinate coordinate, Direction direction)
		=> direction switch
		{
			Direction.Up => coordinate.Up,
			Direction.Down => coordinate.Down,
			Direction.Left => coordinate.Left,
			Direction.Right => coordinate.Right,
			_ => throw new ArgumentOutOfRangeException(nameof(direction))
		};
}
