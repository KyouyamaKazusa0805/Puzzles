namespace Puzzles.Flow.Text;

/// <summary>
/// Provides a way to handle console output text.
/// </summary>
public static class ConsoleOut
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
	/// Clear screen in console, and set cursor position to (0, 0).
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>The fresh string.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ClearBoard(ref readonly Grid grid)
#if DISPLAY_COLOR_CONOSLE
		=> $"\e[{grid.Size + 2}A\e[{grid.Size + 2}D";
#else
		=> "\n\n";
#endif

	/// <summary>
	/// Get color name string.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="color">The color.</param>
	/// <returns>Color name string.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe string GetColorNameString(ref readonly Grid grid, int color)
	{
		ref readonly var l = ref ColorDictionary[grid.ColorIds[color]];
		return GetColorString(l.ConsoleOutColorString, l.InputChar);
	}

	/// <summary>
	/// Get color cell string.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="cell">The cell.</param>
	/// <returns>The string.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe string GetColorCellString(ref readonly Grid grid, byte cell)
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
