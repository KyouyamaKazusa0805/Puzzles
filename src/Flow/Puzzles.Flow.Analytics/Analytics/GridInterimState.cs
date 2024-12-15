namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a grid interim state.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal unsafe partial struct GridInterimState
{
	/// <summary>
	/// Indicates the cell states.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public fixed byte Cells[Analyzer.MaxCells];

	/// <summary>
	/// Indicates the positions.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public fixed byte Positions[MaxColors];


	/// <summary>
	/// Indicates the number of free cells.
	/// </summary>
	public byte FreeCellsCount { get; set; }

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
	/// Provides <see langword="this"/> pointer.
	/// </summary>
	private readonly GridInterimState* ThisPointer => (GridInterimState*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));

	private readonly ReadOnlySpan<byte> PositionsSpan => new(ThisPointer->Positions, MaxColors);

	private readonly ReadOnlySpan<byte> CellsSpan => new(ThisPointer->Cells, Analyzer.MaxCells);


	/// <summary>
	/// Print the grid to the specified stream.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="writer">The writer.</param>
	public static void Print(ref readonly Grid grid, ref readonly GridInterimState state, TextWriter writer)
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
