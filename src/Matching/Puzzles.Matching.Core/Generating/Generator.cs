namespace Puzzles.Matching.Generating;

/// <summary>
/// Represents a generator.
/// </summary>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public readonly ref partial struct Generator
{
	/// <summary>
	/// Indicates the local random number generator.
	/// </summary>
	private static readonly Random Rng = Random.Shared;


	/// <summary>
	/// Generates a valid <see cref="Grid"/> that contains at least one step to be used.
	/// </summary>
	/// <param name="rows">The desired number of rows.</param>
	/// <param name="columns">The desired number of columns.</param>
	/// <param name="itemsCount">Indicates the number of items to be used in the grid.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Grid"/> result; or <see langword="null"/> if cancelled.</returns>
	/// <exception cref="ArgumentException">Throws when the argument is invalid.</exception>
	/// <exception cref="InvalidOperationException">
	/// Throws when the argument <paramref name="rows"/> and <paramref name="columns"/> are both odd.
	/// </exception>
	public Grid Generate(int rows, int columns, ItemIndex itemsCount, CancellationToken cancellationToken = default)
	{
		if (itemsCount << 1 > rows * columns)
		{
			throw new ArgumentException($"Argument '{nameof(itemsCount)}' is too much.", nameof(itemsCount));
		}
		if ((rows * columns & 1) != 0)
		{
			throw new InvalidOperationException("Size is invalid - it must an even number.");
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
				var availableCells = BitArrayToIndices(cellStateTable, i);
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
	}

	/// <summary>
	/// Generates a valid <see cref="Grid"/> that contains at least one step to be used;
	/// elements should be guaranteed to appear the specified number of times.
	/// </summary>
	/// <param name="rows">The desired number of rows.</param>
	/// <param name="columns">The desired number of columns.</param>
	/// <param name="items">Indicates items and its appearing times.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Grid"/> result; or <see langword="null"/> if cancelled.</returns>
	/// <exception cref="ArgumentException">Throws when the argument is invalid.</exception>
	public Grid Generate(int rows, int columns, IReadOnlyDictionary<ItemIndex, int> items, CancellationToken cancellationToken = default)
	{
		if (items.Values.Sum() != rows * columns)
		{
			throw new ArgumentException(
				$"Argument '{nameof(items)}' is mismatched due to its invalid number of items configured.",
				nameof(items)
			);
		}
		if ((rows * columns & 1) != 0)
		{
			throw new InvalidOperationException("Size is invalid - it must an even number.");
		}

		// Simply set values into the grid.
		var array = new ItemIndex[rows * columns];
		var index = 0;
		foreach (var (item, count) in items)
		{
			array.AsSpan()[index..(index + count)].Fill(item);
			index += count;
		}

		while (true)
		{
			// Randomly swaps the values thrice.
			// Because the shuffle function will keep all elements having opportunity to be swapped,
			// we may not shuffle more times to keep the array to be more random.
			for (var i = 0; i < 3; i++)
			{
				Rng.Shuffle(array);
			}

			// Check validity of the puzzle.
			var result = new Grid(array, rows, columns);
			if (result.TryGetMatch(out _))
			{
				return result;
			}

			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
		}
		return null!;
	}

	/// <summary>
	/// Generates a valid <see cref="Grid"/> that contains at least one step to be used;
	/// elements should be guaranteed to appear the specified number of times, in range list <paramref name="itemRanges"/>.
	/// </summary>
	/// <param name="rows">The desired number of rows.</param>
	/// <param name="columns">The desired number of columns.</param>
	/// <param name="itemRanges">Indicates items and its appearing times in range between corresponding limit.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Grid"/> result; or <see langword="null"/> if cancelled.</returns>
	/// <exception cref="ArgumentException">Throws when the argument is invalid.</exception>
	/// <exception cref="InvalidOperationException">Throws when the item ranges is invalid.</exception>
	public Grid Generate(int rows, int columns, IReadOnlyDictionary<ItemIndex, (int Min, int Max)> itemRanges, CancellationToken cancellationToken = default)
	{
		if ((rows * columns & 1) != 0)
		{
			throw new InvalidOperationException("Size is invalid - it must an even number.");
		}

		var sum = rows * columns;
		var items = new Dictionary<ItemIndex, int>(itemRanges.Count);
		foreach (var (item, (min, _)) in itemRanges)
		{
			items.Add(item, min);
		}
		for (var delta = sum - items.Values.Sum(); delta > 0; delta -= 2)
		{
			// Find an entry that can insert such value.
			var validItemsToInsert = new List<ItemIndex>();
			foreach (var (item, (_, max)) in itemRanges)
			{
				var current = items[item];
				if (max > current)
				{
					// Valid to add 2 elements.
					validItemsToInsert.Add(item);
				}
			}
			if (validItemsToInsert.Count == 0)
			{
				throw new InvalidOperationException("No entry to be used.");
			}

			// Randomly chosen an entry.
			var chosenItemIndex = validItemsToInsert[Rng.Next(0, validItemsToInsert.Count)];
			items[chosenItemIndex] += 2;
		}
		return Generate(rows, columns, items, cancellationToken);
	}

	/// <summary>
	/// Generates a valid <see cref="Grid"/> that contains at least one step to be used, with average value.
	/// </summary>
	/// <param name="rows">The desired number of rows.</param>
	/// <param name="columns">The desired number of columns.</param>
	/// <param name="itemsCount">Indicates the number of items to be used in the grid.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Grid"/> result; or <see langword="null"/> if cancelled.</returns>
	/// <exception cref="ArgumentException">Throws when the argument is invalid.</exception>
	/// <exception cref="InvalidOperationException">
	/// Throws when the argument <paramref name="rows"/> and <paramref name="columns"/> are both odd.
	/// </exception>
	public Grid GenerateAveraged(int rows, int columns, ItemIndex itemsCount, CancellationToken cancellationToken = default)
	{
		var averagedValue = rows * columns / itemsCount / 2 * 2;
		var realValuesCount = rows * columns;
		var delta = realValuesCount - averagedValue * itemsCount;
		var dictionary = new Dictionary<ItemIndex, int>();
		for (var i = (ItemIndex)0; i < itemsCount; i++)
		{
			dictionary.Add(i, averagedValue);
		}
		for (var (i, j) = (delta, (ItemIndex)0); i > 0; i -= 2, j++)
		{
			dictionary[j] += 2;
		}
		return Generate(rows, columns, dictionary, cancellationToken);
	}


	/// <summary>
	/// Converts bit array instance into valid array of indices.
	/// </summary>
	/// <param name="bitArray">The bit array.</param>
	/// <param name="startIndex">The start index.</param>
	/// <returns>The array.</returns>
	private static ReadOnlySpan<int> BitArrayToIndices(BitArray bitArray, int startIndex)
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
