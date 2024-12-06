namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for heap-based priority queue.
/// </summary>
internal struct HeapBasedQueue
{
	/// <summary>
	/// Indicates the number enqueued.
	/// </summary>
	public int Count;

	/// <summary>
	/// Indicates the maximum allowable queue size.
	/// </summary>
	public int Capacity;

	/// <summary>
	/// Indicates the array of nodes.
	/// </summary>
	public TreeNode[][]? Start;
}
