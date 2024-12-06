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
	/// Gets an array of <see cref="int"/> values indicating the degree values on the path of cells;
	/// using <paramref name="ignoreUsedCells"/> to determine whether used cells should also be counted up.
	/// </summary>
	/// <param name="graph">The graph.</param>
	/// <param name="path">The path.</param>
	/// <param name="ignoreUsedCells">Indicates whether the cells used should be ignored.</param>
	/// <param name="isReversedOrder">Indicates whether the cells should use reversed order to be checked.</param>
	/// <returns>A list of <see cref="int"/> values indicating the degrees.</returns>
	public static ReadOnlySpan<int> GetPathDegrees(Graph graph, Path path, bool ignoreUsedCells, bool isReversedOrder)
	{
		var result = new List<int>(path.Length);
		var usedCells = ignoreUsedCells ? null : new HashSet<Coordinate>(path.Length);
		foreach (var coordinate in isReversedOrder ? path.EnumerateReversed() : path.Enumerate())
		{
			result.Add(ignoreUsedCells ? graph.GetDegreeAt(coordinate) : graph.GetDegreeAt(coordinate, usedCells!));
			usedCells?.Add(coordinate);
		}
		return result.AsSpan();
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

	/// <summary>
	/// Gets the degree value at the specified coordinate.
	/// </summary>
	/// <param name="this">The graph.</param>
	/// <param name="coordinate">The coordinate.</param>
	/// <param name="usedCells">The used cells.</param>
	/// <returns>The degree value. The return value must be 1, 2, 3 or 4.</returns>
	private static int GetDegreeAt(this Graph @this, Coordinate coordinate, HashSet<Coordinate> usedCells)
	{
		var result = 0;
		foreach (var direction in Enum.GetValues<Direction>().AsReadOnlySpan()[1..])
		{
			var newCoordinate = coordinate >> direction;
			if (!newCoordinate.IsOutOfBound(@this) && !usedCells.Contains(newCoordinate) && @this[newCoordinate])
			{
				result++;
			}
		}
		return result;
	}
}
