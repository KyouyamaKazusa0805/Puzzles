namespace Puzzles.Flow.Analytics;

/// <summary>
/// Provides a way to operate with cells.
/// </summary>
internal static class CellConverter
{
	/// <summary>
	/// Create a cell via type, color and direction.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="color">The color.</param>
	/// <param name="direction">The direction.</param>
	/// <returns>The cell.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Cell Create(CellState type, Color color, Direction direction)
		=> (Cell)((color & 0xF) << 4 | (((Cell)direction & 0x3) << 2) | (Cell)type & 0x3);

	/// <summary>
	/// Get the type from a cell value.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <returns>The value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellState GetTypeFromCell(Cell cell) => (CellState)(cell & 0x3);

	/// <summary>
	/// Get the direction from the cell.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <returns>The direction.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Direction GetDirectionFromCell(Cell cell) => (Direction)(cell >> 2 & 0x3);

	/// <summary>
	/// Get the color from the cell.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <returns>The color.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Color GetCellColor(Cell cell) => (Color)(cell >> 4 & 0xF);
}
