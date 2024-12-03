namespace Puzzles.MahjongMatching.Concepts.Primitives;

/// <summary>
/// Represents a mahjong tile kind.
/// </summary>
[Flags]
public enum TileKind : byte
{
	/// <summary>
	/// Indicates the placeholder.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates the kind is bamboo.
	/// </summary>
	Bamboo = 1 << 0,

	/// <summary>
	/// Indicates the kind is character.
	/// </summary>
	Character = 1 << 1,

	/// <summary>
	/// Indicates the kind is dot.
	/// </summary>
	Dot = 1 << 2,

	/// <summary>
	/// Indicates the kind is wind.
	/// </summary>
	Wind = 1 << 3,

	/// <summary>
	/// Indicates the kind is wrigley (dragon).
	/// </summary>
	Wrigley = 1 << 4,

	/// <summary>
	/// Indicates the kind is flower.
	/// </summary>
	Flower = 1 << 5,

	/// <summary>
	/// Indicates the kind is season.
	/// </summary>
	Season = 1 << 6
}
