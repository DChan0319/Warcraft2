using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Basic
{
	public class PlayerCapabilityUnshelter : PlayerCapability
	{
		public PlayerCapabilityUnshelter() : base("Unshelter", TargetType.None) { }

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			return actor.HasCapability(AssetCapabilityType.Unshelter) && actor.ShelteredPeasants.Count != 0;
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			if (!actor.HasCapability(AssetCapabilityType.Unshelter)) return false;
			if (actor.HasActiveCapability(AssetCapabilityType.BuildPeasant)) return false;
			if (actor.GetAction() == AssetAction.Construct) return false;
			if (actor.ShelteredPeasants.Count <= 0) return false;

			return true;
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			var newCommand = new AssetCommand
			{
				Action = AssetAction.Capability,
				Capability = AssetCapabilityType,
				Target = actor,
				ActivatedCapability = new ActivatedCapability(actor, playerData, target)
			};
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
				Actor.RemoveShelteredPeasants();

				// Modify stats and stop standing ground
				Actor.Data.Range = 0;
				Actor.ClearCommands();

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
				Register(new PlayerCapabilityUnshelter());
			}
		}
	}
}