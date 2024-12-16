namespace Puzzles.Flow.Concepts.Formatting;

/// <summary>
/// Represents a format provider for type <see cref="Grid"/>.
/// </summary>
/// <seealso cref="Grid"/>
public abstract class GridFormatInfo : IFormatProvider
{
	/// <summary>
	/// Represents empty character.
	/// </summary>
	protected const char EmptyChar = '.';

	/// <summary>
	/// Indicates all supported flow characters.
	/// </summary>
	private const string SupportedFlowCharacters = "0123456789ABCDEF";


	/// <summary>
	/// Indicates all supported flow characters.
	/// </summary>
	protected static ReadOnlySpan<char> FlowCharacters => SupportedFlowCharacters;


	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public object? GetFormat(Type? formatType) => formatType == GetType() ? this : null;

	/// <summary>
	/// Formats a <see cref="Grid"/> into a <see cref="string"/> value.
	/// </summary>
	/// <param name="grid">The grid instance.</param>
	/// <returns>The formatted string value.</returns>
	protected internal abstract string FormatCore(Grid grid);

	/// <summary>
	/// Parses a <see cref="string"/> value, converting it into a <see cref="Grid"/>.
	/// </summary>
	/// <param name="str">The string value to be parsed.</param>
	/// <returns>The converted <see cref="Grid"/> instance.</returns>
	/// <exception cref="FormatException">Throws when the parsing operation has encountered an unexpected error.</exception>
	/// <exception cref="NotSupportedException">Throws when the parsing operation is not supported.</exception>
	protected internal abstract Grid ParseCore(string str);
}
