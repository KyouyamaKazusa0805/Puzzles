namespace Puzzles.Flow.Concepts.Formatting;

/// <summary>
/// Represents a multi-line grid format.
/// </summary>
public sealed class MultilineGridFormatInfo : GridFormatInfo
{
	/// <inheritdoc/>
	[SuppressMessage("Reliability", "CA2014:Do not use stackalloc in loops", Justification = "<Pending>")]
	protected internal override unsafe string FormatCore(Grid grid)
	{
		var size = grid.Size;
		var charArray = stackalloc char*[size];
		for (var i = 0; i < size; i++)
		{
			var ptr = stackalloc char[size];
			charArray[i] = ptr;

			new Span<char>(ptr, size).Fill('.');
		}

		foreach (var ((startX, startY), (endX, endY), color) in grid.EnumerateFlows())
		{
			charArray[startX][startY] = FlowCharacters[color];
			charArray[endX][endY] = FlowCharacters[color];
		}

		var sb = new StringBuilder();
		for (var i = 0; i < size; i++)
		{
			sb.Append(new ReadOnlySpan<char>(charArray[i], size)).AppendLine();
		}
		return sb.RemoveFrom(^Environment.NewLine.Length).ToString();
	}

	/// <inheritdoc/>
	protected internal override Grid ParseCore(string str)
	{
		var lines = str.Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		var size = lines.Length;
		var colorCoordinates = new Dictionary<Color, List<Coordinate>>();
		for (var i = 0; i < size; i++)
		{
			var line = lines[i];
			if (line.Length != size)
			{
				throw new FormatException("Today grid type doesn't support for analysis on non-squared matrices.");
			}

			for (var j = 0; j < size; j++)
			{
				var character = char.ToUpper(line[j]);
				if (character == EmptyChar)
				{
					continue;
				}

				var color = character switch
				{
					>= '0' and <= '9' => (Color)(character - '0'),
					>= 'A' and <= 'F' => (Color)(character - 'A' + 10),
					_ => throw new NotSupportedException($"The specified color ID '{character}' is not supported.")
				};
				var coordinate = new Coordinate(i, j);
				if (!colorCoordinates.TryAdd(color, [coordinate]))
				{
					colorCoordinates[color].Add(coordinate);
				}
			}
		}

		// Now we have a dictionary that stores start and end points of colors. Now check validity of such points.
		// If valid, store them into a SortedSet<FlowCell> and return.
		var set = new SortedSet<FlowCell>();
		foreach (var (color, coordinates) in colorCoordinates)
		{
			set.Add(
				coordinates is [var start, var end]
					? new(start, end, color)
					: throw new FormatException("Not all colors have a pair of points to be used.")
			);
		}
		return new(size, set);
	}
}
