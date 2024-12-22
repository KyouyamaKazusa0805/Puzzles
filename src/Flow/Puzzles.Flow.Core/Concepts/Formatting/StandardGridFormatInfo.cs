namespace Puzzles.Flow.Concepts.Formatting;

/// <summary>
/// Represents a format information for stardard rules.
/// </summary>
public sealed class StandardGridFormatInfo : GridFormatInfo
{
	/// <summary>
	/// Indicates all supported lengths.
	/// </summary>
	private static readonly Position[] SupportedLengths = [4, 9, 16, 25, 36, 49, 64, 81, 100, 121, 144, 169, 196, 225];


	/// <inheritdoc/>
	protected internal override string FormatCore(Grid grid)
	{
		var size = grid.Size;
		var result = (stackalloc char[size * size]);
		result.Fill('.');

		foreach (var ((startX, startY), (endX, endY), color) in grid.EnumerateFlows())
		{
			var start = startX * size + startY;
			var end = endX * size + endY;
			result[start] = (char)(color + (color < 10 ? '0' : 'A'));
		}
		return result.ToString();
	}

	/// <inheritdoc/>
	protected internal override Grid ParseCore(string str)
	{
		_ = str.AsSpan() is { Length: var strLength } s;
		if (!Array.Exists(SupportedLengths, length => strLength == length))
		{
			throw new FormatException();
		}

		var size = (int)Math.Sqrt(strLength);
		var flowsDictionary = new Dictionary<Color, List<Position>>();
		for (var i = (Position)0; i < strLength; i++)
		{
			var ch = s[i];
			switch (char.ToUpper(ch))
			{
				case '.':
				{
					continue;
				}
				case >= '0' and <= '9':
				{
					var color = (Color)(ch - '0');
					if (!flowsDictionary.TryAdd(color, [i]))
					{
						flowsDictionary[color].Add(i);
					}
					break;
				}
				case >= '0' and <= '9' or >= 'A' or <= 'F':
				{
					var color = (Color)(ch - 'A' + 10);
					if (!flowsDictionary.TryAdd(color, [i]))
					{
						flowsDictionary[color].Add(i);
					}
					break;
				}
				default:
				{
					throw new FormatException();
				}
			}
		}

		var flows = new List<FlowPosition>(flowsDictionary.Count);
		foreach (var (color, positions) in flowsDictionary)
		{
			if (positions is not [var start, var end])
			{
				throw new FormatException();
			}

			var startX = start / size;
			var startY = start % size;
			var endX = end / size;
			var endY = end % size;
			flows.Add(new(new(startX, startY), new(endX, endY), color));
		}
		return new(size, [.. flows]);
	}
}
