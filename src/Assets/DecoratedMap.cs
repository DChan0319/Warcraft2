using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Warcraft.App;
using Warcraft.GameModel;
using Warcraft.Player;
using Warcraft.Util;

namespace Warcraft.Assets
{
	public class DecoratedMap : TerrainMap
	{
		public static readonly List<DecoratedMap> AllMaps = new List<DecoratedMap>();
		public static readonly Dictionary<string, int> MapNameTranslations = new Dictionary<string, int>();

		/// <summary>
		/// List of initial resources for the map.
		/// </summary>
		public readonly List<InitialResource> InitialResources = new List<InitialResource>();

		/// <summary>
		/// List of initial assets for the map.
		/// </summary>
		public readonly List<InitialAsset> InitialAssets = new List<InitialAsset>();

		/// <summary>
		/// List of assets on the map.
		/// </summary>
		public readonly List<PlayerAsset> Assets = new List<PlayerAsset>();

		/// <summary>
		/// Map used for pathfinding
		/// </summary>
		protected int[,] SearchMap = new int[0, 0];

		/// <summary>
		/// Gets the number of players that can play on the map.
		/// </summary>
		public int PlayerCount { get { return InitialResources.Count - 1; } }

		[JsonConstructor]
		public DecoratedMap() { }

		private DecoratedMap(DecoratedMap map, List<PlayerColor> newColors) : base(map)
		{
			Assets = map.Assets.ToList();

			foreach (var asset in map.InitialAssets.ToList())
			{
				var newAsset = asset;
				if (newColors.Count > (int)asset.Color)
					newAsset.Color = newColors[(int)newAsset.Color];

				InitialAssets.Add(newAsset);
			}

			foreach (var resource in map.InitialResources.ToList())
			{
				var newResource = resource;
				if (newColors.Count > (int)resource.Color)
					newResource.Color = newColors[(int)newResource.Color];

				InitialResources.Add(newResource);
			}
		}

		/// <summary>
		/// Reads from <paramref name="dataFile"/> and creates a decorated terrain map.
		/// </summary>
		protected override void Load(TextReader dataFile)
		{
			base.Load(dataFile);

			string temp;

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid decorated terrain map format.");

			// Read resources
			var resourceCount = int.Parse(temp);
			InitialResources.Clear();
			for (var i = 0; i <= resourceCount; i++)
			{
				temp = dataFile.ReadLine();
				if (temp == null)
					throw new FormatException("Failed to read map resources.");

				var resourcesString = temp.Split();
				if (resourcesString.Length < 3)
					throw new Exception("Initial resource list too short.");

				InitialResource initialResource;
				initialResource.Color = (PlayerColor)int.Parse(resourcesString[0]);
				if (i == 0 && initialResource.Color != PlayerColor.None)
					throw new FormatException("Expected first resource to be for color None.");

				initialResource.Gold = int.Parse(resourcesString[1]);
				initialResource.Lumber = int.Parse(resourcesString[2]);
				initialResource.Stone = int.Parse(resourcesString[3]);

				InitialResources.Add(initialResource);
			}

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid decorated terrain map format.");

			// Read assets
			var assetCount = int.Parse(temp);
			for (var i = 0; i < assetCount; i++)
			{
				temp = dataFile.ReadLine();
				if (temp == null)
					throw new FormatException("Failed to read map asset.");

				var assetString = temp.Split();
				if (assetString.Length < 4)
					throw new FormatException("Asset property list too short");

				InitialAsset initialAsset;
				initialAsset.Type = assetString[0];
				initialAsset.Color = (PlayerColor)int.Parse(assetString[1]);
				initialAsset.TilePosition.X = int.Parse(assetString[2]);
				initialAsset.TilePosition.Y = int.Parse(assetString[3]);

				if (initialAsset.TilePosition.X < 0 || initialAsset.TilePosition.Y < 0
					|| initialAsset.TilePosition.X >= MapWidth || initialAsset.TilePosition.Y >= MapHeight)
					throw new Exception("Invalid asset position.");

				InitialAssets.Add(initialAsset);
			}
		}

		public static void LoadMaps()
		{
			var mapFiles = Directory.GetFiles(Paths.Map, "*.map", SearchOption.AllDirectories);
			foreach (var file in mapFiles)
			{
				var tempMap = new DecoratedMap();
				tempMap.Load(file);
				Trace.TraceInformation($"DecoratedMap: Loaded map '{file}'.");
				MapNameTranslations[tempMap.MapName] = AllMaps.Count;
				AllMaps.Add(tempMap);
			}
		}

