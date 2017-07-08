namespace Warcraft.App
{
	/// <summary>
	/// Cursor Types
	/// </summary>
	public enum CursorType
	{
		Pointer,
		Inspect,
		ArrowN,
		ArrowE,
		ArrowS,
		ArrowW,
		TargetOff,
		TargetOn,
		Max
	}

	/// <summary>
	/// Directions
	/// </summary>
	public enum Direction
	{
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest,
		Max
	}

	/// <summary>
	/// Player Colors
	/// </summary>
	public enum PlayerColor
	{
		None,
		Blue,
		Red,
		Green,
		Purple,
		Orange,
		Yellow,
		Black,
		White,
		Max
	}

	/// <summary>
	/// Player Types
	/// </summary>
	public enum PlayerType
	{
		None,
		Human,
		AiEasy,
		AiMedium,
		AiHard
	}

	/// <summary>
	/// Tile Types
	/// </summary>
	public enum TileType
	{
		None,
		Grass,
		Dirt,
		Rock,
		Tree,
		Stump,
		Seedling,
		AdolescentTree,
		Water,
		Wall,
		WallDamaged,
		Rubble,
		Max
	}

	/// <summary>
	/// Terrain Types
	/// </summary>
	public enum TerrainType
	{
		None,
		Grass,
		Dirt,
		Rock,
		Tree,
		Stump,
		Seedling,
		AdolescentTree,
		Water,
		Wall,
		WallDamaged,
		PlayerWall,
		Rubble,
		Peasant,
		Footman,
		Knight,
		Archer,
		Ranger,
		GoldMine,
		TownHall,
		Keep,
		Castle,
		Farm,
		Barracks,
		LumberMill,
		Blacksmith,
		ScoutTower,
		GuardTower,
		CannonTower,
		Max
	}

	/// <summary>
	/// Asset Type
	/// </summary>
	public enum AssetType
	{
		None,
		Wall,
		Peasant,
		Footman,
		Knight,
		Archer,
		Ranger,
		GoldMine,
		TownHall,
		Keep,
		Castle,
		Farm,
		Barracks,
		LumberMill,
		Blacksmith,
		ScoutTower,
		GuardTower,
		CannonTower,
		Max
	}

	/// <summary>
	/// Asset Actions
	/// </summary>
	public enum AssetAction
	{
		None,
		Construct,
		Build,
		Repair,
		Walk,
		StandGround,
		Shelter,
		Attack,
		MineGold,
		HarvestLumber,
		QuarryStone,
		ConveyGold,
		ConveyLumber,
		ConveyStone,
		Death,
		Decay,
		Capability
	}

	/// <summary>
	/// Game Event Types
	/// </summary>
	public enum EventType
	{
		None,
		WorkComplete,
		Selection,
		Acknowledge,
		Ready,
		Death,
		Attacked,
		MissileFire,
		MissileHit,
		Harvest,
		Quarry,
		MeleeHit,
		PlaceAction,
		ButtonTick,
		Max
	}

	/// <summary>
	/// UI Components
	/// </summary>
	public enum UIComponent
	{
		None,
		Viewport,
		ViewportBevelN,
		ViewportBevelE,
		ViewportBevelS,
		ViewportBevelW,
		MiniMap,
		UnitDescription,
		UnitAction
	}
}
