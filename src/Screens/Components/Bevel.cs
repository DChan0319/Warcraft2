using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Warcraft.Assets;
using Warcraft.Screens.Manager;

namespace Warcraft.Screens.Components
{
	public class Bevel
	{
		/// <summary>
		/// <see cref="Tileset"/> used for drawing the bevel
		/// </summary>
		private Tileset Tileset { get; }

		private List<int> TopIndices { get; }
		private List<int> BottomIndices { get; }
		private List<int> LeftIndices { get; }
		private List<int> RightIndices { get; }
		private List<int> CornerIndices { get; }

		/// <summary>
		/// Width of the bevel
		/// </summary>
		public int Width { get; }

		public Bevel(Tileset tileset)
		{
			Tileset = tileset;
			Width = tileset.TileWidth;

			TopIndices = new List<int>(new int[Width]);
			TopIndices[0] = Tileset.GetIndex("tf");
			for (var i = 1; i < Width; i++)
				TopIndices[i] = Tileset.GetIndex("t" + i);

			BottomIndices = new List<int>(new int[Width]);
			BottomIndices[0] = Tileset.GetIndex("bf");
			for (var i = 1; i < Width; i++)
				BottomIndices[i] = Tileset.GetIndex("b" + i);

			LeftIndices = new List<int>(new int[Width]);
			LeftIndices[0] = Tileset.GetIndex("lf");
			for (var i = 1; i < Width; i++)
				LeftIndices[i] = Tileset.GetIndex("l" + i);

			RightIndices = new List<int>(new int[Width]);
			RightIndices[0] = Tileset.GetIndex("rf");
			for (var i = 1; i < Width; i++)
				RightIndices[i] = Tileset.GetIndex("r" + i);

			CornerIndices = new List<int>(new int[4]);
			CornerIndices[0] = Tileset.GetIndex("tl");
			CornerIndices[1] = Tileset.GetIndex("tr");
			CornerIndices[2] = Tileset.GetIndex("bl");
			CornerIndices[3] = Tileset.GetIndex("br");
		}

		/// <summary>
		/// Draws a bevel around <paramref name="area"/> on the current render target.
		/// </summary>
		public void DrawBevel(Rectangle area)
		{
			var spriteBatch = ScreenManager.SpriteBatch;

			var topY = area.Y - Width;
			var bottomY = area.Y + area.Height;
			var leftX = area.X - Width;
			var rightX = area.X + area.Width;

			spriteBatch.Draw(Tileset.GetTile(CornerIndices[0]), new Vector2(leftX, topY));
			spriteBatch.Draw(Tileset.GetTile(CornerIndices[1]), new Vector2(rightX, topY));
			spriteBatch.Draw(Tileset.GetTile(CornerIndices[2]), new Vector2(leftX, bottomY));
			spriteBatch.Draw(Tileset.GetTile(CornerIndices[3]), new Vector2(rightX, bottomY));

			spriteBatch.Draw(Tileset.GetTile(TopIndices[0]), new Rectangle(area.X, topY, area.Width, Width), Color.White);
			spriteBatch.Draw(Tileset.GetTile(BottomIndices[0]), new Rectangle(area.X, bottomY, area.Width, Width), Color.White);
			spriteBatch.Draw(Tileset.GetTile(LeftIndices[0]), new Rectangle(leftX, area.Y, Width, area.Height), Color.White);
			spriteBatch.Draw(Tileset.GetTile(RightIndices[0]), new Rectangle(rightX, area.Y, Width, area.Height), Color.White);
		}
	}
}