		/// <summary>
		/// Creates and returns a new <see cref="DecoratedMap"/>.
		/// </summary>
		public DecoratedMap CreateInitializeMap()
		{
			var returnMap = new DecoratedMap();

			if (returnMap.MapTiles != null && returnMap.MapTiles.GetLength(0) == MapTiles.GetLength(0))
				return returnMap;

			returnMap.MapTiles = new TileType[MapHeight + 2, MapWidth + 2];
			for (var y = 0; y < MapHeight + 2; y++)
			{
				for (var x = 0; x < MapWidth + 2; x++)
				{
					returnMap.MapTiles[y, x] = TileType.None;
				}
			}

			return returnMap;
		}

		public void UpdateMap(VisibilityMap visibilityMap, DecoratedMap resMap)
		{
			if (MapTiles.GetLength(0) != resMap.MapTiles.GetLength(0))
			{
				MapTiles = new TileType[resMap.MapTiles.GetLength(0), resMap.MapTiles.GetLength(1)];
				for (var y = 0; y < MapTiles.GetLength(0); y++)
				{
					for (var x = 0; x < MapTiles.GetLength(1); x++)
					{
						MapTiles[y, x] = TileType.None;
					}
				}
			}

			var toRemove = new List<PlayerAsset>();
			foreach (var asset in Assets)
			{
				bool remove = false;

				if (asset.Speed != 0 || asset.GetAction() == AssetAction.Decay || asset.GetAction() == AssetAction.Attack)
				{
					// Remove all movable units
					toRemove.Add(asset);
					continue;
				}

				for (var yOff = 0; yOff < asset.Data.Size; yOff++)
				{
					var yPos = asset.TilePosition.Y + yOff;

					for (var xOff = 0; xOff < asset.Data.Size; xOff++)
					{
						var xPos = asset.TilePosition.X + xOff;

						var visibilityType = visibilityMap.GetTileType(xPos, yPos);
						if (visibilityType == TileVisibility.Partial || visibilityType == TileVisibility.PartialPartial || visibilityType == TileVisibility.Visible)
						{
							if (asset.Data.Type != AssetType.None)
								remove = true;
							break;
						}
					}
					if (remove)
						break;
				}
				if (remove)
					toRemove.Add(asset);
			}

			foreach (var asset in toRemove)
			{
				Assets.Remove(asset);
			}

			for (var yPos = 0; yPos < MapTiles.GetLength(0); yPos++)
			{
				for (var xPos = 0; xPos < MapTiles.GetLength(1); xPos++)
				{
					var visibilityType = visibilityMap.GetTileType(xPos - 1, yPos - 1);
					if (visibilityType == TileVisibility.Partial || visibilityType == TileVisibility.PartialPartial || visibilityType == TileVisibility.Visible)
						MapTiles[yPos, xPos] = resMap.MapTiles[yPos, xPos];
				}
			}

			var toAdd = new List<PlayerAsset>();
			foreach (var asset in resMap.Assets)
			{
				bool addAsset = false;
				for (var yOff = 0; yOff < asset.Data.Size; yOff++)
				{
					var yPos = asset.TilePosition.Y + yOff;

					for (var xOff = 0; xOff < asset.Data.Size; xOff++)
					{
						var xPos = asset.TilePosition.X + xOff;

						var visibilityType = visibilityMap.GetTileType(xPos, yPos);
						if (visibilityType == TileVisibility.Partial || visibilityType == TileVisibility.PartialPartial || visibilityType == TileVisibility.Visible)
						{
							addAsset = true;
							break;
						}
					}
					if (addAsset)
						break;
				}

				if (addAsset)
					toAdd.Add(asset);
			}

			Assets.AddRange(toAdd);
		}

		/// <summary>
		/// Creates and returns a new <see cref="VisibilityMap"/> based on this map.
		/// </summary>
		public VisibilityMap CreateVisibilityMap()
		{
			return new VisibilityMap(MapWidth, MapHeight, PlayerAssetData.MaxSight);
		}

		/// <summary>
		/// Adds <paramref name="asset"/> to <see cref="Assets"/>.
		/// </summary>
		public void AddAsset(PlayerAsset asset)
		{
			Assets.Add(asset);
		}

		/// <summary>
		/// Removes <paramref name="asset"/> from <see cref="Assets"/>.
		/// </summary>
		public void RemoveAsset(PlayerAsset asset)
		{
			Assets.Remove(asset);
		}

