namespace WaterSort.Concepts;

public partial class Tube
{
	/// <summary>
	/// Represents an enumerator type that can iterate on each element in the tube.
	/// </summary>
	/// <param name="items">Indicates the items.</param>
	public ref struct Enumerator(ReadOnlySpan<Color> items) : IEnumerator<Color>
	{
		/// <summary>
		/// Indicates the items.
		/// </summary>
		private readonly ReadOnlySpan<Color> _items = items;

		/// <summary>
		/// Indicates the index.
		/// </summary>
		private int _index = -1;


		/// <inheritdoc/>
		public readonly Color Current => _items[_index];

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext() => ++_index < _items.Length;

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();
	}
}
