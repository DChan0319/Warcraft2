using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Player;
using Warcraft.Player.Capabilities;

namespace Warcraft.GameModel
{
	public class PlayerData
	{
		/// <summary>
		/// A list of all of the assets that was created by this player.
		/// </summary>
		/// <remarks>
		/// See <see cref="GameModel.AllAssets"/> for more information.
		/// </remarks>
		public Dictionary<int, PlayerAsset> AllAssets { get; private set; }

		public bool IsAi { get; set; }
		public PlayerColor Color { get; set; }
		public DecoratedMap ActualMap { get; set; }
		public DecoratedMap PlayerMap { get; set; }
		public VisibilityMap VisibilityMap { get; set; }
		public Dictionary<string, PlayerAssetData> AssetDatas { get; set; }
		public List<PlayerAsset> Assets { get; set; }
		public List<bool> Upgrades { get; set; }
		public List<GameEvent> GameEvents { get; set; }
		public int Gold { get; set; }
		public int Lumber { get; set; }
		public int Stone { get; set; }
		public int GameCycle { set; get; }
		public int HealSteps { set; get; }

		[JsonIgnore]
		public bool IsAlive { get { return Assets.Count > 0; } }

		[JsonIgnore]
		public int FoodConsumption
		{
			get { return Assets.Select(asset => asset.Data.FoodConsumption).Where(consumption => consumption > 0).Sum(); }
		}

		[JsonIgnore]
		public int FoodProduction
		{
			get
			{
				return Assets
					.Where(asset => asset.Data.FoodConsumption < 0 && (AssetAction.Construct != asset.GetAction() || asset.CurrentCommand().Target == null))
					.Sum(asset => -asset.Data.FoodConsumption);
			}
		}

		[JsonIgnore]
		public int FoodExcess { get { return MathHelper.Max(0, FoodProduction - FoodConsumption); } }

		[JsonIgnore]
		public int UnitCount { get { return Assets.Count(a => a.Speed != 0); } }

		[JsonConstructor, UsedImplicitly]
		private PlayerData() { }

		public PlayerData(DecoratedMap map, PlayerColor color)
		{
			AllAssets = new Dictionary<int, PlayerAsset>();

			IsAi = true; // Todo: Fix AI decision
			Color = color;
			ActualMap = map;
			AssetDatas = PlayerAssetData.DuplicateRegistry(Color);
			PlayerMap = ActualMap.CreateInitializeMap();
			VisibilityMap = ActualMap.CreateVisibilityMap();
			Assets = new List<PlayerAsset>();
			Upgrades = new List<bool>(new bool[(int)AssetCapabilityType.Max]);
			GameEvents = new List<GameEvent>();
			HealSteps = 0;

			foreach (var resource in ActualMap.InitialResources)
			{
				if (resource.Color == Color)
				{
					Gold = resource.Gold;
					Lumber = resource.Lumber;
					Stone = resource.Stone;
				}
			}

			foreach (var initialAsset in ActualMap.InitialAssets)
			{
				if (initialAsset.Color == Color)
				{
					var asset = CreateAsset(initialAsset.Type);
					asset.SetTilePosition(new Position(initialAsset.TilePosition));
					if (PlayerAssetData.NameToType(initialAsset.Type) == AssetType.GoldMine)
					{
						asset.Gold = Gold;
					}
				}
			}
		}

		/// <summary>
		/// Creates an asset of type <paramref name="assetTypeName"/> adds it to the map, and returns it.
		/// </summary>
		public PlayerAsset CreateAsset(string assetTypeName)
		{
			var createdAsset = new PlayerAsset(AssetDatas[assetTypeName]) { CreationCycle = GameCycle };
			Assets.Add(createdAsset);
			ActualMap.AddAsset(createdAsset);

			if (assetTypeName != "None")
			{
				AllAssets[createdAsset.Id] = createdAsset;
			}

			return createdAsset;
		}

		/// <summary>
		/// Removes <paramref name="asset"/> from the map and player data.
		/// </summary>
		public void DeleteAsset(PlayerAsset asset)
		{
			Assets.Remove(asset);
			ActualMap.RemoveAsset(asset);
		}

