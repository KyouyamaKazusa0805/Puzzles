namespace Puzzles.Onet.Analytics;

/// <summary>
/// Represents a tree that describes all branches of trial of a puzzle.
/// </summary>
/// <param name="rootNode">Indicates the root node.</param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.EqualityOperators)]
public sealed partial class BruteForceTree([Property, HashCodeMember] BruteForceNode rootNode) :
	IDataStructure,
	IEnumerable<BruteForceNode>,
	IEquatable<BruteForceTree>,
	IEqualityOperators<BruteForceTree, BruteForceTree, bool>
{
	/// <summary>
	/// Indicates the puzzle.
	/// </summary>
	public Grid Puzzle => RootNode.CurrentState;

	/// <inheritdoc/>
	DataStructureType IDataStructure.Type => DataStructureType.Tree;

	/// <inheritdoc/>
	DataStructureBase IDataStructure.Base => DataStructureBase.LinkedListBased;


	/// <summary>
	/// Determine whether the tree contains the specified node.
	/// </summary>
	/// <param name="node">The node to be checked.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public bool Contains(BruteForceNode node)
	{
		foreach (var n in this)
		{
			if (n == node)
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] BruteForceTree? other)
	{
		return other is not null && equals(RootNode, other.RootNode);


		static bool equals(BruteForceNode a, BruteForceNode b)
		{
			if (a.Children.Count != b.Children.Count)
			{
				return false;
			}

			if (a != b)
			{
				return false;
			}

			var count = a.Children.Count;
			for (var i = 0; i < count; i++)
			{
				if (!equals(a.Children[i], b.Children[i]))
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>
	/// Returns the number of nodes used.
	/// </summary>
	/// <returns>The number of nodes.</returns>
	public int GetNodesCount()
	{
		var result = 0;
		foreach (var _ in this)
		{
			result++;
		}
		return result;
	}

	/// <summary>
	/// Returns the maximum depth of the puzzle.
	/// </summary>
	/// <returns>The maximum depth of the puzzle.</returns>
	/// <seealso href="https://leetcode.cn/problems/maximum-depth-of-n-ary-tree/">LeetCode - Maximum depth of a n-ary tree</seealso>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetMaximumDepth()
	{
		return d(RootNode);


		static int d(BruteForceNode node)
		{
			if (node is null)
			{
				return 0;
			}

			var maxDepth = 0;
			foreach (var child in node.Children)
			{
				var childDepth = d(child);
				maxDepth = Math.Max(maxDepth, childDepth);
			}
			return maxDepth + 1;
		}
	}

	/// <summary>
	/// Returns the success rate of the puzzle.
	/// </summary>
	/// <returns>Success rate.</returns>
	public double GetSuccessRate()
	{
		var (times, successTimes) = (0, 0);
		foreach (var node in GetLeafNodes())
		{
			times++;
			if (node.CurrentState.IsEmpty)
			{
				successTimes++;
			}
		}
		return successTimes / (double)times;
	}

	/// <inheritdoc/>
	public IEnumerator<BruteForceNode> GetEnumerator()
	{
		foreach (var node in GetDescendantNodes(RootNode, true))
		{
			yield return node;
		}
	}

	/// <summary>
	/// Enumerates all nodes that belongs to the current node.
	/// </summary>
	/// <param name="node">The node.</param>
	/// <returns>The descendant nodes.</returns>
	/// <exception cref="InvalidOperationException">Throws when the current node is not inside the current tree.</exception>
	public IEnumerable<BruteForceNode> DescendantNodes(BruteForceNode node) => GetDescendantNodes(node, false);

	/// <summary>
	/// Enumerates all nodes that belongs to the current node, and itself.
	/// </summary>
	/// <param name="node">The node.</param>
	/// <returns>The descendant nodes and itself.</returns>
	/// <exception cref="InvalidOperationException">Throws when the current node is not inside the current tree.</exception>
	public IEnumerable<BruteForceNode> DescendantNodesAndSelf(BruteForceNode node) => GetDescendantNodes(node, true);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Try to get all leaf nodes.
	/// </summary>
	/// <returns>The leaf nodes.</returns>
	private IEnumerable<BruteForceNode> GetLeafNodes()
	{
		foreach (var element in this)
		{
			if (element.Children.Count == 0)
			{
				yield return element;
			}
		}
	}

	/// <summary>
	/// Try to iterate all nodes and its descendant nodes.
	/// </summary>
	/// <param name="node">The node.</param>
	/// <param name="yieldSelf">Indicates whether the node itself can be produced as a part of result.</param>
	/// <returns>A list of nodes.</returns>
	/// <exception cref="InvalidOperationException">Throws when the node is not inside the current tree.</exception>
	private IEnumerable<BruteForceNode> GetDescendantNodes(BruteForceNode node, bool yieldSelf)
	{
		if (!Contains(node))
		{
			throw new InvalidOperationException("The node is not inside the current tree.");
		}

		var queue = new LinkedList<BruteForceNode>();
		queue.AddLast(node);

		while (queue.Count != 0)
		{
			var n = queue.RemoveFirstNode();
			if (n == node && yieldSelf || n != node)
			{
				yield return n;
			}

			foreach (var child in n.Children)
			{
				queue.AddLast(child);
			}
		}
	}
}
