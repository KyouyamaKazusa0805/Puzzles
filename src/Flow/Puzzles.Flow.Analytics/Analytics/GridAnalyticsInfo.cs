namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a grid.
/// </summary>
/// <param name="size">Indicates the size of the board.</param>
/// <param name="colorsCount">Indicates the number of colors used.</param>
/// <param name="isUserOrdered">Indicates whether the ordering is specified by user.</param>
[StructLayout(LayoutKind.Auto)]
internal unsafe ref partial struct GridAnalyticsInfo(
	[Property(Setter = PropertySetters.Set)] int size,
	[Property(Setter = PropertySetters.Set)] Color colorsCount,
	[Property(Setter = PropertySetters.Set)] bool isUserOrdered
) : IBoard
{
	/// <summary>
	/// Indicates the index values of color lookup table of codes.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public fixed int ColorIds[Analyzer.MaxSupportedColorsCount];

	/// <summary>
	/// Indicates the init positions (start positions).
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public fixed Position InitPositions[Analyzer.MaxSupportedColorsCount];

	/// <summary>
	/// Indicates the goal positions (end positions).
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public fixed Position GoalPositions[Analyzer.MaxSupportedColorsCount];

	/// <summary>
	/// Indicates the color table looking up color ID.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public fixed Color ColorTable[1 << 7];

	/// <summary>
	/// Indicates the color ordering.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public fixed Color ColorOrder[Analyzer.MaxSupportedColorsCount];


	/// <inheritdoc/>
	readonly int IBoard.Rows => Size;

	/// <inheritdoc/>
	readonly int IBoard.Columns => Size;

#if DEBUG
	/// <summary>
	/// Provides <see langword="this"/> pointer.
	/// </summary>
	private readonly GridAnalyticsInfo* ThisPointer => (GridAnalyticsInfo*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));

	private readonly ReadOnlySpan<int> ColorIdsSpan => new(ThisPointer->ColorIds, Analyzer.MaxSupportedColorsCount);

	private readonly ReadOnlySpan<Position> InitPositionsSpan => new(ThisPointer->InitPositions, Analyzer.MaxSupportedColorsCount);

	private readonly ReadOnlySpan<Position> GoalPositionsSpan => new(ThisPointer->GoalPositions, Analyzer.MaxSupportedColorsCount);

	private readonly ReadOnlySpan<Color> ColorTableSpan => new(ThisPointer->ColorTable, 1 << 7);

	private readonly ReadOnlySpan<Color> ColorOrderSpan => new(ThisPointer->ColorOrder, Analyzer.MaxSupportedColorsCount);
#endif


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
			d[i] = Min(d0, d1);
		}
		return Min(d[0], d[1]);
	}

	/// <summary>
	/// Get wall distance from the specified coordinate value <paramref name="pos"/>.
	/// </summary>
	/// <param name="pos">The position.</param>
	/// <returns>The distance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly int GetWallDistance(byte pos)
	{
		PositionConverter.GetCoordinateFromPosition(pos, out var x, out var y);
		return GetWallDistance(x, y);
	}
}
