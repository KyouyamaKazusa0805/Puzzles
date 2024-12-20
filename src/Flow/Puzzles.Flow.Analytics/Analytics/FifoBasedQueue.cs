namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for FIFO-based priority queue.
/// </summary>
/// <param name="capacity">The number of elements allocated.</param>
[StructLayout(LayoutKind.Auto)]
internal unsafe partial struct FifoBasedQueue([Field(Accessibility = "public", NamingRule = NamingRules.Property)] int capacity) :
	IAnalysisQueue<FifoBasedQueue>
{
	/// <inheritdoc cref="IAnalysisQueue{TSelf}.Count"/>
	public int Count = 0;

	/// <inheritdoc cref="IAnalysisQueue{TSelf}.Entry"/>
#if USE_ARRAY_POOL
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
	readonly int IAnalysisQueue<FifoBasedQueue>.Capacity => Capacity;

	/// <inheritdoc/>
	readonly TreeNode*[] IAnalysisQueue<FifoBasedQueue>.Entry => Entry;


	/// <inheritdoc/>
	public readonly void Dispose()
#if USE_ARRAY_POOL
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
