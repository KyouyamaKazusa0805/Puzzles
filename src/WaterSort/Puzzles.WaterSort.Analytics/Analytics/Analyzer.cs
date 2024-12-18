namespace Puzzles.WaterSort.Analytics;

using ScorePair = (int StartScore, int EndScore);

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
			var d = 0;
			while (!playground.IsSolved)
			{
				var foundSteps = _collector.Collect(playground);
				if (foundSteps.Length == 0
					|| (
						foundSteps.MakeDifficulty(playground, false) is { Keys: { Count: not 0 } keys } stepsDictionary
							? stepsDictionary.TryGetValue(keys.First(), out var candidates) && candidates.Count != 0
								? candidates[0]
								: ((Step, int)?)null
							: null
					) is not var (step, difficulty))
				{
					return new(puzzle) { IsSolved = false, InterimSteps = [.. steps], FailedReason = FailedReason.PuzzleInvalid };
				}

				d += difficulty;
				steps.Add(step);
				playground.Apply(step);
			}

			return new(puzzle)
			{
				IsSolved = true,
				InterimSteps = [.. steps],
				TotalDifficulty = d,
				ElapsedTime = stopwatch.Elapsed
			};
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

/// <include file='../../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	/// <summary>
	/// Create a dictionary that stores difficulty rating and steps.
	/// </summary>
	/// <param name="steps">The steps.</param>
	/// <param name="puzzle">The puzzle.</param>
	/// <param name="fixRoot">
	/// Indicates whether the tube will be fixed and won't create steps on root tubes
	/// (a tube that only contains one color, with size 1).
	/// </param>
	/// <returns>A dictionary.</returns>
	public static SortedDictionary<ScorePair, List<(Step Step, int Difficulty)>> MakeDifficulty(this ReadOnlySpan<Step> steps, Puzzle puzzle, bool fixRoot)
	{
		// Record scores on color and tube, in order to be used later.
		var scoreDic = new Dictionary<Color, int>();
		var tubeDic = new Dictionary<int, int>();
		for (var i = 0; i < puzzle.Length; i++)
		{
			var tube = puzzle[i];

			// Update tube dictionary table.
			var colorsCount = tube.ColorsCount;

			// Push colors into the temporary list.
			var colorsList = new List<Color>();
			foreach (var color in tube)
			{
				if (colorsList.Count == 0 || colorsList[^1] != color)
				{
					colorsList.Add(color);
				}
			}
			var tubeDifficulty = colorsCount * colorsList.Count;
			tubeDic.Add(i, tubeDifficulty);

			// Check the colors in order to update score dictionary table.
			var j = 1;
			foreach (var color in colorsList)
			{
				if (!scoreDic.TryAdd(color, j))
				{
					scoreDic[color] += j;
				}
				j++;
			}
		}

		// Remove color entries that is not as top color in tubes.
		foreach (var color in scoreDic.Keys.Except(from tube in puzzle select tube.TopColor))
		{
			scoreDic.Remove(color);
		}

		// Update scores of each step.
		// We choosing a step to be applied will create 3 different cases:
		//   (1) Two tubes with only one same color
		//   (2) The step chooses the same color with the minimum score of color table
		//   (3) The step chooses the different color with the minimum score of color table
		// In working, case (1) has priority with (2), and (2) has priority with (3).

		// Now we should sort them by scores of each color, in ascending order.
		var case1Key = (0, 0);
		var resultComparer = Comparer<ScorePair>.Create(scorePairComparison);
		var result = new SortedDictionary<(int, int), List<(Step, int)>>(resultComparer);
		var invalidSteps = new HashSet<Step>();
		foreach (var minimumValueColor in from kvp in scoreDic orderby kvp.Value, kvp.Key select kvp.Key)
		{
			foreach (var step in steps)
			{
				var (startIndex, endIndex) = step;
				var startTube = puzzle[startIndex];
				var endTube = puzzle[endIndex];
				if (fixRoot && startTube.Length == 1)
				{
					// Fix for root tube.
					continue;
				}

				// Check (1).
				if (startTube.ColorsCount == 1 && endTube.ColorsCount == 1
					&& startTube.TopColor == endTube.TopColor
					&& startTube.TopColor == scoreDic.First().Key)
				{
					var diff = getDifficulty(step, minimumValueColor);
					if (!result.TryAdd(case1Key, [(step, diff)]))
					{
						result[case1Key].Add((step, diff));
					}

					invalidSteps.Remove(step);
					continue;
				}

				// Check (3).
				if (startTube.TopColor != minimumValueColor)
				{
					// Fallback to the next step.
					invalidSteps.Add(step);
					continue;
				}

				// Check (2).
				// Now we should check scores of bottle, in order to sort them by the score.
				var tubeScorePair = (tubeDic[startIndex], tubeDic[endIndex]);
				var case2Diff = getDifficulty(step, minimumValueColor);
				if (!result.TryAdd(tubeScorePair, [(step, case2Diff)]))
				{
					result[tubeScorePair].Add((step, case2Diff));
				}
				invalidSteps.Remove(step);
			}
			if (result.Count != 0)
			{
				return result;
			}
		}

		var case3Key = (100, 100);
		foreach (var invalidStep in invalidSteps)
		{
			var pair = (invalidStep, 100);
			if (result.TryAdd(case3Key, [pair]))
			{
				result[case3Key].Add(pair);
			}
		}
		return result;


		static int scorePairComparison(ScorePair x, ScorePair y)
		{
			var (xs, xe) = x;
			var (ys, ye) = y;
			return xs.CompareTo(ys) is var r1 and not 0 ? r1 : xe.CompareTo(ye) is var r2 and not 0 ? r2 : 0;
		}

		int getDifficulty(Step step, Color color) => (tubeDic[step.StartTubeIndex] + color) * 5;
	}
}
