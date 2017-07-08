using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.Extensions;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Basic
{
	public class PlayerCapabilityRepair : PlayerCapability
	{
		public PlayerCapabilityRepair() : base("Repair", TargetType.Asset) { }

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			return actor.Speed > 0 && playerData.Gold != 0 && playerData.Lumber != 0 && playerData.Stone != 0;
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			// All peasants, regardless of color, can repair player-built walls.
			if (target.Data.Type != AssetType.Wall || actor.Data.Type != AssetType.Peasant)
			{
				if (actor.Data.Color != target.Data.Color || target.Speed != 0)
					return false;
			}

			if (target.Health >= target.Data.Health)
				return false;

			return CanInitiate(actor, playerData);
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			if (actor.TilePosition != target.TilePosition)
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

			return false;
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

				var assetCommand = new AssetCommand();
				assetCommand.Action = AssetAction.Repair;
				assetCommand.Target = Target;
				Actor.ClearCommands();
				Actor.PushCommand(assetCommand);

				assetCommand.Action = AssetAction.Walk;
				if (!Actor.Position.IsTileAligned)
					Actor.Direction = Actor.Position.TileOctant().Opposite();
				Actor.PushCommand(assetCommand);

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
				Register(new PlayerCapabilityRepair());
			}
		}
	}
}