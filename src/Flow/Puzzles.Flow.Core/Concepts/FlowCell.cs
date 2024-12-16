namespace Puzzles.Flow.Concepts;

/// <summary>
/// Represents a flow cell.
/// </summary>
/// <param name="StartCoodinate">Indicates the start coordinate.</param>
/// <param name="EndCoordinate">Indicates the end coordinate.</param>
/// <param name="Color">Indicates the color used.</param>
[TypeImpl(TypeImplFlags.ComparisonOperators)]
public readonly partial record struct FlowCell(Coordinate StartCoodinate, Coordinate EndCoordinate, Color Color) :
	IComparable<FlowCell>,
	IComparisonOperators<FlowCell, FlowCell, bool>,
	IEqualityOperators<FlowCell, FlowCell, bool>
{
	/// <inheritdoc/>
	public int CompareTo(FlowCell other)
	{
		if (Color.CompareTo(other.Color) is var r1 and not 0)
		{
			return r1;
		}
		if (StartCoodinate.CompareTo(other.StartCoodinate) is var r2 and not 0)
		{
			return r2;
		}
		if (EndCoordinate.CompareTo(other.EndCoordinate) is var r3 and not 0)
		{
			return r3;
		}
		return 0;
	}
}
