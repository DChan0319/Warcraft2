using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Warcraft.Ai;
using Warcraft.Ai.Triggers;
using Warcraft.App;
using Warcraft.Audio;
using Warcraft.Player;
using Warcraft.Player.Capabilities;
using Warcraft.Renderers;
using Warcraft.Screens.Base;
using Warcraft.Screens.Components;
using Warcraft.Screens.Manager;

namespace Warcraft.Screens
{
	/// <summary>
	/// A test screen with a simple test message in the center.
	/// </summary>
	public class TestScreen : ButtonScreen
	{
		private List<PlayerCommandRequest> playerCommands = new List<PlayerCommandRequest>
		{
			new PlayerCommandRequest(),
			new PlayerCommandRequest(),
			new PlayerCommandRequest(),
			new PlayerCommandRequest(),
			new PlayerCommandRequest(),
			new PlayerCommandRequest(),
			new PlayerCommandRequest(),
			new PlayerCommandRequest(),
			new PlayerCommandRequest()
		};

		private Button menuButton;

		public override void LoadContent()
		{
			Data.CreateGame();

			menuButton = new Button();
			menuButton.Rectangle = new Rectangle(Data.MenuButtonOffset, Data.MenuButtonRenderer.TextArea.Size);
			menuButton.OnClick += MenuButton_OnClick;
			Buttons.Add(menuButton);

			// Play ready effect
			AudioManager.PlayWave("peasant-ready", Settings.General.SfxVolume);

			TriggerManager.Reset();
		}

		private void MenuButton_OnClick()
		{
			ScreenManager.AddScreen(new InGameMenuScreen(this));
		}

