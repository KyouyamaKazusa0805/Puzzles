namespace Puzzles.Flow.Concepts;

/// <summary>
/// Represents a state of cell used.
/// </summary>
public enum CellState : byte
{
	/// <summary>
	/// Indicates the cell is empty (original name: a "free" cell).
	/// </summary>
	Empty = 0,

	/// <summary>
	/// Indicates the cell is used as a path node.
	/// </summary>
	Path = 1,

	/// <summary>
	/// Indicates the cell is used as start cell (original name: an "init" cell).
	/// </summary>
	Start = 2,

	/// <summary>
	/// Indicates the cell is used as end cell (original name: a "goal" cell).
	/// </summary>
	End = 3
}
