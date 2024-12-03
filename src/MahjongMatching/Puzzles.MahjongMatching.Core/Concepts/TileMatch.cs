namespace Puzzles.MahjongMatching.Concepts;

/// <summary>
/// Represents a match of a puzzle.
/// </summary>
/// <param name="Tile1">Indicates the tile 1.</param>
/// <param name="Tile2">Indicates the tile 2.</param>
public readonly record struct TileMatch(PuzzleTile Tile1, PuzzleTile Tile2) : IEqualityOperators<TileMatch, TileMatch, bool>
{
	/// <inheritdoc cref="object.ToString"/>
	/// <exception cref="InvalidOperationException">Throws when the tiles are not same.</exception>
	public override string ToString()
	{
		_ = this is (var (l1, (t1, (x1, y1))), var (l2, (t2, (x2, y2))));
		return $"{t1} & {t2} in layer {l1} & {l2}, coordinate ({x1}, {y1}) & ({x2}, {y2})";
	}
}
