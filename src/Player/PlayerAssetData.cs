using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Warcraft.App;
using Warcraft.Assets.Base;
using Warcraft.Player.Capabilities;
using Warcraft.Util;

namespace Warcraft.Player
{
	public class PlayerAssetData : Asset
	{
		// Linux: Do we really need a pointer back to itself ??
		public string Name { get; private set; }
		public AssetType Type { get; private set; }
		public PlayerColor Color { get; private set; }
		public List<bool> Capabilities { get; private set; }
		public List<AssetType> AssetRequirements { get; private set; }
		public List<PlayerUpgrade> AssetUpgrades { get; private set; }
		public int Health { get; private set; }
		public int Armor { get; private set; }
		public int Sight { get; private set; }
		public int ConstructionSight { get; private set; }
		public int Size { get; private set; }
		public int Speed { get; private set; }
		public int GoldCost { get; private set; }
		public int LumberCost { get; private set; }
		public int StoneCost { get; private set; }
		public int FoodConsumption { get; private set; }
		public int BuildTime { get; private set; }
		public int AttackSteps { get; set; }
		public int ReloadSteps { get; set; }
		public int BasicDamage { get; set; }
		public int PiercingDamage { get; set; }
		public int Range { get; set; }

		/// <summary>
		/// Returns the max sight value in the registry.
		/// </summary>
		public static int MaxSight { get { return Registry.Values.Max(a => a.Sight); } }

		[JsonIgnore]
		public int ArmorUpgrade { get { return AssetUpgrades.Sum(upgrade => upgrade.Armor); } }

		[JsonIgnore]
		public int SightUpgrade { get { return AssetUpgrades.Sum(upgrade => upgrade.Sight); } }

		[JsonIgnore]
		public int SpeedUpgrade { get { return AssetUpgrades.Sum(upgrade => upgrade.Speed); } }

		[JsonIgnore]
		public int BasicDamageUpgrade { get { return AssetUpgrades.Sum(upgrade => upgrade.BasicDamage); } }

		[JsonIgnore]
		public int PiercingDamageUpgrade { get { return AssetUpgrades.Sum(upgrade => upgrade.PiercingDamage); } }

		[JsonIgnore]
		public int RangeUpgrade { get { return AssetUpgrades.Sum(upgrade => upgrade.Range); } }

		protected static Dictionary<string, PlayerAssetData> Registry { get; } = new Dictionary<string, PlayerAssetData>();

		protected static readonly List<string> TypeStrings = new List<string>
		{
			"None",
			"Wall",
			"Peasant",
			"Footman",
			"Knight",
			"Archer",
			"Ranger",
			"GoldMine",
			"TownHall",
			"Keep",
			"Castle",
			"Farm",
			"Barracks",
			"LumberMill",
			"Blacksmith",
			"ScoutTower",
			"GuardTower",
			"CannonTower"
		};

		protected static readonly Dictionary<string, AssetType> NameTypeTranslations = new Dictionary<string, AssetType>
		{
			{ "None", AssetType.None },
			{ "Wall", AssetType.Wall },
			{ "Peasant", AssetType.Peasant },
			{ "Footman", AssetType.Footman },
			{ "Knight", AssetType.Knight },
			{ "Archer", AssetType.Archer },
			{ "Ranger", AssetType.Ranger },
			{ "GoldMine", AssetType.GoldMine },
			{ "TownHall", AssetType.TownHall },
			{ "Keep", AssetType.Keep },
			{ "Castle", AssetType.Castle },
			{ "Farm", AssetType.Farm },
			{ "Barracks", AssetType.Barracks },
			{ "LumberMill", AssetType.LumberMill },
			{ "Blacksmith", AssetType.Blacksmith },
			{ "ScoutTower", AssetType.ScoutTower },
			{ "GuardTower", AssetType.GuardTower },
			{ "CannonTower", AssetType.CannonTower }
		};

		public PlayerAssetData()
		{
			Capabilities = new List<bool>();
			AssetRequirements = new List<AssetType>();
			AssetUpgrades = new List<PlayerUpgrade>();
			Health = 1;
			Armor = 0;
			Sight = 0;
			ConstructionSight = 0;
			Size = 1;
			Speed = 0;
			GoldCost = 0;
			LumberCost = 0;
			StoneCost = 0;
			FoodConsumption = 0;
			BuildTime = 0;
			AttackSteps = 0;
			ReloadSteps = 0;
			BasicDamage = 0;
			PiercingDamage = 0;
			Range = 0;
		}

		private PlayerAssetData(PlayerAssetData other)
		{
			if (other == null)
				return;

			Name = other.Name;
			Type = other.Type;
			Color = other.Color;
			Capabilities = other.Capabilities.ToList();
			AssetRequirements = other.AssetRequirements.ToList();
			AssetUpgrades = other.AssetUpgrades.ToList();
			Health = other.Health;
			Armor = other.Armor;
			Sight = other.Sight;
			ConstructionSight = other.ConstructionSight;
			Size = other.Size;
			Speed = other.Speed;
			GoldCost = other.GoldCost;
			LumberCost = other.LumberCost;
			StoneCost = other.StoneCost;
			FoodConsumption = other.FoodConsumption;
			BuildTime = other.BuildTime;
			AttackSteps = other.AttackSteps;
			ReloadSteps = other.ReloadSteps;
			BasicDamage = other.BasicDamage;
			PiercingDamage = other.PiercingDamage;
			Range = other.Range;
		}

