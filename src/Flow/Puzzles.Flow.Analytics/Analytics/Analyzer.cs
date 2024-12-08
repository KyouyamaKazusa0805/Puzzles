namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents an analyzer.
/// </summary>
public sealed unsafe class Analyzer
{
	/// <summary>
	/// Indicates whether analyzer will check on touchness.
	/// </summary>
	public bool CheckTouchness { get; set; }

	/// <summary>
	/// Indicates whether analyzer will check on stranded cases.
	/// </summary>
	public bool CheckStrandedCases { get; set; }

	/// <summary>
	/// Indicates whether analyzer will check on deadend cases.
	/// </summary>
	public bool CheckDeadendCases { get; set; }

	/// <summary>
	/// Indicates whether analyzer will search for outside-in cases.
	/// </summary>
	public bool SearchOutsideIn { get; set; }

	/// <summary>
	/// Indicates whether analyzer will search fast forwardly.
	/// </summary>
	public bool SearchFastForward { get; set; }

	/// <summary>
	/// Indicates whether analyzer will explore penalized cases.
	/// </summary>
	public bool PenalizeExploration { get; set; }

	/// <summary>
	/// Indicates whether analyzer will automatically adjust colors in searching experience.
	/// </summary>
	public bool ReorderColors { get; set; }

	/// <summary>
	/// Indicates whether analyzer will automatically adjust colors via constrainted priority.
	/// </summary>
	public bool ReorderOnMostConstrained { get; set; }

	/// <summary>
	/// Indicates whether analyzer will force the first color as start.
	/// </summary>
	public bool ForcesFirstColor { get; set; }

	/// <summary>
	/// Indicates whether analyzer will randomize colors.
	/// </summary>
	public bool RandomOrdering { get; set; }

	/// <summary>
	/// Indicates whether analyzer will use best-first search (BFS) rule to check grid.
	/// </summary>
	public bool UsesBestFirstSearch { get; set; }

	/// <summary>
	/// Indicates the maximum memory usage in mega-bytes.
	/// </summary>
	public double MaxMemoryUsage { get; set; }

	/// <summary>
	/// Indicates the number of bottleneck limit.
	/// </summary>
	public int BottleneckLimit { get; set; }

	/// <summary>
	/// Indicates the number of maximum nodes can be reached.
	/// </summary>
	public int MaxNodes { get; set; }

	/// <summary>
	/// Indicates the queue creator method.
	/// </summary>
	[DisallowNull]
	internal delegate*<int, Queue> QueueCreator { get; set; }

	/// <summary>
	/// Indicates the queue enqueuer method.
	/// </summary>
	[DisallowNull]
	internal delegate*<ref Queue, ref TreeNode, void> QueueEnqueuer { get; set; }

	/// <summary>
	/// Indicates the queue dequeuer method.
	/// </summary>
	[DisallowNull]
	internal delegate*<ref Queue, ref TreeNode> QueueDequeuer { get; set; }

	/// <summary>
	/// Indicatees the queue destroyer method.
	/// </summary>
	[DisallowNull]
	internal delegate*<ref Queue, void> QueueDestroyer { get; set; }

	/// <summary>
	/// Indicates the queue clearer method.
	/// </summary>
	[DisallowNull]
	internal delegate*<ref readonly Queue, int> QueueClearer { get; set; }

	/// <summary>
	/// Indicates thee queue peeker method.
	/// </summary>
	[DisallowNull]
	internal delegate*<ref readonly Queue, ref readonly TreeNode> QueuePeeker { get; set; }


