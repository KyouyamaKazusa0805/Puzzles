namespace Puzzles.Flow.Concepts;

/// <summary>
/// Represents a flow cell.
/// </summary>
/// <param name="StartCoodinate">Indicates the start coordinate.</param>
/// <param name="EndCoordinate">Indicates the end coordinate.</param>
/// <param name="Color">Indicates the color used.</param>
public readonly record struct FlowCell(Coordinate StartCoodinate, Coordinate EndCoordinate, Color Color) :
	IEqualityOperators<FlowCell, FlowCell, bool>;
