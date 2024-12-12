namespace Puzzles.WaterSort.Analytics;

/// <summary>
/// Represents a brute force node that describes the current state, and its children states.
/// </summary>
/// <param name="step">Indicates the step.</param>
/// <param name="currentState">Indicates the current state.</param>
[TypeImpl(TypeImplFlags.Object_ToString)]
public sealed partial class BruteForceNode([Property, StringMember] Step step, [Property, StringMember] Puzzle currentState)
{
	/// <summary>
	/// Indicates the children.
	/// </summary>
	public IList<BruteForceNode> Children { get; internal set; } = [];

	[StringMember(nameof(Children))]
	private string ChildrenString => $"[{string.Join(", ", from child in Children select child.Step.ToString())}]";
}
