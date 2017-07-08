using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Extensions;
using Warcraft.GameModel;
using Warcraft.Player;
using Warcraft.Screens.Manager;

namespace Warcraft.Renderers
{
	public class AssetRenderer
	{
		public PlayerData PlayerData { get; set; }
		public DecoratedMap PlayerMap { get; set; }
		public List<MulticolorTileset> Tilesets { get; }
		public Tileset MarkerTileset { get; }
		public List<Tileset> FireTilesets { get; }
		public Tileset BuildingDeathTileset { get; }
		public Tileset ArrowTileset { get; }
		private List<uint> PixelColors;

		public List<int> MarkerIndices { get; set; }
		public List<int> ArrowIndices { get; set; }
		public int PlaceGoodIndex { get; }
		public int PlaceBadIndex { get; }

		public List<List<int>> ConstructIndices { get; set; }
		public List<List<int>> BuildIndices { get; set; }
		public List<List<int>> WalkIndices { get; set; }
		public List<List<int>> NoneIndices { get; set; }
		public List<List<int>> CarryGoldIndices { get; set; }
		public List<List<int>> CarryLumberIndices { get; set; }
		public List<List<int>> AttackIndices { get; set; }
		public List<List<int>> DeathIndices { get; set; }
		public List<List<int>> PlaceIndices { get; set; }

		public int AnimationDownsample
		{
			get
			{
				var updateFrequency = Settings.UpdateFrequency / Settings.Debug.SpeedFactor;
				if (updateFrequency <= TargetFrequency)
					return 1;

				return updateFrequency / TargetFrequency;
			}
		}

		public const int TargetFrequency = 10;

