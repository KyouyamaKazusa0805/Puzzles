namespace Puzzles.MahjongMatching.Concepts;

/// <summary>
/// Represents a puzzle.
/// </summary>
/// <param name="layers">Indicates the layers.</param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.EqualityOperators)]
public sealed partial class Puzzle([Field] params List<Layer> layers) :
	ICloneable,
	ICollection<Layer>,
	IEnumerable<Layer>,
	IEquatable<Puzzle>,
	IEqualityOperators<Puzzle, Puzzle, bool>,
	IFormattable,
	IReadOnlyCollection<Layer>,
	IReadOnlyList<Layer>
{
	/// <summary>
	/// Indicates the length of layers.
	/// </summary>
	public int Count => _layers.Count;

	/// <summary>
	/// Indicates the number of items in the puzzle.
	/// </summary>
	public int ItemsCount => _layers.Sum(static layer => layer.Count);

	/// <summary>
	/// Returns available tiles that can be used for matching.
	/// </summary>
	/// <remarks>
	/// Please note that there're two possible cases to be checked:
	/// <list type="number">
	/// <item>Whether a tile is under a tile</item>
	/// <item>Whether a tile is freed from both left and right side</item>
	/// </list>
	/// <i>For the second point, we may not check up and down side.</i>
	/// </remarks>
	public ReadOnlySpan<PuzzleTile> AvailableTiles
	{
		get
		{
			// Collect all tiles and group them by its layer.
			var tilesDictionary = new Dictionary<LayerIndex, List<PuzzleTile>>();
			for (var i = (LayerIndex)0; i < _layers.Count; i++)
			{
				tilesDictionary.Add(i, []);
			}

			var result = new HashSet<PuzzleTile>();
			for (var i = (LayerIndex)0; i < _layers.Count; i++)
			{
				var layer = _layers[i];
				foreach (var tile in layer)
				{
					var puzzleTile = new PuzzleTile(i, tile);
					result.Add(puzzleTile);
					tilesDictionary[i].Add(puzzleTile);
				}
			}

			// Iterate on values, and determine whether the two points mentioned in <remarks> part are both satisfied.
			foreach (var tiles in tilesDictionary.Values)
			{
				foreach (var tile in tiles)
				{
					// Check for case 1.
					var isOverlappedWithUpperLayer = true;
					if ((LayerIndex)(tile.Layer - 1) is var aboveLayerIndex and >= 0)
					{
						foreach (var oneLayerAboveTile in tilesDictionary[aboveLayerIndex])
						{
							if (oneLayerAboveTile.Overlaps(tile))
							{
								// Failed.
								isOverlappedWithUpperLayer = false;
								break;
							}
						}
					}
					if (!isOverlappedWithUpperLayer)
					{
						result.Remove(tile);
						continue;
					}

					// Check for case 2.
					var (leftIsBlocked, rightIsBlocked) = (false, false);
					foreach (var sameLayerTile in tilesDictionary[tile.Layer])
					{
						if (sameLayerTile == tile)
						{
							continue;
						}

						if (sameLayerTile.IsLeftNextTo(tile))
						{
							leftIsBlocked = true;
						}
						if (sameLayerTile.IsRightNextTo(tile))
						{
							rightIsBlocked = true;
						}
						if ((leftIsBlocked, rightIsBlocked) == (true, true))
						{
							break;
						}
					}
					if (leftIsBlocked && rightIsBlocked)
					{
						// Failed.
						result.Remove(tile);
						continue;
					}
				}
			}

			return result.ToArray();
		}
	}

	/// <inheritdoc/>
	bool ICollection<Layer>.IsReadOnly => false;


	/// <summary>
	/// Gets the layer at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The layer.</returns>
	public Layer this[int index] => _layers[index];


	/// <summary>
	/// Applies the specified match result into the puzzle.
	/// </summary>
	/// <param name="match">The match.</param>
	/// <exception cref="InvalidOperationException">Throws when the match is not a same tile.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Apply(TileMatch match)
	{
		var ((l1, (_, c1)), (l2, (_, c2))) = match;
		this[l1].Remove(c1);
		this[l2].Remove(c2);
	}

	/// <summary>
	/// Clears all layers.
	/// </summary>
	public void Clear() => _layers.Clear();

	/// <summary>
	/// Add a new layer into the puzzle.
	/// </summary>
	/// <param name="layer">The layer to be added.</param>
	public void Add(Layer layer) => _layers.Add(layer);

	/// <summary>
	/// Remove a layer from the puzzle, and return a <see cref="bool"/> value indicating whether the layer exists in the puzzle.
	/// </summary>
	/// <param name="layer">The layer to be removed.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Remove(Layer layer)
	{
		var index = _layers.FindIndex(l => l == layer);
		if (index == -1)
		{
			return false;
		}

		_layers.RemoveAt(index);
		return true;
	}

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] Puzzle? other)
	{
		if (other is null || Count != other.Count)
		{
			return false;
		}

		for (var i = 0; i < Count; i++)
		{
			if (this[i] != other[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Determine whether the current puzzle contains the specified layer.
	/// </summary>
	/// <param name="layer">The layer.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Contains(Layer layer) => IndexOf(layer) != -1;

	/// <summary>
	/// Gets the index of the specified layer.
	/// </summary>
	/// <param name="layer">The layer.</param>
	/// <returns>An <see cref="int"/> value as index.</returns>
	public int IndexOf(Layer layer) => _layers.FindIndex(l => l == layer);

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var hashCode = new HashCode();
		foreach (var layer in _layers)
		{
			hashCode.Add(layer);
		}
		return hashCode.ToHashCode();
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
				var array = this[i].ToString("a").Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
				for (var j = 0; j < array.Length; j++)
				{
					var line = array[j];
					_ = j != array.Length - 1 ? sb.AppendLine($"\t{line}") : sb.Append($"\t{line}");
				}
				_ = i != Count - 1 ? sb.AppendLine(", ") : sb.AppendLine();
			}
			sb.Append(']');
			return sb.ToString();
		}

		string toMaskString() => $"[{string.Join(", ", from layer in _layers select layer.ToString("m"))}]";
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Enumerator GetEnumerator() => new(_layers.AsSpan());

	/// <summary>
	/// Enumerates all tiles exist in the current puzzle.
	/// </summary>
	/// <returns>An enumerator type that can iterate on each tile.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TileEnumerator EnumerateTiles() => new(this);

	/// <summary>
	/// Finds all possible matches of the puzzle.
	/// </summary>
	/// <returns>A list of matches found.</returns>
	public ReadOnlySpan<TileMatch> GetAllMatches()
	{
		var result = new List<TileMatch>();
		foreach (var tileGroup in from element in AvailableTiles group element by element.Tile.TileKey)
		{
			if (tileGroup.Length < 2)
			{
				continue;
			}

			foreach (var pair in tileGroup.AsSpan().GetSubsets(2))
			{
				result.Add(new(pair[0], pair[1]));
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Converts the current instance into a <see cref="ReadOnlySpan{T}"/> instance.
	/// </summary>
	/// <returns>A span.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<Layer> AsSpan() => _layers.AsSpan();

	/// <inheritdoc cref="ICloneable.Clone"/>
	public Puzzle Clone() => [.. from layer in this select layer.Clone()];

	/// <inheritdoc/>
	void ICollection<Layer>.CopyTo(Layer[] array, int arrayIndex) => _layers.AsSpan().CopyTo(array.AsSpan()[arrayIndex..]);

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(format);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _layers.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Layer> IEnumerable<Layer>.GetEnumerator() => ((IEnumerable<Layer>)_layers).GetEnumerator();


	/// <summary>
	/// Negates expression <see cref="ItemsCount"/> != 0.
	/// </summary>
	/// <param name="value">The puzzle to be checked.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !(Puzzle value) => value.ItemsCount == 0;

	/// <summary>
	/// Returns expression value <see cref="ItemsCount"/> != 0.
	/// </summary>
	/// <param name="value">The puzzle to be checked.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator true(Puzzle value) => value.ItemsCount != 0;

	/// <summary>
	/// Negates expression <see cref="ItemsCount"/> != 0.
	/// </summary>
	/// <param name="value">The puzzle to be checked.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator false(Puzzle value) => value.ItemsCount == 0;
}
