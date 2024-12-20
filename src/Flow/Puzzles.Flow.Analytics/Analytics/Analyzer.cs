#undef PERFORM_STABLE_SORT
#define USE_BEST_FIRST_SEARCH

namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents an analyzer.
/// </summary>
/// <remarks>
/// <para>
/// This implementation is just a copy from <see href="https://github.com/mzucker/flow_solver">this repository</see>,
/// but I change the algorithm into C# implementation instead,
/// with some minor updates (like using <see langword="ref"/> and <see langword="ref readonly"/> instead of naked pointer operations)
/// in order to make API more unified in the whole repository.
/// </para>
/// <para>
/// Also, this algorithm uses some logical techniques to reduce complexity of brute forces.
/// For more information about checking dead-ends and some other techniques,
/// please visit <see href="https://mzucker.github.io/2016/08/28/flow-solver.html">this link</see>.
/// </para>
/// <para>
/// You may know that a flow free problem may be solved by reducing it to a 3-SAT problem,
/// but this algorithm doesn't use this algorithm. For more information about 3-SAT solving,
/// please visit <see href="https://www.youtube.com/watch?v=_2A3j9hSqnY">this YouTube video</see>.
/// </para>
/// </remarks>
public sealed unsafe class Analyzer
{
	/// <summary>
	/// Indicates the invalid position value. This value is used as placeholder.
	/// </summary>
	public const byte InvalidPosition = 0xFF;

	/// <summary>
	/// Indicates the maximum size of the grid. This value is a constant and cannot be modified due to massive complexity.
	/// Please note that the maximum value is not 16,
	/// because the coordinate at (15, 15) is a reserved coordinate to represent information "invalid",
	/// which cooresponds to field <see cref="InvalidPosition"/>.
	/// </summary>
	/// <seealso cref="InvalidPosition"/>
	public const int MaxSize = 15;

	/// <summary>
	/// Indicates the maximum length of cells available in the grid. The value is equal to 239.
	/// </summary>
	public const int MaxGridCellsCount = (MaxSize + 1) * MaxSize - 1;

	/// <summary>
	/// Indicates the maximum number of supported colors. This value is a constant and cannot be modified due to massive complexity.
	/// </summary>
	public const int MaxSupportedColorsCount = 16;

	/// <summary>
	/// Indicates the valid color characters.
	/// </summary>
	private const string ValidCharacters = "0123456789ABCDEF";


	/// <summary>
	/// Indicates all directions.
	/// </summary>
	private static readonly Direction[] Directions = [Direction.Left, Direction.Right, Direction.Up, Direction.Down];


	/// <summary>
	/// <para>
	/// Indicates whether analyzer ignores adjacent pairs of cells are start point and end point respectively.
	/// For example, cells (1, 1) and (1, 2) are an adjacent pair of cells. If (1, 1) is the start point and (1, 2) is the end point,
	/// they will be "adjacent-touched" with each other.
	/// </para>
	/// <para>
	/// If this option is set to <see langword="false"/> (enabled checking touchness), the complexity will be much massive
	/// to the case setting this option to <see langword="true"/> (disabled checking touchness);
	/// however, adjacent-touched cases will be checked, so the solution to a puzzle can be checked exhaustively.
	/// </para>
	/// <para>By default the value is <see langword="true"/> (disabled).</para>
	/// </summary>
	public bool DisableAdjacentTouchness { get; set; } = true;

	/// <summary>
	/// Indicates whether analyzer will check on stranded cases.
	/// </summary>
	public bool CheckStrandedCases { get; set; } = true;

	/// <summary>
	/// Indicates whether analyzer will check on deadend cases.
	/// </summary>
	public bool CheckDeadendCases { get; set; } = true;

	/// <summary>
	/// Indicates whether analyzer will search for outside-in cases.
	/// </summary>
	public bool SearchOutsideIn { get; set; } = true;

	/// <summary>
	/// Indicates whether analyzer will search fast forwardly.
	/// </summary>
	public bool SearchFastForward { get; set; } = true;

	/// <summary>
	/// Indicates whether analyzer will explore penalized cases.
	/// </summary>
	public bool PenalizeExploration { get; set; }

	/// <summary>
	/// Indicates whether analyzer will automatically adjust colors in searching experience.
	/// </summary>
	public bool ReorderColors { get; set; } = true;

	/// <summary>
	/// Indicates whether analyzer will automatically adjust colors via constrainted priority.
	/// </summary>
	public bool ReorderOnMostConstrained { get; set; } = true;

	/// <summary>
	/// Indicates whether analyzer will force the first color as start.
	/// </summary>
	public bool ForcesFirstColor { get; set; } = true;

	/// <summary>
	/// Indicates whether analyzer will randomize colors.
	/// </summary>
	public bool RandomOrdering { get; set; }

	/// <summary>
	/// Indicates the maximum memory usage in mega-bytes.
	/// </summary>
	public double MaxMemoryUsage { get; set; } = 128;

