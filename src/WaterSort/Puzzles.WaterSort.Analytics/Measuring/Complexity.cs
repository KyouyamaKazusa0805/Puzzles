namespace Puzzles.WaterSort.Measuring;

/// <summary>
/// Represents a list of methods that can calculate complexity.
/// </summary>
public static class Complexity
{
	/// <summary>
	/// Returns the complexity of the color.
	/// </summary>
	/// <param name="color">The color.</param>
	/// <param name="puzzle">The puzzle.</param>
	/// <returns>The complexity.</returns>
	public static int GetColorComplexity(Color color, Puzzle puzzle)
	{
		var result = 0;
		for (var i = 0; i < puzzle.Length; i++)
		{
			var tube = puzzle[i];

			// Push colors into the temporary list.
			var colorsList = new List<Color>();
			foreach (var c in tube)
			{
				if (colorsList.Count == 0 || colorsList[^1] != c)
				{
					colorsList.Add(c);
				}
			}

			// Check the colors in order to update score dictionary table.
			var j = 1;
			foreach (var c in colorsList)
			{
				if (c == color)
				{
					result += j;
				}
				j++;
			}
		}
		return result;
	}

	/// <summary>
	/// Returns the complexity of the tube.
	/// </summary>
	/// <param name="tube">The tube.</param>
	/// <param name="puzzle">The puzzle.</param>
	/// <returns>The complexity.</returns>
	public static int GetTubeComplexity(Tube tube, Puzzle puzzle)
	{
		var colorsList = new List<Color>();
		foreach (var color in tube)
		{
			if (colorsList.Count == 0 || colorsList[^1] != color)
			{
				colorsList.Add(color);
			}
		}
		return tube.ColorsCount * colorsList.Count;
	}

	/// <summary>
	/// Calcualtes complexity dictionary on both colors and tubes.
	/// </summary>
	/// <param name="puzzle">The puzzle.</param>
	/// <param name="colorComplexity">The complexity dictionary grouped by colors.</param>
	/// <param name="tubeComplexity">The complexity dictionary grouped by tubes, whose key is index of the tube.</param>
	internal static void GetComplexityDictionary(Puzzle puzzle, out Dictionary<Color, int> colorComplexity, out Dictionary<int, int> tubeComplexity)
	{
		var (c, t) = (new Dictionary<Color, int>(), new Dictionary<int, int>());
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
			t.Add(i, tubeDifficulty);

			// Check the colors in order to update score dictionary table.
			var j = 1;
			foreach (var color in colorsList)
			{
				if (!c.TryAdd(color, j))
				{
					c[color] += j;
				}
				j++;
			}
		}
		(colorComplexity, tubeComplexity) = (c, t);
	}
}
