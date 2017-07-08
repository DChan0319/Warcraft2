using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Warcraft.App;
using Warcraft.Player.Capabilities;
using Warcraft.Util;

namespace Warcraft.Player
{
	public class PlayerUpgrade
	{
		public string Name { get; private set; }
		public int Armor { get; private set; }
		public int Sight { get; private set; }
		public int Speed { get; private set; }
		public int BasicDamage { get; private set; }
		public int PiercingDamage { get; private set; }
		public int Range { get; private set; }
		public int GoldCost { get; private set; }
		public int LumberCost { get; private set; }
		public int StoneCost { get; private set; }
		public int ResearchTime { get; private set; }
		public List<AssetType> AffectedAssets { get; private set; }
		protected static Dictionary<string, PlayerUpgrade> NameRegistry = new Dictionary<string, PlayerUpgrade>();
		protected static Dictionary<AssetCapabilityType, PlayerUpgrade> TypeRegister = new Dictionary<AssetCapabilityType, PlayerUpgrade>();

		public static void LoadUpgrades()
		{
			Trace.TraceInformation($"{typeof(PlayerUpgrade).Name}: Loading asset upgrades...");
			var sw = new Stopwatch();
			sw.Start();

			var upgradeFiles = Directory.GetFiles(Paths.Upgrade, "*.dat", SearchOption.AllDirectories);
			foreach (var file in upgradeFiles)
			{
				using (var dataFile = new StreamReader(file))
					Load(dataFile);
			}

			sw.Stop();
			Trace.TraceInformation($"{typeof(PlayerUpgrade).Name}: Finished loading asset upgrades in {sw.Elapsed.TotalSeconds:0.000} seconds.");
		}

		private static void Load(TextReader dataFile)
		{
			var name = dataFile.ReadLine();
			if (name == null)
				throw new FormatException("Invalid upgrade file format.");

			var upgradeData = PlayerCapability.NameToType(name);
			if (upgradeData == AssetCapabilityType.None && name != PlayerCapability.TypeToName(AssetCapabilityType.None))
				throw new FormatException($"Unknown upgrade type '{name}'.");

			PlayerUpgrade playerUpgrade;
			if (!NameRegistry.TryGetValue(name, out playerUpgrade))
			{
				playerUpgrade = new PlayerUpgrade
				{
					Name = name,
					AffectedAssets = new List<AssetType>()
				};
				NameRegistry[name] = playerUpgrade;
				TypeRegister[upgradeData] = playerUpgrade;
			}

			var temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid upgrade file format.");
			playerUpgrade.Armor = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid upgrade file format.");
			playerUpgrade.Sight = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid upgrade file format.");
			playerUpgrade.Speed = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid upgrade file format.");
			playerUpgrade.BasicDamage = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid upgrade file format.");
			playerUpgrade.PiercingDamage = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid upgrade file format.");
			playerUpgrade.Range = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid upgrade file format.");
			playerUpgrade.GoldCost = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid upgrade file format.");
			playerUpgrade.LumberCost = int.Parse(temp);

			// Todo: StoneCost

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid upgrade file format.");
			playerUpgrade.ResearchTime = int.Parse(temp);

			temp = dataFile.ReadLine();
			if (temp == null)
				throw new FormatException("Invalid upgrade file format.");
			var affectedAssetCount = int.Parse(temp);
			for (var i = 0; i < affectedAssetCount; i++)
			{
				temp = dataFile.ReadLine();
				if (temp == null)
					throw new FormatException("Invalid upgrade file format.");
				playerUpgrade.AffectedAssets.Add(PlayerAssetData.NameToType(temp));
			}
		}

		public static PlayerUpgrade FindUpgradeByName(string name)
		{
			PlayerUpgrade playerUpgrade;
			if (!NameRegistry.TryGetValue(name, out playerUpgrade))
				return null;

			return playerUpgrade;
		}

		public static PlayerUpgrade FindUpgradeByType(AssetCapabilityType type)
		{
			PlayerUpgrade playerUpgrade;
			if (!TypeRegister.TryGetValue(type, out playerUpgrade))
				return null;

			return playerUpgrade;
		}
	}
}