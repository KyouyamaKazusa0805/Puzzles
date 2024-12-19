namespace Puzzles.Flow.Analytics;

/// <summary>
/// Provides with extension methods on <see cref="Direction"/>.
/// </summary>
/// <seealso cref="Direction"/>
public static class DirectionExtensions
{
	/// <summary>
	/// Gets the direction delta value that represents the coordinate advancing.
	/// </summary>
	/// <param name="this">The direction.</param>
	/// <returns>The delta array.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is not defined.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<int> GetDirectionDelta(this Direction @this)
		=> @this switch
		{
			Direction.Up => [0, -1, -16],
			Direction.Down => [0, 1, 16],
			Direction.Left => [-1, 0, -1],
			Direction.Right => [1, 0, 1],
			_ => throw new ArgumentOutOfRangeException(nameof(@this))
		};
}
