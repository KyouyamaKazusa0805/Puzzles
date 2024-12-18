namespace Puzzles.Onet.Analytics;

/// <summary>
/// Represents a solving path node.
/// </summary>
/// <param name="Match">Indicates the match.</param>
/// <param name="GridState">Indicates the current grid state.</param>
/// <param name="Difficulty">Indicates the difficulty rating.</param>
/// <param name="Parent">Indicates the parent node.</param>
public sealed record SolvingPathNode(ItemMatch? Match, Grid GridState, double Difficulty, SolvingPathNode? Parent) :
	IEqualityOperators<SolvingPathNode, SolvingPathNode, bool>
{
	/// <summary>
	/// Initializes a <see cref="SolvingPathNode"/> instance.
	/// </summary>
	/// <param name="gridState">The grid state.</param>
	public SolvingPathNode(Grid gridState) : this(null, gridState, 0, null)
	{
	}


	/// <summary>
	/// Indicates the number of ancestors.
	/// </summary>
	public int AncestorsCount
	{
		get
		{
			var result = 0;
			for (var node = this; node is not null; node = node.Parent)
			{
				result++;
			}
			return result;
		}
	}


	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] SolvingPathNode? other)
		=> other is not null && Match == other.Match && GridState == other.GridState/* && Difficulty == other.Difficulty*/;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Match, GridState/*, Difficulty*/);

	/// <include
	///     file="../../../global-doc-comments.xml"
	///     path="/g/csharp9/feature[@name='records']/target[@name='method' and @cref='PrintMembers']"/>
	private bool PrintMembers(StringBuilder builder)
	{
		if (Match is not null)
		{
			builder.Append($"{nameof(Match)} = ");
			builder.Append(Match.ToString());
			builder.Append(", ");
		}
		builder.Append($"{nameof(GridState)} = ");
		builder.Append(GridState.ToString("f"));
		if (Difficulty != 0)
		{
			builder.Append($", {nameof(Difficulty)} = ");
			builder.Append(Difficulty);
		}
		if (Parent is { Match: { } parentMatch })
		{
			builder.Append($", {nameof(Parent)} = ");
			builder.Append(Parent is not null ? parentMatch.ToString() : "<null>");
		}
		return true;
	}
}
