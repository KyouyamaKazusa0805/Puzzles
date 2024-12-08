namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a node in searching on A* or BFS algorithm.
/// </summary>
[TypeImpl(
	TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.AllEqualityComparisonOperators,
	IsLargeStructure = true)]
internal unsafe partial struct TreeNode :
	IComparable<TreeNode>,
	IComparisonOperators<TreeNode, TreeNode, bool>,
	IEquatable<TreeNode>,
	IEqualityOperators<TreeNode, TreeNode, bool>
{
	/// <summary>
	/// Indicates the cost to come (this field will be ignored in BFS).
	/// </summary>
	[HashCodeMember]
	public double CostToCome;

	/// <summary>
	/// Indicates the cost to go (this field will be ignored in BFS).
	/// </summary>
	[HashCodeMember]
	public double CostToGo;

	/// <summary>
	/// Indicates the current progress state.
	/// </summary>
	public ProcessState State;

	/// <summary>
	/// Indicates the parent of this node (can also be <see langword="null"/>).
	/// </summary>
	public TreeNode* Parent;


	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public readonly bool Equals(ref readonly TreeNode other) => Compare(in this, in other) == 0;

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	public readonly int CompareTo(ref readonly TreeNode other) => Compare(in this, in other);

	/// <inheritdoc/>
	readonly bool IEquatable<TreeNode>.Equals(TreeNode other) => Equals(in other);

	/// <inheritdoc/>
	readonly int IComparable<TreeNode>.CompareTo(TreeNode other) => Compare(in this, in other);


	/// <summary>
	/// Compare two <see cref="TreeNode"/> values.
	/// </summary>
	/// <param name="a">The first node to be compared.</param>
	/// <param name="b">The second node to be compared.</param>
	/// <returns>An <see cref="int"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Compare(ref readonly TreeNode a, ref readonly TreeNode b)
	{
		var af = a.CostToCome + a.CostToGo;
		var bf = b.CostToCome + b.CostToGo;
		return af != bf ? Math.Sign(af - bf) : Unsafe.IsAddressLessThan(in a, in b) ? -1 : 1;
	}
}
