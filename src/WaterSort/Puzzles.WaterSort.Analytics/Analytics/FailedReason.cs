namespace Puzzles.WaterSort.Analytics;

/// <summary>
/// Represents a failed reason.
/// </summary>
public enum FailedReason
{
	/// <summary>
	/// Indicates there's no failure.
	/// </summary>
	None,

	/// <summary>
	/// Indicates the puzzle is invalid.
	/// </summary>
	PuzzleInvalid,

	/// <summary>
	/// Indicates a user has cancelled the task.
	/// </summary>
	UserCancelled,

	/// <summary>
	/// Indicates an unhandled exception is thrown.
	/// </summary>
	UnhandledException
}
