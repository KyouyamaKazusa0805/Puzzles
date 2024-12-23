namespace Puzzles.Onet.Analytics;

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
	/// Indicates the puzzle is invalid or failed.
	/// </summary>
	PuzzleInvalid,

	/// <summary>
	/// Indicates the user has cancelled the analysis.
	/// </summary>
	UserCancelled,

	/// <summary>
	/// Indicates an unhandled exception is thrown.
	/// </summary>
	UnhandledException
}
