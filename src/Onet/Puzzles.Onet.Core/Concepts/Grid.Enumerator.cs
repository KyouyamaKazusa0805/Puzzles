namespace Puzzles.Onet.Concepts;

public partial class Grid
{
	/// <summary>
	/// Represents an enumerator type that can iterate on each element.
	/// </summary>
	/// <param name="values">The values.</param>
	public ref struct Enumerator(ItemIndex[] values) : IEnumerator<ItemIndex>
	{
		/// <summary>
		/// Indicates the index.
		/// </summary>
		private int _index = -1;


		/// <inheritdoc/>
		public readonly ItemIndex Current => values[_index];

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext() => ++_index < values.Length;

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();
	}
}
