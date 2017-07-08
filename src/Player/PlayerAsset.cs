using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Warcraft.App;

namespace Warcraft.Player
{
	public class PlayerAsset
	{
		/// <summary>
		/// Unique Asset Id
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// Id pool to generate ids from.
		/// </summary>
		private static int IdPool { get; set; }

		public int CreationCycle { get; set; }

		public int Health
		{
			get { return health; }
			set { health = MathHelper.Clamp(value, 0, Data.Health); }
		}
		private int health;

		public int Gold { get; set; }
		public int Lumber { get; set; }
		public int Stone { get; set; }
		public int Step { get; set; }
		public int IdleSteps { get; set; }
		public int RandomTurnSteps { get; set; }

		public List<PlayerAsset> ShelteredPeasants { get; private set; }

		public Point MoveRemainder;

		public Position TilePosition;
		public Position Position;

		public Direction Direction { get; set; }
		public List<AssetCommand> Commands { get; private set; }

		/// <summary>
		/// Returns whether the asset is being trained (thus hidden inside a building).
		/// </summary>
		public bool Training { get; set; }

		[JsonProperty(Order = -2)]
		public PlayerAssetData Data { get; set; }

		/// <summary>
		/// See <see cref="Settings.UpdateFrequency"/>.
		/// </summary>
		public static int UpdateFrequency = Settings.UpdateFrequency / Settings.Debug.SpeedFactor;
		public static int UpdateDivisor = UpdateFrequency * 32;

		[JsonIgnore]
		public int Speed { get { return Data.Speed; } }

		/// <summary>
		/// Returns the base + upgraded armor.
		/// </summary>
		[JsonIgnore]
		public int EffectiveArmor { get { return Data.Armor + Data.ArmorUpgrade; } }

		/// <summary>
		/// Returns the base + upgraded sight.
		/// </summary>
		[JsonIgnore]
		public int EffectiveSight { get { return Data.Sight + Data.SightUpgrade; } }

		/// <summary>
		/// Returns the base + upgraded speed.
		/// </summary>
		[JsonIgnore]
		public int EffectiveSpeed { get { return Speed + Data.SpeedUpgrade; } }

		/// <summary>
		/// Returns the base + upgraded basic damage.
		/// </summary>
		[JsonIgnore]
		public int EffectiveBasicDamage { get { return Data.BasicDamage + Data.BasicDamageUpgrade; } }

		/// <summary>
		/// Returns the base + upgraded piercing damage.
		/// </summary>
		[JsonIgnore]
		public int EffectivePiercingDamage { get { return Data.PiercingDamage + Data.PiercingDamageUpgrade; } }

		/// <summary>
		/// Returns the base + upgraded range.
		/// </summary>
		[JsonIgnore]
		public int EffectiveRange { get { return Data.Range + Data.RangeUpgrade; } }

		/// <summary>
		/// Returns whether this asset still has <see cref="Health"/> left.
		/// </summary>
		[JsonIgnore]
		public bool IsAlive { get { return Health > 0; } }

		/// <summary>
		/// Returns whether this asset is a fighter.
		/// </summary>
		[JsonIgnore]
		public bool IsMilitary { get { return Data.Type == AssetType.Footman || Data.Type == AssetType.Knight || Data.Type == AssetType.Archer || Data.Type == AssetType.Ranger; } }

		/// <summary>
		/// Returns whether the asset is conveying resources (thus hidden inside a building).
		/// </summary>
		[JsonIgnore]
		public bool Conveying
		{
			get
			{
				var action = GetAction();
				return action == AssetAction.ConveyGold || action == AssetAction.ConveyLumber || action == AssetAction.ConveyStone;
			}
		}

		/// <summary>
		/// Returns whether the asset is performing an action that results
		/// in being hidden (mining, conveying, building, training, or sheltering).
		/// </summary>
		[JsonIgnore]
		public bool PerformingHiddenAction { get { return Conveying || Training || GetAction() == AssetAction.MineGold || GetAction() == AssetAction.Shelter; } }

