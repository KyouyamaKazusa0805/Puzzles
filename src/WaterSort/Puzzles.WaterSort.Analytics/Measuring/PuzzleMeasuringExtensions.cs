namespace Puzzles.WaterSort.Measuring;

/// <summary>
/// Provides with extension methods on <see cref="Puzzle"/>.
/// </summary>
/// <seealso cref="Puzzle"/>
public static class PuzzleMeasuringExtensions
{
	/// <summary>
	/// Indicates the tube coordinates for each tube in each size of puzzle.
	/// </summary>
	private static readonly Dictionary<int, Coordinate[]> TubeCoordinates = new()
	{
		{ 2, [new(0, 0), new(0, 2)] },
		{ 3, [new(0, 0), new(0, 2), new(1, 0)] },
		{ 4, [new(0, 1), new(0, 3), new(1, 0), new(1, 2)] },
		{ 5, [new(0, 0), new(0, 2), new(0, 4), new(1, 0), new(1, 2)] },
		{ 6, [new(0, 1), new(0, 3), new(0, 5), new(1, 0), new(1, 2), new(1, 4)] },
		{ 7, [new(0, 0), new(0, 2), new(0, 4), new(0, 6), new(1, 0), new(1, 2), new(1, 4)] },
		{ 8, [new(0, 1), new(0, 3), new(0, 5), new(0, 7), new(1, 0), new(1, 2), new(1, 4), new(1, 6)] },
		{ 9, [new(0, 0), new(0, 2), new(0, 4), new(0, 6), new(0, 8), new(1, 0), new(1, 2), new(1, 4), new(1, 6)] },
		{
			10,
			[
				new(0, 0), new(0, 2), new(0, 4), new(0, 6),
				new(1, 0), new(1, 2), new(1, 4), new(1, 6),
				new(2, 0), new(2, 2)
			]
		},
		{
			11,
			[
				new(0, 0), new(0, 2), new(0, 4), new(0, 6),
				new(1, 0), new(1, 2), new(1, 4), new(1, 6),
				new(2, 0), new(2, 2), new(2, 4)
			]
		},
		{
			12,
			[
				new(0, 1), new(0, 3), new(0, 5), new(0, 7),
				new(1, 1), new(1, 3), new(1, 5), new(1, 7),
				new(2, 0), new(2, 2), new(2, 4), new(2, 6)
			]
		},
		{
			13,
			[
				new(0, 1), new(0, 3), new(0, 5), new(0, 7),
				new(1, 0), new(1, 2), new(1, 4), new(1, 6), new(1, 8),
				new(2, 0), new(2, 2), new(2, 4), new(2, 6)
			]
		},
		{
			14,
			[
				new(0, 0), new(0, 2), new(0, 4), new(0, 6), new(0, 8),
				new(1, 0), new(1, 2), new(1, 4), new(1, 6), new(1, 8),
				new(2, 0), new(2, 2), new(2, 4), new(2, 6)
			]
		},
		{
			15,
			[
				new(0, 1), new(0, 3), new(0, 5), new(0, 7), new(0, 9),
				new(1, 1), new(1, 3), new(1, 5), new(1, 7), new(1, 9),
				new(2, 0), new(2, 2), new(2, 4), new(2, 6), new(2, 8)
			]
		}
	};

	/// <summary>
	/// Represents central points.
	/// </summary>
	private static readonly Dictionary<int, DoublePair> CentralPoints = new()
	{
		{ 2, (0, 1) }, { 3, (.5, 1) }, { 4, (.5, 2) }, { 5, (.5, 1) },
		{ 6, (.5, 3) }, { 7, (.5, 3) }, { 8, (.5, 4) }, { 9, (.5, 4) }, { 10, (1, 3) },
		{ 11, (1, 3) }, { 12, (1, 4) }, { 13, (1, 4) }, { 14, (1, 4) }, { 15, (1, 5) }
	};


	/// <summary>
	/// Returns the central point of the puzzle.
	/// </summary>
	/// <param name="this">The puzzle.</param>
	/// <returns>The central point.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DoublePair GetCentralPoint(this Puzzle @this) => CentralPoints[@this.Length];

	/// <summary>
	/// Returns the tube coordinate.
	/// </summary>
	/// <param name="this">The puzzle.</param>
	/// <param name="tubeIndex">The tube index.</param>
	/// <returns>The coordinate.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Coordinate GetTubeCoordinate(this Puzzle @this, int tubeIndex) => TubeCoordinates[@this.Length][tubeIndex];
}
