namespace Puzzles.Flow.Concepts;

/// <summary>
/// Represents a grid that defines the start and end points of flows.
/// </summary>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_ToString | TypeImplFlags.EqualityOperators)]
public sealed partial class Grid :
	IBoard,
	IEquatable<Grid>,
	IEqualityOperators<Grid, Grid, bool>,
	IFormattable,
	IParsable<Grid>
{
	/// <summary>
	/// Indicates the size.
	/// </summary>
	private readonly int _size;

	/// <summary>
	/// Indicates the flow cells.
	/// </summary>
	private readonly SortedSet<FlowCell> _cells;


	/// <summary>
	/// Initializes a <see cref="Grid"/> with a flow cell list.
	/// </summary>
	/// <param name="size">The size.</param>
	/// <param name="cells">The cells.</param>
	private Grid(int size, FlowCell[] cells) => (_size, _cells) = (size, [.. cells]);


	/// <inheritdoc/>
	int IBoard.Rows => _size;

	/// <inheritdoc/>
	int IBoard.Columns => _size;


	/// <summary>
	/// Indicates the backing comparer to compare equality of field <see cref="_cells"/>.
	/// </summary>
	[field: MaybeNull]
	private static IEqualityComparer<SortedSet<FlowCell>> Comparer => field ??= SortedSet<FlowCell>.CreateSetComparer();


	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] Grid? other) => other is not null && Comparer.Equals(_cells, other._cells);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(_size, Comparer.GetHashCode(_cells));

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider) => ToString(null, formatProvider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? formatProvider)
	{
		throw new NotImplementedException();
	}


	/// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
	public static bool TryParse(string? s, [NotNullWhen(true)] out Grid? result) => TryParse(s, null, out result);

	/// <inheritdoc/>
	public static bool TryParse(string? s, IFormatProvider? provider, [NotNullWhen(true)] out Grid? result)
	{
		try
		{
			if (s is null)
			{
				throw new FormatException();
			}

			result = Parse(s, provider);
			return true;
		}
		catch (FormatException)
		{
			result = null;
			return false;
		}
	}

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static Grid Parse(string s) => Parse(s, null);

	/// <inheritdoc/>
	public static Grid Parse(string s, IFormatProvider? provider)
	{
		throw new NotImplementedException();
	}
}
