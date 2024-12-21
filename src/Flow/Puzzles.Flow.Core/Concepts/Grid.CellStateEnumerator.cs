namespace Puzzles.Flow.Concepts;

public partial class Grid
{
	/// <summary>
	/// Represents an enumerator that can iterate on each cell states of the grid.
	/// </summary>
	/// <param name="_grid">The grid.</param>
	public struct CellStateEnumerator(Grid _grid) : IEnumerator<CellState>, IEnumerable<CellState>
	{
		/// <summary>
		/// Indicates the current index.
		/// </summary>
		private byte _index = unchecked((byte)-1);


		/// <inheritdoc/>
		public readonly CellState Current => _grid.GetState(_index);

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public readonly CellStateEnumerator GetEnumerator() => this;

		/// <inheritdoc/>
		public bool MoveNext() => ++_index < _grid.Size * _grid.Size;

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <inheritdoc/>
		readonly IEnumerator<CellState> IEnumerable<CellState>.GetEnumerator() => GetEnumerator();
	}
}
