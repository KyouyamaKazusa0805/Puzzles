namespace Puzzles.Flow.Drawing;

/// <summary>
/// Represents a color lookup element.
/// </summary>
/// <param name="InputChar">Indicates the input character.</param>
/// <param name="ConsoleOutColorString">Indicates the console out colorization string.</param>
/// <param name="ForegroundRgbString">Indicates the foreground color string.</param>
/// <param name="BackgroundRgbString">Indicates the background color string.</param>
public readonly record struct ColorLookup(char InputChar, string ConsoleOutColorString, string ForegroundRgbString, string BackgroundRgbString) :
	IEqualityOperators<ColorLookup, ColorLookup, bool>;
