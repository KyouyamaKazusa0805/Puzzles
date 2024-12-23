namespace Puzzles.Onet.Concepts;

/// <summary>
/// Represents a match of item pair.
/// </summary>
/// <param name="Start">Indicates the start.</param>
/// <param name="End">Indicates the end.</param>
/// <param name="Interims">Indicates the interim coordinates.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Equatable)]
public sealed partial record ItemMatch(
	[property: HashCodeMember, EquatableMember] Coordinate Start,
	[property: HashCodeMember, EquatableMember] Coordinate End,
	params Coordinate[] Interims
) : IEqualityOperators<ItemMatch, ItemMatch, bool>
{
	/// <summary>
	/// Indicates the difficulty of the step.
	/// </summary>
	public double Difficulty { get; set; }

	/// <summary>
	/// Indicates the number of turning.
	/// </summary>
	public int TurningCount => Interims.Length;

	/// <summary>
	/// Indicates the distance of the match.
	/// </summary>
	public int Distance
	{
		get
		{
#pragma warning disable format
			return Interims switch
			{
				[var a, var b] => length(Start, a) + length(a, b) + length(b, End),
				[var a] => length(Start, a) + length(a, End),
				[] => length(Start, End),
				_ => throw new InvalidOperationException("The internal data is invalid.")
			};
#pragma warning restore format


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static int length(Coordinate coordinate1, Coordinate coordinate2)
				=> Math.Abs(coordinate1.X - coordinate2.X) + Math.Abs(coordinate1.Y - coordinate2.Y);
		}
	}


	/// <inheritdoc cref="object.ToString"/>
	public string ToFullString()
	{
#pragma warning disable format
		var interimsString = Interims switch
		{
			[var (ax, ay), var (bx, by)] => $", interims [{(ax, ay)}, {(bx, by)}]",
			[var (ax, ay)] => $", interims [{(ax, ay)}]",
			_ => string.Empty
		};
#pragma warning restore format
		return $"{(Start.X, Start.Y)} <-> {(End.X, End.Y)}{interimsString}";
	}

	/// <include
	///     file="../../../global-doc-comments.xml"
	///     path="/g/csharp9/feature[@name='records']/target[@name='method' and @cref='PrintMembers']"/>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"{nameof(Start)} = ");
		builder.Append(Start);
		builder.Append($", {nameof(End)} = ");
		builder.Append(End);
		if (Interims.Length != 0)
		{
			builder.Append($", {nameof(Interims)} = [");
			for (var i = 0; i < Interims.Length; i++)
			{
				builder.Append(Interims[i].ToString());
				if (i != Interims.Length - 1)
				{
					builder.Append(", ");
				}
			}
			builder.Append(']');
		}
		return true;
	}


	/// <summary>
	/// Reverses the match, making start coordinate to be end one, and end coordinate to be start one.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The reversed result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ItemMatch operator ~(ItemMatch value) => new(value.End, value.Start, [.. value.Interims.Reverse()]);
}
