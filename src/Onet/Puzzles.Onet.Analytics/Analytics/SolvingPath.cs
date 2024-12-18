namespace Puzzles.Onet.Analytics;

/// <summary>
/// Represents a solving path.
/// </summary>
/// <param name="lastNode">The last node.</param>
public sealed class SolvingPath(SolvingPathNode lastNode) : HashSet<SolvingPathNode>(GetNodes(lastNode))
{
	/// <summary>
	/// Indicates the total difficulty.
	/// </summary>
	public double TotalDifficulty => this.Sum(static node => node.Difficulty);

	/// <summary>
	/// Indicates the difficulty values.
	/// </summary>
	public ReadOnlySpan<double> DifficultyValues => (from node in this select node.Difficulty).ToArray();


	/// <inheritdoc/>
	public override string ToString() => string.Join(Environment.NewLine, from node in this select node.ToString());


	/// <summary>
	/// Create a list of <see cref="SolvingPathNode"/> instance via the last node.
	/// </summary>
	/// <param name="lastNode">The last node.</param>
	/// <returns>A list of nodes.</returns>
	private static Stack<SolvingPathNode> GetNodes(SolvingPathNode lastNode)
	{
		var result = new Stack<SolvingPathNode>();
		for (var node = lastNode; node is not null; node = node.Parent)
		{
			result.Push(node);
		}
		return result;
	}
}
