namespace Puzzles.Flow.Concepts.Primitives;

/// <summary>
/// Represents a type of cell used.
/// </summary>
public enum CellType : byte
{
	/// <summary>
	/// Indicates the cell is free (unused).
	/// </summary>
	Free = 0,

	/// <summary>
	/// Indicates the cell is used as a path node.
	/// </summary>
	Path = 1,

	/// <summary>
	/// Indicates the cell is used as start cell.
	/// </summary>
	Init = 2,

	/// <summary>
	/// Indicates the cell is used as end cell.
	/// </summary>
	Goal = 3
}
