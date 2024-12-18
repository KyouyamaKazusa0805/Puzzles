namespace Puzzles.Hamiltonian.Concepts.Formatting;

/// <summary>
/// Provides with indexed path format information.
/// </summary>
/// <param name="rows">Indicates the rows used.</param>
/// <param name="columns">Indicates the columns used.</param>
[method: SetsRequiredMembers]
public sealed partial class IndexedPathFormatInfo(
	[Property(Accessibility = "public required", Setter = PropertySetters.Init)] int rows,
	[Property(Accessibility = "public required", Setter = PropertySetters.Init)] int columns
) : PathFormatInfo
{
	/// <inheritdoc/>
	protected internal override string FormatCore(Path path)
	{
		var sb = new StringBuilder();
		sb.Append($"{Rows}:{Columns}:");
		foreach (var (r, c) in path)
		{
			var index = r * Columns + c;
			sb.Append(index.ToString("00"));
		}
		return sb.ToString();
	}

	/// <inheritdoc/>
	protected internal override Path ParseCore(string str)
	{
		var split = str.Split(':');
		var valuesSpan = split[2].AsSpan();
		var values = new List<Coordinate>(valuesSpan.Length >> 1);
		for (var i = 0; i < valuesSpan.Length; i += 2)
		{
			var index = int.Parse(valuesSpan[i..(i + 2)]);
			var coordinate = new Coordinate(index / Columns, index % Columns);
			values.Add(coordinate);
		}
		return [.. values];
	}
}
