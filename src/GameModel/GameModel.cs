using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Extensions;
using Warcraft.Player;
using Warcraft.Player.Capabilities;
using Warcraft.Util;

namespace Warcraft.GameModel
{
	/// <summary>
	/// Game Model
	/// </summary>
	/// <remarks>
	/// All public data members here are serialized and stored when saving the game.
	/// </remarks>
	public class GameModel
	{
		private const int MaxUnitGroupCount = 10;
		public RandomNumberGenerator Random { get; private set; }

		public DecoratedMap ActualMap { get; private set; }
		public List<PlayerData> Players { get; private set; }
		public List<PlayerAsset> SelectedAssets { get; set; }
		public List<PlayerAsset>[] UnitGroups { get; set; }

		public Dictionary<Position, TileType> OriginalTileType { get; private set; }
		public int[,] LumberAvailable { get; private set; }
		public int[,] StoneAvailable { get; private set; }
		public Dictionary<Position, int> GrowthCurrentStep { get; private set; }
		public Dictionary<Position, int> GrowthSteps { get; private set; }
		public int GameCycle { get; private set; }
		public int MineTime { get; private set; }
		public int MineSteps { get; private set; }
		public int HarvestTime { get; private set; }
		public int HarvestSteps { get; private set; }
		public int QuarryTime { get; private set; }
		public int QuarrySteps { get; private set; }
		public int ConveyTime { get; private set; }
		public int ConveySteps { get; private set; }
		public int DeathTime { get; private set; }
		public int DeathSteps { get; private set; }
		public int DecayTime { get; private set; }
		public int DecaySteps { get; private set; }
		public int GoldPerMining { get; private set; }
		public int LumberPerHarvest { get; private set; }
		public int StonePerQuarry { get; private set; }
		public int StonePerRubble { get; private set; }

		protected PlayerAsset[,] AssetOccupancyMap { get; private set; }
		protected RouterMap RouterMap { get; private set; }

		/// <summary>
		/// Returns a dictionary of all assets belonging to all players, indexed by asset id.
		/// </summary>
		/// <remarks>
		/// We do it this way because we want to be able to save the asset list throughout a
		/// save/load game, otherwise the dictionary will not be loaded, and the asset would
		/// be missing from the AI.
		/// 
		/// If we made this dictionary in here, we would not be able to access it until
		/// the GameModel constructor is complete, but we would need the GameModel inside
		/// the PlayerData constructor. Therefore, it wouldn't be possible to create it in
		/// the GameModel.
		/// 
		/// Instead, we go through all of the players, get each of their assets, and add
		/// all of their assets to the dictionary and filter out the ones we have already added.
		/// </remarks>
		[JsonIgnore]
		public Dictionary<int, PlayerAsset> AllAssets
		{
			get
			{
				var assets = new Dictionary<int, PlayerAsset>();
				foreach (var player in Players)
				{
					foreach (var asset in player.AllAssets)
					{
						if (!assets.ContainsKey(asset.Key))
							assets[asset.Key] = asset.Value;
					}
				}
				return assets;
			}
		}

		[JsonConstructor, UsedImplicitly]
		public GameModel() { }

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			RouterMap = new RouterMap();
			AssetOccupancyMap = new PlayerAsset[ActualMap.MapHeight, ActualMap.MapWidth];
		}

		public GameModel(int mapIndex, ulong seed, List<PlayerColor> newColors)
		{
			Random = new RandomNumberGenerator();
			Random.Seed(seed);

			MineTime = 5;
			MineSteps = PlayerAsset.UpdateFrequency * MineTime;
			HarvestTime = 5;
			HarvestSteps = PlayerAsset.UpdateFrequency * HarvestTime;
			QuarryTime = 5;
			QuarrySteps = PlayerAsset.UpdateFrequency * QuarryTime;
			ConveyTime = 1;
			ConveySteps = PlayerAsset.UpdateFrequency * ConveyTime;
			DeathTime = 1;
			DeathSteps = PlayerAsset.UpdateFrequency * DeathTime;
			DecayTime = 4;
			DecaySteps = PlayerAsset.UpdateFrequency * DecayTime;
			GoldPerMining = 100;
			LumberPerHarvest = 100;
			StonePerQuarry = 100;
			StonePerRubble = 75;

			ActualMap = DecoratedMap.DuplicateMap(mapIndex, newColors);
			RouterMap = new RouterMap();
			SelectedAssets = new List<PlayerAsset>();
			UnitGroups = new List<PlayerAsset>[MaxUnitGroupCount];
			for (var i = 0; i < MaxUnitGroupCount; i++)
				UnitGroups[i] = new List<PlayerAsset>();

			Players = new List<PlayerData>(new PlayerData[(int)PlayerColor.Max]);
			for (var playerIndex = 0; playerIndex < (int)PlayerColor.Max; playerIndex++)
			{
				Players[playerIndex] = new PlayerData(ActualMap, (PlayerColor)playerIndex);
			}

			AssetOccupancyMap = new PlayerAsset[ActualMap.MapHeight, ActualMap.MapWidth];

			OriginalTileType = new Dictionary<Position, TileType>();

			LumberAvailable = new int[ActualMap.MapHeight, ActualMap.MapWidth];
			StoneAvailable = new int[ActualMap.MapHeight, ActualMap.MapWidth];
			for (var y = 0; y < ActualMap.MapHeight; y++)
			{
				for (var x = 0; x < ActualMap.MapWidth; x++)
				{
					var tileType = ActualMap.GetTileType(x, y);
					switch (tileType)
					{
						case TileType.Tree: LumberAvailable[y, x] = Players[0].Lumber; break;
						case TileType.Rock: StoneAvailable[y, x] = Players[0].Stone; break;
					}
				}
			}

			GrowthCurrentStep = new Dictionary<Position, int>();
			GrowthSteps = new Dictionary<Position, int>();

			// Todo: The rest
		}

