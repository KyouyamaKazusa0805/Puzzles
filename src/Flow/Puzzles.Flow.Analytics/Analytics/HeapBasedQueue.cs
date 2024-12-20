namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for heap-based priority queue.
/// </summary>
/// <param name="maxNodes">The number of elements allocated.</param>
internal unsafe struct HeapBasedQueue(int maxNodes) : IAnalysisQueue<HeapBasedQueue>
{
	/// <summary>
	/// Indicates the number enqueued.
	/// </summary>
	public int Count = 0;

	/// <summary>
	/// Indicates the maximum allowable queue size.
	/// </summary>
	public int Capacity = maxNodes;

	/// <summary>
	/// Indicates the array of nodes.
	/// </summary>
	public TreeNode** Start = (TreeNode**)NativeMemory.Alloc((nuint)maxNodes, (nuint)sizeof(TreeNode*));


	/// <inheritdoc/>
	public readonly bool IsEmpty => Count == 0;

	/// <inheritdoc/>
	readonly int IAnalysisQueue<HeapBasedQueue>.Count => Count;

	/// <inheritdoc/>
	readonly int IAnalysisQueue<HeapBasedQueue>.Capacity => Capacity;

	/// <inheritdoc/>
	readonly TreeNode** IAnalysisQueue<HeapBasedQueue>.Start => Start;


	/// <inheritdoc/>
	public readonly void Dispose() => NativeMemory.Free(Start);

	/// <inheritdoc/>
	public readonly ref TreeNode Peek()
	{
		Debug.Assert(!IsEmpty);
		return ref *Start[0];
	}

	/// <inheritdoc/>
	public void Enqueue(ref readonly TreeNode node)
	{
		Debug.Assert(Count < Capacity);

		var i = Count++;
		var pi = parentIndex(i);
		Start[i] = (TreeNode*)Unsafe.AsPointer(ref Unsafe.AsRef(in node));

		while (i > 0 && TreeNode.Compare(in Start[pi][0], in Start[i][0]) > 0)
		{
			var temp = Start[pi];
			Start[pi] = Start[i];
			Start[i] = temp;
			i = pi;
			pi = parentIndex(i);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int parentIndex(int i) => i - 1 >> 1;
	}

	/// <inheritdoc/>
	public ref TreeNode Dequeue()
	{
		Debug.Assert(!IsEmpty);

		ref var result = ref *Start[0];
		Count--;
		if (Count != 0)
		{
			Start[0] = Start[Count];
			repair(ref this, 0);
		}
		return ref result;


		static void repair(ref HeapBasedQueue queue, int i)
		{
			var li = leftChildIndex(i);
			var ri = li + 1;
			var smallest = i;
			if (li < queue.Count && TreeNode.Compare(in queue.Start[i][0], in queue.Start[li][0]) > 0)
			{
				smallest = li;
			}
			if (ri < queue.Count && TreeNode.Compare(in queue.Start[smallest][0], in queue.Start[ri][0]) > 0)
			{
				smallest = ri;
			}

			if (smallest != i)
			{
				var temp = queue.Start[i];
				queue.Start[i] = queue.Start[smallest];
				queue.Start[smallest] = temp;
				repair(ref queue, smallest);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int leftChildIndex(int i) => (i << 1) + 1;
	}


	/// <inheritdoc/>
	static HeapBasedQueue IAnalysisQueue<HeapBasedQueue>.Create(int maxNodes) => new(maxNodes);
}
