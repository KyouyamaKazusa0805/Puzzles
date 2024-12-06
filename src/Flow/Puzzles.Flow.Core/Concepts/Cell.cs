namespace Puzzles.Flow.Concepts;

/// <summary>
/// Provides a way to operate with cells.
/// </summary>
public static class Cell
{
	/// <summary>
	/// Create a cell via type, color and direction.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="color">The color.</param>
	/// <param name="direction">The direction.</param>
	/// <returns>The cell.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte CellCreate(byte type, byte color, Direction direction)
		=> (byte)(color & 0xF << 4 | (byte)direction & 0x3 << 2 | type & 0x3);

	/// <summary>
	/// Get the color from the cell.
	/// </summary>
	/// <param name="c">The cell.</param>
	/// <returns>The color.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte GetCellColor(byte c) => (byte)((c >> 4) & 0xF);

	/// <summary>
	/// Get the type from a cell value.
	/// </summary>
	/// <param name="c">The cell.</param>
	/// <returns>The value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellType GetTypeFromCell(byte c) => (CellType)(c & 0x3);

	/// <summary>
	/// Get the direction from the cell.
	/// </summary>
	/// <param name="c">The cell.</param>
	/// <returns>The direction.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Direction GetDirectionFromCell(byte c) => (Direction)((c >> 2) & 0x3);
}
