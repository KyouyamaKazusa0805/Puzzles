namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for FIFO-based (first in first out) priority queue.
/// </summary>
/// <param name="capacity">The number of elements allocated.</param>
[StructLayout(LayoutKind.Auto)]
internal unsafe partial struct FifoBasedQueue(
#if DYNAMIC_ALLOCATION
	[Property(Setter = PropertySetters.PrivateSet)] int capacity
#else
	[Property] int capacity
#endif
) : IAnalysisQueue<FifoBasedQueue>
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

	/// <summary>
	/// Indicates the next index to dequeue.
	/// </summary>
	private int _next = 0;


	/// <inheritdoc/>
	public readonly bool IsEmpty => _next == Count;

	/// <inheritdoc/>
	readonly int IAnalysisQueue<FifoBasedQueue>.Count => Count;

	/// <inheritdoc/>
#if USE_NEW_ARRAY || USE_ARRAY_POOL
	readonly TreeNode*[] IAnalysisQueue<FifoBasedQueue>.Entry => Entry;
#else
	readonly TreeNode** IAnalysisQueue<FifoBasedQueue>.Entry => Entry;
#endif


	/// <inheritdoc/>
	public void Grow()
	{
#if DYNAMIC_ALLOCATION
		Capacity <<= 1;
#if USE_NEW_ARRAY
		var entry = Unsafe.As<nint[]>(Entry);
		Array.Resize(ref entry, Capacity);
		Entry = Unsafe.As<TreeNode*[]>(entry);
#elif USE_ARRAY_POOL
		var tempArray = Entry[..];
		ArrayPool<nint>.Shared.Return(Unsafe.As<nint[]>(Entry));
		Entry = Unsafe.As<TreeNode*[]>(ArrayPool<nint>.Shared.Rent(Capacity));
		tempArray.CopyTo(Entry, 0);
#elif USE_NATIVE_MEMORY
		Entry = (TreeNode**)NativeMemory.Realloc(Entry, (nuint)Capacity * (nuint)sizeof(TreeNode*));
#endif
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
		return ref *Entry[_next];
	}

	/// <inheritdoc/>
	public void Enqueue(ref readonly TreeNode node)
	{
		Debug.Assert(Count < Capacity);
		Entry[Count++] = (TreeNode*)Unsafe.AsPointer(ref Unsafe.AsRef(in node));
	}

	/// <inheritdoc/>
	public ref TreeNode Dequeue()
	{
		Debug.Assert(!IsEmpty);
		return ref *Entry[_next++];
	}


	/// <inheritdoc/>
	static FifoBasedQueue IAnalysisQueue<FifoBasedQueue>.Create(int maxNodes) => new(maxNodes);
}
