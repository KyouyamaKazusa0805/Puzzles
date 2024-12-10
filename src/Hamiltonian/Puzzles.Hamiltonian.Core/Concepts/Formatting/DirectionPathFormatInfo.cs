namespace Puzzles.Hamiltonian.Concepts.Formatting;

/// <summary>
/// Provides a direction path information.
/// </summary>
/// <remarks>
/// Format:
/// <code><![CDATA[
/// <start-row>:<start-column>:<direction-arrows>
/// ]]></code>
/// Example:
/// <code><![CDATA[
/// 0:0:↓↓↓→→→↓↓←↑←←↓↓→↓←↓→→↑→↓→→↑↑←↑→→↑↑←←↑↑→↑←←↓←
/// ]]></code>
/// </remarks>
public sealed class DirectionPathFormatInfo : PathFormatInfo
{
	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public override object? GetFormat(Type? formatType) => formatType == typeof(PathFormatInfo) ? this : null;

	/// <inheritdoc/>
	protected internal override string FormatCore(Path path) => new(from direction in path.Directions select direction.GetArrow());

	/// <inheritdoc/>
	protected internal override Path ParseCore(string str)
	{
		var split = str.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		var startCoordinate = new Coordinate(int.Parse(split[0]), int.Parse(split[1]));
		var coordinates = new List<Coordinate> { startCoordinate };
		var currentCoordinate = startCoordinate;
		foreach (var character in split[2])
		{
			currentCoordinate >>= character;
			coordinates.Add(currentCoordinate);
		}
		return [.. coordinates];
	}
}
