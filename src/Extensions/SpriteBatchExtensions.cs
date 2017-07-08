using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Warcraft.Screens.Manager;

namespace Warcraft.Extensions
{
	public static class SpriteBatchExtensions
	{
		/// <summary>
		/// Draws a line from <paramref name="start"/> to <paramref name="end"/> with <paramref name="color"/>.
		/// </summary>
		/// <remarks>
		/// Adapted from http://gamedev.stackexchange.com/a/44016.
		/// </remarks>
		public static void DrawLine(this SpriteBatch spriteBatch, Color color, Vector2 start, Vector2 end)
		{
			var edge = end - start;
			var angle = (float)Math.Atan2(edge.Y, edge.X);
			spriteBatch.Draw(ScreenManager.Pixel, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 1), null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
		}

		/// <summary>
		/// Draws a hallow rectangle at (<paramref name="x"/>, <paramref name="y"/>) of size (<paramref name="width"/>, <paramref name="height"/>) with <paramref name="color"/>.
		/// </summary>
		public static void DrawHallowRectangle(this SpriteBatch spriteBatch, int x, int y, int width, int height, Color color)
		{
			DrawHallowRectangle(spriteBatch, new Rectangle(x, y, width, height), color);
		}

		/// <summary>
		/// Draws a hallow rectangle matching the dimensions and location of <paramref name="rectangle"/> with <paramref name="color"/>.
		/// </summary>
		public static void DrawHallowRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color)
		{
			spriteBatch.Draw(ScreenManager.Pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, 1), color); // Top
			spriteBatch.Draw(ScreenManager.Pixel, new Rectangle(rectangle.X, rectangle.Y, 1, rectangle.Height), color); // Left
			spriteBatch.Draw(ScreenManager.Pixel, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - 1, rectangle.Width, 1), color); // Bottom
			spriteBatch.Draw(ScreenManager.Pixel, new Rectangle(rectangle.X + rectangle.Width - 1, rectangle.Y, 1, rectangle.Height), color); // Right
		}

		/// <summary>
		/// Draws a filled rectangle at (<paramref name="x"/>, <paramref name="y"/>) of size (<paramref name="width"/>, <paramref name="height"/>) with <paramref name="color"/>.
		/// </summary>
		public static void DrawFilledRectangle(this SpriteBatch spriteBatch, int x, int y, int width, int height, Color color)
		{
			spriteBatch.DrawFilledRectangle(new Rectangle(x, y, width, height), color);
		}

		/// <summary>
		/// Draws a filled rectangle matching the dimensions and location of <paramref name="rectangle"/> with <paramref name="color"/>.
		/// </summary>
		/// <remarks>
		/// Adapted from http://stackoverflow.com/a/5752115.
		/// </remarks>
		public static void DrawFilledRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color)
		{
			spriteBatch.Draw(ScreenManager.Pixel, rectangle.Location.ToVector2(), null, color, 0f, Vector2.Zero, rectangle.Size.ToVector2(), SpriteEffects.None, 0f);
		}
	}
}