	/// <summary>
	/// Indicates the number of bottleneck limit.
	/// </summary>
	public int BottleneckLimit { get; set; } = 3;

	/// <summary>
	/// Indicates the number of maximum nodes can be reached.
	/// </summary>
	public int MaxNodes { get; set; }


	/// <summary>
	/// Try to analyze the grid, and return an <see cref="AnalysisResult"/> instance indicating the solved result.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <exception cref="InvalidOperationException">Throws when the string is invalid.</exception>
	public AnalysisResult Analyze(Grid grid)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		if (!tryLoadPuzzle(grid.ToString(), grid.Size, out var gridInfo, out var state))
		{
			stopwatch.Stop();
			return new(grid) { IsSolved = false, FailedReason = FailedReason.Invalid, ElapsedTime = stopwatch.Elapsed };
		}

		OrderColors(ref gridInfo, in state, null);

		var result = Search<
#if USE_BEST_FIRST_SEARCH
			HeapBasedQueue
#else
			FifoBasedQueue
#endif
		>(in gridInfo, ref state, out var elapsed, out var nodes, out var finalState);
		stopwatch.Stop();

		return result == SearchingResult.Success
			? AnalysisResult.Create(grid, in gridInfo, in finalState, stopwatch.Elapsed)
			: new(grid) { IsSolved = false, FailedReason = (FailedReason)(int)result, ElapsedTime = elapsed };


