namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents the failed reason.
/// </summary>
public enum FailedReason
{
	/// <summary>
	/// Indicates there's no failure.
	/// </summary>
	None,

	/// <summary>
	/// Indicates the failed reason is that the puzzle has no valid solutions.
	/// </summary>
	Invalid,

	/// <summary>
	/// Indicates the failed reason is that the puzzle has run out of memory to be allocated.
	/// </summary>
	OutOfMemory,

	/// <summary>
	/// Indicates the failed reason is that an unexpected exception is thrown.
	/// </summary>
	ExceptionThrown
}
