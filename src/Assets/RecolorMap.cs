using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Warcraft.Assets.Base;
using Warcraft.Extensions;
using Warcraft.Screens.Manager;
using Warcraft.Util;

namespace Warcraft.Assets
{
	/// <summary>
	/// Represents a set of colors which can be used to recolor textures.
	/// </summary>
	public class RecolorMap : Asset
	{
		protected readonly Dictionary<string, int> Mapping = new Dictionary<string, int>();
		protected uint[,] Colors;
		protected uint[,] OriginalColors;

		/// <summary>
		/// Gets the number of groups in the recolor map.
		/// </summary>
		public int GroupCount => Colors.GetLength(0);

		/// <summary>
		/// Reads from <paramref name="dataFile"/> and creates a recolor map.
		/// </summary>
		protected override void Load(TextReader dataFile)
		{
			// Create png file path from current directory + path
			var pngPath = dataFile.ReadLine();
			if (pngPath == null)
				throw new Exception("Missing recolor map texture path.");
			pngPath = Path.Combine(Paths.Image, pngPath);

			if (!File.Exists(pngPath))
				throw new FileNotFoundException("Recolor map image file not found.", pngPath);

			// Open the png file for reading
			using (var recolorMapStream = new FileStream(pngPath, FileMode.Open))
			using (var recolorMapImage = new Bitmap(recolorMapStream))
			{
				// Initialize color arrays
				Colors = new uint[recolorMapImage.Height, recolorMapImage.Width];
				OriginalColors = new uint[recolorMapImage.Height, recolorMapImage.Width];

				for (var y = 0; y < Colors.GetLength(0); y++)
				{
					for (var x = 0; x < Colors.GetLength(1); x++)
					{
						OriginalColors[y, x] = ((uint)recolorMapImage.GetPixel(x, y).ToArgb()).SwapRnB();
						Colors[y, x] = ((uint)(recolorMapImage.GetPixel(x, y).ToArgb() | 0xFF000000)).SwapRnB();
					}
				}

				//  Get number of colors
				int numColors;
				if (!int.TryParse(dataFile.ReadLine(), out numColors))
					throw new FormatException("Invalid recolor map data file format.");

				if (numColors != Colors.GetLength(0))
					throw new Exception("Number of colors does not match size of color list.");

				for (var i = 0; i < numColors; i++)
				{
					var line = dataFile.ReadLine();
					if (line == null)
						throw new FormatException("Invalid recolor data map file format.");

					Mapping[line] = i;
				}
			}
		}

		/// <summary>
		/// Returns the value of the color for <paramref name="colorName"/>.
		/// </summary>
		public int FindColor(string colorName)
		{
			if (!Mapping.ContainsKey(colorName))
				return -1;

			return Mapping[colorName];
		}

		/// <summary>
		/// Returns the color value for the <paramref name="groupIndex"/> and <paramref name="colorIndex"/>.
		/// </summary>
		public uint ColorValue(int groupIndex, int colorIndex)
		{
			if (groupIndex < 0 || colorIndex < 0 || groupIndex >= OriginalColors.GetLength(0))
				return 0x00000000;

			if (colorIndex >= OriginalColors.GetLength(1))
				return 0x00000000;

			return OriginalColors[groupIndex, colorIndex];
		}

		/// <summary>
		/// Returns a new <see cref="Texture2D"/> of <paramref name="texture"/> with a color applied.
		/// </summary>
		public Texture2D RecolorTexture(int colorIndex, Texture2D texture)
		{
			if (colorIndex < 0 || colorIndex >= Colors.GetLength(0))
				return null;

			var pixels = new uint[texture.Width * texture.Height];
			texture.GetData(pixels);

			for (var i = 0; i < pixels.Length; i++)
			{
				var alpha = (pixels[i] & 0xFF000000) >> 24;

				// Skip if the pixel is completely transparent
				if (alpha == 0)
					continue;

				for (var index = 0; index < Colors.GetLength(1); index++)
				{
					if ((pixels[i] & 0x00FFFFFF) == (Colors[0, index] & 0x00FFFFFF))
					{
						var newColor = Colors[colorIndex, index];

						// Recalculate color with applied alpha
						var r = newColor & 0x000000FF;
						var g = newColor & 0x0000FF00;
						var b = newColor & 0x00FF0000;
						pixels[i] = (alpha << 24) | ((b * alpha / 0xFF) & 0x00FF0000) | ((g * alpha / 0xFF) & 0x0000FF00) | ((r * alpha / 0xFF) & 0x000000FF);
						break;
					}
				}
			}

			var result = new Texture2D(ScreenManager.Graphics, texture.Width, texture.Height);
			result.SetData(pixels);
			return result;
		}
	}
}