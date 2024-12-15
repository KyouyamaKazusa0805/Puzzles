namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a grid interim state.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal unsafe struct GridInterimState
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

#if DEBUG
	/// <summary>
	/// Provides <see langword="this"/> pointer.
	/// </summary>
	private readonly GridInterimState* ThisPointer => (GridInterimState*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));

	private readonly ReadOnlySpan<byte> PositionsSpan => new(ThisPointer->Positions, MaxColors);

	private readonly ReadOnlySpan<byte> CellsSpan => new(ThisPointer->Cells, Analyzer.MaxCells);
#endif


	/// <summary>
	/// Print the grid to the specified stream.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="writer">The writer.</param>
	public static void Print(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state, TextWriter writer)
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

/// <summary>
/// Provides a way to handle console output text.
/// </summary>
file static unsafe class ConsoleOut
{
	/// <summary>
	/// Get color string.
	/// </summary>
	/// <param name="colorString">The color console out string.</param>
	/// <param name="inputChar">The input character.</param>
	/// <returns>The string.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetColorString(string colorString, char inputChar)
#if DISPLAY_COLOR_CONOSLE
		=> $"\e[30;{colorString}m{inputChar}\e[0m";
#else
		=> inputChar.ToString();
#endif

	/// <summary>
	/// Get color cell string.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="cell">The cell.</param>
	/// <returns>The string.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetColorCellString(ref readonly GridAnalyticsInfo grid, byte cell)
	{
		var type = Cell.GetTypeFromCell(cell);
		var color = Cell.GetCellColor(cell);
		var direction = Cell.GetDirectionFromCell(cell);
		ref readonly var l = ref ColorDictionary[grid.ColorIds[color]];
		return type switch
		{
			CellType.Free => " ",
			CellType.Path => GetColorString(l.ConsoleOutColorString, direction.GetArrow()),
			_ => GetColorString(l.ConsoleOutColorString, type == CellType.Init ? 'o' : 'O')
		};
	}
}
