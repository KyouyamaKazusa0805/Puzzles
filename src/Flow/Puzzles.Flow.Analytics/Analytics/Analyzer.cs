namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents an analyzer.
/// </summary>
public sealed unsafe class Analyzer
{
	/// <summary>
	/// Indicates whether analyzer will check on touchness.
	/// </summary>
	public bool CheckTouchness { get; set; }

	/// <summary>
	/// Indicates whether analyzer will check on stranded cases.
	/// </summary>
	public bool CheckStranded { get; set; }

	/// <summary>
	/// Indicates whether analyzer will check on dead-end cases.
	/// </summary>
	public bool CheckDeadends { get; set; }

	/// <summary>
	/// Indicates whether analyzer will search for outside-in cases.
	/// </summary>
	public bool SearchOutsideIn { get; set; }

	/// <summary>
	/// Indicates whether analyzer will search fast forwardly.
	/// </summary>
	public bool SearchFastForward { get; set; }

	/// <summary>
	/// Indicates whether analyzer will explore penalized cases.
	/// </summary>
	public bool PenalizeExploration { get; set; }

	/// <summary>
	/// Indicates whether analyzer will automatically adjust colors in searching experience.
	/// </summary>
	public bool ReorderColors { get; set; }

	/// <summary>
	/// Indicates whether analyzer will automatically adjust colors via constrainted priority.
	/// </summary>
	public bool ReorderOnMostConstrained { get; set; }

	/// <summary>
	/// Indicates whether analyzer will force the first color as start.
	/// </summary>
	public bool ForcesFirstColor { get; set; }

	/// <summary>
	/// Indicates whether analyzer will randomize colors.
	/// </summary>
	public bool RandomOrdering { get; set; }

	/// <summary>
	/// Indicates whether analyzer will use best-first search (BFS) rule to check grid.
	/// </summary>
	public bool UsesBestFirstSearch { get; set; }

	/// <summary>
	/// Indicates the maximum memory usage in mega-bytes.
	/// </summary>
	public double MaxMemoryUsage { get; set; }

	/// <summary>
	/// Indicates the number of bottleneck limit.
	/// </summary>
	public int BottleneckLimit { get; set; }

	/// <summary>
	/// Indicates the number of maximum nodes can be reached.
	/// </summary>
	public int MaxNodes { get; set; }

	/// <summary>
	/// Indicates the queue creator method.
	/// </summary>
	[DisallowNull]
	internal delegate*<int, Queue> QueueCreator { get; set; }

	/// <summary>
	/// Indicates the queue enqueuer method.
	/// </summary>
	[DisallowNull]
	internal delegate*<ref Queue, ref TreeNode, void> QueueEnqueuer { get; set; }

	/// <summary>
	/// Indicates the queue dequeuer method.
	/// </summary>
	[DisallowNull]
	internal delegate*<ref Queue, ref TreeNode> QueueDequeuer { get; set; }

	/// <summary>
	/// Indicatees the queue destroyer method.
	/// </summary>
	[DisallowNull]
	internal delegate*<ref Queue, void> QueueDestroyer { get; set; }

	/// <summary>
	/// Indicates the queue clearer method.
	/// </summary>
	[DisallowNull]
	internal delegate*<ref readonly Queue, int> QueueClearer { get; set; }

	/// <summary>
	/// Indicates thee queue peeker method.
	/// </summary>
	[DisallowNull]
	internal delegate*<ref readonly Queue, ref readonly TreeNode> QueuePeeker { get; set; }
}
