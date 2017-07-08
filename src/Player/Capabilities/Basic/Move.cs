using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.Extensions;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Basic
{
	public class PlayerCapabilityMove : PlayerCapability
	{
		public PlayerCapabilityMove() : base("Move", TargetType.TerrainOrAsset) { }

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			return actor.Speed > 0;
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			return actor.Speed > 0;
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			if (actor.TilePosition == target.TilePosition)
				return false;

			AssetCommand newCommand = new AssetCommand
			{
				Action = AssetAction.Capability,
				Capability = AssetCapabilityType,
				Target = target,
				ActivatedCapability = new ActivatedCapability(actor, playerData, target)
			};
			actor.ClearCommands();
			actor.PushCommand(newCommand);
			return true;
		}

		protected class ActivatedCapability : ActivatedPlayerCapability
		{
			public ActivatedCapability(PlayerAsset actor, PlayerData playerData, PlayerAsset target) : base(actor, playerData, target) { }

			public override int PercentComplete(int max)
			{
				return 0;
			}

			public override bool IncrementStep()
			{
				PlayerData.AddGameEvent(Actor, EventType.Acknowledge);

				var assetCommand = new AssetCommand
				{
					Action = AssetAction.Walk,
					Target = Target
				};
				if (!Actor.Position.IsTileAligned)
					Actor.Direction = Actor.Position.TileOctant().Opposite();
				Actor.ClearCommands();
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
				Register(new PlayerCapabilityMove());
			}
		}
	}
}