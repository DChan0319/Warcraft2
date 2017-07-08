using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Upgrade
{
	public class PlayerCapabilityUnitUpgrade : PlayerCapability
	{
		private string UpgradeName;

		public PlayerCapabilityUnitUpgrade(string upgradeName) : base(upgradeName, TargetType.None)
		{
			UpgradeName = upgradeName;
		}

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			var upgrade = PlayerUpgrade.FindUpgradeByName(UpgradeName);
			if (upgrade != null)
			{
				if (playerData.Gold < upgrade.GoldCost) return false;
				if (playerData.Lumber < upgrade.LumberCost) return false;
				if (playerData.Stone < upgrade.StoneCost) return false;
				// Linux: AssetRequirementMet is commented out here.
			}

			return true;
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			return CanInitiate(actor, playerData);
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			var upgrade = PlayerUpgrade.FindUpgradeByName(UpgradeName);
			if (upgrade != null)
			{
				actor.ClearCommands();

				var newCommand = new AssetCommand();
				newCommand.Action = AssetAction.Capability;
				newCommand.Capability = AssetCapabilityType;
				newCommand.Target = target;
				newCommand.ActivatedCapability = new ActivatedCapability(actor, playerData, target, actor.Data, UpgradeName, upgrade.GoldCost, upgrade.LumberCost, upgrade.StoneCost, upgrade.ResearchTime * PlayerAsset.UpdateFrequency);
				actor.PushCommand(newCommand);

				return true;
			}

			return false;
		}

		private class ActivatedCapability : ActivatedPlayerCapability
		{
			public PlayerAssetData UpgradingType;
			public string UpgradeName;
			public int CurrentStep;
			public int TotalSteps;
			public int Gold;
			public int Lumber;
			public int Stone;

			public ActivatedCapability(PlayerAsset actor, PlayerData playerData, PlayerAsset target, PlayerAssetData upgradingType, string upgradeName, int gold, int lumber, int stone, int steps)
				: base(actor, playerData, target)
			{
				UpgradingType = upgradingType;
				UpgradeName = upgradeName;
				CurrentStep = 0;
				TotalSteps = steps;
				Gold = gold;
				Lumber = lumber;
				Stone = stone;

				playerData.Gold -= Gold;
				playerData.Lumber -= Lumber;
				playerData.Stone -= Stone;
				upgradingType.RemoveCapability(NameToType(upgradeName));
			}

			public override int PercentComplete(int max)
			{
				return CurrentStep * max / TotalSteps;
			}

			public override bool IncrementStep()
			{
				CurrentStep++;
				Actor.Step++;

				if (CurrentStep >= TotalSteps)
				{
					PlayerData.AddUpgrade(UpgradeName);
					Actor.PopCommand();

					if (UpgradeName.EndsWith("2"))
					{
						UpgradingType.AddCapability(NameToType(UpgradeName.Substring(0, UpgradeName.Length - 1) + '3'));
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
				UpgradingType.AddCapability(NameToType(UpgradeName));
				Actor.PopCommand();
			}
		}

		[PlayerCapabilityRegistrant, UsedImplicitly]
		public class Registrant
		{
			public Registrant()
			{
				Register(new PlayerCapabilityUnitUpgrade("WeaponUpgrade2"));
				Register(new PlayerCapabilityUnitUpgrade("WeaponUpgrade3"));
				Register(new PlayerCapabilityUnitUpgrade("ArmorUpgrade2"));
				Register(new PlayerCapabilityUnitUpgrade("ArmorUpgrade3"));
				Register(new PlayerCapabilityUnitUpgrade("ArrowUpgrade2"));
				Register(new PlayerCapabilityUnitUpgrade("ArrowUpgrade3"));
				Register(new PlayerCapabilityUnitUpgrade("Longbow"));
				Register(new PlayerCapabilityUnitUpgrade("RangerScouting"));
				Register(new PlayerCapabilityUnitUpgrade("Marksmanship"));
			}
		}
	}
}