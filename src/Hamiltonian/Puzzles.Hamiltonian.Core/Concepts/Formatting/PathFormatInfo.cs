namespace Puzzles.Hamiltonian.Concepts.Formatting;

/// <summary>
/// Represents a path format information object.
/// </summary>
public abstract class PathFormatInfo : IFormatProvider
{
	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public abstract object? GetFormat(Type? formatType);

	/// <summary>
	/// Formats a <see cref="Path"/> into a <see cref="string"/> value.
	/// </summary>
	/// <param name="path">The path instance.</param>
	/// <returns>The formatted string value.</returns>
	protected internal abstract string FormatCore(Path path);

	/// <summary>
	/// Parses a <see cref="string"/> value, converting it into a <see cref="Path"/>.
	/// </summary>
	/// <param name="str">The string value to be parsed.</param>
	/// <returns>The converted <see cref="Path"/> instance.</returns>
	/// <exception cref="FormatException">Throws when the parsing operation has encountered an unexpected error.</exception>
	/// <exception cref="NotSupportedException">Throws when the parsing operation is not supported.</exception>
	protected internal abstract Path ParseCore(string str);
}
