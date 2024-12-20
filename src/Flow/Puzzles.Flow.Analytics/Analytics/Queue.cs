namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents a local queue union type.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal ref struct Queue
{
	/// <summary>
	/// Indicates the heap-based value.
	/// </summary>
	[FieldOffset(0)]
	public HeapBasedQueue HeapBased;

	/// <summary>
	/// Indicates the FIFO-based value.
	/// </summary>
	[FieldOffset(0)]
	public FifoBasedQueue FifoBased;
}
