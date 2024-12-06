namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a progress state.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public unsafe partial struct ProgressState
{
	/// <summary>
	/// Indicates the cell states.
	/// </summary>
	public fixed byte Cells[MaxCells];

	/// <summary>
	/// Indicates the positions.
	/// </summary>
	public fixed byte Positions[MaxColors];


	/// <summary>
	/// Indicates the number of freed cells.
	/// </summary>
	public byte FreedCellsCount { get; set; }

	/// <summary>
	/// Indicates the last color.
	/// </summary>
	public byte LastColor { get; set; }

	/// <summary>
	/// Indicates the bit flags indicating whether each color has been completed or not
	/// (cur_pos is adjacent to goal_pos).
	/// </summary>
	public short CompletedMask { get; set; }
}
