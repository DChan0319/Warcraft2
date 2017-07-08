using System.Collections.Generic;
using Warcraft.App;

namespace Warcraft.Player
{
	public class PlayerCommandRequest
	{
		public AssetCapabilityType Action { get; set; }
		public List<PlayerAsset> Actors { get; set; }
		public PlayerColor TargetColor { get; set; }
		public AssetType TargetType { get; set; }
		public Position TargetLocation;

		public PlayerCommandRequest()
		{
			Actors = new List<PlayerAsset>();
		}
	}

	public enum AssetCapabilityType
	{
		None,
		BuildPeasant,
		BuildFootman,
		BuildKnight,
		BuildArcher,
		BuildRanger,
		BuildFarm,
		BuildTownHall,
		BuildBarracks,
		BuildLumberMill,
		BuildBlacksmith,
		BuildKeep,
		BuildCastle,
		BuildScoutTower,
		BuildGuardTower,
		BuildCannonTower,
		Move,
		Repair,
		Mine,
		BuildSimple,
		BuildAdvanced,
		Convey,
		Shelter,
		Unshelter,
		Cancel,
		BuildWall,
		Attack,
		StandGround,
		Patrol,
		WeaponUpgrade1,
		WeaponUpgrade2,
		WeaponUpgrade3,
		ArrowUpgrade1,
		ArrowUpgrade2,
		ArrowUpgrade3,
		ArmorUpgrade1,
		ArmorUpgrade2,
		ArmorUpgrade3,
		Longbow,
		RangerScouting,
		Marksmanship,
		Max
	}
}