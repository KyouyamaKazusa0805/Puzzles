namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for FIFO-based priority queue.
/// </summary>
internal unsafe struct FifoBasedQueue
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
	public TreeNode** Start;


	public static void Enqueue(Queue* queue, TreeNode* n)
	{
		Debug.Assert(queue->FifoBased.Count < queue->FifoBased.Capacity);

		queue->FifoBased.Start[queue->FifoBased.Count++] = n;
	}

	public static void Destroy(Queue* queue) => NativeMemory.Free(queue->FifoBased.Start);

	public static bool IsEmpty(Queue* queue) => queue->FifoBased.Next == queue->FifoBased.Count;

	public static TreeNode* Peek(Queue* queue)
	{
		Debug.Assert(!IsEmpty(queue));

		return queue->FifoBased.Start[queue->FifoBased.Next];
	}

	public static TreeNode* Dequeue(Queue* queue)
	{
		Debug.Assert(!IsEmpty(queue));

		return queue->FifoBased.Start[queue->FifoBased.Next++];
	}

	/// <summary>
	/// Create a <see cref="FifoBasedQueue"/> instance.
	/// </summary>
	/// <param name="maxNodes">The maximum nodes.</param>
	/// <returns>The <see cref="FifoBasedQueue"/> created.</returns>
	public static Queue Create(int maxNodes)
		=> new()
		{
			FifoBased =
			{
				Start = (TreeNode**)NativeMemory.Alloc((nuint)maxNodes, (nuint)sizeof(TreeNode*)),
				Count = 0,
				Capacity = maxNodes,
				Next = 0
			}
		};
}
