#define USE_ARRAY_POOL
#undef USE_NATIVE_MEMORY
#if USE_ARRAY_POOL && USE_NATIVE_MEMORY
#warning If both symbols 'USE_ARRAY_POOL' and 'USE_NATIVE_MEMORY' are set, symbol 'USE_NATIVE_MEMORY' will be ignored.
#elif !USE_ARRAY_POOL && !USE_NATIVE_MEMORY
#error You must set at least one symbol 'USE_ARRAY_POOL' and 'USE_NATIVE_MEMORY'.
#endif

namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a memory manager for <see cref="TreeNode"/> instances allocation.
/// </summary>
/// <param name="capacity">Indicates the capacity.</param>
internal sealed unsafe partial class TreeNodeMemoryManager([Property] int capacity) : MemoryManager<TreeNode>
{
	/// <summary>
	/// Indicates the number of nodes solved.
	/// </summary>
	public int Count { get; private set; } = 0;

	/// <summary>
	/// Indicates the allocated block.
	/// </summary>
#if USE_ARRAY_POOL
	public TreeNode[] Entry { get; private set; } = ArrayPool<TreeNode>.Shared.Rent(capacity);
#elif USE_NATIVE_MEMORY
	public TreeNode* Entry { get; private set; } = (TreeNode*)NativeMemory.Alloc((nuint)capacity, (nuint)sizeof(TreeNode));
#else
	public TreeNode* Entry { get; private set; } = null;
#endif

	/// <inheritdoc/>
	public override Memory<TreeNode> Memory => Entry.AsMemory()[..Count];


	/// <inheritdoc/>
	public void Dispose()
#if USE_ARRAY_POOL
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
	/// <returns>The reference to the next node; or <see langword="ref null"/> if failed to create.</returns>
	public ref TreeNode Rent()
	{
		if (Count >= Capacity)
		{
			return ref Unsafe.NullRef<TreeNode>();
		}

		ref var result = ref Entry[Count];
		Count++;
		return ref result;
	}

	/// <summary>
	/// Create a node via the specified parent and state.
	/// </summary>
	/// <param name="parent">The parent node.</param>
	/// <param name="state">The state.</param>
	/// <returns>The created node, or return <see langword="ref null"/> if failed to allocate.</returns>
	public ref TreeNode CreateNode(ref readonly TreeNode parent, ref GridInterimState state)
	{
		ref var result = ref Rent();
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
	[DoesNotReturn]
	public override void Unpin() => throw new NotImplementedException();

	/// <inheritdoc/>
	public override Span<TreeNode> GetSpan() => Memory.Span[..Count];

	/// <inheritdoc/>
	[DoesNotReturn]
	public override MemoryHandle Pin(int elementIndex = 0) => throw new NotImplementedException();

	/// <inheritdoc/>
	protected override void Dispose(bool disposing) => Dispose();
}