		public AssetRenderer(RecolorMap colors, List<MulticolorTileset> tilesets, Tileset markerTileset, List<Tileset> fireTilesets, Tileset buildingDeathTileset, Tileset arrowTileset, PlayerData player, DecoratedMap map)
		{
			Tilesets = tilesets;
			MarkerTileset = markerTileset;
			FireTilesets = fireTilesets;
			BuildingDeathTileset = buildingDeathTileset;
			ArrowTileset = arrowTileset;
			PlayerData = player;
			PlayerMap = map;

			PixelColors = new List<uint>(new uint[(int)PlayerColor.Max + 3]);
			PixelColors[(int)PlayerColor.None] = colors.ColorValue(colors.FindColor("none"), 0);
			PixelColors[(int)PlayerColor.Blue] = colors.ColorValue(colors.FindColor("blue"), 0);
			PixelColors[(int)PlayerColor.Red] = colors.ColorValue(colors.FindColor("red"), 0);
			PixelColors[(int)PlayerColor.Green] = colors.ColorValue(colors.FindColor("green"), 0);
			PixelColors[(int)PlayerColor.Purple] = colors.ColorValue(colors.FindColor("purple"), 0);
			PixelColors[(int)PlayerColor.Orange] = colors.ColorValue(colors.FindColor("orange"), 0);
			PixelColors[(int)PlayerColor.Yellow] = colors.ColorValue(colors.FindColor("yellow"), 0);
			PixelColors[(int)PlayerColor.Black] = colors.ColorValue(colors.FindColor("black"), 0);
			PixelColors[(int)PlayerColor.White] = colors.ColorValue(colors.FindColor("white"), 0);
			PixelColors[(int)PlayerColor.Max] = colors.ColorValue(colors.FindColor("self"), 0);
			PixelColors[(int)PlayerColor.Max + 1] = colors.ColorValue(colors.FindColor("enemy"), 0);
			PixelColors[(int)PlayerColor.Max + 2] = colors.ColorValue(colors.FindColor("building"), 0);

			MarkerIndices = new List<int>();
			for (var markerIndex = 0; ; markerIndex++)
			{
				var index = MarkerTileset.GetIndex("marker-" + markerIndex);
				if (index < 0)
					break;
				MarkerIndices.Add(index);
			}
			PlaceGoodIndex = MarkerTileset.GetIndex("place-good");
			PlaceBadIndex = MarkerTileset.GetIndex("place-bad");

			// Todo: CorpseTileset

			ArrowIndices = new List<int>();
			foreach (var direction in new[] { "attack-n", "attack-ne", "attack-e", "attack-se", "attack-s", "attack-sw", "attack-w", "attack-nw" })
			{
				for (var stepIndex = 0; ; stepIndex++)
				{
					var tileIndex = ArrowTileset.GetIndex(direction + stepIndex);
					if (tileIndex >= 0)
						ArrowIndices.Add(tileIndex);
					else
						break;
				}
			}

			ConstructIndices = new List<List<int>>(new List<int>[Tilesets.Count]);
			BuildIndices = new List<List<int>>(new List<int>[Tilesets.Count]);
			WalkIndices = new List<List<int>>(new List<int>[Tilesets.Count]);
			NoneIndices = new List<List<int>>(new List<int>[Tilesets.Count]);
			CarryGoldIndices = new List<List<int>>(new List<int>[Tilesets.Count]);
			CarryLumberIndices = new List<List<int>>(new List<int>[Tilesets.Count]);
			AttackIndices = new List<List<int>>(new List<int>[Tilesets.Count]);
			DeathIndices = new List<List<int>>(new List<int>[Tilesets.Count]);
			PlaceIndices = new List<List<int>>(new List<int>[Tilesets.Count]);

			Trace.TraceInformation("AssetRenderer: Checking assets");
			var sw = new Stopwatch();
			sw.Start();

			for (var typeIndex = 0; typeIndex < Tilesets.Count; typeIndex++)
			{
				if (Tilesets[typeIndex] == null)
					continue;

				var innerSw = new Stopwatch();
				innerSw.Start();

				var tileset = Tilesets[typeIndex];

				ConstructIndices[typeIndex] = new List<int>();
				BuildIndices[typeIndex] = new List<int>();
				WalkIndices[typeIndex] = new List<int>();
				NoneIndices[typeIndex] = new List<int>();
				CarryGoldIndices[typeIndex] = new List<int>();
				CarryLumberIndices[typeIndex] = new List<int>();
				AttackIndices[typeIndex] = new List<int>();
				DeathIndices[typeIndex] = new List<int>();
				PlaceIndices[typeIndex] = new List<int>();

				foreach (var direction in new[] { "walk-n", "walk-ne", "walk-e", "walk-se", "walk-s", "walk-sw", "walk-w", "walk-nw" })
					CheckAsset(tileset, WalkIndices, direction, typeIndex);

				CheckAsset(tileset, ConstructIndices, "construct-", typeIndex, true);

				foreach (var direction in new[] { "gold-n", "gold-ne", "gold-e", "gold-se", "gold-s", "gold-sw", "gold-w", "gold-nw" })
					CheckAsset(tileset, CarryGoldIndices, direction, typeIndex);

				foreach (var direction in new[] { "lumber-n", "lumber-ne", "lumber-e", "lumber-se", "lumber-s", "lumber-sw", "lumber-w", "lumber-nw" })
					CheckAsset(tileset, CarryLumberIndices, direction, typeIndex);

				foreach (var direction in new[] { "attack-n", "attack-ne", "attack-e", "attack-se", "attack-s", "attack-sw", "attack-w", "attack-nw" })
					CheckAsset(tileset, AttackIndices, direction, typeIndex);
				if (AttackIndices[typeIndex].Count == 0)
				{
					for (var index = 0; index < (int)Direction.Max; index++)
					{
						int tileIndex;
						if ((tileIndex = tileset.GetIndex("active")) >= 0)
							AttackIndices[typeIndex].Add(tileIndex);
						else if ((tileIndex = tileset.GetIndex("inactive")) >= 0)
							AttackIndices[typeIndex].Add(tileIndex);
					}
				}

				var lastDirection = "death-nw";
				foreach (var direction in new[] { "death-n", "death-ne", "death-e", "death-se", "death-s", "death-sw", "death-w", "death-nw" })
				{
					for (var stepIndex = 0; ; stepIndex++)
					{
						var tileIndex = tileset.GetIndex(direction + stepIndex);
						if (tileIndex >= 0)
							DeathIndices[typeIndex].Add(tileIndex);
						else
						{
							tileIndex = tileset.GetIndex(lastDirection + stepIndex);
							if (tileIndex >= 0)
								DeathIndices[typeIndex].Add(tileIndex);
							else
								break;
						}
					}
					lastDirection = direction;
				}
				// Note: There's an empty if statement here in the Linux code.

				foreach (var direction in new[] { "none-n", "none-ne", "none-e", "none-se", "none-s", "none-sw", "none-w", "none-nw" })
				{
					var tileIndex = tileset.GetIndex(direction);
					if (tileIndex >= 0)
						NoneIndices[typeIndex].Add(tileIndex);
					else if (WalkIndices[typeIndex].Count > 0)
						NoneIndices[typeIndex].Add(WalkIndices[typeIndex][NoneIndices[typeIndex].Count * (WalkIndices[typeIndex].Count / (int)Direction.Max)]);
					else if ((tileIndex = tileset.GetIndex("inactive")) >= 0 || (tileIndex = tileset.GetIndex("inactive-0")) >= 0)
						NoneIndices[typeIndex].Add(tileIndex);
				}

				foreach (var direction in new[] { "build-n", "build-ne", "build-e", "build-se", "build-s", "build-sw", "build-w", "build-nw" })
				{
					for (var stepIndex = 0; ; stepIndex++)
					{
						var tileIndex = tileset.GetIndex(direction + stepIndex);
						if (tileIndex >= 0)
							BuildIndices[typeIndex].Add(tileIndex);
						else
						{
							if (stepIndex == 0)
							{
								if ((tileIndex = tileset.GetIndex("active")) >= 0)
									BuildIndices[typeIndex].Add(tileIndex);
								else if ((tileIndex = tileset.GetIndex("inactive")) >= 0)
									BuildIndices[typeIndex].Add(tileIndex);
							}
							break;
						}
					}
				}

				PlaceIndices[typeIndex].Add(tileset.GetIndex("place"));

				innerSw.Stop();
			}

			sw.Stop();
			Trace.TraceInformation($"AssetRenderer: Done checking assets in {sw.Elapsed.TotalSeconds:0.000} seconds.");
		}

