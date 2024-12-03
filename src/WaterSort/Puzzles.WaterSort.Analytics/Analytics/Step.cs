namespace WaterSort.Analytics;

/// <summary>
/// Represents a move step.
/// </summary>
/// <param name="StartTubeIndex">Indicates the start tube index.</param>
/// <param name="EndTubeIndex">Indicates the end tube index.</param>
public readonly record struct Step(int StartTubeIndex, int EndTubeIndex) : IEqualityOperators<Step, Step, bool>
{
	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => $"[{StartTubeIndex}] -> [{EndTubeIndex}]";
}
