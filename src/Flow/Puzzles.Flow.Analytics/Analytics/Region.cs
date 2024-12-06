namespace Puzzles.Flow.Analytics;

/// <summary>
/// Indicates disjointed set data structure for connected component analysis of free spaces (see region_ functions).
/// </summary>
/// <param name="Parent">Indicates the parent index.</param>
/// <param name="Rank">Indicates rank (see Wikipedia article).</param>
internal record struct Region(byte Parent, byte Rank);
