namespace Puzzles.Hamiltonian.Concepts;

/// <summary>
/// Represents a direction.
/// </summary>
[Flags]
public enum Direction : byte
{
	/// <summary>
	/// Indicates the direction is none.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates the direction is up.
	/// </summary>
	Up = 1 << 0,

	/// <summary>
	/// Indicates the direction is down.
	/// </summary>
	Down = 1 << 1,

	/// <summary>
	/// Indicates the direction is left.
	/// </summary>
	Left = 1 << 2,

	/// <summary>
	/// Indicates the direction is right.
	/// </summary>
	Right = 1 << 3
}
