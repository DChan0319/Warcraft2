using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.Extensions;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Basic
{
	public class PlayerCapabilityAttack : PlayerCapability
	{
		public PlayerCapabilityAttack() : base("Attack", TargetType.Asset) { }

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			return actor.Speed > 0;
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			// All non-peasant units, regardless of color, can attack player-built walls.
			if (target != null && (target.Data.Type != AssetType.Wall || actor.Data.Type == AssetType.Peasant))
			{
				if (actor.Data.Color == target.Data.Color || target.Data.Color == PlayerColor.None)
					return false;
			}

			return actor.Speed > 0;
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			if (actor.TilePosition == target.TilePosition)
				return false;

			var newCommand = new AssetCommand
			{
				Action = AssetAction.Capability,
				Capability = AssetCapabilityType,
				Target = target,
				ActivatedCapability = new ActivatedCapability(actor, playerData, target)
			};

			actor.Commands.Clear();
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

				var assetCommand = new AssetCommand
				{
					Action = AssetAction.Attack,
					Target = Target
				};

				Actor.Commands.Clear();
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
				Register(new PlayerCapabilityAttack());
			}
		}
	}
}
