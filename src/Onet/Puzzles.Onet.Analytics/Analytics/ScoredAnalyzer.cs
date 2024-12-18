namespace Puzzles.Onet.Analytics;

/// <summary>
/// Represents an analyzer that will find a list of steps, with scoring rules that makes each step have a minimal one.
/// </summary>
public sealed class ScoredAnalyzer
{
	/// <summary>
	/// Indicates the backing collector.
	/// </summary>
	private readonly Collector _collector = new();


	/// <summary>
	/// Try to analyze the grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="maxBranchesCount">The maximum branches count.</param>
	/// <param name="startPointCreator">
	/// The start point to be checked. The default is <see langword="null"/> (un-assigned).
	/// It will be initialized as the center of the grid (half width and height).
	/// The default is <see langword="null"/>.
	/// </param>
	/// <param name="distanceType">
	/// Indicates the distance type to be checked. The default is <see cref="DistanceType.Manhattan"/>.
	/// </param>
	/// <param name="distanceWeight">Indicates the distance weight. The default is 10.</param>
	/// <param name="visualDistanceWeight">Indicates the visual distance weight. The default is 1.</param>
	/// <returns>Returns an array of steps, and the internal and visual distance values.</returns>
	public ReadOnlySpan<SolvingPath> Analyze(
		Grid grid,
		int maxBranchesCount,
		Func<(double X, double Y)>? startPointCreator = null,
		DistanceType distanceType = DistanceType.Manhattan,
		int distanceWeight = 10,
		int visualDistanceWeight = 1
	)
	{
		var width = grid.RowsLength;
		var height = grid.ColumnsLength;
		var halfWidth = width / 2D;
		var halfHeight = height / 2D;
		startPointCreator ??= () => (halfWidth, halfHeight);

		var playground = grid.Clone();
		var queue = new LinkedList<SolvingPathNode>();
		queue.AddLast(new SolvingPathNode(playground));

		var paths = new List<SolvingPath>(maxBranchesCount);
		while (queue.Count != 0)
		{
			var node = queue.RemoveFirstNode();
			_ = (node.Match is { End: var (x, y) } ? (X: x, Y: y) : startPointCreator()) is var (px, py);

			var branches = new SortedDictionary<double, List<SolvingPathNode>>();
			foreach (var match in _collector.Collect(node.GridState))
			{
				var currentState = node.GridState.Clone();
				currentState.Apply(match);

				foreach (var isReversed in (false, true))
				{
					var newMatch = isReversed ? ~match : match;
					_ = (newMatch.Start.X, newMatch.Start.Y) is (var nx, var ny) ns;
					_ = (newMatch.End.X, newMatch.End.Y) is (var nex, var ney) ne;
					var visualDistance = distanceType switch
					{
						DistanceType.Euclid => Math.Sqrt((px - nx) * (px - nx) + (py - ny) * (py - ny)),
						DistanceType.Manhattan => Math.Abs(px - nx) + Math.Abs(py - ny),
						_ => 0
					};
					var distance = distanceType switch
					{
						DistanceType.Euclid => Math.Sqrt((nex - nx) * (nex - nx) + (ney - ny) * (ney - ny)),
						DistanceType.Manhattan => Math.Abs(nex - nx) + Math.Abs(ney - ny),
						DistanceType.Solved => getSolvedDistance(
							ns,
							ne,
							from interim in newMatch.Interims
							select ((double, double))(interim.X, interim.Y)
						),
						_ => 0
					};
					var difficulty = visualDistance * visualDistanceWeight + distance * distanceWeight;
					var newNode = new SolvingPathNode(newMatch, currentState, difficulty, node);
					if (!branches.TryAdd(difficulty, [newNode]))
					{
						branches[difficulty].Add(newNode);
					}
				}
			}

			// Find for the minimal-scored match, and determine the next branches.
			if (branches.Keys.FirstOrDefault(-1) is not (var minimalKey and >= 0))
			{
				continue;
			}

			foreach (var childNode in branches[minimalKey])
			{
				if (!childNode.GridState.IsEmpty)
				{
					queue.AddLast(childNode);
					continue;
				}

				// The puzzle has already finished.
				paths.Add(new(childNode));
				if (paths.Count == maxBranchesCount)
				{
					return paths.AsSpan();
				}
			}
		}

		// Not enough branches found.
		return paths.AsSpan();


		static double getSolvedDistance((double X, double Y) start, (double X, double Y) end, ReadOnlySpan<(double X, double Y)> interims)
		{
#pragma warning disable format
			return interims switch
			{
				[var a, var b] => length(start, a) + length(a, b) + length(b, end),
				[var a] => length(start, a) + length(a, end),
				[] => length(start, end),
				_ => throw new InvalidOperationException("The internal data is invalid.")
			};
#pragma warning restore format


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static double length((double X, double Y) coordinate1, (double X, double Y) coordinate2)
				=> Math.Abs(coordinate1.X - coordinate2.X) + Math.Abs(coordinate1.Y - coordinate2.Y);
		}
	}
}
