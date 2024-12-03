namespace Puzzles.MahjongMatching.Concepts;

/// <summary>
/// Represents a mahjong tile.
/// </summary>
/// <param name="mask">Indicates the backing mask.</param>
public readonly partial struct Tile([Field] ushort mask) : IEquatable<Tile>, IEqualityOperators<Tile, Tile, bool>
{
	/// <summary>
	/// Indicates the maximum value bits.
	/// </summary>
	private const ushort MaxValueBits = 511;


	/// <summary>
	/// Indicates whether the tile is honor (wind or wrigley).
	/// </summary>
	public bool IsHonor => Kind is TileKind.Wind or TileKind.Wrigley;

	/// <summary>
	/// Indicates whether the tile is suit (bamboo, character or dot).
	/// </summary>
	public bool IsSuit => Kind is TileKind.Bamboo or TileKind.Character or TileKind.Dot;

	/// <summary>
	/// Indicates whether the tile is singleton (flower or season).
	/// </summary>
	public bool IsSingleton => Kind is TileKind.Flower or TileKind.Season;

	/// <summary>
	/// Indicates the target rank (value).
	/// </summary>
	public int Rank => (short.TrailingZeroCount((short)(_mask & MaxValueBits)) is var result and not 16 ? result : 0) + 1;

	/// <summary>
	/// Indicates the kind of the current tile.
	/// </summary>
	public TileKind Kind => (TileKind)(_mask >> 9 & 127);


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Tile comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(Tile other) => _mask == other._mask;

	/// <inheritdoc/>
	public override int GetHashCode() => _mask;

	/// <summary>
	/// Returns the internal mask.
	/// </summary>
	/// <returns>The mask.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ushort AsMask() => _mask;

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> Kind switch
		{
			TileKind.Bamboo => $"{nameof(TileKind.Bamboo)} {Rank}",
			TileKind.Character => $"{nameof(TileKind.Character)} {Rank}",
			TileKind.Dot => $"{nameof(TileKind.Dot)} {Rank}",
			TileKind.Wind => ((WindKind)Rank - 1).ToString(),
			TileKind.Wrigley => ((WrigleyKind)Rank - 1).ToString(),
			TileKind.Flower => ((FlowerKind)Rank - 1).ToString(),
			TileKind.Season => ((SeasonKind)Rank - 1).ToString(),
			_ => "<undefined>"
		};


	/// <summary>
	/// Creates a <see cref="Tile"/> instance.
	/// </summary>
	/// <param name="kind">The kind.</param>
	/// <param name="rank">The rank (value).</param>
	/// <returns>The <see cref="Tile"/> value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="kind"/> is not defined;
	/// or <paramref name="rank"/> is invalid under the specified kind.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe Tile Create(TileKind kind, int rank)
	{
		delegate*<int, Tile> creator = kind switch
		{
			TileKind.Bamboo => &Bamboo,
			TileKind.Character => &Character,
			TileKind.Dot => &Dot,
			TileKind.Wind => &Wind,
			TileKind.Wrigley => &Wrigley,
			TileKind.Flower => &Flower,
			TileKind.Season => &Season,
			_ => throw new ArgumentOutOfRangeException(nameof(kind))
		};
		return creator(rank);
	}

	/// <summary>
	/// Creates a <see cref="Tile"/> instance via the bamboo.
	/// </summary>
	/// <param name="rank">The rank.</param>
	/// <returns>The <see cref="Tile"/> value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the value is less than 0 or greater than or equal to 9.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Bamboo(int rank)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(rank, 9);
		return new((ushort)((int)TileKind.Bamboo << 9 | 1 << (rank - 1)));
	}

	/// <summary>
	/// Creates a <see cref="Tile"/> instance via the character.
	/// </summary>
	/// <param name="rank">The rank.</param>
	/// <returns>The <see cref="Tile"/> value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the value is less than 0 or greater than or equal to 9.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Character(int rank)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(rank, 9);
		return new((ushort)((int)TileKind.Character << 9 | 1 << (rank - 1)));
	}

	/// <summary>
	/// Creates a <see cref="Tile"/> instance via the dot.
	/// </summary>
	/// <param name="rank">The rank.</param>
	/// <returns>The <see cref="Tile"/> value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the value is less than 0 or greater than or equal to 9.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Dot(int rank)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(rank, 9);
		return new((ushort)((int)TileKind.Dot << 9 | 1 << (rank - 1)));
	}

	/// <summary>
	/// Creates a <see cref="Tile"/> instance via the wind.
	/// </summary>
	/// <param name="rank">The rank.</param>
	/// <returns>The <see cref="Tile"/> value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the value is less than 0 or greater than or equal to 4.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Wind(int rank)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(rank, 4);
		return new((ushort)((int)TileKind.Wind << 9 | 1 << (rank - 1)));
	}

	/// <inheritdoc cref="Wind(int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Wind(WindKind rank) => Wind((int)rank);

	/// <summary>
	/// Creates a <see cref="Tile"/> instance via the wrigley.
	/// </summary>
	/// <param name="rank">The rank.</param>
	/// <returns>The <see cref="Tile"/> value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the value is less than 0 or greater than or equal to 3.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Wrigley(int rank)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(rank, 3);
		return new((ushort)((int)TileKind.Wrigley << 9 | 1 << (rank - 1)));
	}

	/// <inheritdoc cref="Wrigley(int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Wrigley(WrigleyKind rank) => Wrigley((int)rank);

	/// <summary>
	/// Creates a <see cref="Tile"/> instance via the flower.
	/// </summary>
	/// <param name="rank">The rank.</param>
	/// <returns>The <see cref="Tile"/> value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the value is less than 0 or greater than or equal to 4.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Flower(int rank)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(rank, 4);
		return new((ushort)((int)TileKind.Flower << 9 | 1 << (rank - 1)));
	}

	/// <inheritdoc cref="Flower(int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Flower(FlowerKind rank) => Flower((int)rank);

	/// <summary>
	/// Creates a <see cref="Tile"/> instance via the season.
	/// </summary>
	/// <param name="rank">The rank.</param>
	/// <returns>The <see cref="Tile"/> value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the value is less than 0 or greater than or equal to 4.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Season(int rank)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(rank, 4);
		return new((ushort)((int)TileKind.Season << 9 | 1 << (rank - 1)));
	}

	/// <inheritdoc cref="Season(int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Tile Season(SeasonKind rank) => Season((int)rank);


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Tile left, Tile right) => left.Equals(right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Tile left, Tile right) => !(left == right);
}
