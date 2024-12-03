namespace Puzzles.MahjongMatching.Concepts;

public partial class Layer
{
	/// <summary>
	/// Represents an enumerator type that can iterate on each tile.
	/// </summary>
	/// <param name="_layer">The layer.</param>
	public ref struct TileEnumerator(Layer _layer) : IEnumerator<Tile>, IEnumerable<Tile>
	{
		/// <summary>
		/// Indicates the current index.
		/// </summary>
		private int _index = -1;


		/// <inheritdoc/>
		public readonly Tile Current => _layer._tiles[_index];

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext() => ++_index < _layer.Count;

		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public readonly TileEnumerator GetEnumerator() => this;

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator() => _layer._tiles.GetEnumerator();

		/// <inheritdoc/>
		readonly IEnumerator<Tile> IEnumerable<Tile>.GetEnumerator() => _layer._tiles.GetEnumerator();
	}
}
