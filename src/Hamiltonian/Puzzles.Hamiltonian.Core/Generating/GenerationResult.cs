namespace Puzzles.Hamiltonian.Generating;

/// <summary>
/// Represents a generation result.
/// </summary>
/// <param name="Success">Indicates whether the generated result is successful.</param>
/// <param name="Graph">Indicates the graph generated.</param>
/// <param name="Start">Indicates the start position.</param>
/// <param name="End">Indicates the end position.</param>
/// <param name="Path">Indicates the path.</param>
public sealed record GenerationResult(
	[property: MemberNotNullWhen(true, nameof(Graph), nameof(Path))] bool Success,
	Graph? Graph,
	Coordinate Start,
	Coordinate End,
	Path? Path
) : IEqualityOperators<GenerationResult, GenerationResult, bool>;
