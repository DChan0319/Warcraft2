using JetBrains.Annotations;
using Newtonsoft.Json;
using Warcraft.App;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Upgrade
{
	public class PlayerCapabilityBuildRanger : PlayerCapability
	{
		private string UnitName;

		public PlayerCapabilityBuildRanger(string unitName) : base($"Build{unitName}", TargetType.None)
		{
			UnitName = unitName;
		}

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			if (actor.Data.Type == AssetType.LumberMill)
			{
				var upgrade = PlayerUpgrade.FindUpgradeByName($"Build{UnitName}");
				if (upgrade != null)
				{
					if (playerData.Gold < upgrade.GoldCost) return false;
					if (playerData.Lumber < upgrade.LumberCost) return false;
					if (playerData.Stone < upgrade.StoneCost) return false;
					if (!playerData.AssetRequirementsMet(UnitName)) return false;
				}
			}
			else if (actor.Data.Type == AssetType.Barracks)
			{
				PlayerAssetData assetData;
				if (playerData.AssetDatas.TryGetValue(UnitName, out assetData))
				{
					if (playerData.Gold < assetData.GoldCost) return false;
					if (playerData.Lumber < assetData.LumberCost) return false;
					if (playerData.Stone < assetData.StoneCost) return false;
					if (playerData.FoodConsumption + assetData.FoodConsumption > playerData.FoodProduction) return false;
				}
			}

			return true;
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			return CanInitiate(actor, playerData);
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			if (actor.Data.Type == AssetType.LumberMill)
			{
				var upgrade = PlayerUpgrade.FindUpgradeByName($"Build{UnitName}");
				if (upgrade != null)
				{
					actor.ClearCommands();

					var newCommand = new AssetCommand
					{
						Action = AssetAction.Capability,
						Capability = AssetCapabilityType,
						Target = target,
						ActivatedCapability = new ActivatedCapability(actor, playerData, target, actor.Data, UnitName, upgrade.GoldCost, upgrade.LumberCost, upgrade.StoneCost, upgrade.ResearchTime * PlayerAsset.UpdateFrequency)
					};
					actor.PushCommand(newCommand);

					return true;
				}
			}
			else if (actor.Data.Type == AssetType.Barracks)
			{
				PlayerAssetData assetData;
				if (playerData.AssetDatas.TryGetValue(UnitName, out assetData))
				{
					var newAsset = playerData.CreateAsset(UnitName);
					newAsset.SetPosition(actor.Position);
					newAsset.Health = 1;
					newAsset.Training = true;

					var newCommand = new AssetCommand
					{
						Action = AssetAction.Capability,
						Capability = AssetCapabilityType,
						Target = newAsset,
						ActivatedCapability = new ActivatedCapability(actor, playerData, newAsset, actor.Data, UnitName, assetData.GoldCost, assetData.LumberCost, assetData.StoneCost, assetData.BuildTime * PlayerAsset.UpdateFrequency)
					};
					actor.PushCommand(newCommand);

					return true;
				}
			}

			return false;
		}

		private class ActivatedCapability : ActivatedPlayerCapability
		{
			public PlayerAssetData UpgradingType;
			public string UnitName;
			public int CurrentStep;
			public int TotalSteps;
			public int Gold;
			public int Lumber;
			public int Stone;

			/// <summary>
			/// This is required for the game to be able
			/// to save/load a unit in training properly.
			/// </summary>
			[JsonConstructor, UsedImplicitly]
			public ActivatedCapability() { }

			public ActivatedCapability(PlayerAsset actor, PlayerData playerData, PlayerAsset target, PlayerAssetData upgradingType, string unitName, int gold, int lumber, int stone, int steps)
				: base(actor, playerData, target)
			{
				UnitName = unitName;
				CurrentStep = 0;
				TotalSteps = steps;
				Gold = gold;
				Lumber = lumber;
				Stone = stone;

				PlayerData.Gold -= Gold;
				playerData.Lumber -= Lumber;
				playerData.Stone -= Stone;

				if (Actor.Data.Type == AssetType.LumberMill)
				{
					UpgradingType = upgradingType;
					UpgradingType.RemoveCapability(NameToType($"Build{UnitName}"));
				}
				else if (Actor.Data.Type == AssetType.Barracks)
				{
					var assetCommand = new AssetCommand();
					assetCommand.Action = AssetAction.Construct;
					assetCommand.Target = Actor;
					Target.PushCommand(assetCommand);
				}
			}

			public override int PercentComplete(int max)
			{
				return CurrentStep * max / TotalSteps;
			}

			public override bool IncrementStep()
			{
				if (Actor.Data.Type == AssetType.Barracks)
				{
					var addHealth = Target.Data.Health * (CurrentStep + 1) / TotalSteps - Target.Data.Health * CurrentStep / TotalSteps;
					Target.Health += addHealth;
				}

				CurrentStep++;
				Actor.Step++;

				if (CurrentStep >= TotalSteps)
				{
					if (Actor.Data.Type == AssetType.LumberMill)
					{
						var barracks = PlayerData.AssetDatas["Barracks"];
						var ranger = PlayerData.AssetDatas["Ranger"];
						var lumberMill = PlayerData.AssetDatas["LumberMill"];

						barracks.AddCapability(AssetCapabilityType.BuildRanger);
						barracks.AddCapability(AssetCapabilityType.BuildArcher);
						lumberMill.AddCapability(AssetCapabilityType.Longbow);
						lumberMill.AddCapability(AssetCapabilityType.RangerScouting);
						lumberMill.AddCapability(AssetCapabilityType.Marksmanship);

						// Upgrade all archers
						foreach (var asset in PlayerData.Assets)
						{
							if (asset.Data.Type == AssetType.Archer)
							{
								var addHealth = ranger.Health - asset.Data.Health;
								asset.Data = ranger;
								asset.Health += addHealth;
							}
						}

						PlayerData.AddGameEvent(Actor, EventType.WorkComplete);
					}
					else if (Actor.Data.Type == AssetType.Barracks)
					{
						Target.Training = false;
						Target.PopCommand();
						Target.SetTilePosition(PlayerData.PlayerMap.FindAssetPlacement(Target, Actor, new Position(PlayerData.PlayerMap.MapWidth - 1, PlayerData.PlayerMap.MapHeight - 1)));

						PlayerData.AddGameEvent(Target, EventType.Ready);
					}

					Actor.PopCommand();
					return true;
				}

				return false;
			}

			public override void Cancel()
			{
				PlayerData.Gold += Gold;
				PlayerData.Lumber += Lumber;
				PlayerData.Stone += Stone;

				if (Actor.Data.Type == AssetType.LumberMill)
					UpgradingType.AddCapability(NameToType($"Build{UnitName}"));
				else if (Actor.Data.Type == AssetType.Barracks)
					PlayerData.DeleteAsset(Target);

				Actor.PopCommand();
			}
		}

		[PlayerCapabilityRegistrant, UsedImplicitly]
		public class Registrant
		{
			public Registrant()
			{
				Register(new PlayerCapabilityBuildRanger("Ranger"));
			}
		}
	}
}