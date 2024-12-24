namespace Puzzles.MahjongMatching.Analytics;

/// <summary>
/// Represents a collector.
/// </summary>
public sealed class Analyzer
{
	/// <summary>
	/// Collect for all possible matches.
	/// </summary>
	/// <param name="puzzle">The puzzle to be checked.</param>
	/// <returns>A list of matches.</returns>
	public AnalysisResult Analyze(Puzzle puzzle)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		try
		{
			var steps = new List<TileMatch>();
			var playground = puzzle.Clone();
			while (playground.ItemsCount != 0)
			{
				var foundSteps = playground.GetAllMatches();
				if (foundSteps.Length == 0)
				{
					return new(puzzle) { IsSolved = false, FailedReason = FailedReason.PuzzleInvalid };
				}

				var step = foundSteps[0];
				steps.Add(step);
				playground.Apply(step);
			}

			return new(puzzle) { IsSolved = true, InterimSteps = [.. steps], ElapsedTime = stopwatch.Elapsed };
		}
		catch (Exception ex)
		{
			return new(puzzle) { IsSolved = false, FailedReason = FailedReason.UnhandledException, UnhandledException = ex };
		}
		finally
		{
			stopwatch.Stop();
		}
	}
}