		private void CheckAsset(MulticolorTileset tileset, List<List<int>> indices, string prefix, int typeIndex, bool addNegative1 = false)
		{
			for (var stepIndex = 0; ; stepIndex++)
			{
				var tileIndex = tileset.GetIndex(prefix + stepIndex);
				if (tileIndex >= 0)
					indices[typeIndex].Add(tileIndex);
				else
				{
					if (stepIndex == 0 && addNegative1)
						indices[typeIndex].Add(-1);
					break;
				}
			}
		}

		/// <summary>
		/// Sets te <see cref="PlayerMap"/> to <paramref name="map"/>.
		/// </summary>
		public void SetMap(DecoratedMap map)
		{
			PlayerMap = map;
		}

		public void DrawAssets(Rectangle area, bool mask = false)
		{
			var screenRightX = area.X + area.Width - 1;
			var screenBottomY = area.Y + area.Height - 1;
			var finalRenderList = new List<AssetRenderData>();

			foreach (var asset in PlayerMap.Assets)
			{
				var tempRenderData = new AssetRenderData();
				tempRenderData.Type = asset.Data.Type;

				if (tempRenderData.Type <= AssetType.None || (int)tempRenderData.Type >= Tilesets.Count)
					continue;

				var tileset = Tilesets[(int)tempRenderData.Type];

				tempRenderData.Position.X = asset.Position.X + (asset.Data.Size - 1) * Position.HalfTileWidth - tileset.TileWidth / 2;
				tempRenderData.Position.Y = asset.Position.Y + (asset.Data.Size - 1) * Position.HalfTileHeight - tileset.TileHeight / 2;
				tempRenderData.PixelColor = new PixelType(asset).ToPixelColor();

				var rightX = tempRenderData.Position.X + tileset.TileWidth - 1;
				tempRenderData.BottomY = tempRenderData.Position.Y + tileset.TileHeight - 1;

				var onScreen = !(rightX < area.X || tempRenderData.Position.X > screenRightX || tempRenderData.BottomY < area.Y || tempRenderData.Position.Y > screenBottomY);

				tempRenderData.Position -= area.Location;
				tempRenderData.ColorIndex = Math.Max((int)asset.Data.Color - 1, 0);
				tempRenderData.TileIndex = -1;

				if (onScreen)
				{
					int actionSteps, currentStep, tileIndex;
					switch (asset.GetAction())
					{
						case AssetAction.Build:
							actionSteps = BuildIndices[(int)tempRenderData.Type].Count;
							actionSteps /= (int)Direction.Max;
							if (actionSteps != 0)
							{
								tileIndex = (int)asset.Direction * actionSteps + asset.Step / AnimationDownsample % actionSteps;
								tempRenderData.TileIndex = BuildIndices[(int)tempRenderData.Type][tileIndex];
							}
							break;

						case AssetAction.Construct:
							actionSteps = ConstructIndices[(int)tempRenderData.Type].Count;
							if (actionSteps != 0)
							{
								var totalSteps = asset.Data.BuildTime * PlayerAsset.UpdateFrequency;
								currentStep = asset.Step * actionSteps / totalSteps;
								if (currentStep == ConstructIndices[(int)tempRenderData.Type].Count)
									currentStep--;
								tempRenderData.TileIndex = ConstructIndices[(int)tempRenderData.Type][currentStep];
							}
							break;

						case AssetAction.Walk:
							if (asset.Lumber != 0)
							{
								actionSteps = CarryLumberIndices[(int)tempRenderData.Type].Count;
								actionSteps /= (int)Direction.Max;
								tileIndex = (int)asset.Direction * actionSteps + asset.Step / AnimationDownsample % actionSteps;
								tempRenderData.TileIndex = CarryLumberIndices[(int)tempRenderData.Type][tileIndex];
							}
							else if (asset.Gold != 0 || asset.Stone != 0)
							{
								actionSteps = CarryGoldIndices[(int)tempRenderData.Type].Count;
								actionSteps /= (int)Direction.Max;
								tileIndex = (int)asset.Direction * actionSteps + asset.Step / AnimationDownsample % actionSteps;
								tempRenderData.TileIndex = CarryGoldIndices[(int)tempRenderData.Type][tileIndex];
							}
							else
							{
								actionSteps = WalkIndices[(int)tempRenderData.Type].Count;
								actionSteps /= (int)Direction.Max;
								tileIndex = (int)asset.Direction * actionSteps + asset.Step / AnimationDownsample % actionSteps;
								tempRenderData.TileIndex = WalkIndices[(int)tempRenderData.Type][tileIndex];
							}
							break;

						case AssetAction.Attack:
							currentStep = asset.Step % (asset.Data.AttackSteps + asset.Data.ReloadSteps);
							if (currentStep < asset.Data.AttackSteps)
							{
								actionSteps = AttackIndices[(int)tempRenderData.Type].Count;
								actionSteps /= (int)Direction.Max;
								tileIndex = (int)asset.Direction * actionSteps + currentStep * actionSteps / asset.Data.AttackSteps;
								tempRenderData.TileIndex = AttackIndices[(int)tempRenderData.Type][tileIndex];
							}
							else
							{
								tempRenderData.TileIndex = NoneIndices[(int)tempRenderData.Type][(int)asset.Direction];
							}
							break;

						case AssetAction.Repair:
						case AssetAction.HarvestLumber:
						case AssetAction.QuarryStone:
							actionSteps = AttackIndices[(int)tempRenderData.Type].Count;
							actionSteps /= (int)Direction.Max;
							tileIndex = (int)asset.Direction * actionSteps + asset.Step / AnimationDownsample % actionSteps;
							tempRenderData.TileIndex = AttackIndices[(int)tempRenderData.Type][tileIndex];
							break;

						case AssetAction.MineGold:
							break;

						case AssetAction.StandGround:
						case AssetAction.None:
							if (tempRenderData.Type != AssetType.Wall)
							{
								tempRenderData.TileIndex = NoneIndices[(int)tempRenderData.Type][(int)asset.Direction];
								if (asset.Speed != 0)
								{
									if (asset.Lumber != 0)
									{
										actionSteps = CarryLumberIndices[(int)tempRenderData.Type].Count;
										actionSteps /= (int)Direction.Max;
										tempRenderData.TileIndex = CarryLumberIndices[(int)tempRenderData.Type][(int)asset.Direction * actionSteps];
									}
									else if (asset.Gold != 0 || asset.Stone != 0)
									{
										actionSteps = CarryGoldIndices[(int)tempRenderData.Type].Count;
										actionSteps /= (int)Direction.Max;
										tempRenderData.TileIndex = CarryGoldIndices[(int)tempRenderData.Type][(int)asset.Direction * actionSteps];
									}
								}
							}
							else
							{
								// Connect adjacent walls
								var wallIndex = 0;
								int[] xoff = { 0, 1, 0, -1 };
								int[] yoff = { -1, 0, 1, 0 };
								foreach (var innerAsset in PlayerMap.Assets)
								{
									var wallMask = 0x1;
									for (var index = 0; index < xoff.Length; index++)
									{
										// If the adjacent tile is another player-created wall or rubble
										if (innerAsset.Data.Type == AssetType.Wall && asset.TilePosition.X + xoff[index] == innerAsset.TilePosition.X && asset.TilePosition.Y + yoff[index] == innerAsset.TilePosition.Y
											|| PlayerMap.GetTileType(asset.TilePosition.X + xoff[index], asset.TilePosition.Y + yoff[index]) == TileType.Rubble)
											wallIndex |= wallMask;

										wallMask <<= 1;
									}
								}

								// Display damaged tile if health less than half
								if (asset.Health <= asset.Data.Health / 2)
									tempRenderData.TileIndex = Tilesets[(int)tempRenderData.Type].GetIndex("damaged-" + wallIndex);
								else
									tempRenderData.TileIndex = Tilesets[(int)tempRenderData.Type].GetIndex("inactive-" + wallIndex);
							}
							break;

						case AssetAction.Capability:
							// Units
							if (asset.Speed > 0)
							{
								if (asset.CurrentCommand().Capability == AssetCapabilityType.Patrol || asset.CurrentCommand().Capability == AssetCapabilityType.StandGround)
									tempRenderData.TileIndex = NoneIndices[(int)tempRenderData.Type][(int)asset.Direction];
							}
							// Buildings
							else
							{
								tempRenderData.TileIndex = NoneIndices[(int)tempRenderData.Type][(int)asset.Direction];
							}
							break;

						case AssetAction.Death:
							actionSteps = DeathIndices[(int)tempRenderData.Type].Count;
							if (asset.Speed != 0)
							{
								actionSteps /= (int)Direction.Max;
								if (actionSteps != 0)
								{
									currentStep = asset.Step / AnimationDownsample;
									if (currentStep >= actionSteps)
										currentStep = actionSteps - 1;

									tempRenderData.TileIndex = DeathIndices[(int)tempRenderData.Type][(int)asset.Direction * actionSteps + currentStep];
								}
							}
							else
							{
								if (asset.Step < BuildingDeathTileset.Count)
								{
									tempRenderData.TileIndex = Tilesets[(int)tempRenderData.Type].Count + asset.Step;
									tempRenderData.Position.X += Tilesets[(int)tempRenderData.Type].TileWidth / 2 - BuildingDeathTileset.TileWidth / 2;
									tempRenderData.Position.Y += Tilesets[(int)tempRenderData.Type].TileHeight / 2 - BuildingDeathTileset.TileHeight / 2;
								}
							}
							break;
					}

					if (tempRenderData.TileIndex >= 0)
						finalRenderList.Add(tempRenderData);
				}
			}

			finalRenderList.Sort(delegate (AssetRenderData first, AssetRenderData second)
			{
				if (first.BottomY < second.BottomY)
					return 1;
				if (first.BottomY > second.BottomY)
					return -1;

				return first.Position.X <= second.Position.X ? 1 : -1;
			});
			foreach (var asset in finalRenderList)
			{
				if (asset.TileIndex < Tilesets[(int)asset.Type].Count)
				{
					if (!mask)
					{
						ScreenManager.SpriteBatch.Draw(Tilesets[(int)asset.Type].GetTile(asset.TileIndex, asset.ColorIndex), asset.Position.ToVector2());
					}
					else
					{
						ScreenManager.SpriteBatch.Draw(Tilesets[(int)asset.Type].GetClippedTile(asset.TileIndex), asset.Position.ToVector2(), asset.PixelColor);
					}
				}
				else
				{
					var tile = BuildingDeathTileset.GetTile(asset.TileIndex);
					if (tile != null)
						ScreenManager.SpriteBatch.Draw(tile, asset.Position.ToVector2());
				}
			}
		}