		/// <summary>
		/// Returns whether this asset exists on the <see cref="ActualMap"/>.
		/// </summary>
		public bool ValidAsset(PlayerAsset asset)
		{
			return ActualMap.Assets.Contains(asset);
		}

		/// <summary>
		/// Returns the <see cref="PlayerData"/> with the respective <paramref name="color"/>.
		/// </summary>
		public PlayerData Player(PlayerColor color)
		{
			if (color < 0 || color >= PlayerColor.Max)
				return null;

			return Players[(int)color];
		}

		public void Timestep()
		{
			var currentEvents = new List<GameEvent>();
			var tempEvent = new GameEvent();

			// Clear occupancy map
			for (var y = 0; y < AssetOccupancyMap.GetLength(0); y++)
			{
				for (var x = 0; x < AssetOccupancyMap.GetLength(1); x++)
				{
					AssetOccupancyMap[y, x] = null;
				}
			}

			// Update occupancy map
			foreach (var asset in ActualMap.Assets)
			{
				if (!asset.PerformingHiddenAction)
				{
					AssetOccupancyMap[asset.TilePosition.Y, asset.TilePosition.X] = asset;
				}
			}

			// Update player visibilities
			for (var playerIndex = 1; playerIndex < (int)PlayerColor.Max; playerIndex++)
			{
				if (Players[playerIndex].IsAlive)
				{
					Players[playerIndex].UpdateVisibility();
				}
			}

			foreach (var asset in ActualMap.Assets.ToList())
			{
				if (asset.GetAction() == AssetAction.None)
					asset.PopCommand();

				if (asset.GetAction() == AssetAction.Capability)
				{
					var command = asset.CurrentCommand();
					if (command.ActivatedCapability != null)
					{
						command.ActivatedCapability.IncrementStep();
					}
					else
					{
						var playerCapability = PlayerCapability.FindCapability(command.Capability);
						asset.PopCommand();

						if (playerCapability.CanApply(asset, Players[(int)asset.Data.Color], command.Target))
						{
							playerCapability.Apply(asset, Players[(int)asset.Data.Color], command.Target);
						}
					}
				}
				else if (asset.GetAction() == AssetAction.HarvestLumber)
				{
					var command = asset.CurrentCommand();
					var tilePosition = command.Target.TilePosition;
					var harvestDirection = asset.TilePosition.AdjacentTileDirection(tilePosition);

					if (ActualMap.GetTileType(tilePosition) != TileType.Tree)
					{
						harvestDirection = Direction.Max;
						tilePosition = asset.TilePosition;
					}

					if (harvestDirection == Direction.Max)
					{
						if (asset.TilePosition == tilePosition)
						{
							// Find new lumber
							var newPosition = Players[(int)asset.Data.Color].PlayerMap.FindNearestReachableTileType(asset.TilePosition, TileType.Tree);
							asset.PopCommand();
							if (newPosition.X >= 0)
							{
								newPosition.SetFromTile(newPosition);
								command.Target = Players[(int)asset.Data.Color].CreateMarker(newPosition, false);
								asset.PushCommand(command);
								command.Action = AssetAction.Walk;
								asset.PushCommand(command);
								asset.Step = 0;
							}
						}
						else
						{
							var newCommand = command;
							newCommand.Action = AssetAction.Walk;
							asset.PushCommand(newCommand);
							asset.Step = 0;
						}
					}
					else
					{
						tempEvent.Type = EventType.Harvest;
						tempEvent.Asset = asset;
						currentEvents.Add(tempEvent);

						asset.Direction = harvestDirection;
						asset.Step++;

						if (asset.Step >= HarvestSteps)
						{
							var nearestRepository = Players[(int)asset.Data.Color].FindNearestOwnedAsset(asset.Position, AssetType.TownHall, AssetType.Keep, AssetType.Castle, AssetType.LumberMill);

							LumberAvailable[tilePosition.Y, tilePosition.X] -= LumberPerHarvest;
							if (LumberAvailable[tilePosition.Y, tilePosition.X] <= 0)
							{
								ActualMap.ChangeTileType(tilePosition, TileType.Stump);

								GrowthCurrentStep[tilePosition] = 0;
							}

							if (nearestRepository != null)
							{
								command.Action = AssetAction.ConveyLumber;
								command.Target = nearestRepository;
								asset.PushCommand(command);
								command.Action = AssetAction.Walk;
								asset.PushCommand(command);
								asset.Lumber = LumberPerHarvest;
								asset.Step = 0;
							}
							else
							{
								asset.PopCommand();
								asset.Lumber = LumberPerHarvest;
								asset.Step = 0;
							}
						}
					}
				}
				else if (asset.GetAction() == AssetAction.QuarryStone)
				{
					var command = asset.CurrentCommand();
					var tilePosition = command.Target.TilePosition;
					var harvestDirection = asset.TilePosition.AdjacentTileDirection(tilePosition);

					var currentTileType = ActualMap.GetTileType(tilePosition);
					if (currentTileType != TileType.Rock)
					{
						harvestDirection = Direction.Max;
						tilePosition = asset.TilePosition;
					}

					// Use the asset's direction if they're harvesting rubble
					// Otherwise, the direction will be max, which would have no sprite.
					if (currentTileType == TileType.Rubble)
					{
						harvestDirection = asset.Direction;
					}

					if (harvestDirection == Direction.Max)
					{
						if (asset.TilePosition == tilePosition)
						{
							// Find new stone
							var newPosition = Players[(int)asset.Data.Color].PlayerMap.FindNearestReachableTileType(asset.TilePosition, TileType.Rock);
							asset.PopCommand();
							if (newPosition.X >= 0)
							{
								newPosition.SetFromTile(newPosition);
								command.Target = Players[(int)asset.Data.Color].CreateMarker(newPosition, false);
								asset.PushCommand(command);
								command.Action = AssetAction.Walk;
								asset.PushCommand(command);
								asset.Step = 0;
							}
						}
						else
						{
							var newCommand = command;
							newCommand.Action = AssetAction.Walk;
							asset.PushCommand(newCommand);
							asset.Step = 0;
						}
					}
					else
					{
						tempEvent.Type = EventType.Quarry;
						tempEvent.Asset = asset;
						currentEvents.Add(tempEvent);

						asset.Direction = harvestDirection;
						asset.Step++;

						if (asset.Step >= QuarrySteps)
						{
							var nearestRepository = Players[(int)asset.Data.Color].FindNearestOwnedAsset(asset.Position, AssetType.TownHall, AssetType.Keep, AssetType.Castle, AssetType.Blacksmith);

							var stoneAmount = 0;
							// Change to dirt if rock
							if (currentTileType == TileType.Rock)
							{
								stoneAmount = StonePerQuarry;
								StoneAvailable[tilePosition.Y, tilePosition.X] -= stoneAmount;
								if (StoneAvailable[tilePosition.Y, tilePosition.X] <= 0)
									ActualMap.ChangeTileType(tilePosition, TileType.Dirt);
							}
							// Change to underlying tile type if rubble
							else if (currentTileType == TileType.Rubble)
							{
								stoneAmount = StonePerRubble;
								ActualMap.ChangeTileType(tilePosition, OriginalTileType[tilePosition]);
								OriginalTileType.Remove(tilePosition);
							}

							if (nearestRepository != null)
							{
								command.Action = AssetAction.ConveyStone;
								command.Target = nearestRepository;
								asset.PushCommand(command);
								command.Action = AssetAction.Walk;
								asset.PushCommand(command);
								asset.Stone = stoneAmount;
								asset.Step = 0;
							}
							else
							{
								asset.PopCommand();
								asset.Stone = stoneAmount;
								asset.Step = 0;
							}
						}
					}
				}
				else if (asset.GetAction() == AssetAction.MineGold)
				{
					var command = asset.CurrentCommand();
					var closestPosition = command.Target.ClosestPosition(asset.Position);
					var tilePosition = new Position();

					tilePosition.SetToTile(closestPosition);
					var mineDirection = asset.TilePosition.AdjacentTileDirection(tilePosition);

					if (mineDirection == Direction.Max && asset.TilePosition != tilePosition)
					{
						var newCommand = command;
						newCommand.Action = AssetAction.Walk;
						asset.PushCommand(newCommand);
						asset.Step = 0;
					}
					else
					{
						if (asset.Step == 0)
						{
							if ((command.Target.Commands.Count + 1) * GoldPerMining <= command.Target.Gold)
							{
								var newCommand = new AssetCommand();
								newCommand.Action = AssetAction.Build;
								newCommand.Target = asset;

								command.Target.EnqueueCommand(newCommand);
								asset.Step++;
								asset.SetTilePosition(command.Target.TilePosition);
							}
							else
							{
								asset.PopCommand();
							}
						}
						else
						{
							asset.Step++;
							if (asset.Step >= MineSteps)
							{
								var oldTarget = command.Target;
								var nearestRepository = Players[(int)asset.Data.Color].FindNearestOwnedAsset(asset.Position, AssetType.TownHall, AssetType.Keep, AssetType.Castle);
								var nextTarget = new Position(Players[(int)asset.Data.Color].PlayerMap.MapWidth - 1, Players[(int)asset.Data.Color].PlayerMap.MapHeight - 1);

								command.Target.Gold -= GoldPerMining;
								command.Target.PopCommand();

								if (command.Target.Gold <= 0)
								{
									var newCommand = new AssetCommand();
									newCommand.Action = AssetAction.Death;

									command.Target.ClearCommands();
									command.Target.PushCommand(newCommand);
									command.Target.Step = 0;
								}

								asset.Gold = GoldPerMining;

								if (nearestRepository != null)
								{
									command.Action = AssetAction.ConveyGold;
									command.Target = nearestRepository;
									asset.PushCommand(command);
									command.Action = AssetAction.Walk;
									asset.PushCommand(command);
									asset.Step = 0;
									nextTarget = command.Target.TilePosition;
								}
								else
								{
									asset.PopCommand();
								}

								asset.SetTilePosition(Players[(int)asset.Data.Color].PlayerMap.FindAssetPlacement(asset, oldTarget, nextTarget));
							}
						}
					}
				}
				else if (asset.GetAction() == AssetAction.StandGround)
				{
					var command = asset.CurrentCommand();

					var newTarget = Players[(int)asset.Data.Color].FindNearestEnemy(asset.Position, asset.EffectiveRange);
					if (newTarget == null)
					{
						command.Action = AssetAction.None;
					}
					else
					{
						command.Action = AssetAction.Attack;
						command.Target = newTarget;
					}

					asset.PushCommand(command);
					asset.Step = 0;
				}
				else if (asset.GetAction() == AssetAction.Repair)
				{
					var currentCommand = asset.CurrentCommand();

					if (currentCommand.Target.IsAlive)
					{
						var repairDirection = asset.TilePosition.AdjacentTileDirection(currentCommand.Target.TilePosition, currentCommand.Target.Data.Size);
						if (repairDirection == Direction.Max)
						{
							currentCommand.Action = AssetAction.Walk;
							asset.PushCommand(currentCommand);
							asset.Step = 0;
						}
						else
						{
							asset.Direction = repairDirection;
							asset.Step++;

							if (asset.Step == asset.Data.AttackSteps)
							{
								var playerData = Players[(int)asset.Data.Color];
								if (playerData.Gold != 0 && playerData.Lumber != 0 && playerData.Stone != 0)
								{
									var repairPoints = currentCommand.Target.Data.Health * (asset.Data.AttackSteps + asset.Data.ReloadSteps) / (PlayerAsset.UpdateFrequency * currentCommand.Target.Data.BuildTime);
									if (repairPoints == 0)
										repairPoints = 1;

									playerData.Gold--;
									playerData.Lumber--;
									playerData.Stone--;

									currentCommand.Target.Health += repairPoints;
									if (currentCommand.Target.Health == currentCommand.Target.Data.Health)
									{
										playerData.AddGameEvent(asset, EventType.WorkComplete);
										asset.PopCommand();
									}
								}
								else
								{
									asset.PopCommand();
								}
							}

							if (asset.Step >= asset.Data.AttackSteps + asset.Data.ReloadSteps)
								asset.Step = 0;
						}
					}
					else
					{
						asset.PopCommand();
					}
				}
				else if (asset.GetAction() == AssetAction.Attack)
				{
					var currentCommand = asset.CurrentCommand();

					if (asset.Data.Type == AssetType.None)
					{
						var closestTargetPosition = currentCommand.Target.ClosestPosition(asset.Position);
						var deltaPosition = new Position(closestTargetPosition.X - asset.Position.X, closestTargetPosition.Y - asset.Position.Y);
						var movement = Position.TileWidth * 5 / PlayerAsset.UpdateFrequency;
						var targetDistance = asset.Position.Distance(closestTargetPosition);
						var divisor = (targetDistance + movement - 1) / movement;

						if (divisor != 0)
						{
							deltaPosition.X /= divisor;
							deltaPosition.Y /= divisor;
						}

						asset.Position.X += deltaPosition.X;
						asset.Position.Y += deltaPosition.Y;
						if (asset.Position != closestTargetPosition)
							asset.Direction = asset.Position.DirectionTo(closestTargetPosition);

						if (asset.Position.DistanceSquared(closestTargetPosition) < Position.HalfTileWidth * Position.HalfTileHeight)
						{
							tempEvent.Type = EventType.MissileHit;
							tempEvent.Asset = asset;
							currentEvents.Add(tempEvent);

							if (currentCommand.Target.IsAlive)
							{
								Players[(int)currentCommand.Target.Data.Color].AddGameEvent(currentCommand.Target, EventType.Attacked);

								var targetCommand = currentCommand.Target.CurrentCommand();
								if (targetCommand.Action != AssetAction.MineGold)
								{
									if (targetCommand.Action == AssetAction.ConveyGold || targetCommand.Action == AssetAction.ConveyLumber || targetCommand.Action == AssetAction.ConveyStone)
									{
										currentCommand.Target = targetCommand.Target;
									}
									else if (targetCommand.Action == AssetAction.Capability && targetCommand.Target != null)
									{
										if (currentCommand.Target.Speed != 0 && targetCommand.Action == AssetAction.Construct)
										{
											currentCommand.Target = targetCommand.Target;
										}
									}

									currentCommand.Target.Health -= asset.Health;

									if (currentCommand.Target.IsAlive)
									{
										if (currentCommand.Target.Data.Type == AssetType.Peasant)
										{
											CalculatePeasantFlee(currentCommand.Target, asset);
										}
										else if (currentCommand.Target.IsIdle && currentCommand.Target.IsMilitary)
										{
											// Counterattack by putting the target unit into Stand Ground
											currentCommand.Target.ClearCommands();
											currentCommand.Target.PushCommand(new AssetCommand
											{
												Action = AssetAction.StandGround
											});
										}
									}
									else
									{
										tempEvent.Type = EventType.Death;
										tempEvent.Asset = currentCommand.Target;
										currentEvents.Add(tempEvent);

										var command = currentCommand.Target.CurrentCommand();
										// Remove Constructing
										if (command.Action == AssetAction.Capability && command.Target != null)
										{
											if (command.Target.GetAction() == AssetAction.Construct)
												Players[(int)command.Target.Data.Color].DeleteAsset(command.Target);
										}
										else if (command.Action == AssetAction.Construct)
										{
											if (currentCommand.Target.Data.Type != AssetType.Wall)
												command.Target?.Commands.Clear();
										}

										currentCommand.Target.Direction = asset.Direction.Opposite();
										command.Action = AssetAction.Death;
										currentCommand.Target.Commands.Clear();
										currentCommand.Target.PushCommand(command);
										currentCommand.Target.Step = 0;

										// Kill sheltered peasants
										if (currentCommand.Target.ShelteredPeasants.Count != 0)
										{
											foreach (var peasant in currentCommand.Target.ShelteredPeasants)
											{
												currentEvents.Add(new GameEvent
												{
													Type = EventType.Death,
													Asset = peasant
												});
												Players[(int)peasant.Data.Color].DeleteAsset(peasant);
											}
										}
									}
								}
							}

							Players[(int)asset.Data.Color].DeleteAsset(asset);
						}
					}
					else if (currentCommand.Target.IsAlive)
					{
						if (asset.EffectiveRange == 1)
						{
							var attackDirection = asset.TilePosition.AdjacentTileDirection(currentCommand.Target.TilePosition, currentCommand.Target.Data.Size);
							if (attackDirection == Direction.Max)
							{
								var nextCommand = asset.NextCommand();
								if (nextCommand.Action != AssetAction.StandGround)
								{
									currentCommand.Action = AssetAction.Walk;
									asset.PushCommand(currentCommand);
									asset.Step = 0;
								}
								else
									asset.PopCommand();
							}
							else
							{
								asset.Direction = attackDirection;
								asset.Step++;

								if (asset.Step == asset.Data.AttackSteps)
								{
									var damage = MathHelper.Max(0, asset.EffectiveBasicDamage - currentCommand.Target.EffectiveArmor);
									damage += asset.EffectivePiercingDamage;

									if ((Random.Random() & 0x1) == 1)
										damage /= 2;

									currentCommand.Target.Health -= damage;

									tempEvent.Type = EventType.MeleeHit;
									tempEvent.Asset = asset;
									currentEvents.Add(tempEvent);
									Players[(int)currentCommand.Target.Data.Color].AddGameEvent(currentCommand.Target, EventType.Attacked);

									if (currentCommand.Target.IsAlive)
									{
										if (currentCommand.Target.Data.Type == AssetType.Peasant)
										{
											CalculatePeasantFlee(currentCommand.Target, asset);
										}
										else if (currentCommand.Target.IsIdle && currentCommand.Target.IsMilitary)
										{
											// Counterattack by putting the target unit into Stand Ground
											currentCommand.Target.ClearCommands();
											currentCommand.Target.PushCommand(new AssetCommand
											{
												Action = AssetAction.StandGround
											});
										}
									}
									else
									{
										tempEvent.Type = EventType.Death;
										tempEvent.Asset = currentCommand.Target;
										currentEvents.Add(tempEvent);

										var command = currentCommand.Target.CurrentCommand();
										if (command.Action == AssetAction.Capability && command.Target != null)
										{
											if (command.Target.GetAction() == AssetAction.Construct)
												Players[(int)command.Target.Data.Color].DeleteAsset(command.Target);
										}
										else if (command.Action == AssetAction.Construct)
										{
											if (currentCommand.Target.Data.Type != AssetType.Wall)
												command.Target?.Commands.Clear();
										}

										command.Capability = AssetCapabilityType.None;
										command.Target = null;
										command.ActivatedCapability = null;
										command.Action = AssetAction.Death;

										currentCommand.Target.Direction = attackDirection.Opposite();
										currentCommand.Target.Commands.Clear();
										currentCommand.Target.PushCommand(command);
										currentCommand.Target.Step = 0;

										// Kill sheltered peasants
										if (currentCommand.Target.ShelteredPeasants.Count != 0)
										{
											foreach (var peasant in currentCommand.Target.ShelteredPeasants)
											{
												currentEvents.Add(new GameEvent
												{
													Type = EventType.Death,
													Asset = peasant
												});
												Players[(int)peasant.Data.Color].DeleteAsset(peasant);
											}
										}
									}
								}

								if (asset.Step >= asset.Data.AttackSteps + asset.Data.ReloadSteps)
									asset.Step = 0;
							}
						}
						else
						{
							var closestTargetPosition = currentCommand.Target.ClosestPosition(asset.Position);
							if (closestTargetPosition.DistanceSquared(asset.Position) > RangeToDistanceSquared(asset.EffectiveRange))
							{
								var nextCommand = asset.NextCommand();
								if (nextCommand.Action != AssetAction.StandGround)
								{
									currentCommand.Action = AssetAction.Walk;
									asset.PushCommand(currentCommand);
									asset.Step = 0;
								}
								else
									asset.PopCommand();
							}
							else
							{
								var attackDirection = asset.Position.DirectionTo(closestTargetPosition);
								asset.Direction = attackDirection;
								asset.Step++;

								if (asset.Step == asset.Data.AttackSteps)
								{
									var arrowAsset = Players[(int)PlayerColor.None].CreateAsset("None");
									var damage = MathHelper.Max(0, asset.EffectiveBasicDamage - currentCommand.Target.EffectiveArmor);
									damage += asset.EffectivePiercingDamage;

									if ((Random.Random() & 0x1) == 1)
										damage /= 2;

									tempEvent.Type = EventType.MissileFire;
									tempEvent.Asset = asset;
									currentEvents.Add(tempEvent);

									arrowAsset.Health = damage;
									arrowAsset.SetPosition(asset.Position);

									if (arrowAsset.Position.X < closestTargetPosition.X)
										arrowAsset.Position.X += Position.HalfTileWidth;
									else if (arrowAsset.Position.X > closestTargetPosition.X)
										arrowAsset.Position.X -= Position.HalfTileWidth;

									if (arrowAsset.Position.Y < closestTargetPosition.Y)
										arrowAsset.Position.Y += Position.HalfTileHeight;
									else if (arrowAsset.Position.Y > closestTargetPosition.Y)
										arrowAsset.Position.Y -= Position.HalfTileHeight;

									arrowAsset.Direction = attackDirection;

									var attackCommand = new AssetCommand();
									attackCommand.Action = AssetAction.Construct;
									attackCommand.Target = asset;
									arrowAsset.PushCommand(attackCommand);

									attackCommand.Action = AssetAction.Attack;
									attackCommand.Target = currentCommand.Target;
									arrowAsset.PushCommand(attackCommand);
								}

								if (asset.Step >= asset.Data.AttackSteps + asset.Data.ReloadSteps)
									asset.Step = 0;
							}
						}
					}
					else
					{
						var nextCommand = asset.NextCommand();
						asset.PopCommand();

						if (nextCommand.Action != AssetAction.StandGround)
						{
							var newTarget = Players[(int)asset.Data.Color].FindNearestEnemy(asset.Position, asset.EffectiveSight);
							if (newTarget != null)
							{
								currentCommand.Target = newTarget;
								asset.PushCommand(currentCommand);
								asset.Step = 0;
							}
						}
					}
				}
				else if (asset.Conveying)
				{
					asset.Step++;

					if (asset.Step >= ConveySteps)
					{
						var command = asset.CurrentCommand();

						Players[(int)asset.Data.Color].Gold += asset.Gold;
						Players[(int)asset.Data.Color].Lumber += asset.Lumber;
						Players[(int)asset.Data.Color].Stone += asset.Stone;

						asset.Gold = 0;
						asset.Lumber = 0;
						asset.Stone = 0;

						asset.PopCommand();
						asset.Step = 0;

						var nextTarget = new Position(Players[(int)asset.Data.Color].PlayerMap.MapWidth - 1, Players[(int)asset.Data.Color].PlayerMap.MapHeight - 1);
						if (asset.GetAction() != AssetAction.None)
							nextTarget = asset.CurrentCommand().Target.TilePosition;

						asset.SetTilePosition(Players[(int)asset.Data.Color].PlayerMap.FindAssetPlacement(asset, command.Target, nextTarget));
					}
				}
				else if (asset.GetAction() == AssetAction.Construct)
				{
					asset.CurrentCommand().ActivatedCapability?.IncrementStep();
				}
				else if (asset.GetAction() == AssetAction.Death)
				{
					asset.Step++;
					if (asset.Step > DeathSteps)
					{
						if (asset.Speed != 0)
						{
							var decayCommand = new AssetCommand
							{
								Action = AssetAction.Decay
							};

							var corpseAsset = Players[(int)PlayerColor.None].CreateAsset("None");
							corpseAsset.SetPosition(asset.Position);
							corpseAsset.Direction = asset.Direction;
							corpseAsset.PushCommand(decayCommand);
						}
						// Destroyed player-built walls become rubble
						else if (asset.Data.Type == AssetType.Wall)
						{
							// Only add if it's not already there, otherwise
							// you could have a tile repeatedly be rubble, even after harvesting.
							if (!OriginalTileType.ContainsKey(asset.TilePosition))
								OriginalTileType[asset.TilePosition] = ActualMap.GetTileType(asset.TilePosition);
							ActualMap.ChangeTileType(asset.TilePosition, TileType.Rubble);
						}

						Players[(int)asset.Data.Color].DeleteAsset(asset);
					}
				}
				else if (asset.GetAction() == AssetAction.Decay)
				{
					asset.Step++;
					if (asset.Step > DecaySteps)
						Players[(int)asset.Data.Color].DeleteAsset(asset);
				}

				if (asset.GetAction() == AssetAction.Walk)
				{
					if (asset.Position.IsTileAligned)
					{
						var command = asset.CurrentCommand();
						var nextCommand = asset.NextCommand();
						var mapTarget = command.Target.ClosestPosition(asset.Position);

						if (ActualMap.GetTileType(asset.TilePosition) == TileType.Stump || ActualMap.GetTileType(asset.TilePosition) == TileType.Seedling)
						{
							ActualMap.ChangeTileType(asset.TilePosition, TileType.Stump);
							GrowthCurrentStep[asset.TilePosition] = 0;
						}

						if (nextCommand.Action == AssetAction.Attack)
						{
							if (nextCommand.Target.ClosestPosition(asset.Position).DistanceSquared(asset.Position) <= RangeToDistanceSquared(asset.EffectiveRange))
							{
								asset.PopCommand();
								asset.Step = 0;
								continue;
							}
						}

						var travelDirection = RouterMap.FindRoute(Players[(int)asset.Data.Color].PlayerMap, asset, mapTarget);
						if (travelDirection != Direction.Max)
							asset.Direction = travelDirection;
						else
						{
							var tilePosition = new Position();
							tilePosition.SetToTile(mapTarget);

							if (asset.TilePosition == tilePosition || asset.TilePosition.AdjacentTileDirection(tilePosition) != Direction.Max)
							{
								asset.PopCommand();
								asset.Step = 0;
								continue;
							}

							if (nextCommand.Action == AssetAction.HarvestLumber)
							{
								var newPosition = Players[(int)asset.Data.Color].PlayerMap.FindNearestReachableTileType(asset.TilePosition, TileType.Tree);
								// Find new lumber
								asset.PopCommand();
								asset.PopCommand();
								if (newPosition.X >= 0)
								{
									newPosition.SetFromTile(newPosition);
									command.Action = AssetAction.HarvestLumber;
									command.Target = Players[(int)asset.Data.Color].CreateMarker(newPosition, false);
									asset.PushCommand(command);
									command.Action = AssetAction.Walk;
									asset.PushCommand(command);
									asset.Step = 0;
									continue;
								}
							}
							else if (nextCommand.Action == AssetAction.QuarryStone)
							{
								var newPosition = Players[(int)asset.Data.Color].PlayerMap.FindNearestReachableTileType(asset.TilePosition, TileType.Rock);
								// Find new stone
								asset.PopCommand();
								asset.PopCommand();
								if (newPosition.X >= 0)
								{
									newPosition.SetFromTile(newPosition);
									command.Action = AssetAction.QuarryStone;
									command.Target = Players[(int)asset.Data.Color].CreateMarker(newPosition, false);
									asset.PushCommand(command);
									command.Action = AssetAction.Walk;
									asset.PushCommand(command);
									asset.Step = 0;
									continue;
								}
							}
							else
							{
								command.Action = AssetAction.None;
								asset.PushCommand(command);
								asset.Step = 0;
								continue;
							}
						}
					}

					if (!asset.MoveStep(AssetOccupancyMap))
					{
						asset.Direction = asset.Position.TileOctant().Opposite();
					}
				}
			}

			// Health regeneration
			for (var index = 0; index < (int)PlayerColor.Max; index++)
			{
				// Calculate healing period
				if (Players[index].FoodExcess > 0)
					Players[index].HealSteps = Math.Max((int)(Math.Pow(Players[index].UnitCount, 2) / (2 * Players[index].FoodExcess)), 4);
				else
					Players[index].HealSteps = 0;

				// Heal idle assets
				foreach (var unit in Players[index].Assets.Where(a => a.Speed != 0))
				{
					if (unit.IsIdle)
					{
						unit.IdleSteps++;
						if (unit.IdleSteps >= Players[index].HealSteps && Players[index].HealSteps > 0)
						{
							if (unit.Health < unit.Data.Health)
								unit.Health++;

							unit.IdleSteps = 0;
						}
					}
					else
					{
						unit.IdleSteps = 0;
					}
				}
			}

			CalculateRandomTurn();

			CalculateTreeGrowth();

			GameCycle++;

			for (var index = 0; index < (int)PlayerColor.Max; index++)
			{
				Players[index].GameCycle++;
				Players[index].GameEvents.AddRange(currentEvents);
			}
		}

