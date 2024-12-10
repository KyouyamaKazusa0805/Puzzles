namespace Puzzles.Hamiltonian.Concepts;

/// <summary>
/// Represents a linked node for a coordinate.
/// </summary>
[TypeImpl(TypeImplFlags.Object_ToString)]
internal sealed partial class CoordinateNode([Property, StringMember] Coordinate coordinate, [Property] CoordinateNode? parent)
{
	/// <summary>
	/// Initializes a <see cref="CoordinateNode"/> instance.
	/// </summary>
	/// <param name="coordinate">The coordinate.</param>
	public CoordinateNode(Coordinate coordinate) : this(coordinate, null)
	{
	}


	/// <summary>
	/// Indicates the root node.
	/// </summary>
	public CoordinateNode Root
	{
		get
		{
			var result = this;
			var p = Parent;
			while (p is not null)
			{
				result = p;
				p = p.Parent;
			}
			return result;
		}
	}

	[StringMember(nameof(Parent))]
	private string ParentNodeString => Parent?.Coordinate.ToString() ?? "<null>";
}