		public void DrawSelections(Rectangle rectangle, List<PlayerAsset> selectionList, Rectangle selectionRectangle, bool highlightBuildings)
		{
			var rectangleColor = PixelColors[(int)PlayerColor.Max];
			var screenBottomRight = new Point(rectangle.X + rectangle.Width - 1, rectangle.Y + rectangle.Height - 1);

			if (highlightBuildings)
			{
				rectangleColor = PixelColors[(int)PlayerColor.Max + 2];

				foreach (var asset in PlayerMap.Assets)
				{
					AssetRenderData tempRenderData;
					tempRenderData.Type = asset.Data.Type;
					if (tempRenderData.Type == AssetType.None) continue;

					if (tempRenderData.Type >= 0 && (int)tempRenderData.Type <= Tilesets.Count)
					{
						if (asset.Speed == 0)
						{
							var offset = tempRenderData.Type == AssetType.GoldMine ? 1 : 0;

							tempRenderData.Position.X = asset.Position.X + (asset.Data.Size - 1) * Position.HalfTileWidth - Tilesets[(int)tempRenderData.Type].TileWidth / 2;
							tempRenderData.Position.Y = asset.Position.Y + (asset.Data.Size - 1) * Position.HalfTileHeight - Tilesets[(int)tempRenderData.Type].TileHeight / 2;
							tempRenderData.Position.X -= offset * Position.TileWidth;
							tempRenderData.Position.Y -= offset * Position.TileHeight;

							var rightX = tempRenderData.Position.X + Tilesets[(int)tempRenderData.Type].TileWidth + 2 * offset * Position.TileWidth - 1;
							tempRenderData.BottomY = tempRenderData.Position.Y + Tilesets[(int)tempRenderData.Type].TileHeight + 2 * offset * Position.TileWidth - 1;

							var onScreen = true;
							if (rightX < rectangle.X || tempRenderData.Position.X > screenBottomRight.X) onScreen = false;
							else if (tempRenderData.BottomY < rectangle.Y || tempRenderData.BottomY > screenBottomRight.Y) onScreen = false;
							tempRenderData.Position.X -= rectangle.X;
							tempRenderData.Position.Y -= rectangle.Y;
							if (onScreen)
							{
								ScreenManager.SpriteBatch.DrawHallowRectangle(new Rectangle(tempRenderData.Position.X, tempRenderData.Position.Y, Tilesets[(int)tempRenderData.Type].TileWidth + 2 * offset * Position.TileWidth, Tilesets[(int)tempRenderData.Type].TileHeight + 2 * offset * Position.TileHeight), rectangleColor.ToColor());
							}
						}
					}
				}
			}

			if (selectionRectangle.Width != 0 && selectionRectangle.Height != 0)
			{
				var selection = new Point(selectionRectangle.X - rectangle.X, selectionRectangle.Y - rectangle.Y);
				ScreenManager.SpriteBatch.DrawHallowRectangle(selection.X, selection.Y, selectionRectangle.Width, selectionRectangle.Height, rectangleColor.ToColor());
			}

			if (selectionList.Count > 0)
			{
				var asset = selectionList.First();
				if (asset.Data.Color == PlayerColor.None)
				{
					rectangleColor = PixelColors[(int)PlayerColor.None];
				}
				else if (PlayerData.Color != asset.Data.Color)
				{
					rectangleColor = PixelColors[(int)PlayerColor.Max + 1];
				}
			}

			foreach (var asset in selectionList)
			{
				AssetRenderData tempRenderData;
				tempRenderData.Type = asset.Data.Type;
				if (tempRenderData.Type == AssetType.None)
				{
					if (asset.GetAction() == AssetAction.Decay)
					{
						// Todo: Check decay action
					}
					else if (asset.GetAction() != AssetAction.Attack)
					{
						var bottomRight = new Point();
						var onScreen = true;

						tempRenderData.Position.X = asset.Position.X - MarkerTileset.TileWidth / 2;
						tempRenderData.Position.Y = asset.Position.Y - MarkerTileset.TileWidth / 2;
						bottomRight.X = tempRenderData.Position.X + MarkerTileset.TileWidth;
						bottomRight.Y = tempRenderData.Position.Y + MarkerTileset.TileHeight;

						if (rectangle.X > bottomRight.X || screenBottomRight.X < tempRenderData.Position.X)
							onScreen = false;
						else if (rectangle.Y > tempRenderData.Position.Y || screenBottomRight.Y < tempRenderData.Position.Y)
							onScreen = false;

						tempRenderData.Position -= rectangle.Location;

						if (onScreen)
						{
							var markerIndex = asset.Step / AnimationDownsample;
							if (markerIndex < MarkerIndices.Count)
								ScreenManager.SpriteBatch.Draw(MarkerTileset.GetTile(MarkerIndices[markerIndex]), tempRenderData.Position.ToVector2());
						}
					}
				}
				else if (tempRenderData.Type >= 0 && (int)tempRenderData.Type < Tilesets.Count)
				{
					var onScreen = true;

					tempRenderData.Position.X = asset.Position.X - Position.HalfTileWidth;
					tempRenderData.Position.Y = asset.Position.Y - Position.HalfTileHeight;

					var rectangleWidth = Position.TileWidth * asset.Data.Size;
					var rectangleHeight = Position.TileHeight * asset.Data.Size;
					var bottomRight = new Point(tempRenderData.Position.X + rectangleWidth, tempRenderData.Position.Y + rectangleHeight);

					if (bottomRight.X < rectangle.X || tempRenderData.Position.X > screenBottomRight.X)
						onScreen = false;
					else if (bottomRight.Y < rectangle.Y || tempRenderData.Position.Y > screenBottomRight.Y)
						onScreen = false;
					else if (asset.PerformingHiddenAction)
						onScreen = false;

					tempRenderData.Position -= rectangle.Location;

					if (onScreen)
						ScreenManager.SpriteBatch.DrawHallowRectangle(tempRenderData.Position.X, tempRenderData.Position.Y, rectangleWidth, rectangleHeight, rectangleColor.ToColor());
				}
			}
		}

