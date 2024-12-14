namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a type that is used for sorting colors if <see cref="Analyzer.ReorderColors"/> is set <see langword="true"/>.
/// </summary>
/// <seealso cref="Analyzer.ReorderColors"/>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_ToString | TypeImplFlags.AllEqualityComparisonOperators, IsLargeStructure = true)]
internal partial struct ColorFeature :
	IComparable<ColorFeature>,
	IComparisonOperators<ColorFeature, ColorFeature, bool>,
	IEquatable<ColorFeature>
{
	/// <summary>
	/// Indicates the index reordered.
	/// </summary>
	[StringMember]
	public byte Index;

	/// <summary>
	/// Indicates the index which is user-specified.
	/// </summary>
	[StringMember]
	public int UserIndex;

	/// <summary>
	/// Indicates the minimal distance.
	/// </summary>
	[StringMember]
	public int MinDistance;

	/// <summary>
	/// Indicates the wall distance.
	/// </summary>
	[StringMember]
	public (int First, int Second) WallDistance;


	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public readonly bool Equals(ref readonly ColorFeature other) => Compare(this, other) == 0;

	/// <inheritdoc/>
	public override readonly int GetHashCode()
		=> HashCode.Combine(UserIndex, MinDistance, WallDistance.First, WallDistance.Second);

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	public readonly int CompareTo(ref readonly ColorFeature other) => Compare(this, other);

	/// <inheritdoc/>
	readonly bool IEquatable<ColorFeature>.Equals(ColorFeature other) => Compare(this, other) == 0;

	/// <inheritdoc/>
	readonly int IComparable<ColorFeature>.CompareTo(ColorFeature other) => Compare(this, other);


	/// <summary>
	/// Compares two <see cref="ColorFeature"/> instances.
	/// </summary>
	/// <param name="left">The first element to be compared.</param>
	/// <param name="right">The second element to be compared.</param>
	/// <returns>An <see cref="int"/> indicating the result.</returns>
	public static int Compare(ColorFeature left, ColorFeature right)
	{
		if (left.UserIndex.CompareTo(right.UserIndex) is var r1 and not 0)
		{
			return r1;
		}
		if (left.WallDistance.First.CompareTo(right.WallDistance.First) is var r2 and not 0)
		{
			return r2;
		}
		if (left.WallDistance.Second.CompareTo(right.WallDistance.Second) is var r3 and not 0)
		{
			return -r3;
		}
		return -left.MinDistance.CompareTo(right.MinDistance);
	}
}
