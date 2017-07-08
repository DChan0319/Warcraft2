using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.Extensions;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Basic
{
	public class PlayerCapabilityMineHarvest : PlayerCapability
	{
		public PlayerCapabilityMineHarvest() : base("Mine", TargetType.TerrainOrAsset) { }

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			return actor.HasCapability(AssetCapabilityType.Mine);
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			if (!actor.HasCapability(AssetCapabilityType.Mine))
				return false;

			if (actor.Gold != 0 || actor.Lumber != 0 || actor.Stone != 0)
				return false;

			if (target.Data.Type == AssetType.GoldMine)
				return true;

			if (target.Data.Type != AssetType.None)
				return false;

			var tileType = playerData.PlayerMap.GetTileType(target.TilePosition);
			return tileType == TileType.Tree || tileType == TileType.Rock || tileType == TileType.Rubble;
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			var newCommand = new AssetCommand
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
					Target = Target
				};

				if (Target.Data.Type == AssetType.GoldMine)
				{
					assetCommand.Action = AssetAction.MineGold;
				}
				else
				{
					var tileType = PlayerData.PlayerMap.GetTileType(Target.TilePosition);
					switch (tileType)
					{
						case TileType.Tree: assetCommand.Action = AssetAction.HarvestLumber; break;
						case TileType.Rock:
						case TileType.Rubble: assetCommand.Action = AssetAction.QuarryStone; break;
					}
				}

				Actor.ClearCommands();
				Actor.PushCommand(assetCommand);
				assetCommand.Action = AssetAction.Walk;
				if (!Actor.TilePosition.IsTileAligned)
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
				Register(new PlayerCapabilityMineHarvest());
			}
		}
	}
}