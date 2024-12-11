namespace Puzzles.WaterSort.Analytics;

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
	/// Indicates the random number generator.
	/// </summary>
	private readonly Random _rng = Random.Shared;


	/// <summary>
	/// Indicates whether the analyzer randomly choose a step to be applied.
	/// </summary>
	/// <remarks><b><i>Please note that this option may cause the puzzle to be failed to analyze.</i></b></remarks>
	public bool RandomSelectSteps { get; set; }


	/// <summary>
	/// Try to analyze a puzzle.
	/// </summary>
	/// <param name="puzzle">The puzzle.</param>
	/// <returns>An <see cref="AnalysisResult"/> instance indicating result.</returns>
	public AnalysisResult Analyze(Puzzle puzzle)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		try
		{
			var steps = new List<Step>();
			var playground = puzzle.Clone();
			var depth = puzzle.Depth;
			while (!playground.IsSolved || !playground.TrueForAll(tube => tube.Length == depth || tube.Length == 0))
			{
				var foundSteps = _collector.Collect(playground);
				if (foundSteps.Length == 0)
				{
					return new(puzzle) { IsSolved = false, FailedReason = FailedReason.PuzzleInvalid };
				}

				var step = foundSteps[RandomSelectSteps ? _rng.Next(0, foundSteps.Length) : 0];
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
