namespace Puzzles.Flow.Concepts;

public partial class Grid
{
	/// <summary>
	/// Represents an enumerator type that can iterate on each flow cell.
	/// </summary>
	/// <param name="_grid">The grid.</param>
	public ref struct FlowCellEnumerator(Grid _grid) : IEnumerator<FlowCell>, IEnumerable<FlowCell>
	{
		/// <summary>
		/// Indicates the backing enumerator.
		/// </summary>
		private SortedSet<FlowCell>.Enumerator _enumerator = _grid._cells.GetEnumerator();


		/// <inheritdoc/>
		public readonly FlowCell Current => _enumerator.Current;

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public readonly FlowCellEnumerator GetEnumerator() => this;

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
		readonly IEnumerator<FlowCell> IEnumerable<FlowCell>.GetEnumerator() => _grid._cells.GetEnumerator();
	}
}