		/// <summary>
		/// Loads all resource files from the resources directory.
		/// </summary>
		public static void LoadTypes()
		{
			var resourceFiles = Directory.GetFiles(Paths.Resource, "*.dat", SearchOption.AllDirectories);
			foreach (var file in resourceFiles)
				new PlayerAssetData().Load(file);

			Registry["None"] = new PlayerAssetData
			{
				Name = "None",
				Type = AssetType.None,
				Color = PlayerColor.None,
				Health = 256
			};
		}

		/// <summary>
		/// Reads from <paramref name="dataFile"/> and creates a player asset data.
		/// </summary>
		protected override void Load(TextReader dataFile)
		{
			PlayerAssetData playerAssetData;

			var name = dataFile.ReadLine();
			if (name == null)
				throw new FormatException("Invalid resource file format.");

			AssetType assetType;
			if (!Enum.TryParse(name, out assetType))
				throw new Exception($"Unknown asset type ({name}).");

			if (Registry.ContainsKey(name))
				playerAssetData = Registry[name];
			else
			{
				playerAssetData = new PlayerAssetData { Name = name };
				Registry[name] = playerAssetData;
			}
			playerAssetData.Type = assetType;
			playerAssetData.Color = PlayerColor.None;

			var temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.Health = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.Armor = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.Sight = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.ConstructionSight = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.Size = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.Speed = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.GoldCost = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.LumberCost = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.StoneCost = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.FoodConsumption = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.BuildTime = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.AttackSteps = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.ReloadSteps = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.BasicDamage = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.PiercingDamage = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			playerAssetData.Range = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			var capabilityCount = int.Parse(temp);
			playerAssetData.Capabilities = new List<bool>(new bool[(int)AssetCapabilityType.Max]);
			for (var i = 0; i < capabilityCount; i++)
			{
				temp = dataFile.ReadLine();
				if (temp == null)
					throw new FormatException("Invalid resource file format.");
				playerAssetData.AddCapability(PlayerCapability.NameToType(temp));
			}

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid resource file format.");
			var assetRequirementCount = int.Parse(temp);
			for (var i = 0; i < assetRequirementCount; i++)
			{
				temp = dataFile.ReadLine();
				if (temp == null)
					throw new FormatException("Invalid resource file format.");
				playerAssetData.AssetRequirements.Add(NameToType(temp));
			}
		}

		/// <summary>
		/// Returns a copy of <see cref="Registry"/>.
		/// </summary>
		public static Dictionary<string, PlayerAssetData> DuplicateRegistry(PlayerColor color)
		{
			var returnRegistry = new Dictionary<string, PlayerAssetData>();

			// Todo: Check if ToDictionary will give a deep enough copy. Might be faster.
			foreach (var entry in Registry)
				returnRegistry[entry.Key] = new PlayerAssetData(entry.Value) { Color = color };

			return returnRegistry;
		}

		/// <summary>
		/// Gets the default values for the <paramref name="assetName"/>.
		/// </summary>
		public static PlayerAssetData FindDefaultFromName(string assetName)
		{
			return Registry.ContainsKey(assetName) ? Registry[assetName] : new PlayerAssetData();
		}

		/// <summary>
		/// Gets the default values for <paramref name="type"/>.
		/// </summary>
		public static PlayerAssetData FindDefaultFromType(AssetType type)
		{
			return FindDefaultFromName(TypeToName(type));
		}

		/// <summary>
		/// Converts <paramref name="name"/> to an <see cref="AssetType"/>
		/// or returns <see cref="AssetType.None"/> if it does not exist.
		/// </summary>
		public static AssetType NameToType(string name)
		{
			return NameTypeTranslations.ContainsKey(name) ? NameTypeTranslations[name] : AssetType.None;
		}

		/// <summary>
		/// Gets the string name for <paramref name="type"/>
		/// or <see cref="string.Empty"/> if it does not exist.
		/// </summary>
		public static string TypeToName(AssetType type)
		{
			if (type < 0 || (int)type >= TypeStrings.Count)
				return string.Empty;
			return TypeStrings[(int)type];
		}

		#region Capabilities

		/// <summary>
		/// Adds the <paramref name="capability"/> to the current asset data.
		/// </summary>
		public void AddCapability(AssetCapabilityType capability)
		{
			if (capability < 0 || (int)capability >= Capabilities.Count)
				return;

			Capabilities[(int)capability] = true;
		}

		/// <summary>
		/// Removes the <paramref name="capability"/> from the current asset data.
		/// </summary>
		public void RemoveCapability(AssetCapabilityType capability)
		{
			if (capability < 0 || (int)capability >= Capabilities.Count)
				return;

			Capabilities[(int)capability] = false;
		}

		/// <summary>
		/// Returns whether this asset data has <paramref name="capability"/>.
		/// </summary>
		public bool HasCapability(AssetCapabilityType capability)
		{
			if (capability < 0 || (int)capability >= Capabilities.Count)
				return false;

			return Capabilities[(int)capability];
		}

		/// <summary>
		/// Returns a list of <see cref="AssetCapabilityType"/> that this asset
		/// can perform.
		/// </summary>
		public IEnumerable<AssetCapabilityType> GetCapabilities()
		{
			var result = new List<AssetCapabilityType>();

			for (var capability = (int)AssetCapabilityType.None; capability < (int)AssetCapabilityType.Max; capability++)
			{
				if (Capabilities[capability])
					result.Add((AssetCapabilityType)capability);
			}

			return result;
		}

		#endregion

		#region Upgrades

		/// <summary>
		/// Adds <see cref="upgrade"/> to <see cref="AssetUpgrades"/>.
		/// </summary>
		public void AddUpgrade(PlayerUpgrade upgrade)
		{
			AssetUpgrades.Add(upgrade);
		}

		#endregion
	}
}