		/// <summary>
		/// Forces peasants that are idle or with low health to run from the <paramref name="attacker"/>.
		/// </summary>
		public void CalculatePeasantFlee(PlayerAsset target, PlayerAsset attacker)
		{
			var action = target.GetAction();
			if (target.Data.Type == AssetType.Peasant &&
				(target.IsIdle ||
				 target.Health < target.Data.Health * 0.25f && (action == AssetAction.Repair || action == AssetAction.HarvestLumber || action == AssetAction.QuarryStone)))
			{
				target.ClearCommands();
				target.PushCommand(new AssetCommand
				{
					Action = AssetAction.Walk,
					Target = Players[(int)target.Data.Color].CreateMarker(GetFleePosition(target, attacker), false)
				});
			}
		}

		/// <summary>
		/// Returns the position one tile from the <paramref name="attacker"/>.
		/// </summary>
		public static Position GetFleePosition(PlayerAsset peasant, PlayerAsset attacker)
		{
			// Makes sure that peasant moves to adjacent tile
			var newPosition = new Position();
			newPosition.SetXFromTile(peasant.TilePosition.X + Math.Sign(peasant.TilePosition.X - attacker.TilePosition.X));
			newPosition.SetYFromTile(peasant.TilePosition.Y + Math.Sign(peasant.TilePosition.Y - attacker.TilePosition.Y));
			return newPosition;
		}

