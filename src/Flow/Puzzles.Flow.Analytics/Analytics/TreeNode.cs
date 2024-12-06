namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a node in searching on A* or BFS algorithm.
/// </summary>
internal unsafe struct TreeNode
{
	/// <summary>
	/// Indicates the cost to come (this field will be ignored in BFS).
	/// </summary>
	public double CostToCome;

	/// <summary>
	/// Indicates the cost to go (this field will be ignored in BFS).
	/// </summary>
	public double CostToGo;

	/// <summary>
	/// Indicates the current progress state.
	/// </summary>
	public ProgressState State;

	/// <summary>
	/// Indicates the parent of this node (can also be <see langword="null"/>).
	/// </summary>
	public TreeNode* Parent;
}
