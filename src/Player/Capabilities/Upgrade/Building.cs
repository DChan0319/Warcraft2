using JetBrains.Annotations;
using Newtonsoft.Json;
using Warcraft.App;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Upgrade
{
	public class PlayerCapabilityBuildingUpgrade : PlayerCapability
	{
		private string BuildingName;

		public PlayerCapabilityBuildingUpgrade(string buildingName) : base($"Build{buildingName}", TargetType.None)
		{
			BuildingName = buildingName;
		}

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			PlayerAssetData playerAssetData;
			if (!playerData.AssetDatas.TryGetValue(BuildingName, out playerAssetData))
				return false;

			if (playerData.Gold < playerAssetData.GoldCost) return false;
			if (playerData.Lumber < playerAssetData.LumberCost) return false;
			if (playerData.Stone < playerAssetData.StoneCost) return false;
			if (!playerData.AssetRequirementsMet(BuildingName)) return false;

			return true;
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			return CanInitiate(actor, playerData);
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			PlayerAssetData playerAssetData;
			if (!playerData.AssetDatas.TryGetValue(BuildingName, out playerAssetData))
				return false;

			// The sheltered peasants must be removed because
			// the data is going to be replaced when the upgrade begins.
			actor.RemoveShelteredPeasants();

			actor.ClearCommands();
			var newCommand = new AssetCommand();
			newCommand.Action = AssetAction.Capability;
			newCommand.Capability = AssetCapabilityType;
			newCommand.Target = target;
			newCommand.ActivatedCapability = new ActivatedCapability(actor, playerData, target, actor.Data, playerAssetData, playerAssetData.GoldCost, playerAssetData.LumberCost, playerAssetData.StoneCost, playerAssetData.BuildTime * PlayerAsset.UpdateFrequency);
			actor.PushCommand(newCommand);

			return true;
		}

		private class ActivatedCapability : ActivatedPlayerCapability
		{
			public PlayerAssetData OriginalData;
			public PlayerAssetData UpgradeData;
			public int CurrentStep;
			public int TotalSteps;
			public int Gold;
			public int Lumber;
			public int Stone;

			/// <summary>
			/// This is required for the game to be able
			/// to save/load a building upgrade properly.
			/// </summary>
			[JsonConstructor, UsedImplicitly]
			public ActivatedCapability() { }

			public ActivatedCapability(PlayerAsset actor, PlayerData playerData, PlayerAsset target, PlayerAssetData originalData, PlayerAssetData upgradeData, int gold, int lumber, int stone, int steps)
				: base(actor, playerData, target)
			{
				OriginalData = originalData;
				UpgradeData = upgradeData;
				CurrentStep = 0;
				TotalSteps = steps;
				Gold = gold;
				Lumber = lumber;
				Stone = stone;

				playerData.Gold -= Gold;
				playerData.Lumber -= Lumber;
				playerData.Stone -= Stone;
			}

			public override int PercentComplete(int max)
			{
				return CurrentStep * max / TotalSteps;
			}

			public override bool IncrementStep()
			{
				var addHealth = (UpgradeData.Health - OriginalData.Health) * (CurrentStep + 1) / TotalSteps - (UpgradeData.Health - OriginalData.Health) * CurrentStep / TotalSteps;
				Actor.Health += addHealth;

				if (CurrentStep == 0)
				{
					var assetCommand = Actor.CurrentCommand();
					assetCommand.Action = AssetAction.Construct;
					Actor.PopCommand();
					Actor.PushCommand(assetCommand);
					Actor.Data = UpgradeData;
					Actor.Step = 0;
				}

				CurrentStep++;
				Actor.Step++;

				if (CurrentStep >= TotalSteps)
				{
					PlayerData.AddGameEvent(Actor, EventType.WorkComplete);

					Actor.PopCommand();
					if (Actor.Data.Range != 0)
					{
						var command = new AssetCommand();
						command.Action = AssetAction.StandGround;
						Actor.PushCommand(command);
					}

					return true;
				}

				return false;
			}

			public override void Cancel()
			{
				PlayerData.Gold += Gold;
				PlayerData.Lumber += Lumber;
				PlayerData.Stone += Stone;
				Actor.Data = OriginalData;
				// Linux: The linux version does not clamp the health, so
				//        the health stays over max. This actually causes the
				//        health colors to go out of bounds.
				// Set the health again so it gets clamped.
				Actor.Health = Actor.Health;
				Actor.PopCommand();
			}
		}

		[PlayerCapabilityRegistrant, UsedImplicitly]
		public class Registrant
		{
			public Registrant()
			{
				Register(new PlayerCapabilityBuildingUpgrade("Keep"));
				Register(new PlayerCapabilityBuildingUpgrade("Castle"));
				Register(new PlayerCapabilityBuildingUpgrade("GuardTower"));
				Register(new PlayerCapabilityBuildingUpgrade("CannonTower"));
			}
		}
	}
}