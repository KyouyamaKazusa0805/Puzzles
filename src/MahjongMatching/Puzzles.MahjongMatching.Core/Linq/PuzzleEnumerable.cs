namespace Puzzles.MahjongMatching.Linq;

/// <summary>
/// Provides with extension methods on <see cref="Puzzle"/>.
/// </summary>
/// <seealso cref="Puzzle"/>
public static class PuzzleEnumerable
{
	/// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
	public static ReadOnlySpan<TResult> Select<TResult>(this Puzzle @this, Func<Layer, TResult> selector)
	{
		var result = new List<TResult>();
		foreach (var element in @this)
		{
			result.Add(selector(element));
		}
		return result.AsSpan();
	}
}