		/// <summary>
		/// Returns the number of owned assets matching the predicate.
		/// </summary>
		public int OwnedAssetCount(Predicate<PlayerAsset> predicate)
		{
			return PlayerMap.Assets.Count(a => a.Data.Color == Color && predicate.Invoke(a));
		}

		/// <summary>
		/// Returns the number of assets on the map matching the predicate.
		/// </summary>
		public int MapAssetCount(Predicate<PlayerAsset> predicate)
		{
			return PlayerMap.Assets.Count(predicate.Invoke);
		}

		/// <summary>
		/// Returns whether the requirements to for this asset are met.
		/// </summary>
		public bool AssetRequirementsMet(string assetTypeName)
		{
			var assetCount = new List<int>(new int[(int)AssetType.Max]);

			foreach (var asset in Assets)
			{
				if (asset.GetAction() != AssetAction.Construct)
					assetCount[(int)asset.Data.Type]++;
			}

			foreach (var requirement in AssetDatas[assetTypeName].AssetRequirements)
			{
				if (assetCount[(int)requirement] != 0)
					continue;

				if (requirement == AssetType.Keep && assetCount[(int)AssetType.Castle] > 0)
					continue;

				if (requirement == AssetType.TownHall && (assetCount[(int)AssetType.Keep] > 0 || assetCount[(int)AssetType.Castle] > 0))
					continue;

				return false;
			}

			return true;
		}

		#region Upgrades

		/// <summary>
		/// Adds the upgrade corresponding to <paramref name="upgradeName"/>
		/// to the player data.
		/// </summary>
		public void AddUpgrade(string upgradeName)
		{
			var upgrade = PlayerUpgrade.FindUpgradeByName(upgradeName);
			if (upgrade == null)
				return;

			foreach (var assetType in upgrade.AffectedAssets)
			{
				var assetName = PlayerAssetData.TypeToName(assetType);

				PlayerAssetData assetData;
				if (AssetDatas.TryGetValue(assetName, out assetData))
					assetData.AddUpgrade(upgrade);
			}

			Upgrades[(int)PlayerCapability.NameToType(upgradeName)] = true;
		}

		/// <summary>
		/// Returns whether the player data has the upgrade.
		/// </summary>
		public bool HasUpgrade(AssetCapabilityType upgrade)
		{
			if (upgrade < 0 || (int)upgrade >= Upgrades.Count)
				return false;

			return Upgrades[(int)upgrade];
		}

		#endregion

		/// <summary>
		/// Creates a marker at <paramref name="pos"/> and returns it.
		/// Adds it to the map if <paramref name="addToMap"/> is true.
		/// </summary>
		public PlayerAsset CreateMarker(Position pos, bool addToMap)
		{
			var newMarker = new PlayerAsset(AssetDatas["None"]);
			var tilePosition = new Position();
			tilePosition.SetToTile(pos);
			newMarker.SetTilePosition(tilePosition);
			if (addToMap)
				PlayerMap.AddAsset(newMarker);

			return newMarker;
		}

		public void UpdateVisibility()
		{
			var toRemove = new List<PlayerAsset>();

			VisibilityMap.Update(Assets);
			PlayerMap.UpdateMap(VisibilityMap, ActualMap);

			foreach (var asset in PlayerMap.Assets)
			{
				if (asset.Data.Type == AssetType.None && asset.GetAction() == AssetAction.None)
				{
					asset.Step++;

					if (asset.Step * 2 > PlayerAsset.UpdateFrequency)
					{
						toRemove.Add(asset);
					}
				}
			}

			foreach (var asset in toRemove)
			{
				PlayerMap.RemoveAsset(asset);
			}
		}

