namespace Puzzles.Hamiltonian.Measuring;

/// <summary>
/// Represents a rater for path entropy (i.e. the number of switching on rows and columns).
/// </summary>
public static class PathEntropy
{
	/// <summary>
	/// Gets the entropy value of the path.
	/// </summary>
	/// <param name="this">The path.</param>
	/// <returns>The value.</returns>
	public static int GetEntropyValue(Path @this)
	{
		var series = new List<bool>();
		for (var i = 0; i < @this.Length - 1; i++)
		{
			var a = @this[i];
			var b = @this[i + 1];
			var flag = a.RowIndex != b.RowIndex;
			if (series.Count == 0)
			{
				series.Add(flag);
			}
			else if (series[^1] != flag)
			{
				series.Add(flag);
			}
		}
		return series.Count;
	}
}
