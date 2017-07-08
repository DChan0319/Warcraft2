using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LuaSharp;
using Warcraft.Ai.Triggers;
using Warcraft.App;
using Warcraft.GameModel;
using Warcraft.Player;
using Warcraft.Player.Capabilities;
using Warcraft.Util;
// ReSharper disable InconsistentNaming

namespace Warcraft.Ai
{
	/// <summary>
	/// C# implementation of Linux's libai
	/// </summary>
	public static class LibAi
	{
		/// <summary>
		/// Current Player Object
		/// </summary>
		private static PlayerData PlayerData { get; set; }

		private static Dictionary<int, PlayerAsset> AllAssetMap { get; set; } = new Dictionary<int, PlayerAsset>();

		private static PlayerCommandRequest Command { get; set; }

		/// <summary>
		/// Loads lua scripts and calls CalculateCommand.
		/// </summary>
		public static void Initialize(AiPlayer player, PlayerCommandRequest command, PlayerType difficulty, int cycle)
		{
			command.Action = AssetCapabilityType.None;
			command.Actors.Clear();
			command.TargetColor = PlayerColor.None;
			command.TargetType = AssetType.None;
			PlayerData = player.PlayerData;
			Command = command;

			var L = Lua.luaL_newstate();

			Register(L);

			// Load AI.lua
			var aiScript = Path.Combine(Paths.Scripts, "AI.lua");
			var check = Lua.luaL_dofile(L, aiScript);
			if (check != 0)
			{
				Trace.TraceInformation($"Lua script: {aiScript} did not load properly.");
				Trace.TraceInformation("Lua Errors:");
				Trace.TraceInformation(Lua.lua_tostring(L, -1));
			}

			// Get difficulty script
			var difficultyScript = Paths.Scripts;
			switch (difficulty)
			{
				case PlayerType.AiEasy: difficultyScript = Path.Combine(difficultyScript, "AIEasy.lua"); break;
				case PlayerType.AiMedium: difficultyScript = Path.Combine(difficultyScript, "AIMed.lua"); break;
				case PlayerType.AiHard: difficultyScript = Path.Combine(difficultyScript, "AIHard.lua"); break;
				default: difficultyScript = Path.Combine(difficultyScript, "AIEasy.lua"); break;
			}

			// Load difficulty script
			check += Lua.luaL_dofile(L, difficultyScript);
			if (check != 0)
			{
				Trace.TraceInformation($"Lua script: {difficultyScript} did not load properly.");
				Trace.TraceInformation("Lua Errors:");
				Trace.TraceInformation(Lua.lua_tostring(L, -1));
			}

			SetupAllAssets();

			// Get function pointer to CalculateCommand
			Lua.lua_getglobal(L, "CalculateCommand");

			Lua.lua_pushinteger(L, cycle);
			Lua.lua_pcall(L, 1, 0, 0); // Call CalculateCommand

			Lua.lua_close(L);

			command.Actors = Command.Actors;
			command.Action = Command.Action;
			command.TargetColor = Command.TargetColor;
			command.TargetType = Command.TargetType;
			command.TargetLocation = Command.TargetLocation;
		}

		/// <summary>
		/// Checks for Triggers.
		/// </summary>
		public static void CheckPlayer(PlayerData player, int cycle)
		{
			PlayerData = player;

			var L = Lua.luaL_newstate();

			Register(L);

			// Load AI.lua
			var aiScript = Path.Combine(Paths.Scripts, "AI.lua");
			var check = Lua.luaL_dofile(L, aiScript);
			if (check != 0)
			{
				Trace.TraceInformation($"Lua script: {aiScript} did not load properly.");
				Trace.TraceInformation("Lua Errors:");
				Trace.TraceInformation(Lua.lua_tostring(L, -1));
			}

			// Load Events.lua
			var eventScript = Path.Combine(Paths.Scripts, "Events.lua");
			check += Lua.luaL_dofile(L, eventScript);
			if (check != 0)
			{
				Trace.TraceInformation($"Lua script: {eventScript} did not load properly.");
				Trace.TraceInformation("Lua Errors:");
				Trace.TraceInformation(Lua.lua_tostring(L, -1));
			}

			// Load Triggers.lua
			var triggerScript = Path.Combine(Paths.Scripts, "Triggers.lua");
			check += Lua.luaL_dofile(L, triggerScript);
			if (check != 0)
			{
				Trace.TraceInformation($"Lua script: {triggerScript} did not load properly.");
				Trace.TraceInformation("Lua Errors:");
				Trace.TraceInformation(Lua.lua_tostring(L, -1));
			}

			SetupAllAssets();

			// Set cycle to a global variable in Lua for triggers
			Lua.luaL_dostring(L, "cycle = " + cycle);

			// Populate the trigger map
			if (!TriggerManager.IsFileOpened && TriggerManager.IsValidTriggerFile)
			{
				var mapName = PlayerData.ActualMap.MapName;
				TriggerManager.ParseTriggerCsv(mapName);
			}

			// Check all the triggers
			if (TriggerManager.IsValidTriggerFile)
			{
				TriggerManager.CheckTriggers(L, (int)PlayerData.Color);
			}

			Lua.lua_close(L);
		}