		/// <summary>
		/// Returns a list of assets inside the selection rectangle.
		/// </summary>
		public List<PlayerAsset> SelectAssets(Rectangle selectionRectangle, AssetType assetType, bool selectIdentical = false)
		{
			var selectedAssets = new List<PlayerAsset>();

			if (selectionRectangle.Width == 0 || selectionRectangle.Height == 0)
			{
				var position = new Position(selectionRectangle.X, selectionRectangle.Y);

				var bestAsset = SelectAsset(position, assetType);
				if (bestAsset == null)
					return selectedAssets;

				selectedAssets.Add(bestAsset);

				// Selects same units when double clicking
				if (selectIdentical && bestAsset.Speed != 0)
					selectedAssets.AddRange(Assets.Where(asset => asset != bestAsset && asset.Data.Type == assetType && !asset.PerformingHiddenAction));
			}
			else
			{
				var anyMoveable = false;
				foreach (var asset in Assets)
				{
					// Select a maximum of 9 units
					if (selectedAssets.Count >= 9)
						break;

					if (!selectionRectangle.Contains(asset.Position.ToPoint()) || asset.PerformingHiddenAction)
						continue;

					if (anyMoveable)
					{
						if (asset.Speed != 0)
							selectedAssets.Add(asset);
					}
					else
					{
						if (asset.Speed != 0)
						{
							selectedAssets.Clear();
							selectedAssets.Add(asset);
							anyMoveable = true;
						}
						else if (selectedAssets.Count == 0)
							selectedAssets.Add(asset);
					}
				}
			}

			return selectedAssets;
		}

		/// <summary>
		/// Selects the asset closest to <paramref name="pos"/>.
		/// </summary>
		public PlayerAsset SelectAsset(Position pos, AssetType assetType)
		{
			if (assetType == AssetType.None)
				return null;

			PlayerAsset bestAsset = null;
			var bestDistanceSquared = -1;

			foreach (var asset in Assets)
			{
				if (asset.Data.Type == assetType && !asset.PerformingHiddenAction)
				{
					var currentDistance = asset.Position.DistanceSquared(pos);

					if (bestDistanceSquared == -1 || currentDistance < bestDistanceSquared)
					{
						bestDistanceSquared = currentDistance;
						bestAsset = asset;
					}
				}
			}

			return bestAsset;
		}

		/// <summary>
		/// Returns the closest asset owned by this player within <paramref name="assetTypes"/>.
		/// </summary>
		public PlayerAsset FindNearestOwnedAsset(Position pos, params AssetType[] assetTypes)
		{
			PlayerAsset bestAsset = null;
			var bestDistanceSquared = -1;

			foreach (var asset in Assets)
			{
				foreach (var assetType in assetTypes)
				{
					if (asset.Data.Type == assetType && (asset.GetAction() != AssetAction.Construct || assetType == AssetType.Keep || assetType == AssetType.Castle))
					{
						var currentDistance = asset.Position.DistanceSquared(pos);

						if (bestDistanceSquared == -1 || currentDistance < bestDistanceSquared)
						{
							bestDistanceSquared = currentDistance;
							bestAsset = asset;
						}
						break;
					}
				}
			}

			return bestAsset;
		}

		/// <summary>
		/// Returns the closest asset matching <paramref name="assetType"/>.
		/// </summary>
		public PlayerAsset FindNearestAsset(Position pos, AssetType assetType)
		{
			PlayerAsset bestAsset = null;
			var bestDistanceSquared = -1;

			foreach (var asset in PlayerMap.Assets)
			{
				if (asset.Data.Type != assetType) continue;

				var currentDistance = asset.Position.DistanceSquared(pos);
				if (bestDistanceSquared == -1 || currentDistance < bestDistanceSquared)
				{
					bestDistanceSquared = currentDistance;
					bestAsset = asset;
				}
			}

			return bestAsset;
		}

		/// <summary>
		/// Returns the closest <see cref="PlayerAsset"/> that does not match this
		/// player's color within <paramref name="range"/> of <paramref name="pos"/>.
		/// </summary>
		public PlayerAsset FindNearestEnemy(Position pos, int range)
		{
			PlayerAsset bestAsset = null;
			var bestDistanceSquared = -1;

			if (range > 0)
				range = GameModel.RangeToDistanceSquared(range);

			foreach (var asset in PlayerMap.Assets)
			{
				if (asset.Data.Color == Color || asset.Data.Color == PlayerColor.None || !asset.IsAlive)
					continue;

				var command = asset.CurrentCommand();
				if (command.Action == AssetAction.Capability)
				{
					if (command.Target != null && command.Target.GetAction() == AssetAction.Construct)
						continue;
				}

				if (asset.PerformingHiddenAction)
					continue;

				var currentDistance = asset.ClosestPosition(pos).DistanceSquared(pos);
				if (range < 0 || currentDistance <= range)
				{
					if (bestDistanceSquared == -1 || currentDistance < bestDistanceSquared)
					{
						bestDistanceSquared = currentDistance;
						bestAsset = asset;
					}
				}
			}

			return bestAsset;
		}

