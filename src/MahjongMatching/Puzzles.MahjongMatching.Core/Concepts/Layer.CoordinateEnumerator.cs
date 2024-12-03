namespace Puzzles.MahjongMatching.Concepts;

public partial class Layer
{
	/// <summary>
	/// Represents an enumerator type that can iterate on each coordinate.
	/// </summary>
	/// <param name="_layer">The layer.</param>
	public ref struct CoordinateEnumerator(Layer _layer) : IEnumerator<Coordinate>, IEnumerable<Coordinate>
	{
		/// <summary>
		/// Indicates the current index.
		/// </summary>
		private int _index = -1;


		/// <inheritdoc/>
		public readonly Coordinate Current => _layer._coordinates[_index];

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext() => ++_index < _layer.Count;

		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public readonly CoordinateEnumerator GetEnumerator() => this;

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator() => _layer._coordinates.GetEnumerator();

		/// <inheritdoc/>
		readonly IEnumerator<Coordinate> IEnumerable<Coordinate>.GetEnumerator() => _layer._coordinates.GetEnumerator();
	}
}
