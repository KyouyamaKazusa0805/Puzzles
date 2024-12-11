namespace Puzzles.WaterSort.Analytics;

/// <summary>
/// Provides a way to make an exhaustive search to find for all possible valid steps in the whole solving environment.
/// </summary>
public sealed class BruteForce
{
	/// <summary>
	/// Indicates the collector.
	/// </summary>
	private readonly Collector _collector = new();


	/// <summary>
	/// Create a <see cref="BruteForceNode"/> to describe each steps and its related steps.
	/// </summary>
	/// <param name="puzzle">The puzzle.</param>
	/// <returns>A <see cref="BruteForceNode"/> instance.</returns>
	public BruteForceNode CreateTree(Puzzle puzzle)
	{
		// This is a n-ary tree, we should firstly add all possible steps found into one root node.
		var rootNode = new BruteForceNode(puzzle.Clone());
		var queue = new LinkedList<BruteForceNode>();
		foreach (var step in _collector.Collect(puzzle))
		{
			var puzzleApplied = puzzle.Clone();
			puzzleApplied.Apply(step);

			var node = new BruteForceNode(step, puzzleApplied, rootNode);
			queue.AddLast(node);
		}

		// Use BFS algorithm to make such exhaustive searching.
		while (queue.Count != 0)
		{
			var currentNode = queue.RemoveFirstNode();
			var (_, p, _) = currentNode;
			if (p.IsSolved)
			{
				continue;
			}

			var copied = p.Clone();
			var foundSteps = _collector.Collect(copied);
			foreach (var step in foundSteps)
			{
				var puzzleApplied = copied.Clone();
				puzzleApplied.Apply(step);

				var node = new BruteForceNode(step, puzzleApplied, currentNode);
				queue.AddLast(node);
			}
		}

		// Return the root node.
		return rootNode;
	}
}