		public override void HandleInput(InputState input)
		{
			// ButtonScreen Input Handler
			base.HandleInput(input);

			// Handle horizontal panning
			if (input.CurrentKeyboardState.IsKeyDown(Keys.Left))
				Data.ViewportRenderer.PanWest(10);
			if (input.CurrentKeyboardState.IsKeyDown(Keys.Right))
				Data.ViewportRenderer.PanEast(10);

			// Handle vertical panning
			if (input.CurrentKeyboardState.IsKeyDown(Keys.Up))
				Data.ViewportRenderer.PanNorth(10);
			if (input.CurrentKeyboardState.IsKeyDown(Keys.Down))
				Data.ViewportRenderer.PanSouth(10);

			// Handle keyboard input
			if (input.WasKeyPressed(Keys.F1))
			{
				MenuButton_OnClick();
				return;
			}

			foreach (var key in input.GetPressedKeys())
			{
				// Handle Unit Groups
				if (key >= Keys.D0 && key <= Keys.D9 || key >= Keys.NumPad0 && key <= Keys.NumPad9)
				{
					var @base = Keys.D0;
					if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
						@base = Keys.NumPad0;

					if (input.CurrentKeyboardState.IsKeyDown(Keys.LeftControl) || input.CurrentKeyboardState.IsKeyDown(Keys.RightControl))
					{
						// Register Unit Group (max 9 assets)
						Data.GameModel.UnitGroups[key - @base] = Data.GameModel.SelectedAssets.Where(a => a.Speed != 0 && a.IsInterruptible).Take(9).ToList();
					}
					else
					{
						// Load Unit Group
						Data.GameModel.SelectedAssets = Data.GameModel.UnitGroups[key - @base].Where(a => a.Speed != 0 && a.IsInterruptible).ToList();
					}
				}

				if (Data.GameModel.SelectedAssets.Count > 0)
				{
					var canMove = Data.GameModel.SelectedAssets.All(asset => asset.Speed != 0 && (Settings.Debug.ControlEnemies || asset.Data.Color == Data.PlayerColor));

					if (key == Keys.Escape)
					{
						Data.CurrentAssetCapabilityType = AssetCapabilityType.None;
					}

					if (Data.CurrentAssetCapabilityType == AssetCapabilityType.BuildSimple)
					{
						AssetCapabilityType assetCapability;
						if (!Hotkeys.BuildHotkeys.TryGetValue(key, out assetCapability))
							continue;

						var playerCapability = PlayerCapability.FindCapability(assetCapability);
						if (playerCapability == null)
							continue;

						var actorTarget = Data.GameModel.SelectedAssets.First();
						if (playerCapability.CanInitiate(actorTarget, Data.GameModel.Player(Data.PlayerColor)))
						{
							Data.GameModel.Player(Data.PlayerColor).AddGameEvent(EventType.ButtonTick);

							Data.CurrentAssetCapabilityType = assetCapability;
						}
					}
					else if (canMove)
					{
						AssetCapabilityType assetCapability;
						if (!Hotkeys.UnitHotkeys.TryGetValue(key, out assetCapability))
							continue;

						var hasCapability = Data.GameModel.SelectedAssets.All(asset => asset.HasCapability(assetCapability));
						if (!hasCapability)
							continue;

						Data.GameModel.Player(Data.PlayerColor).AddGameEvent(EventType.ButtonTick);

						var playerCapability = PlayerCapability.FindCapability(assetCapability);
						if (playerCapability != null)
						{
							if (playerCapability.TargetType == TargetType.None || playerCapability.TargetType == TargetType.Player)
							{
								var actorTarget = Data.GameModel.SelectedAssets.First();
								if (!playerCapability.CanApply(actorTarget, Data.GameModel.Player(Data.PlayerColor), actorTarget))
									continue;

								playerCommands[(int)Data.PlayerColor].Action = assetCapability;
								playerCommands[(int)Data.PlayerColor].Actors = Data.GameModel.SelectedAssets;
								playerCommands[(int)Data.PlayerColor].TargetColor = PlayerColor.None;
								playerCommands[(int)Data.PlayerColor].TargetType = AssetType.None;
								playerCommands[(int)Data.PlayerColor].TargetLocation = actorTarget.TilePosition;
								Data.CurrentAssetCapabilityType = AssetCapabilityType.None;
							}
							else
							{
								Data.CurrentAssetCapabilityType = assetCapability;
							}
						}
						else
						{
							Data.CurrentAssetCapabilityType = assetCapability;
						}
					}
					else
					{
						AssetCapabilityType assetCapability;
						if (!Hotkeys.BuildingHotkeys.TryGetValue(key, out assetCapability))
							continue;

						var hasCapability = Data.GameModel.SelectedAssets.All(asset => asset.HasCapability(assetCapability));
						if (!hasCapability)
							continue;

						Data.GameModel.Player(Data.PlayerColor).AddGameEvent(EventType.ButtonTick);

						var playerCapability = PlayerCapability.FindCapability(assetCapability);
						if (playerCapability != null)
						{
							if (playerCapability.TargetType == TargetType.None || playerCapability.TargetType == TargetType.Player)
							{
								var actorTarget = Data.GameModel.SelectedAssets.First();
								if (!playerCapability.CanApply(actorTarget, Data.GameModel.Player(Data.PlayerColor), actorTarget))
									continue;

								playerCommands[(int)Data.PlayerColor].Action = assetCapability;
								playerCommands[(int)Data.PlayerColor].Actors = Data.GameModel.SelectedAssets;
								playerCommands[(int)Data.PlayerColor].TargetColor = PlayerColor.None;
								playerCommands[(int)Data.PlayerColor].TargetType = AssetType.None;
								playerCommands[(int)Data.PlayerColor].TargetLocation = actorTarget.TilePosition;
								Data.CurrentAssetCapabilityType = AssetCapabilityType.None;
							}
							else
							{
								Data.CurrentAssetCapabilityType = assetCapability;
							}
						}
						else
						{
							Data.CurrentAssetCapabilityType = assetCapability;
						}
					}
				}
			}

			switch (GetUIComponent(input.CurrentMouseState.Position))
			{
				case UIComponent.Viewport:
					{
						var viewportPosition = new Position(input.CurrentMouseState.Position - Data.ViewportOffset);
						var mouseDetailedPosition = new Position(viewportPosition.ToPoint() + Data.ViewportRenderer.Bounds.Location);
						if (Data.ViewportRenderer.Bounds.Contains(mouseDetailedPosition.ToPoint()) && Data.TypeRenderTarget.Bounds.Contains(viewportPosition.ToPoint()))
						{
							var pixelType = PixelType.GetPixelType(viewportPosition);

							if (input.RightClick && !input.RightDown && Data.GameModel.SelectedAssets.Count > 0)
							{
								var canMove = Data.GameModel.SelectedAssets.All(asset => asset.Speed != 0 && (Settings.Debug.ControlEnemies || asset.Data.Color == Data.PlayerColor));
								if (canMove)
								{
									if (pixelType.Color != PlayerColor.None || pixelType.AssetType == AssetType.Wall)
									{
										// Walk, Deliver, Repair, Attack
										playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Move;
										playerCommands[(int)Data.PlayerColor].TargetColor = pixelType.Color;
										playerCommands[(int)Data.PlayerColor].TargetType = pixelType.AssetType;
										playerCommands[(int)Data.PlayerColor].Actors = Data.GameModel.SelectedAssets;
										playerCommands[(int)Data.PlayerColor].TargetLocation = mouseDetailedPosition;

										// Treat player-built walls differently
										if (pixelType.AssetType == AssetType.Wall)
										{
											// If any selected units aren't a peasant, attack the wall.
											if (Data.GameModel.SelectedAssets.Any(a => a.Data.Type != AssetType.Peasant))
											{
												playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Attack;
											}
											// Otherwise, all selected units are peasants; repair the wall.
											else
											{
												var targetAsset = Data.GameModel.Player(PlayerColor.None).SelectAsset(mouseDetailedPosition, pixelType.AssetType);
												if (targetAsset.Speed == 0 && targetAsset.Health < targetAsset.Data.Health)
													playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Repair;
											}
										}
										else
										{
											if (pixelType.Color == Data.PlayerColor && Data.GameModel.SelectedAssets.All(a => a.Data.Color == Data.PlayerColor))
											{
												var haveGold = Data.GameModel.SelectedAssets.Any(asset => asset.Gold != 0);
												var haveLumber = Data.GameModel.SelectedAssets.Any(asset => asset.Lumber != 0);
												var haveStone = Data.GameModel.SelectedAssets.Any(asset => asset.Stone != 0);

												if (haveGold)
												{
													if (playerCommands[(int)Data.PlayerColor].TargetType == AssetType.TownHall || playerCommands[(int)Data.PlayerColor].TargetType == AssetType.Keep || playerCommands[(int)Data.PlayerColor].TargetType == AssetType.Castle)
														playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Convey;
												}
												else if (haveLumber)
												{
													if (playerCommands[(int)Data.PlayerColor].TargetType == AssetType.TownHall || playerCommands[(int)Data.PlayerColor].TargetType == AssetType.Keep || playerCommands[(int)Data.PlayerColor].TargetType == AssetType.Castle || playerCommands[(int)Data.PlayerColor].TargetType == AssetType.LumberMill)
														playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Convey;
												}
												else if (haveStone)
												{
													if (playerCommands[(int)Data.PlayerColor].TargetType == AssetType.TownHall || playerCommands[(int)Data.PlayerColor].TargetType == AssetType.Keep || playerCommands[(int)Data.PlayerColor].TargetType == AssetType.Castle || playerCommands[(int)Data.PlayerColor].TargetType == AssetType.Blacksmith)
														playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Convey;
												}
												else
												{
													var targetAsset = Data.GameModel.Player(Data.PlayerColor).SelectAsset(mouseDetailedPosition, pixelType.AssetType);
													if (targetAsset.Speed == 0 && targetAsset.Health < targetAsset.Data.Health)
														playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Repair;
													else
														playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Shelter;
												}
											}
											else
											{
												playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Attack;
											}
										}
									}
									else
									{
										// Walk, Mine, Harvest
										playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Move;
										playerCommands[(int)Data.PlayerColor].TargetColor = PlayerColor.None;
										playerCommands[(int)Data.PlayerColor].TargetType = AssetType.None;
										playerCommands[(int)Data.PlayerColor].Actors = Data.GameModel.SelectedAssets;
										playerCommands[(int)Data.PlayerColor].TargetLocation = mouseDetailedPosition;

										var canHarvest = Data.GameModel.SelectedAssets.All(asset => asset.HasCapability(AssetCapabilityType.Mine));
										if (canHarvest)
										{
											if (pixelType.Type != TerrainType.Tree && pixelType.Type != TerrainType.Rock && pixelType.Type != TerrainType.Rubble)
											{
												if (pixelType.Type == TerrainType.GoldMine)
												{
													playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Mine;
													playerCommands[(int)Data.PlayerColor].TargetType = AssetType.GoldMine;
												}
											}
											else
											{
												playerCommands[(int)Data.PlayerColor].Action = AssetCapabilityType.Mine;

												var tempTilePosition = new Position();
												tempTilePosition.SetToTile(playerCommands[(int)Data.PlayerColor].TargetLocation);
												if (Data.GameModel.Player(Data.PlayerColor).PlayerMap.GetTileType(tempTilePosition) != TileType.Tree)
												{
													// Could be tree pixel, but tops of next row
													tempTilePosition.Y += 1;
													if (Data.GameModel.Player(Data.PlayerColor).PlayerMap.GetTileType(tempTilePosition) == TileType.Tree)
														playerCommands[(int)Data.PlayerColor].TargetLocation.SetFromTile(tempTilePosition);
												}
											}
										}
									}
								}

								Data.CurrentAssetCapabilityType = AssetCapabilityType.None;
							}
							else if (input.LeftClick)
							{
								if (Data.CurrentAssetCapabilityType == AssetCapabilityType.None || Data.CurrentAssetCapabilityType == AssetCapabilityType.BuildSimple)
								{
									if (input.LeftDown)
										input.MouseDown = mouseDetailedPosition.ToPoint();
									else
									{
										var searchColor = Data.PlayerColor;
										var previousSelections = Data.GameModel.SelectedAssets.ToList();

										var selectionRectangle = new Rectangle();
										selectionRectangle.X = Math.Min(input.MouseDown.X, mouseDetailedPosition.X);
										selectionRectangle.Y = Math.Min(input.MouseDown.Y, mouseDetailedPosition.Y);
										selectionRectangle.Width = Math.Max(input.MouseDown.X, mouseDetailedPosition.X) - selectionRectangle.X;
										selectionRectangle.Height = Math.Max(input.MouseDown.Y, mouseDetailedPosition.Y) - selectionRectangle.Y;

										if (selectionRectangle.Width < Position.TileWidth || selectionRectangle.Height < Position.TileHeight || input.DoubleClick)
										{
											selectionRectangle.X = mouseDetailedPosition.X;
											selectionRectangle.Y = mouseDetailedPosition.Y;
											selectionRectangle.Width = 0;
											selectionRectangle.Height = 0;
											searchColor = pixelType.Color;
										}

										if (searchColor != Data.PlayerColor)
											Data.GameModel.SelectedAssets.Clear();

										if (input.CurrentKeyboardState.IsKeyDown(Keys.LeftShift) || input.CurrentKeyboardState.IsKeyDown(Keys.RightShift))
										{
											if (Data.GameModel.SelectedAssets.Count > 0)
											{
												var tempAsset = Data.GameModel.SelectedAssets.First();
												if (tempAsset.Data.Color != Data.PlayerColor)
													Data.GameModel.SelectedAssets.Clear();
											}

											Data.GameModel.SelectedAssets.AddRange(Data.GameModel.Player(searchColor).SelectAssets(selectionRectangle, pixelType.AssetType, input.DoubleClick));
											Data.GameModel.SelectedAssets = Data.GameModel.SelectedAssets.Distinct().ToList();
										}
										else
										{
											previousSelections.Clear();
											Data.GameModel.SelectedAssets = Data.GameModel.Player(searchColor).SelectAssets(selectionRectangle, pixelType.AssetType, input.DoubleClick);
										}

										foreach (var asset in Data.GameModel.SelectedAssets)
										{
											var foundPrevious = previousSelections.Any(prevAsset => prevAsset == asset);
											if (!foundPrevious)
												Data.GameModel.Player(Data.PlayerColor).AddGameEvent(asset, EventType.Selection);
										}

										input.MouseDown = Point.Zero;
									}
									Data.CurrentAssetCapabilityType = AssetCapabilityType.None;
								}
								else
								{
									var playerCapability = PlayerCapability.FindCapability(Data.CurrentAssetCapabilityType);
									if (playerCapability != null && !input.LeftDown)
									{
										if ((playerCapability.TargetType == TargetType.Asset || playerCapability.TargetType == TargetType.TerrainOrAsset) && pixelType.AssetType != AssetType.None)
										{
											var newTarget = Data.GameModel.Player(pixelType.Color).SelectAsset(mouseDetailedPosition, pixelType.AssetType);
											if (playerCapability.CanApply(Data.GameModel.SelectedAssets.First(), Data.GameModel.Player(Data.PlayerColor), newTarget))
											{
												Data.GameModel.Player(Data.PlayerColor).AddGameEvent(newTarget, EventType.PlaceAction);

												playerCommands[(int)Data.PlayerColor].Action = Data.CurrentAssetCapabilityType;
												playerCommands[(int)Data.PlayerColor].Actors = Data.GameModel.SelectedAssets;
												playerCommands[(int)Data.PlayerColor].TargetColor = pixelType.Color;
												playerCommands[(int)Data.PlayerColor].TargetType = pixelType.AssetType;
												playerCommands[(int)Data.PlayerColor].TargetLocation = mouseDetailedPosition;
												Data.CurrentAssetCapabilityType = AssetCapabilityType.None;
											}
										}
										else if ((playerCapability.TargetType == TargetType.Terrain || playerCapability.TargetType == TargetType.TerrainOrAsset) && pixelType.AssetType == AssetType.None && pixelType.Color == PlayerColor.None)
										{
											var newTarget = Data.GameModel.Player(Data.PlayerColor).CreateMarker(mouseDetailedPosition, false);
											if (playerCapability.CanApply(Data.GameModel.SelectedAssets.First(), Data.GameModel.Player(Data.PlayerColor), newTarget))
											{
												Data.GameModel.Player(Data.PlayerColor).AddGameEvent(newTarget, EventType.PlaceAction);

												playerCommands[(int)Data.PlayerColor].Action = Data.CurrentAssetCapabilityType;
												playerCommands[(int)Data.PlayerColor].Actors = Data.GameModel.SelectedAssets;
												playerCommands[(int)Data.PlayerColor].TargetColor = PlayerColor.None;
												playerCommands[(int)Data.PlayerColor].TargetType = AssetType.None;
												playerCommands[(int)Data.PlayerColor].TargetLocation = mouseDetailedPosition;
												Data.CurrentAssetCapabilityType = AssetCapabilityType.None;
											}
										}
										// Linux: Empty else statement
									}
								}
							}

							if (Data.CurrentAssetCapabilityType == AssetCapabilityType.None)
							{
								if (pixelType.Color == Data.PlayerColor)
								{
									Data.CursorType = CursorType.Inspect;
								}
							}
							else
							{
								var playerCapability = PlayerCapability.FindCapability(Data.CurrentAssetCapabilityType);
								if (playerCapability != null)
								{
									var canApply = false;
									if (pixelType.AssetType == AssetType.None)
									{
										if (playerCapability.TargetType == TargetType.Terrain || playerCapability.TargetType == TargetType.TerrainOrAsset)
										{
											var newTarget = Data.GameModel.Player(Data.PlayerColor).CreateMarker(new Position(mouseDetailedPosition), false);
											canApply = playerCapability.CanApply(Data.GameModel.SelectedAssets.First(), Data.GameModel.Player(Data.PlayerColor), newTarget);
										}
									}
									else
									{
										if (playerCapability.TargetType == TargetType.Asset || playerCapability.TargetType == TargetType.TerrainOrAsset)
										{
											var newTarget = Data.GameModel.Player(pixelType.Color).SelectAsset(new Position(mouseDetailedPosition), pixelType.AssetType);
											canApply = playerCapability.CanApply(Data.GameModel.SelectedAssets.First(), Data.GameModel.Player(Data.PlayerColor), newTarget);
										}
									}

									Data.CursorType = canApply ? CursorType.TargetOn : CursorType.TargetOff;
								}
							}
						}
					}
					break;

				case UIComponent.ViewportBevelN:
					Data.ViewportRenderer.PanNorth(10);
					Data.CursorType = CursorType.ArrowN;
					break;
				case UIComponent.ViewportBevelE:
					Data.ViewportRenderer.PanEast(10);
					Data.CursorType = CursorType.ArrowE;
					break;
				case UIComponent.ViewportBevelS:
					Data.ViewportRenderer.PanSouth(10);
					Data.CursorType = CursorType.ArrowS;
					break;
				case UIComponent.ViewportBevelW:
					Data.ViewportRenderer.PanWest(10);
					Data.CursorType = CursorType.ArrowW;
					break;

				case UIComponent.MiniMap:
					if (input.LeftClick || input.LeftDown)
					{
						var screenToMiniMap = input.CurrentMouseState.Position - Data.MiniMapOffset;
						var miniMapToDetailedMap = MiniMapToDetailedMap(screenToMiniMap);
						Data.ViewportRenderer.CenterViewport(miniMapToDetailedMap);
					}
					break;

				case UIComponent.UnitDescription:
					if (input.LeftClick && !input.LeftDown)
					{
						var iconPressed = Data.UnitDescriptionRenderer.GetSelection(input.CurrentMouseState.Position - Data.UnitDescriptionOffset);
						if (Data.GameModel.SelectedAssets.Count == 1)
						{
							if (iconPressed == 0)
								Data.ViewportRenderer.CenterViewport(Data.GameModel.SelectedAssets.First().Position);
						}
						else if (iconPressed >= 0)
						{
							while (iconPressed != 0)
							{
								iconPressed--;
								Data.GameModel.SelectedAssets.RemoveAt(0);
							}
							while (Data.GameModel.SelectedAssets.Count > 1)
							{
								Data.GameModel.SelectedAssets.RemoveAt(Data.GameModel.SelectedAssets.Count - 1);
							}

							Data.GameModel.Player(Data.PlayerColor).AddGameEvent(Data.GameModel.SelectedAssets.First(), EventType.Selection);
						}
					}
					break;

				case UIComponent.UnitAction:
					if (input.LeftClick && !input.LeftDown)
					{
						var capabilityType = Data.UnitActionRenderer.GetSelection(input.CurrentMouseState.Position - Data.UnitActionOffset);
						var playerCapability = PlayerCapability.FindCapability(capabilityType);

						if (capabilityType != AssetCapabilityType.None)
						{
							Data.GameModel.Player(Data.PlayerColor).AddGameEvent(EventType.ButtonTick);
						}

						if (playerCapability != null)
						{
							if (playerCapability.TargetType == TargetType.None || playerCapability.TargetType == TargetType.Player)
							{
								var actor = Data.GameModel.SelectedAssets.First();
								if (playerCapability.CanApply(actor, Data.GameModel.Player(Data.PlayerColor), actor))
								{
									playerCommands[(int)Data.PlayerColor].Action = capabilityType;
									playerCommands[(int)Data.PlayerColor].Actors = Data.GameModel.SelectedAssets;
									playerCommands[(int)Data.PlayerColor].TargetColor = PlayerColor.None;
									playerCommands[(int)Data.PlayerColor].TargetType = AssetType.None;
									playerCommands[(int)Data.PlayerColor].TargetLocation = actor.Position;
									Data.CurrentAssetCapabilityType = AssetCapabilityType.None;
								}
							}
							else
							{
								Data.CurrentAssetCapabilityType = capabilityType;
							}
						}
						else
						{
							Data.CurrentAssetCapabilityType = capabilityType;
						}
					}
					break;
			}
		}

