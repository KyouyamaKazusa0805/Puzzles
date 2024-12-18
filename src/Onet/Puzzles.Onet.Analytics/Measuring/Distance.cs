namespace Puzzles.Onet.Measuring;

/// <summary>
/// Provides a list of methods that operate with distances.
/// </summary>
public static class Distance
{
	/// <summary>
	/// Get distance value.
	/// </summary>
	/// <param name="match">The match.</param>
	/// <param name="type">The type.</param>
	/// <returns>The distance.</returns>
	/// <exception cref="ArgumentOutOfRangeException">The distance type.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double GetDistance(this ItemMatch match, DistanceType type)
		=> type switch
		{
			DistanceType.Euclid => GetEuclidDistance(match.Start, match.End),
			DistanceType.Manhattan => GetManhattanDistance(match.Start, match.End),
			DistanceType.Solved => GetSolvedDistance(match),
			_ => throw new ArgumentOutOfRangeException(nameof(type))
		};

	/// <summary>
	/// Get Euclid distance for two <see cref="Coordinate"/> instances.
	/// </summary>
	/// <param name="a">Indicates the first coordinate to be checked.</param>
	/// <param name="b">Indicates the second coordinate to be checked.</param>
	/// <returns>A <see cref="double"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double GetEuclidDistance(Coordinate a, Coordinate b)
	{
		var ((ax, ay), (bx, by)) = (a, b);
		return Math.Sqrt((ax - ay) * (ax - ay) + (bx - by) * (bx - by));
	}

	/// <summary>
	/// Get Manhattan distance for two <see cref="Coordinate"/> instances.
	/// </summary>
	/// <param name="a">Indicates the first coordinate to be checked.</param>
	/// <param name="b">Indicates the second coordinate to be checked.</param>
	/// <returns>A <see cref="double"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetManhattanDistance(Coordinate a, Coordinate b)
	{
		var ((ax, ay), (bx, by)) = (a, b);
		return Math.Abs(ax - bx) + Math.Abs(ay - by);
	}

	/// <summary>
	/// Get solved distance for two <see cref="Coordinate"/> instances.
	/// </summary>
	/// <param name="match">Indicates the match to be checked.</param>
	/// <param name="weight">The weight value. By default the value is 3.</param>
	/// <returns>A <see cref="double"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetSolvedDistance(ItemMatch match, int weight = 3) => weight * match.Interims.Length + match.Distance;
}
