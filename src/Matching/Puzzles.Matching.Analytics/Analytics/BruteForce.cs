namespace Puzzles.Matching.Analytics;

/// <summary>
/// Provides a way to enumerate all possible cases of matching.
/// </summary>
public sealed class BruteForce
{
	/// <inheritdoc cref="CreateTree(Grid, out LinkedList{InvertedBruteForceNode})"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BruteForceTree CreateTree(Grid puzzle) => CreateTree(puzzle, out _);

	/// <summary>
	/// Create a <see cref="BruteForceTree"/> to describe each steps and its related steps.
	/// </summary>
	/// <param name="puzzle">The puzzle.</param>
	/// <param name="invertedNodes">The inverted nodes.</param>
	/// <returns>A <see cref="BruteForceTree"/> instance.</returns>
	public BruteForceTree CreateTree(Grid puzzle, out LinkedList<InvertedBruteForceNode> invertedNodes)
	{
		// This is a n-ary tree, we should firstly add all possible steps found into one root node.
		invertedNodes = new LinkedList<InvertedBruteForceNode>();
		var rootNode = new InvertedBruteForceNode(puzzle.Clone());
		invertedNodes.AddLast(rootNode);

		var queue = new LinkedList<InvertedBruteForceNode>();
		foreach (var step in new HashSet<ItemMatch>([.. puzzle.GetAllMatches()]))
		{
			var puzzleApplied = puzzle.Clone();
			puzzleApplied.Apply(step);

			var node = new InvertedBruteForceNode(step, puzzleApplied, rootNode);
			queue.AddLast(node);
			invertedNodes.AddLast(node);
		}

		// Use BFS algorithm to make such exhaustive searching.
		while (queue.Count != 0)
		{
			var currentNode = queue.RemoveFirstNode();
			var (_, p, _) = currentNode;
			if (p.IsEmpty)
			{
				continue;
			}

			foreach (var step in p.GetAllMatches())
			{
				var puzzleApplied = p.Clone();
				puzzleApplied.Apply(step);

				var node = new InvertedBruteForceNode(step, puzzleApplied, currentNode);
				queue.AddLast(node);
				invertedNodes.AddLast(node);
			}
		}

		// Return the root node.
		return new(getRootNode(invertedNodes));


		static BruteForceNode getRootNode(LinkedList<InvertedBruteForceNode> nodes)
		{
			// Create a dictionary to map original nodes to new tree nodes.
			var map = new Dictionary<InvertedBruteForceNode, BruteForceNode>();

			// Populate the dictionary with new TreeNode instances.
			foreach (var node in nodes)
			{
				map[node] = new BruteForceNode(node.Step, node.CurrentPuzzle);
			}

			// Set children for each TreeNode based on the parent property.
			var root = default(BruteForceNode);
			foreach (var node in nodes)
			{
				if (node.Parent is null)
				{
					// Identify the root node.
					root = map[node];
					continue;
				}

				if (map.TryGetValue(node.Parent, out var n))
				{
					n.Children.Add(map[node]);
				}
			}

			return root!;
		}
	}
}