		public override void Calculate(GameTime gameTime, bool coveredByOtherScreen)
		{
			if (coveredByOtherScreen)
				return;

			if (CheckForWin())
				return;

			for (int i = 1; i < (int)PlayerColor.Max; i++)
			{
				if (Data.GameModel.Player((PlayerColor)i).IsAlive)
				{
					LibAi.CheckPlayer(Data.GameModel.Player((PlayerColor)i), Data.GameModel.GameCycle);

					if (Data.GameModel.Player((PlayerColor)i).IsAi && Settings.Debug.EnableAi)
					{
						if (Settings.Debug.UseAiScripts)
							LibAi.Initialize(Data.AiPlayers[i], playerCommands[i], Data.LoadingPlayerTypes[i], Data.GameModel.GameCycle);
						else
							Data.AiPlayers[i].CalculateCommand(playerCommands[i]);
					}
				}
			}

			for (var i = 1; i < (int)PlayerColor.Max; i++)
			{
				if (playerCommands[i].Action != AssetCapabilityType.None)
				{
					var playerCapability = PlayerCapability.FindCapability(playerCommands[i].Action);
					if (playerCapability != null)
					{
						PlayerAsset newTarget = null;

						if (playerCapability.TargetType != TargetType.None && playerCapability.TargetType != TargetType.Player)
						{
							if (playerCommands[i].TargetType == AssetType.None)
							{
								newTarget = Data.GameModel.Player((PlayerColor)i).CreateMarker(playerCommands[i].TargetLocation, true);
							}
							else
							{
								newTarget = Data.GameModel.Player(playerCommands[i].TargetColor).SelectAsset(playerCommands[i].TargetLocation, playerCommands[i].TargetType);
							}
						}

						foreach (var actor in playerCommands[i].Actors)
						{
							if (playerCapability.CanApply(actor, Data.GameModel.Player((PlayerColor)i), newTarget) && (actor.IsInterruptible || playerCommands[i].Action == AssetCapabilityType.Cancel))
							{
								playerCapability.Apply(actor, Data.GameModel.Player((PlayerColor)i), newTarget);
							}
						}
					}
					playerCommands[i].Action = AssetCapabilityType.None;
				}
			}

			Data.GameModel.Timestep();

			foreach (var asset in Data.GameModel.SelectedAssets.ToList())
			{
				if (!Data.GameModel.ValidAsset(asset) || !asset.IsAlive)
				{
					Data.GameModel.SelectedAssets.Remove(asset);
					continue;
				}

				if (asset.Speed == 0 || asset.GetAction() != AssetAction.Capability) continue;
				var command = asset.CurrentCommand();
				if (command.Target != null && command.Target.GetAction() == AssetAction.Construct)
				{
					Data.GameModel.Player(Data.PlayerColor).AddGameEvent(command.Target, EventType.Selection);

					Data.GameModel.SelectedAssets.Clear();
					Data.GameModel.SelectedAssets.Add(command.Target);
					break;
				}
			}
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

			if (coveredByOtherScreen || IsExiting)
				return;

			Data.SoundEventRenderer.RenderEvents(Data.ViewportRenderer.Bounds);
			Data.GameModel.ClearGameEvents();
		}

