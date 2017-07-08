using System.Collections.Generic;
using System.Linq;
using Warcraft.App;
using Warcraft.GameModel;
using Warcraft.Player;
using Warcraft.Player.Capabilities;

namespace Warcraft.Ai
{
	public class AiPlayer
	{
		public PlayerData PlayerData;
		protected int Cycle;
		protected int DownSample;

		public AiPlayer(PlayerData playerData, int downSample)
		{
			PlayerData = playerData;
			Cycle = 0;
			DownSample = downSample;
		}

		public void CalculateCommand(PlayerCommandRequest command)
		{
			command.Action = AssetCapabilityType.None;
			command.Actors.Clear();
			command.TargetColor = PlayerColor.None;
			command.TargetType = AssetType.None;

			if (Cycle % DownSample == 0)
			{
				if (PlayerData.MapAssetCount(a => a.Data.Type == AssetType.GoldMine) == 0)
				{
					SearchMap(command);
				}
				else if (PlayerData.OwnedAssetCount(a => a.Data.Type == AssetType.TownHall || a.Data.Type == AssetType.Keep || a.Data.Type == AssetType.Castle) == 0)
				{
					BuildTownHall(command);
				}
				else if (PlayerData.OwnedAssetCount(a => a.Data.Type == AssetType.Peasant) < 5)
				{
					ActivatePeasants(command, true);
				}
				else if (PlayerData.VisibilityMap.SeenPercent(100) < 12)
				{
					SearchMap(command);
				}
				else
				{
					var completedAction = false;
					var footmanCount = PlayerData.OwnedAssetCount(a => a.Data.Type == AssetType.Footman || a.Data.Type == AssetType.Knight);
					var archerCount = PlayerData.OwnedAssetCount(a => a.Data.Type == AssetType.Archer || a.Data.Type == AssetType.Ranger);

					// Build farms
					if (!completedAction && PlayerData.FoodConsumption >= PlayerData.FoodProduction)
						completedAction = BuildBuilding(command, AssetType.Farm, AssetType.Farm);

					// Activate Peasants
					if (!completedAction)
						completedAction = ActivatePeasants(command, false);

					// Build barracks
					if (!completedAction && PlayerData.OwnedAssetCount(a => a.Data.Type == AssetType.Barracks) == 0)
						completedAction = BuildBuilding(command, AssetType.Barracks, AssetType.Farm);

					// Train footmen/knights
					if (!completedAction && footmanCount < 5)
						completedAction = TrainFootman(command);

					// Builder lumber mills
					if (!completedAction && PlayerData.OwnedAssetCount(a => a.Data.Type == AssetType.LumberMill) == 0)
						completedAction = BuildBuilding(command, AssetType.LumberMill, AssetType.Barracks);

					// Train archers/rangers
					if (!completedAction && archerCount < 5)
						completedAction = TrainArcher(command);

					// Find enemies
					if (!completedAction && footmanCount != 0)
						completedAction = FindEnemies(command);

					// Activate fighters
					if (!completedAction)
						completedAction = ActivateFighters(command);

					// Attack enemies
					if (!completedAction && footmanCount >= 5 && archerCount >= 5)
						completedAction = AttackEnemies(command);
				}
			}

			Cycle++;
		}

		private bool SearchMap(PlayerCommandRequest command)
		{
			var idleAssets = GetIdleAssets();

			var moveableAsset = idleAssets.FirstOrDefault(a => a.Speed != 0);
			if (moveableAsset == null) return false;

			var unknownPosition = PlayerData.PlayerMap.FindNearestReachableTileType(moveableAsset.TilePosition, TileType.None);
			if (unknownPosition.X < 0) return false;

			command.Action = AssetCapabilityType.Move;
			command.Actors.Add(moveableAsset);
			command.TargetLocation.SetFromTile(unknownPosition);
			return true;
		}

		private bool FindEnemies(PlayerCommandRequest command)
		{
			var townHall = PlayerData.Assets.First(a => a.HasCapability(AssetCapabilityType.BuildPeasant));
			return PlayerData.FindNearestEnemy(townHall.Position, -1) == null && SearchMap(command);
		}

		private bool AttackEnemies(PlayerCommandRequest command)
		{
			var averageLocation = new Position(0, 0);

			foreach (var asset in PlayerData.Assets)
			{
				if (asset.Data.Type != AssetType.Footman && asset.Data.Type != AssetType.Knight && asset.Data.Type != AssetType.Archer && asset.Data.Type != AssetType.Ranger) continue;
				if (asset.Commands.Any(c => c.Action == AssetAction.Attack)) continue;

				command.Actors.Add(asset);
				averageLocation.X += asset.Position.X;
				averageLocation.Y += asset.Position.Y;
			}

			if (command.Actors.Count == 0) return false;

			averageLocation.X /= command.Actors.Count;
			averageLocation.Y /= command.Actors.Count;

			var targetEnemy = PlayerData.FindNearestEnemy(averageLocation, -1);
			if (targetEnemy == null)
			{
				command.Actors.Clear();
				return SearchMap(command);
			}

			command.Action = AssetCapabilityType.Attack;
			command.TargetLocation = targetEnemy.Position;
			command.TargetColor = targetEnemy.Data.Color;
			command.TargetType = targetEnemy.Data.Type;
			return true;
		}

