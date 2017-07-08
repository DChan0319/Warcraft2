using JetBrains.Annotations;
using Warcraft.App;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Basic
{
	public class PlayerCapabilityCancel : PlayerCapability
	{
		public PlayerCapabilityCancel() : base("Cancel", TargetType.None) { }

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
			if (actor.Data.Type != AssetType.Wall)
			{
				var newCommand = new AssetCommand
				{
					Action = AssetAction.Capability,
					Capability = AssetCapabilityType,
					Target = target,
					ActivatedCapability = new ActivatedCapability(actor, playerData, target)
				};
				actor.PushCommand(newCommand);
			}
			else
			{
				// Cancel all other walls being built by the builder
				var builder = actor.CurrentCommand().Target;
				foreach (var targetCommand in builder.Commands)
				{
					if (targetCommand.Target.Data.Type != AssetType.Wall)
						continue;

					var newCommand = new AssetCommand
					{
						Action = AssetAction.Capability,
						Capability = AssetCapabilityType,
						Target = target,
						ActivatedCapability = new ActivatedCapability(targetCommand.Target, playerData, target)
					};
					targetCommand.Target.PushCommand(newCommand);
				}
			}

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
				Actor.PopCommand();

				if (Actor.GetAction() == AssetAction.None)
					return true;

				var assetCommand = Actor.CurrentCommand();
				if (assetCommand.Action == AssetAction.Construct)
				{
					if (assetCommand.Target != null)
						assetCommand.Target.CurrentCommand().ActivatedCapability.Cancel();
					else if (assetCommand.ActivatedCapability != null)
						assetCommand.ActivatedCapability.Cancel();
				}
				else
					assetCommand.ActivatedCapability?.Cancel();

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
				Register(new PlayerCapabilityCancel());
			}
		}
	}
}