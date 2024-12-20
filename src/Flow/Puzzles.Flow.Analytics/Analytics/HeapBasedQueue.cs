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
	public static void Enqueue(ref Queue queue, ref readonly TreeNode node)
	{
		Debug.Assert(queue.HeapBased.Count < queue.HeapBased.Capacity);

		var i = queue.HeapBased.Count++;
		var pi = parentIndex(i);
		queue.HeapBased.Start[i] = (TreeNode*)Unsafe.AsPointer(ref Unsafe.AsRef(in node));

		while (i > 0 && TreeNode.Compare(in queue.HeapBased.Start[pi][0], in queue.HeapBased.Start[i][0]) > 0)
		{
			var temp = queue.HeapBased.Start[pi];
			queue.HeapBased.Start[pi] = queue.HeapBased.Start[i];
			queue.HeapBased.Start[i] = temp;
			i = pi;
			pi = parentIndex(i);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int parentIndex(int i) => i - 1 >> 1;
	}

	/// <inheritdoc/>
	public static void Destroy(ref readonly Queue queue) => NativeMemory.Free(queue.HeapBased.Start);

	/// <inheritdoc/>
	public static bool IsEmpty(ref readonly Queue queue) => queue.HeapBased.Count == 0;

	/// <inheritdoc/>
	public static ref TreeNode Peek(scoped ref readonly Queue queue)
	{
		Debug.Assert(!IsEmpty(in queue));
		return ref *queue.HeapBased.Start[0];
	}

	/// <inheritdoc/>
	public static ref TreeNode Dequeue(scoped ref Queue queue)
	{
		Debug.Assert(!IsEmpty(in queue));

		ref var result = ref *queue.HeapBased.Start[0];
		queue.HeapBased.Count--;
		if (queue.HeapBased.Count != 0)
		{
			queue.HeapBased.Start[0] = queue.HeapBased.Start[queue.HeapBased.Count];
			repair(ref queue, 0);
		}
		return ref result;


		static void repair(ref Queue queue, int i)
		{
			var li = leftChildIndex(i);
			var ri = li + 1;
			var smallest = i;
			if (li < queue.HeapBased.Count
				&& TreeNode.Compare(in queue.HeapBased.Start[i][0], in queue.HeapBased.Start[li][0]) > 0)
			{
				smallest = li;
			}
			if (ri < queue.HeapBased.Count
				&& TreeNode.Compare(in queue.HeapBased.Start[smallest][0], in queue.HeapBased.Start[ri][0]) > 0)
			{
				smallest = ri;
			}

			if (smallest != i)
			{
				var temp = queue.HeapBased.Start[i];
				queue.HeapBased.Start[i] = queue.HeapBased.Start[smallest];
				queue.HeapBased.Start[smallest] = temp;
				repair(ref queue, smallest);
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
