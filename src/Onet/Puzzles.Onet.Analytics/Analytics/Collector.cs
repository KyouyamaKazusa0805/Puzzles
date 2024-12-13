namespace Puzzles.Onet.Analytics;

/// <summary>
/// Provides with a collector object.
/// </summary>
public sealed class Collector
{
	/// <summary>
	/// Try to find all possible steps appeared in the grid; if no steps found, an empty array will be returned.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>All matched items.</returns>
	public ReadOnlySpan<ItemMatch> Collect(Grid grid)
	{
		var result = new List<ItemMatch>();
		foreach (var (key, coordinates) in grid.ItemsSet)
		{
			if (key == Grid.EmptyKey)
			{
				continue;
			}

			foreach (var pair in coordinates.ToArray().AsReadOnlySpan().GetSubsets(2))
			{
				if (TryPair(grid, pair[0], pair[1], out var r))
				{
					result.Add(r);
				}
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Determine whether the current coordinate is out of bound.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="coordinate">The coordinate.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsCoordinateOutOfBound(Grid grid, Coordinate coordinate)
	{
		var (x, y) = coordinate;
		return x < 0 || x >= grid.RowsLength || y < 0 || y >= grid.ColumnsLength;
	}

	/// <summary>
	/// Determine whether two values are paired under the matching rule;
	/// if so, return <see langword="true"/> and return an <see cref="ItemMatch"/> object
	/// to parameter <paramref name="result"/> indicating the result details.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="coordinate1">Indicates the first coordinate.</param>
	/// <param name="coordinate2">Indicates the second coordinate.</param>
	/// <param name="result">Indicates the result match.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <exception cref="InvalidOperationException">Throws when the grid is too small (lower than 2x2).</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryPair(Grid grid, Coordinate coordinate1, Coordinate coordinate2, [NotNullWhen(true)] out ItemMatch? result)
	{
		if (grid.RowsLength < 2 || grid.ColumnsLength < 2)
		{
			throw new InvalidOperationException("The grid is too small.");
		}

		if (IsSameRowPaired(grid, coordinate1, coordinate2))
		{
			result = new(coordinate1, coordinate2);
			return true;
		}
		if (IsSameColumnPaired(grid, coordinate1, coordinate2))
		{
			result = new(coordinate1, coordinate2);
			return true;
		}
		if (IsTurningOncePaired(grid, coordinate1, coordinate2, out var interim))
		{
			result = new(coordinate1, coordinate2, interim);
			return true;
		}
		if (IsTurningTwicePaired(grid, coordinate1, coordinate2, out var interimPair))
		{
			result = new(coordinate1, coordinate2, interimPair);
			return true;
		}

		result = null;
		return false;
	}

	/// <summary>
	/// Determine whether two coordinates is in same row, and can be paired.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="coordinate1">Indicates the first coordinate.</param>
	/// <param name="coordinate2">Indicates the second coordinate.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	private bool IsSameRowPaired(Grid grid, Coordinate coordinate1, Coordinate coordinate2)
	{
		if (coordinate1 == coordinate2)
		{
			return false;
		}

		if (coordinate1.X != coordinate2.X)
		{
			return false;
		}

		var c1 = coordinate1.Y;
		var c2 = coordinate2.Y;
		var start = Math.Min(c1, c2);
		var end = Math.Max(c1, c2);
		for (var i = start + 1; i < end; i++)
		{
			if (Blocks(grid, coordinate1.X, i))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Determine whether two coordinates is in same column, and can be paired.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="coordinate1">Indicates the first coordinate.</param>
	/// <param name="coordinate2">Indicates the second coordinate.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	private bool IsSameColumnPaired(Grid grid, Coordinate coordinate1, Coordinate coordinate2)
	{
		if (coordinate1 == coordinate2)
		{
			return false;
		}

		if (coordinate1.Y != coordinate2.Y)
		{
			return false;
		}

		var r1 = coordinate1.X;
		var r2 = coordinate2.X;
		var start = Math.Min(r1, r2);
		var end = Math.Max(r1, r2);
		for (var i = start + 1; i < end; i++)
		{
			if (Blocks(grid, i, coordinate1.Y))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Determine whether two coordinates can be paired with one-time turning.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="coordinate1">Indicates the first coordinate.</param>
	/// <param name="coordinate2">Indicates the second coordinate.</param>
	/// <param name="interim">Indicates the interim on turning.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsTurningOncePaired(Grid grid, Coordinate coordinate1, Coordinate coordinate2, out Coordinate interim)
	{
		if (coordinate1 == coordinate2)
		{
			interim = default;
			return false;
		}

		var (cx, dy) = coordinate1;
		var (dx, cy) = coordinate2;
		if (!Blocks(grid, cx, cy))
		{
			if (IsSameRowPaired(grid, coordinate1, new(cx, cy)) && IsSameColumnPaired(grid, new(cx, cy), coordinate2))
			{
				interim = new(cx, cy);
				return true;
			}
		}
		if (!Blocks(grid, dx, dy))
		{
			if (IsSameColumnPaired(grid, coordinate1, new(dx, dy)) && IsSameRowPaired(grid, new(dx, dy), coordinate2))
			{
				interim = new(dx, dy);
				return true;
			}
		}

		interim = default;
		return false;
	}

	/// <summary>
	/// Determine whether two coordinates can be paired with two-time turning.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="coordinate1">Indicates the first coordinate.</param>
	/// <param name="coordinate2">Indicates the second coordinate.</param>
	/// <param name="interims">Indicates the interim coordinates on turning.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	private bool IsTurningTwicePaired(Grid grid, Coordinate coordinate1, Coordinate coordinate2, [NotNullWhen(true)] out Coordinate[]? interims)
	{
		if (coordinate1 == coordinate2)
		{
			interims = null;
			return false;
		}

		interims = [new(-2, -2), new(-2, -2)];
		for (var i = -1; i <= grid.RowsLength; i++)
		{
			for (var j = -1; j <= grid.ColumnsLength; j++)
			{
				if (i == coordinate1.X && j == coordinate1.Y || i == coordinate2.X && j == coordinate2.Y)
				{
					continue;
				}

				if (Blocks(grid, i, j))
				{
					continue;
				}

				if (checkType1(coordinate1, new(i, j), coordinate2, out var temp0, out var temp1))
				{
					if (temp0 == default && temp1 == default)
					{
						interims = [temp0, temp1];
					}
					else
					{
						var path0 = new ItemMatch(coordinate1, coordinate2, interims[0], interims[1]).Distance;
						var path1 = new ItemMatch(coordinate1, coordinate2, temp0, temp1).Distance;
						if (path1 < path0)
						{
							interims = [temp0, temp1];
						}
					}
				}

				if (checkType2(coordinate1, new(i, j), coordinate2, out temp0, out temp1))
				{
					if (temp0 == default && temp1 == default)
					{
						interims = [temp0, temp1];
					}
					else
					{
						var path0 = new ItemMatch(coordinate1, coordinate2, interims[0], interims[1]).Distance;
						var path1 = new ItemMatch(coordinate1, coordinate2, temp0, temp1).Distance;
						if (path1 < path0)
						{
							interims = [temp0, temp1];
						}
					}
				}
			}
		}

		if (interims is [(-2, -2), (-2, -2)])
		{
			interims = null;
			return false;
		}
		return true;


		bool checkType1(
			Coordinate coordinate1,
			Coordinate interim,
			Coordinate coordinate2,
			out Coordinate resultInterim1,
			out Coordinate resultInterim2
		)
		{
			if (IsTurningOncePaired(grid, coordinate1, interim, out resultInterim1)
				&& (IsSameRowPaired(grid, interim, coordinate2) || IsSameColumnPaired(grid, interim, coordinate2)))
			{
				resultInterim2 = interim;
				return true;
			}

			(resultInterim1, resultInterim2) = (default, default);
			return false;
		}

		bool checkType2(
			Coordinate coordinate1,
			Coordinate interim,
			Coordinate coordinate2,
			out Coordinate resultInterim1,
			out Coordinate resultInterim2
		)
		{
			if (IsTurningOncePaired(grid, interim, coordinate2, out resultInterim2)
				&& (IsSameRowPaired(grid, coordinate1, interim) || IsSameColumnPaired(grid, coordinate1, interim)))
			{
				resultInterim1 = interim;
				return true;
			}

			(resultInterim1, resultInterim2) = (default, default);
			return false;
		}
	}

	/// <summary>
	/// Determine whether the grid has blocked the specified coordinate.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="x">The row index.</param>
	/// <param name="y">The column index.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool Blocks(Grid grid, int x, int y) => Blocks(grid, new(x, y));

	/// <summary>
	/// Determine whether the grid has blocked the specified coordinate.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="coordinate">The coordinate.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool Blocks(Grid grid, Coordinate coordinate)
		=> !IsCoordinateOutOfBound(grid, coordinate) && grid[coordinate] != Grid.EmptyKey;
}
