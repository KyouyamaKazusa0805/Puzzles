namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a queue that supports analysis as node used by <see cref="TreeNode"/>.
/// </summary>
/// <typeparam name="TSelf"><include file="../../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
/// <seealso cref="TreeNode"/>
internal interface IAnalysisQueue<TSelf> : IDisposable where TSelf : struct, IAnalysisQueue<TSelf>, allows ref struct
{
	/// <summary>
	/// Determine whether the specified queue is empty.
	/// </summary>
	public abstract bool IsEmpty { get; }

	/// <summary>
	/// Indicates the number of elements in the collection.
	/// </summary>
	public abstract int Count { get; }

	/// <summary>
	/// Indicates the capacity of the collection.
	/// </summary>
	public abstract int Capacity { get; }

	/// <summary>
	/// Indicates the nodes stored. All elements are stored as pointers.
	/// </summary>
	public abstract unsafe TreeNode*[] Entry { get; }


	/// <summary>
	/// Try to enqueue a <see cref="TreeNode"/> into <typeparamref name="TSelf"/>.
	/// </summary>
	/// <param name="node">The node to be added.</param>
	public abstract void Enqueue(ref readonly TreeNode node);

	/// <summary>
	/// Try to peek the specified queue, and return the last element in the collection, without any operation to effect the collection.
	/// </summary>
	/// <returns>The peek node.</returns>
	public abstract ref TreeNode Peek();

	/// <summary>
	/// Try to dequeue the specified queue, and return the removed element.
	/// </summary>
	/// <returns>The removed node.</returns>
	public abstract ref TreeNode Dequeue();


	/// <summary>
	/// Try to create an instance of type <typeparamref name="TSelf"/> via the specified length of nodes to be allocated.
	/// </summary>
	/// <param name="maxNodes">The number of elements allocated.</param>
	/// <returns>An instance of type <typeparamref name="TSelf"/>.</returns>
	public static abstract TSelf Create(int maxNodes);
}
