namespace Puzzles.WaterSort.Analytics;

/// <summary>
/// Represents a brute force node that describes the current state, and its children states.
/// </summary>
/// <param name="step">Indicates the step.</param>
/// <param name="currentState">Indicates the current state.</param>
[TypeImpl(TypeImplFlags.AllObjectMethods | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class BruteForceNode(
	[Property, HashCodeMember, StringMember] Step step,
	[Property, HashCodeMember, StringMember] Puzzle currentState
) :
	IEquatable<BruteForceNode>,
	IEqualityOperators<BruteForceNode, BruteForceNode, bool>
{
	/// <summary>
	/// Indicates the children.
	/// </summary>
	public IList<BruteForceNode> Children { get; internal set; } = [];

	[StringMember(nameof(Children))]
	private string ChildrenString => $"[{string.Join(", ", from child in Children select child.Step.ToString())}]";

	[EquatableMember]
	private Step StepEntry => Step;

	[EquatableMember]
	private Puzzle CurrentStateEntry => CurrentState;
}
