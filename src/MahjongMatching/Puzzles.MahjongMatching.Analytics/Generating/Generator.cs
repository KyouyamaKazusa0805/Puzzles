namespace Puzzles.MahjongMatching.Generating;

/// <summary>
/// Provides a generator.
/// </summary>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public readonly ref partial struct Generator()
{
	/// <summary>
	/// Indicates the tile distinct keys.
	/// </summary>
	private static readonly Tile[] TileDistinctKeys = [
		Tile.Bamboo(1), Tile.Bamboo(2), Tile.Bamboo(3),
		Tile.Bamboo(4), Tile.Bamboo(5), Tile.Bamboo(6),
		Tile.Bamboo(7), Tile.Bamboo(8), Tile.Bamboo(9),
		Tile.Character(1), Tile.Character(2), Tile.Character(3),
		Tile.Character(4), Tile.Character(5), Tile.Character(6),
		Tile.Character(7), Tile.Character(8), Tile.Character(9),
		Tile.Dot(1), Tile.Dot(2), Tile.Dot(3),
		Tile.Dot(4), Tile.Dot(5), Tile.Dot(6),
		Tile.Dot(7), Tile.Dot(8), Tile.Dot(9),
		Tile.Wind(1), Tile.Wind(2), Tile.Wind(3), Tile.Wind(4),
		Tile.Wrigley(1), Tile.Wrigley(2), Tile.Wrigley(3),
		Tile.Flower(1),
		Tile.Season(1)
	];

	/// <summary>
	/// Indicates flowers.
	/// </summary>
	[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>")]
	private static volatile Tile[] _flowers = [Tile.Flower(1), Tile.Flower(2), Tile.Flower(3), Tile.Flower(4)];

	/// <summary>
	/// Indicates seasons.
	/// </summary>
	[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>")]
	private static volatile Tile[] _seasons = [Tile.Season(1), Tile.Season(2), Tile.Season(3), Tile.Season(4)];


	/// <summary>
	/// Indicates the backing random number generator.
	/// </summary>
	private readonly Random _random = new();

	/// <summary>
	/// Indicates the backing analyzer.
	/// </summary>
	private readonly Analyzer _analzyer = new();


	/// <summary>
	/// Try to generate a puzzle via the pattern.
	/// </summary>
	/// <param name="pattern">
	/// The pattern; although the target type is <see cref="Puzzle"/>, it may not require any tiles predefined.
	/// </param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>The puzzle; or <see langword="null"/> if cancelled.</returns>
	public Puzzle Generate(Puzzle pattern, CancellationToken cancellationToken = default)
	{
		var elementsCount = pattern.ItemsCount;
		var tilesMarker = new BitArray(TileDistinctKeys.Length << 1);
		var tiles = new Tile[TileDistinctKeys.Length << 1];

		// Initialize tile temp array.
		for (var (i, j) = (0, 0); i < TileDistinctKeys.Length; i++, j += 2)
		{
			tiles[j] = TileDistinctKeys[i];
			tiles[j + 1] = TileDistinctKeys[i];
		}

		while (true)
		{
			// Initialize marker bits.
			tilesMarker.SetAll(true);

			// Randomly choose a list of tiles.
			var chosenTiles = new List<Tile>(elementsCount);
			for (var i = 0; i < elementsCount; i += 2)
			{
				var validTiles = getIndices(tilesMarker);
				var selectedTileIndex = validTiles[_random.Next(0, validTiles.Length)];
				var selectedTile = tiles[selectedTileIndex];

				// Double adding.
				chosenTiles.Add(selectedTile);
				chosenTiles.Add(selectedTile);

				// Remove the entry.
				tilesMarker[selectedTileIndex] = false;
			}

			// Check singleton tiles. If the chosen tile is a season tile or a flower tile,
			// we should keep the tile to be different because those two kinds of tiles are singleton tiles,
			// rather than 4 tiles in the other kinds of tiles.
			var chosenTilesIndexed = chosenTiles.Select(static (tile, i) => (Tile: tile, Index: i));
			var flowerTileIndices = chosenTilesIndexed.Where(static pair => pair.Tile.Kind == TileKind.Flower).ToArray();
			var seasonTileIndices = chosenTilesIndexed.Where(static pair => pair.Tile.Kind == TileKind.Season).ToArray();
			_random.Shuffle(_flowers);
			_random.Shuffle(_seasons);
			for (var i = 0; i < flowerTileIndices.Length; i++)
			{
				chosenTiles[flowerTileIndices[i].Index] = _flowers[i];
			}
			for (var i = 0; i < seasonTileIndices.Length; i++)
			{
				chosenTiles[seasonTileIndices[i].Index] = _seasons[i];
			}

			// Put chosen tiles into the puzzle, in random order.
			var result = pattern.Clone();
			var resultEntry = result.EnumerateTiles();
			var randomized = CollectionsMarshal.AsSpan(chosenTiles);
			_random.Shuffle(randomized);

			for (var i = 0; i < elementsCount; i++)
			{
				resultEntry[i] = randomized[i];
			}

			// Check validity.
			if (_analzyer.Analyze(result).IsSolved)
			{
				return result;
			}

			if (cancellationToken.IsCancellationRequested)
			{
				return null!;
			}
		}


		static ReadOnlySpan<int> getIndices(BitArray array)
		{
			var result = new List<int>(array.GetCardinality());
			for (var i = 0; i < array.Length; i++)
			{
				if (array[i])
				{
					result.Add(i);
				}
			}
			return result.AsSpan();
		}
	}
}
