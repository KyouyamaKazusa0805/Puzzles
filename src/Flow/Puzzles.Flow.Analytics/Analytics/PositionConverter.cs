namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents an easy way to convert values to position.
/// </summary>
internal static class PositionConverter
{
	/// <summary>
	/// Get position index from coordinate values <paramref name="x"/> and <paramref name="y"/>.
	/// </summary>
	/// <param name="x">The x value.</param>
	/// <param name="y">The y value.</param>
	/// <returns>The position index.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Position GetPositionFromCoordinate(Position x, Position y) => (Position)((y & 0xF) << 4 | x & 0xF);

	/// <summary>
	/// Get position offset, or return <see cref="Analyzer.InvalidPosition"/> if bounds.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="x">The x value.</param>
	/// <param name="y">The y value.</param>
	/// <param name="direction">The direction.</param>
	/// <returns>The offset value.</returns>
	/// <seealso cref="Analyzer.InvalidPosition"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Position GetOffsetPosition(ref readonly GridAnalyticsInfo grid, int x, int y, Direction direction)
	{
		var delta = direction.GetDirectionDelta();
		var offsetX = (byte)(x + delta[0]);
		var offsetY = (byte)(y + delta[1]);
		return IsCoordinateValid(in grid, offsetX, offsetY) ? GetPositionFromCoordinate(offsetX, offsetY) : Analyzer.InvalidPosition;
	}

	/// <summary>
	/// Get position from another position with advanced direction.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="position">The position.</param>
	/// <param name="direction">The direction.</param>
	/// <returns>The position result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Position GetOffsetPosition(ref readonly GridAnalyticsInfo grid, Position position, Direction direction)
	{
		GetCoordinateFromPosition(position, out var x, out var y);
		return GetOffsetPosition(in grid, x, y, direction);
	}

	/// <summary>
	/// Determine whether the coordinate is valid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="x">The x value.</param>
	/// <param name="y">The y value.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsCoordinateValid(ref readonly GridAnalyticsInfo grid, int x, int y)
		=> x >= 0 && x < grid.Size && y >= 0 && y < grid.Size;

	/// <summary>
	/// Gets coordinate values from a position index.
	/// </summary>
	/// <param name="position">The position index.</param>
	/// <param name="x">The x value.</param>
	/// <param name="y">The y value.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetCoordinateFromPosition(Position position, out int x, out int y)
		=> (x, y) = (position & 0xF, position >> 4 & 0xF);
}
