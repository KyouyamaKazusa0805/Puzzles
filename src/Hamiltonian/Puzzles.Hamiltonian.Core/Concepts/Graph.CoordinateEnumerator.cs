namespace Puzzles.Hamiltonian.Concepts;

public partial class Graph
{
	/// <summary>
	/// Represents an enumerator type that iterates on each coordinate of the cells used.
	/// </summary>
	public ref struct CoordinateEnumerator : IEnumerator<Coordinate>
	{
		/// <summary>
		/// Indicates the number of columns.
		/// </summary>
		private readonly int _columns;

		/// <summary>
		/// Indicates the sequence.
		/// </summary>
		private readonly BitArray _sequence;

		/// <summary>
		/// Indicates the index.
		/// </summary>
		private int _index;


		/// <summary>
		/// Initializes a <see cref="CoordinateEnumerator"/> instance.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		/// <param name="columns">The number of columns.</param>
		internal CoordinateEnumerator(BitArray sequence, int columns) => (_sequence, _index, _columns) = (sequence, -1, columns);


		/// <inheritdoc/>
		public Coordinate Current { get; private set; }

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public readonly CoordinateEnumerator GetEnumerator() => this;

		/// <inheritdoc/>
		public bool MoveNext()
		{
			while (++_index < _sequence.Count)
			{
				if (_sequence[_index])
				{
					Current = new(_index / _columns, _index % _columns);
					return true;
				}
			}
			return false;
		}

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();
	}
}
