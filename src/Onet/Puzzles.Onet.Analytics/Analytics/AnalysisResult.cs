namespace Puzzles.Onet.Analytics;

/// <summary>
/// Represents a type that stores the result of a analysis operation.
/// </summary>
/// <param name="grid">Indicates the base grid.</param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class AnalysisResult([Property, HashCodeMember] Grid grid) :
	IEnumerable<ItemMatch>,
	IEquatable<AnalysisResult>,
	IEqualityOperators<AnalysisResult, AnalysisResult, bool>
{
	/// <summary>
	/// Indicates whether the puzzle is fully solved.
	/// </summary>
	[HashCodeMember]
	[EquatableMember]
	public required bool IsSolved { get; init; }

	/// <summary>
	/// Indicates the total difficulty.
	/// </summary>
	public double TotalDifficulty => Matches.Sum(static match => match.Difficulty);

	/// <summary>
	/// Indicates the maxinum difficulty.
	/// </summary>
	public double MaxDifficulty => Matches.Max(static match => match.Difficulty);

	/// <summary>
	/// Indicates the failed reason.
	/// </summary>
	public FailedReason FailedReason { get; init; }

	/// <summary>
	/// Indicates the matches found during the analysis.
	/// </summary>
	public ReadOnlySpan<ItemMatch> Matches => InterimMatches;

	/// <summary>
	/// Indicates the grids applied in each step in solving procedure.
	/// </summary>
	public ReadOnlySpan<Grid> Grids => InterimGrids;

	/// <summary>
	/// Indicates the elapsed time.
	/// </summary>
	public TimeSpan ElapsedTime { get; init; }

	/// <summary>
	/// Indicates the exception encountered.
	/// </summary>
	public Exception? UnhandledException { get; init; }

	/// <summary>
	/// Indicates the matches.
	/// </summary>
	internal ItemMatch[]? InterimMatches { get; init; }

	/// <summary>
	/// Indicates the interim grids.
	/// </summary>
	internal Grid[]? InterimGrids
	{
		get
		{
			if (InterimMatches is null)
			{
				return null;
			}

			var grid = Grid.Clone();
			var result = new Grid[InterimMatches.Length];
			for (var i = 0; i < InterimMatches.Length; i++)
			{
				var match = InterimMatches[i];
				grid.Apply(match);
				result[i] = grid.Clone();
			}
			return result;
		}
	}

	[EquatableMember]
	private Grid PuzzleEntry => Grid;


	/// <inheritdoc/>
	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.AppendLine("Puzzle:");
		sb.AppendLine(Grid.ToString());
		sb.AppendLine("---");

		if (IsSolved)
		{
			if (InterimMatches is not null)
			{
				sb.AppendLine("Steps:");
				foreach (var step in InterimMatches)
				{
					sb.AppendLine(step.ToFullString());
				}
				sb.AppendLine("---");
				sb.AppendLine("Puzzle is solved.");
				sb.AppendLine($@"Elapsed time: {ElapsedTime:hh\:mm\:ss\.fff}");
			}
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

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<ItemMatch> GetEnumerator() => new(Matches);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => (InterimMatches ?? []).GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<ItemMatch> IEnumerable<ItemMatch>.GetEnumerator() => (InterimMatches ?? []).AsEnumerable().GetEnumerator();
}
