namespace Puzzles.Hamiltonian.Measuring;

/// <summary>
/// Represents a degree measurer.
/// </summary>
public static class Degree
{
	/// <summary>
	/// Gets the sum of the degree value.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The sum degree value.</returns>
	public static int GetDegreeSum(Graph graph)
	{
		var result = 0;
		foreach (var coordinate in graph.EnumerateCoordinates())
		{
			result += graph.GetDegreeAt(coordinate);
		}
		return result;
	}

	/// <summary>
	/// Gets a dictionary that describes the times of appearing of nodes of the specified degree.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <returns>The dictionary that describes the times of appearing.</returns>
	public static FrozenDictionary<int, int> GetDegreeFrequency(Graph graph)
	{
		var result = new Dictionary<int, int>(4);
		foreach (var coordinate in graph.EnumerateCoordinates())
		{
			var degree = graph.GetDegreeAt(coordinate);
			if (!result.TryAdd(degree, 1))
			{
				result[degree]++;
			}
		}
		return result.ToFrozenDictionary();
	}
}
