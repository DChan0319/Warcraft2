using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Extensions;
using Warcraft.Screens.Manager;

namespace Warcraft.Renderers
{
	public class MapRenderer
	{
		/// <summary>
		/// Tileset used to render the map
		/// </summary>
		private Tileset Tileset { get; }

		/// <summary>
		/// Terrain map used to render the map
		/// </summary>
		private TerrainMap TerrainMap { get; set; }

		/// <summary>
		/// The width of the rendered map (in tiles).
		/// </summary>
		public int MapWidth => TerrainMap.MapWidth;
		/// <summary>
		/// The height of the rendered map (in tiles).
		/// </summary>
		public int MapHeight => TerrainMap.MapHeight;

		/// <summary>
		/// The width of the rendered map (in pixels).
		/// </summary>
		public int MapDetailedWidth => TerrainMap.MapWidth * Tileset.TileWidth;
		/// <summary>
		/// The height of the rendered map (in pixels).
		/// </summary>
		public int MapDetailedHeight => TerrainMap.MapHeight * Tileset.TileHeight;

		/// <summary>
		/// Returns the tile type at the specified coordinates.
		/// </summary>
		public TileType GetTileType(int x, int y) => TerrainMap.GetTileType(x / Tileset.TileWidth, y / Tileset.TileHeight);

		private List<int> GrassIndices { get; } = new List<int>();
		private List<int> TreeIndices { get; } = new List<int>();
		private int SeedlingIndex { get; }
		private int AdolescentTreeIndex { get; }
		private List<int> DirtIndices { get; } = new List<int>();
		private List<int> WaterIndices { get; } = new List<int>();
		private List<int> RockIndices { get; } = new List<int>();
		private List<int> WallIndices { get; } = new List<int>();
		private List<int> WallDamagedIndices { get; } = new List<int>();
		private List<int> RubbleIndices { get; } = new List<int>();
		private List<uint> PixelIndices { get; } = new List<uint>(new uint[(int)TileType.Max]);

		private readonly Dictionary<int, int> treeUnknown = new Dictionary<int, int>();
		private readonly Dictionary<int, int> waterUnknown = new Dictionary<int, int>();
		private readonly Dictionary<int, int> dirtUnknown = new Dictionary<int, int>();
		private readonly Dictionary<int, int> rockUnknown = new Dictionary<int, int>();

		private readonly List<bool> unknownTree = new List<bool>(new bool[0x100]);
		private readonly List<bool> unknownWater = new List<bool>(new bool[0x100]);
		private readonly List<bool> unknownDirt = new List<bool>(new bool[0x100]);
		private readonly List<bool> unknownRock = new List<bool>(new bool[0x100]);

		private readonly Dictionary<int, bool> unknownUnknownTree = new Dictionary<int, bool>();
		private readonly Dictionary<int, bool> unknownUnknownWater = new Dictionary<int, bool>();
		private readonly Dictionary<int, bool> unknownUnknownDirt = new Dictionary<int, bool>();
		private readonly Dictionary<int, bool> unknownUnknownRock = new Dictionary<int, bool>();

