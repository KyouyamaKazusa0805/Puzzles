namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents data structure for node storage.
/// </summary>
internal unsafe struct NodeStorage
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
	public void Unalloc(TreeNode* n)
	{
		Debug.Assert(Count != 0 && n == Start + Count - 1);
		Count--;
	}

	/// <summary>
	/// Try allocate the next node.
	/// </summary>
	/// <returns>The next node pointer.</returns>
	public TreeNode* Alloc()
	{
		if (Count >= Capacity)
		{
			return null;
		}

		var result = Start + Count;
		Count++;
		return result;
	}

	/// <summary>
	/// Create a node via the specified parent and state.
	/// </summary>
	/// <param name="parent">The parent node.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <returns>The created node.</returns>
	public TreeNode* CreateNode(TreeNode* parent, ref readonly Grid grid, ref readonly ProcessState state)
	{
		var result = Alloc();
		if (result == null)
		{
			return null;
		}

		result->Parent = parent;
		result->CostToCome = 0;
		result->CostToGo = 0;
		result->State = state;
		return result;
	}


	/// <summary>
	/// Create single linear allocator for searching nodes.
	/// </summary>
	/// <param name="maxNodes">The maximum nodes.</param>
	/// <returns>The <see cref="NodeStorage"/> value.</returns>
	public static NodeStorage Create(int maxNodes)
		=> new()
		{
			Start = (TreeNode*)NativeMemory.Alloc((nuint)(maxNodes * sizeof(TreeNode))),
			Capacity = maxNodes,
			Count = 0
		};
}
