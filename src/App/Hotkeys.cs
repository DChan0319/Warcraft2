using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Warcraft.Player;

namespace Warcraft.App
{
	public class Hotkeys
	{
		public static readonly Dictionary<Keys, AssetCapabilityType> UnitHotkeys = new Dictionary<Keys, AssetCapabilityType>
		{
			{ Keys.A, AssetCapabilityType.Attack },
			{ Keys.B, AssetCapabilityType.BuildSimple },
			{ Keys.G, AssetCapabilityType.Convey },
			{ Keys.M, AssetCapabilityType.Move },
			{ Keys.P, AssetCapabilityType.Patrol },
			{ Keys.R, AssetCapabilityType.Repair },
			{ Keys.S, AssetCapabilityType.Shelter },
			{ Keys.T, AssetCapabilityType.StandGround }
		};

		public static readonly Dictionary<Keys, AssetCapabilityType> BuildHotkeys = new Dictionary<Keys, AssetCapabilityType>
		{
			{ Keys.B, AssetCapabilityType.BuildBarracks },
			{ Keys.F, AssetCapabilityType.BuildFarm },
			{ Keys.H, AssetCapabilityType.BuildTownHall },
			{ Keys.L, AssetCapabilityType.BuildLumberMill },
			{ Keys.S, AssetCapabilityType.BuildBlacksmith },
			{ Keys.T, AssetCapabilityType.BuildScoutTower },
			{ Keys.W, AssetCapabilityType.BuildWall }
		};

		public static readonly Dictionary<Keys, AssetCapabilityType> BuildingHotkeys = new Dictionary<Keys, AssetCapabilityType>
		{
			{ Keys.A, AssetCapabilityType.BuildArcher },
			{ Keys.F, AssetCapabilityType.BuildFootman },
			{ Keys.K, AssetCapabilityType.BuildKnight },
			{ Keys.P, AssetCapabilityType.BuildPeasant },
			{ Keys.R, AssetCapabilityType.BuildRanger },
			{ Keys.U, AssetCapabilityType.Unshelter }
		};
	}
}