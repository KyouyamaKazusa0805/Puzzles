namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents data structure for node storage.
/// </summary>
internal unsafe struct NodeStorage
{
	/// <summary>
	/// Indicates the capacity.
	/// </summary>
	public int Capacity;

	/// <summary>
	/// Indicates the number of nodes solved.
	/// </summary>
	public int Count;

	/// <summary>
	/// Indicates the allocated block.
	/// </summary>
	public TreeNode* Start;
}
