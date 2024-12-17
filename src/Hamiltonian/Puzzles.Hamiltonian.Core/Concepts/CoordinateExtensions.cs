namespace Puzzles.Hamiltonian.Concepts;

/// <summary>
/// Represents a coordinate.
/// </summary>
public static class CoordinateExtensions
{
	/// <summary>
	/// Indicates whether the coordinate is out of bound.
	/// </summary>
	/// <param name="this">The coordinate.</param>
	/// <param name="graph">The graph.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public static bool IsOutOfBound(this Coordinate @this, Graph graph)
	{
		var rows = graph.RowsLength;
		var columns = graph.ColumnsLength;
		return @this.X < 0 || @this.X >= rows || @this.Y < 0 || @this.Y >= columns;
	}

	/// <summary>
	/// Converts the current coordinate into an absolute index.
	/// </summary>
	/// <param name="this">The coordinate.</param>
	/// <param name="graph">The graph.</param>
	/// <returns>The absolute index.</returns>
	public static int ToIndex(this Coordinate @this, Graph graph) => @this.X * graph.ColumnsLength + @this.Y;
}
