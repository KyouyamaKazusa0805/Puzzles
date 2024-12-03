namespace Puzzles.MahjongMatching.Concepts;

/// <summary>
/// Represents a tile data inside a puzzle.
/// </summary>
/// <param name="Layer">Indicates the layer.</param>
/// <param name="Tile">Indicates the tile.</param>
public readonly record struct PuzzleTile(LayerIndex Layer, LayerTile Tile) : IEqualityOperators<PuzzleTile, PuzzleTile, bool>
{
	/// <inheritdoc cref="Coordinate.Overlaps(Coordinate)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Overlaps(PuzzleTile other) => Tile.Overlaps(other.Tile);

	/// <inheritdoc cref="Coordinate.IsNextTo(Coordinate)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsNextTo(PuzzleTile other) => Tile.IsNextTo(other.Tile);

	/// <summary>
	/// Returns a <see cref="bool"/> value indicating
	/// whether the current instance is left-side with argument <paramref name="other"/>.
	/// </summary>
	/// <param name="other">The other instance to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsLeftNextTo(PuzzleTile other) => IsNextTo(other) && Tile.Coordinate.Y < other.Tile.Coordinate.Y;

	/// <summary>
	/// Returns a <see cref="bool"/> value indicating
	/// whether the current instance is left-side with argument <paramref name="other"/>.
	/// </summary>
	/// <param name="other">The other instance to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsRightNextTo(PuzzleTile other) => IsNextTo(other) && Tile.Coordinate.Y > other.Tile.Coordinate.Y;
}
