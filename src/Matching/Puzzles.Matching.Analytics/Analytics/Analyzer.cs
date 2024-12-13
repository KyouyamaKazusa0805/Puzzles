namespace Puzzles.Matching.Analytics;

/// <summary>
/// Represents an analyzer object that can analyze a match puzzle.
/// </summary>
public sealed class Analyzer
{
	/// <summary>
	/// Indicates the backing random number generator.
	/// </summary>
	private readonly Random _rng = new();


	/// <summary>
	/// Indicates whether the analyzer will randomly select steps to be applied.
	/// </summary>
	public bool RandomSelectSteps { get; set; }


	/// <summary>
	/// Try to analyze a puzzle, and return the steps found, encapsulated by <see cref="AnalysisResult"/>.
	/// </summary>
	/// <param name="grid">The grid to be analyzed.</param>
	/// <param name="cancellationToken">Indicates the cancellation token that can cancel the current operation.</param>
	/// <returns>An instance of type <see cref="AnalysisResult"/> indicating the result information.</returns>
	public AnalysisResult Analyze(Grid grid, CancellationToken cancellationToken = default)
	{
		try
		{
			var stopwatch = new Stopwatch();
			var playground = grid.Clone();
			var steps = new List<ItemMatch>();

			stopwatch.Start();
			while (!playground.IsEmpty)
			{
				var allMatches = playground.GetAllMatches();
				if (allMatches.IsEmpty)
				{
					return new(grid) { IsSolved = false, InterimMatches = [.. steps], FailedReason = FailedReason.PuzzleInvalid };
				}

				var matchesSorted =
					from match in allMatches
					orderby match.TurningCount, match.Distance
					select match;
				var first = matchesSorted[RandomSelectSteps ? _rng.Next(0, matchesSorted.Length) : 0];
				playground.Apply(first);
				steps.Add(first);

				cancellationToken.ThrowIfCancellationRequested();
			}

			stopwatch.Stop();
			return new(grid) { IsSolved = true, InterimMatches = [.. steps], ElapsedTime = stopwatch.Elapsed };
		}
		catch (OperationCanceledException)
		{
			return new(grid) { IsSolved = false, FailedReason = FailedReason.UserCancelled };
		}
		catch (Exception ex)
		{
			return new(grid) { IsSolved = false, FailedReason = FailedReason.UnhandledException, UnhandledException = ex };
		}
	}
}