	/// <summary>
	/// Try to load a puzzle via its string representation, and return its bound grid data structure and state.
	/// </summary>
	/// <param name="gridString">The grid string.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the loading operation is successful.</returns>
	public bool TryLoadPuzzle(string gridString, out Grid grid, out ProcessState state)
	{
		state = default;
		new Span<byte>(Unsafe.AsPointer(ref state.Positions[0]), MaxColors).Fill(byte.MaxValue);
		state.LastColor = MaxColors;

		grid = default;
		new Span<byte>(Unsafe.AsPointer(ref grid.ColorTable[0]), 1 << 7).Fill(byte.MaxValue);
		new Span<byte>(Unsafe.AsPointer(ref grid.InitPositions[0]), MaxColors).Fill(byte.MaxValue);
		new Span<byte>(Unsafe.AsPointer(ref grid.GoalPositions[0]), MaxColors).Fill(byte.MaxValue);

		var y = (byte)0;
		var lines = gridString.Split("\r\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		while (grid.Size == 0 || y < grid.Size)
		{
			var line = lines[y];
			for (var x = (byte)0; x < grid.Size; x++)
			{
				var c = line[x];
				if (c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f')
				{
					state.FreedCellsCount++;
					continue;
				}

				var pos = Position.GetPositionFromCoordinate(x, y);
				Debug.Assert(pos < MaxCells);

				var color = grid.ColorTable[char.ToUpper(c)];
				if (color >= grid.ColorsCount)
				{
					color = grid.ColorsCount;
					if (grid.ColorsCount == MaxColors)
					{
						// Too many colors.
						(grid, state) = (default, default);
						return false;
					}

					var id = GetColor(c);
					if (id < 0 || id >= MaxColors)
					{
						// Color value is invalid or not supported.
						(grid, state) = (default, default);
						return false;
					}

					grid.ColorIds[color] = id;
					grid.ColorOrder[color] = color;

					c = char.ToUpper(c);
					grid.ColorsCount++;
					grid.ColorTable[c] = color;
					grid.InitPositions[color] = state.Positions[color] = pos;
					state.Cells[pos] = Cell.Create(CellType.Init, color, 0);
				}
				else
				{
					if (grid.GoalPositions[color] != InvalidPos)
					{
						// Multiple endpoints found.
						(grid, state) = (default, default);
						return false;
					}

					grid.GoalPositions[color] = pos;
					state.Cells[pos] = Cell.Create(CellType.Goal, color, 0);
				}
			}

			y++;
		}

		if (grid.ColorsCount == 0)
		{
			// The grid is empty.
			(grid, state) = (default, default);
			return false;
		}

		for (var color = (byte)0; color < grid.ColorsCount; color++)
		{
			if (grid.GoalPositions[color] == InvalidPos)
			{
				// Such color contains start point but not for end point.
				(grid, state) = (default, default);
				return false;
			}

			if (SearchOutsideIn)
			{
				var initDistance = grid.GetWallDistance(grid.InitPositions[color]);
				var goalDistance = grid.GetWallDistance(grid.GoalPositions[color]);
				if (goalDistance < initDistance)
				{
					// Swap.
					(grid.InitPositions[color], grid.GoalPositions[color]) = (grid.GoalPositions[color], grid.InitPositions[color]);

					state.Cells[grid.InitPositions[color]] = Cell.Create(CellType.Init, color, 0);
					state.Cells[grid.GoalPositions[color]] = Cell.Create(CellType.Goal, color, 0);
					state.Positions[color] = grid.InitPositions[color];
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Add the current color bit flag to the regions adjacent to the current or goal position.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="resultMap">The result map.</param>
	/// <param name="pos">The position.</param>
	/// <param name="cFlag">The flag.</param>
	/// <param name="resultFlags">The result flags.</param>
	private void AddColor(
		ref readonly Grid grid,
		ref readonly ProcessState state,
		Span<byte> resultMap,
		byte pos,
		short cFlag,
		Span<short> resultFlags
	)
	{
		foreach (var direction in Directions)
		{
			var neighborPos = Position.GetOffsetPosition(in grid, pos, direction);
			if (neighborPos != InvalidPos)
			{
				// Find out what region it is in.
				// If it is in a valid region, we should add this color to the region.
				var neighborRegion = resultMap[neighborPos];
				if (neighborRegion != InvalidPos)
				{
					resultFlags[neighborRegion] |= cFlag;
				}
			}
		}
	}

	/// <summary>
	/// Print regions.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="resultMap">The result map.</param>
	/// <param name="writer">The writer.</param>
	private void PrintRegions(ref readonly Grid grid, ref readonly ProcessState state, ReadOnlySpan<byte> resultMap, TextWriter writer)
	{
		const char blockChar = '#';
		writer.Write(blockChar);
		for (var x = 0; x < grid.Size; x++)
		{
			writer.Write(blockChar);
		}
		writer.WriteLine(blockChar);

		for (var y = (byte)0; y < grid.Size; y++)
		{
			writer.Write(blockChar);
			for (var x = (byte)0; x < grid.Size; x++)
			{
				var pos = Position.GetPositionFromCoordinate(x, y);
				var rid = resultMap[pos];
				ref readonly var l = ref ColorDictionary[rid % MaxColors];
				if (state.Cells[pos] == 0)
				{
					Debug.Assert(rid != InvalidPos);
					var c = (char)('A' + rid % 60);
					writer.Write(ConsoleOut.GetColorString(l.ConsoleOutColorString, l.InputChar));
				}
				else
				{
					Debug.Assert(rid == InvalidPos);
					writer.Write(' ');
				}
			}
			writer.WriteLine(blockChar);
		}

		writer.Write(blockChar);
		for (var x = 0; x < grid.Size; x++)
		{
			writer.Write(blockChar);
		}
		writer.WriteLine(blockChar);
		writer.WriteLine();
	}

	/// <summary>
	/// Order colors.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="userOrder">User order.</param>
	[SuppressMessage("Style", "IDE0042:Deconstruct variable declaration", Justification = "<Pending>")]
	private void OrderColors(ref Grid grid, ref ProcessState state, string? userOrder)
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

		var cf = (stackalloc ColorFeature[MaxColors]);
		cf.Clear();

		for (var color = (byte)0; color < grid.ColorsCount; color++)
		{
			cf[color].Index = color;
			cf[color].UserIndex = MaxColors;
		}

		if (ReorderColors)
		{
			for (var color = (byte)0; color < grid.ColorsCount; color++)
			{
				var x = (First: 0, Second: 0);
				var y = (First: 0, Second: 0);
				foreach (var isFirst in (true, false))
				{
					ref var p = ref isFirst ? ref cf[color].WallDistance.First : ref cf[color].WallDistance.Second;
					p = grid.GetWallDistance(isFirst ? x.First : x.Second, isFirst ? y.First : y.Second);
				}

				var dx = Math.Abs(x.Second - x.First);
				var dy = Math.Abs(y.Second - y.First);
				cf[color].MinDistance = dx + dy;
			}
		}

		if (userOrder is not null)
		{
			var k = 0;
			foreach (var c in userOrder.ToUpper())
			{
				var color = c < 127 ? grid.ColorTable[c] : MaxColors;
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

		cf.Sort(ColorFeature.Compare);
		for (var i = 0; i < grid.ColorsCount; i++)
		{
			grid.ColorOrder[i] = cf[i].Index;
		}
	}

	/// <summary>
	/// Determine whether the current grid can move.
	/// </summary>
	/// <param name="state">The current state.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="color">The color.</param>
	/// <param name="direction">The direction.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="grid"/> or <paramref name="color"/> is invalid.
	/// </exception>
	private bool CanMove(ref readonly ProcessState state, ref readonly Grid grid, int color, Direction direction)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(color, grid.ColorsCount);
		ArgumentOutOfRangeException.ThrowIfZero(state.CompletedMask >> color & 1);

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
		Debug.Assert(newPosition < MaxCells);

		if (!CheckTouchness && newPosition == grid.GoalPositions[color])
		{
			return true;
		}

		if (state.Cells[newPosition] != 0)
		{
			// Must be empty.
			return false;
		}

		if (CheckTouchness)
		{
			// All puzzles are designed so that a new path segment is adjacent to at most one path segment of the same color -
			// the predecessor to the new segment.
			// We check this by iterating over the neighbors.
			foreach (var neighborDirection in Directions)
			{
				// Assemble position.
				var neighborPosition = Position.GetOffsetPosition(in grid, newX, newY, neighborDirection);
				if (neighborPosition != InvalidPos && state.Cells[neighborPosition] != 0
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
	private bool IsDeadend(ref readonly Grid grid, ref readonly ProcessState state, byte pos)
	{
		Debug.Assert(pos != InvalidPos && state.Cells[pos] == 0);

		Position.GetCoordinateFromPosition(pos, out var x, out var y);
		var freedCount = 0;
		foreach (var direction in Directions)
		{
			var neighborPos = Position.GetOffsetPosition(in grid, x, y, direction);
			if (neighborPos == InvalidPos)
			{
				continue;
			}

			if (state.Cells[neighborPos] == 0)
			{
				freedCount++;
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
					freedCount++;
				}
			}
		}
		return freedCount <= 1;
	}

	/// <summary>
	/// Check for deadend regions of free-space where there is no way to put an active path into and out of it.
	/// Any free-space node which has only one free neighbor represents such a deadend.
	/// For the purposes of this check, current and goal positions coutn as "free".
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	private bool CheckDeadends(ref readonly Grid grid, ref readonly ProcessState state)
	{
		var color = state.LastColor;
		if (color >= grid.ColorsCount)
		{
			return false;
		}

		var currentPos = state.Positions[color];
		Position.GetCoordinateFromPosition(currentPos, out var x, out var y);
		foreach (var direction in Directions)
		{
			var neighborPos = Position.GetOffsetPosition(in grid, x, y, direction);
			if (neighborPos != InvalidPos && state.Cells[neighborPos] == 0 && IsDeadend(in grid, in state, neighborPos))
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
	private bool IsForced(ref readonly Grid grid, ref readonly ProcessState state, byte color, byte pos)
	{
		var freedCount = 0;
		var otherEndpointsCount = 0;
		foreach (var direction in Directions)
		{
			var neighborPos = Position.GetOffsetPosition(in grid, pos, direction);
			if (neighborPos == InvalidPos || neighborPos == state.Positions[color])
			{
				continue;
			}

			if (state.Cells[neighborPos] == 0)
			{
				freedCount++;
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
		return freedCount == 1 && otherEndpointsCount == 0;
	}

	/// <summary>
	/// Find a forced color.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="forcedColor">The forced color.</param>
	/// <param name="forcedDirection">The forced direction.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	private bool FindForced(ref readonly Grid grid, ref readonly ProcessState state, out int forcedColor, out Direction forcedDirection)
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

			var freedDirection = (Direction)byte.MaxValue;
			var freedCount = 0;
			foreach (var direction in Directions)
			{
				var neighborPos = Position.GetOffsetPosition(in grid, state.Positions[color], direction);
				if (neighborPos == InvalidPos)
				{
					continue;
				}

				if (state.Cells[neighborPos] == 0)
				{
					freedDirection = direction;
					freedCount++;

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
	private byte BuildRegions(ref readonly Grid grid, ref readonly ProcessState state, Span<byte> resultMap)
	{
		var regions = (stackalloc Region[MaxCells]);
		regions.Clear();

		// Build regions.
		for (var y = (byte)0; y < grid.Size; y++)
		{
			for (var x = (byte)0; x < grid.Size; x++)
			{
				var pos = Position.GetPositionFromCoordinate(x, y);
				if (state.Cells[pos] != 0)
				{
					regions[pos] = Region.Create(InvalidPos);
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

		var resultLookup = (stackalloc byte[MaxCells]);
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
				if (root != InvalidPos)
				{
					if (resultLookup[root] == InvalidPos)
					{
						resultLookup[root] = result++;
					}
					resultMap[pos] = resultLookup[root];
				}
				else
				{
					resultMap[pos] = InvalidPos;
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
	private byte GetNextMoveColor(ref readonly Grid grid, ref readonly ProcessState state)
	{
		var lastColor = state.LastColor;
		if (lastColor < grid.ColorsCount && (state.CompletedMask >> lastColor & 1) == 0)
		{
			return lastColor;
		}

		if (!grid.IsUserOrdered && ReorderOnMostConstrained)
		{
			var bestColor = (byte)255;
			var bestFree = 4;

			for (var i = 0; i < grid.ColorsCount; i++)
			{
				var color = grid.ColorOrder[i];
				if ((state.CompletedMask >> color & 1) != 0)
				{
					// Already completed.
					continue;
				}

				var freedCount = GetFreedCoordinatesCount(in grid, in state, state.Positions[color]);
				if (freedCount < bestFree)
				{
					bestFree = freedCount;
					bestColor = color;
				}
			}

			Debug.Assert(bestColor < grid.ColorsCount);
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
	/// <returns>A <see cref="double"/> result.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the <paramref name="color"/> specified is exceeded the limit.
	/// </exception>
	private double MakeMove(ref readonly Grid grid, ref ProcessState state, byte color, Direction direction, bool forced)
	{
		// Make sure the color is valid.
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(color, grid.ColorsCount);

		// Update the cell with the new cell value.
		var move = Cell.Create(CellType.Path, color, direction);

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
		Debug.Assert(newPosition < MaxCells);

		if (!CheckTouchness && newPosition == grid.GoalPositions[color])
		{
			state.Cells[grid.GoalPositions[color]] = Cell.Create(CellType.Goal, color, direction);
			state.CompletedMask |= (short)(1 << color);
			return 0;
		}

		// Make sure it's empty.
		Debug.Assert(state.Cells[newPosition] == 0);

		// Update cells and new position.
		state.Cells[newPosition] = move;
		state.Positions[color] = newPosition;
		state.FreedCellsCount--;

		state.LastColor = color;

		var actionCost = 1D;
		var goalDirection = (Direction)255;

		if (CheckTouchness)
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

		if (goalDirection >= 0)
		{
			state.Cells[grid.GoalPositions[color]] = Cell.Create(CellType.Goal, color, goalDirection);
			state.CompletedMask |= (short)(1 << color);
			actionCost = 0;
		}
		else
		{
			var freedCount = GetFreedCoordinatesCount(in grid, in state, newX, newY);
			if (PenalizeExploration && freedCount == 2)
			{
				actionCost = 2;
			}
		}

		return forced ? actionCost = 0 : actionCost;
	}

	/// <summary>
	/// Get the number of freed spaces around coordinate x and y.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="x">The x coordinate value.</param>
	/// <param name="y">The y coordinate value.</param>
	/// <returns>An <see cref="int"/> value indicating the number.</returns>
	private int GetFreedCoordinatesCount(ref readonly Grid grid, ref readonly ProcessState state, int x, int y)
	{
		var result = 0;
		foreach (var direction in Directions)
		{
			var neighborPosition = Position.GetOffsetPosition(in grid, x, y, direction);
			if (neighborPosition != InvalidPos && state.Cells[neighborPosition] == 0)
			{
				result++;
			}
		}
		return result;
	}

	/// <summary>
	/// Get the number of freed spaces around coordinate x and y.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="position">The position.</param>
	/// <returns>An <see cref="int"/> value indicating the number.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetFreedCoordinatesCount(ref readonly Grid grid, ref readonly ProcessState state, byte position)
	{
		Position.GetCoordinateFromPosition(position, out var x, out var y);
		return GetFreedCoordinatesCount(in grid, in state, x, y);
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
	private short GetStrandedColors(
		ref readonly Grid grid,
		ref readonly ProcessState state,
		int resultCount,
		Span<byte> resultMap,
		int chokePointColor,
		int maxStranded
	)
	{
		// For each region, we have bit flags to track whether current or goal position
		// is adjacent to the region. These get init'ed to 0.
		var currentResultFlags = (stackalloc short[resultCount]);
		var goalResultFlags = (stackalloc short[resultCount]);
		currentResultFlags.Clear();
		goalResultFlags.Clear();

		var strandedCount = 0;
		var result = (short)0;
		var forChokePoint = chokePointColor < grid.ColorsCount;

		// For each color, figure out which regions touch its current and goal position,
		// and make sure no color is stranded.
		for (var color = 0; color < grid.ColorsCount; color++)
		{
			var cFlag = (short)(1 << color);

			// No worries if completed.
			if ((state.CompletedMask & cFlag) != 0 || color == chokePointColor)
			{
				continue;
			}

			AddColor(in grid, in state, resultMap, state.Positions[color], cFlag, currentResultFlags);
			AddColor(in grid, in state, resultMap, grid.GoalPositions[color], cFlag, goalResultFlags);

			if (!CheckTouchness)
			{
				var delta = Math.Abs(state.Positions[color] - grid.GoalPositions[color]);
				if (delta == 1 || delta == 16)
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
}
