namespace Puzzles.Flow.Concepts;

/// <summary>
/// Represents a grid.
/// </summary>
/// <param name="size">Indicates the size of the board.</param>
/// <param name="colorsCount">Indicates the number of colors used.</param>
/// <param name="isUserOrdered">Indicates whether the ordering is specified by user.</param>
[StructLayout(LayoutKind.Auto)]
public unsafe partial struct Grid(
	[Property] int size,
	[Property(Setter = PropertySetters.Set)] byte colorsCount,
	[Property] bool isUserOrdered
)
{
	/// <summary>
	/// Indicates the init positions (start positions).
	/// </summary>
	public fixed byte InitPositions[MaxColors];

	/// <summary>
	/// Indicates the goal positions (end positions).
	/// </summary>
	public fixed byte GoalPositions[MaxColors];

	/// <summary>
	/// Indicates the color table looking up color ID.
	/// </summary>
	public fixed byte ColorTable[1 << 7];

	/// <summary>
	/// Indicates the index values of color lookup table of codes.
	/// </summary>
	public fixed int ColorIds[MaxColors];

	/// <summary>
	/// Indicates the color ordering.
	/// </summary>
	public fixed byte ColorOrder[MaxColors];


	/// <summary>
	/// Get wall distance from the specified coordinate values <paramref name="x"/> and <paramref name="y"/>.
	/// </summary>
	/// <param name="x">The x value.</param>
	/// <param name="y">The y value.</param>
	/// <returns>The distance.</returns>
	public readonly int GetWallDistance(int x, int y)
	{
		var p = (stackalloc[] { x, y });
		var d = (stackalloc int[2]);
		for (var i = 0; i < 2; i++)
		{
			var d0 = p[i];
			var d1 = Size - 1 - p[i];
			d[i] = Math.Min(d0, d1);
		}
		return Math.Min(d[0], d[1]);
	}

	/// <summary>
	/// Get wall distance from the specified coordinate value <paramref name="pos"/>.
	/// </summary>
	/// <param name="pos">The position.</param>
	/// <returns>The distance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly int GetWallDistance(byte pos)
	{
		Position.GetCoordinateFromPosition(pos, out var x, out var y);
		return GetWallDistance(x, y);
	}
}
