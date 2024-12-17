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
	public fixed byte Positions[Analyzer.MaxColors];

	/// <summary>
	/// Indicates the color lookup dictionary.
	/// </summary>
	internal static readonly ColorLookup[] ColorDictionary = [
		new('0', "101", "ff0000", "723939"), // red
		new('1', "104", "0000ff", "393972"), // blue
		new('2', "103", "eeee00", "6e6e39"), // yellow
		new('3', "42", "008100", "395539"), // green
		new('4', "43", "ff8000", "725539"), // orange
		new('5', "106", "00ffff", "397272"), // cyan
		new('6', "105", "ff00ff", "723972"), // magenta
		new('7', "41", "a52a2a", "5f4242"), // maroon
		new('8', "45", "800080", "553955"), // purple
		new('9', "100", "a6a6a6", "5f5e5f"), // gray
		new('A', "107", "ffffff", "727272"), // white
		new('B', "102", "00ff00", "397239"), // bright green
		new('C', "47", "bdb76b", "646251"), // tan
		new('D', "44", "00008b", "393958"), // dark blue
		new('E', "46", "008180", "395555"), // dark cyan
		new('F', "35", "ff1493", "72415a") // pink?
	];


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

	private readonly ReadOnlySpan<byte> PositionsSpan => new(ThisPointer->Positions, Analyzer.MaxColors);

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

	/// <summary>
	/// Returns the color index of dictionary table <see cref="ColorDictionary"/>.
	/// </summary>
	/// <param name="c">The input character.</param>
	/// <returns>The index.</returns>
	/// <seealso cref="ColorDictionary"/>
	internal static int GetColor(char c)
	{
		c = char.ToUpper(c);
		for (var i = 0; i < Analyzer.MaxColors; i++)
		{
			if (ColorDictionary[i].InputChar == c)
			{
				return i;
			}
		}
		return -1;
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
		ref readonly var l = ref GridInterimState.ColorDictionary[grid.ColorIds[color]];
		return type switch
		{
			CellType.Free => " ",
			CellType.Path => GetColorString(l.ConsoleOutColorString, direction.GetArrow()),
			_ => GetColorString(l.ConsoleOutColorString, type == CellType.Init ? 'o' : 'O')
		};
	}
}
