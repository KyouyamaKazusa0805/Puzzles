namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a process state.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal unsafe partial struct ProcessState
{
	/// <summary>
	/// Indicates the cell states.
	/// </summary>
	public fixed byte Cells[Analyzer.MaxCells];

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
	/// (<c>currentPosition</c> is adjacent to <c>goalPosition</c>).
	/// </summary>
	public short CompletedMask { get; set; }


	/// <summary>
	/// Print the grid to the specified stream.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="writer">The writer.</param>
	public static void Print(ref readonly Grid grid, ref readonly ProcessState state, TextWriter writer)
	{
		const char blockChar = '#';

		writer.Write(blockChar);
		for (var x = 0; x < grid.Size; x++)
		{
			writer.Write(blockChar);
		}
		writer.WriteLine(blockChar);

		for (var y = (byte)0; y < grid.Size; y++)
		{
			writer.Write(blockChar);
			for (var x = (byte)0; x < grid.Size; x++)
			{
				var cell = state.Cells[Position.GetPositionFromCoordinate(x, y)];
				writer.Write(ConsoleOut.GetColorCellString(in grid, cell));
			}
			writer.WriteLine(blockChar);
		}

		writer.Write(blockChar);
		for (var x = 0; x < grid.Size; x++)
		{
			writer.Write(blockChar);
		}
		writer.WriteLine(blockChar);
	}
}