		bool tryLoadPuzzle(string gridString, int size, out GridAnalyticsInfo gridInfo, out GridInterimState state)
		{
			state = default;
			new Span<byte>(Unsafe.AsPointer(ref state.Positions[0]), MaxSupportedColorsCount).Fill(byte.MaxValue);
			state.LastColor = MaxSupportedColorsCount;

			gridInfo = default;
			new Span<Color>(Unsafe.AsPointer(ref gridInfo.ColorTable[0]), 1 << 7).Fill(Color.MaxValue);
			new Span<byte>(Unsafe.AsPointer(ref gridInfo.InitPositions[0]), MaxSupportedColorsCount).Fill(byte.MaxValue);
			new Span<byte>(Unsafe.AsPointer(ref gridInfo.GoalPositions[0]), MaxSupportedColorsCount).Fill(byte.MaxValue);

			gridInfo.Size = size;

			var y = (byte)0;
			var lines = gridString.Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			while (gridInfo.Size == 0 || y < gridInfo.Size)
			{
				var line = lines[y];
				for (var x = (byte)0; x < gridInfo.Size; x++)
				{
					var c = char.ToUpper(line[x]);
					if (!ValidCharacters.Contains(c))
					{
						state.FreeCellsCount++;
						continue;
					}

					var pos = Position.GetPositionFromCoordinate(x, y);
					Debug.Assert(pos < MaxGridCellsCount);

					var color = gridInfo.ColorTable[c];
					if (color >= gridInfo.ColorsCount)
					{
						color = gridInfo.ColorsCount;
						if (gridInfo.ColorsCount == MaxSupportedColorsCount)
						{
							// Too many colors.
							goto ReturnFalse;
						}

						var id = c is >= 'A' and <= 'F' ? c - 'A' + 10 : c - '0';
						if (id < 0 || id >= MaxSupportedColorsCount)
						{
							// Color value is invalid or not supported.
							goto ReturnFalse;
						}

						gridInfo.ColorIds[color] = id;
						gridInfo.ColorOrder[color] = color;

						gridInfo.ColorsCount++;
						gridInfo.ColorTable[c] = color;
						gridInfo.InitPositions[color] = state.Positions[color] = pos;
						state.Cells[pos] = Cell.Create(CellState.Start, color, 0);
					}
					else
					{
						if (gridInfo.GoalPositions[color] != InvalidPosition)
						{
							// Multiple endpoints found.
							goto ReturnFalse;
						}

						gridInfo.GoalPositions[color] = pos;
						state.Cells[pos] = Cell.Create(CellState.End, color, 0);
					}
				}

				y++;
			}

			if (gridInfo.ColorsCount == 0)
			{
				// The grid is empty.
				goto ReturnFalse;
			}

			for (var color = (Color)0; color < gridInfo.ColorsCount; color++)
			{
				if (gridInfo.GoalPositions[color] == InvalidPosition)
				{
					// Such color contains start point but not for end point.
					goto ReturnFalse;
				}

				if (SearchOutsideIn)
				{
					var initDistance = gridInfo.GetWallDistance(gridInfo.InitPositions[color]);
					var goalDistance = gridInfo.GetWallDistance(gridInfo.GoalPositions[color]);
					if (goalDistance < initDistance)
					{
						// Swap.
						(gridInfo.InitPositions[color], gridInfo.GoalPositions[color]) = (gridInfo.GoalPositions[color], gridInfo.InitPositions[color]);

						state.Cells[gridInfo.InitPositions[color]] = Cell.Create(CellState.Start, color, 0);
						state.Cells[gridInfo.GoalPositions[color]] = Cell.Create(CellState.End, color, 0);
						state.Positions[color] = gridInfo.InitPositions[color];
					}
				}
			}
			return true;

		ReturnFalse:
			gridInfo = default;
			state = default;
			return false;
		}
	}

	/// <summary>
	/// Add the current color bit flag to the regions adjacent to the current or goal position.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="resultMap">The result map.</param>
	/// <param name="pos">The position.</param>
	/// <param name="cFlag">The flag.</param>
	/// <param name="resultFlags">The result flags.</param>
	private void AddColor(
		ref readonly GridAnalyticsInfo grid,
		Span<byte> resultMap,
		byte pos,
		ColorMask cFlag,
		Span<ColorMask> resultFlags
	)
	{
		foreach (var direction in Directions)
		{
			var neighborPos = Position.GetOffsetPosition(in grid, pos, direction);
			if (neighborPos != InvalidPosition)
			{
				// Find out what region it is in.
				// If it is in a valid region, we should add this color to the region.
				var neighborRegion = resultMap[neighborPos];
				if (neighborRegion != InvalidPosition)
				{
					resultFlags[neighborRegion] |= cFlag;
				}
			}
		}
	}

	/// <summary>
	/// Order colors.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="userOrder">User order.</param>
	private void OrderColors(ref GridAnalyticsInfo grid, ref readonly GridInterimState state, string? userOrder)
	{
		if (RandomOrdering)
		{
			var rng = Random.Shared;
			for (var i = grid.ColorsCount - 1; i > 0; i--)
			{
				var j = rng.Next(0, i + 1);
				(grid.ColorOrder[i], grid.ColorOrder[j]) = (grid.ColorOrder[j], grid.ColorOrder[i]);
			}
			return;
		}

		var cf = (stackalloc ColorFeature[MaxSupportedColorsCount]);
		cf.Clear();

		for (var color = (Color)0; color < grid.ColorsCount; color++)
		{
			cf[color].Index = color;
			cf[color].UserIndex = MaxSupportedColorsCount;
		}

		if (ReorderColors)
		{
			for (var color = (Color)0; color < grid.ColorsCount; color++)
			{
				var (xf, xs) = (0, 0);
				var (yf, ys) = (0, 0);
				foreach (var isFirst in (true, false))
				{
					ref var p = ref isFirst ? ref cf[color].WallDistance.First : ref cf[color].WallDistance.Second;
					Position.GetCoordinateFromPosition(state.Positions[color], out isFirst ? ref xf : ref xs, out isFirst ? ref yf : ref ys);
					p = grid.GetWallDistance(isFirst ? xf : xs, isFirst ? yf : ys);
				}

				var dx = Abs(xs - xf);
				var dy = Abs(ys - yf);
				cf[color].MinDistance = dx + dy;
			}
		}

		if (userOrder is not null)
		{
			var k = 0;
			foreach (var c in userOrder.ToUpper())
			{
				var color = c < 127 ? grid.ColorTable[c] : MaxSupportedColorsCount;
				if (color >= grid.ColorsCount)
				{
					throw new InvalidOperationException("The current color specified in user list is not used in the puzzle.");
				}
				if (cf[color].UserIndex < grid.ColorsCount)
				{
					throw new InvalidOperationException("The current color specified is already used.");
				}
				cf[color].UserIndex = k++;
			}
			grid.IsUserOrdered = true;
		}

#if PERFORM_STABLE_SORT
		bubbleSort(cf[..grid.ColorsCount], ColorFeature.Compare);
#else
		cf[..grid.ColorsCount].Sort(ColorFeature.Compare);
#endif
		for (var i = 0; i < grid.ColorsCount; i++)
		{
			grid.ColorOrder[i] = cf[i].Index;
		}


#if PERFORM_STABLE_SORT
		static void bubbleSort<T>(Span<T> cf, Comparison<T> comparison)
		{
			for (var i = 0; i < cf.Length - 1; i++)
			{
				for (var j = 0; j < cf.Length - 1 - i; j++)
				{
					if (comparison(cf[j], cf[j + 1]) >= 0)
					{
						(cf[j], cf[j + 1]) = (cf[j + 1], cf[j]);
					}
				}
			}
		}
#endif
	}

	/// <summary>
	/// To update node costs.
	/// </summary>
	/// <param name="node">The node.</param>
	/// <param name="actionCost">The action cost.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateNodeCosts(ref TreeNode node, int actionCost)
	{
		node.CostToCome = node.Parent != null ? node.Parent->CostToCome + actionCost : 0;
		node.CostToGo = node.State.FreeCellsCount;
	}

	/// <summary>
	/// Determine whether the current grid can move.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The current state.</param>
	/// <param name="color">The color.</param>
	/// <param name="direction">The direction.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="grid"/> or <paramref name="color"/> is invalid.
	/// </exception>
	private bool CanMove(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state, Color color, Direction direction)
	{
		ArgumentOutOfRangeException.ThrowIfNotEqual(color == Color.MaxValue || color < grid.ColorsCount, true);
		ArgumentOutOfRangeException.ThrowIfNotEqual(state.CompletedMask >> color & 1, 0);

		// Get current position x and y.
		Position.GetCoordinateFromPosition(state.Positions[color], out var currentX, out var currentY);

		// Get new x and y.
		var delta = direction.GetDirectionDelta();
		var newX = (byte)(currentX + delta[0]);
		var newY = (byte)(currentY + delta[1]);

		// If outside bounds, not equal.
		if (!Position.IsCoordinateValid(in grid, newX, newY))
		{
			return false;
		}

		// Create a new position.
		var newPosition = Position.GetPositionFromCoordinate(newX, newY);
		Debug.Assert(newPosition < MaxGridCellsCount);

		if (!DisableAdjacentTouchness && newPosition == grid.GoalPositions[color])
		{
			return true;
		}

		if (state.Cells[newPosition] != 0)
		{
			// Must be empty.
			return false;
		}

		if (DisableAdjacentTouchness)
		{
			// All puzzles are designed so that a new path segment is adjacent to at most one path segment of the same color -
			// the predecessor to the new segment.
			// We check this by iterating over the neighbors.
			foreach (var neighborDirection in Directions)
			{
				// Assemble position.
				var neighborPosition = Position.GetOffsetPosition(in grid, newX, newY, neighborDirection);
				if (neighborPosition != InvalidPosition && state.Cells[neighborPosition] != 0
					&& neighborPosition != state.Positions[color]
					&& neighborPosition != grid.GoalPositions[color]
					&& Cell.GetCellColor(state.Cells[neighborPosition]) == color)
				{
					return false;
				}
			}
		}

		// Valid.
		return true;
	}

	/// <summary>
	/// Indicates the current position is deadend.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="pos">The position.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	private bool IsDeadend(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state, byte pos)
	{
		Debug.Assert(pos != InvalidPosition && state.Cells[pos] == 0);

		Position.GetCoordinateFromPosition(pos, out var x, out var y);
		var freeCount = 0;
		foreach (var direction in Directions)
		{
			var neighborPos = Position.GetOffsetPosition(in grid, x, y, direction);
			if (neighborPos == InvalidPosition)
			{
				continue;
			}

			if (state.Cells[neighborPos] == 0)
			{
				freeCount++;
				continue;
			}

			for (var color = 0; color < grid.ColorsCount; color++)
			{
				if ((state.CompletedMask >> color & 1) != 0)
				{
					continue;
				}

				if (neighborPos == state.Positions[color] || neighborPos == grid.GoalPositions[color])
				{
					freeCount++;
				}
			}
		}
		return freeCount <= 1;
	}

	/// <summary>
	/// Check for deadend regions of free-space where there is no way to put an active path into and out of it.
	/// Any free-space node which has only one free neighbor represents such a deadend.
	/// For the purposes of this check, current and goal positions coutn as "free".
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	private bool CheckDeadends(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state)
	{
		var color = state.LastColor;
		if (color >= grid.ColorsCount && color != Color.MaxValue)
		{
			return false;
		}

		var currentPos = state.Positions[color];
		Position.GetCoordinateFromPosition(currentPos, out var x, out var y);
		foreach (var direction in Directions)
		{
			var neighborPos = Position.GetOffsetPosition(in grid, x, y, direction);
			if (neighborPos != InvalidPosition && state.Cells[neighborPos] == 0 && IsDeadend(in grid, in state, neighborPos))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determine whether the puzzle is forced.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="color">The color.</param>
	/// <param name="pos">The position.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	private bool IsForced(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state, Color color, byte pos)
	{
		var freeCount = 0;
		var otherEndpointsCount = 0;
		foreach (var direction in Directions)
		{
			var neighborPos = Position.GetOffsetPosition(in grid, pos, direction);
			if (neighborPos == InvalidPosition || neighborPos == state.Positions[color])
			{
				continue;
			}

			if (state.Cells[neighborPos] == 0)
			{
				freeCount++;
				continue;
			}

			for (var otherColor = 0; otherColor < grid.ColorsCount; otherColor++)
			{
				if (otherColor == color || (state.CompletedMask >> otherColor & 1) != 0)
				{
					continue;
				}

				if (neighborPos == state.Positions[otherColor] || neighborPos == grid.GoalPositions[otherColor])
				{
					otherEndpointsCount++;
				}
			}
		}
		return freeCount == 1 && otherEndpointsCount == 0;
	}

	/// <summary>
	/// Determine whether the specified coordinate value is free.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="x">The coordinate x value.</param>
	/// <param name="y">The coordinate y value.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	private bool IsFree(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state, byte x, byte y)
		=> Position.IsCoordinateValid(in grid, x, y) && state.Cells[Position.GetPositionFromCoordinate(x, y)] == 0;

	/// <summary>
	/// Find a forced color.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="forcedColor">The forced color.</param>
	/// <param name="forcedDirection">The forced direction.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	private bool FindForced(
		ref readonly GridAnalyticsInfo grid,
		ref readonly GridInterimState state,
		out Color forcedColor,
		out Direction forcedDirection
	)
	{
		// If there is a free-space next to an endpoint and the free-space has only one free neighbor,
		// we must extend the endpoint into it.
		for (var i = 0; i < grid.ColorsCount; i++)
		{
			var color = grid.ColorOrder[i];
			if ((state.CompletedMask >> color & 1) != 0)
			{
				continue;
			}

			var freeDirection = (Direction)byte.MaxValue;
			var freeCount = 0;
			foreach (var direction in Directions)
			{
				var neighborPos = Position.GetOffsetPosition(in grid, state.Positions[color], direction);
				if (neighborPos == InvalidPosition)
				{
					continue;
				}

				if (state.Cells[neighborPos] == 0)
				{
					freeDirection = direction;
					freeCount++;

					if (IsForced(in grid, in state, color, neighborPos))
					{
						forcedColor = color;
						forcedDirection = direction;
						return true;
					}
				}
			}
		}

		forcedColor = default;
		forcedDirection = default;
		return false;
	}

	/// <summary>
	/// <para>Perform connected components analysis on game board.</para>
	/// <para>
	/// This is a 2-pass operation: one to build and merge the disjoint-set data structures,
	/// and another to re-index them so each unique region of free space gets its own index, starting at zero.
	/// Returns the number of freespace regions.
	/// </para>
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="resultMap">The map.</param>
	/// <returns>The regions.</returns>
	private byte BuildRegions(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state, Span<byte> resultMap)
	{
		var regions = (stackalloc Region[MaxGridCellsCount]);
		regions.Clear();

		// Build regions.
		for (var y = (byte)0; y < grid.Size; y++)
		{
			for (var x = (byte)0; x < grid.Size; x++)
			{
				var pos = Position.GetPositionFromCoordinate(x, y);
				if (state.Cells[pos] != 0)
				{
					regions[pos] = Region.Create(InvalidPosition);
					continue;
				}

				regions[pos] = Region.Create(pos);
				if (x != 0)
				{
					var pl = Position.GetPositionFromCoordinate((byte)(x - 1), y);
					if (state.Cells[pl] == 0)
					{
						Region.Unite(regions, pos, pl);
					}
				}
				if (y != 0)
				{
					var pu = Position.GetPositionFromCoordinate(x, (byte)(y - 1));
					if (state.Cells[pu] == 0)
					{
						Region.Unite(regions, pos, pu);
					}
				}
			}
		}

		var resultLookup = (stackalloc byte[MaxGridCellsCount]);
		var result = (byte)0;

		resultLookup.Fill(byte.MaxValue);
		resultMap.Fill(byte.MaxValue);

		// Order regions.
		for (var y = (byte)0; y < grid.Size; y++)
		{
			for (var x = (byte)0; x < grid.Size; x++)
			{
				var pos = Position.GetPositionFromCoordinate(x, y);
				var root = Region.Find(regions, pos);
				if (root != InvalidPosition)
				{
					if (resultLookup[root] == InvalidPosition)
					{
						resultLookup[root] = result++;
					}
					resultMap[pos] = resultLookup[root];
				}
				else
				{
					resultMap[pos] = InvalidPosition;
				}
			}
		}
		return result;
	}

	/// <summary>
	/// Pick the next color to move deterministically.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <returns>The next color chosen.</returns>
	private byte GetNextMoveColor(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state)
	{
		var lastColor = state.LastColor;
		if (lastColor < grid.ColorsCount && (state.CompletedMask >> lastColor & 1) == 0)
		{
			return lastColor;
		}

		if (!grid.IsUserOrdered && ReorderOnMostConstrained)
		{
			var bestColor = Color.MaxValue;
			var bestFree = 4;

			for (var i = 0; i < grid.ColorsCount; i++)
			{
				var color = grid.ColorOrder[i];
				if ((state.CompletedMask >> color & 1) != 0)
				{
					// Already completed.
					continue;
				}

				var freeCount = GetFreeCoordinatesCount(in grid, in state, state.Positions[color]);
				if (freeCount < bestFree)
				{
					bestFree = freeCount;
					bestColor = color;
				}
			}

			Debug.Assert(bestColor == Color.MaxValue || bestColor < grid.ColorsCount);
			return bestColor;
		}
		else
		{
			for (var i = 0; i < grid.ColorsCount; i++)
			{
				var color = grid.ColorOrder[i];
				if ((state.CompletedMask >> color & 1) != 0)
				{
					continue;
				}
				return color;
			}
			throw new InvalidProgramException();
		}
	}

	/// <summary>
	/// Make a valid move.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="color">The color.</param>
	/// <param name="direction">The direction.</param>
	/// <param name="forced">Indicates whether the move is forced.</param>
	/// <returns>An <see cref="int"/> result.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the <paramref name="color"/> specified is exceeded the limit.
	/// </exception>
	private int MakeMove(ref readonly GridAnalyticsInfo grid, ref GridInterimState state, Color color, Direction direction, bool forced)
	{
		// Make sure the color is valid.
		ArgumentOutOfRangeException.ThrowIfNotEqual(color == Color.MaxValue || color < grid.ColorsCount, true);

		// Update the cell with the new cell value.
		var move = Cell.Create(CellState.Path, color, direction);

		// Get the current x and y coordinate.
		Position.GetCoordinateFromPosition(state.Positions[color], out var currentX, out var currentY);

		// Assemble new x and y value.
		var delta = direction.GetDirectionDelta();
		var newX = (byte)(currentX + delta[0]);
		var newY = (byte)(currentY + delta[1]);

		// Make sure the value is valid.
		Debug.Assert(Position.IsCoordinateValid(in grid, newX, newY));

		// Make position.
		var newPosition = Position.GetPositionFromCoordinate(newX, newY);
		Debug.Assert(newPosition < MaxGridCellsCount);

		if (!DisableAdjacentTouchness && newPosition == grid.GoalPositions[color])
		{
			state.Cells[grid.GoalPositions[color]] = Cell.Create(CellState.End, color, direction);
			state.CompletedMask |= (ColorMask)(1 << color);
			return 0;
		}

		// Make sure it's empty.
		Debug.Assert(state.Cells[newPosition] == 0);

		// Update cells and new position.
		state.Cells[newPosition] = move;
		state.Positions[color] = newPosition;
		state.FreeCellsCount--;

		state.LastColor = color;

		var actionCost = 1;
		var goalDirection = (Direction)byte.MaxValue;
		if (DisableAdjacentTouchness)
		{
			foreach (var neighborDirection in Directions)
			{
				if (Position.GetOffsetPosition(in grid, newX, newY, neighborDirection) == grid.GoalPositions[color])
				{
					goalDirection = neighborDirection;
					break;
				}
			}
		}

		if (goalDirection != (Direction)byte.MaxValue)
		{
			state.Cells[grid.GoalPositions[color]] = Cell.Create(CellState.End, color, goalDirection);
			state.CompletedMask |= (ColorMask)(1 << color);
			actionCost = 0;
		}
		else
		{
			var freeCount = GetFreeCoordinatesCount(in grid, in state, newX, newY);
			if (PenalizeExploration && freeCount == 2)
			{
				actionCost = 2;
			}
		}

		return forced ? actionCost = 0 : actionCost;
	}

	/// <summary>
	/// Get the number of free spaces around coordinate x and y.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="x">The x coordinate value.</param>
	/// <param name="y">The y coordinate value.</param>
	/// <returns>An <see cref="int"/> value indicating the number.</returns>
	private int GetFreeCoordinatesCount(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state, int x, int y)
	{
		var result = 0;
		foreach (var direction in Directions)
		{
			var neighborPosition = Position.GetOffsetPosition(in grid, x, y, direction);
			if (neighborPosition != InvalidPosition && state.Cells[neighborPosition] == 0)
			{
				result++;
			}
		}
		return result;
	}

	/// <summary>
	/// Get the number of free spaces around coordinate x and y.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="position">The position.</param>
	/// <returns>An <see cref="int"/> value indicating the number.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetFreeCoordinatesCount(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state, byte position)
	{
		Position.GetCoordinateFromPosition(position, out var x, out var y);
		return GetFreeCoordinatesCount(in grid, in state, x, y);
	}

	/// <summary>
	/// Identify bottlenecks on narrow regions - created by a recent move of a color,
	/// then see if it renders the puzzle unsolvable.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <returns>The value.</returns>
	private int CheckBottleneck(ref readonly GridAnalyticsInfo grid, ref readonly GridInterimState state)
	{
		var color = state.LastColor;
		if (color >= grid.ColorsCount && color != Color.MaxValue)
		{
			return 0;
		}

		var pos = state.Positions[color];
		Position.GetCoordinateFromPosition(pos, out var x0, out var y0);
		foreach (var direction in Directions)
		{
			var delta = direction.GetDirectionDelta();
			var dx = delta[0];
			var dy = delta[1];
			var x1 = (byte)(x0 + dx);
			var y1 = (byte)(y0 + dy);
			if (IsFree(in grid, in state, x1, y1))
			{
				for (var n = 0; n < BottleneckLimit; n++)
				{
					var x2 = (byte)(x1 + dx);
					var y2 = (byte)(y1 + dy);
					if (!IsFree(in grid, in state, x2, y2))
					{
						var r = checkChokepoint(in grid, in state, color, direction, n + 1);
						if (r != 0)
						{
							return r;
						}
						break;
					}
					x1 = x2;
					y1 = y2;
				}
			}
		}
		return 0;


		ColorMask checkChokepoint(
			ref readonly GridAnalyticsInfo grid,
			ref readonly GridInterimState state,
			Color color,
			Direction direction,
			int n
		)
		{
			var copy = state;
			for (var i = 0; i < n; i++)
			{
				MakeMove(in grid, ref copy, color, direction, true);
			}

			// Build new region map.
			var resultMap = (stackalloc byte[MaxGridCellsCount]);
			var resultCount = BuildRegions(in grid, in state, resultMap);

			// See if we are stranded.
			return GetStrandedColors(in grid, in state, resultCount, resultMap, color, n + 1);
		}
	}

	/// <summary>
	/// Check the results of the connected-component analysis to make sure that
	/// every color can get solved and no free-space is isolated.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="resultCount">The result count.</param>
	/// <param name="resultMap">The result map.</param>
	/// <param name="chokePointColor">The choke point color.</param>
	/// <param name="maxStranded">The maximum stranded.</param>
	/// <returns>The mask of colors stranded.</returns>
	private ColorMask GetStrandedColors(
		ref readonly GridAnalyticsInfo grid,
		ref readonly GridInterimState state,
		int resultCount,
		Span<byte> resultMap,
		int chokePointColor,
		int maxStranded
	)
	{
		// For each region, we have bit flags to track whether current or goal position
		// is adjacent to the region. These get init'ed to 0.
		var currentResultFlags = (stackalloc ColorMask[resultCount]);
		var goalResultFlags = (stackalloc ColorMask[resultCount]);
		currentResultFlags.Clear();
		goalResultFlags.Clear();

		var strandedCount = 0;
		var result = (ColorMask)0;
		var forChokePoint = chokePointColor < grid.ColorsCount;

		// For each color, figure out which regions touch its current and goal position,
		// and make sure no color is stranded.
		for (var color = 0; color < grid.ColorsCount; color++)
		{
			var cFlag = (ColorMask)(1 << color);

			// No worries if completed.
			if ((state.CompletedMask & cFlag) != 0 || color == chokePointColor)
			{
				continue;
			}

			AddColor(in grid, resultMap, state.Positions[color], cFlag, currentResultFlags);
			AddColor(in grid, resultMap, grid.GoalPositions[color], cFlag, goalResultFlags);

			if (!DisableAdjacentTouchness)
			{
				var delta = Abs(state.Positions[color] - grid.GoalPositions[color]);
				if (delta is 1 or MaxSupportedColorsCount)
				{
					// Adjacent.
					continue;
				}
			}

			// Ensure this color is not stranded -
			// at least region must touch each non-completed color for both current and goal.
			byte r;
			for (r = 0; r < resultCount; r++)
			{
				// See if this region touches the color.
				if ((currentResultFlags[r] & cFlag) != 0 && (goalResultFlags[r] & cFlag) != 0)
				{
					break;
				}
			}

			// There was no region that touched both current and goal, unsolvable from here.
			if (r == resultCount)
			{
				result |= cFlag;
				if (++strandedCount >= maxStranded)
				{
					return result;
				}
			}
		}

		if (!forChokePoint)
		{
			// For each region, make sure that there is at least one color
			// whose current and goal positions touch it;
			// otherwise, the region is stranded.
			for (var r = 0; r < resultCount; r++)
			{
				if ((currentResultFlags[r] & goalResultFlags[r]) == 0)
				{
					return -1;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// The core method to search, performing A* or BFS (best-first search) algorithm to find a solution.
	/// </summary>
	/// <typeparam name="TQueue">The type of the queue to be operated.</typeparam>
	/// <param name="grid">The grid.</param>
	/// <param name="initState">The init state.</param>
	/// <param name="elapsed">The elapsed time.</param>
	/// <param name="nodes">The nodes created.</param>
	/// <param name="finalState">The final state.</param>
	/// <returns>A <see cref="SearchingResult"/> value.</returns>
	private SearchingResult Search<TQueue>(
		ref readonly GridAnalyticsInfo grid,
		ref GridInterimState initState,
		out TimeSpan elapsed,
		out int nodes,
		out GridInterimState finalState
	)
		where TQueue : struct, IAnalysisQueue<TQueue>, allows ref struct
	{
		var nodeMemoryManager = default(TreeNodeMemoryManager)!;
		var queue = default(TQueue);
		try
		{
			var maxNodes = MaxNodes != 0 ? MaxNodes : (int)Floor(MaxMemoryUsage * (1 << 20) / sizeof(TreeNode));
			nodeMemoryManager = new(maxNodes);
			scoped ref var root = ref nodeMemoryManager.CreateNode(in Unsafe.NullRef<TreeNode>(), ref initState);
			UpdateNodeCosts(ref root, 0);

			queue = TQueue.Create(maxNodes);
			var result = SearchingResult.InProgress;
			ref var solutionNode = ref Unsafe.NullRef<TreeNode>();
			var start = Stopwatch.GetTimestamp();
			root = ref Validate(in grid, ref root, nodeMemoryManager);
			if (Unsafe.IsNullRef(in root))
			{
				result = SearchingResult.Unreachable;
			}
			else
			{
				queue.Enqueue(in root);
			}

			while (result == SearchingResult.InProgress)
			{
				if (queue.IsEmpty)
				{
					result = SearchingResult.Unreachable;
					break;
				}

				ref var n = ref queue.Dequeue();
				Debug.Assert(!Unsafe.IsNullRef(in n));

				ref var parentState = ref n.State;
				var color = GetNextMoveColor(in grid, in parentState);

				foreach (var direction in Directions)
				{
					var forced = false;
					if (ForcesFirstColor && !SearchFastForward)
					{
						forced = FindForced(in grid, in n.State, out color, out _);
					}

					if (CanMove(in grid, in n.State, color, direction))
					{
						ref var child = ref nodeMemoryManager.CreateNode(in n, ref parentState);
						if (Unsafe.IsNullRef(in child))
						{
							result = SearchingResult.Full;
							break;
						}

						var actionCost = MakeMove(in grid, ref child.State, color, direction, forced);
						UpdateNodeCosts(ref child, actionCost);
						if (!Unsafe.IsNullRef(in child))
						{
							ref readonly var childState = ref child.State;
							if (childState.FreeCellsCount == 0 && childState.CompletedMask == (1 << grid.ColorsCount) - 1)
							{
								result = SearchingResult.Success;
								solutionNode = ref child;
								break;
							}

							queue.Enqueue(in child);
						}
					}
					if (forced)
					{
						break;
					}
				}
			}

			elapsed = new(Stopwatch.GetTimestamp() - start);
			nodes = nodeMemoryManager.Count;

			if (result == SearchingResult.Success)
			{
				Debug.Assert(!Unsafe.IsNullRef(in solutionNode));
				finalState = solutionNode.State;
			}
			else if (nodeMemoryManager.Count != 0)
			{
				finalState = nodeMemoryManager.Entry[nodeMemoryManager.Count - 1].State;
			}
			else
			{
				finalState = initState;
			}

			// Due to using variable cannot be passed as ref inside other methods,
			// we should append an extra call here to keep memory releasing.
			return result;
		}
		finally
		{
			nodeMemoryManager.Dispose();
			queue.Dispose();
		}
	}

	/// <summary>
	/// Validate the puzzle, and return the tree node in use.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="node">The node.</param>
	/// <param name="memoryManager">The memory manager of <see cref="TreeNode"/> instances.</param>
	/// <returns>The final <see cref="TreeNode"/> instance created.</returns>
	private ref TreeNode Validate(ref readonly GridAnalyticsInfo grid, ref TreeNode node, TreeNodeMemoryManager memoryManager)
	{
		Debug.Assert(Unsafe.AreSame(in node, in memoryManager.Entry[memoryManager.Count - 1]));

		ref var nodeState = ref node.State;
		if (SearchFastForward && ForcesFirstColor && FindForced(in grid, in nodeState, out var color, out var direction))
		{
			if (!CanMove(in grid, in nodeState, color, direction))
			{
				goto ReturnNull;
			}

			ref var forcedChild = ref memoryManager.CreateNode(in node, ref nodeState);
			if (!Unsafe.IsNullRef(in forcedChild))
			{
				MakeMove(in grid, ref forcedChild.State, color, direction, true);
				UpdateNodeCosts(ref forcedChild, 0);

				forcedChild = ref Validate(in grid, ref forcedChild, memoryManager);
				if (Unsafe.IsNullRef(in forcedChild))
				{
					goto ReturnNull;
				}
				return ref forcedChild;
			}
		}

		if (CheckDeadendCases && CheckDeadends(in grid, in nodeState))
		{
			goto ReturnNull;
		}

		if (CheckStrandedCases)
		{
			var resultMap = (stackalloc byte[MaxGridCellsCount]);
			var resultCount = BuildRegions(in grid, in nodeState, resultMap);
			if (GetStrandedColors(in grid, in nodeState, resultCount, resultMap, MaxSupportedColorsCount, 1) != 0)
			{
				goto ReturnNull;
			}
		}

		if (BottleneckLimit != 0 && CheckBottleneck(in grid, in nodeState) != 0)
		{
			goto ReturnNull;
		}
		return ref node;

	ReturnNull:
		Debug.Assert(Unsafe.AreSame(in node, in memoryManager.Entry[memoryManager.Count - 1]));
		memoryManager.Return(in node);
		return ref Unsafe.NullRef<TreeNode>();
	}
}