		public override void BeginDraw(GameTime gameTime)
		{
			// Render the minimap
			ScreenManager.Graphics.SetRenderTarget(Data.MiniMapRenderTarget);
			Data.MiniMapRenderer.DrawMiniMap(Data.ViewportRenderer.Bounds);

			// Render the resource display
			ScreenManager.Graphics.SetRenderTarget(Data.ResourceRenderTarget);
			ScreenManager.Graphics.Clear(Color.Transparent);
			Data.ResourceRenderer.DrawResources();

			// Render the unit description box
			ScreenManager.Graphics.SetRenderTarget(Data.UnitDescriptionRenderTarget);
			ScreenManager.Graphics.Clear(Color.Transparent);
			Data.UnitDescriptionRenderer.DrawUnitDescription(Data.GameModel.SelectedAssets);

			// Render the unit action box
			ScreenManager.Graphics.SetRenderTarget(Data.UnitActionRenderTarget);
			ScreenManager.Graphics.Clear(Color.Transparent);
			Data.UnitActionRenderer.DrawUnitAction(Data.GameModel.SelectedAssets, Data.CurrentAssetCapabilityType);

			// Get selection rectangle
			var selectionRectangle = new Rectangle();
			var tempPosition = new Position(ScreenManager.Input.CurrentMouseState.Position - Data.ViewportOffset + Data.ViewportRenderer.Bounds.Location);
			if (ScreenManager.Input.LeftDown && ScreenManager.Input.MouseDown != Point.Zero)
			{
				selectionRectangle.X = MathHelper.Min(ScreenManager.Input.MouseDown.X, tempPosition.X);
				selectionRectangle.Y = MathHelper.Min(ScreenManager.Input.MouseDown.Y, tempPosition.Y);
				selectionRectangle.Width = MathHelper.Max(ScreenManager.Input.MouseDown.X, tempPosition.X) - selectionRectangle.X;
				selectionRectangle.Height = MathHelper.Max(ScreenManager.Input.MouseDown.Y, tempPosition.Y) - selectionRectangle.Y;
			}
			else
				selectionRectangle.Location = tempPosition.ToPoint();

			// Add marker assets to list
			var selectedAssetsAndMarkers = Data.GameModel.SelectedAssets.ToList();
			selectedAssetsAndMarkers.AddRange(Data.GameModel.Player(Data.PlayerColor).PlayerMap.Assets.Where(a => a.Data.Type == AssetType.None));

			// Render the viewport
			Data.ViewportRenderer.DrawViewport(selectedAssetsAndMarkers, selectionRectangle);
		}

