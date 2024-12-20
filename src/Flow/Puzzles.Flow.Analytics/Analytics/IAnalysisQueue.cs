namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a queue that supports analysis as node used by <see cref="TreeNode"/>.
/// </summary>
/// <typeparam name="TSelf"><include file="../../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
/// <seealso cref="TreeNode"/>
internal unsafe interface IAnalysisQueue<TSelf> where TSelf : IAnalysisQueue<TSelf>, allows ref struct
{
	/// <summary>
	/// Indicates the number of elements in the collection.
	/// </summary>
	public int Count { get; }

	/// <summary>
	/// Indicates the capacity of the collection.
	/// </summary>
	public int Capacity { get; }

	/// <summary>
	/// Indicates the nodes of a 2D array, specified as pointer.
	/// </summary>
	public TreeNode** Start { get; }


	/// <summary>
	/// Try to enqueue a <see cref="TreeNode"/> into a <see cref="Queue"/>.
	/// </summary>
	/// <param name="queue">The queue to be used.</param>
	/// <param name="node">The node to be added.</param>
	public static abstract void Enqueue(ref Queue queue, ref readonly TreeNode node);

	/// <summary>
	/// Try to release memory of a <see cref="Queue"/>.
	/// </summary>
	/// <param name="queue">The queue to be released.</param>
	public static abstract void Destroy(ref readonly Queue queue);

	/// <summary>
	/// Determine whether the specified queue is empty.
	/// </summary>
	/// <param name="queue">The queue to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public static abstract bool IsEmpty(ref readonly Queue queue);

	/// <summary>
	/// Try to peek the specified queue, and return the last element in the collection, without any operation to effect the collection.
	/// </summary>
	/// <param name="queue">The queue to be checked.</param>
	/// <returns>The peek node.</returns>
	public static abstract ref TreeNode Peek(scoped ref readonly Queue queue);

	/// <summary>
	/// Try to dequeue the specified queue, and return the removed element.
	/// </summary>
	/// <param name="queue">The queue to be checked.</param>
	/// <returns>The removed node.</returns>
	public static abstract ref TreeNode Dequeue(scoped ref Queue queue);

	/// <summary>
	/// Try to create a <see cref="Queue"/> instance via the specified length of nodes to be allocated.
	/// </summary>
	/// <param name="maxNodes">The number of elements allocated.</param>
	/// <returns>A <see cref="Queue"/> instance.</returns>
	public static abstract Queue Create(int maxNodes);
}
