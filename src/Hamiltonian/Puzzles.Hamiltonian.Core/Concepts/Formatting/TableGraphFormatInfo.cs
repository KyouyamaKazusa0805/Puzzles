namespace Puzzles.Hamiltonian.Concepts.Formatting;

/// <summary>
/// Represents a format object that can format a <see cref="Graph"/> instance via table displaying.
/// </summary>
public sealed class TableGraphFormatInfo : GraphFormatInfo
{
	/// <summary>
	/// Indicates the display chars. By default its <c>('#', ' ')</c>.
	/// </summary>
	public (char OnChar, char OffChar) DisplayChars { get; init; } = ('#', ' ');


	/// <inheritdoc/>
	protected internal override string FormatCore(Graph graph)
	{
		var sb = new StringBuilder();
		for (var i = 0; i < graph.Size; i++)
		{
			sb.Append(graph[i] ? DisplayChars.OnChar : DisplayChars.OffChar);
			if ((i + 1) % graph.ColumnsLength == 0)
			{
				sb.AppendLine();
			}
		}
		return sb.RemoveFrom(^Environment.NewLine.Length).ToString();
	}

	/// <inheritdoc/>
	protected internal override Graph ParseCore(string str) => throw new NotSupportedException();
}
