namespace Puzzles.WaterSort.Analytics;

/// <summary>
/// Represents a move step.
/// </summary>
/// <param name="startTubeIndex">Indicates the start tube index.</param>
/// <param name="endTubeIndex">Indicates the end tube index.</param>
/// <param name="difficulty">Indicates the difficulty rating.</param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class Step(
	[Property, HashCodeMember] int startTubeIndex,
	[Property, HashCodeMember] int endTubeIndex,
	[Property] int difficulty
) :
	IEquatable<Step>,
	IEqualityOperators<Step, Step, bool>
{
	[EquatableMember]
	private int StartTubeIndexEntry => StartTubeIndex;

	[EquatableMember]
	private int EndTubeIndexEntry => EndTubeIndex;


	/// <include file="../../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out int startTubeIndex, out int endTubeIndex)
		=> (startTubeIndex, endTubeIndex) = (StartTubeIndex, EndTubeIndex);

	/// <include file="../../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out int startTubeIndex, out int endTubeIndex, out int difficulty)
		=> ((startTubeIndex, endTubeIndex), difficulty) = (this, Difficulty);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => $"[{StartTubeIndex}] -> [{EndTubeIndex}]";
}
