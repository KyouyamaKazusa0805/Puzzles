namespace Puzzles.MahjongMatching.Concepts.Formatting;

/// <summary>
/// Represents a puzzle format information.
/// </summary>
public sealed partial class PuzzleFormatInfo : IFormatProvider
{
	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public object? GetFormat(Type? formatType) => formatType == typeof(PuzzleFormatInfo) ? this : null;

	/// <summary>
	/// Converts the current puzzle into a string.
	/// </summary>
	/// <param name="puzzle">The puzzle.</param>
	/// <returns>The string value.</returns>
	internal string FormatCore(Puzzle puzzle)
	{
		var stack = new Stack<Layer>(puzzle);
		var coordinatesSb = new StringBuilder();
		var tilesSb = new StringBuilder();
		var layerIndex = 0;
		foreach (var layer in stack)
		{
			coordinatesSb.Append(layerIndex);
			var tileGroups = from tile in layer.AsSpan() group tile by tile.Coordinate.X;
			foreach (var (index, tileGroup) in tileGroups.Index())
			{
				var x = tileGroup.Key;
				coordinatesSb.Append(getTileChar(x));
				foreach (var (groupIndex, (tile, (_, y))) in tileGroup.AsSpan().Index())
				{
					coordinatesSb.Append(getTileChar(y));
					tilesSb.Append(getTileChar(getTileId(tile)));
					if (groupIndex != tileGroup.Length - 1)
					{
						coordinatesSb.Append(',');
					}
				}
				if (index != tileGroups.Length - 1)
				{
					coordinatesSb.Append('.');
				}
			}
			if (layerIndex != stack.Count - 1)
			{
				coordinatesSb.Append(';');
			}
			layerIndex++;
		}
		return $"{coordinatesSb}:{tilesSb}";


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static char getTileChar(int id)
			=> id switch { <= 9 => (char)(id + '0'), < 36 => (char)(id - 10 + 'A'), _ => (char)(id - 36 + 'a') };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int getTileId(Tile tile)
			=> tile switch
			{
				{ Kind: TileKind.Dot, Rank: var rank } => rank - 1,
				{ Kind: TileKind.Bamboo, Rank: var rank } => rank + 8,
				{ Kind: TileKind.Character, Rank: var rank } => rank + 17,
				{ Kind: TileKind.Wind, Rank: var rank } => ((WindKind)rank - 1) switch
				{
					WindKind.East => 28,
					WindKind.South => 29,
					WindKind.West => 30,
					WindKind.North => 31
				},
				{ Kind: TileKind.Wrigley, Rank: var rank } => ((WrigleyKind)rank - 1) switch
				{
					WrigleyKind.Red => 32,
					WrigleyKind.Green => 33,
					WrigleyKind.White => 34,
				},
				{ Kind: TileKind.Flower } => 35,
				{ Kind: TileKind.Season } => 36
			};
	}

	/// <summary>
	/// Parses the specified string into a puzzle.
	/// </summary>
	/// <param name="str">The string.</param>
	/// <returns>The puzzle.</returns>
	internal Puzzle ParseCore(string str)
	{
		var parts = str.Split(':');
		if (parts is not [var coordinatesDataPart, var tilesDataPart])
		{
			throw new FormatException();
		}

		var layers = new Stack<Layer>();
		var layerParts = coordinatesDataPart.Split(';');
		for (var (i, j) = (0, 0); i < layerParts.Length; i++)
		{
			var layer = new Layer();
			var layerPart = layerParts[i][1..];
			var rowParts = layerPart.Split('.');
			foreach (var rowPart in rowParts)
			{
				var x = toFactor(rowPart[0]);
				foreach (var columnString in rowPart[1..].Split(','))
				{
					var y = toFactor(columnString[0]);
					layer.Add(new(toTile(toFactor(tilesDataPart[j++])), new(x, y)));
				}
			}
			layers.Push(layer);
		}
		return [.. layers];


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int toFactor(char ch)
			=> ch switch
			{
				>= '0' and <= '9' => ch - '0',
				>= 'A' and <= 'Z' => ch - 'A' + 10,
				>= 'a' and <= 'z' => ch - 'a' + 36,
				_ => throw new FormatException()
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static Tile toTile(int cardId)
			=> cardId switch
			{
				< 9 => Tile.Dot(cardId + 1),
				< 18 => Tile.Bamboo(cardId - 8),
				< 27 => Tile.Character(cardId - 17),
				28 => Tile.Wind(WindKind.East),
				29 => Tile.Wind(WindKind.South),
				30 => Tile.Wind(WindKind.West),
				31 => Tile.Wind(WindKind.North),
				32 => Tile.Wrigley(WrigleyKind.Red),
				33 => Tile.Wrigley(WrigleyKind.Green),
				34 => Tile.Wrigley(WrigleyKind.White),
				35 => Tile.Flower(FlowerKind.Plum),
				36 => Tile.Season(SeasonKind.Spring),
				_ => throw new ArgumentOutOfRangeException(nameof(cardId))
			};
	}
}
