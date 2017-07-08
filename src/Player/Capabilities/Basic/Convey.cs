using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Basic
{
	public class PlayerCapabilityConvey : PlayerCapability
	{
		public PlayerCapabilityConvey() : base("Convey", TargetType.Asset)
		{
		}

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			return actor.Speed != 0 && (actor.Gold != 0 || actor.Lumber != 0 || actor.Stone != 0);
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			if (actor.Speed != 0 && (actor.Gold != 0 || actor.Lumber != 0 || actor.Stone != 0))
			{
				if (target.GetAction() == AssetAction.Construct) return false;
				if (target.Data.Type == AssetType.TownHall || target.Data.Type == AssetType.Keep || target.Data.Type == AssetType.Castle) return true;
				if (actor.Lumber != 0 && target.Data.Type == AssetType.LumberMill) return true;
				if (actor.Stone != 0 && target.Data.Type == AssetType.Blacksmith) return true;
			}

			return false;
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			var newCommand = new AssetCommand();
			newCommand.Action = AssetAction.Capability;
			newCommand.Capability = AssetCapabilityType;
			newCommand.Target = target;
			newCommand.ActivatedCapability = new ActivatedCapability(actor, playerData, target);

			actor.ClearCommands();
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
				PlayerData.AddGameEvent(Actor, EventType.Acknowledge);

				Actor.PopCommand();

				var assetCommand = new AssetCommand();

				if (Actor.Gold != 0) assetCommand.Action = AssetAction.ConveyGold;
				else if (Actor.Lumber != 0) assetCommand.Action = AssetAction.ConveyLumber;
				else if (Actor.Stone != 0) assetCommand.Action = AssetAction.ConveyStone;

				assetCommand.Target = Target;
				Actor.PushCommand(assetCommand);
				assetCommand.Action = AssetAction.Walk;
				Actor.PushCommand(assetCommand);
				Actor.Step = 0;

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
				Register(new PlayerCapabilityConvey());
			}
		}
	}
}