namespace Puzzles.Hamiltonian.Generating;

/// <summary>
/// Represents a generator that can generate a <see cref="Graph"/> instance with a unique path.
/// </summary>
/// <param name="rows">The number of rows.</param>
/// <param name="columns">The number of columns.</param>
/// <seealso cref="Graph"/>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public readonly ref partial struct Generator(int rows, int columns)
{
	/// <summary>
	/// Indicates the random number generator.
	/// </summary>
	private readonly Random _random = new();

	/// <summary>
	/// Indicates the backing solver.
	/// </summary>
	private readonly Solver _solver = new();


	/// <summary>
	/// Indicates the number of rows should be generated.
	/// </summary>
	public int RowsLength { get; } = rows;

	/// <summary>
	/// Indicates the number of columns should be generated.
	/// </summary>
	public int ColumnsLength { get; } = columns;


	/// <summary>
	/// Generate a graph puzzle with a unique path, by using the specified cell as the start.
	/// </summary>
	/// <param name="start">The start position.</param>
	/// <param name="minFillingRate">Indicates the minimum filling rate.</param>
	/// <param name="maxFillingRate">Indicates the maximum filling rate.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the generation.</param>
	/// <returns>The result generated. If canceled, <see langword="null"/> will be returned.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the <paramref name="minFillingRate"/> or <paramref name="maxFillingRate"/> is invalid.
	/// </exception>
	public GenerationResult Generate(
		Coordinate start,
		double minFillingRate,
		double maxFillingRate,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(minFillingRate, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(minFillingRate, 1);
		ArgumentOutOfRangeException.ThrowIfLessThan(maxFillingRate, 0);
		ArgumentOutOfRangeException.ThrowIfEqual(minFillingRate > maxFillingRate, true);

		if (maxFillingRate > 1)
		{
			maxFillingRate = 1;
		}

		while (true)
		{
			var graph = new Graph(RowsLength, ColumnsLength);
			var coordinates = new Stack<Coordinate>();
			var size = _random.Next((int)(RowsLength * ColumnsLength * minFillingRate), (int)(RowsLength * ColumnsLength * maxFillingRate));
			try
			{
				dfs(in this, graph, start, start, coordinates, size, cancellationToken);
				if (graph.IsEmpty)
				{
					continue;
				}
			}
			catch (OperationCanceledException)
			{
				goto NextLoop;
			}
			catch (InvalidOperationException)
			{
				continue;
			}
			catch
			{
			}

			// Verify the uniqueness of the puzzle.
			if (!_solver.IsValid(graph, start, null, out var resultPath))
			{
				goto NextLoop;
			}

			// Check whether at least one row or column is empty (out of using).
			var flag = true;
			for (var i = 0; i < graph.RowsLength; i++)
			{
				if (graph.SliceRow(i).All(booleanCondition))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				for (var i = 0; i < graph.ColumnsLength; i++)
				{
					if (graph.SliceColumn(i).All(booleanCondition))
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				continue;
			}
			return new(true, graph, start, coordinates.Peek(), resultPath);

		NextLoop:
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
		}
		return new(false, default, default, default, default);


		static bool booleanCondition(bool element) => !element;

		static void dfs(
			ref readonly Generator @this,
			Graph graph,
			Coordinate start,
			Coordinate current,
			Stack<Coordinate> coordinates,
			int size,
			CancellationToken cancellationToken = default
		)
		{
			graph[current] = true;
			coordinates.Push(current);

			if (!graph.IsEmpty && !@this._solver.IsValid(graph, start, current, out _))
			{
				goto Backtrack;
			}

			if (coordinates.Count >= size)
			{
				throw new();
			}

			// Check validity of four directions.
			var directions = new List<Direction>(4);
			if (!current.Up.IsOutOfBound(graph) && !graph[current.Up])
			{
				directions.Add(Direction.Up);
			}
			if (!current.Down.IsOutOfBound(graph) && !graph[current.Down])
			{
				directions.Add(Direction.Down);
			}
			if (!current.Left.IsOutOfBound(graph) && !graph[current.Left])
			{
				directions.Add(Direction.Left);
			}
			if (!current.Right.IsOutOfBound(graph) && !graph[current.Right])
			{
				directions.Add(Direction.Right);
			}
			if (directions.Count == 0)
			{
				// No directions available.
				goto Backtrack;
			}

			var nextDirection = directions[@this._random.Next(0, directions.Count)];
			dfs(in @this, graph, start, current >> nextDirection, coordinates, size, cancellationToken);

		Backtrack:
			// If here, the path won't be okay.
			graph[current] = false;
			coordinates.Pop();
			cancellationToken.ThrowIfCancellationRequested();
		}
	}
}
