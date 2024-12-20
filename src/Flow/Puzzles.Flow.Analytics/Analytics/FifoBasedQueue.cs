namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates the data structure for FIFO-based priority queue.
/// </summary>
/// <param name="capacity">The number of elements allocated.</param>
[StructLayout(LayoutKind.Auto)]
internal unsafe partial struct FifoBasedQueue([Field(Accessibility = "public", NamingRule = NamingRules.Property)] int capacity) :
	IAnalysisQueue<FifoBasedQueue>
{
	/// <summary>
	/// Indicates the number enqueued.
	/// </summary>
	public int Count = 0;

	/// <summary>
	/// Indicates the array of nodes.
	/// </summary>
	public TreeNode*[] Start = Unsafe.As<TreeNode*[]>(ArrayPool<nint>.Shared.Rent(capacity));

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
	readonly TreeNode*[] IAnalysisQueue<FifoBasedQueue>.Start => Start;


	/// <inheritdoc/>
	public readonly void Dispose() => ArrayPool<nint>.Shared.Return(Unsafe.As<nint[]>(Start));

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
	static FifoBasedQueue IAnalysisQueue<FifoBasedQueue>.Create(int maxNodes) => new(maxNodes);
}
