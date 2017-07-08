using System.Windows.Forms.VisualStyles;
using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.Extensions;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Basic
{
	public class PlayerCapabilityPatrol : PlayerCapability
	{
		public PlayerCapabilityPatrol() : base("Patrol", TargetType.Terrain) { }

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
			if (actor.TilePosition != target.TilePosition)
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

			return false;
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

				Actor.ClearCommands();

				var patrolCommand = new AssetCommand
				{
					Action = AssetAction.Capability,
					Capability = AssetCapabilityType.Patrol,
					Target = PlayerData.CreateMarker(Actor.Position, false)
				};
				patrolCommand.ActivatedCapability = new ActivatedCapability(Actor, PlayerData, patrolCommand.Target);
				Actor.PushCommand(patrolCommand);

				var walkCommand = new AssetCommand
				{
					Action = AssetAction.Walk,
					Target = Target
				};

				if (!Actor.Position.IsTileAligned)
					Actor.Direction = Actor.Position.TileOctant().Opposite();

				Actor.PushCommand(walkCommand);

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
				Register(new PlayerCapabilityPatrol());
			}
		}
	}
}