		/// <summary>
		/// Returns the map in <paramref name="index"/> of <see cref="AllMaps"/>.
		/// </summary>
		public static DecoratedMap GetMap(int index)
		{
			if (index < 0 || index >= AllMaps.Count) return new DecoratedMap();
			return AllMaps[index];
		}

		/// <summary>
		/// Creates a duplicate of the map in <paramref name="mapIndex"/>.
		/// </summary>
		public static DecoratedMap DuplicateMap(int mapIndex, List<PlayerColor> newColors)
		{
			if (mapIndex < 0 || mapIndex >= AllMaps.Count)
				return new DecoratedMap();

			return new DecoratedMap(AllMaps[mapIndex], newColors);
		}

		/// <summary>
		/// Returns the position of the nearest tile of <paramref name="type"/>
		/// that can be reached from <paramref name="pos"/>.
		/// </summary>
		public Position FindNearestReachableTileType(Position pos, TileType type)
		{
			var searchQueue = new Queue<Point>();
			Point currentSearch;
			int[] searchXOffsets = { 0, 1, 0, -1 };
			int[] searchYOffsets = { -1, 0, 1, 0 };

			if (SearchMap.GetLength(0) != MapTiles.GetLength(0))
			{
				SearchMap = new int[MapTiles.GetLength(0), MapTiles.GetLength(1)];

				var lastYIndex = MapTiles.GetLength(0) - 1;
				var lastXIndex = MapTiles.GetLength(1) - 1;

				for (var index = 0; index < MapTiles.GetLength(0); index++)
				{
					SearchMap[index, 0] = (int)SearchStatus.Visited;
					SearchMap[index, lastXIndex] = (int)SearchStatus.Visited;
				}

				for (var index = 1; index < lastXIndex; index++)
				{
					SearchMap[0, index] = (int)SearchStatus.Visited;
					SearchMap[lastYIndex, index] = (int)SearchStatus.Visited;
				}
			}

			for (var y = 0; y < MapHeight; y++)
			{
				for (var x = 0; x < MapWidth; x++)
				{
					SearchMap[y + 1, x + 1] = (int)SearchStatus.Unvisited;
				}
			}

			foreach (var asset in Assets)
			{
				if (asset.TilePosition == pos)
					continue;

				for (var y = 0; y < asset.Data.Size; y++)
					for (var x = 0; x < asset.Data.Size; x++)
						SearchMap[asset.TilePosition.Y + y + 1, asset.TilePosition.X + x + 1] = (int)SearchStatus.Visited;
			}

			currentSearch.X = pos.X + 1;
			currentSearch.Y = pos.Y + 1;
			searchQueue.Enqueue(currentSearch);

			while (searchQueue.Count != 0)
			{
				currentSearch = searchQueue.Dequeue();
				SearchMap[currentSearch.Y, currentSearch.X] = (int)SearchStatus.Visited;

				for (var index = 0; index < searchXOffsets.Length; index++)
				{
					Point tempSearch;
					tempSearch.X = currentSearch.X + searchXOffsets[index];
					tempSearch.Y = currentSearch.Y + searchYOffsets[index];

					if (SearchMap[tempSearch.Y, tempSearch.X] != (int)SearchStatus.Unvisited)
						continue;

					var currentTileType = MapTiles[tempSearch.Y, tempSearch.X];

					SearchMap[tempSearch.Y, tempSearch.X] = (int)SearchStatus.Queued;
					if (type == currentTileType)
						return new Position(tempSearch.X - 1, tempSearch.Y - 1);

					if (currentTileType == TileType.Grass || currentTileType == TileType.Dirt || currentTileType == TileType.Stump || currentTileType == TileType.Rubble || currentTileType == TileType.None)
						searchQueue.Enqueue(tempSearch);
				}
			}

			return new Position(-1, -1);
		}

