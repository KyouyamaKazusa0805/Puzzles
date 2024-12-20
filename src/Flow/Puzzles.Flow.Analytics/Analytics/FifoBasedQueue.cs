namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for FIFO-based priority queue.
/// </summary>
internal unsafe struct FifoBasedQueue : IAnalysisQueue<FifoBasedQueue>
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
	public TreeNode** Start;

	/// <summary>
	/// Indicates the next index to dequeue.
	/// </summary>
	private int _next;


	/// <inheritdoc/>
	public readonly bool IsEmpty => _next == Count;

	/// <inheritdoc/>
	readonly int IAnalysisQueue<FifoBasedQueue>.Count => Count;

	/// <inheritdoc/>
	readonly int IAnalysisQueue<FifoBasedQueue>.Capacity => Capacity;

	/// <inheritdoc/>
	readonly TreeNode** IAnalysisQueue<FifoBasedQueue>.Start => Start;


	/// <inheritdoc/>
	public readonly void Dispose() => NativeMemory.Free(Start);

	/// <inheritdoc/>
	public readonly ref TreeNode Peek()
	{
		Debug.Assert(!IsEmpty);
		return ref *Start[_next];
	}

	/// <inheritdoc/>
	public void Enqueue(ref readonly TreeNode node)
	{
		Debug.Assert(Count < Capacity);
		Start[Count++] = (TreeNode*)Unsafe.AsPointer(ref Unsafe.AsRef(in node));
	}

	/// <inheritdoc/>
	public ref TreeNode Dequeue()
	{
		Debug.Assert(!IsEmpty);
		return ref *Start[_next++];
	}


	/// <inheritdoc/>
	public static FifoBasedQueue Create(int maxNodes)
		=> new()
		{
			Start = (TreeNode**)NativeMemory.Alloc((nuint)maxNodes, (nuint)sizeof(TreeNode*)),
			Count = 0,
			Capacity = maxNodes,
			_next = 0
		};
}
