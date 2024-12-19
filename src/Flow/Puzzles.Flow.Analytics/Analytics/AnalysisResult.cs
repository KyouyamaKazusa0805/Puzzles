namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents the analysis result.
/// </summary>
/// <param name="grid">The grid to be used.</param>
public sealed partial class AnalysisResult([Property] Grid grid)
{
	/// <summary>
	/// Indicates whether the puzzle has been solved.
	/// </summary>
	public required bool IsSolved { get; init; }
}