		/// <summary>
		/// Retrieves a copy of all of the assets in the game.
		/// </summary>
		private static void SetupAllAssets()
		{
			AllAssetMap.Clear();
			AllAssetMap = Data.GameModel.AllAssets.ToDictionary(a => a.Key, a => a.Value);
		}

		/// <summary>
		/// Register commands used by lua scripts.
		/// </summary>
		private static void Register(IntPtr L)
		{
			Lua.luaL_openlibs(L);

			Register(L, print);

			Register(L, GetGold);
			Register(L, SetGold);
			Register(L, GetLumber);
			Register(L, SetLumber);
			Register(L, GetStone);
			Register(L, SetStone);

			Register(L, GetFoodProduction);
			Register(L, GetFoodConsumption);

			Register(L, AddAsset);
			Register(L, RemoveAsset);

			Register(L, GetAssets);
			Register(L, GetIdleAssets);
			Register(L, GetMovableIdleAssets);
			Register(L, GetMovableDefendingIdleAssets);
			Register(L, GetNearestAsset);
			Register(L, GetAssetPosition);
			Register(L, GetAssetTilePosition);

			Register(L, GetAssetGold);
			Register(L, GetAssetLumber);
			Register(L, GetAssetStone);

			Register(L, GetAssetHealth);
			Register(L, SetAssetHealth);
			Register(L, GetAssetDamageTaken);
			Register(L, DamageAsset);

			Register(L, GetType);
			Register(L, GetColor);

			Register(L, IsInterruptible);
			Register(L, AssetHasAction);
			Register(L, GetCurAction);
			Register(L, GetCanApply);
			Register(L, GetCanInitiate);
			Register(L, GetAssetsWithCapability);
			Register(L, AssetHasActiveCapability);
			Register(L, AssetHasCapability);

			Register(L, GetFoundAssetCount);
			Register(L, GetPlayerAssetCount);
			Register(L, GetAssetSpeed);
			Register(L, GetSeenPercent);

			Register(L, GetBestAssetPlacement);
			Register(L, ShiftPosToCenter);

			Register(L, FindNearestReachableTileType);
			Register(L, EnemiesNotDiscovered);
			Register(L, FindNearestEnemy);

			Register(L, TryCommand);

			Register(L, AddActor);
			Register(L, ClearActors);
		}

		/// <summary>
		/// Creates a function reference to a function and registers it with lua with the same name.
		/// </summary>
		private static void Register(IntPtr L, Lua.LuaNativeFunction function)
		{
			Lua.lua_register(L, function.Method.Name, Lua.GetFunctionRef(L, function));
		}

		/// <summary>
		/// Overloads the default lua 'print' function to Trace the output.
		/// </summary>
		private static int print(IntPtr L)
		{
			var nArgs = Lua.lua_gettop(L);

			for (var i = 1; i <= nArgs; i++)
			{
				if (Lua.lua_isstring(L, i))
				{
					Trace.TraceInformation(Lua.lua_tostring(L, i));
				}
			}

			return 0;
		}

		/// <summary>
		/// Pushes the player's gold amount onto the lua stack.
		/// </summary>
		/// <returns>
		/// [Lua] int Amount of gold
		/// </returns>
		private static int GetGold(IntPtr L)
		{
			Lua.lua_pushnumber(L, PlayerData.Gold);
			return 1;
		}

