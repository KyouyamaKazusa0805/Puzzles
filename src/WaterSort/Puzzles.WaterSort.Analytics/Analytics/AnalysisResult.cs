namespace WaterSort.Analytics;

/// <summary>
/// Represents the result of analysis.
/// </summary>
/// <param name="puzzle">A puzzle.</param>
public sealed class AnalysisResult(Puzzle puzzle) :
	IEquatable<AnalysisResult>,
	IEqualityOperators<AnalysisResult, AnalysisResult, bool>
{
	/// <summary>
	/// Indicates whether the puzzle is fully solved.
	/// </summary>
	[MemberNotNullWhen(true, nameof(InterimSteps))]
	public required bool IsSolved { get; init; }

	/// <summary>
	/// Indicates the failed reason.
	/// </summary>
	public FailedReason FailedReason { get; init; }

	/// <summary>
	/// Indicates the steps found during the analysis.
	/// </summary>
	public ReadOnlySpan<Step> Steps => InterimSteps;

	/// <summary>
	/// Indicates the elapsed time.
	/// </summary>
	public TimeSpan ElapsedTime { get; init; }

	/// <summary>
	/// Indicates the base puzzle.
	/// </summary>
	public Puzzle Puzzle { get; } = puzzle;

	/// <summary>
	/// Indicates the exception encountered.
	/// </summary>
	public Exception? UnhandledException { get; init; }

	/// <summary>
	/// Indicates the steps.
	/// </summary>
	internal Step[]? InterimSteps { get; init; }


	/// <inheritdoc/>
	public override bool Equals(object? obj) => Equals(obj as AnalysisResult);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] AnalysisResult? other)
		=> other is not null && IsSolved == other.IsSolved && Puzzle == other.Puzzle;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Puzzle, IsSolved);

	/// <inheritdoc/>
	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.AppendLine("Puzzle:");
		sb.AppendLine(Puzzle.ToString());
		sb.AppendLine("---");

		if (IsSolved)
		{
			sb.AppendLine("Steps:");
			foreach (var step in InterimSteps)
			{
				sb.AppendLine($"{step} (color {Puzzle[step.StartTubeIndex].TopColor}, size {Puzzle[step.StartTubeIndex].TopColorSpannedCount})");
			}
			sb.AppendLine("---");
			sb.AppendLine("Puzzle is solved.");
			sb.AppendLine($@"Elapsed time: {ElapsedTime:hh\:mm\:ss\.fff}");
		}
		else
		{
			sb.AppendLine($"Puzzle isn't solved. Reason code: '{FailedReason}'.");
			if (UnhandledException is not null)
			{
				sb.AppendLine($"Unhandled exception: {UnhandledException}");
			}
		}
		return sb.ToString();
	}


	/// <inheritdoc/>
	public static bool operator ==(AnalysisResult? left, AnalysisResult? right)
		=> (left, right) switch { (not null, not null) => left.Equals(right), (null, null) => true, _ => false };

	/// <inheritdoc/>
	public static bool operator !=(AnalysisResult? left, AnalysisResult? right) => !(left == right);
}
