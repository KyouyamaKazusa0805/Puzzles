namespace Puzzles.Hamiltonian.Concepts;

public partial class Graph
{
	/// <summary>
	/// Represents an enumerator type that iterates on each bit of the sequence.
	/// </summary>
	public ref struct Enumerator : IEnumerator<bool>
	{
		/// <summary>
		/// Indicates the sequence.
		/// </summary>
		private readonly BitArray _sequence;

		/// <summary>
		/// Indicates the index.
		/// </summary>
		private int _index;


		/// <summary>
		/// Initializes an <see cref="Enumerator"/> instance.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		internal Enumerator(BitArray sequence) => (_sequence, _index) = (sequence, -1);


		/// <inheritdoc/>
		public readonly bool Current => _sequence[_index];

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext() => ++_index < _sequence.Length;

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();
	}
}
