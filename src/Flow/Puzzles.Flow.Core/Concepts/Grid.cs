namespace Puzzles.Flow.Concepts;

/// <summary>
/// Represents a grid that defines the start and end points of flows.
/// </summary>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_ToString | TypeImplFlags.EqualityOperators)]
public sealed partial class Grid([Property] int size, [Field] SortedSet<FlowPosition> cells) :
	IBoard,
	ICloneable,
	IDataStructure,
	IEnumerable<FlowPosition>,
	IEquatable<Grid>,
	IEqualityOperators<Grid, Grid, bool>,
	IFormattable,
	IParsable<Grid>,
	IReadOnlyList<FlowPosition>
{
	/// <summary>
	/// Indicates the number of colors used.
	/// </summary>
	public int Length => _cells.Count;

	/// <summary>
	/// Indicates the minimum flow used.
	/// </summary>
	public FlowPosition Min => _cells.Min;

	/// <summary>
	/// Indicates the maximum flow used.
	/// </summary>
	public FlowPosition Max => _cells.Max;

	/// <inheritdoc/>
	int IBoard.Rows => Size;

	/// <inheritdoc/>
	int IBoard.Columns => Size;

	/// <inheritdoc/>
	int IReadOnlyCollection<FlowPosition>.Count => Length;

	/// <inheritdoc/>
	DataStructureType IDataStructure.Type => DataStructureType.Set;

	/// <inheritdoc/>
	DataStructureBase IDataStructure.Base => DataStructureBase.LinkedListBased;


	/// <summary>
	/// Indicates the backing comparer to compare equality of field <see cref="_cells"/>.
	/// </summary>
	[field: MaybeNull]
	private static IEqualityComparer<SortedSet<FlowPosition>> Comparer => field ??= SortedSet<FlowPosition>.CreateSetComparer();


	/// <inheritdoc/>
	public FlowPosition this[int index]
	{
		get
		{
			var i = -1;
			foreach (var flow in EnumerateFlows())
			{
				if (++i == index)
				{
					return flow;
				}
			}
			throw new IndexOutOfRangeException();
		}
	}


	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] Grid? other) => other is not null && Comparer.Equals(_cells, other._cells);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Size, Comparer.GetHashCode(_cells));

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider)
		=> (formatProvider as GridFormatInfo ?? new StandardGridFormatInfo()).FormatCore(this);

	/// <summary>
	/// Gets the state at the specified cell. This method never returns <see cref="CellState.Path"/>.
	/// </summary>
	/// <param name="cell">The desired cell.</param>
	/// <returns>The cell state.</returns>
	/// <seealso cref="CellState.Path"/>
	public CellState GetState(byte cell)
	{
		foreach (var ((startX, startY), (endX, endY), _) in EnumerateFlows())
		{
			var start = startX * Size + startY;
			var end = endX * Size + endY;
			if (cell == start)
			{
				return CellState.Start;
			}
			if (cell == end)
			{
				return CellState.End;
			}
		}
		return CellState.Empty;
	}

	/// <summary>
	/// Returns an enumerator object that can iterate on each cell, to get its corresponding state.
	/// </summary>
	/// <returns>An enumerator instance.</returns>
	public CellStateEnumerator EnumerateCellStates() => new(this);

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public FlowPositionEnumerator GetEnumerator() => new(this);

	/// <summary>
	/// Returns an enumerator object that can iterate on each element of each flow.
	/// </summary>
	/// <returns>An enumerator instance.</returns>
	public FlowPositionEnumerator EnumerateFlows() => new(this);

	/// <summary>
	/// Create a <see cref="ReadOnlySpan{T}"/> to store all flows.
	/// </summary>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> instance.</returns>
	public ReadOnlySpan<FlowPosition> AsSpan() => _cells.ToArray();

	/// <inheritdoc cref="ICloneable.Clone"/>
	public Grid Clone() => new(Size, [.. _cells]);

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _cells.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<FlowPosition> IEnumerable<FlowPosition>.GetEnumerator() => _cells.GetEnumerator();


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
		=> (provider as GridFormatInfo ?? new StandardGridFormatInfo()).ParseCore(s);
}
