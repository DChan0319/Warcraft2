using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Warcraft.App;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities.Build
{
	public class PlayerCapabilityBuildNormal : PlayerCapability
	{
		public readonly string BuildingName;

		public PlayerCapabilityBuildNormal(string buildingName) : base($"Build{buildingName}", TargetType.TerrainOrAsset)
		{
			BuildingName = buildingName;
		}

		public override bool CanInitiate(PlayerAsset actor, PlayerData playerData)
		{
			PlayerAssetData assetType;
			if (!playerData.AssetDatas.TryGetValue(BuildingName, out assetType))
				return false;

			return assetType.GoldCost <= playerData.Gold && assetType.LumberCost <= playerData.Lumber && assetType.StoneCost <= playerData.Stone;
		}

		public override bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			PlayerAssetData assetType;
			if (actor != target && target.Data.Type != AssetType.None) return false;
			if (!playerData.AssetDatas.TryGetValue(BuildingName, out assetType)) return false;
			if (assetType.GoldCost > playerData.Gold || assetType.LumberCost > playerData.Lumber || assetType.StoneCost > playerData.Stone) return false;

			if (assetType.Type != AssetType.Wall)
			{
				if (!playerData.PlayerMap.CanPlaceAsset(target.TilePosition, assetType.Size, actor)) return false;
			}
			else
			{
				// Check asset placements from the builder's position to the cursor's position.
				if (Math.Abs(actor.TilePosition.X - target.TilePosition.X) > Math.Abs(actor.TilePosition.Y - target.TilePosition.Y))
				{
					var start = Math.Min(actor.TilePosition.X, target.TilePosition.X);
					var stop = Math.Max(actor.TilePosition.X, target.TilePosition.X);
					for (var x = start; x <= stop; x++)
					{
						if (!playerData.PlayerMap.CanPlaceAsset(new Position(x, actor.TilePosition.Y), assetType.Size, actor))
							return false;
					}
				}
				else
				{
					var start = Math.Min(actor.TilePosition.Y, target.TilePosition.Y);
					var stop = Math.Max(actor.TilePosition.Y, target.TilePosition.Y);
					for (var y = start; y <= stop; y++)
					{
						if (!playerData.PlayerMap.CanPlaceAsset(new Position(actor.TilePosition.X, y), assetType.Size, actor))
							return false;
					}
				}
			}

			return true;
		}

		public override bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			PlayerAssetData assetType;
			if (!playerData.AssetDatas.TryGetValue(BuildingName, out assetType)) return false;

			actor.ClearCommands();

			var newCommand = new AssetCommand();
			PlayerAsset newAsset;
			if (assetType.Type != AssetType.Wall)
			{
				if (actor.TilePosition == target.TilePosition)
				{
					newAsset = playerData.CreateAsset(BuildingName);
					newAsset.SetTilePosition(target.TilePosition);
					newAsset.Health = 1;

					newCommand.Action = AssetAction.Capability;
					newCommand.Capability = AssetCapabilityType;
					newCommand.Target = newAsset;
					newCommand.ActivatedCapability = new ActivatedCapability(actor, playerData, newAsset, assetType.GoldCost, assetType.LumberCost, assetType.StoneCost, assetType.BuildTime * PlayerAsset.UpdateFrequency);
					actor.PushCommand(newCommand);
				}
				else
				{
					newCommand.Action = AssetAction.Capability;
					newCommand.Capability = AssetCapabilityType;
					newCommand.Target = target;
					actor.PushCommand(newCommand);

					newCommand.Action = AssetAction.Walk;
					actor.PushCommand(newCommand);
				}
			}
			else
			{
				// Create walls from the actor's position to the target position
				if (Math.Abs(actor.TilePosition.X - target.TilePosition.X) > Math.Abs(actor.TilePosition.Y - target.TilePosition.Y))
				{
					var offset = target.TilePosition.X - actor.TilePosition.X;
					for (var i = 0; i <= Math.Abs(target.TilePosition.X - actor.TilePosition.X); i++)
					{
						var targetPosition = actor.TilePosition;
						targetPosition.X += offset;

						// Create walls with no color association.
						newAsset = Data.GameModel.Player(PlayerColor.None).CreateAsset(BuildingName);
						newAsset.SetTilePosition(targetPosition);
						newAsset.Health = 1;

						newCommand.Action = AssetAction.Capability;
						newCommand.Capability = AssetCapabilityType;
						newCommand.Target = newAsset;
						newCommand.ActivatedCapability = new ActivatedCapability(actor, playerData, newAsset, assetType.GoldCost, assetType.LumberCost, assetType.StoneCost, assetType.BuildTime * PlayerAsset.UpdateFrequency);
						actor.PushCommand(newCommand);

						if (offset > 0) offset--;
						else offset++;
					}
				}
				else
				{
					var offset = target.TilePosition.Y - actor.TilePosition.Y;
					for (var i = 0; i <= Math.Abs(target.TilePosition.Y - actor.TilePosition.Y); i++)
					{
						var targetPosition = actor.TilePosition;
						targetPosition.Y += offset;

						// Create walls with no color association.
						newAsset = Data.GameModel.Player(PlayerColor.None).CreateAsset(BuildingName);
						newAsset.SetTilePosition(targetPosition);
						newAsset.Health = 1;

						newCommand.Action = AssetAction.Capability;
						newCommand.Capability = AssetCapabilityType;
						newCommand.Target = newAsset;
						newCommand.ActivatedCapability = new ActivatedCapability(actor, playerData, newAsset, assetType.GoldCost, assetType.LumberCost, assetType.StoneCost, assetType.BuildTime * PlayerAsset.UpdateFrequency);
						actor.PushCommand(newCommand);

						if (offset > 0) offset--;
						else offset++;
					}
				}
			}

			return true;
		}

		private class ActivatedCapability : ActivatedPlayerCapability
		{
			public int CurrentStep;
			public int TotalSteps;
			public int Gold;
			public int Lumber;
			public int Stone;

			/// <summary>
			/// This is required for the game to be able
			/// to save/load a building construction properly.
			/// </summary>
			[JsonConstructor, UsedImplicitly]
			public ActivatedCapability() { }

			public ActivatedCapability(PlayerAsset actor, PlayerData playerData, PlayerAsset target, int gold, int lumber, int stone, int steps) : base(actor, playerData, target)
			{
				CurrentStep = 0;
				TotalSteps = steps;
				Gold = gold;
				Lumber = lumber;
				Stone = stone;

				playerData.Gold -= Gold;
				playerData.Lumber -= Lumber;
				playerData.Stone -= Stone;

				var assetCommand = new AssetCommand
				{
					Action = AssetAction.Construct,
					Target = Actor
				};
				target.PushCommand(assetCommand);
			}

			public override int PercentComplete(int max)
			{
				return CurrentStep * max / TotalSteps;
			}

			public override bool IncrementStep()
			{
				// Skip over assets that were destroyed while waiting to be built (walls)
				if (CurrentStep == 0 && Target.Health == 0)
				{
					if (Actor.NextCommand().Action != Actor.CurrentCommand().Action)
						PlayerData.AddGameEvent(Actor, EventType.WorkComplete);

					Actor.PopCommand();
					return false;
				}

				var addHealth = Target.Data.Health * (CurrentStep + 1) / TotalSteps - Target.Data.Health * CurrentStep / TotalSteps;
				Target.Health = MathHelper.Min(Target.Health + addHealth, Target.Data.Health);

				CurrentStep++;
				Actor.Step++;
				Target.Step++;

				// Finished building
				if (CurrentStep >= TotalSteps)
				{
					// Only play sfx if the next command is not also build (for building multiple walls)
					if (Actor.NextCommand().Action != Actor.CurrentCommand().Action)
						PlayerData.AddGameEvent(Actor, EventType.WorkComplete);

					Target.PopCommand();
					Actor.PopCommand();
					Actor.SetTilePosition(PlayerData.PlayerMap.FindAssetPlacement(Actor, Target, new Position(PlayerData.PlayerMap.MapWidth - 1, PlayerData.PlayerMap.MapHeight - 1)));
					Actor.Step = 0;
					Target.Step = 0;

					return true;
				}

				return false;
			}

			public override void Cancel()
			{
				PlayerData.Gold += Gold;
				PlayerData.Lumber += Lumber;
				PlayerData.Stone += Stone;

				if (Target.Data.Type != AssetType.Wall)
					PlayerData.DeleteAsset(Target);
				else
				{
					// Walls have no color association.
					Data.GameModel.Player(PlayerColor.None).DeleteAsset(Target);
				}

				Actor.PopCommand();
			}
		}

		[PlayerCapabilityRegistrant, UsedImplicitly]
		public class Registrant
		{
			public Registrant()
			{
				Register(new PlayerCapabilityBuildNormal("Wall"));
				Register(new PlayerCapabilityBuildNormal("TownHall"));
				Register(new PlayerCapabilityBuildNormal("Farm"));
				Register(new PlayerCapabilityBuildNormal("Barracks"));
				Register(new PlayerCapabilityBuildNormal("LumberMill"));
				Register(new PlayerCapabilityBuildNormal("Blacksmith"));
				Register(new PlayerCapabilityBuildNormal("ScoutTower"));
			}
		}
	}
}