		/// <summary>
		/// Returns whether the current command can be interrupted.
		/// </summary>
		[JsonIgnore]
		public bool IsInterruptible
		{
			get
			{
				var command = CurrentCommand();
				switch (command.Action)
				{
					case AssetAction.Construct:
					case AssetAction.Build:
					case AssetAction.Shelter:
					case AssetAction.MineGold:
					case AssetAction.ConveyGold:
					case AssetAction.ConveyLumber:
					case AssetAction.ConveyStone:
					case AssetAction.Death:
					case AssetAction.Decay:
						return false;
					case AssetAction.Capability:
						if (command.Target != null)
							return command.Target.GetAction() != AssetAction.Construct;
						return true;
					default: return true;
				}
			}
		}

		/// <summary>
		/// Returns whether asset is idle (none or standing ground).
		/// </summary>
		[JsonIgnore]
		public bool IsIdle
		{
			get
			{
				var action = GetAction();
				return action == AssetAction.None || action == AssetAction.StandGround;
			}
		}

		/// <summary>
		/// Returns the shelter capacity of the asset.
		/// </summary>
		[JsonIgnore]
		public int ShelterCapacity
		{
			get
			{
				switch (Data.Type)
				{
					case AssetType.TownHall: return 4;
					case AssetType.Keep: return 6;
					case AssetType.Castle: return 8;
					default: return 0;
				}
			}
		}

		[JsonConstructor, UsedImplicitly]
		private PlayerAsset() { }

		public PlayerAsset(PlayerAssetData data)
		{
			CreationCycle = 0;
			Data = data;
			Health = data.Health;
			Gold = 0;
			Lumber = 0;
			Stone = 0;
			Step = 0;
			MoveRemainder = Point.Zero;
			Direction = Direction.South;
			ShelteredPeasants = new List<PlayerAsset>();
			Commands = new List<AssetCommand>();
			TilePosition = new Position();
			Position = new Position();
			IdleSteps = 0;
			RandomTurnSteps = int.MaxValue;

			Id = IdPool++;
		}

		/// <summary>
		/// Sets the tile position and updates the detailed position.
		/// </summary>
		public void SetTilePosition(Position toPosition)
		{
			Position.SetFromTile(toPosition);
			TilePosition = toPosition;
		}

		/// <summary>
		/// Sets the detailed position and updates the tiled position.
		/// </summary>
		public void SetPosition(Position toTilePosition)
		{
			TilePosition.SetToTile(toTilePosition);
			Position = toTilePosition;
		}

		/// <summary>
		/// Returns the position closest to <paramref name="assetPosition"/>.
		/// </summary>
		public Position ClosestPosition(Position assetPosition)
		{
			return assetPosition.ClosestPosition(Position, Data.Size);
		}

		#region Commands

		/// <summary>
		/// Adds <paramref name="command"/> to the beginning of the command list.
		/// </summary>
		public void PushCommand(AssetCommand command)
		{
			Commands.Add(command);
		}

		/// <summary>
		/// Adds <paramref name="command"/> to the end of the command list.
		/// </summary>
		public void EnqueueCommand(AssetCommand command)
		{
			Commands.Insert(0, command);
		}

		/// <summary>
		/// Removes the first command in the command list, if any.
		/// </summary>
		public void PopCommand()
		{
			if (Commands.Count > 0)
				Commands.RemoveAt(Commands.Count - 1);
		}

		/// <summary>
		/// Clears the command list.
		/// </summary>
		public void ClearCommands()
		{
			Commands.Clear();
		}

		/// <summary>
		/// Returns the first command in the command list,
		/// or an empty command if there are none.
		/// </summary>
		public AssetCommand CurrentCommand()
		{
			return Commands.Count > 0 ? Commands.Last() : new AssetCommand { Action = AssetAction.None };
		}

		/// <summary>
		/// Returns the second command in the command list,
		/// or an empty command if there are none.
		/// </summary>
		public AssetCommand NextCommand()
		{
			return Commands.Count > 1 ? Commands.ElementAt(Commands.Count - 2) : new AssetCommand { Action = AssetAction.None };
		}

