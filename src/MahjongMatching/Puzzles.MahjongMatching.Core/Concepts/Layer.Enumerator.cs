namespace Puzzles.MahjongMatching.Concepts;

public partial class Layer
{
	/// <summary>
	/// Represents an enumerator type that can iterate on each tile information.
	/// </summary>
	/// <param name="_layer">The layer.</param>
	public ref struct Enumerator(Layer _layer) : IEnumerator<LayerTile>
	{
		/// <summary>
		/// Indicates the current index.
		/// </summary>
		private int _index = -1;


		/// <inheritdoc/>
		public readonly LayerTile Current => _layer[_index];

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext() => ++_index < _layer.Count;

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();
	}
}
