namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates disjointed set data structure for connected component analysis of free spaces (see region_ functions).
/// </summary>
/// <param name="Parent">Indicates the parent index.</param>
/// <param name="Rank">Indicates rank (see Wikipedia article).</param>
internal record struct Region(Position Parent, byte Rank)
{
	/// <summary>
	/// Merge two regions.
	/// </summary>
	/// <param name="regions">The regions.</param>
	/// <param name="a">The first position.</param>
	/// <param name="b">The second position.</param>
	/// <returns>The merged result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Unite(Span<Region> regions, Position a, Position b)
	{
		var rootA = Find(regions, a);
		var rootB = Find(regions, b);
		if (rootA == rootB)
		{
			return;
		}

		if (regions[rootA].Rank < regions[rootB].Rank)
		{
			regions[rootA].Parent = rootB;
		}
		else if (regions[rootA].Rank > regions[rootB].Rank)
		{
			regions[rootB].Parent = rootA;
		}
		else
		{
			regions[rootB].Parent = rootA;
			regions[rootA].Rank++;
		}
	}

	/// <summary>
	/// Find for the position from the given index <paramref name="position"/>.
	/// </summary>
	/// <param name="regions">The regions.</param>
	/// <param name="position">The index.</param>
	/// <returns>The position.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte Find(Span<Region> regions, Position position)
	{
		if (regions[position].Parent != Analyzer.InvalidPosition && regions[position].Parent != position)
		{
			Debug.Assert(position != Analyzer.InvalidPosition);
			Debug.Assert(position < Analyzer.MaxGridCellsCount);
			regions[position].Parent = Find(regions, regions[position].Parent);
		}
		return regions[position].Parent;
	}

	/// <summary>
	/// Create a <see cref="Region"/> instance via the specified position.
	/// </summary>
	/// <param name="position">The position.</param>
	/// <returns>The <see cref="Region"/> instance created.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Region Create(Position position) => new(position, 0);
}
