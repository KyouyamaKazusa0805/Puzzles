namespace Puzzles.Hamiltonian.Concepts.Formatting;

/// <summary>
/// Provides a coordinate format information.
/// </summary>
public sealed class CoordinatePathFormatInfo : PathFormatInfo
{
	/// <summary>
	/// Indicates the separator. By default its <c>" -> "</c>.
	/// </summary>
	public string Separator { get; init; } = " -> ";


	/// <inheritdoc/>
	protected internal override string FormatCore(Path path)
	{
		var sb = new StringBuilder();
		foreach (var element in path.Span)
		{
			sb.Append(element.ToString());
			sb.Append(Separator);
		}
		return sb.RemoveFrom(^Separator.Length).ToString();
	}

	/// <inheritdoc/>
	[DoesNotReturn]
	protected internal override Path ParseCore(string str) => throw new NotSupportedException();
}
