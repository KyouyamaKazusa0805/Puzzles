namespace Puzzles.Flow.Buffers;

/// <summary>
/// Represents a memory manager for <see cref="TreeNode"/> instances allocation.
/// </summary>
/// <param name="capacity">Indicates the capacity.</param>
internal sealed unsafe partial class TreeNodeMemoryManager(
#if DYNAMIC_ALLOCATION
	[Property(Setter = PropertySetters.PrivateSet)] int capacity
#else
	[Property] int capacity
#endif
) : MemoryManager<TreeNode>
{
	/// <summary>
	/// Indicates the number of nodes solved.
	/// </summary>
	public int Count { get; private set; } = 0;

	/// <summary>
	/// Indicates the allocated block.
	/// </summary>
#if USE_NEW_ARRAY
	public TreeNode[] Entry { get; private set; } = new TreeNode[capacity];
#elif USE_ARRAY_POOL
	public TreeNode[] Entry { get; private set; } = ArrayPool<TreeNode>.Shared.Rent(capacity);
#elif USE_NATIVE_MEMORY
	public TreeNode* Entry { get; private set; } = (TreeNode*)NativeMemory.Alloc((nuint)capacity, (nuint)sizeof(TreeNode));
#else
	public TreeNode* Entry { get; private set; } = null;
#endif

	/// <inheritdoc/>
	public override Memory<TreeNode> Memory
#if USE_NEW_ARRAY || USE_ARRAY_POOL
		=> Entry.AsMemory()[..Count];
#else
	{
		[DoesNotReturn]
		get => throw new NotSupportedException($"Cannot apply memory if member '{nameof(Entry)}' is a pointer type.");
	}
#endif


	/// <inheritdoc/>
	public void Dispose()
#if USE_NEW_ARRAY
	{
		// Managed array may not be required to be released.
	}
#elif USE_ARRAY_POOL
		=> ArrayPool<TreeNode>.Shared.Return(Entry);
#elif USE_NATIVE_MEMORY
		=> NativeMemory.Free(Entry);
#else
		=> throw new NotImplementedException();
#endif

	/// <summary>
	/// Return the tree node to memory, to free the memory up.
	/// </summary>
	/// <param name="n">The node.</param>
	public void Return(ref readonly TreeNode n)
	{
		Debug.Assert(Count != 0 && Unsafe.AreSame(in n, in Entry[Count - 1]));
		Count--;
	}

	/// <summary>
	/// Try to create a new <see cref="TreeNode"/> from memory.
	/// </summary>
	/// <typeparam name="TQueue">The type of the queue.</typeparam>
	/// <param name="queue">The queue.</param>
	/// <returns>The reference to the next node; or <see langword="ref null"/> if failed to create.</returns>
	public ref TreeNode Rent<TQueue>(scoped ref TQueue queue) where TQueue : struct, IAnalysisQueue<TQueue>, allows ref struct
	{
		if (Count >= Capacity)
		{
#if DYNAMIC_ALLOCATION
			Capacity <<= 1;
#if USE_NEW_ARRAY
			var entry = Entry;
			Array.Resize(ref entry, Capacity);
			Entry = entry;
#else
			return ref Unsafe.NullRef<TreeNode>();
#endif

			// Also execute dynamic allocation on queue.
			queue.Grow();
#else
			return ref Unsafe.NullRef<TreeNode>();
#endif
		}

		return ref Entry[Count++];
	}

	/// <summary>
	/// Create a node via the specified parent and state.
	/// </summary>
	/// <typeparam name="TQueue">The type of the queue.</typeparam>
	/// <param name="parent">The parent node.</param>
	/// <param name="state">The state.</param>
	/// <param name="queue">The queue.</param>
	/// <returns>The created node, or return <see langword="ref null"/> if failed to allocate.</returns>
	public ref TreeNode CreateNode<TQueue>(ref readonly TreeNode parent, ref GridInterimState state, scoped ref TQueue queue)
		where TQueue : struct, IAnalysisQueue<TQueue>, allows ref struct
	{
		ref var result = ref Rent(ref queue);
		if (Unsafe.IsNullRef(in result))
		{
			return ref Unsafe.NullRef<TreeNode>();
		}

		result.Parent = (TreeNode*)Unsafe.AsPointer(ref Unsafe.AsRef(in parent));
		result.CostToCome = 0;
		result.CostToGo = 0;
		result.State = new()
		{
			LastColor = state.LastColor,
			FreeCellsCount = state.FreeCellsCount,
			CompletedMask = state.CompletedMask
		};

		var pResult = (TreeNode*)Unsafe.AsPointer(ref result);
		var pState = (GridInterimState*)Unsafe.AsPointer(ref state);
		Unsafe.CopyBlock(pResult->State.Cells, pState->Cells, sizeof(byte) * Analyzer.MaxGridCellsCount);
		Unsafe.CopyBlock(pResult->State.Positions, pState->Positions, sizeof(byte) * Analyzer.MaxSupportedColorsCount);
		return ref result;
	}

	/// <inheritdoc/>
	public override void Unpin()
	{
	}

	/// <inheritdoc/>
	public override Span<TreeNode> GetSpan() => Memory.Span[..Count];

	/// <inheritdoc/>
	public override MemoryHandle Pin(int elementIndex = 0) => default;

	/// <inheritdoc/>
	protected override void Dispose(bool disposing) => Dispose();
}