		/// <summary>
		/// Draws arrows and fire effects onto the current render target.
		/// </summary>
		public void DrawOverlays(Rectangle area)
		{
			var screenBottomRight = new Point(area.X + area.Width - 1, area.Y + area.Height - 1);

			foreach (var asset in PlayerMap.Assets)
			{
				var tempRenderData = new AssetRenderData
				{
					Type = asset.Data.Type
				};

				if (tempRenderData.Type == AssetType.None)
				{
					if (asset.GetAction() == AssetAction.Attack)
					{
						var onScreen = true;

						tempRenderData.Position.X = asset.Position.X - ArrowTileset.TileWidth / 2;
						tempRenderData.Position.Y = asset.Position.Y - ArrowTileset.TileHeight / 2;
						var rightX = tempRenderData.Position.X + ArrowTileset.TileWidth;
						tempRenderData.BottomY = tempRenderData.Position.Y + ArrowTileset.TileHeight;

						if (area.X > rightX || screenBottomRight.X < tempRenderData.Position.X)
							onScreen = false;
						else if (area.Y > tempRenderData.BottomY || screenBottomRight.Y < tempRenderData.Position.Y)
							onScreen = false;

						tempRenderData.Position -= area.Location;

						if (onScreen)
						{
							var actionSteps = ArrowIndices.Count / (int)Direction.Max;
							var arrowIndex = (int)asset.Direction * actionSteps + (PlayerData.GameCycle - asset.CreationCycle) % actionSteps;
							ScreenManager.SpriteBatch.Draw(ArrowTileset.GetTile(ArrowIndices[arrowIndex]), tempRenderData.Position.ToVector2());
						}
					}
				}
				else if (asset.Data.Speed == 0)
				{
					// Don't render fire for damaged walls
					if (asset.Data.Type == AssetType.Wall) continue;

					var currentAction = asset.GetAction();
					if (currentAction != AssetAction.Death)
					{
						var hitRange = asset.Health * FireTilesets.Count * 2 / asset.Data.Health;

						if (currentAction == AssetAction.Construct)
						{
							var command = asset.CurrentCommand();

							if (command.Target != null)
								command = command.Target.CurrentCommand();

							if (command.ActivatedCapability != null)
							{
								var divisor = command.ActivatedCapability.PercentComplete(asset.Data.Health);
								if (divisor == 0) divisor = 1;

								hitRange = asset.Health * FireTilesets.Count * 2 / divisor;
							}
						}

						if (hitRange < FireTilesets.Count)
						{
							var tilesetIndex = FireTilesets.Count - hitRange - 1;

							tempRenderData.TileIndex = (PlayerData.GameCycle - asset.CreationCycle) % FireTilesets[tilesetIndex].Count;
							tempRenderData.Position.X = asset.Position.X + (asset.Data.Size - 1) * Position.HalfTileWidth - FireTilesets[tilesetIndex].TileWidth / 2;
							tempRenderData.Position.Y = asset.Position.Y + (asset.Data.Size - 1) * Position.HalfTileHeight - FireTilesets[tilesetIndex].TileHeight;

							var rightX = tempRenderData.Position.X + FireTilesets[tilesetIndex].TileWidth - 1;
							tempRenderData.BottomY = tempRenderData.Position.Y + FireTilesets[tilesetIndex].TileHeight - 1;

							var onScreen = true;
							if (area.X > rightX || screenBottomRight.X < tempRenderData.Position.X)
								onScreen = false;
							else if (area.Y > tempRenderData.BottomY || screenBottomRight.Y < tempRenderData.Position.Y)
								onScreen = false;

							tempRenderData.Position -= area.Location;

							if (onScreen)
								ScreenManager.SpriteBatch.Draw(FireTilesets[tilesetIndex].GetTile(tempRenderData.TileIndex), tempRenderData.Position.ToVector2());
						}
					}
				}
			}
		}

