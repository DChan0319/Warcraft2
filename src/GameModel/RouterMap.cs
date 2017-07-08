using System.Collections.Generic;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Player;

namespace Warcraft.GameModel
{
	public class RouterMap
	{
		private int[,] map = new int[0, 0];

		/// <summary>
		/// Returns whether <paramref name="dir"/> faces the direction opposite of <paramref name="otherDir"/>.
		/// </summary>
		public static bool MovingAway(Direction dir, Direction otherDir)
		{
			if (otherDir < 0 || otherDir >= Direction.Max)
				return false;

			var value = ((int)Direction.Max + (int)otherDir - (int)dir) % (int)Direction.Max;

			return value <= 1 || value >= (int)(Direction.Max - 1);
		}

		/// <summary>
		/// Returns the direction that best moves <paramref name="asset"/> towards <paramref name="target"/>.
		/// </summary>
		public Direction FindRoute(DecoratedMap resMap, PlayerAsset asset, Position target)
		{
			var mapWidth = resMap.MapWidth;
			var mapHeight = resMap.MapHeight;
			var startX = asset.TilePosition.X;
			var startY = asset.TilePosition.Y;
			var bestSearch = new SearchTarget();
			Position targetTile = new Position(), tempTile = new Position();
			Direction[] searchDirections = { Direction.North, Direction.East, Direction.South, Direction.West };
			int[] resMapXOffsets = { 0, 1, 0, -1 };
			int[] resMapYOffsets = { -1, 0, 1, 0 };
			int[] diagCheckXOffsets = { 0, 1, 1, 1, 0, -1, -1, -1 };
			int[] diagCheckYOffsets = { -1, -1, 0, 1, 1, 1, 0, -1 };
			var searchQueue = new Queue<SearchTarget>();

			targetTile.SetToTile(target);
			if (map.GetLength(0) != mapHeight + 2 || map.GetLength(1) != mapWidth + 2)
			{
				var lastYIndex = mapHeight + 1;
				var lastXIndex = mapWidth + 1;
				map = new int[mapHeight + 2, mapWidth + 2];

				for (var y = 0; y < map.GetLength(0); y++)
				{
					map[y, 0] = (int)SearchStatus.Visited;
					map[y, lastXIndex] = (int)SearchStatus.Visited;
				}

				for (var x = 0; x < mapWidth; x++) // Should this be map width?
				{
					map[0, x + 1] = (int)SearchStatus.Visited;
					map[lastYIndex, x + 1] = (int)SearchStatus.Visited;
				}
			}

			if (asset.TilePosition == targetTile)
			{
				var deltaX = target.X - asset.Position.X;
				var deltaY = target.Y - asset.Position.Y;

				if (deltaX > 0)
				{
					if (deltaY > 0) return Direction.NorthEast;
					if (deltaY < 0) return Direction.SouthEast;
					return Direction.East;
				}

				if (deltaX < 0)
				{
					if (deltaY > 0) return Direction.NorthWest;
					if (deltaY < 0) return Direction.SouthWest;
					return Direction.West;
				}

				if (deltaY > 0) return Direction.North;
				if (deltaY < 0) return Direction.South;
				return Direction.Max;
			}

			for (var y = 0; y < mapHeight; y++)
			{
				for (var x = 0; x < mapWidth; x++)
				{
					map[y + 1, x + 1] = (int)SearchStatus.Unvisited;
				}
			}

			foreach (var res in resMap.Assets)
			{
				if (asset == res) continue;
				if (res.Data.Type == AssetType.None) continue;
				if (res.GetAction() != AssetAction.Walk || res.Data.Color != asset.Data.Color)
				{
					if (res.Data.Color == asset.Data.Color && res.PerformingHiddenAction)
						continue;

					for (var yOff = 0; yOff < res.Data.Size; yOff++)
					{
						for (var xOff = 0; xOff < res.Data.Size; xOff++)
						{
							map[res.TilePosition.Y + yOff + 1, res.TilePosition.X + xOff + 1] = (int)SearchStatus.Visited;
						}
					}
				}
				else
				{
					map[res.TilePosition.Y + 1, res.TilePosition.X + 1] = (int)SearchStatus.Occupied - (int)res.Direction;
				}
			}

			var currentTile = asset.TilePosition;
			var currentSearch = new SearchTarget
			{
				X = bestSearch.X = currentTile.X,
				Y = bestSearch.Y = currentTile.Y,
				Steps = 0,
				TargetDistanceSquared = bestSearch.TargetDistanceSquared = currentTile.DistanceSquared(targetTile),
				InDirection = bestSearch.InDirection = Direction.Max
			};
			map[startY + 1, startX + 1] = (int)SearchStatus.Visited;

			while (true)
			{
				if (currentTile == targetTile)
				{
					bestSearch = currentSearch;
					break;
				}

				if (currentSearch.TargetDistanceSquared < bestSearch.TargetDistanceSquared)
				{
					bestSearch = currentSearch;
				}

				for (var i = 0; i < searchDirections.Length; i++)
				{
					tempTile.X = currentSearch.X + resMapXOffsets[i];
					tempTile.Y = currentSearch.Y + resMapYOffsets[i];

					if (map[tempTile.Y + 1, tempTile.X + 1] == (int)SearchStatus.Unvisited
						|| MovingAway(searchDirections[i], (Direction)(SearchStatus.Occupied - map[tempTile.Y + 1, tempTile.X + 1])))
					{
						map[tempTile.Y + 1, tempTile.X + 1] = i;

						var currentTileType = resMap.GetTileType(tempTile.X, tempTile.Y);
						if (currentTileType == TileType.Grass || currentTileType == TileType.Dirt || currentTileType == TileType.Stump || currentTileType == TileType.Rubble || currentTileType == TileType.Seedling || currentTileType == TileType.None)
						{
							SearchTarget tempSearch;
							tempSearch.X = tempTile.X;
							tempSearch.Y = tempTile.Y;
							tempSearch.Steps = currentSearch.Steps + 1;
							tempSearch.TileType = currentTileType;
							tempSearch.TargetDistanceSquared = tempTile.DistanceSquared(targetTile);
							tempSearch.InDirection = searchDirections[i];
							searchQueue.Enqueue(tempSearch);
						}
					}
				}

				if (searchQueue.Count == 0)
					break;

				currentSearch = searchQueue.Dequeue();
				currentTile.X = currentSearch.X;
				currentTile.Y = currentSearch.Y;
			}

			var directionBeforeLast = bestSearch.InDirection;
			var lastInDirection = bestSearch.InDirection;
			currentTile.X = bestSearch.X;
			currentTile.Y = bestSearch.Y;

			while (currentTile.X != startX || currentTile.Y != startY)
			{
				var i = map[currentTile.Y + 1, currentTile.X + 1];
				directionBeforeLast = lastInDirection;
				lastInDirection = searchDirections[i];
				currentTile.X -= resMapXOffsets[i];
				currentTile.Y -= resMapYOffsets[i];
			}

			if (directionBeforeLast != lastInDirection)
			{
				var currentTileType = resMap.GetTileType(startX + diagCheckXOffsets[(int)directionBeforeLast], startY + diagCheckYOffsets[(int)directionBeforeLast]);
				if (currentTileType == TileType.Grass || currentTileType == TileType.Dirt || currentTileType == TileType.Stump || currentTileType == TileType.Rubble || currentTileType == TileType.None)
				{
					var sum = (int)lastInDirection + (int)directionBeforeLast;
					if (sum == 6 && (lastInDirection == Direction.North || directionBeforeLast == Direction.North))
						sum += 8;
					sum /= 2;
					lastInDirection = (Direction)sum;
				}
			}

			return lastInDirection;
		}
	}

	public struct SearchTarget
	{
		public int X;
		public int Y;
		public int Steps;
		public TileType TileType;
		public int TargetDistanceSquared;
		public Direction InDirection;
	}

	public enum SearchStatus
	{
		Occupied = -3,
		Visited,
		Unvisited
	}
}