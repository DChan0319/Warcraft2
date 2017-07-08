using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Warcraft.Screens.Manager;

namespace Warcraft.Assets
{
	/// <summary>
	/// Represents a tileset which can be used to write text.
	/// </summary>
	public class FontTileset : MulticolorTileset
	{
		private List<int> characterWidths;
		private List<List<int>> deltaWidths;
		private List<int> characterTops;
		private List<int> characterBottoms;

		/// <summary>
		/// Creates a new instance of a <see cref="FontTileset"/>.
		/// </summary>
		public FontTileset(RecolorMap recolorMap) : base(recolorMap)
		{ }

		/// <summary>
		/// Reads the <paramref name="dataFile"/> and creates a font tileset.
		/// </summary>
		protected override void Load(TextReader dataFile)
		{
			base.Load(dataFile);

			// Initialize lists
			characterWidths = new List<int>(new int[Tiles[0].Count]);
			deltaWidths = new List<List<int>>(new List<int>[Tiles[0].Count]).ToList();
			characterTops = new List<int>(new int[Tiles[0].Count]);
			characterBottoms = new List<int>(new int[Tiles[0].Count]);

			// Read the character widths
			for (var i = 0; i < Tiles[0].Count; i++)
			{
				var widths = dataFile.ReadLine();
				if (widths == null)
					throw new FormatException("Invalid font data file format.");

				characterWidths[i] = int.Parse(widths);
			}

			// Read the delta widths for each character
			for (var fromIndex = 0; fromIndex < Tiles[0].Count; fromIndex++)
			{
				deltaWidths[fromIndex] = new List<int>(new int[Tiles[0].Count]);

				var line = dataFile.ReadLine();
				if (line == null)
					throw new FormatException("Invalid font data file format.");

				var values = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

				if (values.Length != Tiles[0].Count)
					throw new Exception($"Number of character values not equal to tile count ({values.Length} vs {Tiles[0].Count}).");

				for (var toIndex = 0; toIndex < Tiles[0].Count; toIndex++)
					deltaWidths[fromIndex][toIndex] = int.Parse(values[toIndex]);
			}

			// Calculate the top and bottom opaque pixels for each character
			var bottomOccurrence = Enumerable.Repeat(0, Tiles[0].Count + 1).ToList();
			for (var i = 0; i < Tiles[0].Count; i++)
			{
				int topOpaque, bottomOpaque;
				GetOpaqueBounds(Tiles[0][i], out topOpaque, out bottomOpaque);
				characterTops[i] = topOpaque;
				characterBottoms[i] = bottomOpaque;
				bottomOccurrence[bottomOpaque]++;
			}
		}

		/// <summary>
		/// Returns the top and bottom boundaries where the texture
		/// is opaque through <paramref name="topOpaque"/> and <paramref name="bottomOpaque"/>.
		/// </summary>
		private static void GetOpaqueBounds(Texture2D texture, out int topOpaque, out int bottomOpaque)
		{
			topOpaque = texture.Height;
			bottomOpaque = 0;

			var data = new Color[texture.Width * texture.Height];
			texture.GetData(data);

			// Go through each pixel
			for (var y = 0; y < texture.Height; y++)
			{
				for (var x = 0; x < texture.Width; x++)
				{
					// Check if the pixel is opaque
					if (data[x + y * texture.Width].A > 0)
					{
						if (y < topOpaque)
							topOpaque = y;
						bottomOpaque = y;
					}
				}
			}
		}

		/// <summary>
		/// Draws the string in <paramref name="text"/> at the position (<paramref name="x"/>, <paramref name="y"/>).
		/// </summary>
		public void DrawText(int x, int y, string text)
		{
			DrawTextColored(0, x, y, text);
		}

		/// <summary>
		/// Draws the string in <paramref name="text"/> at the position (<paramref name="x"/>, <paramref name="y"/>)
		/// with the color at <paramref name="colorIndex"/>.
		/// </summary>
		public void DrawTextColored(int colorIndex, int x, int y, string text)
		{
			var lastChar = 0;
			for (var i = 0; i < text.Length; i++)
			{
				var nextChar = text[i] - ' ';

				if (i != 0)
					x += characterWidths[lastChar] + deltaWidths[lastChar][nextChar];

				ScreenManager.SpriteBatch.Draw(Tiles[colorIndex][nextChar], new Vector2(x, y));
				lastChar = nextChar;
			}
		}

		/// <summary>
		/// Draws the string in <paramref name="text"/> at the position (<paramref name="x"/>, <paramref name="y"/>)
		/// with the color at <paramref name="colorIndex"/> and with a shadow with color at <paramref name="shadowColorIndex"/>.
		/// </summary>
		public void DrawTextWithShadow(int x, int y, int colorIndex, int shadowColorIndex, int shadowWidth, string text)
		{
			if (colorIndex < 0 || colorIndex >= Tiles.Count)
				return;

			if (shadowColorIndex < 0 || shadowColorIndex >= Tiles.Count)
				return;

			DrawTextColored(shadowColorIndex, x + shadowWidth, y + shadowWidth, text);
			DrawTextColored(colorIndex, x, y, text);
		}

		/// <summary>
		/// Returns the dimensions of <paramref name="text"/>.
		/// </summary>
		public Rectangle MeasureText(string text)
		{
			var lastChar = 0;
			int width = 0, height = TileHeight;
			int bottom = 0, top = TileHeight;

			for (var i = 0; i < text.Length; i++)
			{
				var nextChar = text[i] - ' ';

				if (i != 0)
					width += deltaWidths[lastChar][nextChar];

				width += characterWidths[nextChar];

				if (characterTops[nextChar] < top)
					top = characterTops[nextChar];

				if (characterBottoms[nextChar] > bottom)
					bottom = characterBottoms[nextChar];

				lastChar = nextChar;
			}

			return new Rectangle(top, bottom, width, height);
		}
	}

	public enum FontSize
	{
		Small, Medium, Large, Giant
	}
}