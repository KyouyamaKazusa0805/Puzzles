namespace Puzzles.Onet.Analytics;

using DoublePair = (double X, double Y);

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
	/// <param name="chooseMinimal">
	/// Indicates whether we should choose the minimal one in every branch. If not, the maximal one will be chosen.
	/// </param>
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
		bool chooseMinimal = false,
		Func<DoublePair>? startPointCreator = null,
		DistanceType distanceType = DistanceType.Manhattan,
		int distanceWeight = 10,
		int visualDistanceWeight = 1
	)
	{
		var paths = new List<SolvingPath>(maxBranchesCount);
		try
		{
			DoublePair defaultStartPointCreator() => (grid.RowsLength / 2D, grid.ColumnsLength / 2D);
			analyzeRecursively(
				grid,
				grid,
				maxBranchesCount,
				chooseMinimal,
				startPointCreator ?? defaultStartPointCreator,
				distanceType,
				distanceWeight,
				visualDistanceWeight,
				new(n()),
				paths
			);
		}
		catch
		{
		}
		return paths.AsSpan();


		static bool node_Equals(SolvingPathNode? left, SolvingPathNode? right)
		{
			SolvingPathNode? n1, n2;
			for ((n1, n2) = (left, right); n1 is not null && n2 is not null; (n1, n2) = (n1.Parent, n2.Parent))
			{
				if (n1 != n2)
				{
					return false;
				}
			}
			return n1 is null && n2 is null;
		}

		static int node_GetHashCode(SolvingPathNode obj)
		{
			var hashCode = new HashCode();
			for (var node = obj; node is not null; node = node.Parent)
			{
				hashCode.Add(node);
			}
			return hashCode.ToHashCode();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double getDistance(DistanceType distanceType, ItemMatch newMatch, int nx, int ny, (int X, int Y) ns, int nex, int ney, (int X, int Y) ne)
			=> distanceType switch
			{
				DistanceType.Euclid => Math.Sqrt((nex - nx) * (nex - nx) + (ney - ny) * (ney - ny)),
				DistanceType.Manhattan => Math.Abs(nex - nx) + Math.Abs(ney - ny),
				DistanceType.Solved => getSolvedDistance(ns, ne, from i in newMatch.Interims select (DoublePair)(i.X, i.Y)),
				_ => 0
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double getVisualDistance(DistanceType distanceType, double px, double py, int nx, int ny)
			=> distanceType switch
			{
				DistanceType.Euclid => Math.Sqrt((px - nx) * (px - nx) + (py - ny) * (py - ny)),
				DistanceType.Manhattan => Math.Abs(px - nx) + Math.Abs(py - ny),
				_ => 0
			};

#pragma warning disable format
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double getSolvedDistance(DoublePair start, DoublePair end, ReadOnlySpan<DoublePair> interims)
			=> interims switch
			{
				[var a, var b] => getLength(start, a) + getLength(a, b) + getLength(b, end),
				[var a] => getLength(start, a) + getLength(a, end),
				[] => getLength(start, end),
				_ => throw new InvalidOperationException("The internal data is invalid.")
			};
#pragma warning restore format

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double getLength(DoublePair coordinate1, DoublePair coordinate2)
			=> Math.Abs(coordinate1.X - coordinate2.X) + Math.Abs(coordinate1.Y - coordinate2.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static IEqualityComparer<SolvingPathNode> n() => EqualityComparer<SolvingPathNode>.Create(node_Equals, node_GetHashCode);

		void analyzeRecursively(
			Grid initialGrid,
			Grid currentGrid,
			int maxBranchesCount,
			bool chooseMinimal,
			Func<DoublePair> startPointCreator,
			DistanceType distanceType,
			int distanceWeight,
			int visualDistanceWeight,
			HashSet<SolvingPathNode> traversedNodes,
			List<SolvingPath> paths
		)
		{
			var queue = new LinkedList<SolvingPathNode>();
			queue.AddLast(new SolvingPathNode(currentGrid.Clone()));

			var scoreComparer = Comparer<double>.Create(chooseMinimal ? static (l, r) => l.CompareTo(r) : static (l, r) => -l.CompareTo(r));
			while (queue.Count != 0)
			{
				var node = queue.RemoveFirstNode();
				_ = (node.Match is { End: var (x, y) } ? (x, y) : startPointCreator()) is var (px, py);

				var branches = new SortedDictionary<double, List<SolvingPathNode>>(scoreComparer);
				foreach (var match in _collector.Collect(node.GridState))
				{
					var currentState = node.GridState.Clone();
					currentState.Apply(match);

					foreach (var isReversed in (false, true))
					{
						var newMatch = isReversed ? ~match : match;
						_ = (newMatch.Start.X, newMatch.Start.Y) is (var nx, var ny) ns;
						_ = (newMatch.End.X, newMatch.End.Y) is (var nex, var ney) ne;
						var visualDistance = getVisualDistance(distanceType, px, py, nx, ny);
						var distance = getDistance(distanceType, newMatch, nx, ny, ns, nex, ney, ne);
						var difficulty = visualDistance * visualDistanceWeight + distance * distanceWeight;
						var newNode = new SolvingPathNode(newMatch, currentState, difficulty, node);
						if (!branches.TryAdd(difficulty, [newNode]))
						{
							branches[difficulty].Add(newNode);
						}
					}
				}

				// Find for the minimal-scored or maximal-scored match, and determine the next branches.
				if (branches.Keys.FirstOrDefault(-1) is not (var chosenKey and >= 0))
				{
					continue;
				}

				foreach (var childNode in branches[chosenKey])
				{
					if (!childNode.GridState.IsEmpty)
					{
						queue.AddLast(childNode);
						traversedNodes.Add(childNode);
						continue;
					}

					// The puzzle has already finished.
					paths.Add(new(childNode));
					if (paths.Count == maxBranchesCount)
					{
						throw new("Enough!");
					}
				}
			}

			// Here we should make a recursion:
			// If we cannot find enough number of branches, we should recursively checking for other branches from the root node,
			// and check which nodes are not used and traversed, and adding them into the queue.
			var recursionQueue = new LinkedList<SolvingPathNode>();
			recursionQueue.AddLast(new SolvingPathNode(initialGrid.Clone()));
			while (recursionQueue.Count != 0)
			{
				var node = recursionQueue.RemoveFirstNode();
				foreach (var match in _collector.Collect(node.GridState))
				{
					var currentGridState = node.GridState.Clone();
					currentGridState.Apply(match);

					// Check whether the node is recorded in traversed nodes. If not, the node is a new one.
					var newNode = new SolvingPathNode(match, currentGridState, default, node);
					if (!traversedNodes.Add(newNode))
					{
						continue;
					}

					// Do recursion.
					recursionQueue.AddLast(newNode);
					analyzeRecursively(
						initialGrid,
						currentGridState,
						maxBranchesCount,
						chooseMinimal,
						startPointCreator,
						distanceType,
						distanceWeight,
						visualDistanceWeight,
						traversedNodes,
						paths
					);
				}
			}
		}
	}
}