		private bool BuildTownHall(PlayerCommandRequest command)
		{
			var idleAssets = GetIdleAssets();

			var builderAsset = idleAssets.FirstOrDefault(a => a.HasCapability(AssetCapabilityType.BuildTownHall));
			if (builderAsset == null) return false;

			var goldMineAsset = PlayerData.FindNearestAsset(builderAsset.Position, AssetType.GoldMine);

			var placement = PlayerData.FindBestAssetPlacement(goldMineAsset.TilePosition, builderAsset, AssetType.TownHall, 1);
			if (placement.X < 0)
				return SearchMap(command);

			command.Action = AssetCapabilityType.BuildTownHall;
			command.Actors.Add(builderAsset);
			command.TargetLocation.SetFromTile(placement);
			return true;
		}

		private bool BuildBuilding(PlayerCommandRequest command, AssetType buildingType, AssetType nearType)
		{
			PlayerAsset builderAsset = null;
			PlayerAsset townHallAsset = null;
			PlayerAsset nearAsset = null;
			AssetCapabilityType buildAction;
			var assetIsIdle = false;

			switch (buildingType)
			{
				case AssetType.Barracks: buildAction = AssetCapabilityType.BuildBarracks; break;
				case AssetType.LumberMill: buildAction = AssetCapabilityType.BuildLumberMill; break;
				case AssetType.Blacksmith: buildAction = AssetCapabilityType.BuildBlacksmith; break;
				default: buildAction = AssetCapabilityType.BuildFarm; break;
			}

			foreach (var asset in PlayerData.Assets)
			{
				if (asset.HasCapability(buildAction) && asset.IsInterruptible)
				{
					if (builderAsset == null || !assetIsIdle && asset.GetAction() == AssetAction.None)
					{
						builderAsset = asset;
						assetIsIdle = asset.GetAction() == AssetAction.None;
					}
				}

				if (asset.HasCapability(AssetCapabilityType.BuildPeasant))
					townHallAsset = asset;

				if (asset.HasActiveCapability(buildAction))
					return false;

				if (nearType == asset.Data.Type && asset.GetAction() != AssetAction.Construct)
					nearAsset = asset;

				if (buildingType == asset.Data.Type && asset.GetAction() == AssetAction.Construct)
					return false;
			}

			if (nearAsset == null && buildingType != nearType)
				return false;

			if (builderAsset == null)
				return false;

			var playerCapability = PlayerCapability.FindCapability(buildAction);
			var mapCenter = new Position(PlayerData.PlayerMap.MapWidth / 2, PlayerData.PlayerMap.MapHeight / 2);
			var sourcePosition = townHallAsset.TilePosition;

			if (nearAsset != null)
				sourcePosition = nearAsset.TilePosition;

			if (mapCenter.X < sourcePosition.X)
				sourcePosition.X -= townHallAsset.Data.Size / 2;
			else if (mapCenter.X > sourcePosition.X)
				sourcePosition.X += townHallAsset.Data.Size / 2;

			if (mapCenter.Y < sourcePosition.Y)
				sourcePosition.Y -= townHallAsset.Data.Size / 2;
			else if (mapCenter.Y > sourcePosition.Y)
				sourcePosition.Y += townHallAsset.Data.Size / 2;

			var placement = PlayerData.FindBestAssetPlacement(sourcePosition, builderAsset, buildingType, 1);
			if (placement.X < 0)
				return SearchMap(command);

			if (playerCapability == null || !playerCapability.CanInitiate(builderAsset, PlayerData))
				return false;

			command.Action = buildAction;
			command.Actors.Add(builderAsset);
			command.TargetLocation.SetFromTile(placement);
			return true;
		}