		public Position FindAssetPlacement(PlayerAsset placeAsset, PlayerAsset fromAsset, Position nextTileTarget)
		{
			int bestDistance = -1;
			var bestPosition = new Position(-1, -1);

			var topY = fromAsset.TilePosition.Y - placeAsset.Data.Size;
			var bottomY = fromAsset.TilePosition.Y + fromAsset.Data.Size;
			var leftX = fromAsset.TilePosition.X - placeAsset.Data.Size;
			var rightX = fromAsset.TilePosition.X + fromAsset.Data.Size;

			while (true)
			{
				var skipped = 0;

				int currentDistance;
				if (topY >= 0)
				{
					var toX = MathHelper.Min(rightX, MapWidth - 1);
					for (var currentX = MathHelper.Max(leftX, 0); currentX <= toX; currentX++)
					{
						if (CanPlaceAsset(new Position(currentX, topY), placeAsset.Data.Size, placeAsset))
						{
							var tempPosition = new Position(currentX, topY);
							currentDistance = tempPosition.DistanceSquared(nextTileTarget);

							if (bestDistance == -1 || currentDistance < bestDistance)
							{
								bestDistance = currentDistance;
								bestPosition = tempPosition;
							}
						}
					}
				}
				else
					skipped++;

				if (MapWidth > rightX)
				{
					var toY = MathHelper.Min(bottomY, MapHeight - 1);
					for (var currentY = MathHelper.Max(topY, 0); currentY <= toY; currentY++)
					{
						if (CanPlaceAsset(new Position(rightX, currentY), placeAsset.Data.Size, placeAsset))
						{
							var tempPosition = new Position(rightX, currentY);
							currentDistance = tempPosition.DistanceSquared(nextTileTarget);

							if (bestDistance == -1 || currentDistance < bestDistance)
							{
								bestDistance = currentDistance;
								bestPosition = tempPosition;
							}
						}
					}
				}
				else
					skipped++;

				if (MapHeight > bottomY)
				{
					var toX = MathHelper.Max(leftX, 0);
					for (var currentX = MathHelper.Min(rightX, MapWidth - 1); currentX >= toX; currentX--)
					{
						if (CanPlaceAsset(new Position(currentX, bottomY), placeAsset.Data.Size, placeAsset))
						{
							var tempPosition = new Position(currentX, bottomY);
							currentDistance = tempPosition.DistanceSquared(nextTileTarget);

							if (bestDistance == -1 || currentDistance < bestDistance)
							{
								bestDistance = currentDistance;
								bestPosition = tempPosition;
							}
						}
					}
				}
				else
					skipped++;

				if (leftX >= 0)
				{
					var toY = MathHelper.Max(topY, 0);
					for (var currentY = MathHelper.Min(bottomY, MapHeight - 1); currentY >= toY; currentY--)
					{
						if (CanPlaceAsset(new Position(leftX, currentY), placeAsset.Data.Size, placeAsset))
						{
							var tempPosition = new Position(leftX, currentY);
							currentDistance = tempPosition.DistanceSquared(nextTileTarget);

							if (bestDistance == -1 || currentDistance < bestDistance)
							{
								bestDistance = currentDistance;
								bestPosition = tempPosition;
							}
						}
					}
				}
				else
					skipped++;

				if (skipped == 4)
					break;

				if (bestDistance != -1)
					break;

				topY--;
				bottomY++;
				leftX--;
				rightX++;
			}

			return bestPosition;
		}

		/// <summary>
		/// Returns whether the an asset of size <paramref name="size"/> can be placed at <paramref name="position"/>.
		/// </summary>
		public bool CanPlaceAsset(Position position, int size, PlayerAsset ignoreAsset)
		{
			for (var yOff = 0; yOff < size; yOff++)
			{
				for (var xOff = 0; xOff < size; xOff++)
				{
					var tileTerrainType = GetTileType(position.X + xOff, position.Y + yOff);
					if (tileTerrainType != TileType.Grass && tileTerrainType != TileType.Dirt && tileTerrainType != TileType.Stump && tileTerrainType != TileType.Rubble)
						return false;
				}
			}

			var rightX = position.X + size;
			var bottomY = position.Y + size;

			if (rightX >= MapWidth) return false;
			if (bottomY >= MapHeight) return false;

			foreach (var asset in Assets)
			{
				var offset = asset.Data.Type == AssetType.GoldMine ? 1 : 0;

				if (asset.Data.Type == AssetType.None) continue;
				if (asset == ignoreAsset) continue;
				if (asset.TilePosition.X - offset >= rightX) continue;
				if (asset.TilePosition.X + asset.Data.Size + offset <= position.X) continue;
				if (asset.TilePosition.Y - offset >= bottomY) continue;
				if (asset.TilePosition.Y + asset.Data.Size + offset <= position.Y) continue;

				return false;
			}

			return true;
		}
	}

	public struct InitialResource
	{
		public PlayerColor Color;
		public int Gold;
		public int Lumber;
		public int Stone;
	}

	public struct InitialAsset
	{
		public string Type;
		public PlayerColor Color;
		public Point TilePosition;
	}

	public enum SearchStatus
	{
		Unvisited,
		Queued,
		Visited
	}
}