		/// <summary>
		/// Loads and reads from <paramref name="fileName"/> to create a map renderer.
		/// </summary>
		public MapRenderer(string fileName, Tileset tileset, TerrainMap terrainMap)
		{
			// Setup
			Tileset = tileset;
			TerrainMap = terrainMap;

			Trace.TraceInformation($"{GetType().Name}: Reading map render data for map '{terrainMap.MapName}'...");
			var sw = new Stopwatch();
			sw.Start();

			// Open data file for reading
			using (var dataFile = new StreamReader(fileName))
			{
				int itemCount;
				if (!int.TryParse(dataFile.ReadLine(), out itemCount))
					throw new FormatException("Invalid terrain map data file format.");

				for (var i = 0; i < itemCount; i++)
				{
					var line = dataFile.ReadLine();
					if (line == null)
						throw new FormatException("Missing terrain data.");
					var tokens = line.Split();

					var colorValue = Convert.ToUInt32(tokens[1], 16).SwapRnB();

					int pixelIndex;
					switch (tokens[0])
					{
						case "grass": pixelIndex = (int)TileType.Grass; break;
						case "dirt": pixelIndex = (int)TileType.Dirt; break;
						case "rock": pixelIndex = (int)TileType.Rock; break;
						case "tree": pixelIndex = (int)TileType.Tree; break;
						case "stump": pixelIndex = (int)TileType.Stump; break;
						case "water": pixelIndex = (int)TileType.Water; break;
						case "wall": pixelIndex = (int)TileType.Wall; break;
						case "wall-damaged": pixelIndex = (int)TileType.WallDamaged; break;
						case "seedling": pixelIndex = (int)TileType.Seedling; break;
						case "adolescent": pixelIndex = (int)TileType.AdolescentTree; break;
						default: pixelIndex = (int)TileType.Rubble; break;
					}
					PixelIndices[pixelIndex] = colorValue;
				}

				var index = 0;
				while (true)
				{
					var value = Tileset.GetIndex("grass-" + index);
					if (value == -1)
						break;
					GrassIndices.Add(value);
					index++;
				}

				for (var i = 0; i < 0x40; i++) TreeIndices.Add(tileset.GetIndex("tree-" + i));
				SeedlingIndex = tileset.GetIndex("seedling");
				AdolescentTreeIndex = tileset.GetIndex("adolescent");
				for (var i = 0; i < 0x100; i++) DirtIndices.Add(tileset.GetIndex("dirt-" + i));
				for (var i = 0; i < 0x100; i++) WaterIndices.Add(tileset.GetIndex("water-" + i));

				WaterIndices[0x00] = DirtIndices[0xFF];

				for (var i = 0; i < 0x100; i++) RockIndices.Add(tileset.GetIndex("rock-" + i));
				for (var i = 0; i < 0x10; i++) WallIndices.Add(tileset.GetIndex("wall-" + i));
				for (var i = 0; i < 0x10; i++) WallDamagedIndices.Add(tileset.GetIndex("wall-damaged-" + i));
				for (var i = 0; i < 0x10; i++) RubbleIndices.Add(tileset.GetIndex("rubble-" + i));

				if (!int.TryParse(dataFile.ReadLine(), out itemCount))
					throw new FormatException("Invalid terrain map data file format.");

				for (var i = 0; i < itemCount; i++)
				{
					var readLine = dataFile.ReadLine();
					if (readLine == null)
						throw new FormatException("Missing terrain data.");
					var line = readLine.Split();

					var sourceIndex = Convert.ToInt32(line[1], 16);

					switch (line[0])
					{
						case "dirt":
							for (var lineIndex = 2; lineIndex < line.Length; lineIndex++)
								DirtIndices[Convert.ToInt32(line[lineIndex], 16)] = DirtIndices[sourceIndex];
							break;

						case "rock":
							for (var lineIndex = 2; lineIndex < line.Length; lineIndex++)
								RockIndices[Convert.ToInt32(line[lineIndex], 16)] = RockIndices[sourceIndex];
							break;

						case "tree":
							for (var lineIndex = 2; lineIndex < line.Length; lineIndex++)
								TreeIndices[Convert.ToInt32(line[lineIndex], 16)] = TreeIndices[sourceIndex];
							break;

						case "water":
							for (var lineIndex = 2; lineIndex < line.Length; lineIndex++)
								WaterIndices[Convert.ToInt32(line[lineIndex], 16)] = WaterIndices[sourceIndex];
							break;

						case "wall":
							for (var lineIndex = 2; lineIndex < line.Length; lineIndex++)
								WallIndices[Convert.ToInt32(line[lineIndex], 16)] = WallIndices[sourceIndex];
							break;

						case "wall-damaged":
							for (var lineIndex = 2; lineIndex < line.Length; lineIndex++)
								WallDamagedIndices[Convert.ToInt32(line[lineIndex], 16)] = WallDamagedIndices[sourceIndex];
							break;
					}
				}

				sw.Stop();
				Trace.TraceInformation($"{GetType().Name}: Finished reading map render data in {sw.Elapsed.TotalSeconds:0.000} seconds.");
			}
		}

