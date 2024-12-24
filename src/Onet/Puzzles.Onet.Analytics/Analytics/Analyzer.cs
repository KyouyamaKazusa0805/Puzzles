namespace Puzzles.Onet.Analytics;

/// <summary>
/// Represents an analyzer object that can analyze a match puzzle.
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
	/// Indicates the distance weight.
	/// </summary>
	public double DistanceWeight { get; set; } = 1;

	/// <summary>
	/// Indicates the visual distance weight.
	/// </summary>
	public double VisualDistanceWeight { get; set; } = .3;

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
	public Func<Grid, DoublePair> StartPointCreator { get; set; } = static g => (g.RowsLength / 2D, g.ColumnsLength / 2D);


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
				var allMatches = _collector.Collect(playground);
				if (allMatches.IsEmpty)
				{
					return new(grid) { IsSolved = false, InterimMatches = [.. steps], FailedReason = FailedReason.PuzzleInvalid };
				}

				// Sort all steps by difficulty ratings, and find for the best one.
				var ((psx, psy), (pex, pey)) = steps is [.., var ((sx, sy), (ex, ey), _)]
					? ((sx, sy), (ex, ey))
					: (StartPointCreator(grid), StartPointCreator(grid));
				var matchesDictionary = new Dictionary<ItemMatch, double>(allMatches.Length << 1);
				var maximumDifficulty = double.MinValue;
				foreach (var match in allMatches)
				{
					foreach (var isReversed in (false, true))
					{
						var newMatch = isReversed ? ~match : match with { };
						_ = (newMatch.Start.X, newMatch.Start.Y) is (var nx, var ny) ns;
						_ = (newMatch.End.X, newMatch.End.Y) is (var nex, var ney) ne;
						var d2 = getVisualDistance(DistanceType, pex, pey, nx, ny);
						var d4 = getVisualDistance(DistanceType, pex, pey, nex, ney);
						var distance = getDistance(DistanceType, newMatch, nx, ny, ns, nex, ney, ne);
						var difficulty = (2 * d2 + d4) * VisualDistanceWeight + distance * DistanceWeight;
						matchesDictionary.Add(newMatch, difficulty);
						newMatch.Difficulty = difficulty;

						if (difficulty >= maximumDifficulty)
						{
							maximumDifficulty = difficulty;
						}
					}
				}

				var matchesWeightDictionary = new Dictionary<ItemMatch, double>(allMatches.Length << 1);
				var weightSum = 0D;
				foreach (var newMatch in matchesDictionary.Keys)
				{
					var currentDifficulty = matchesDictionary[newMatch];
					var weight = Math.Exp((maximumDifficulty - currentDifficulty + 1) / TemporatureFactor);
					matchesWeightDictionary.Add(newMatch, weight);
					weightSum += weight;
				}

				var matchesPossibilityDictionary = new Dictionary<ItemMatch, double>(allMatches.Length << 1);
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


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double getDistance(DistanceType distanceType, ItemMatch newMatch, int nx, int ny, (int X, int Y) ns, int nex, int ney, (int X, int Y) ne)
			=> distanceType switch
			{
				DistanceType.Euclid => Math.Sqrt((nex - nx) * (nex - nx) + (ney - ny) * (ney - ny)),
				DistanceType.Manhattan => Math.Abs(nex - nx) + Math.Abs(ney - ny),
				DistanceType.Solved => getSolvedDistance(ns, ne, from i in newMatch.Interims select (DoublePair)(i.X, i.Y)),
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

#pragma warning disable format
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double getSolvedDistance(DoublePair start, DoublePair end, ReadOnlySpan<DoublePair> interims)
			=> interims switch
			{
				[var a, var b] => getLength(start, a) + getLength(a, b) + getLength(b, end),
				[var a] => getLength(start, a) + getLength(a, end),
				[] => getLength(start, end),
				_ => throw new InvalidOperationException("The internal data is invalid.")
			};
#pragma warning restore format

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static double getLength(DoublePair coordinate1, DoublePair coordinate2)
			=> Math.Abs(coordinate1.X - coordinate2.X) + Math.Abs(coordinate1.Y - coordinate2.Y);
	}
}
