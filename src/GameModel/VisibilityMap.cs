using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Player;

namespace Warcraft.GameModel
{
	public class VisibilityMap
	{
		public TileVisibility[,] Map { get; set; }
		public int MaxVisibility { get; private set; }
		public int TotalMapTiles { get; private set; }
		public int UnseenTiles { get; private set; }

		public int Height => Map.GetLength(0) - 2 * MaxVisibility;
		public int Width => Map.GetLength(1) - 2 * MaxVisibility;

		public VisibilityMap(int width, int height, int maxVisibility)
		{
			MaxVisibility = maxVisibility;
			Map = new TileVisibility[height + 2 * MaxVisibility, width + 2 * MaxVisibility];
			for (var y = 0; y < Map.GetLength(0); y++)
			{
				for (var x = 0; x < Map.GetLength(1); x++)
				{
					Map[y, x] = TileVisibility.None;
				}
			}
			UnseenTiles = TotalMapTiles = width * height;
		}

		/// <summary>
		/// Returns the percentage of tiles seen.
		/// </summary>
		public int SeenPercent(int max)
		{
			return max * (TotalMapTiles - UnseenTiles) / TotalMapTiles;
		}

		/// <summary>
		/// Returns the tile type at (<paramref name="x"/>, <paramref name="y"/>).
		/// </summary>
		public TileVisibility GetTileType(int x, int y)
		{
			if (-MaxVisibility > x || -MaxVisibility > y)
				return TileVisibility.None;
			if (Map.GetLength(0) <= y + MaxVisibility || Map.GetLength(1) <= x + MaxVisibility)
				return TileVisibility.None;

			return Map[y + MaxVisibility, x + MaxVisibility];
		}

		/// <summary>
		/// Updates the visibility of tiles and assets.
		/// </summary>
		public void Update(List<PlayerAsset> assets)
		{
			if (Settings.Debug.Flash)
			{
				for (var y = 0; y < Map.GetLength(0); y++)
				{
					for (var x = 0; x < Map.GetLength(1); x++)
					{
						Map[y, x] = TileVisibility.Visible;
					}
				}
			}
			else
			{
				for (var y = 0; y < Map.GetLength(0); y++)
				{
					for (var x = 0; x < Map.GetLength(1); x++)
					{
						if (Map[y, x] == TileVisibility.Visible || Map[y, x] == TileVisibility.Partial)
							Map[y, x] = TileVisibility.Seen;
						else if (Map[y, x] == TileVisibility.PartialPartial)
							Map[y, x] = TileVisibility.SeenPartial;
					}
				}

				foreach (var asset in assets)
				{
					var anchor = asset.TilePosition;
					var sight = asset.EffectiveSight + asset.Data.Size / 2;
					var sightSquared = sight * sight;
					anchor.X += asset.Data.Size / 2;
					anchor.Y += asset.Data.Size / 2;

					for (var x = 0; x <= sight; x++)
					{
						var xSquared = x * x;
						var xSquared1 = x != 0 ? (x - 1) * (x - 1) : 0;

						for (var y = 0; y <= sight; y++)
						{
							var ySquared = y * y;
							var ySquared1 = y != 0 ? (y - 1) * (y - 1) : 0;

							if (xSquared + ySquared < sightSquared)
							{
								Map[anchor.Y - y + MaxVisibility, anchor.X - x + MaxVisibility] = TileVisibility.Visible;
								Map[anchor.Y - y + MaxVisibility, anchor.X + x + MaxVisibility] = TileVisibility.Visible;
								Map[anchor.Y + y + MaxVisibility, anchor.X - x + MaxVisibility] = TileVisibility.Visible;
								Map[anchor.Y + y + MaxVisibility, anchor.X + x + MaxVisibility] = TileVisibility.Visible;
							}
							else if (xSquared1 + ySquared1 < sightSquared)
							{
								var currentVisibility = Map[anchor.Y - y + MaxVisibility, anchor.X - x + MaxVisibility];
								if (currentVisibility == TileVisibility.Seen)
									Map[anchor.Y - y + MaxVisibility, anchor.X - x + MaxVisibility] = TileVisibility.Partial;
								else if (currentVisibility == TileVisibility.None || currentVisibility == TileVisibility.SeenPartial)
									Map[anchor.Y - y + MaxVisibility, anchor.X - x + MaxVisibility] = TileVisibility.PartialPartial;

								currentVisibility = Map[anchor.Y - y + MaxVisibility, anchor.X + x + MaxVisibility];
								if (currentVisibility == TileVisibility.Seen)
									Map[anchor.Y - y + MaxVisibility, anchor.X + x + MaxVisibility] = TileVisibility.Partial;
								else if (currentVisibility == TileVisibility.None || currentVisibility == TileVisibility.SeenPartial)
									Map[anchor.Y - y + MaxVisibility, anchor.X + x + MaxVisibility] = TileVisibility.PartialPartial;

								currentVisibility = Map[anchor.Y + y + MaxVisibility, anchor.X - x + MaxVisibility];
								if (currentVisibility == TileVisibility.Seen)
									Map[anchor.Y + y + MaxVisibility, anchor.X - x + MaxVisibility] = TileVisibility.Partial;
								else if (currentVisibility == TileVisibility.None || currentVisibility == TileVisibility.SeenPartial)
									Map[anchor.Y + y + MaxVisibility, anchor.X - x + MaxVisibility] = TileVisibility.PartialPartial;

								currentVisibility = Map[anchor.Y + y + MaxVisibility, anchor.X + x + MaxVisibility];
								if (currentVisibility == TileVisibility.Seen)
									Map[anchor.Y + y + MaxVisibility, anchor.X + x + MaxVisibility] = TileVisibility.Partial;
								else if (currentVisibility == TileVisibility.None || currentVisibility == TileVisibility.SeenPartial)
									Map[anchor.Y + y + MaxVisibility, anchor.X + x + MaxVisibility] = TileVisibility.PartialPartial;
							}
						}
					}
				}
			}

			Point min, max;
			min.Y = MaxVisibility;
			max.Y = Map.GetLength(0) - MaxVisibility;
			min.X = MaxVisibility;
			max.X = Map.GetLength(1) - MaxVisibility;
			UnseenTiles = 0;
			for (var y = min.Y; y < max.Y; y++)
			{
				for (var x = min.X; x < max.X; x++)
				{
					if (Map[y, x] == TileVisibility.None)
						UnseenTiles++;
				}
			}
		}
	}

	public enum TileVisibility
	{
		None,
		PartialPartial,
		Partial,
		Visible,
		SeenPartial,
		Seen
	}
}