		/// <summary>
		/// Returns the action of the first command, or None
		/// if there are no commands.
		/// </summary>
		public AssetAction GetAction()
		{
			return Commands.Count > 0 ? Commands.Last().Action : AssetAction.None;
		}

		/// <summary>
		/// Returns whether there are and commands that match <paramref name="capability"/>.
		/// </summary>
		public bool HasActiveCapability(AssetCapabilityType capability)
		{
			return Commands.Where(c => c.Action == AssetAction.Capability).Any(command => command.Capability == capability);
		}

		#endregion

		#region Capabilities

		/// <summary>
		/// Returns whether this asset has <paramref name="capability"/>.
		/// </summary>
		public bool HasCapability(AssetCapabilityType capability)
		{
			return Data.HasCapability(capability);
		}

		#endregion

		/// <summary>
		/// Moves the player asset from its current position towards its current direction.
		/// </summary>
		public bool MoveStep(PlayerAsset[,] occupancyMap)
		{
			var currentOctant = Position.TileOctant();
			int[] deltaX = { 0, 5, 7, 5, 0, -5, -7, -5 };
			int[] deltaY = { -7, -5, 0, 5, 7, 5, 0, -5 };
			Position currentTile = TilePosition, currentPosition = Position;

			if (currentOctant == Direction.Max || currentOctant == Direction)
			{
				// Already aligned, just move
				var newX = Speed * deltaX[(int)Direction] * Position.TileWidth + MoveRemainder.X;
				var newY = Speed * deltaY[(int)Direction] * Position.TileHeight + MoveRemainder.Y;
				MoveRemainder.X = newX % UpdateDivisor;
				MoveRemainder.Y = newY % UpdateDivisor;
				Position.X += newX / UpdateDivisor;
				Position.Y += newY / UpdateDivisor;
			}
			else
			{
				// Entering
				var newX = Speed * deltaX[(int)Direction] * Position.TileWidth + MoveRemainder.X;
				var newY = Speed * deltaY[(int)Direction] * Position.TileHeight + MoveRemainder.Y;
				Point tempMoveRemainder;
				tempMoveRemainder.X = newX % UpdateDivisor;
				tempMoveRemainder.Y = newY % UpdateDivisor;
				var newPosition = new Position(Position.X + newX / UpdateDivisor, Position.Y + newY / UpdateDivisor);

				if (newPosition.TileOctant() == Direction)
				{
					newPosition.SetToTile(newPosition);
					newPosition.SetFromTile(newPosition);
					tempMoveRemainder = Point.Zero;
				}

				Position = newPosition;
				MoveRemainder = tempMoveRemainder;
			}

			TilePosition.SetToTile(Position);

			if (currentTile != TilePosition)
			{
				if (occupancyMap[TilePosition.Y, TilePosition.X] != null)
				{
					var returnValue = false;
					if (occupancyMap[TilePosition.Y, TilePosition.X].GetAction() == AssetAction.Walk)
						returnValue = occupancyMap[TilePosition.Y, TilePosition.X].Direction == currentPosition.TileOctant();

					TilePosition = currentTile;
					Position = currentPosition;
					return returnValue;
				}

				occupancyMap[TilePosition.Y, TilePosition.X] = occupancyMap[currentTile.Y, currentTile.X];
				occupancyMap[currentTile.Y, currentTile.X] = null;
			}

			Step++;
			return true;
		}

		/// <summary>
		/// Removes all sheltered peasants from the asset.
		/// </summary>
		public void RemoveShelteredPeasants()
		{
			var nextTileTarget = new Position(App.Data.GameModel.Player(Data.Color).PlayerMap.MapWidth - 1, App.Data.GameModel.Player(Data.Color).PlayerMap.MapHeight - 1);
			foreach (var peasant in ShelteredPeasants)
			{
				peasant.SetTilePosition(App.Data.GameModel.Player(Data.Color).PlayerMap.FindAssetPlacement(peasant, this, nextTileTarget));
				peasant.ClearCommands();
				peasant.Direction = Direction.South;
			}
			ShelteredPeasants.Clear();
		}
	}
}