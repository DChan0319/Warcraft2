using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Basic
{
	public class PlayerCapabilityShelter : PlayerCapability
	{
		public PlayerCapabilityShelter() : base("Shelter", TargetType.TerrainOrAsset) { }

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			return actor.HasCapability(AssetCapabilityType.Shelter);
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			if (actor.Data.Color != target.Data.Color) return false;
			if (actor.Data.Type != AssetType.Peasant) return false;
			if (target.HasActiveCapability(AssetCapabilityType.BuildPeasant)) return false;
			if (target.GetAction() == AssetAction.Construct) return false;
			if (target.ShelteredPeasants.Count >= target.ShelterCapacity) return false;

			return true;
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			var newCommand = new AssetCommand
			{
				Action = AssetAction.Shelter,
				Capability = AssetCapabilityType,
				Target = target,
				ActivatedCapability = new ActivatedCapability(actor, playerData, target)
			};
			actor.PushCommand(newCommand);

			newCommand.Action = AssetAction.Capability;
			actor.PushCommand(newCommand);

			newCommand.Action = AssetAction.Walk;
			actor.PushCommand(newCommand);

			return true;
		}

		private class ActivatedCapability : ActivatedPlayerCapability
		{
			public ActivatedCapability(PlayerAsset actor, PlayerData playerData, PlayerAsset target) : base(actor, playerData, target) { }

			public override int PercentComplete(int max)
			{
				return 0;
			}

			public override bool IncrementStep()
			{
				if (Target.ShelteredPeasants.Count >= Target.ShelterCapacity)
				{
					Actor.ClearCommands();
					return false;
				}

				Actor.PopCommand();
				Actor.SetTilePosition(Target.TilePosition);
				Data.GameModel.SelectedAssets.Remove(Actor);

				Target.ShelteredPeasants.Add(Actor);

				// Modify stats and begin standing ground
				if (Target.ShelteredPeasants.Count == 1)
				{
					Target.Data.AttackSteps = 1;
					Target.Data.ReloadSteps = 120 / Target.ShelteredPeasants.Count;
					Target.Data.BasicDamage = 2;
					Target.Data.PiercingDamage = 6;
					Target.Data.Range = 6;
					Target.PushCommand(new AssetCommand { Action = AssetAction.StandGround });
				}

				return true;
			}

			public override void Cancel()
			{
				Actor.PopCommand();
			}
		}

		[PlayerCapabilityRegistrant, UsedImplicitly]
		public class Registrant
		{
			public Registrant()
			{
				Register(new PlayerCapabilityShelter());
			}
		}
	}
}