		/// <summary>
		/// Returns the position where the asset would best fit.
		/// </summary>
		public Position FindBestAssetPlacement(Position position, PlayerAsset builder, AssetType assetType, int buffer)
		{
			var assetData = AssetDatas[PlayerAssetData.TypeToName(assetType)];
			var placementSize = assetData.Size + 2 * buffer;
			var maxDistance = MathHelper.Max(PlayerMap.MapWidth, PlayerMap.MapHeight);

			for (var distance = 1; distance < maxDistance; distance++)
			{
				var bestPosition = new Position();
				var bestDistance = -1;
				var leftX = position.X - distance;
				var topY = position.Y - distance;
				var rightX = position.X + distance;
				var bottomY = position.Y + distance;
				var leftValid = true;
				var rightValid = true;
				var topValid = true;
				var bottomValid = true;

				if (leftX < 0)
				{
					leftX = 0;
					leftValid = false;
				}

				if (topY < 0)
				{
					topY = 0;
					topValid = false;
				}

				if (rightX >= PlayerMap.MapWidth)
				{
					rightX = PlayerMap.MapWidth - 1;
					rightValid = false;
				}

				if (bottomY >= PlayerMap.MapHeight)
				{
					bottomY = PlayerMap.MapHeight - 1;
					bottomValid = false;
				}

				if (topValid)
				{
					for (var index = leftX; index <= rightX; index++)
					{
						var tempPosition = new Position(index, topY);
						if (!PlayerMap.CanPlaceAsset(tempPosition, placementSize, builder)) continue;

						var currentDistance = builder.TilePosition.DistanceSquared(tempPosition);
						if (bestDistance == -1 || currentDistance < bestDistance)
						{
							bestDistance = currentDistance;
							bestPosition = tempPosition;
						}
					}
				}

				if (rightValid)
				{
					for (var index = topY; index <= bottomY; index++)
					{
						var tempPosition = new Position(rightX, index);
						if (!PlayerMap.CanPlaceAsset(tempPosition, placementSize, builder)) continue;

						var currentDistance = builder.TilePosition.DistanceSquared(tempPosition);
						if (bestDistance == -1 || currentDistance < bestDistance)
						{
							bestDistance = currentDistance;
							bestPosition = tempPosition;
						}
					}
				}

				if (bottomValid)
				{
					for (var index = leftX; index <= rightX; index++)
					{
						var tempPosition = new Position(index, bottomY);
						if (!PlayerMap.CanPlaceAsset(tempPosition, placementSize, builder)) continue;

						var currentDistance = builder.TilePosition.DistanceSquared(tempPosition);
						if (bestDistance == -1 || currentDistance < bestDistance)
						{
							bestDistance = currentDistance;
							bestPosition = tempPosition;
						}
					}
				}

				if (leftValid)
				{
					for (var index = topY; index <= bottomY; index++)
					{
						var tempPosition = new Position(leftX, index);
						if (!PlayerMap.CanPlaceAsset(tempPosition, placementSize, builder)) continue;

						var currentDistance = builder.TilePosition.DistanceSquared(tempPosition);
						if (bestDistance == -1 || currentDistance < bestDistance)
						{
							bestDistance = currentDistance;
							bestPosition = tempPosition;
						}
					}
				}

				if (bestDistance != -1)
					return new Position(bestPosition.X + buffer, bestPosition.Y + buffer);
			}

			return new Position(-1, -1);
		}

		#region Game Events

		/// <summary>
		/// Adds a new game event to <see cref="GameEvents"/>.
		/// </summary>
		/// <param name="type">The type of event</param>
		public void AddGameEvent(EventType type)
		{
			AddGameEvent(null, type);
		}

		/// <summary>
		/// Adds a new game event to <see cref="GameEvents"/>.
		/// </summary>
		/// <param name="asset">The asset which invoked the event</param>
		/// <param name="type">The type of event</param>
		public void AddGameEvent(PlayerAsset asset, EventType type)
		{
			GameEvents.Add(new GameEvent { Type = type, Asset = asset });
		}

		#endregion
	}
}