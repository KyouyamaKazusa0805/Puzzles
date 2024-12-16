namespace Puzzles.Flow.Concepts;

/// <summary>
/// Represents a coordinate.
/// </summary>
/// <param name="X">Indicates the row index.</param>
/// <param name="Y">Indicates the column index.</param>
[TypeImpl(TypeImplFlags.ComparisonOperators)]
public readonly partial record struct Coordinate(int X, int Y) :
	IComparable<Coordinate>,
	IComparisonOperators<Coordinate, Coordinate, bool>,
	IEqualityOperators<Coordinate, Coordinate, bool>
{
	/// <inheritdoc/>
	public int CompareTo(Coordinate other)
	{
		if (X.CompareTo(other.X) is var r1 and not 0)
		{
			return r1;
		}
		if (Y.CompareTo(other.Y) is var r2 and not 0)
		{
			return r2;
		}
		return 0;
	}
}