		public override void Draw(GameTime gameTime)
		{
			// Draw background
			for (var y = 0; y < ScreenManager.Graphics.Viewport.Height; y += Data.BackgroundTileset.TileHeight)
			{
				for (var x = 0; x < ScreenManager.Graphics.Viewport.Width; x += Data.BackgroundTileset.TileWidth)
				{
					ScreenManager.SpriteBatch.Draw(Data.BackgroundTileset.GetTile(0), new Vector2(x, y));
				}
			}

			// Draw bevels
			Data.InnerBevel.DrawBevel(new Rectangle(Data.ViewportOffset, Data.ViewportRenderTarget.Bounds.Size));
			Data.InnerBevel.DrawBevel(new Rectangle(Data.MiniMapOffset, Data.MiniMapRenderTarget.Bounds.Size));
			Data.OuterBevel.DrawBevel(new Rectangle(Data.UnitDescriptionOffset, Data.UnitDescriptionRenderTarget.Bounds.Size));
			Data.OuterBevel.DrawBevel(new Rectangle(Data.UnitActionOffset, Data.UnitActionRenderTarget.Bounds.Size));

			// Draw the viewport
			ScreenManager.SpriteBatch.Draw(Data.ViewportRenderTarget, Data.ViewportOffset.ToVector2());

			// Draw the minimap
			ScreenManager.SpriteBatch.Draw(Data.MiniMapRenderTarget, Data.MiniMapOffset.ToVector2());

			// Draw the resource display
			ScreenManager.SpriteBatch.Draw(Data.ResourceRenderTarget, new Vector2(Data.ViewportOffset.X, 0));

			// Draw the unit description box
			ScreenManager.SpriteBatch.Draw(Data.UnitDescriptionRenderTarget, Data.UnitDescriptionOffset.ToVector2());

			// Draw the unit action box
			ScreenManager.SpriteBatch.Draw(Data.UnitActionRenderTarget, Data.UnitActionOffset.ToVector2());

			// Draw the menu button
			Data.MenuButtonRenderer.DrawButton(menuButton);
		}

