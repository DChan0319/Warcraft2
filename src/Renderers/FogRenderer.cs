using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Warcraft.Assets;
using Warcraft.Extensions;
using Warcraft.GameModel;
using Warcraft.Screens.Manager;

namespace Warcraft.Renderers
{
	public class FogRenderer
	{
		private Tileset Tileset { get; }
		private VisibilityMap Map { get; }
		private readonly int noneIndex;
		private readonly int seenIndex;
		private readonly int partialIndex;
		private readonly List<int> fogIndices;
		private readonly List<int> blackIndices;

		private static readonly List<bool> UnknownFog = new List<bool>(new bool[0x100]);
		private static readonly List<bool> UnknownBlack = new List<bool>(new bool[0x100]);

		public FogRenderer(Tileset tileset, VisibilityMap map)
		{
			fogIndices = new List<int>();
			blackIndices = new List<int>();

			int index, visibleIndex;
			var originalValues = new List<int> { 0x0B, 0x16, 0xD0, 0x68, 0x07, 0x94, 0xE0, 0x29, 0x03, 0x06, 0x14, 0x90, 0x60, 0xC0, 0x09, 0x28, 0x01, 0x02, 0x04, 0x10, 0x80, 0x40, 0x20, 0x08 };

			Tileset = tileset;
			Map = map;
			visibleIndex = Tileset.GetIndex("visible");
			noneIndex = Tileset.GetIndex("none");
			seenIndex = Tileset.GetIndex("seen");
			partialIndex = Tileset.GetIndex("partial");

			for (index = 0; index < 0x100; index++)
				fogIndices.Add(Tileset.GetIndex("pf-" + index));

			fogIndices[0x00] = seenIndex;
			fogIndices[0x03] = fogIndices[0x07];
			fogIndices[0x06] = fogIndices[0x07];
			fogIndices[0x14] = fogIndices[0x94];
			fogIndices[0x90] = fogIndices[0x94];
			fogIndices[0x60] = fogIndices[0xE0];
			fogIndices[0xC0] = fogIndices[0xE0];
			fogIndices[0x09] = fogIndices[0x29];
			fogIndices[0x28] = fogIndices[0x29];

			// Linux: A bunch of other FogIndices are commented here.

			for (index = 0; index < 0x100; index++)
				blackIndices.Add(Tileset.GetIndex("pb-" + index));

			blackIndices[0x00] = noneIndex;
			blackIndices[0x03] = blackIndices[0x07];
			blackIndices[0x06] = blackIndices[0x07];
			blackIndices[0x14] = blackIndices[0x94];
			blackIndices[0x90] = blackIndices[0x94];
			blackIndices[0x60] = blackIndices[0xE0];
			blackIndices[0xC0] = blackIndices[0xE0];
			blackIndices[0x09] = blackIndices[0x29];
			blackIndices[0x28] = blackIndices[0x29];

			// Linux: A bunch of other BlackIndices are commented here.

			var nextIndex = Tileset.Count;
			Tileset.Count = Tileset.Count + (0x100 - originalValues.Count) * 2;
			// Todo: Create Clipping Masks
			for (var allowedHamming = 1; allowedHamming < 8; allowedHamming++)
			{
				for (var value = 0; value < 0x100; value++)
				{
					if (fogIndices[value] == -1)
					{
						var bestMatch = -1;
						var bestHamming = 8;

						foreach (var originalValue in originalValues)
						{
							var currentHamming = HammingDistance(originalValue, value);
							if (currentHamming == HammingDistance(0, ~originalValue & value))
							{
								if (currentHamming < bestHamming)
								{
									bestHamming = currentHamming;
									bestMatch = originalValue;
								}
							}
						}

						if (bestHamming <= allowedHamming)
						{
							var currentValue = value & ~bestMatch;
							var firstBest = bestMatch;

							bestMatch = -1;
							bestHamming = 8;

							foreach (var originalValue in originalValues)
							{
								var currentHamming = HammingDistance(originalValue, currentValue);
								if (currentHamming == HammingDistance(0, ~originalValue & currentValue))
								{
									if (currentHamming < bestHamming)
									{
										bestHamming = currentHamming;
										bestMatch = originalValue;
									}
								}
							}

							// Todo: DuplicateClippedTile
							//FogIndices[bestMatch] = FogIndices[firstBest];
							fogIndices[value] = nextIndex;
							//BlackIndices[bestMatch] = BlackIndices[firstBest];
							blackIndices[value] = nextIndex + 1;
							nextIndex += 2;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns the binary hamming distance between two numbers.
		/// </summary>
		private static int HammingDistance(int v1, int v2)
		{
			var delta = v1 ^ v2;
			var distance = 0;

			while (delta != 0)
			{
				if ((delta & 0x01) != 0)
				{
					distance++;
				}
				delta >>= 1;
			}

			return distance;
		}

		public void DrawMap(Rectangle area)
		{
			for (int yindex = area.Y / Tileset.TileHeight, ypos = -(area.Y % Tileset.TileHeight); ypos < area.Height; yindex++, ypos += Tileset.TileHeight)
			{
				for (int xindex = area.X / Tileset.TileWidth, xpos = -(area.X % Tileset.TileWidth); xpos < area.Width; xindex++, xpos += Tileset.TileWidth)
				{
					var tileType = Map.GetTileType(xindex, yindex);

					if (tileType == TileVisibility.None)
					{
						ScreenManager.SpriteBatch.Draw(Tileset.GetTile(noneIndex), new Vector2(xpos, ypos));
						continue;
					}

					if (tileType == TileVisibility.Visible)
						continue;

					if (tileType == TileVisibility.Seen || tileType == TileVisibility.SeenPartial)
						ScreenManager.SpriteBatch.Draw(Tileset.GetTile(seenIndex), new Vector2(xpos, ypos));

					if (tileType == TileVisibility.Partial || tileType == TileVisibility.PartialPartial)
					{
						int visibilityIndex = 0, visibilityMask = 0x1;

						for (var yOff = -1; yOff < 2; yOff++)
						{
							for (var xOff = -1; xOff < 2; xOff++)
							{
								if (xOff != 0 || yOff != 0)
								{
									var visibleTile = Map.GetTileType(xindex + xOff, yindex + yOff);
									if (visibleTile == TileVisibility.Visible)
										visibilityIndex |= visibilityMask;

									visibilityMask <<= 1;
								}
							}
						}

						if (fogIndices[visibilityIndex] == -1)
						{
							if (!UnknownFog[visibilityIndex])
							{
								Trace.TraceError($"Unknown fog 0x{visibilityIndex:X2} @ ({xindex}, {yindex})");
								UnknownFog[visibilityIndex] = true;
							}
						}

						ScreenManager.SpriteBatch.Draw(Tileset.GetTile(fogIndices[visibilityIndex]), new Vector2(xpos, ypos));
					}

					if (tileType == TileVisibility.SeenPartial || tileType == TileVisibility.PartialPartial)
					{
						int visibilityIndex = 0, visibilityMask = 0x1;

						for (var yOff = -1; yOff < 2; yOff++)
						{
							for (var xOff = -1; xOff < 2; xOff++)
							{
								if (xOff != 0 || yOff != 0)
								{
									var visibleTile = Map.GetTileType(xindex + xOff, yindex + yOff);
									if (visibleTile == TileVisibility.Visible || visibleTile == TileVisibility.Partial || visibleTile == TileVisibility.Seen)
										visibilityIndex |= visibilityMask;

									visibilityMask <<= 1;
								}
							}
						}

						if (blackIndices[visibilityIndex] == -1)
						{
							if (!UnknownBlack[visibilityIndex])
							{
								Trace.TraceError($"Unknown black 0x{visibilityIndex:X2} @ ({xindex}, {yindex})");
								UnknownBlack[visibilityIndex] = true;
							}
						}

						ScreenManager.SpriteBatch.Draw(Tileset.GetTile(blackIndices[visibilityIndex]), new Vector2(xpos, ypos));
					}
				}
			}
		}

		/// <summary>
		/// Draws the minimap fog of the <see cref="VisibilityMap"/> on the current render target.
		/// </summary>
		public void DrawMiniMap()
		{
			for (var ypos = 0; ypos < Map.Height; ypos++)
			{
				var xpos = 0;

				while (xpos < Map.Width)
				{
					var tileType = Map.GetTileType(xpos, ypos);
					var xanchor = xpos;
					while (xpos < Map.Width && Map.GetTileType(xpos, ypos) == tileType)
						xpos++;

					if (tileType == TileVisibility.Visible)
						continue;

					uint color;
					switch (tileType)
					{
						case TileVisibility.None: color = 0xFF000000; break;
						case TileVisibility.Seen:
						case TileVisibility.SeenPartial: color = 0xA8000000; break;
						default: color = 0x54000000; break;
					}
					ScreenManager.SpriteBatch.DrawLine(color.ToColor(), new Vector2(xanchor, ypos), new Vector2(xpos, ypos));
				}
			}
		}
	}
}