		/// <summary>
		/// Sets the player's gold amount to given value.
		/// 
		/// [Lua] int Amount of gold
		/// </summary>
		/// <returns>
		/// [Lua] int Updated amount of gold
		/// </returns>
		private static int SetGold(IntPtr L)
		{
			var setResource = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);
			PlayerData.Gold = setResource;
			Lua.lua_pushnumber(L, PlayerData.Gold);
			return 1;
		}

		/// <summary>
		/// Pushes the player's lumber amount onto the lua stack.
		/// </summary>
		/// <returns>
		/// [Lua] int Amount of lumber
		/// </returns>
		private static int GetLumber(IntPtr L)
		{
			Lua.lua_pushnumber(L, PlayerData.Lumber);
			return 1;
		}

		/// <summary>
		/// Sets the player's lumber amount to given value.
		/// 
		/// [Lua] int Amount of lumber
		/// </summary>
		/// <returns>
		/// [Lua] int Updated amount of lumber
		/// </returns>
		private static int SetLumber(IntPtr L)
		{
			var setResource = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);
			PlayerData.Lumber = setResource;
			Lua.lua_pushnumber(L, PlayerData.Lumber);
			return 1;
		}

		/// <summary>
		/// Pushes the player's stone amount onto the lua stack.
		/// </summary>
		/// <returns>
		/// [Lua] int Amount of stone
		/// </returns>
		private static int GetStone(IntPtr L)
		{
			Lua.lua_pushnumber(L, PlayerData.Stone);
			return 1;
		}

		/// <summary>
		/// Sets the player's stone amount to given value.
		/// 
		/// [Lua] int Amount of stone
		/// </summary>
		/// <returns>
		/// [Lua] int Updated amount of stone
		/// </returns>
		private static int SetStone(IntPtr L)
		{
			var setResource = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);
			PlayerData.Stone = setResource;
			Lua.lua_pushnumber(L, PlayerData.Stone);
			return 1;
		}

		/// <summary>
		/// Pushes the player's food production amount onto the lua stack.
		/// </summary>
		/// <returns>
		/// [Lua] int Food production amount
		/// </returns>
		private static int GetFoodProduction(IntPtr L)
		{
			Lua.lua_pushnumber(L, PlayerData.FoodProduction);
			return 1;
		}

		/// <summary>
		/// Pushes the player's food consumption amount onto the lua stack.
		/// </summary>
		/// <returns>
		/// [Lua] int Food consumption amount
		/// </returns>
		private static int GetFoodConsumption(IntPtr L)
		{
			Lua.lua_pushnumber(L, PlayerData.FoodConsumption);
			return 1;
		}

		/// <summary>
		/// Adds an asset of the given type to the map, with the best location
		/// closest to the provided location.
		/// 
		/// [Lua] int assetType
		/// [Lua] int tilePositionX
		/// [Lua] int tilePositionY
		/// [Lua] int radius
		/// </summary>
		private static int AddAsset(IntPtr L)
		{
			var assetType = (AssetType)Lua.lua_tointeger(L, 1);

			var tilePositionX = Lua.lua_tointeger(L, 2);
			var tilePositionY = Lua.lua_tointeger(L, 3);
			var radius = Lua.lua_tointeger(L, 4);

			Lua.lua_pop(L, 4);

			PlayerAsset newAsset;
			var tilePosition = new Position(tilePositionX, tilePositionY);

			var newAssetType = PlayerData.AssetDatas[PlayerAssetData.TypeToName(assetType)];
			var placementSize = newAssetType.Size;

			for (var distance = 0; distance <= radius; distance++)
			{
				var leftX = tilePosition.X - distance;
				var topY = tilePosition.Y - distance;
				var rightX = tilePosition.X + distance;
				var bottomY = tilePosition.Y + distance;
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

				if (rightX >= PlayerData.ActualMap.MapWidth)
				{
					rightX = PlayerData.ActualMap.MapWidth - 1;
					rightValid = false;
				}

				if (bottomY >= PlayerData.ActualMap.MapHeight)
				{
					bottomY = PlayerData.ActualMap.MapHeight - 1;
					bottomValid = false;
				}

				if (topValid)
				{
					for (var index = leftX; index <= rightX; index++)
					{
						var tempPosition = new Position(index, topY);
						if (!PlayerData.ActualMap.CanPlaceAsset(tempPosition, placementSize, null)) continue;

						newAsset = PlayerData.CreateAsset(PlayerAssetData.TypeToName(assetType));
						newAsset.SetTilePosition(tempPosition);
						SetupAllAssets();
						return 0;
					}
				}

				if (rightValid)
				{
					for (var index = topY; index <= bottomY; index++)
					{
						var tempPosition = new Position(rightX, index);
						if (!PlayerData.ActualMap.CanPlaceAsset(tempPosition, placementSize, null)) continue;

						newAsset = PlayerData.CreateAsset(PlayerAssetData.TypeToName(assetType));
						newAsset.SetTilePosition(tempPosition);
						SetupAllAssets();
						return 0;
					}
				}

				if (bottomValid)
				{
					for (var index = rightX; index >= leftX; index--)
					{
						var tempPosition = new Position(index, bottomY);
						if (!PlayerData.ActualMap.CanPlaceAsset(tempPosition, placementSize, null)) continue;

						newAsset = PlayerData.CreateAsset(PlayerAssetData.TypeToName(assetType));
						newAsset.SetTilePosition(tempPosition);
						SetupAllAssets();
						return 0;
					}
				}

				if (leftValid)
				{
					for (var index = bottomY; index >= topY; index--)
					{
						var tempPosition = new Position(leftX, index);
						if (!PlayerData.ActualMap.CanPlaceAsset(tempPosition, placementSize, null)) continue;

						newAsset = PlayerData.CreateAsset(PlayerAssetData.TypeToName(assetType));
						newAsset.SetTilePosition(tempPosition);
						SetupAllAssets();
						return 0;
					}
				}
			}

			Trace.TraceInformation($"No valid asset position within radius {radius}.");

			return 0;
		}

		/// <summary>
		/// Removes the asset with the given id.
		/// 
		/// [Lua] int Asset Id
		/// </summary>
		private static int RemoveAsset(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			var asset = AllAssetMap[assetId];
			PlayerData.DeleteAsset(asset);

			return 0;
		}

		/// <summary>
		/// Pushes a table of all asset ids onto the lua stack.
		/// </summary>
		/// <returns>
		/// [Lua] table A table of all asset ids.
		/// </returns>
		private static int GetAssets(IntPtr L)
		{
			return PushAssets(L, PlayerData.Assets.ToList());
		}

		/// <summary>
		/// Pushes a table of all idle asset ids onto the lua stack.
		/// </summary>
		/// <returns>
		/// [Lua] table A table of all idle asset ids.
		/// </returns>
		private static int GetIdleAssets(IntPtr L)
		{
			return PushAssets(L, PlayerData.Assets.Where(a => a.GetAction() == AssetAction.None && a.Data.Type != AssetType.None).ToList());
		}

		/// <summary>
		/// Pushes a table of all movable, idle asset ids onto the lua stack.
		/// </summary>
		/// <returns>
		/// [Lua] table A table of all movable, idle asset ids.
		/// </returns>
		private static int GetMovableIdleAssets(IntPtr L)
		{
			return PushAssets(L, PlayerData.Assets.Where(a => a.GetAction() == AssetAction.None && a.Data.Type != AssetType.None && a.Speed != 0).ToList());
		}
		/// <summary>
		/// Pushes a table of all movable, defending, idle asset ids onto the lua stack.
		/// </summary>
		/// <returns>
		/// [Lua] table A table of all movable, defending, idle asset ids.
		/// </returns>
		private static int GetMovableDefendingIdleAssets(IntPtr L)
		{
			return PushAssets(L, PlayerData.Assets.Where(a => a.GetAction() == AssetAction.StandGround && a.Data.Type != AssetType.None).ToList());
		}

		/// <summary>
		/// Helper function that pushes the given assets' ids onto the lua stack.
		/// </summary>
		private static int PushAssets(IntPtr L, IReadOnlyCollection<PlayerAsset> assets)
		{
			var i = 1;
			Lua.lua_createtable(L, assets.Count, 0);
			var newTable = Lua.lua_gettop(L);

			foreach (var asset in assets)
			{
				Lua.lua_pushinteger(L, asset.Id);
				Lua.lua_rawseti(L, newTable, i);
				i++;
			}

			return 1;
		}

		/// <summary>
		/// Pushes the id of nearest asset of the specified type onto the lua stack.
		/// 
		/// [Lua] int positionX
		/// [Lua] int positionY
		/// [Lua] int assetType
		/// </summary>
		/// <returns>
		/// [Lua] int Asset Id
		/// </returns>
		private static int GetNearestAsset(IntPtr L)
		{
			var positionX = Lua.lua_tointeger(L, 1);
			var positionY = Lua.lua_tointeger(L, 2);
			var assetType = (AssetType)Lua.lua_tointeger(L, 3);

			Lua.lua_pop(L, 3);

			var position = new Position(positionX, positionY);
			var asset = PlayerData.FindNearestAsset(position, assetType);

			if (asset != null)
				Lua.lua_pushnumber(L, asset.Id);
			else
				Lua.lua_pushnumber(L, -1);
			return 1;
		}

		/// <summary>
		/// Pushes the position of the given asset id onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [Lua] int Position X
		/// [Lua] int Position Y
		/// </returns>
		private static int GetAssetPosition(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var position = AllAssetMap[assetId].Position;

			Lua.lua_pushnumber(L, position.X);
			Lua.lua_pushnumber(L, position.Y);
			return 2;
		}

		/// <summary>
		/// Pushes the tile position of the given asset id onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [Lua] int TilePosition X
		/// [Lua] int TilePosition Y
		/// </returns>
		private static int GetAssetTilePosition(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var tilePosition = AllAssetMap[assetId].TilePosition;

			Lua.lua_pushnumber(L, tilePosition.X);
			Lua.lua_pushnumber(L, tilePosition.Y);
			return 2;
		}

		/// <summary>
		/// Pushes the amount of gold of the given asset id onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [lua] int Asset's gold
		/// </returns>
		private static int GetAssetGold(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, asset.Gold);
			return 1;
		}

		/// <summary>
		/// Pushes the amount of lumber of the given asset id onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [lua] int Asset's lumber
		/// </returns>
		private static int GetAssetLumber(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, asset.Lumber);
			return 1;
		}

		/// <summary>
		/// Pushes the amount of stone of the given asset id onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [lua] int Asset's stone
		/// </returns>
		private static int GetAssetStone(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, asset.Stone);
			return 1;
		}

		/// <summary>
		/// Pushes the amount of health of the given asset id onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [lua] int Asset's health
		/// </returns>
		private static int GetAssetHealth(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, asset.Health);
			return 1;
		}

		/// <summary>
		/// Sets the amount of health of the given asset id.
		/// 
		/// [Lua] int assetId
		/// [Lua] int assetHealth
		/// </summary>
		private static int SetAssetHealth(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			var assetHealth = Lua.lua_tointeger(L, 2);
			Lua.lua_pop(L, 2);

			var asset = AllAssetMap[assetId];
			asset.Health += assetHealth;

			return 0;
		}

		/// <summary>
		/// Pushes the difference between full and current health of the given asset id onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [Lua] int Health difference
		/// </returns>
		private static int GetAssetDamageTaken(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushnumber(L, asset.Data.Health - asset.Health);
			return 1;
		}

		/// <summary>
		/// Deals damage to the given asset id and checks if the asset is alive.
		/// 
		/// [Lua] int assetId
		/// [Lua] int damage
		/// </summary>
		private static int DamageAsset(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			var damage = Lua.lua_tointeger(L, 2);
			Lua.lua_pop(L, 2);

			var asset = AllAssetMap[assetId];
			var command = asset.CurrentCommand();

			if (asset.IsAlive)
			{
				asset.Health -= damage;
				if (!asset.IsAlive)
				{
					command.Action = AssetAction.Death;
					asset.ClearCommands();
					asset.PushCommand(command);
					asset.Step = 0;
				}
			}

			return 0;
		}

		/// <summary>
		/// Pushes the asset type of the given asset id onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [Lua] int Asset type
		/// </returns>
		private static int GetType(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, (int)asset.Data.Type);
			return 1;
		}

		/// <summary>
		/// Pushes the asset color of the given asset id onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [Lua] int Asset color
		/// </returns>
		private static int GetColor(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, (int)asset.Data.Color);
			return 1;
		}

		/// <summary>
		/// Pushes the whether the given asset id is performing an interruptible action onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [Lua] int Interruptible
		/// </returns>
		private static int IsInterruptible(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, asset.IsInterruptible ? 1 : 0);
			return 1;
		}

		/// <summary>
		/// Pushes whether the given asset id is performing the given action onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// [Lua] int action
		/// </summary>
		/// <returns>
		/// [Lua] int Whether the asset is performing that action
		/// </returns>
		private static int AssetHasAction(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			var action = (AssetAction)Lua.lua_tointeger(L, 2);
			Lua.lua_pop(L, 2);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, asset.Commands.Any(a => a.Action == action) ? 1 : 0);
			return 1;
		}

		/// <summary>
		/// Pushes the current action of the given asset id onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [Lua] int Action
		/// </returns>
		private static int GetCurAction(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, (int)asset.GetAction());
			return 1;
		}

		/// <summary>
		/// Pushes if the given asset id can apply the given capability onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// [Lua] int type
		/// </summary>
		/// <returns>
		/// [Lua] int Whether the asset can apply the capability
		/// </returns>
		private static int GetCanApply(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			var type = (AssetCapabilityType)Lua.lua_tointeger(L, 2);
			Lua.lua_pop(L, 2);

			var asset = AllAssetMap[assetId];
			var playerCapability = PlayerCapability.FindCapability(type);

			Lua.lua_pushinteger(L, playerCapability != null && playerCapability.CanApply(asset, PlayerData, asset) ? 1 : 0);
			return 1;
		}

		/// <summary>
		/// Pushes if the given asset id can initiate the given capability onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// [Lua] int type
		/// </summary>
		/// <returns>
		/// [Lua] int Whether the asset can initate the capability
		/// </returns>
		private static int GetCanInitiate(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			var type = (AssetCapabilityType)Lua.lua_tointeger(L, 2);
			Lua.lua_pop(L, 2);

			var asset = AllAssetMap[assetId];
			var playerCapability = PlayerCapability.FindCapability(type);

			Lua.lua_pushinteger(L, playerCapability != null && playerCapability.CanInitiate(asset, PlayerData) ? 1 : 0);
			return 1;
		}

		/// <summary>
		/// Pushes a table of all asset ids that can perform the given capability.
		/// 
		/// [Lua] int capability
		/// </summary>
		/// <returns>
		/// [Lua] table Asset ids that can perform the given capability
		/// </returns>
		private static int GetAssetsWithCapability(IntPtr L)
		{
			var capability = (AssetCapabilityType)Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var capableAssets = PlayerData.Assets.Where(a => a.HasCapability(capability)).ToList();
			Lua.lua_createtable(L, capableAssets.Count, 0);
			var capableAssetTable = Lua.lua_gettop(L);

			var i = 0;
			foreach (var asset in capableAssets)
			{
				Lua.lua_pushinteger(L, asset.Id);
				Lua.lua_rawseti(L, capableAssetTable, i);
				i++;
			}

			return 1;
		}

		/// <summary>
		/// Pushes if the given asset id has the given capability active onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// [Lua] int type
		/// </summary>
		/// <returns>
		/// [Lua] int Whether the given capability is active
		/// </returns>
		private static int AssetHasActiveCapability(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			var type = (AssetCapabilityType)Lua.lua_tointeger(L, 2);
			Lua.lua_pop(L, 2);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, asset.HasActiveCapability(type) ? 1 : 0);
			return 1;
		}

		/// <summary>
		/// Pushes if the given asset id has the given capability onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// [Lua] int type
		/// </summary>
		/// <returns>
		/// [Lua] int Whether the asset has the given capability
		/// </returns>
		private static int AssetHasCapability(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			var type = (AssetCapabilityType)Lua.lua_tointeger(L, 2);
			Lua.lua_pop(L, 2);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, asset.HasCapability(type) ? 1 : 0);
			return 1;
		}

		/// <summary>
		/// Pushes the number of assets matching the given asset type.
		/// 
		/// [Lua] int type
		/// </summary>
		/// <returns>
		/// [Lua] int Amount of assets matching type
		/// </returns>
		private static int GetFoundAssetCount(IntPtr L)
		{
			var type = (AssetType)Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			Lua.lua_pushnumber(L, PlayerData.MapAssetCount(a => a.Data.Type == type));
			return 1;
		}

		/// <summary>
		/// Pushes the number of owned assets matching the given asset type.
		/// 
		/// [Lua] int type
		/// </summary>
		/// <returns>
		/// [Lua] int Amount of owned assets matching type
		/// </returns>
		private static int GetPlayerAssetCount(IntPtr L)
		{
			var type = (AssetType)Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			Lua.lua_pushnumber(L, PlayerData.OwnedAssetCount(a => a.Data.Type == type));
			return 1;
		}

		/// <summary>
		/// Pushes the speed of the given asset onto the lua stack.
		/// 
		/// [Lua] int assetId
		/// </summary>
		/// <returns>
		/// [Lua] int Asset speed
		/// </returns>
		private static int GetAssetSpeed(IntPtr L)
		{
			var assetId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			var asset = AllAssetMap[assetId];
			Lua.lua_pushinteger(L, asset.Speed);
			return 1;
		}

		/// <summary>
		/// Pushes the percentage of the map that has been seen onto the lua stack.
		/// </summary>
		/// <returns>
		/// [Lua] int Seen percentage
		/// </returns>
		private static int GetSeenPercent(IntPtr L)
		{
			Lua.lua_pushinteger(L, PlayerData.VisibilityMap.SeenPercent(100));
			return 1;
		}

		/// <summary>
		/// Pushes the position where the asset would be best placed at the given position onto the lua stack.
		/// 
		/// [Lua] int positionX
		/// [Lua] int positionY
		/// [Lua] int assetId
		/// [Lua] int assetType
		/// </summary>
		/// <returns>
		/// [Lua] int Position X
		/// [Lua] int Position Y
		/// </returns>
		private static int GetBestAssetPlacement(IntPtr L)
		{
			var positionX = Lua.lua_tointeger(L, 1);
			var positionY = Lua.lua_tointeger(L, 2);
			var assetId = Lua.lua_tointeger(L, 3);
			var assetType = (AssetType)Lua.lua_tointeger(L, 4);
			Lua.lua_pop(L, 4);

			var position = new Position();
			var tempPosition = new Position(positionX, positionY);
			position.SetToTile(tempPosition);

			var bestPosition = PlayerData.FindBestAssetPlacement(position, AllAssetMap[assetId], assetType, 1);

			Lua.lua_pushnumber(L, bestPosition.X);
			Lua.lua_pushnumber(L, bestPosition.Y);
			return 2;
		}

		/// <summary>
		/// Pushes the position closer to the center of the map onto the lua stack.
		/// 
		/// [Lua] int positionX
		/// [Lua] int positionY
		/// [Lua] int assetId
		/// [Lua] int assetType
		/// [Lua] int townHallAssetId
		/// </summary>
		/// <returns>
		/// [Lua] int Position X
		/// [Lua] int Position Y
		/// </returns>
		private static int ShiftPosToCenter(IntPtr L)
		{
			var positionX = Lua.lua_tointeger(L, 1);
			var positionY = Lua.lua_tointeger(L, 2);
			var assetId = Lua.lua_tointeger(L, 3);
			var assetType = (AssetType)Lua.lua_tointeger(L, 4);
			var townHallAssetId = Lua.lua_tointeger(L, 5);
			Lua.lua_pop(L, 5);

			var position = new Position();
			var tempPosition = new Position(positionX, positionY);
			position.SetToTile(tempPosition);

			var mapCenter = new Position(PlayerData.PlayerMap.MapWidth / 2, PlayerData.PlayerMap.MapHeight / 2);

			if (mapCenter.X < position.X)
				position.X -= AllAssetMap[townHallAssetId].Data.Size / 2;
			else if (mapCenter.X > position.X)
				position.X += AllAssetMap[townHallAssetId].Data.Size / 2;

			if (mapCenter.Y < position.Y)
				position.Y -= AllAssetMap[townHallAssetId].Data.Size / 2;
			else if (mapCenter.Y > position.Y)
				position.Y += AllAssetMap[townHallAssetId].Data.Size / 2;

			var bestPosition = PlayerData.FindBestAssetPlacement(position, AllAssetMap[assetId], assetType, 1);
			bestPosition.SetFromTile(bestPosition);
			Lua.lua_pushnumber(L, bestPosition.X);
			Lua.lua_pushnumber(L, bestPosition.Y);
			return 2;
		}

		/// <summary>
		/// Pushes the position of the nearest tile type that can be reached onto the lua stack.
		/// 
		/// [Lua] int positionX
		/// [Lua] int positionY
		/// [Lua] int tileType
		/// </summary>
		/// <returns>
		/// [Lua] int TilePosition X
		/// [Lua] int TilePosition Y
		/// </returns>
		private static int FindNearestReachableTileType(IntPtr L)
		{
			var positionX = Lua.lua_tointeger(L, 1);
			var positionY = Lua.lua_tointeger(L, 2);
			var tileType = (TileType)Lua.lua_tointeger(L, 3);
			Lua.lua_pop(L, 3);

			var position = new Position();
			var tempPosition = new Position(positionX, positionY);
			position.SetToTile(tempPosition);

			var tilePosition = PlayerData.PlayerMap.FindNearestReachableTileType(position, tileType);
			Lua.lua_pushnumber(L, tilePosition.X);
			Lua.lua_pushnumber(L, tilePosition.Y);
			return 2;
		}

		/// <summary>
		/// Pushes whether there are enemies left on the map onto the lua stack.
		/// 
		/// [Lua] int positionX
		/// [Lua] int positionY
		/// </summary>
		/// <returns>
		/// [Lua] int Whether there are enemies on the map
		/// </returns>
		private static int EnemiesNotDiscovered(IntPtr L)
		{
			var positionX = Lua.lua_tointeger(L, 1);
			var positionY = Lua.lua_tointeger(L, 2);
			Lua.lua_pop(L, 2);

			var position = new Position(positionX, positionY);
			Lua.lua_pushnumber(L, PlayerData.FindNearestEnemy(position, -1) != null ? 1 : 0);
			return 1;
		}

		/// <summary>
		/// Pushes the asset id of the nearest enemy or -1 if there are none onto the lua stack.
		/// 
		/// [Lua] int positionX
		/// [Lua] int positionY
		/// </summary>
		/// <returns>
		/// [Lua] int assetId
		/// </returns>
		private static int FindNearestEnemy(IntPtr L)
		{
			var positionX = Lua.lua_tointeger(L, 1);
			var positionY = Lua.lua_tointeger(L, 2);
			Lua.lua_pop(L, 2);

			var position = new Position(positionX, positionY);
			var targetEnemy = PlayerData.FindNearestEnemy(position, -1);
			Lua.lua_pushnumber(L, targetEnemy?.Id ?? -1);
			return 1;
		}

		/// <summary>
		/// Attempts to perform the given action on the given asset.
		/// 
		/// [Lua] int tilePositionFlag
		/// [Lua] int actionType
		///   [Lua] int targetX
		///   [Lua] int targetY
		///     [Lua] int targetType
		///       [Lua] int targetColor
		/// </summary>
		/// <returns>
		/// [Lua] int 1
		/// </returns>
		private static int TryCommand(IntPtr L)
		{
			var tilePositionFlag = (PositionFlag)Lua.lua_tointeger(L, 1);
			var actionType = (AssetCapabilityType)Lua.lua_tointeger(L, 2);
			var stackSize = Lua.lua_gettop(L);

			Command.Action = actionType;

			if (stackSize > 2)
			{
				var targetX = Lua.lua_tointeger(L, 3);
				var targetY = Lua.lua_tointeger(L, 4);
				var targetPosition = new Position(targetX, targetY);

				switch (tilePositionFlag)
				{
					case PositionFlag.FromTile: Command.TargetLocation.SetFromTile(targetPosition); break;
					case PositionFlag.FromAsset: Command.TargetLocation = targetPosition; break;
				}
			}

			if (stackSize == 6)
			{
				Command.TargetColor = (PlayerColor)Lua.lua_tointeger(L, 6);
				Lua.lua_pop(L, 1);
				stackSize--;
			}

			if (stackSize == 5)
			{
				Command.TargetType = (AssetType)Lua.lua_tointeger(L, 5);
			}

			Lua.lua_pop(L, stackSize);
			Lua.lua_pushnumber(L, 1);
			return 1;
		}

		/// <summary>
		/// Adds an actor to the list of actors for the next command.
		/// 
		/// [Lua] int actorId
		/// </summary>
		private static int AddActor(IntPtr L)
		{
			var actorId = Lua.lua_tointeger(L, 1);
			Lua.lua_pop(L, 1);

			Command.Actors.Add(AllAssetMap[actorId]);

			return 0;
		}

		/// <summary>
		/// Clears the list of actors for the next command.
		/// </summary>
		private static int ClearActors(IntPtr L)
		{
			Command.Actors.Clear();

			return 0;
		}

		private enum PositionFlag
		{
			FromAsset = 1,
			FromTile
		}
	}
}