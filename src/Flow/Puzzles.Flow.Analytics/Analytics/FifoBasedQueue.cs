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
	readonly int IAnalysisQueue<FifoBasedQueue>.Count => Count;

	/// <inheritdoc/>
	readonly int IAnalysisQueue<FifoBasedQueue>.Capacity => Capacity;

	/// <inheritdoc/>
	readonly TreeNode** IAnalysisQueue<FifoBasedQueue>.Start => Start;


	/// <inheritdoc/>
	public static void Enqueue(ref Queue queue, ref readonly TreeNode node)
	{
		Debug.Assert(queue.FifoBased.Count < queue.FifoBased.Capacity);
		queue.FifoBased.Start[queue.FifoBased.Count++] = (TreeNode*)Unsafe.AsPointer(ref Unsafe.AsRef(in node));
	}

	/// <inheritdoc/>
	public static void Destroy(ref readonly Queue queue) => NativeMemory.Free(queue.FifoBased.Start);

	/// <inheritdoc/>
	public static bool IsEmpty(ref readonly Queue queue) => queue.FifoBased._next == queue.FifoBased.Count;

	/// <inheritdoc/>
	public static ref TreeNode Peek(scoped ref readonly Queue queue)
	{
		Debug.Assert(!IsEmpty(in queue));
		return ref *queue.FifoBased.Start[queue.FifoBased._next];
	}

	/// <inheritdoc/>
	public static ref TreeNode Dequeue(scoped ref Queue queue)
	{
		Debug.Assert(!IsEmpty(in queue));
		return ref *queue.FifoBased.Start[queue.FifoBased._next++];
	}

	/// <inheritdoc/>
	public static Queue Create(int maxNodes)
		=> new()
		{
			FifoBased =
			{
				Start = (TreeNode**)NativeMemory.Alloc((nuint)maxNodes, (nuint)sizeof(TreeNode*)),
				Count = 0,
				Capacity = maxNodes,
				_next = 0
			}
		};
}
