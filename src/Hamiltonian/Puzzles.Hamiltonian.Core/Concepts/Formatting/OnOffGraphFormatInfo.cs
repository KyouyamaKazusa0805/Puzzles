namespace Puzzles.Hamiltonian.Concepts.Formatting;

/// <summary>
/// Represents a format information object that uses "on" and "off" state to format a graph.
/// </summary>
public sealed partial class OnOffGraphFormatInfo : GraphFormatInfo
{
	/// <summary>
	/// Indicates whether the formatter will emit size part. By default the value is <see langword="true"/>.
	/// </summary>
	public bool WithSize { get; init; } = true;


	[GeneratedRegex("""^(\d+):(\d+)(:([01]{4,}))?$""", RegexOptions.Compiled)]
	private static partial Regex ParsingFormatPattern { get; }

	[GeneratedRegex("1", RegexOptions.Compiled)]
	private static partial Regex OnStatePattern { get; }


	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public override object? GetFormat(Type? formatType) => formatType == typeof(OnOffGraphFormatInfo) ? this : null;

	/// <inheritdoc/>
	protected internal override string FormatCore(Graph graph)
	{
		var charSequence = (stackalloc char[graph.Size]);
		for (var i = 0; i < graph.Size; i++)
		{
			charSequence[i] = graph[i] ? '1' : '0';
		}
		return WithSize ? $"{graph.RowsLength}:{graph.ColumnsLength}:{charSequence}" : charSequence.ToString();
	}

	/// <inheritdoc/>
	protected internal override Graph ParseCore(string str)
	{
		switch (ParsingFormatPattern.Match(str))
		{
			case { Success: true, Groups: [_, { Value: var a }, { Value: var b }, _, { Value: var c }] }:
			{
				if (c == string.Empty)
				{
					return new(int.Parse(a), int.Parse(b));
				}

				var result = new Graph(int.Parse(a), int.Parse(b));
				foreach (var match in OnStatePattern.EnumerateMatches(c))
				{
					result[match.Index] = true;
				}
				return result;
			}
			default:
			{
				throw new FormatException();
			}
		}
	}
}
