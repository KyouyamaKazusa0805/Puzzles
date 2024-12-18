namespace Puzzles.Flow.Concepts;

/// <summary>
/// Represents a flow, including its start and end position, and its color.
/// </summary>
/// <param name="StartCoodinate">Indicates the start coordinate.</param>
/// <param name="EndCoordinate">Indicates the end coordinate.</param>
/// <param name="Color">Indicates the color used.</param>
[TypeImpl(TypeImplFlags.ComparisonOperators)]
public readonly partial record struct FlowPosition(Coordinate StartCoodinate, Coordinate EndCoordinate, Color Color) :
	IComparable<FlowPosition>,
	IComparisonOperators<FlowPosition, FlowPosition, bool>,
	IEqualityOperators<FlowPosition, FlowPosition, bool>
{
	/// <inheritdoc/>
	public int CompareTo(FlowPosition other)
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