		private bool ActivatePeasants(PlayerCommandRequest command, bool trainMore)
		{
			PlayerAsset miningAsset = null;
			PlayerAsset interruptableAsset = null;
			PlayerAsset townHallAsset = null;

			var goldMiners = 0;
			var lumberHarvesters = 0;
			var switchToGold = false;
			var switchToLumber = false;

			foreach (var asset in PlayerData.Assets)
			{
				if (asset.HasCapability(AssetCapabilityType.Mine))
				{
					if (miningAsset == null && asset.GetAction() == AssetAction.None)
						miningAsset = asset;

					if (asset.Commands.Any(c => c.Action == AssetAction.MineGold))
					{
						goldMiners++;
						if (asset.IsInterruptible && asset.GetAction() != AssetAction.None)
							interruptableAsset = asset;
					}
					else if (asset.Commands.Any(c => c.Action == AssetAction.HarvestLumber))
					{
						lumberHarvesters++;
						if (asset.IsInterruptible && asset.GetAction() != AssetAction.None)
							interruptableAsset = asset;
					}
				}

				if (asset.HasCapability(AssetCapabilityType.BuildPeasant) && asset.GetAction() == AssetAction.None)
					townHallAsset = asset;
			}

			if (goldMiners >= 2 && lumberHarvesters == 0)
				switchToLumber = true;
			else if (lumberHarvesters >= 2 && goldMiners == 0)
				switchToGold = true;

			if (miningAsset != null || interruptableAsset != null && (switchToLumber || switchToGold))
			{
				if (miningAsset != null && (miningAsset.Lumber != 0 || miningAsset.Gold != 0))
				{
					command.Action = AssetCapabilityType.Convey;
					command.TargetColor = townHallAsset.Data.Color;
					command.Actors.Add(miningAsset);
					command.TargetType = townHallAsset.Data.Type;
					command.TargetLocation = townHallAsset.Position;
				}
				else
				{
					if (miningAsset == null)
						miningAsset = interruptableAsset;

					var goldMineAsset = PlayerData.FindNearestAsset(miningAsset.Position, AssetType.GoldMine);
					if (goldMiners != 0 && (PlayerData.Gold > PlayerData.Lumber * 3 || switchToLumber))
					{
						var lumberLocation = PlayerData.PlayerMap.FindNearestReachableTileType(miningAsset.TilePosition, TileType.Tree);
						if (lumberLocation.X < 0)
							return SearchMap(command);

						command.Action = AssetCapabilityType.Mine;
						command.Actors.Add(miningAsset);
						command.TargetLocation.SetFromTile(lumberLocation);
					}
					else
					{
						command.Action = AssetCapabilityType.Mine;
						command.Actors.Add(miningAsset);
						command.TargetType = AssetType.GoldMine;
						command.TargetLocation = goldMineAsset.Position;
					}
				}

				return true;
			}

			if (townHallAsset == null || !trainMore) return false;

			var playerCapability = PlayerCapability.FindCapability(AssetCapabilityType.BuildPeasant);
			if (playerCapability?.CanApply(townHallAsset, PlayerData, townHallAsset) != true) return false;

			command.Action = AssetCapabilityType.BuildPeasant;
			command.Actors.Add(townHallAsset);
			command.TargetLocation = townHallAsset.Position;
			return true;
		}

		private bool ActivateFighters(PlayerCommandRequest command)
		{
			var idleAssets = GetIdleAssets();
			foreach (var asset in idleAssets)
			{
				if (asset.Data.Speed == 0 || asset.Data.Type == AssetType.Peasant ||
					asset.Commands.All(c => c.Action != AssetAction.StandGround) || asset.HasActiveCapability(AssetCapabilityType.StandGround))
					continue;

				command.Actors.Add(asset);
			}

			if (command.Actors.Count == 0)
				return false;

			command.Action = AssetCapabilityType.StandGround;
			return true;
		}

		private bool TrainFootman(PlayerCommandRequest command)
		{
			var idleAssets = GetIdleAssets();
			PlayerAsset trainingAsset = null;
			var buildType = AssetCapabilityType.BuildFootman;

			foreach (var asset in idleAssets)
			{
				if (asset.HasCapability(AssetCapabilityType.BuildFootman))
				{
					trainingAsset = asset;
					buildType = AssetCapabilityType.BuildFootman;
					break;
				}

				if (asset.HasCapability(AssetCapabilityType.BuildKnight))
				{
					trainingAsset = asset;
					buildType = AssetCapabilityType.BuildKnight;
					break;
				}
			}

			if (trainingAsset == null) return false;

			var playerCapability = PlayerCapability.FindCapability(buildType);
			if (playerCapability?.CanApply(trainingAsset, PlayerData, trainingAsset) != true) return false;

			command.Action = buildType;
			command.Actors.Add(trainingAsset);
			command.TargetLocation = trainingAsset.Position;
			return true;
		}

		private bool TrainArcher(PlayerCommandRequest command)
		{
			var idleAssets = GetIdleAssets();
			PlayerAsset trainingAsset = null;
			var buildType = AssetCapabilityType.BuildArcher;

			foreach (var asset in idleAssets)
			{
				if (asset.HasCapability(AssetCapabilityType.BuildArcher))
				{
					trainingAsset = asset;
					buildType = AssetCapabilityType.BuildArcher;
					break;
				}

				if (asset.HasCapability(AssetCapabilityType.BuildRanger))
				{
					trainingAsset = asset;
					buildType = AssetCapabilityType.BuildRanger;
					break;
				}
			}

			if (trainingAsset == null) return false;

			var playerCapability = PlayerCapability.FindCapability(buildType);
			if (playerCapability?.CanApply(trainingAsset, PlayerData, trainingAsset) != true) return false;

			command.Action = buildType;
			command.Actors.Add(trainingAsset);
			command.TargetLocation = trainingAsset.Position;
			return true;
		}

		private IEnumerable<PlayerAsset> GetIdleAssets()
		{
			return PlayerData.Assets.Where(a => a.GetAction() == AssetAction.None && a.Data.Type != AssetType.None);
		}
	}
}