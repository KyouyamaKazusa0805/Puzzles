namespace Puzzles.Flow.Analytics;

/// <summary>
/// Represents the analysis result.
/// </summary>
/// <param name="grid">The grid to be used.</param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class AnalysisResult([Property, HashCodeMember] Grid grid) :
	IEnumerable<Path>,
	IEquatable<AnalysisResult>,
	IEqualityOperators<AnalysisResult, AnalysisResult, bool>
{
	/// <summary>
	/// Indicates whether the puzzle has been solved.
	/// </summary>
	[MemberNotNullWhen(true, nameof(InterimPaths))]
	[HashCodeMember]
	[EquatableMember]
	public required bool IsSolved { get; init; }

	/// <summary>
	/// Indicates the failed reason.
	/// </summary>
	public FailedReason FailedReason { get; init; }

	/// <summary>
	/// Indicates the elapsed time.
	/// </summary>
	public TimeSpan ElapsedTime { get; init; }

	/// <summary>
	/// Indicates the found paths.
	/// </summary>
	[EquatableMember]
	public ReadOnlySpan<Path> Paths => InterimPaths;

	/// <summary>
	/// Indicates the exception thrown while solution searching.
	/// This property won't be <see langword="null"/> if property <see cref="FailedReason"/>
	/// returns <see cref="FailedReason.ExceptionThrown"/>.
	/// </summary>
	public Exception? UnhandledException { get; init; }

	/// <summary>
	/// Indicates the paths.
	/// </summary>
	internal Path[]? InterimPaths { get; init; }

	[EquatableMember]
	private Grid GridEntry => Grid;


	/// <inheritdoc/>
	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.AppendLine("Puzzle:");
		sb.AppendLine(Grid.ToString());
		sb.AppendLine("---");

		if (IsSolved)
		{
			sb.AppendLine("Paths:");
			foreach (var path in Paths)
			{
				sb.AppendLine(path.ToString());
			}
			sb.AppendLine("---");
			sb.AppendLine("Puzzle is solved.");
			sb.AppendLine($@"Elapsed time: {ElapsedTime:hh\:mm\:ss\.fff}");
		}
		else
		{
			sb.AppendLine($"Puzzle can't be solved. Reason code: '{FailedReason}'.");
			if (UnhandledException is not null)
			{
				sb.AppendLine($"Unhandled exception: {UnhandledException}");
			}
		}
		return sb.ToString();
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<Path> GetEnumerator() => new(Paths);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Paths.ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Path> IEnumerable<Path>.GetEnumerator() => Paths.ToArray().AsEnumerable().GetEnumerator();


	/// <summary>
	/// Creates an <see cref="AnalysisResult"/> instance via the current grid information and state.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="gridInfo">The grid information.</param>
	/// <param name="state">The grid state.</param>
	/// <param name="elapsed">The time elapsed.</param>
	/// <returns>The analysis result.</returns>
	internal static unsafe AnalysisResult Create(
		Grid grid,
		ref readonly GridAnalyticsInfo gridInfo,
		ref readonly GridInterimState state,
		TimeSpan elapsed
	)
	{
		var pathCellsDictionary = new SortedDictionary<Color, List<Coordinate>>();
		var startCoordinates = (stackalloc Coordinate[gridInfo.ColorsCount]);
		var endCoordinates = (stackalloc Coordinate[gridInfo.ColorsCount]);
		for (var column = (byte)0; column < gridInfo.Size; column++)
		{
			for (var row = (byte)0; row < gridInfo.Size; row++)
			{
				var cell = state.Cells[Position.GetPositionFromCoordinate(column, row)];
				var coordinate = new Coordinate(row, column);
				var type = Cell.GetTypeFromCell(cell);
				var color = Cell.GetCellColor(cell);
				switch (type)
				{
					case CellState.Start or CellState.End:
					{
						(type == CellState.Start ? ref startCoordinates[color] : ref endCoordinates[color]) = coordinate;
						goto case CellState.Path; // Fallthrough
					}
					case CellState.Path:
					{
						if (!pathCellsDictionary.TryAdd(color, [coordinate]))
						{
							pathCellsDictionary[color].Add(coordinate);
						}
						break;
					}
					default:
					{
						return new(grid)
						{
							IsSolved = false,
							FailedReason = FailedReason.ExceptionThrown,
							UnhandledException = new InvalidOperationException("This method can only be called in solved cases."),
							ElapsedTime = elapsed
						};
					}
				}
			}
		}

		// Construct paths.
		var paths = new List<Path>();
		foreach (var (color, coordinates) in pathCellsDictionary)
		{
			paths.Add(new(new(startCoordinates[color], endCoordinates[color], color), [.. coordinates]));
		}
		return new(grid) { IsSolved = true, InterimPaths = [.. paths], ElapsedTime = elapsed };
	}
}
