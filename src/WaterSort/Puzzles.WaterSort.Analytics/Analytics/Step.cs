namespace Puzzles.WaterSort.Analytics;

/// <summary>
/// Represents a move step.
/// </summary>
/// <param name="Start">Indicates the start tube index.</param>
/// <param name="End">Indicates the end tube index.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Equatable)]
public sealed partial record Step([property: HashCodeMember] int Start, [property: HashCodeMember] int End) :
	IEqualityOperators<Step, Step, bool>
{
	/// <summary>
	/// Indicates the difficulty.
	/// </summary>
	public double Difficulty { get; internal set; }

	[EquatableMember]
	private int StartTubeIndexEntry => Start;

	[EquatableMember]
	private int EndTubeIndexEntry => End;


	/// <include file="../../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out int startTubeIndex, out int endTubeIndex, out double difficulty)
		=> ((startTubeIndex, endTubeIndex), difficulty) = (this, Difficulty);

	/// <summary>
	/// Indicates whether the step is valid on applying to the specified puzzle.
	/// </summary>
	/// <param name="puzzle">The puzzle.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool IsValidFor(Puzzle puzzle)
		=> puzzle[Start].TopColor != Color.MaxValue && !(puzzle[Start].IsMonocolored && puzzle[End].IsEmpty);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => $"[{Start}] -> [{End}]";


	/// <summary>
	/// Negates the step.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The negated result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Step operator ~(Step value) => new(value.End, value.Start);
}
