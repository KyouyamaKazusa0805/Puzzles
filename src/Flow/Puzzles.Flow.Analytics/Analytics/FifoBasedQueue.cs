namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for FIFO-based priority queue.
/// </summary>
internal struct FifoBasedQueue
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
	/// Indicates the next index to dequeue.
	/// </summary>
	public int Next;

	/// <summary>
	/// Indicates the array of nodes.
	/// </summary>
	public TreeNode[][]? Start;
}
