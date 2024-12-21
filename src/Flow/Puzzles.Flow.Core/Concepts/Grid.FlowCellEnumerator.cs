namespace Puzzles.Flow.Concepts;

public partial class Grid
{
	/// <summary>
	/// Represents an enumerator type that can iterate on each flow cell.
	/// </summary>
	/// <param name="_grid">The grid.</param>
	public ref struct FlowPositionEnumerator(Grid _grid) : IEnumerator<FlowPosition>, IEnumerable<FlowPosition>
	{
		/// <summary>
		/// Indicates the backing enumerator.
		/// </summary>
		private SortedSet<FlowPosition>.Enumerator _enumerator = _grid._cells.GetEnumerator();


		/// <inheritdoc/>
		public readonly FlowPosition Current => _enumerator.Current;

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public readonly FlowPositionEnumerator GetEnumerator() => this;

		/// <inheritdoc/>
		public bool MoveNext() => _enumerator.MoveNext();

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator() => _grid._cells.GetEnumerator();

		/// <inheritdoc/>
		readonly IEnumerator<FlowPosition> IEnumerable<FlowPosition>.GetEnumerator() => _grid._cells.GetEnumerator();
	}
}
