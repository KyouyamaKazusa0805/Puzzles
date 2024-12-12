namespace Puzzles.WaterSort.Analytics;

/// <summary>
/// Provides a linked list node that describes the parent usages for a step.
/// </summary>
/// <param name="Step">Indicates the step.</param>
/// <param name="CurrentPuzzle">Indicates the current puzzle state.</param>
/// <param name="Parent">Indicates the parent node.</param>
public sealed record InvertedBruteForceNode(Step Step, Puzzle CurrentPuzzle, InvertedBruteForceNode? Parent) :
	IEqualityOperators<InvertedBruteForceNode, InvertedBruteForceNode, bool>
{
	/// <summary>
	/// Initializes a <see cref="InvertedBruteForceNode"/> instance.
	/// </summary>
	/// <param name="currentPuzzle">Indicates the current puzzle.</param>
	public InvertedBruteForceNode(Puzzle currentPuzzle) : this(default, currentPuzzle, null)
	{
	}


	/// <summary>
	/// Indicates the number of ancestors in the whole chain.
	/// </summary>
	public int AncestorsCount
	{
		get
		{
			var result = 0;
			for (var tempNode = this; tempNode is not null && tempNode.Step != default; tempNode = tempNode.Parent)
			{
				result++;
			}
			return result;
		}
	}


	/// <include
	///     file="../../../global-doc-comments.xml"
	///     path="/g/csharp9/feature[@name='records']/target[@name='method' and @cref='PrintMembers']"/>
	private bool PrintMembers(StringBuilder builder)
	{
		// Due to design of the game, two indices cannot be same in a step (start index and end index).
		// Therefore, we can append a condition (== default) to determine whether the value is uninitialized.
		builder.Append($"{nameof(Step)} = ");
		builder.Append(Step == default ? "<default>" : Step.ToString());
		builder.Append($", {nameof(Parent)} = ");
		builder.Append(
			Parent switch
			{
				{ Step: var step } => step == default ? "<default>" : step.ToString(),
				_ => "<null>"
			}
		);
		builder.Append($", {nameof(AncestorsCount)} = ");
		builder.Append(AncestorsCount);
		return true;
	}
}
