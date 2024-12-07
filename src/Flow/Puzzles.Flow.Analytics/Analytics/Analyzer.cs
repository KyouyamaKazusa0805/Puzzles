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
	public bool CheckStranded { get; set; }

	/// <summary>
	/// Indicates whether analyzer will check on dead-end cases.
	/// </summary>
	public bool CheckDeadends { get; set; }

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
	public bool TryLoadPuzzle(string gridString, out Grid grid, out ProgressState state)
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
	/// Order colors.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <param name="userOrder">User order.</param>
	[SuppressMessage("Style", "IDE0042:Deconstruct variable declaration", Justification = "<Pending>")]
	private void OrderColors(ref Grid grid, ref ProgressState state, string? userOrder)
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
	private bool CanMove(ref readonly ProgressState state, ref readonly Grid grid, int color, Direction direction)
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
	/// Pick the next color to move deterministically.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="state">The state.</param>
	/// <returns>The next color chosen.</returns>
	private byte GetNextMoveColor(ref readonly Grid grid, ref readonly ProgressState state)
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
	private double MakeMove(ref readonly Grid grid, ref ProgressState state, byte color, Direction direction, bool forced)
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
	private int GetFreedCoordinatesCount(ref readonly Grid grid, ref readonly ProgressState state, int x, int y)
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
	private int GetFreedCoordinatesCount(ref readonly Grid grid, ref readonly ProgressState state, byte position)
	{
		Position.GetCoordinateFromPosition(position, out var x, out var y);
		return GetFreedCoordinatesCount(in grid, in state, x, y);
	}
}
