using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Extensions;
using Warcraft.Player;

namespace Warcraft.Renderers
{
	public class PixelType
	{
		public readonly PlayerColor Color;
		public readonly TerrainType Type;

		/// <summary>
		/// Returns an AssetType based on the <see cref="Type"/>.
		/// </summary>
		public AssetType AssetType
		{
			get
			{
				switch (Type)
				{
					case TerrainType.PlayerWall: return AssetType.Wall;
					case TerrainType.Peasant: return AssetType.Peasant;
					case TerrainType.Footman: return AssetType.Footman;
					case TerrainType.Knight: return AssetType.Knight;
					case TerrainType.Archer: return AssetType.Archer;
					case TerrainType.Ranger: return AssetType.Ranger;
					case TerrainType.GoldMine: return AssetType.GoldMine;
					case TerrainType.TownHall: return AssetType.TownHall;
					case TerrainType.Keep: return AssetType.Keep;
					case TerrainType.Castle: return AssetType.Castle;
					case TerrainType.Farm: return AssetType.Farm;
					case TerrainType.Barracks: return AssetType.Barracks;
					case TerrainType.LumberMill: return AssetType.LumberMill;
					case TerrainType.Blacksmith: return AssetType.Blacksmith;
					case TerrainType.ScoutTower: return AssetType.ScoutTower;
					case TerrainType.GuardTower: return AssetType.GuardTower;
					case TerrainType.CannonTower: return AssetType.CannonTower;
					default: return AssetType.None;
				}
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="PixelType"/>
		/// based on <paramref name="r"/> and <paramref name="g"/>.
		/// </summary>
		public PixelType(int r, int g)
		{
			Color = (PlayerColor)r;
			Type = (TerrainType)g;
		}

		/// <summary>
		/// Creates a new instance of <see cref="PixelType"/>
		/// based on <paramref name="asset"/>.
		/// </summary>
		public PixelType(PlayerAsset asset)
		{
			switch (asset.Data.Type)
			{
				case AssetType.Wall: Type = TerrainType.PlayerWall; break;
				case AssetType.Peasant: Type = TerrainType.Peasant; break;
				case AssetType.Footman: Type = TerrainType.Footman; break;
				case AssetType.Knight: Type = TerrainType.Knight; break;
				case AssetType.Archer: Type = TerrainType.Archer; break;
				case AssetType.Ranger: Type = TerrainType.Ranger; break;
				case AssetType.GoldMine: Type = TerrainType.GoldMine; break;
				case AssetType.TownHall: Type = TerrainType.TownHall; break;
				case AssetType.Keep: Type = TerrainType.Keep; break;
				case AssetType.Castle: Type = TerrainType.Castle; break;
				case AssetType.Farm: Type = TerrainType.Farm; break;
				case AssetType.Barracks: Type = TerrainType.Barracks; break;
				case AssetType.LumberMill: Type = TerrainType.LumberMill; break;
				case AssetType.Blacksmith: Type = TerrainType.Blacksmith; break;
				case AssetType.ScoutTower: Type = TerrainType.ScoutTower; break;
				case AssetType.GuardTower: Type = TerrainType.GuardTower; break;
				case AssetType.CannonTower: Type = TerrainType.CannonTower; break;
				default: Type = TerrainType.None; break;
			}

			Color = asset.Data.Color;
		}

		/// <summary>
		/// Creates a new instance of <see cref="PixelType"/>
		/// based on <paramref name="tileType"/>.
		/// </summary>
		public PixelType(TileType tileType)
		{
			Color = PlayerColor.None;

			switch (tileType)
			{
				case TileType.Grass: Type = TerrainType.Grass; break;
				case TileType.Dirt: Type = TerrainType.Dirt; break;
				case TileType.Rock: Type = TerrainType.Rock; break;
				case TileType.Tree: Type = TerrainType.Tree; break;
				case TileType.Stump: Type = TerrainType.Stump; break;
				case TileType.Seedling: Type = TerrainType.Seedling; break;
				case TileType.AdolescentTree: Type = TerrainType.AdolescentTree; break;
				case TileType.Water: Type = TerrainType.Water; break;
				case TileType.Wall: Type = TerrainType.Wall; break;
				case TileType.WallDamaged: Type = TerrainType.WallDamaged; break;
				case TileType.Rubble: Type = TerrainType.Rubble; break;
				default: Type = TerrainType.Max; break;
			}
		}

		/// <summary>
		/// Returns the integral value of <see cref="Color"/>.
		/// </summary>
		public Color ToPixelColor()
		{
			var result = (uint)Color;
			result |= (uint)Type << 8;
			result |= 0xFF000000;
			return result.ToColor();
		}

		/// <summary>
		/// Returns a <see cref="PixelType"/> based on the
		/// point <paramref name="p"/> of the screen.
		/// </summary>
		public static PixelType GetPixelType(Position p)
		{
			return GetPixelType(p.X, p.Y);
		}

		/// <summary>
		/// Returns a <see cref="PixelType"/> based on the
		/// (<paramref name="x"/>, <paramref name="y"/>) position of the screen.
		/// </summary>
		private static PixelType GetPixelType(int x, int y)
		{
			var data = new Color[1];
			Data.TypeRenderTarget.GetData(0, new Rectangle(x, y, 1, 1), data, 0, 1);
			return new PixelType(data[0].R, data[0].G);
		}
	}
}