		/// <summary>
		/// Sets the <see cref="TerrainMap"/> to <paramref name="map"/>.
		/// </summary>
		public void SetMap(DecoratedMap map)
		{
			TerrainMap = map;
		}

		/// <summary>
		/// Renders the map on to the current render target.
		/// </summary>
		public void DrawMap(Rectangle area, int level, bool mask = false)
		{
			if (level == 0)
			{
				for (int yindex = area.Y / Tileset.TileHeight, ypos = -(area.Y % Tileset.TileHeight); ypos < area.Height; yindex++, ypos += Tileset.TileHeight)
				{
					for (int xindex = area.X / Tileset.TileWidth, xpos = -(area.X % Tileset.TileWidth); xpos < area.Width; xindex++, xpos += Tileset.TileWidth)
					{
						var tileType = TerrainMap.GetTileType(xindex, yindex);
						var pixelType = new PixelType(tileType);

						var tilePosition = new Vector2(xpos, ypos);

						if (tileType == TileType.Tree)
						{
							int treeIndex = 0, treeMask = 0x1, unknownMask = 0, displayIndex;

							for (var yoff = 0; yoff < 2; yoff++)
							{
								for (var xoff = -1; xoff < 2; xoff++)
								{
									var tile = TerrainMap.GetTileType(xindex + xoff, yindex + yoff);

									if (tile == TileType.Tree)
										treeIndex |= treeMask;
									else if (tile == TileType.None)
										unknownMask |= treeMask;

									treeMask <<= 1;
								}
							}

							if (TreeIndices[treeIndex] == -1)
							{
								if (!unknownTree[treeIndex] && unknownMask == 0)
								{
									Trace.TraceError($"Unknown tree 0x{treeIndex:X2} @ ({xindex}, {yindex}).");
									unknownTree[treeIndex] = true;
								}

								displayIndex = FindUnknown(TileType.Tree, treeIndex, unknownMask);
								if (displayIndex == -1)
								{
									if (!unknownUnknownTree.ContainsKey((treeIndex << 8) | unknownMask))
									{
										unknownUnknownTree[(treeIndex << 8) | unknownMask] = true;
										Trace.TraceError($"Unknown tree 0x{treeIndex:X2}/{unknownMask:X2} @ ({xindex}, {yindex}).");
									}
								}
							}
							else
							{
								displayIndex = TreeIndices[treeIndex];
							}

							if (displayIndex != -1)
							{
								if (!mask)
									ScreenManager.SpriteBatch.Draw(Tileset.GetTile(displayIndex), tilePosition);
								else
									ScreenManager.SpriteBatch.Draw(Tileset.GetClippedTile(displayIndex), tilePosition, pixelType.ToPixelColor());
							}
						}
						else if (tileType == TileType.Water)
						{
							int waterIndex = 0, waterMask = 0x1, unknownMask = 0, displayIndex;

							for (var yoff = -1; yoff < 2; yoff++)
							{
								for (var xoff = -1; xoff < 2; xoff++)
								{
									if (yoff != 0 || xoff != 0)
									{
										var tile = TerrainMap.GetTileType(xindex + xoff, yindex + yoff);

										if (tile == TileType.Water)
											waterIndex |= waterMask;
										else if (tile == TileType.None)
											unknownMask |= waterMask;

										waterMask <<= 1;
									}
								}
							}

							if (WaterIndices[waterIndex] == -1)
							{
								if (!unknownWater[waterIndex] && unknownMask == 0)
								{
									Trace.TraceError($"Unknown water 0x{waterIndex:X2} @ ({xindex}, {yindex}).");
									unknownWater[waterIndex] = true;
								}

								displayIndex = FindUnknown(TileType.Water, waterIndex, unknownMask);
								if (displayIndex == -1)
								{
									if (!unknownUnknownWater.ContainsKey((waterIndex << 8) | unknownMask))
									{
										unknownUnknownWater[(waterIndex << 8) | unknownMask] = true;
										Trace.TraceError($"Unknown water 0x{waterIndex:X2}/{unknownMask:X2} @ ({xindex}, {yindex}).");
									}
								}
							}
							else
							{
								displayIndex = WaterIndices[waterIndex];
							}

							if (displayIndex != -1)
							{
								if (!mask)
									ScreenManager.SpriteBatch.Draw(Tileset.GetTile(displayIndex), tilePosition);
								else
									ScreenManager.SpriteBatch.Draw(Tileset.GetClippedTile(displayIndex), tilePosition, pixelType.ToPixelColor());
							}
						}
						else if (tileType == TileType.Grass)
						{
							int otherIndex = 0, otherMask = 0x1, unknownMask = 0, displayIndex;

							for (var yoff = -1; yoff < 2; yoff++)
							{
								for (var xoff = -1; xoff < 2; xoff++)
								{
									if (yoff != 0 || xoff != 0)
									{
										var tile = TerrainMap.GetTileType(xindex + xoff, yindex + yoff);

										if (tile == TileType.Water || tile == TileType.Dirt || tile == TileType.Rock)
											otherIndex |= otherMask;
										else if (tile == TileType.None)
											unknownMask |= otherMask;

										otherMask <<= 1;
									}
								}
							}

							if (otherIndex != 0)
							{
								if (DirtIndices[otherIndex] == -1)
								{
									if (!unknownDirt[otherIndex] && unknownMask == 0)
									{
										Trace.TraceError($"Unknown dirt 0x{otherIndex:X2} @ ({xindex}, {yindex}).");
										unknownDirt[otherIndex] = true;
									}

									displayIndex = FindUnknown(TileType.Dirt, otherIndex, unknownMask);
									if (displayIndex == -1)
									{
										if (!unknownUnknownDirt.ContainsKey((otherIndex << 8) | unknownMask))
										{
											unknownUnknownDirt[(otherIndex << 8) | unknownMask] = true;
											Trace.TraceError($"Unknown dirt 0x{otherIndex:X2}/{unknownMask:X2} @ ({xindex}, {yindex}).");
										}
									}
								}
								else
								{
									displayIndex = DirtIndices[otherIndex];
								}

								if (!mask)
									ScreenManager.SpriteBatch.Draw(Tileset.GetTile(displayIndex), tilePosition);
								else
									ScreenManager.SpriteBatch.Draw(Tileset.GetClippedTile(displayIndex), tilePosition, pixelType.ToPixelColor());
							}
							else
							{
								if (!mask)
									ScreenManager.SpriteBatch.Draw(Tileset.GetTile(GrassIndices[0x00]), tilePosition);
								else
									ScreenManager.SpriteBatch.Draw(Tileset.GetClippedTile(GrassIndices[0x00]), tilePosition, pixelType.ToPixelColor());
							}
						}
						else if (tileType == TileType.Rock)
						{
							int rockIndex = 0, rockMask = 0x1, unknownMask = 0, displayIndex;

							for (var yoff = -1; yoff < 2; yoff++)
							{
								for (var xoff = -1; xoff < 2; xoff++)
								{
									if (yoff != 0 || xoff != 0)
									{
										var tile = TerrainMap.GetTileType(xindex + xoff, yindex + yoff);

										if (tile == TileType.Rock)
											rockIndex |= rockMask;
										else if (tile == TileType.None)
											unknownMask |= rockMask;

										rockMask <<= 1;
									}
								}
							}

							if (RockIndices[rockIndex] == -1)
							{
								if (!unknownRock[rockIndex] && unknownMask == 0)
								{
									Trace.TraceError($"Unknown rock 0x{rockIndex:X2} @ ({xindex}, {yindex}).");
									unknownRock[rockIndex] = true;
								}

								displayIndex = FindUnknown(TileType.Rock, rockIndex, unknownMask);
								if (displayIndex == -1)
								{
									if (!unknownUnknownRock.ContainsKey((rockIndex << 8) | unknownMask))
									{
										unknownUnknownRock[(rockIndex << 8) | unknownMask] = true;
										Trace.TraceError($"Unknown rock 0x{rockIndex:X2}/{unknownMask:X2} @ ({xindex}, {yindex}).");
									}
								}
							}
							else
							{
								displayIndex = RockIndices[rockIndex];
							}

							if (displayIndex != -1)
							{
								if (!mask)
									ScreenManager.SpriteBatch.Draw(Tileset.GetTile(displayIndex), tilePosition);
								else
									ScreenManager.SpriteBatch.Draw(Tileset.GetClippedTile(displayIndex), tilePosition, pixelType.ToPixelColor());
							}
						}
						else if (tileType == TileType.Wall || tileType == TileType.WallDamaged)
						{
							int wallIndex = 0, wallMask = 0x1, displayIndex;
							int[] xoff = { 0, 1, 0, -1 };
							int[] yoff = { -1, 0, 1, 0 };

							for (var index = 0; index < xoff.Length; index++)
							{
								var tile = TerrainMap.GetTileType(xindex + xoff[index], yindex + yoff[index]);

								if (tile == TileType.Wall || tile == TileType.WallDamaged)
								{
									wallIndex |= wallMask;
								}

								wallMask <<= 1;
							}
							displayIndex = tileType == TileType.Wall ? WallIndices[wallIndex] : WallDamagedIndices[wallIndex];

							if (displayIndex != -1)
							{
								if (!mask)
									ScreenManager.SpriteBatch.Draw(Tileset.GetTile(displayIndex), tilePosition);
								else
									ScreenManager.SpriteBatch.Draw(Tileset.GetClippedTile(displayIndex), tilePosition, pixelType.ToPixelColor());
							}
						}
						else
						{
							int tileIndex;
							switch (TerrainMap.GetTileType(xindex, yindex))
							{
								case TileType.Grass: tileIndex = (GrassIndices[0x00]); break;
								case TileType.Dirt: tileIndex = (DirtIndices[0xFF]); break;
								case TileType.Rock: tileIndex = (RockIndices[0x00]); break;
								case TileType.Tree: tileIndex = (TreeIndices[0x00]); break;
								case TileType.Stump: tileIndex = (TreeIndices[0x00]); break;
								case TileType.Seedling: tileIndex = (SeedlingIndex); break;
								case TileType.AdolescentTree: tileIndex = (AdolescentTreeIndex); break;
								case TileType.Water: tileIndex = (WaterIndices[0x00]); break;
								case TileType.Wall: tileIndex = (WallIndices[0x00]); break;
								case TileType.WallDamaged: tileIndex = (WallDamagedIndices[0x00]); break;
								case TileType.Rubble: tileIndex = (RubbleIndices[0x00]); break;
								default: continue;
							}
							if (!mask)
								ScreenManager.SpriteBatch.Draw(Tileset.GetTile(tileIndex), tilePosition);
							else
								ScreenManager.SpriteBatch.Draw(Tileset.GetClippedTile(tileIndex), tilePosition, pixelType.ToPixelColor());
						}
					}
				}
			}
			else
			{
				for (int yindex = area.Y / Tileset.TileHeight, ypos = -(area.Y % Tileset.TileHeight); ypos < area.Height; yindex++, ypos += Tileset.TileHeight)
				{
					for (int xindex = area.X / Tileset.TileWidth, xpos = -(area.X % Tileset.TileWidth); xpos < area.Width; xindex++, xpos += Tileset.TileWidth)
					{
						if (TerrainMap.GetTileType(xindex, yindex + 1) == TileType.Tree && TerrainMap.GetTileType(xindex, yindex) != TileType.Tree)
						{
							var pixelType = new PixelType(TileType.Tree);
							int treeIndex = 0, treeMask = 0x1;

							for (var yoff = 0; yoff < 2; yoff++)
							{
								for (var xoff = -1; xoff < 2; xoff++)
								{
									if (TerrainMap.GetTileType(xindex + xoff, yindex + yoff) == TileType.Tree)
									{
										treeIndex |= treeMask;
									}

									treeMask <<= 1;
								}
							}

							// This can end up being null somehow...
							// Related to fog.
							var tile = Tileset.GetTile(TreeIndices[treeIndex]);
							var clippedTile = Tileset.GetClippedTile(TreeIndices[treeIndex]);
							if (tile != null)
							{
								if (!mask)
									ScreenManager.SpriteBatch.Draw(tile, new Vector2(xpos, ypos));
								else
									ScreenManager.SpriteBatch.Draw(clippedTile, new Vector2(xpos, ypos), pixelType.ToPixelColor());
							}
						}
					}
				}
			}
		}

