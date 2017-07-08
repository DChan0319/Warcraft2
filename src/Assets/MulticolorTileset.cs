using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Warcraft.Assets
{
	/// <summary>
	/// Represents a set of tilesets that have been recolored using a <see cref="RecolorMap"/>.
	/// </summary>
	public class MulticolorTileset : Tileset
	{
		/// <summary>
		/// List of colored tile textures, each element a dictionary of tiles.
		/// </summary>
		protected new List<Dictionary<int, Texture2D>> Tiles { get; } = new List<Dictionary<int, Texture2D>>();

		/// <summary>
		/// The <see cref="RecolorMap"/> used for this tileset.
		/// </summary>
		public RecolorMap RecolorMap { get; }

		/// <summary>
		/// Creates a new instance of a <see cref="MulticolorTileset"/>.
		/// </summary>
		public MulticolorTileset(RecolorMap recolorMap)
		{
			RecolorMap = recolorMap;
		}

		/// <summary>
		/// Reads from <paramref name="dataFile"/> and creates a tileset
		/// and applies coloring using <see cref="RecolorMap"/>.
		/// </summary>
		protected override void Load(TextReader dataFile)
		{
			base.Load(dataFile);

			// Add original tileset
			Tiles.Add(base.Tiles);

			// Recolor each tileset and store it
			for (var i = 1; i < RecolorMap.GroupCount; i++)
			{
				var recoloredTiles = new Dictionary<int, Texture2D>();

				// Color the tile and put it in the dictionary
				foreach (var tile in base.Tiles)
					recoloredTiles.Add(tile.Key, RecolorMap.RecolorTexture(i, tile.Value));

				Tiles.Add(recoloredTiles);
			}
		}

		/// <summary>
		/// Returns the first tile that matches <paramref name="colorIndex"/> and <paramref name="tileIndex"/>
		/// or null if it does not exist.
		/// </summary>
		public Texture2D GetTile(int tileIndex, int colorIndex) => Tiles[colorIndex].FirstOrDefault(t => t.Key == tileIndex).Value;
	}
}