namespace Puzzles.WaterSort.Analytics;

/// <summary>
/// Represents the result of analysis.
/// </summary>
/// <param name="puzzle">Indicates the base puzzle.</param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class AnalysisResult([Property, HashCodeMember] Puzzle puzzle) :
	IEquatable<AnalysisResult>,
	IEqualityOperators<AnalysisResult, AnalysisResult, bool>
{
	/// <summary>
	/// Indicates whether the puzzle is fully solved.
	/// </summary>
	[MemberNotNullWhen(true, nameof(InterimSteps), nameof(InterimPuzzleStates))]
	[HashCodeMember]
	[EquatableMember]
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
	/// Indicates the interim puzzle states during the analysis.
	/// </summary>
	public ReadOnlySpan<Puzzle> PuzzleStates => InterimPuzzleStates;

	/// <summary>
	/// Indicates the elapsed time.
	/// </summary>
	public TimeSpan ElapsedTime { get; init; }

	/// <summary>
	/// Indicates the exception encountered.
	/// </summary>
	public Exception? UnhandledException { get; init; }

	/// <summary>
	/// Indicates the steps.
	/// </summary>
	internal Step[]? InterimSteps { get; init; }

	/// <summary>
	/// Indicates the interim puzzles.
	/// </summary>
	private Puzzle[]? InterimPuzzleStates
	{
		get
		{
			if (InterimSteps is null)
			{
				return null;
			}

			var result = new Puzzle[InterimSteps.Length];
			var playground = Puzzle.Clone();
			for (var i = 0; i < InterimSteps.Length; i++)
			{
				playground.Apply(InterimSteps[i]);
				result[i] = playground.Clone();
			}
			return result;
		}
	}

	[EquatableMember]
	private Puzzle PuzzleEntry => Puzzle;


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
				var topColor = Puzzle[step.StartTubeIndex].TopColor;
				var topColorString = topColor == Color.MaxValue ? "<empty>" : topColor.ToString();
				sb.AppendLine($"{step} (color {topColorString}, size {Puzzle[step.StartTubeIndex].TopColorSpannedCount})");
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
}
