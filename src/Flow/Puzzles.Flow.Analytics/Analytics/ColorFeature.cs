namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a type that is used for sorting colors if <see cref="Analyzer.ReorderColors"/> is set <see langword="true"/>.
/// </summary>
/// <seealso cref="Analyzer.ReorderColors"/>
internal struct ColorFeature
{
	/// <summary>
	/// Indicates the index reordered.
	/// </summary>
	public int Index;

	/// <summary>
	/// Indicates the index which is user-specified.
	/// </summary>
	public int UserIndex;

	/// <summary>
	/// Indicates the minimal distance.
	/// </summary>
	public int MinDistance;

	/// <summary>
	/// Indicates the wall distance.
	/// </summary>
	public (int, int) WallDistance;
}
