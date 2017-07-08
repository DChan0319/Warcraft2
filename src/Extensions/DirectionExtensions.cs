using Warcraft.App;

namespace Warcraft.Extensions
{
	public static class DirectionExtensions
	{
		/// <summary>
		/// Returns the direction opposite to <paramref name="dir"/>.
		/// </summary>
		public static Direction Opposite(this Direction dir)
		{
			return (Direction)(((int)dir + (int)Direction.Max / 2) % (int)Direction.Max);
		}

		/// <summary>
		/// Returns the direction represented by <paramref name="dir"/>, rolling over if necessary.
		/// </summary>
		public static Direction ToDirection(this int dir)
		{
			return (Direction)((dir + (int)Direction.Max) % (int)Direction.Max);
		}
	}
}