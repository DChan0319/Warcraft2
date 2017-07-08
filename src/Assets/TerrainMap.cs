using System;
using System.IO;
using Warcraft.App;
using Warcraft.Assets.Base;
using Warcraft.Player;

namespace Warcraft.Assets
{
	public class TerrainMap : Asset
	{
		/// <summary>
		/// Map Name
		/// </summary>
		public string MapName { get; set; }

		/// <summary>
		/// Map Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Array of allowed AI scripts on the map, delimited by spaces.
		/// </summary>
		public string[] AiScripts { get; set; }

		/// <summary>
		/// Width of the map (in tiles), not including the borders.
		/// </summary>
		public int MapWidth => MapTiles.GetLength(1) - 2;

		/// <summary>
		/// Height of the map (in tiles), not including the borders.
		/// </summary>
		public int MapHeight => MapTiles.GetLength(0) - 2;

		/// <summary>
		/// Two-dimensional array of <see cref="TileType"/> representing the map.
		/// </summary>
		public TileType[,] MapTiles { get; set; }

		protected TerrainMap() { }

		protected TerrainMap(TerrainMap other)
		{
			MapTiles = new TileType[other.MapHeight + 2, other.MapWidth + 2];
			for (var y = 0; y < MapHeight + 2; y++)
			{
				for (var x = 0; x < MapWidth + 2; x++)
				{
					MapTiles[y, x] = other.MapTiles[y, x];
				}
			}
			MapName = other.MapName;
		}

		/// <summary>
		/// Reads the <paramref name="dataFile"/> and creates a terrain map.
		/// </summary>
		protected override void Load(TextReader dataFile)
		{
			// Read map name
			MapName = dataFile.ReadLine();

			// Parse map dimensions
			var dimensionsLine = dataFile.ReadLine();
			if (dimensionsLine == null)
			{
				throw new FormatException("Invalid terrain map data format (missing dimensions).");
			}
			var dimensions = dimensionsLine.Split(' ');
			int width, height;
			if (!int.TryParse(dimensions[0], out width) || !int.TryParse(dimensions[1], out height))
				throw new FormatException("Invalid terrain map data file format (invalid dimensions).");

			Description = dataFile.ReadLine();
			AiScripts = dataFile.ReadLine()?.Split();

			if (width < 8 || height < 8)
				throw new Exception("Invalid map dimensions (map too small).");

			// Initialize Map Size
			MapTiles = new TileType[height + 2, width + 2];

			// Go through each row
			for (var y = 0; y < height + 2; y++)
			{
				var mapRow = dataFile.ReadLine();
				if (mapRow == null)
					throw new FormatException($"Map is missing columns (expected {height}, read {y}).");

				if (mapRow.Length < width + 2)
					throw new FormatException($"Map row too short (expected {width}, read {mapRow.Length}).");

				// Go through each column
				for (var x = 0; x < width + 2; x++)
				{
					switch (mapRow[x])
					{
						case 'G': MapTiles[y, x] = TileType.Grass; break;
						case 'F': MapTiles[y, x] = TileType.Tree; break;
						case 'D': MapTiles[y, x] = TileType.Dirt; break;
						case 'W': MapTiles[y, x] = TileType.Wall; break;
						case 'w': MapTiles[y, x] = TileType.WallDamaged; break;
						case 'R': MapTiles[y, x] = TileType.Rock; break;
						case ' ': MapTiles[y, x] = TileType.Water; break;
						default: throw new Exception($"Unknown map tile type ({mapRow[x]}) at ({x}, {y}).");
					}
				}
			}
		}

		/// <summary>
		/// Returns the <see cref="TileType"/> for the tile at <paramref name="pos"/>.
		/// </summary>
		public TileType GetTileType(Position pos)
		{
			return GetTileType(pos.X, pos.Y);
		}

		/// <summary>
		/// Returns the <see cref="TileType"/> for the tile at (<paramref name="x"/> + 1, <paramref name="y"/> + 1).
		/// </summary>
		public TileType GetTileType(int x, int y)
		{
			if (x < -1 || y < -1) return TileType.None;
			if (y + 1 > MapHeight) return TileType.None;
			if (x + 1 > MapWidth) return TileType.None;
			return MapTiles[y + 1, x + 1];
		}

		/// <summary>
		/// Changes the tile type at <paramref name="pos"/> to <paramref name="type"/>.
		/// </summary>
		public void ChangeTileType(Position pos, TileType type)
		{
			ChangeTileType(pos.X, pos.Y, type);
		}

		/// <summary>
		/// Changes the tile type at (<paramref name="x"/>, <paramref name="y"/>) to <paramref name="type"/>.
		/// </summary>
		public void ChangeTileType(int x, int y, TileType type)
		{
			if (x < -1 || y < -1) return;
			if (MapTiles.GetLength(0) <= y + 1) return;
			if (MapTiles.GetLength(1) <= x + 1) return;

			MapTiles[y + 1, x + 1] = type;
		}
	}
}
