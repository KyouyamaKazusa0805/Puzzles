namespace WaterSort.Analytics;

/// <summary>
/// Provides with extension methods on <see cref="Puzzle"/>.
/// </summary>
/// <seealso cref="Puzzle"/>
public static class PuzzleExtensions
{
	/// <summary>
	/// Applies the current step.
	/// </summary>
	/// <param name="puzzle">The puzzle.</param>
	/// <param name="step">The step to be applied.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Apply(this Puzzle puzzle, Step step)
		=> puzzle[step.EndTubeIndex].Push(puzzle[step.StartTubeIndex].Pop(out var count), count);
}
