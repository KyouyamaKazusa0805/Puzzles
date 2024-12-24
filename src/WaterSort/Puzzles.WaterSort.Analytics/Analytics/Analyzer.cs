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
	/// Indicates the backing random number generator.
	/// </summary>
	private readonly Random _rng = new();


	/// <summary>
	/// Indicates the preference weight.
	/// </summary>
	public double PreferenceWeight { get; set; } = 1;

	/// <summary>
	/// Indicates the visual distance weight.
	/// </summary>
	public double VisualDistanceWeight { get; set; } = .05;

	/// <summary>
	/// Indicates temporature factor.
	/// </summary>
	public double TemporatureFactor { get; set; } = 2;

	/// <summary>
	/// Indicates the calculating distance type.
	/// </summary>
	public DistanceType DistanceType { get; set; } = DistanceType.Manhattan;

	/// <summary>
	/// Represents a start point creator.
	/// </summary>
	public Func<Puzzle, DoublePair> StartPointCreator { get; set; } = static g => g.GetCentralPoint();


	/// <summary>
	/// Try to analyze a puzzle.
	/// </summary>
	/// <param name="puzzle">The puzzle.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>An <see cref="AnalysisResult"/> instance indicating result.</returns>
	public AnalysisResult Analyze(Puzzle puzzle, CancellationToken cancellationToken = default)
	{
		try
		{
			var stopwatch = new Stopwatch();
			var playground = puzzle.Clone();
			var steps = new List<Step>();

			stopwatch.Start();
			while (!playground.IsSolved)
			{
				var allMatches = _collector.Collect(playground);
				if (allMatches.IsEmpty)
				{
					return new(puzzle) { IsSolved = false, InterimSteps = [.. steps], FailedReason = FailedReason.PuzzleInvalid };
				}

				// Sort all steps by difficulty ratings, and find for the best one.
				var ((psx, psy), (pex, pey)) = steps is [.., var (start, end)]
					&& (puzzle.GetTubeCoordinate(start), puzzle.GetTubeCoordinate(end)) is var ((sx, sy), (ex, ey))
					? ((sx, sy), (ex, ey))
					: (StartPointCreator(puzzle), StartPointCreator(puzzle));
				var matchesDictionary = new Dictionary<Step, double>(allMatches.Length);
				var maximumDifficulty = double.MinValue;
				foreach (var match in allMatches)
				{
					var (nx, ny) = puzzle.GetTubeCoordinate(match.Start);
					var (nex, ney) = puzzle.GetTubeCoordinate(match.End);
					var d1 = getVisualDistance(DistanceType, psx, psy, nx, ny);
					var d2 = getVisualDistance(DistanceType, pex, pey, nx, ny);
					var d3 = getVisualDistance(DistanceType, psx, psy, nex, ney);
					var d4 = getVisualDistance(DistanceType, pex, pey, nex, ney);
					var d5 = getDistance(DistanceType, match, nx, ny, (nx, ny), nex, ney, (nex, ney));

					var v = playground[match.Start].TopColorSpannedCount + playground[match.End].TopColorSpannedCount;
					var f = playground[match.End].IsMonocolored ? 0 : 2;
					var difficulty = (d1 + d2 + d3 + d4 + 4 * d5) * VisualDistanceWeight + (v + f) * PreferenceWeight;
					matchesDictionary.Add(match, difficulty);
					match.Difficulty = difficulty;

					if (difficulty >= maximumDifficulty)
					{
						maximumDifficulty = difficulty;
					}
				}

				var matchesWeightDictionary = new Dictionary<Step, double>(allMatches.Length);
				var weightSum = 0D;
				foreach (var newMatch in matchesDictionary.Keys)
				{
					var currentDifficulty = matchesDictionary[newMatch];
					var weight = Math.Exp((maximumDifficulty - currentDifficulty + 1) / TemporatureFactor);
					matchesWeightDictionary.Add(newMatch, weight);
					weightSum += weight;
				}

				var matchesPossibilityDictionary = new Dictionary<Step, double>(allMatches.Length);
				foreach (var newMatch in matchesDictionary.Keys)
				{
					var currentDifficulty = matchesDictionary[newMatch];
					var weight = matchesWeightDictionary[newMatch];
					var possibility = weight / weightSum;
					matchesPossibilityDictionary.Add(newMatch, possibility);
				}

				var possibilityNodes = new List<double> { 0 };
				var previousPossibilityValue = 0D;
				foreach (var key in matchesDictionary.Keys)
				{
					var value = matchesPossibilityDictionary[key];
					possibilityNodes.Add(previousPossibilityValue + value);
					previousPossibilityValue += value;
				}

				var selectedPossibility = _rng.NextDouble();
				var selectedIndex = 0;
				for (; selectedIndex < possibilityNodes.Count - 1; selectedIndex++)
				{
					var p = possibilityNodes[selectedIndex];
					var q = possibilityNodes[selectedIndex + 1];
					if (selectedPossibility >= p && selectedPossibility <= q)
					{
						break;
					}
				}

				var selectedMatch = matchesDictionary.ElementAt(selectedIndex).Key;
				playground.Apply(selectedMatch);
				steps.Add(selectedMatch);

				cancellationToken.ThrowIfCancellationRequested();
			}

			stopwatch.Stop();
			return new(puzzle) { IsSolved = true, InterimSteps = [.. steps], ElapsedTime = stopwatch.Elapsed };
		}
		catch (OperationCanceledException)
		{
			return new(puzzle) { IsSolved = false, FailedReason = FailedReason.UserCancelled };
		}
		catch (Exception ex)
		{
			return new(puzzle) { IsSolved = false, FailedReason = FailedReason.UnhandledException, UnhandledException = ex };
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double getDistance(
			DistanceType distanceType,
			Step newMatch,
			int nx,
			int ny,
			DoublePair ns,
			int nex,
			int ney,
			DoublePair ne
		) => distanceType switch
		{
			DistanceType.Euclid => Math.Sqrt((nex - nx) * (nex - nx) + (ney - ny) * (ney - ny)),
			DistanceType.Manhattan => Math.Abs(nex - nx) + Math.Abs(ney - ny),
			DistanceType.Solved => getLength(ns, ne),
			_ => 0
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double getVisualDistance(DistanceType distanceType, double px, double py, int nx, int ny)
			=> distanceType switch
			{
				DistanceType.Euclid => Math.Sqrt((px - nx) * (px - nx) + (py - ny) * (py - ny)),
				DistanceType.Manhattan => Math.Abs(px - nx) + Math.Abs(py - ny),
				_ => 0
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double getLength(DoublePair coordinate1, DoublePair coordinate2)
			=> Math.Abs(coordinate1.X - coordinate2.X) + Math.Abs(coordinate1.Y - coordinate2.Y);
	}
}
