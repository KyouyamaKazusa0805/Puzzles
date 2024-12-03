namespace WaterSort.Analytics;

/// <summary>
/// Represents an analyzer.
/// </summary>
public sealed class Analyzer
{
	/// <summary>
	/// Indicates the backing collector.
	/// </summary>
	private readonly Collector _collector = new();


	/// <summary>
	/// Try to analyze a puzzle.
	/// </summary>
	/// <param name="puzzle">The puzzle.</param>
	/// <param name="depth">The depth of unified value on applying to each tube.</param>
	/// <returns>An <see cref="AnalysisResult"/> instance indicating result.</returns>
	public AnalysisResult Analyze(Puzzle puzzle, int depth)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		try
		{
			var steps = new List<Step>();
			var playground = puzzle.Clone();
			while (!playground || !playground.TrueForAll(tube => tube.Length == depth || tube.Length == 0))
			{
				var foundSteps = _collector.Collect(playground, depth);
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
