namespace Puzzles.Matching.Concepts;

/// <summary>
/// Represents a coordinate.
/// </summary>
/// <param name="X">Indicates the row index.</param>
/// <param name="Y">Indicates the column index.</param>
public readonly record struct Coordinate(int X, int Y) : IEqualityOperators<Coordinate, Coordinate, bool>;
