namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for heap-based priority queue.
/// </summary>
internal unsafe struct HeapBasedQueue : IAnalysisQueue<HeapBasedQueue>
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


	/// <inheritdoc/>
	readonly int IAnalysisQueue<HeapBasedQueue>.Count => Count;

	/// <inheritdoc/>
	readonly int IAnalysisQueue<HeapBasedQueue>.Capacity => Capacity;

	/// <inheritdoc/>
	readonly TreeNode** IAnalysisQueue<HeapBasedQueue>.Start => Start;


	/// <inheritdoc/>
	public static void Enqueue(Queue* queue, TreeNode* node)
	{
		Debug.Assert(queue->HeapBased.Count < queue->HeapBased.Capacity);

		var i = queue->HeapBased.Count++;
		var pi = parentIndex(i);
		queue->HeapBased.Start[i] = node;

		while (i > 0 && TreeNode.Compare(in queue->HeapBased.Start[pi][0], in queue->HeapBased.Start[i][0]) > 0)
		{
			var temp = queue->HeapBased.Start[pi];
			queue->HeapBased.Start[pi] = queue->HeapBased.Start[i];
			queue->HeapBased.Start[i] = temp;
			i = pi;
			pi = parentIndex(i);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int parentIndex(int i) => i - 1 >> 1;
	}

	/// <inheritdoc/>
	public static void Destroy(Queue* queue) => NativeMemory.Free(queue->HeapBased.Start);

	/// <inheritdoc/>
	public static bool IsEmpty(Queue* queue) => queue->HeapBased.Count == 0;

	/// <inheritdoc/>
	public static TreeNode* Peek(Queue* queue)
	{
		Debug.Assert(!IsEmpty(queue));
		return queue->HeapBased.Start[0];
	}

	/// <inheritdoc/>
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
			var li = leftChildIndex(i);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int leftChildIndex(int i) => (i << 1) + 1;
	}

	/// <inheritdoc/>
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
}
