namespace Puzzles.MahjongMatching.Concepts;

/// <summary>
/// Represents a coordinate. Please note that all mahjong tiles will occupy 4 coordinates in total.
/// </summary>
/// <param name="X">Indicates the vertical index (row).</param>
/// <param name="Y">Indicates the horizontal index (column).</param>
[TypeImpl(TypeImplFlags.ComparisonOperators)]
public readonly partial record struct Coordinate(int X, int Y) :
	IComparable<Coordinate>,
	IComparisonOperators<Coordinate, Coordinate, bool>,
	IEqualityOperators<Coordinate, Coordinate, bool>,
	IMinMaxValue<Coordinate>
{
	/// <summary>
	/// Indicates the minimal possible value of the current type.
	/// </summary>
	public static readonly Coordinate MinValue = default;

	/// <summary>
	/// Indicates the maximal possible value of the current type.
	/// </summary>
	public static readonly Coordinate MaxValue = new(byte.MaxValue, byte.MaxValue);


	/// <summary>
	/// Indicates the first position that can fill with a new tile in upward direction.
	/// </summary>
	public Coordinate Up => new(X - 2, Y);

	/// <summary>
	/// Indicates the first position that can fill with a new tile in downward direction.
	/// </summary>
	public Coordinate Down => new(X + 2, Y);

	/// <summary>
	/// Indicates the first position that can fill with a new tile in left direction.
	/// </summary>
	public Coordinate Left => new(X, Y - 2);

	/// <summary>
	/// Indicates the first position that can fill with a new tile in right direction.
	/// </summary>
	public Coordinate Right => new(X, Y + 2);


	/// <inheritdoc/>
	static Coordinate IMinMaxValue<Coordinate>.MinValue => MinValue;

	/// <inheritdoc/>
	static Coordinate IMinMaxValue<Coordinate>.MaxValue => MaxValue;


	/// <summary>
	/// Determine whether the current coordinate overlaps with the specified coordinate.
	/// </summary>
	/// <param name="other">The other coordinate to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating whether they are overlapped with each other.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Overlaps(Coordinate other) => Math.Abs(X - other.X) < 2 && Math.Abs(Y - other.Y) < 2;

	/// <summary>
	/// Determine whether the coordinate is near to the specified one.
	/// </summary>
	/// <param name="other">The other coordinate to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <remarks>
	/// There're two kind of "near":
	/// <code>
	/// 1)
	///   .--.--.
	///   |  |  |
	///   '--'--'
	///    t1 t2
	///
	/// 2)
	///   .--.
	///   |  |--.
	///   '--|  |
	///      '--'
	///    t1 t2
	/// </code>
	/// From two cases, we can know that the X coordinate value satisfies the equation <c>|x1 - x2| &lt;= 1</c>,
	/// and Y coordinate value satisfies the equation <c>|y1 - y2| &lt;= 2</c>.
	/// </remarks>
	public bool IsNextTo(Coordinate other) => Math.Abs(X - other.X) <= 1 && Math.Abs(Y - other.Y) <= 2;

	/// <inheritdoc/>
	public int CompareTo(Coordinate other) => X.CompareTo(other.X) is var r and not 0 ? r : Y.CompareTo(other.Y);

	/// <summary>
	/// Converts the current instance as mask that can be used in bit operations.
	/// </summary>
	/// <returns>A mask of type <see cref="short"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public short AsMask() => (short)(X << 8 | Y);

	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"{nameof(X)} = ");
		builder.Append(X);
		builder.Append($", {nameof(Y)} = ");
		builder.Append(Y);
		return true;
	}


	/// <summary>
	/// Creates a <see cref="Coordinate"/> from mask.
	/// </summary>
	/// <param name="mask">The mask.</param>
	/// <returns>The coordinate value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Coordinate FromMask(short mask) => new(mask >>> 8, mask & byte.MaxValue);


	/// <summary>
	/// Advances the pointer to move a step into a new position in the specified direction.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	/// <param name="direction">The direction.</param>
	/// <returns>The new coordinate.</returns>
	/// <exception cref="InvalidOperationException">Throws when the coordinate is out of border.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Coordinate operator >>(Coordinate coordinate, Direction direction)
		=> direction switch
		{
			Direction.Up => coordinate.Up,
			Direction.Down => coordinate.Down,
			Direction.Left => coordinate.Left,
			Direction.Right => coordinate.Right,
			_ => throw new InvalidOperationException()
		};
}
