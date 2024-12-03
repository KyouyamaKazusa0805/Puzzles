namespace Puzzles.MahjongMatching.Concepts;

public partial class Puzzle
{
	/// <summary>
	/// Represents an enumerator type that can iterate on each tile in the puzzle.
	/// </summary>
	public ref struct TileEnumerator : IEnumerator<PuzzleTile>, IEnumerable<PuzzleTile>
	{
		/// <summary>
		/// Indicates the backing tiles.
		/// </summary>
		private readonly List<PuzzleTile> _tiles;

		/// <summary>
		/// Indicates the backing puzzle.
		/// </summary>
		private readonly Puzzle _puzzle;

		/// <summary>
		/// The enumerator instance.
		/// </summary>
		private List<PuzzleTile>.Enumerator _enumerator;


		/// <summary>
		/// Initializes a <see cref="TileEnumerator"/> instance via the puzzle.
		/// </summary>
		/// <param name="puzzle">The puzzle.</param>
		public TileEnumerator(Puzzle puzzle)
		{
			_puzzle = puzzle;

			var result = new List<PuzzleTile>();
			for (var i = (LayerIndex)0; i < puzzle._layers.Count; i++)
			{
				var layer = puzzle._layers[i];
				foreach (var tile in layer)
				{
					var puzzleTile = new PuzzleTile(i, tile);
					result.Add(puzzleTile);
				}
			}

			_tiles = result;
			_enumerator = result.GetEnumerator();
		}


		/// <summary>
		/// Indicates the number of tiles.
		/// </summary>
		public readonly int Count => _tiles.Count;


		/// <summary>
		/// Gets or sets the tile at the specified index.
		/// </summary>
		/// <param name="index">The desired index.</param>
		/// <value>The value to be set.</value>
		/// <returns>The tile.</returns>
		public readonly Tile this[int index]
		{
			get => _tiles[index].Tile.Tile;

			set
			{
				foreach (var layer in _puzzle)
				{
					var count = layer.Count;
					if (index < count)
					{
						var layerCasted = (IList<LayerTile>)layer;
						layerCasted[index] = layerCasted[index] with { Tile = value };
						return;
					}
					index -= count;
				}
			}
		}


		/// <inheritdoc/>
		public readonly PuzzleTile Current => _enumerator.Current;

		/// <summary>
		/// Indicates the currently iterated layer index.
		/// </summary>
		public readonly LayerIndex CurrentLayerIndex => Current.Layer;

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public readonly TileEnumerator GetEnumerator() => this;

		/// <inheritdoc/>
		public bool MoveNext() => _enumerator.MoveNext();

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator() => _enumerator;

		/// <inheritdoc/>
		readonly IEnumerator<PuzzleTile> IEnumerable<PuzzleTile>.GetEnumerator() => _enumerator;
	}
}