		/// <summary>
		/// Returns the <see cref="UIComponent"/> at <paramref name="pos"/>.
		/// </summary>
		private static UIComponent GetUIComponent(Point pos)
		{
			var viewportBounds = new Rectangle(Data.ViewportOffset, Data.ViewportRenderer.Bounds.Size);
			if (!viewportBounds.Contains(pos))
			{
				if (pos.X >= viewportBounds.Left - Data.InnerBevel.Width && pos.X < viewportBounds.Left)
				{
					if (pos.Y >= viewportBounds.Top && pos.Y < viewportBounds.Bottom)
						return UIComponent.ViewportBevelW;
				}
				else if (pos.X >= viewportBounds.Right && pos.X < viewportBounds.Right + Data.InnerBevel.Width)
				{
					if (pos.Y >= viewportBounds.Top && pos.Y < viewportBounds.Bottom)
						return UIComponent.ViewportBevelE;
				}
				else if (pos.X >= viewportBounds.Left && pos.X < viewportBounds.Right)
				{
					if (pos.Y >= viewportBounds.Top - Data.InnerBevel.Width && pos.Y < viewportBounds.Top)
						return UIComponent.ViewportBevelN;
					if (pos.Y >= viewportBounds.Bottom && pos.Y < viewportBounds.Bottom + Data.InnerBevel.Width)
						return UIComponent.ViewportBevelS;
				}
			}
			else
				return UIComponent.Viewport;

			var miniMapBounds = new Rectangle(Data.MiniMapOffset, Data.MiniMapRenderTarget.Bounds.Size);
			if (miniMapBounds.Contains(pos))
				return UIComponent.MiniMap;

			var unitDescriptionBounds = new Rectangle(Data.UnitDescriptionOffset, Data.UnitDescriptionRenderTarget.Bounds.Size);
			if (unitDescriptionBounds.Contains(pos))
				return UIComponent.UnitDescription;

			var unitActionBounds = new Rectangle(Data.UnitActionOffset, Data.UnitActionRenderTarget.Bounds.Size);
			if (unitActionBounds.Contains(pos))
				return UIComponent.UnitAction;

			return UIComponent.None;
		}

