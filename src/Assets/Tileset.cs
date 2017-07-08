using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Warcraft.Assets.Base;
using Warcraft.Extensions;
using Warcraft.Screens.Manager;
using Warcraft.Util;

namespace Warcraft.Assets
{
	/// <summary>
	/// Represents a set of tiles.
	/// </summary>
	public class Tileset : Asset
	{
		/// <summary>
		/// Width of a tile in the tile set.
		/// </summary>
		public int TileWidth { get; private set; }
		/// <summary>
		/// Height of a tile in the tile set.
		/// </summary>
		public int TileHeight { get; private set; }

		/// <summary>
		/// Tile textures, indexed by tile name.
		/// </summary>
		protected Dictionary<int, Texture2D> Tiles { get; } = new Dictionary<int, Texture2D>();

		/// <summary>
		/// Clipped tile textures, indexed by tile name.
		/// </summary>
		protected Dictionary<int, Texture2D> ClippedTiles { get; } = new Dictionary<int, Texture2D>();

		/// <summary>
		/// Number of tiles stored.
		/// </summary>
		private int count;
		public int Count
		{
			get { return count; }
			set
			{
				if (value < 0) return;
				if (TileWidth == 0 || TileHeight == 0) return;
				if (value < count)
				{
					var toRemove = Tiles.Keys.Where(key => key >= count);
					foreach (var i in toRemove)
						Tiles.Remove(i);

					count = value;
					UpdateGroupNames();
					return;
				}

				for (var i = count; i < value; i++)
					Tiles[i] = new Texture2D(ScreenManager.Graphics, TileWidth, TileHeight);
				count = value;
			}
		}

		private Dictionary<string, int> GroupSteps { get; } = new Dictionary<string, int>();
		private List<string> GroupNames { get; } = new List<string>();

		/// <summary>
		/// Reads the <paramref name="dataFile"/> and creates a tileset.
		/// </summary>
		protected override void Load(TextReader dataFile)
		{
			// Create png file path
			var pngPath = dataFile.ReadLine();
			if (pngPath == null)
				throw new Exception("Missing tileset texture path.");
			pngPath = Path.Combine(Paths.Image, pngPath);

			if (!File.Exists(pngPath))
				throw new FileNotFoundException("Tileset image file not found.", pngPath);

			// Open the png file for reading
			using (var tilesetStream = new FileStream(pngPath, FileMode.Open))
			using (var tilesetImage = new Bitmap(tilesetStream))
			{
				//  Get number of tiles
				int numTiles;
				if (!int.TryParse(dataFile.ReadLine(), out numTiles))
					throw new FormatException("Invalid data file format.");
				count = numTiles;

				// Calculate tile dimensions
				TileWidth = tilesetImage.Width;
				TileHeight = tilesetImage.Height / numTiles;

				Tiles.Clear();

				// Create and add tile to Tiles
				for (var i = 0; i < numTiles; i++)
				{
					var tileName = dataFile.ReadLine();
					if (tileName == null)
						throw new IndexOutOfRangeException($"Fewer tiles than expected (expected: {numTiles}, read: {i}).");

					var tileRectangle = new Rectangle(0, TileHeight * i, TileWidth, TileHeight);
					using (var bitmap = tilesetImage.QuickClone(tileRectangle))
					{
						var tex = bitmap.ToTexture2D();
						tex.Name = tileName;
						Tiles.Add(i, tex);
						ClippedTiles.Add(i, CreateClippingMask(tex));
					}
				}

				UpdateGroupNames();
			}
		}

		/// <summary>
		/// Updates the list of group names and steps.
		/// </summary>
		private void UpdateGroupNames()
		{
			GroupSteps.Clear();
			GroupNames.Clear();
			for (int i = 0; i < Count; i++)
			{
				string groupName = null;
				int groupStep = 0;

				if (ParseGroupName(Tiles.First(t => t.Key == i).Value.Name, ref groupName, ref groupStep))
				{
					if (GroupSteps.ContainsKey(groupName))
					{
						if (GroupSteps[groupName] <= groupStep)
							GroupSteps[groupName] = groupStep + 1;
					}
					else
					{
						GroupSteps[groupName] = groupStep + 1;
						GroupNames.Add(groupName);
					}
				}
			}
		}

		private bool ParseGroupName(string tileName, ref string aniname, ref int anistep)
		{
			var lastIndex = tileName.Length;

			if (lastIndex == 0)
				return false;

			do
			{
				lastIndex--;

				if (char.IsDigit(tileName[lastIndex]))
					continue;

				if (lastIndex + 1 == tileName.Length)
					return false;

				aniname = tileName.Substring(0, lastIndex + 1);
				anistep = int.Parse(tileName.Substring(lastIndex + 1));
				return true;
			} while (lastIndex != 0);

			return false;
		}

		/// <summary>
		/// Creates a clipping mask for the provided texture.
		/// </summary>
		public Texture2D CreateClippingMask(Texture2D texture)
		{
			var pixels = new uint[texture.Width * texture.Height];
			texture.GetData(pixels);

			for (var i = 0; i < pixels.Length; i++)
			{
				var alpha = (pixels[i] & 0xFF000000) >> 24;
				pixels[i] = alpha != 0 ? 0xFFFFFFFF : 0;
			}

			var result = new Texture2D(ScreenManager.Graphics, texture.Width, texture.Height);
			result.SetData(pixels);
			return result;
		}

		/// <summary>
		/// Returns the index of the tile with the given <paramref name="name"/>
		/// or -1 if it does not exist.
		/// </summary>
		public int GetIndex(string name)
		{
			try { return Tiles.First(t => t.Value.Name == name).Key; }
			catch { return -1; }
		}

		/// <summary>
		/// Returns the <see cref="Texture2D"/> that matches the given <paramref name="index"/>
		/// or null if it does not exist.
		/// </summary>
		public Texture2D GetTile(int index) => Tiles.FirstOrDefault(t => t.Key == index).Value;

		/// <summary>
		/// Returns the first tile that matches <paramref name="tileIndex"/> or null if it does not exist.
		/// </summary>
		public Texture2D GetClippedTile(int tileIndex) => ClippedTiles.FirstOrDefault(t => t.Key == tileIndex).Value;
	}
}