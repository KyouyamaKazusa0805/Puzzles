namespace Puzzles.Hamiltonian.Transforming;

/// <summary>
/// Provides a way to transform a path.
/// </summary>
public static class Transformation
{
	/// <summary>
	/// Rotates a path clockwise.
	/// </summary>
	/// <inheritdoc cref="TransformCore(Path, int, int, Func{string, string})"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path RotateClockwise(this Path path, int rows, int columns)
		=> TransformCore(path, rows, columns, LocalTransformer.RotateClockwise);

	/// <summary>
	/// Rotates a path counter-clockwise.
	/// </summary>
	/// <inheritdoc cref="TransformCore(Path, int, int, Func{string, string})"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path RotateCounterclockwise(this Path path, int rows, int columns)
		=> TransformCore(path, rows, columns, LocalTransformer.RotateCounterclockwise);

	/// <summary>
	/// Mirrors a path left and right.
	/// </summary>
	/// <inheritdoc cref="TransformCore(Path, int, int, Func{string, string})"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path MirrorLeftRight(this Path path, int rows, int columns)
		=> TransformCore(path, rows, columns, LocalTransformer.MirrorLeftRight);

	/// <summary>
	/// Mirrors a path top and bottom.
	/// </summary>
	/// <inheritdoc cref="TransformCore(Path, int, int, Func{string, string})"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path MirrorTopBottom(this Path path, int rows, int columns)
		=> TransformCore(path, rows, columns, LocalTransformer.MirrorTopBottom);

	/// <summary>
	/// Mirrors a path diagonal.
	/// </summary>
	/// <inheritdoc cref="TransformCore(Path, int, int, Func{string, string})"/>
	/// <exception cref="InvalidOperationException">
	/// Throws when the <paramref name="rows"/> is not equal to <paramref name="columns"/>.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path MirrorDiagonal(this Path path, int rows, int columns)
		=> TransformCore(path, rows, columns, LocalTransformer.MirrorDiagonal);

	/// <summary>
	/// Mirrors a path anti-diagonal.
	/// </summary>
	/// <inheritdoc cref="TransformCore(Path, int, int, Func{string, string})"/>
	/// <exception cref="InvalidOperationException">
	/// Throws when the <paramref name="rows"/> is not equal to <paramref name="columns"/>.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Path MirrorAntidiagonal(this Path path, int rows, int columns)
		=> TransformCore(path, rows, columns, LocalTransformer.MirrorAntidiagonal);

	/// <summary>
	/// The local method to transform a path.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <param name="rows">The number of rows.</param>
	/// <param name="columns">The number of columns.</param>
	/// <param name="transformer">The transformer method.</param>
	/// <returns>The result path.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Path TransformCore(Path path, int rows, int columns, Func<string, string> transformer)
	{
		var formatProvider = new IndexedPathFormatInfo(rows, columns);
		return Path.Parse(transformer(path.ToString(formatProvider)), formatProvider);
	}
}

/// <summary>
/// Provides a local transformer.
/// </summary>
file static class LocalTransformer
{
	/// <inheritdoc cref="Transformation.RotateClockwise(Path, int, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string RotateClockwise(this string value)
		=> TransformCore(
			value,
			(rows, columns) => (columns, rows),
			(coordinate, rows, columns) =>
			{
				var rowIndex = coordinate / columns;
				var columnIndex = coordinate % columns;
				return columnIndex * rows + rows - rowIndex - 1;
			}
		);

	/// <inheritdoc cref="Transformation.RotateCounterclockwise(Path, int, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string RotateCounterclockwise(this string value)
		=> value.RotateClockwise().RotateClockwise().RotateClockwise();

	/// <inheritdoc cref="Transformation.MirrorLeftRight(Path, int, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string MirrorLeftRight(this string value)
		=> TransformCore(
			value,
			(rows, columns) => (rows, columns),
			(coordinate, rows, columns) =>
			{
				var rowIndex = coordinate / columns;
				var columnIndex = coordinate % columns;
				return rowIndex * columns + columns - 1 - columnIndex;
			}
		);

	/// <inheritdoc cref="Transformation.MirrorTopBottom(Path, int, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string MirrorTopBottom(this string value)
		=> TransformCore(
			value,
			(rows, columns) => (rows, columns),
			(coordinate, rows, columns) =>
			{
				var rowIndex = coordinate / columns;
				var columnIndex = coordinate % columns;
				return (rows - rowIndex - 1) * columns + columnIndex;
			}
		);

	/// <inheritdoc cref="Transformation.MirrorDiagonal(Path, int, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string MirrorDiagonal(this string value)
		=> TransformCore(
			value,
			DiagonalSizeChanger,
			(coordinate, rows, columns) =>
			{
				var rowIndex = coordinate / columns;
				var columnIndex = coordinate % columns;
				return columnIndex * columns + rowIndex;
			}
		);

	/// <inheritdoc cref="Transformation.MirrorAntidiagonal(Path, int, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string MirrorAntidiagonal(this string value)
		=> TransformCore(
			value,
			DiagonalSizeChanger,
			(coordinate, rows, columns) =>
			{
				var rowIndex = coordinate / columns;
				var columnIndex = coordinate % columns;
				return (columns - 1 - columnIndex) * columns + (columns - 1 - rowIndex);
			}
		);

	/// <summary>
	/// Called by <see cref="MirrorDiagonal(string)"/> and <see cref="MirrorAntidiagonal(string)"/>.
	/// </summary>
	private static (int, int) DiagonalSizeChanger(int rows, int columns)
		=> rows == columns
			? (rows, rows)
			: throw new InvalidOperationException("This transformation requires the number of rows and columns must be same.");

	/// <summary>
	/// The backing method to transform a string text (path).
	/// </summary>
	/// <param name="value">The value to be transformed.</param>
	/// <param name="sizeChanger">The changer method that changes the size value into the result values.</param>
	/// <param name="valueTransformer">The method that transforms each coordinate value.</param>
	/// <returns>The result.</returns>
	private static string TransformCore(string value, Func<int, int, (int, int)> sizeChanger, Func<int, int, int, int> valueTransformer)
	{
		var split = value.Split(':');
		var rows = int.Parse(split[0]);
		var columns = int.Parse(split[1]);
		var valuesSpan = split[2].AsSpan();
		var values = new List<int>(valuesSpan.Length >> 1);
		for (var i = 0; i < valuesSpan.Length; i += 2)
		{
			values.Add(int.Parse(valuesSpan[i..(i + 2)]));
		}

		// Projects the coordinates.
		var result = new List<int>(valuesSpan.Length >> 1);
		for (var i = 0; i < values.Count; i++)
		{
			result.Add(valueTransformer(values[i], rows, columns));
		}

		// Gets the new rows and columns.
		(rows, columns) = sizeChanger(rows, columns);

		// Return the value back.
		var sb = new StringBuilder();
		sb.Append($"{rows}:{columns}:");
		foreach (var element in result)
		{
			sb.Append(element.ToString("00"));
		}
		return sb.ToString();
	}
}
