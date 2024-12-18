namespace Puzzles.Hamiltonian.Concepts.Formatting;

/// <summary>
/// Represents a graph format information object.
/// </summary>
public abstract class GraphFormatInfo : IFormatProvider
{
	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public object? GetFormat(Type? formatType) => formatType == GetType() ? this : null;

	/// <summary>
	/// Formats a <see cref="Graph"/> into a <see cref="string"/> value.
	/// </summary>
	/// <param name="graph">The graph instance.</param>
	/// <returns>The formatted string value.</returns>
	protected internal abstract string FormatCore(Graph graph);

	/// <summary>
	/// Parses a <see cref="string"/> value, converting it into a <see cref="Graph"/>.
	/// </summary>
	/// <param name="str">The string value to be parsed.</param>
	/// <returns>The converted <see cref="Graph"/> instance.</returns>
	/// <exception cref="FormatException">Throws when the parsing operation has encountered an unexpected error.</exception>
	/// <exception cref="NotSupportedException">Throws when the parsing operation is not supported.</exception>
	protected internal abstract Graph ParseCore(string str);
}
