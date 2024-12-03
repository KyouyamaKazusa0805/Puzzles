namespace Puzzles.MahjongMatching.Concepts;

/// <summary>
/// Represents a puzzle layer.
/// </summary>
public sealed partial class Layer :
	ICloneable,
	ICollection<LayerTile>,
	IEnumerable<LayerTile>,
	IEquatable<Layer>,
	IEqualityOperators<Layer, Layer, bool>,
	IFormattable,
	IList<LayerTile>,
	IReadOnlyCollection<LayerTile>,
	IReadOnlyList<LayerTile>
{
	/// <summary>
	/// Indicates the tiles.
	/// </summary>
	private readonly List<Tile> _tiles = [];

	/// <summary>
	/// Indicates the coordinates.
	/// </summary>
	private readonly List<Coordinate> _coordinates = [];


	/// <summary>
	/// Initializes a <see cref="Layer"/> instance.
	/// </summary>
	public Layer()
	{
	}

	/// <summary>
	/// Initializes a <see cref="Layer"/> instance an their coordinates.
	/// </summary>
	/// <param name="tiles">Indicates the tiles.</param>
	/// <param name="coordinates">Indicates the coordinates.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the number of argument <paramref name="tiles"/> is not equal
	/// to the number of argument <paramref name="coordinates"/>.
	/// </exception>
	public Layer(Tile[] tiles, Coordinate[] coordinates)
	{
		ArgumentOutOfRangeException.ThrowIfNotEqual(tiles.Length, coordinates.Length);

		_tiles = [.. tiles];
		_coordinates = [.. coordinates];
	}


	/// <summary>
	/// Indicates the number of elements in the current collection.
	/// </summary>
	public int Count => _tiles.Count;

	/// <summary>
	/// Indicates the minimal position of the layer.
	/// </summary>
	public Coordinate MinimumPosition => _coordinates.AsSpan().Min();

	/// <summary>
	/// Indicates the maximum position of the layer.
	/// </summary>
	public Coordinate MaximumPosition => _coordinates.AsSpan().Max();

	/// <inheritdoc/>
	bool ICollection<LayerTile>.IsReadOnly => false;


	/// <summary>
	/// Gets the element at the specified index.
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>The tile and its coordinate.</returns>
	public LayerTile this[int index] => new(_tiles[index], _coordinates[index]);

	/// <inheritdoc/>
	LayerTile IList<LayerTile>.this[int index]
	{
		get => this[index];

		set
		{
			_tiles[index] = value.Tile;
			_coordinates[index] = value.Coordinate;
		}
	}


	/// <summary>
	/// Clears the collection, removing all elements from the collection.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		_tiles.Clear();
		_coordinates.Clear();
	}

	/// <inheritdoc/>
	public void Add(LayerTile item) => Add(item.Tile, item.Coordinate);

	/// <summary>
	/// Adds a new tile into the current layer.
	/// </summary>
	/// <param name="tile">The tile.</param>
	/// <param name="coordinate">The coordinate.</param>
	/// <exception cref="InvalidOperationException">Throws when the tile cannot be filled into the layer.</exception>
	public void Add(Tile tile, Coordinate coordinate)
	{
		if (_coordinates.FindIndex(c => c.Overlaps(coordinate)) is var overlappedItemIndex and not -1)
		{
			var message = $"The current coordinate is invalid because it overlaps with the element '{_tiles[overlappedItemIndex]}'.";
			throw new InvalidOperationException(message);
		}

		_tiles.Add(tile);
		_coordinates.Add(coordinate);
	}

	/// <summary>
	/// Removes the tile from the current layer.
	/// </summary>
	/// <param name="tile">The tile.</param>
	public void Remove(Tile tile)
	{
		if (_tiles.IndexOf(tile) is var index and not -1)
		{
			_tiles.RemoveAt(index);
			_coordinates.RemoveAt(index);
		}
	}

	/// <summary>
	/// Removes the tile at the specified coordinate. from the current layer.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	public void Remove(Coordinate coordinate)
	{
		if (_coordinates.IndexOf(coordinate) is var index and not -1)
		{
			_tiles.RemoveAt(index);
			_coordinates.RemoveAt(index);
		}
	}

	/// <inheritdoc/>
	public bool Remove(LayerTile item)
	{
		if (_tiles.IndexOf(item.Tile) is var index1 and not -1
			&& _coordinates.IndexOf(item.Coordinate) is var index2 and not -1
			&& index1 == index2)
		{
			_tiles.RemoveAt(index1);
			_coordinates.RemoveAt(index2);
			return true;
		}
		return false;
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as Layer);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] Layer? other)
	{
		if (other is null)
		{
			return false;
		}

		var leftTiles = _tiles.ToHashSet();
		var rightTiles = other._tiles.ToHashSet();
		if (!leftTiles.SetEquals(rightTiles))
		{
			return false;
		}

		var leftCoordinates = _coordinates.ToHashSet();
		var rightCoordinates = other._coordinates.ToHashSet();
		return leftCoordinates.SetEquals(rightCoordinates);
	}

	/// <summary>
	/// Determine whether the specified tile is inside the current layer.
	/// </summary>
	/// <param name="tile">The tile.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Contains(Tile tile) => _tiles.Contains(tile);

	/// <summary>
	/// Determine whether the specified tile is inside the current coordinate.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Contains(Coordinate coordinate) => _coordinates.Contains(coordinate);

	/// <inheritdoc/>
	public bool Contains(LayerTile item) => Contains(item.Tile) && Contains(item.Coordinate);

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var hashCode = new HashCode();
		foreach (var tile in _tiles)
		{
			hashCode.Add(tile);
		}
		foreach (var coordinate in _coordinates)
		{
			hashCode.Add(coordinate);
		}
		return hashCode.ToHashCode();
	}

	/// <inheritdoc/>
	public int IndexOf(LayerTile item)
	{
		for (var i = 0; i < Count; i++)
		{
			if (_tiles[i] == item.Tile && _coordinates[i] == item.Coordinate)
			{
				return i;
			}
		}
		return -1;
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(string? format)
	{
		return format switch
		{
			null or "a" => toArrayString(),
			"m" => toMaskString(),
			_ => throw new FormatException()
		};


		string toArrayString()
		{
			var sb = new StringBuilder();
			sb.AppendLine("[");
			for (var i = 0; i < Count; i++)
			{
				sb.Append('\t');
				sb.Append(this[i].ToString());
				_ = i != Count - 1 ? sb.AppendLine(", ") : sb.AppendLine();
			}
			sb.Append(']');
			return sb.ToString();
		}

		string toMaskString()
		{
			var sb = new StringBuilder();
			sb.Append('[');
			for (var i = 0; i < Count; i++)
			{
				sb.Append(this[i].AsMask());
				if (i != Count - 1)
				{
					sb.Append(", ");
				}
			}
			sb.Append(']');
			return sb.ToString();
		}
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Enumerator GetEnumerator() => new(this);

	/// <summary>
	/// Returns a <see cref="TileEnumerator"/> that can iterate on each tile.
	/// </summary>
	/// <returns>A <see cref="TileEnumerator"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TileEnumerator EnumerateTiles() => new(this);

	/// <summary>
	/// Returns a <see cref="CoordinateEnumerator"/> that can iterate on each coordinate.
	/// </summary>
	/// <returns>A <see cref="CoordinateEnumerator"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CoordinateEnumerator EnumerateCoordinates() => new(this);

	/// <summary>
	/// Converts the current instance into a <see cref="ReadOnlySpan{T}"/> instance.
	/// </summary>
	/// <returns>A span.</returns>
	public ReadOnlySpan<LayerTile> AsSpan() => from pair in this select pair;

	/// <inheritdoc cref="ICloneable.Clone"/>
	public Layer Clone() => [.. from pair in this select pair];

	/// <inheritdoc/>
	void ICollection<LayerTile>.CopyTo(LayerTile[] array, int arrayIndex)
	{
		if (Count + arrayIndex > array.Length)
		{
			throw new ArgumentException($"The argument '{nameof(array)}' doesn't contain enough space.", nameof(array));
		}
		AsSpan().CopyTo(array.AsSpan()[arrayIndex..]);
	}

	/// <inheritdoc/>
	void IList<LayerTile>.Insert(int index, LayerTile item)
	{
		_tiles.Insert(index, item.Tile);
		_coordinates.Insert(index, item.Coordinate);
	}

	/// <inheritdoc/>
	void IList<LayerTile>.RemoveAt(int index)
	{
		_tiles.RemoveAt(index);
		_coordinates.RemoveAt(index);
	}

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(format);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => this.AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<LayerTile> IEnumerable<LayerTile>.GetEnumerator()
	{
		for (var i = 0; i < Count; i++)
		{
			yield return new(_tiles[i], _coordinates[i]);
		}
	}


	/// <summary>
	/// Negates the expression <see cref="Count"/> != 0.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !(Layer value) => value.Count == 0;

	/// <summary>
	/// Returns the expression <see cref="Count"/> != 0.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator true(Layer value) => value.Count != 0;

	/// <summary>
	/// Negates the expression <see cref="Count"/> != 0.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator false(Layer value) => value.Count == 0;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Layer? left, Layer? right)
		=> (left, right) switch { (not null, not null) => left.Equals(right), (null, null) => true, _ => false };

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Layer? left, Layer? right) => !(left == right);
}
