namespace Puzzles.Hamiltonian.Transforming;

/// <summary>
/// Provides a way to transform a <see cref="Graph"/>.
/// </summary>
/// <seealso cref="Graph"/>
public static class GraphTransformation
{
	/// <summary>
	/// Rotate a coordinate clockwise.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	/// <param name="rows">The number of rows.</param>
	/// <param name="columns">The number of columns.</param>
	/// <returns>The result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Coordinate RotateClockwise(this Coordinate coordinate, int rows, int columns)
		=> new(columns - coordinate.RowIndex - 1, coordinate.ColumnIndex);

	/// <summary>
	/// Rotate a path clockwise.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <param name="rows">The number of rows.</param>
	/// <param name="columns">The number of columns.</param>
	/// <returns>The result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path RotateClockwise(this Path path, int rows, int columns)
		=> [.. from coordinate in path.Span select coordinate.RotateClockwise(rows, columns)];

	/// <summary>
	/// Rotate a graph clockwise.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The result.</returns>
	public static Graph RotateClockwise(this Graph graph)
	{
		var matrix = (bool[,])graph;
		var rows = graph.ColumnsLength;
		var columns = graph.RowsLength;

		// 1 2 3      7 4 1
		// 4 5 6  ->  8 5 2
		// 7 8 9      9 6 3
		var result = new bool[rows, columns];
		for (var i = 0; i < rows; i++)
		{
			for (var j = 0; j < columns; j++)
			{
				result[i, j] = matrix[columns - j - 1, i];
			}
		}
		return new(result);
	}

	/// <summary>
	/// Rotate a graph counter-clockwise.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The result.</returns>
	public static Graph RotateCounterclockwise(this Graph graph)
	{
		var matrix = (bool[,])graph;
		var rows = graph.ColumnsLength;
		var columns = graph.RowsLength;

		// 1 2 3      3 6 9
		// 4 5 6  ->  2 5 8
		// 7 8 9      1 4 7
		var result = new bool[rows, columns];
		for (var i = 0; i < graph.RowsLength; i++)
		{
			for (var j = 0; j < graph.ColumnsLength; j++)
			{
				result[rows - j - 1, i] = matrix[i, j];
			}
		}
		return new(result);
	}

	/// <summary>
	/// Rotate a graph 180 degrees.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The result.</returns>
	/// <remarks><i>This method just calls method <see cref="RotateClockwise(Graph)"/> twice.</i></remarks>
	/// <seealso cref="RotateClockwise(Graph)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Graph RotatePi(this Graph graph) => graph.RotateClockwise().RotateClockwise();

	/// <summary>
	/// Mirror left-right the coordinate.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	/// <param name="rows">The number of rows.</param>
	/// <param name="columns">The number of columns.</param>
	/// <returns>The result graph mirrored.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Coordinate MirrorLeftRight(this Coordinate coordinate, int rows, int columns)
		=> new(coordinate.ColumnIndex, columns - 1 - coordinate.RowIndex);

	/// <summary>
	/// Mirror left-right the graph.
	/// </summary>
	/// <param name="path">The graph.</param>
	/// <param name="rows">The number of rows.</param>
	/// <param name="columns">The number of columns.</param>
	/// <returns>The result graph mirrored.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path MirrorLeftRight(this Path path, int rows, int columns)
		=> [.. from coordinate in path.Span select coordinate.MirrorLeftRight(rows, columns)];

	/// <summary>
	/// Mirror left-right the graph.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The result graph mirrored.</returns>
	public static Graph MirrorLeftRight(this Graph graph)
	{
		var rows = graph.RowsLength;
		var columns = graph.ColumnsLength;
		var result = new Graph(rows, columns);
		for (var i = 0; i < rows; i++)
		{
			for (var j = 0; j < columns; j++)
			{
				result[i * columns + j] = graph[i * columns + (columns - 1 - j)];
			}
		}
		return result;
	}

	/// <summary>
	/// Mirror top-bottom the graph.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The result graph mirrored.</returns>
	public static Graph MirrorTopBottom(this Graph graph)
	{
		var rows = graph.RowsLength;
		var columns = graph.ColumnsLength;
		var result = new Graph(rows, columns);
		for (var i = 0; i < rows; i++)
		{
			for (var j = 0; j < columns; j++)
			{
				result[i * columns + j] = graph[(columns - 1 - i) * columns + j];
			}
		}
		return result;
	}

	/// <summary>
	/// Mirror diagonal the graph, or throws <see cref="ArgumentOutOfRangeException"/> if the graph is not a square.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The result graph mirrored.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the graph is not square.</exception>
	public static Graph MirrorDiagonal(this Graph graph)
	{
		ArgumentOutOfRangeException.ThrowIfNotEqual(graph.IsSquare, true);

		var rows = graph.RowsLength;
		var columns = graph.ColumnsLength;
		var result = new Graph(rows, columns);
		for (var i = 0; i < rows; i++)
		{
			for (var j = 0; j < columns; j++)
			{
				result[i * columns + j] = graph[j * columns + i];
			}
		}
		return result;
	}

	/// <summary>
	/// Mirror anti-diagonal the graph, or throws <see cref="ArgumentOutOfRangeException"/> if the graph is not a square.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The result graph mirrored.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the graph is not square.</exception>
	public static Graph MirrorAntidiagonal(this Graph graph)
	{
		ArgumentOutOfRangeException.ThrowIfNotEqual(graph.IsSquare, true);

		var rows = graph.RowsLength;
		var columns = graph.ColumnsLength;
		var result = new Graph(rows, columns);
		for (var i = 0; i < rows; i++)
		{
			for (var j = 0; j < columns; j++)
			{
				result[i * columns + j] = graph[(columns - 1 - j) * columns + (columns - 1 - i)];
			}
		}
		return result;
	}
}