		/// <summary>
		/// Draws a preview of the placement of assets.
		/// </summary>
		public void DrawPlacement(Rectangle area, Position pos, AssetType type, PlayerAsset builder)
		{
			var screenBottomRight = new Point(area.X + area.Width - 1, area.Y + area.Height - 1);

			if (type != AssetType.None)
			{
				var tempPosition = new Position();
				var tempTilePosition = new Position();
				var assetType = PlayerAssetData.FindDefaultFromType(type);

				tempTilePosition.SetToTile(pos);
				tempPosition.SetFromTile(tempTilePosition);

				tempPosition.X += (assetType.Size - 1) * Position.HalfTileWidth - Tilesets[(int)type].TileWidth / 2;
				tempPosition.Y += (assetType.Size - 1) * Position.HalfTileHeight - Tilesets[(int)type].TileHeight / 2;
				Point placementBottomRight;
				placementBottomRight.X = tempPosition.X + Tilesets[(int)type].TileWidth;
				placementBottomRight.Y = tempPosition.Y + Tilesets[(int)type].TileHeight;

				tempTilePosition.SetToTile(tempPosition);
				int xOff = 0, yOff = 0;
				var placementTiles = new int[assetType.Size, assetType.Size];
				for (var y = 0; y < placementTiles.GetLength(0); y++)
				{
					for (var x = 0; x < placementTiles.GetLength(1); x++)
					{
						var tileType = PlayerMap.GetTileType(tempTilePosition.X + xOff, tempTilePosition.Y + yOff);
						if (tileType == TileType.Grass || tileType == TileType.Dirt || tileType == TileType.Stump || tileType == TileType.Rubble)
						{
							placementTiles[y, x] = 1;
						}

						xOff++;
					}

					xOff = 0;
					yOff++;
				}

				xOff = tempTilePosition.X + assetType.Size;
				yOff = tempTilePosition.Y + assetType.Size;
				foreach (var asset in PlayerMap.Assets)
				{
					Point min, max;
					var offset = asset.Data.Type == AssetType.GoldMine ? 1 : 0;

					if (asset == builder) continue;
					if (xOff <= asset.TilePosition.X - offset) continue;
					if (tempTilePosition.X >= asset.TilePosition.X + asset.Data.Size + offset) continue;
					if (yOff <= asset.TilePosition.Y - offset) continue;
					if (tempTilePosition.Y >= asset.TilePosition.Y + asset.Data.Size + offset) continue;

					min.X = MathHelper.Max(tempTilePosition.X, asset.TilePosition.X - offset);
					max.X = MathHelper.Min(xOff, asset.TilePosition.X + asset.Data.Size + offset);
					min.Y = MathHelper.Max(tempTilePosition.Y, asset.TilePosition.Y + offset);
					max.Y = MathHelper.Min(yOff, asset.TilePosition.Y + asset.Data.Size + offset);

					for (var y = min.Y; y < max.Y; y++)
					{
						for (var x = min.X; x < max.X; x++)
						{
							placementTiles[y - tempTilePosition.Y, x - tempTilePosition.X] = 0;
						}
					}
				}

				var onScreen = true;
				if (placementBottomRight.X <= area.X) onScreen = false;
				else if (placementBottomRight.Y <= area.Y) onScreen = false;
				else if (screenBottomRight.X <= tempPosition.X) onScreen = false;
				else if (screenBottomRight.Y <= tempPosition.Y) onScreen = false;
				if (!onScreen) return;

				// Draw asset and markers
				Point p;
				tempPosition.X -= area.X;
				tempPosition.Y -= area.Y;
				ScreenManager.SpriteBatch.Draw(Tilesets[(int)type].GetTile(PlaceIndices[(int)type][0], (int)PlayerData.Color - 1), tempPosition.ToPoint().ToVector2());
				p.X = tempPosition.X;
				p.Y = tempPosition.Y;
				for (var y = 0; y < placementTiles.GetLength(0); y++)
				{
					for (var x = 0; x < placementTiles.GetLength(1); x++)
					{
						ScreenManager.SpriteBatch.Draw(MarkerTileset.GetTile(placementTiles[y, x] == 1 ? PlaceGoodIndex : PlaceBadIndex), p.ToVector2());
						p.X += MarkerTileset.TileWidth;
					}
					p.X = tempPosition.X;
					p.Y += MarkerTileset.TileHeight;
				}
			}
		}

		private struct AssetRenderData
		{
			public AssetType Type;
			public Point Position;
			public int BottomY;
			public int TileIndex;
			public int ColorIndex;
			public Color PixelColor;
		}

		/// <summary>
		/// Draws the assets of the <see cref="PlayerMap"/> onto the current render target.
		/// </summary>
		public void DrawMiniAssets()
		{
			if (PlayerData != null)
			{
				foreach (var asset in PlayerMap.Assets)
				{
					var assetColor = asset.Data.Color;
					if (assetColor == PlayerData.Color)
						assetColor = PlayerColor.Max;

					ScreenManager.SpriteBatch.DrawFilledRectangle(asset.TilePosition.X, asset.TilePosition.Y, asset.Data.Size, asset.Data.Size, PixelColors[(int)assetColor].ToColor());
				}
			}
			else
			{
				foreach (var asset in PlayerMap.InitialAssets)
				{
					var size = PlayerAssetData.FindDefaultFromName(asset.Type).Size;
					ScreenManager.SpriteBatch.DrawFilledRectangle(asset.TilePosition.X, asset.TilePosition.Y, size, size, PixelColors[(int)asset.Color].ToColor());
				}
			}
		}
	}
}