namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for heap-based priority queue.
/// </summary>
internal unsafe struct HeapBasedQueue
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


	public static void Enqueue(Queue* queue, TreeNode* node)
	{
		Debug.Assert(queue->HeapBased.Count < queue->HeapBased.Capacity);

		var i = queue->HeapBased.Count++;
		var pi = HeapQueueParentIndex(i);
		queue->HeapBased.Start[i] = node;

		while (i > 0 && TreeNode.Compare(in queue->HeapBased.Start[pi][0], in queue->HeapBased.Start[i][0]) > 0)
		{
			var temp = queue->HeapBased.Start[pi];
			queue->HeapBased.Start[pi] = queue->HeapBased.Start[i];
			queue->HeapBased.Start[i] = temp;
			i = pi;
			pi = HeapQueueParentIndex(i);
		}
	}

	public static void Destroy(Queue* queue) => NativeMemory.Free(queue->HeapBased.Start);

	public static bool IsEmpty(Queue* queue) => queue->HeapBased.Count == 0;

	public static TreeNode* Peek(Queue* queue)
	{
		Debug.Assert(!IsEmpty(queue));
		return queue->HeapBased.Start[0];
	}

	public static TreeNode* Dequeue(Queue* queue)
	{
		Debug.Assert(!IsEmpty(queue));

		var result = queue->HeapBased.Start[0];
		queue->HeapBased.Count--;
		if (queue->HeapBased.Count != 0)
		{
			queue->HeapBased.Start[0] = queue->HeapBased.Start[queue->HeapBased.Count];
			repair(queue, 0);
		}
		return result;


		static void repair(Queue* queue, int i)
		{
			var li = HeapQueueLeftChildIndex(i);
			var ri = li + 1;
			var smallest = i;
			if (li < queue->HeapBased.Count
				&& TreeNode.Compare(in queue->HeapBased.Start[i][0], in queue->HeapBased.Start[li][0]) > 0)
			{
				smallest = li;
			}
			if (ri < queue->HeapBased.Count
				&& TreeNode.Compare(in queue->HeapBased.Start[smallest][0], in queue->HeapBased.Start[ri][0]) > 0)
			{
				smallest = ri;
			}

			if (smallest != i)
			{
				var temp = queue->HeapBased.Start[i];
				queue->HeapBased.Start[i] = queue->HeapBased.Start[smallest];
				queue->HeapBased.Start[smallest] = temp;
				repair(queue, smallest);
			}
		}
	}

	/// <summary>
	/// Create a <see cref="HeapBasedQueue"/> instance.
	/// </summary>
	/// <param name="maxNodes">The maximum nodes.</param>
	/// <returns>A <see cref="HeapBasedQueue"/> instance.</returns>
	public static Queue Create(int maxNodes)
		=> new()
		{
			HeapBased =
			{
				Start = (TreeNode**)NativeMemory.Alloc((nuint)maxNodes, (nuint)sizeof(TreeNode*)),
				Count = 0,
				Capacity = maxNodes
			}
		};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int HeapQueueParentIndex(int i) => i - 1 >> 1;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int HeapQueueLeftChildIndex(int i) => (i << 1) + 1;
}
