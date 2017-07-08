using Microsoft.Xna.Framework;

namespace Warcraft.Extensions
{
	/// <summary>
	/// Static class for <see cref="Color"/> extensions.
	/// </summary>
	public static class ColorExtensions
	{
		/// <summary>
		/// Returns the <paramref name="color"/> with the R and B bytes swapped.
		/// </summary>
		/// <example>
		/// 0xAARRBBGG.SwapRnB() -> 0xAAGGBBRR
		/// 0x01234567.SwapRnB() -> 0x01674523
		/// </example>
		public static uint SwapRnB(this uint color)
		{
			var r = color & 0xFF;
			var b = (color & 0xFF0000) >> 16;
			return color & 0xFF00FF00 | r << 16 | b;
		}

		/// <summary>
		/// Returns a new <see cref="Color"/> based on <paramref name="color"/>.
		/// </summary>
		public static Color ToColor(this uint color)
		{
			return new Color(color);
		}
	}
}