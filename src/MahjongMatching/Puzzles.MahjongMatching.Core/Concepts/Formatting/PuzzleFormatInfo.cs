namespace Puzzles.MahjongMatching.Concepts.Formatting;

/// <summary>
/// Represents a puzzle format information.
/// </summary>
public sealed class PuzzleFormatInfo : IFormatProvider
{
	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public object? GetFormat(Type? formatType) => formatType == typeof(PuzzleFormatInfo) ? this : null;


	/// <summary>
	/// Converts the current instance into a <see cref="Tile"/> via ID.
	/// </summary>
	/// <param name="cardId">The card ID.</param>
	/// <returns>The tile.</returns>
	/// <exception cref="InvalidOperationException">Throws when the argument is greater than 36.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Tile ToTile(int cardId)
		=> cardId switch
		{
			< 9 => Tile.Bamboo(cardId),
			< 18 => Tile.Character(cardId - 9),
			< 27 => Tile.Dot(cardId - 18),
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
