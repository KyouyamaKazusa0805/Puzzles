namespace Puzzles.MahjongMatching.Linq;

/// <summary>
/// Provides with extension methods on <see cref="Layer"/>.
/// </summary>
/// <seealso cref="Layer"/>
public static class LayerEnumerable
{
	/// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
	public static ReadOnlySpan<TResult> Select<TResult>(this Layer @this, Func<LayerTile, TResult> selector)
	{
		var result = new List<TResult>();
		foreach (var element in @this)
		{
			result.Add(selector(element));
		}
		return result.AsSpan();
	}
}
