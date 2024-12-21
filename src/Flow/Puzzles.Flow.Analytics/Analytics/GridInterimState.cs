namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a grid interim state.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal struct GridInterimState
{
	/// <summary>
	/// Indicates the cell states.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public unsafe fixed Cell Cells[Analyzer.MaxGridCellsCount];

	/// <summary>
	/// Indicates the positions.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public unsafe fixed Position Positions[Analyzer.MaxSupportedColorsCount];


	/// <summary>
	/// Indicates the number of free cells.
	/// </summary>
	public Cell FreeCellsCount { get; set; }

	/// <summary>
	/// Indicates the last color.
	/// </summary>
	public Color LastColor { get; set; }

	/// <summary>
	/// Indicates the bit flags indicating whether each color has been completed or not
	/// (<c>currentPosition</c> is adjacent to <c>goalPosition</c>).
	/// </summary>
	public ColorMask CompletedMask { get; set; }

#if DEBUG
	/// <summary>
	/// Provides <see langword="this"/> pointer.
	/// </summary>
	private readonly unsafe GridInterimState* ThisPointer => (GridInterimState*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));

	private readonly unsafe ReadOnlySpan<Position> PositionsSpan => new(ThisPointer->Positions, Analyzer.MaxSupportedColorsCount);

	private readonly unsafe ReadOnlySpan<Cell> CellsSpan => new(ThisPointer->Cells, Analyzer.MaxGridCellsCount);
#endif
}
