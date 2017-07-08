using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities
{
	public abstract class PlayerCapability
	{
		public string Name { get; set; }
		public AssetCapabilityType AssetCapabilityType { get; set; }
		public TargetType TargetType { get; set; }
		public static readonly Dictionary<string, PlayerCapability> NameRegistry = new Dictionary<string, PlayerCapability>();
		public static readonly Dictionary<int, PlayerCapability> TypeRegistry = new Dictionary<int, PlayerCapability>();

		/// <summary>
		/// Contains all capability types.
		/// </summary>
		private static readonly List<string> TypeStrings = new List<string>
		{
			"None",
			"BuildPeasant",
			"BuildFootman",
			"BuildKnight",
			"BuildArcher",
			"BuildRanger",
			"BuildFarm",
			"BuildTownHall",
			"BuildBarracks",
			"BuildLumberMill",
			"BuildBlacksmith",
			"BuildKeep",
			"BuildCastle",
			"BuildScoutTower",
			"BuildGuardTower",
			"BuildCannonTower",
			"Move",
			"Repair",
			"Mine",
			"BuildSimple",
			"BuildAdvanced",
			"Convey",
			"Shelter",
			"Unshelter",
			"Cancel",
			"BuildWall",
			"Attack",
			"StandGround",
			"Patrol",
			"WeaponUpgrade1",
			"WeaponUpgrade2",
			"WeaponUpgrade3",
			"ArrowUpgrade1",
			"ArrowUpgrade2",
			"ArrowUpgrade3",
			"ArmorUpgrade1",
			"ArmorUpgrade2",
			"ArmorUpgrade3",
			"Longbow",
			"RangerScouting",
			"Marksmanship",
		};

		/// <summary>
		/// Contains mappings from capability name to <see cref="AssetCapabilityType"/>s.
		/// </summary>
		private static readonly Dictionary<string, AssetCapabilityType> NameTypeTranslations = new Dictionary<string, AssetCapabilityType>
		{
			{ "None", AssetCapabilityType.None },
			{ "BuildPeasant", AssetCapabilityType.BuildPeasant },
			{ "BuildFootman", AssetCapabilityType.BuildFootman },
			{ "BuildKnight", AssetCapabilityType.BuildKnight },
			{ "BuildArcher", AssetCapabilityType.BuildArcher },
			{ "BuildRanger", AssetCapabilityType.BuildRanger },
			{ "BuildFarm", AssetCapabilityType.BuildFarm },
			{ "BuildTownHall", AssetCapabilityType.BuildTownHall },
			{ "BuildBarracks", AssetCapabilityType.BuildBarracks },
			{ "BuildLumberMill", AssetCapabilityType.BuildLumberMill },
			{ "BuildBlacksmith", AssetCapabilityType.BuildBlacksmith },
			{ "BuildKeep", AssetCapabilityType.BuildKeep },
			{ "BuildCastle", AssetCapabilityType.BuildCastle },
			{ "BuildScoutTower", AssetCapabilityType.BuildScoutTower },
			{ "BuildGuardTower", AssetCapabilityType.BuildGuardTower },
			{ "BuildCannonTower", AssetCapabilityType.BuildCannonTower },
			{ "Move", AssetCapabilityType.Move },
			{ "Repair", AssetCapabilityType.Repair },
			{ "Mine", AssetCapabilityType.Mine },
			{ "BuildSimple", AssetCapabilityType.BuildSimple },
			{ "BuildAdvanced", AssetCapabilityType.BuildAdvanced },
			{ "Convey", AssetCapabilityType.Convey },
			{ "Shelter", AssetCapabilityType.Shelter },
			{ "Unshelter", AssetCapabilityType.Unshelter },
			{ "Cancel", AssetCapabilityType.Cancel },
			{ "BuildWall", AssetCapabilityType.BuildWall },
			{ "Attack", AssetCapabilityType.Attack },
			{ "StandGround", AssetCapabilityType.StandGround },
			{ "Patrol", AssetCapabilityType.Patrol },
			{ "WeaponUpgrade1", AssetCapabilityType.WeaponUpgrade1 },
			{ "WeaponUpgrade2", AssetCapabilityType.WeaponUpgrade2 },
			{ "WeaponUpgrade3", AssetCapabilityType.WeaponUpgrade3 },
			{ "ArrowUpgrade1", AssetCapabilityType.ArrowUpgrade1 },
			{ "ArrowUpgrade2", AssetCapabilityType.ArrowUpgrade2 },
			{ "ArrowUpgrade3", AssetCapabilityType.ArrowUpgrade3 },
			{ "ArmorUpgrade1", AssetCapabilityType.ArmorUpgrade1 },
			{ "ArmorUpgrade2", AssetCapabilityType.ArmorUpgrade2 },
			{ "ArmorUpgrade3", AssetCapabilityType.ArmorUpgrade3 },
			{ "Longbow", AssetCapabilityType.Longbow },
			{ "RangerScouting", AssetCapabilityType.RangerScouting },
			{ "Marksmanship", AssetCapabilityType.Marksmanship }
		};

		protected PlayerCapability(string name, TargetType targetType)
		{
			Name = name;
			AssetCapabilityType = NameToType(name);
			TargetType = targetType;
		}

		public abstract bool CanInitiate(PlayerAsset actor, PlayerData playerData);

		/// <summary>
		/// Returns whether the capability can be applied.
		/// </summary>
		public abstract bool CanApply(PlayerAsset actor, PlayerData playerData, PlayerAsset target);

		/// <summary>
		/// Applies the capability.
		/// </summary>
		public abstract bool Apply(PlayerAsset actor, PlayerData playerData, PlayerAsset target);

		/// <summary>
		/// Adds the <paramref name="capability"/> into the registry.
		/// </summary>
		[UsedImplicitly]
		public static bool Register(PlayerCapability capability)
		{
			Trace.TraceInformation($"PlayerCapability: Registering capability '{capability.Name}'...");
			if (FindCapability(capability.Name) != null)
				return false;

			NameRegistry[capability.Name] = capability;
			TypeRegistry[(int)NameToType(capability.Name)] = capability;

			return true;
		}

		/// <summary>
		/// Returns the capability matching <paramref name="type"/>,
		/// or throws an exception if it does not exist.
		/// </summary>
		public static PlayerCapability FindCapability(AssetCapabilityType type)
		{
			return TypeRegistry.ContainsKey((int)type) ? TypeRegistry[(int)type] : null;
		}

		/// <summary>
		/// Returns the capability matching <paramref name="name"/>
		/// or throws an exception if it does not exist.
		/// </summary>
		private static PlayerCapability FindCapability(string name)
		{
			return NameRegistry.ContainsKey(name) ? NameRegistry[name] : null;
		}

		/// <summary>
		/// Returns the <see cref="AssetCapabilityType"/> matching <paramref name="name"/>
		/// or throws an exception if it does not exist.
		/// </summary>
		public static AssetCapabilityType NameToType(string name)
		{
			return NameTypeTranslations[name];
		}

		/// <summary>
		/// Returns the name of the capability matching <paramref name="type"/>
		/// or an empty string if it does not exist.
		/// </summary>
		public static string TypeToName(AssetCapabilityType type)
		{
			if (type < 0 || (int)type >= TypeStrings.Count)
				return string.Empty;

			return TypeStrings[(int)type];
		}
	}

	/// <summary>
	/// Player Capability Registrant Attribute
	/// </summary>
	/// <remarks>
	/// We need this because C# does not initialize static members
	/// until the class is instantiated or the member is referenced.
	/// Using this attribute, we can use Reflection to find all capabilities
	/// and register them at that time.
	/// In C++, static members are initialized at the start of execution.
	/// </remarks>
	public class PlayerCapabilityRegistrant : Attribute { }

	public enum TargetType
	{
		None,
		Asset,
		Terrain,
		TerrainOrAsset,
		Player
	}
}