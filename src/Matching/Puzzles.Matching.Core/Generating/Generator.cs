namespace Puzzles.Matching.Generating;

/// <summary>
/// Represents a generator.
/// </summary>
public readonly ref struct Generator
{
	/// <summary>
	/// Indicates the local random number generator.
	/// </summary>
	private static readonly Random Rng = Random.Shared;


	/// <inheritdoc cref="ReadOnlySpan{T}.Equals"/>
	[Obsolete("This method is hidden on purpose.", true)]
	[DoesNotReturn]
	public override bool Equals([NotNullWhen(true)] object? obj) => throw new NotSupportedException();

	/// <inheritdoc cref="ReadOnlySpan{T}.GetHashCode"/>
	[Obsolete("This method is hidden on purpose.", true)]
	[DoesNotReturn]
	public override int GetHashCode() => throw new NotSupportedException();

	/// <inheritdoc cref="ReadOnlySpan{T}.Equals"/>
	[Obsolete("This method is hidden on purpose.", true)]
	[DoesNotReturn]
	public override string ToString() => throw new NotSupportedException();

	/// <summary>
	/// Generates a valid <see cref="Grid"/> that contains at least one step to be used.
	/// </summary>
	/// <param name="rows">The desired number of rows.</param>
	/// <param name="columns">The desired number of columns.</param>
	/// <param name="itemsCount">Indicates the number of items to be used in the grid.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Grid"/> result; or <see langword="null"/> if cancelled.</returns>
	/// <exception cref="InvalidOperationException">Throws when the argument is invalid.</exception>
	public Grid Generate(int rows, int columns, ItemIndex itemsCount, CancellationToken cancellationToken = default)
	{
		if (itemsCount << 1 > rows * columns)
		{
			throw new ArgumentException($"Argument '{nameof(itemsCount)}' is too much.", nameof(itemsCount));
		}

		while (true)
		{
			var cellStateTable = new BitArray(rows * columns);
			var array = new ItemIndex[rows * columns];
			array.AsSpan().Fill(Grid.EmptyKey);

			for (var i = 0; i < rows * columns;)
			{
				// If the current cell is already filled a value, skip the current cell.
				if (array[i] != Grid.EmptyKey)
				{
					i++;
					continue;
				}

				var itemKind = (ItemIndex)Rng.Next(0, itemsCount);
				var availableCells = b(cellStateTable, i);
				int chosenCell;
				do
				{
					chosenCell = availableCells[Rng.Next(0, availableCells.Length)];
				} while (cellStateTable[chosenCell] || chosenCell == i);

				// Make them a pair.
				cellStateTable[i] = cellStateTable[chosenCell] = true;
				array[i] = array[chosenCell] = itemKind;
			}

			// Check whether the grid contains all possible kinds of required items.
			var flags = new BitArray(itemsCount);
			foreach (var element in array)
			{
				flags[element] = true;
			}
			if (!flags.HasAllSet())
			{
				goto CheckCancellationToken;
			}

			// A grid is finished. Now check validity of the grid state.
			var result = new Grid(array, rows, columns);
			if (result.TryGetMatch(out _))
			{
				return result;
			}

		CheckCancellationToken:
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
		}
		return null!;


		static ReadOnlySpan<int> b(BitArray bitArray, int startIndex)
		{
			var result = new List<int>();
			for (var i = startIndex; i < bitArray.Length; i++)
			{
				if (!bitArray[i])
				{
					result.Add(i);
				}
			}
			return result.AsSpan();
		}
	}
}