		/// <summary>
		/// Returns the detailed map position from the minimap mouse position.
		/// </summary>
		/// <param name="pos">Minimap mouse position</param>
		private static Position MiniMapToDetailedMap(Point pos)
		{
			var x = (int)(pos.X * Data.GameModel.ActualMap.MapWidth / (Data.MiniMapRenderer.VisibleWidth * Data.MiniMapRenderer.Scale.X));
			var y = (int)(pos.Y * Data.GameModel.ActualMap.MapHeight / (Data.MiniMapRenderer.VisibleHeight * Data.MiniMapRenderer.Scale.Y));

			x = MathHelper.Clamp(x, 0, Data.GameModel.ActualMap.MapWidth - 1);
			y = MathHelper.Clamp(y, 0, Data.GameModel.ActualMap.MapHeight - 1);

			var temp = new Position(x, y);
			temp.SetFromTile(temp);
			return temp;
		}

		/// <summary>
		/// Checks for the number of players alive on the map.
		/// If there are fewer than 2 players left,
		/// deletes the same file and exits the game.
		/// </summary>
		private bool CheckForWin()
		{
			var count = 0;

			for (var i = 1; i < (int)PlayerColor.Max; i++)
			{
				if (Data.GameModel.Player((PlayerColor)i).IsAlive)
					count++;
			}

			if (count < 2)
			{
				try
				{
					File.Delete(Settings.SaveFileName);
				}
				catch { }

				Data.GameModel = null;
				IsExiting = true;
				return true;
			}

			return false;
		}
	}
}