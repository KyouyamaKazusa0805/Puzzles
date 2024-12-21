namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for heap-based priority queue.
/// </summary>
/// <param name="capacity">The number of elements allocated.</param>
[StructLayout(LayoutKind.Auto)]
internal unsafe partial struct HeapBasedQueue(
#if DYNAMIC_ALLOCATION
	[Property(Setter = PropertySetters.PrivateSet)] int capacity
#else
	[Property] int capacity
#endif
) : IAnalysisQueue<HeapBasedQueue>
{
	/// <inheritdoc cref="IAnalysisQueue{TSelf}.Count"/>
	public int Count = 0;

	/// <inheritdoc cref="IAnalysisQueue{TSelf}.Entry"/>
#if USE_NEW_ARRAY
	public TreeNode*[] Entry = new TreeNode*[capacity];
#elif USE_ARRAY_POOL
	public TreeNode*[] Entry = Unsafe.As<TreeNode*[]>(ArrayPool<nint>.Shared.Rent(capacity));
#elif USE_NATIVE_MEMORY
	public TreeNode** Entry = (TreeNode**)NativeMemory.Alloc((nuint)capacity, (nuint)sizeof(TreeNode*));
#else
	public TreeNode** Entry = null;
#endif


	/// <inheritdoc/>
	public readonly bool IsEmpty => Count == 0;

	/// <inheritdoc/>
	readonly int IAnalysisQueue<HeapBasedQueue>.Count => Count;

	/// <inheritdoc/>
	readonly int IAnalysisQueue<HeapBasedQueue>.Capacity => Capacity;

	/// <inheritdoc/>
#if USE_NEW_ARRAY || USE_ARRAY_POOL
	readonly TreeNode*[] IAnalysisQueue<HeapBasedQueue>.Entry => Entry;
#else
	readonly TreeNode** IAnalysisQueue<HeapBasedQueue>.Entry => Entry;
#endif


	/// <inheritdoc/>
	public void Grow()
	{
#if DYNAMIC_ALLOCATION && USE_NEW_ARRAY
		Capacity <<= 1;
		var entry = Unsafe.As<nint[]>(Entry);
		Array.Resize(ref entry, Capacity);
		Entry = Unsafe.As<TreeNode*[]>(entry);
#endif
	}

	/// <inheritdoc/>
	public readonly void Dispose()
#if USE_NEW_ARRAY
	{
		// Managed array may not be required to be released.
	}
#elif USE_ARRAY_POOL
		=> ArrayPool<nint>.Shared.Return(Unsafe.As<nint[]>(Entry));
#elif USE_NATIVE_MEMORY
		=> NativeMemory.Free(Entry);
#else
		=> throw new NotImplementedException();
#endif

	/// <inheritdoc/>
	public readonly ref TreeNode Peek()
	{
		Debug.Assert(!IsEmpty);
		return ref *Entry[0];
	}

	/// <inheritdoc/>
	public void Enqueue(ref readonly TreeNode node)
	{
		Debug.Assert(Count < Capacity);

		var i = Count++;
		var pi = parentIndex(i);
		Entry[i] = (TreeNode*)Unsafe.AsPointer(ref Unsafe.AsRef(in node));

		while (i > 0 && TreeNode.Compare(in Entry[pi][0], in Entry[i][0]) > 0)
		{
			var temp = Entry[pi];
			Entry[pi] = Entry[i];
			Entry[i] = temp;
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

		ref var result = ref *Entry[0];
		Count--;
		if (Count != 0)
		{
			Entry[0] = Entry[Count];
			repair(ref this, 0);
		}
		return ref result;


		static void repair(ref HeapBasedQueue queue, int i)
		{
			var li = leftChildIndex(i);
			var ri = li + 1;
			var smallest = i;
			if (li < queue.Count && TreeNode.Compare(in queue.Entry[i][0], in queue.Entry[li][0]) > 0)
			{
				smallest = li;
			}
			if (ri < queue.Count && TreeNode.Compare(in queue.Entry[smallest][0], in queue.Entry[ri][0]) > 0)
			{
				smallest = ri;
			}

			if (smallest != i)
			{
				var temp = queue.Entry[i];
				queue.Entry[i] = queue.Entry[smallest];
				queue.Entry[smallest] = temp;
				repair(ref queue, smallest);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int leftChildIndex(int i) => (i << 1) + 1;
	}


	/// <inheritdoc/>
	static HeapBasedQueue IAnalysisQueue<HeapBasedQueue>.Create(int maxNodes) => new(maxNodes);
}
