using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.Extensions;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Basic
{
	public class PlayerCapabilityStandGround : PlayerCapability
	{
		public PlayerCapabilityStandGround() : base("StandGround", TargetType.None) { }

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			return true;
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			return true;
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			actor.ClearCommands();

			var newCommand = new AssetCommand
			{
				Action = AssetAction.Capability,
				Capability = AssetCapabilityType,
				Target = target,
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
				PlayerData.AddGameEvent(Actor, EventType.Acknowledge);

				Actor.ClearCommands();

				var assetCommand = new AssetCommand
				{
					Target = PlayerData.CreateMarker(Actor.Position, false),
					Action = AssetAction.StandGround
				};
				Actor.PushCommand(assetCommand);

				if (!Actor.Position.IsTileAligned)
				{
					assetCommand.Action = AssetAction.Walk;
					Actor.Direction = Actor.Position.TileOctant().Opposite();
					Actor.PushCommand(assetCommand);
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
				Register(new PlayerCapabilityStandGround());
			}
		}
	}
}