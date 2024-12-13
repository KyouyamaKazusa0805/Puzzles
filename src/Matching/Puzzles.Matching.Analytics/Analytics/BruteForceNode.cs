namespace Puzzles.Matching.Analytics;

/// <summary>
/// Represents a brute force node that describes the current state, and its children states.
/// </summary>
/// <param name="step">Indicates the step.</param>
/// <param name="currentState">Indicates the current state.</param>
[TypeImpl(TypeImplFlags.AllObjectMethods | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class BruteForceNode(
	[Property, HashCodeMember, StringMember] ItemMatch? step,
	[Property, HashCodeMember, StringMember] Grid currentState
) :
	IEquatable<BruteForceNode>,
	IEqualityOperators<BruteForceNode, BruteForceNode, bool>
{
	/// <summary>
	/// Indicates the children.
	/// </summary>
	public IList<BruteForceNode> Children { get; } = [];

	[StringMember(nameof(Children))]
	private string ChildrenString
	{
		get
		{
			var str = string.Join(", ", from child in Children select child.Step?.ToString() ?? "<null>");
			return $"[{str}]";
		}
	}

	[EquatableMember]
	private ItemMatch? StepEntry => Step;

	[EquatableMember]
	private Grid CurrentStateEntry => CurrentState;
}
