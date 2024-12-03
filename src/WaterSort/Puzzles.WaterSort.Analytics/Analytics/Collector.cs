namespace WaterSort.Analytics;

/// <summary>
/// Represents a collector.
/// </summary>
public sealed class Collector
{
	/// <summary>
	/// Gets all possible steps.
	/// </summary>
	/// <param name="puzzle">The puzzle.</param>
	/// <param name="depth">
	/// The depth of all tube, meaning the maximum number of different colors can be appeared inside one tube.
	/// </param>
	/// <returns>The result steps.</returns>
	public ReadOnlySpan<Step> Collect(Puzzle puzzle, int depth)
	{
		var result = new List<Step>();

		// Peek all tubes, to get all possible colors that can be created as steps.
		var dictionary = new Dictionary<int, Color>();
		for (var i = 0; i < puzzle.Length; i++)
		{
			dictionary.Add(i, puzzle[i].TopColor);
		}

		// Find a pair of moves, and determine whether such step is valid, limited by the specified depth.
		foreach (var color in dictionary.Values.ToHashSet())
		{
			// Find different tubes whose peek item is the current color,
			// and group them by the number of same-colored elements from top.
			var validTubes = new List<int>();
			for (var i = 0; i < puzzle.Length; i++)
			{
				var tube = puzzle[i];
				if (tube.TopColor == color || tube.TopColor == Color.MaxValue)
				{
					validTubes.Add(i);
				}
			}

			// Iterate two dictionaries "from" and "to", to make valid moves.
			foreach (var fromTubeIndex in validTubes)
			{
				if (puzzle[fromTubeIndex].TopColor == Color.MaxValue)
				{
					continue;
				}

				foreach (var toTubeIndex in validTubes)
				{
					if (fromTubeIndex == toTubeIndex)
					{
						continue;
					}

					// Check whether the move is invalid: [a] -> [b] with [a] only contains one color.
					// This moving operation may cause a cyclic moving: [0] -> [1] and then [1] -> [0].
					if (puzzle[fromTubeIndex].IsMonocolored && puzzle[toTubeIndex].IsEmpty)
					{
						continue;
					}

					// Check the number of top elements.
					var topCount = puzzle[fromTubeIndex].TopColorSpannedCount;
					var lastPositionsCount = depth - puzzle[toTubeIndex].Length;
					if (lastPositionsCount < topCount)
					{
						// Invalid move.
						continue;
					}

					result.Add(new(fromTubeIndex, toTubeIndex));
				}
			}
		}
		return result.AsSpan();
	}
}
