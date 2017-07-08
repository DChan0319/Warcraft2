using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Warcraft.App;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Train
{
	public class PlayerCapabilityTrainNormal : PlayerCapability
	{
		public readonly string UnitName;

		public PlayerCapabilityTrainNormal(string unitName) : base($"Build{unitName}", TargetType.None)
		{
			UnitName = unitName;
		}

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			PlayerAssetData assetType;
			if (!playerData.AssetDatas.TryGetValue(UnitName, out assetType))
				return false;

			if (assetType.GoldCost > playerData.Gold)
				return false;

			if (assetType.LumberCost > playerData.Lumber)
				return false;

			if (assetType.StoneCost > playerData.Stone)
				return false;

			if (assetType.FoodConsumption + playerData.FoodConsumption > playerData.FoodProduction)
				return false;

			return playerData.AssetRequirementsMet(UnitName);
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			return CanInitiate(actor, playerData);
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			PlayerAssetData assetType;
			if (!playerData.AssetDatas.TryGetValue(UnitName, out assetType))
				return false;

			var newAsset = playerData.CreateAsset(UnitName);
			newAsset.SetPosition(actor.Position);
			newAsset.Health = 1;
			newAsset.Training = true;

			var newCommand = new AssetCommand
			{
				Action = AssetAction.Capability,
				Capability = AssetCapabilityType,
				Target = newAsset,
				ActivatedCapability = new ActivatedCapability(actor, playerData, newAsset, assetType.GoldCost, assetType.LumberCost, assetType.StoneCost, assetType.BuildTime * PlayerAsset.UpdateFrequency)
			};
			actor.PushCommand(newCommand);
			actor.Step = 0;

			return false;
		}

		private class ActivatedCapability : ActivatedPlayerCapability
		{
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

			public ActivatedCapability(PlayerAsset actor, PlayerData playerData, PlayerAsset target, int gold, int lumber, int stone, int steps) : base(actor, playerData, target)
			{
				CurrentStep = 0;
				TotalSteps = steps;
				Gold = gold;
				Lumber = lumber;
				Stone = stone;

				playerData.Gold -= Gold;
				playerData.Lumber -= Lumber;
				playerData.Stone -= Stone;

				var assetCommand = new AssetCommand
				{
					Action = AssetAction.Construct,
					Target = Actor
				};
				target.PushCommand(assetCommand);
			}

			public override int PercentComplete(int max)
			{
				return CurrentStep * max / TotalSteps;
			}

			public override bool IncrementStep()
			{
				int addHealth = Target.Data.Health * (CurrentStep + 1) / TotalSteps - Target.Data.Health * CurrentStep / TotalSteps;
				Target.Health = MathHelper.Min(Target.Health + addHealth, Target.Data.Health);

				CurrentStep++;
				Actor.Step++;
				Target.Step++;

				// Finished training
				if (CurrentStep >= TotalSteps)
				{
					PlayerData.AddGameEvent(Target, EventType.Ready);

					Target.Training = false;
					Target.PopCommand();
					Actor.PopCommand();
					Target.SetTilePosition(PlayerData.PlayerMap.FindAssetPlacement(Target, Actor, new Position(PlayerData.PlayerMap.MapWidth - 1, PlayerData.PlayerMap.MapHeight - 1)));
					return true;
				}

				return false;
			}

			public override void Cancel()
			{
				PlayerData.Gold += Gold;
				PlayerData.Lumber += Lumber;
				PlayerData.Stone += Stone;
				PlayerData.DeleteAsset(Target);
				Actor.PopCommand();
			}
		}

		[PlayerCapabilityRegistrant, UsedImplicitly]
		public class Registrant
		{
			public Registrant()
			{
				Register(new PlayerCapabilityTrainNormal("Peasant"));
				Register(new PlayerCapabilityTrainNormal("Footman"));
				Register(new PlayerCapabilityTrainNormal("Archer"));
			}
		}
	}
}