		/// <summary>
		/// Calculates random turn step for each unit on the map.
		/// </summary>
		public void CalculateRandomTurn()
		{
			for (var index = 0; index < (int)PlayerColor.Max; index++)
			{
				foreach (var unit in Players[index].Assets.Where(a => a.IsAlive && a.Speed != 0))
				{
					if (unit.IsIdle)
					{
						// Was already idle, decrement and check
						if (unit.RandomTurnSteps < int.MaxValue)
						{
							unit.RandomTurnSteps--;
							if (unit.RandomTurnSteps <= 0)
							{
								// Add or subtract one to the current direction
								// to only change one step CW or CCW
								unit.Direction = ((int)unit.Direction + ((Random.Random() & 0x1) == 1 ? 1 : -1)).ToDirection();
								unit.RandomTurnSteps = (int)(Random.Random() % 60 + 40);
							}
						}
						// Wasn't idle, assign new time
						else
						{
							unit.RandomTurnSteps = (int)(Random.Random() % 60 + 40);
						}
					}
					else
						unit.RandomTurnSteps = int.MaxValue;
				}
			}
		}

		/// <summary>
		/// Calculates forest regrowth step.
		/// </summary>
		public void CalculateTreeGrowth()
		{
			foreach (var pos in GrowthCurrentStep.Keys.ToList())
			{
				var numberAdjacentTrees = 0;
				for (var x = -1; x <= 1; x++)
				{
					for (var y = -1; y <= 1; y++)
					{
						if (ActualMap.GetTileType(pos.X + x, pos.Y + y) == TileType.Tree)
							numberAdjacentTrees++;
					}
				}
				GrowthSteps[pos] = 2700 / (1 + numberAdjacentTrees);
				if (AssetOccupancyMap[pos.Y, pos.X] == null)
					GrowthCurrentStep[pos]++;

				if (GrowthCurrentStep[pos] >= GrowthSteps[pos])
				{
					switch (ActualMap.GetTileType(pos))
					{
						case TileType.Stump:
							ActualMap.ChangeTileType(pos, TileType.Seedling);
							GrowthCurrentStep[pos] = 0;
							break;

						case TileType.Seedling:
							ActualMap.ChangeTileType(pos, TileType.AdolescentTree);
							GrowthCurrentStep[pos] = 0;
							break;

						case TileType.AdolescentTree:
							ActualMap.ChangeTileType(pos, TileType.Tree);
							LumberAvailable[pos.Y, pos.X] = Players[0].Lumber;
							GrowthCurrentStep.Remove(pos);
							GrowthSteps.Remove(pos);
							break;
					}
				}
			}
		}

		/// <summary>
		/// Clears all game events.
		/// </summary>
		public void ClearGameEvents()
		{
			for (var index = 0; index < (int)PlayerColor.Max; index++)
				Players[index].GameEvents.Clear();
		}

		public static int RangeToDistanceSquared(int range)
		{
			range *= Position.TileWidth;
			range *= range;
			range += Position.TileWidth * Position.TileWidth;
			return range;
		}
	}

	public struct GameEvent
	{
		public EventType Type;
		public PlayerAsset Asset;
	}
}