		private int FindUnknown(TileType type, int known, int unknown)
		{
			Dictionary<int, int> typeUnknown;
			List<int> typeIndices;

			switch (type)
			{
				case TileType.Dirt: typeUnknown = dirtUnknown; typeIndices = DirtIndices; break;
				case TileType.Rock: typeUnknown = rockUnknown; typeIndices = RockIndices; break;
				case TileType.Tree: typeUnknown = treeUnknown; typeIndices = TreeIndices; break;
				case TileType.Water: typeUnknown = waterUnknown; typeIndices = WaterIndices; break;
				default: return -1;
			}

			int val;
			if (typeUnknown.TryGetValue((known << 8) | unknown, out val))
				return val;

			List<int> hammingSet;
			MakeHammingSet(unknown, out hammingSet);
			foreach (var value in hammingSet)
			{
				if (typeIndices[known | value] != -1)
				{
					typeUnknown[(known << 8) | unknown] = typeIndices[known | value];
					return typeIndices[known | value];
				}
			}

			return -1;
		}

		private void MakeHammingSet(int hammingValue, out List<int> hammingSet)
		{
			hammingSet = new List<int>();

			for (var index = 0; index < 8; index++)
			{
				var value = 1 << index;
				if ((hammingValue & value) != 0)
					hammingSet.Add(value);
			}

			var lastEnd = hammingSet.Count;
			var bitCount = hammingSet.Count;
			var anchor = 0;
			for (var totalBits = 1; totalBits < bitCount; totalBits++)
			{
				for (var lastIndex = anchor; lastIndex < lastEnd; lastIndex++)
				{
					for (var bitIndex = 0; bitIndex < bitCount; bitIndex++)
					{
						var newValue = hammingSet[lastIndex] | hammingSet[bitIndex];
						if (newValue != hammingSet[lastIndex])
						{
							var found = false;
							for (var index = lastEnd; index < hammingSet.Count; index++)
							{
								if (newValue == hammingSet[index])
								{
									found = true;
									break;
								}
							}
							if (!found)
								hammingSet.Add(newValue);
						}
					}
				}

				anchor = lastEnd + 1;
				lastEnd = hammingSet.Count;
			}
		}

		/// <summary>
		/// Draws a minimap of the <see cref="TerrainMap"/> the current render target.
		/// </summary>
		public void DrawMiniMap()
		{
			ScreenManager.Graphics.Clear(Color.Black);

			for (var ypos = 0; ypos < TerrainMap.MapHeight; ypos++)
			{
				var xpos = 0;

				while (xpos < TerrainMap.MapWidth)
				{
					var tileType = TerrainMap.GetTileType(xpos, ypos);
					var xanchor = xpos;
					while (xpos < TerrainMap.MapWidth && TerrainMap.GetTileType(xpos, ypos) == tileType)
						xpos++;

					if (tileType != TileType.None)
						ScreenManager.SpriteBatch.DrawLine(PixelIndices[(int)tileType].ToColor(), new Vector2(xanchor, ypos), new Vector2(xpos, ypos));
				}
			}
		}
	}
}