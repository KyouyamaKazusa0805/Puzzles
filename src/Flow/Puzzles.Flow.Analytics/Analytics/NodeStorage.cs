namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents data structure for node storage.
/// </summary>
internal unsafe ref struct NodeStorage
{
	/// <summary>
	/// Indicates the capacity.
	/// </summary>
	public int Capacity;

	/// <summary>
	/// Indicates the number of nodes solved.
	/// </summary>
	public int Count;

	/// <summary>
	/// Indicates the allocated block.
	/// </summary>
	public TreeNode* Start;


	/// <summary>
	/// Destroy the memory allocated.
	/// </summary>
	[SuppressMessage("Style", "IDE0251:Make member 'readonly'", Justification = "<Pending>")]
	public void Destroy() => NativeMemory.Free(Start);

	/// <summary>
	/// Unallocate the tree node.
	/// </summary>
	/// <param name="n">The node.</param>
	public void Unalloc(scoped ref readonly TreeNode n)
	{
		Debug.Assert(Count != 0 && Unsafe.AreSame(in n, in Start[Count - 1]));
		Count--;
	}

	/// <summary>
	/// Try allocate the next node.
	/// </summary>
	/// <returns>The next node pointer.</returns>
	public ref TreeNode Alloc()
	{
		if (Count >= Capacity)
		{
			return ref Unsafe.NullRef<TreeNode>();
		}

		ref var result = ref Start[Count];
		Count++;
		return ref result;
	}

	/// <summary>
	/// Create a node via the specified parent and state.
	/// </summary>
	/// <param name="parent">The parent node.</param>
	/// <param name="state">The state.</param>
	/// <returns>The created node.</returns>
	public ref TreeNode CreateNode(ref readonly TreeNode parent, ref GridInterimState state)
	{
		ref var result = ref Alloc();
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


	/// <summary>
	/// Create single linear allocator for searching nodes.
	/// </summary>
	/// <param name="maxNodes">The maximum nodes.</param>
	/// <returns>The <see cref="NodeStorage"/> value.</returns>
	public static NodeStorage Create(int maxNodes)
		=> new()
		{
			Start = (TreeNode*)NativeMemory.Alloc((nuint)maxNodes, (nuint)sizeof(TreeNode)),
			Capacity = maxNodes,
			Count = 0
		};
}
