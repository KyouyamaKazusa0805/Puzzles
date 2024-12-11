namespace Puzzles.WaterSort.Generating;

/// <summary>
/// Represents a generator.
/// </summary>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public readonly ref partial struct Generator()
{
	/// <summary>
	/// Indicates the backing random number generator.
	/// </summary>
	private readonly Random _random = new();

	/// <summary>
	/// Indicates the analyzer.
	/// </summary>
	private readonly Analyzer _analyzer = new();


	/// <summary>
	/// Generates a puzzle that contains the specified number of tubes, and the maximum depth is the specified one.
	/// </summary>
	/// <param name="tubesCount">The number of tubes.</param>
	/// <param name="colorsCount">The number of colors.</param>
	/// <param name="depth">The depth.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Puzzle"/> instance; or <see langword="null"/> if cancelled.</returns>
	/// <exception cref="ArgumentException">Throws when the argument <paramref name="colorsCount"/> is invalid.</exception>
	public Puzzle Generate(int tubesCount, Color colorsCount, int depth, CancellationToken cancellationToken = default)
	{
		// Check validity of the argument.
		if (tubesCount < colorsCount)
		{
			throw new ArgumentException(
				$"The specified argument '{nameof(colorsCount)}' cannot be satisified because of too large.",
				nameof(colorsCount)
			);
		}

		while (true)
		{
			// Creates a solution.
			var tempTubeLists = new List<Color>[tubesCount];
			for (var i = 0; i < tubesCount; i++)
			{
				tempTubeLists[i] = [];
				if (i < colorsCount)
				{
					for (var j = 0; j < depth; j++)
					{
						tempTubeLists[i].Add((Color)i);
					}
				}
			}

			// Create a list of tubes.
			var tubes = new Tube[tubesCount];
			for (var i = 0; i < tubesCount; i++)
			{
				tubes[i] = new(new(tempTubeLists[i]));
			}

			// Randomly creates steps that can be reverted as normal steps.
			// We choose the algorithm that uses inversion logic to create puzzles,
			// in order to control limit of the number of steps cost in one puzzle.
			var randomStepsCount = _random.Next(5, 40);
			for (var i = 0; i < randomStepsCount;)
			{
				// Step 1: Randomly choose a tube that contains at least two sequential unit of same-colored water.
				// But firstly we should add all possible tubes that can be valid as the start tube in the current step.
				// If there's no tube contains >= 2 units of same-colored water from the top, the puzzle will become the final
				// and we cannot create a new step any more.
				var validStartTubesIndices = new List<int>();
				for (var tempStartTubeIndex = 0; tempStartTubeIndex < tubesCount; tempStartTubeIndex++)
				{
					if (tubes[tempStartTubeIndex].TopColorSpannedCount >= 2)
					{
						validStartTubesIndices.Add(tempStartTubeIndex);
					}
				}
				if (validStartTubesIndices.Count == 0)
				{
					// The puzzle will become the final.
					break;
				}

				var startTubeIndex = validStartTubesIndices[_random.Next(0, validStartTubesIndices.Count)];
				var maxPourOutLength = tubes[startTubeIndex].TopColorSpannedCount;

				// Step 2: Randomly choose a number between 1 and size (>= 1 and < size),
				// where the size is equal to 'tubes[chosenTubeIndex].TopColorSpannedCount'.
				var pourOutLength = _random.Next(1, maxPourOutLength);

				// Step 3: Iterate on all cases between 1 and 'pourOutLength + 1' (>= 1 and <= pourOutLength),
				// determine whether at least one case is valid -
				// there's at least one tube can accommodate with such sequential water of same color.
				var endTube = default(Tube);
				for (var testPourOutLength = pourOutLength; testPourOutLength >= 1; testPourOutLength--)
				{
					// Randomly find a tube that is either an empty tube or a normal tube
					// whose top color is different with the current tube whose cooresponding index is 'chosenTubeIndex'.
					var validTubeIndices = new List<int>();
					for (var tempEndTubeIndex = 0; tempEndTubeIndex < tubesCount; tempEndTubeIndex++)
					{
						if (startTubeIndex == tempEndTubeIndex)
						{
							// Skip for same tube.
							continue;
						}

						// Check whether the current tube can accommodate with such water from the start tube.
						var tempEndTube = tubes[tempEndTubeIndex];
						if (tempEndTube.TopColor != tubes[startTubeIndex].TopColor && depth - tempEndTube.Length >= pourOutLength)
						{
							validTubeIndices.Add(tempEndTubeIndex);
						}
					}
					if (validTubeIndices.Count == 0)
					{
						// There's not a tube can be accommodated with that required water.
						continue;
					}

					// If here, we know that there's at least one tube is valid as a receiver.
					// Now randomly choose one.
					endTube = tubes[validTubeIndices[_random.Next(0, validTubeIndices.Count)]];
					break;
				}
				if (endTube is null)
				{
					// The current start position chosen is invalid. Back to step 1.
					continue;
				}

				// Create a step. Here the step is an inverted one in logic.
				// For example, if a step is move water from index [3] to [8], the step here is [8] to [3].
				// This keeps the puzzle to be valid (at least one possible step can be operated).
				endTube.Push(tubes[startTubeIndex].Pop(pourOutLength), pourOutLength);
				i++;
			}

			// Just check whether auxiliary tubes contain at least one not empty.
			// If so, we should pour out it into the other tubes which are not full.
			// From the algorithm above, we know that the auxiliary tubes are at the several last indices.
			for (var i = colorsCount; i < tubesCount; i++)
			{
				var tube = tubes[i];
				if (tube.IsEmpty)
				{
					continue;
				}

				// If the tube is not empty, we should arrange the containing water into the other tubes which are not full.
				while (!tube.IsEmpty)
				{
					for (var j = 0; j < colorsCount; j++)
					{
						var targetTube = tubes[j];
						if (targetTube.Length == depth || targetTube.Length == 0)
						{
							continue;
						}

						// Pour in & pour out.
						targetTube.Push(tube.Pop(1), 1);
						break;
					}
				}
			}

			// Randomly shuffles the tubes.
			for (var i = 0; i < 10; i++)
			{
				var a = _random.Next(0, tubesCount);
				var b = _random.Next(0, tubesCount);
				if (a == b)
				{
					// To make a != b.
					b = (b + 1) % tubesCount;
				}

				// Swaps them.
				(tempTubeLists[a], tempTubeLists[b]) = (tempTubeLists[b], tempTubeLists[a]);
			}

			// Create the puzzle via tubes.
			var puzzle = new Puzzle(tubes);
			if (_analyzer.Analyze(puzzle).IsSolved)
			{
				return puzzle;
			}

			if (cancellationToken.IsCancellationRequested)
			{
				return null!;
			}
		}
	}
}
