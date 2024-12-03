namespace Puzzles.MahjongMatching.Concepts;

/// <summary>
/// Represents a tile that is inside a layer (contains properties tile and coordinate).
/// </summary>
/// <param name="mask">The mask.</param>
public struct LayerTile(int mask) : IEquatable<LayerTile>, IEqualityOperators<LayerTile, LayerTile, bool>
{
	/// <summary>
	/// Indicates the backing mask.
	/// </summary>
	private int _mask = mask;


	/// <summary>
	/// Initializes a <see cref="LayerTile"/> instance via the tile value and its coordinate.
	/// </summary>
	/// <param name="tile">Indicates the tile.</param>
	/// <param name="coordinate">Indicates the coordinate.</param>
	/// <returns>The tile information.</returns>
	public LayerTile(Tile tile, Coordinate coordinate) : this(coordinate.AsMask() << 16 | (int)tile.AsMask())
	{
	}


	/// <summary>
	/// Indicates the coordinate.
	/// </summary>
	public readonly Coordinate Coordinate => Coordinate.FromMask((short)(_mask >> 16));

	/// <summary>
	/// Indicates the tile.
	/// </summary>
	public Tile Tile
	{
		readonly get => new((ushort)(_mask & (1 << 16) - 1));

		internal set => _mask |= value.AsMask();
	}

	/// <summary>
	/// Indicates the tile key in order to distinct different kinds of tiles that cannot be matched.
	/// </summary>
	public readonly Tile TileKey
		=> Tile switch
		{
			{ Kind: TileKind.Season } => Tile.Season(1),
			{ Kind: TileKind.Flower } => Tile.Flower(1),
			_ => Tile
		};


	/// <summary>
	/// Deconstruct the instance into multiple variables.
	/// </summary>
	/// <param name="tile">The tile.</param>
	/// <param name="coordinate">The coordinate.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void Deconstruct(out Tile tile, out Coordinate coordinate) => (tile, coordinate) = (Tile, Coordinate);

	/// <inheritdoc/>
	public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is LayerTile comparer && Equals(comparer);

	/// <inheritdoc/>
	public readonly bool Equals(LayerTile other) => _mask == other._mask;

	/// <inheritdoc cref="Coordinate.Overlaps(Coordinate)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Overlaps(LayerTile other) => Coordinate.Overlaps(other.Coordinate);

	/// <inheritdoc cref="Coordinate.IsNextTo(Coordinate)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool IsNextTo(LayerTile other) => Coordinate.IsNextTo(other.Coordinate);

	/// <inheritdoc/>
	public override readonly int GetHashCode() => _mask;

	/// <summary>
	/// Converts the current object into a mask.
	/// </summary>
	/// <returns>The mask of type <see cref="int"/>.</returns>
	public readonly int AsMask() => _mask;

	/// <inheritdoc cref="object.ToString"/>
	public override readonly string ToString()
		=> $$"""{{nameof(LayerTile)}} { {{nameof(Tile)}} = {{Tile}}, {{nameof(Coordinate)}} = {{Coordinate}} }""";


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(LayerTile left, LayerTile right) => left.Equals(right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(LayerTile left, LayerTile right) => !(left == right);
}
