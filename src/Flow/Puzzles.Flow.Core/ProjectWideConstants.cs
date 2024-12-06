namespace Puzzles.Flow;

/// <summary>
/// Represents a list of constants and read-only fields used in project.
/// </summary>
public static class ProjectWideConstants
{
	/// <summary>
	/// Indicates the invalid position value.
	/// </summary>
	public const byte InvalidPos = 0xFF;

	/// <summary>
	/// Indicates the maximum number of supported colors. This value is a constant and cannot be modified due to massive complexity.
	/// </summary>
	public const int MaxColors = 16;

	/// <summary>
	/// Indicates the maximum size of the grid. This value is a constant and cannot be modified due to massive complexity.
	/// </summary>
	public const int MaxSize = 15;

	/// <summary>
	/// Indicates the maximum length of cells available in the grid. The value is equal to 239.
	/// </summary>
	public const int MaxCells = (MaxSize + 1) * MaxSize - 1;

	/// <summary>
	/// Represents mega-bytes. The value is equal to 1048576.
	/// </summary>
	internal const int MegaByte = 1 << 20;


	/// <summary>
	/// Indicates the color lookup dictionary.
	/// </summary>
	public static readonly ColorLookup[] ColorDictionary = [
		new('0', "101", "ff0000", "723939"), // red
		new('1', "104", "0000ff", "393972"), // blue
		new('2', "103", "eeee00", "6e6e39"), // yellow
		new('3', "42", "008100", "395539"), // green
		new('4', "43", "ff8000", "725539"), // orange
		new('5', "106", "00ffff", "397272"), // cyan
		new('6', "105", "ff00ff", "723972"), // magenta
		new('7', "41", "a52a2a", "5f4242"), // maroon
		new('8', "45", "800080", "553955"), // purple
		new('9', "100", "a6a6a6", "5f5e5f"), // gray
		new('A', "107", "ffffff", "727272"), // white
		new('B', "102", "00ff00", "397239"), // bright green
		new('C', "47", "bdb76b", "646251"), // tan
		new('D', "44", "00008b", "393958"), // dark blue
		new('E', "46", "008180", "395555"), // dark cyan
		new('F', "35", "ff1493", "72415a") // pink?
	];


	/// <summary>
	/// Returns the color index of dictionary table <see cref="ColorDictionary"/>.
	/// </summary>
	/// <param name="c">The input character.</param>
	/// <returns>The index.</returns>
	/// <seealso cref="ColorDictionary"/>
	public static int GetColor(char c)
	{
		for (var i = 0; i < MaxColors; i++)
		{
			if (ColorDictionary[i].InputChar == c)
			{
				return i;
			}
		}
		return -1;
	}
}
