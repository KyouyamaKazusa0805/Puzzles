namespace Puzzles.Flow.Concepts;

/// <summary>
/// Represents a type of progress in searching.
/// </summary>
public enum SearchingProgressType
{
	/// <summary>
	/// Indicates the progress type is successful (finished).
	/// </summary>
	Success = 0,

	/// <summary>
	/// Indicates the progress type is unreachable (failed).
	/// </summary>
	Unreachable = 1,

	/// <summary>
	/// Indicates the progress type is full.
	/// </summary>
	Full = 2,

	/// <summary>
	/// Indicates the progress type is in-progress.
	/// </summary>
	InProgress = 3
}
