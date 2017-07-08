using System;
using System.Collections.Generic;
using System.Linq;
using LuaSharp;
// ReSharper disable InconsistentNaming

namespace Warcraft.Ai.Triggers
{
	public abstract class Trigger
	{
		public string EventName { get; set; }
		public bool IsPersistent { get; set; }
		public bool HasBeenTriggered { get; set; }
		public List<int> EventArgs { get; private set; }
		public int Player { get; set; }

		public Trigger(string eventName, bool isPersistent, int player)
		{
			EventName = eventName;
			IsPersistent = isPersistent;
			EventArgs = new List<int>();
			Player = player;
		}

		public abstract bool CheckTrigger(IntPtr L);

		public void RunEvent(IntPtr L)
		{
			var numArgs = 0;
			Lua.lua_getglobal(L, EventName);
			var eventArgsCopy = EventArgs.ToList();
			eventArgsCopy.Reverse();

			while (eventArgsCopy.Count != 0)
			{
				Lua.lua_pushnumber(L, eventArgsCopy[eventArgsCopy.Count - 1]);
				eventArgsCopy.RemoveAt(eventArgsCopy.Count - 1);
				numArgs++;
			}

			Lua.lua_pcall(L, numArgs, 0, 0);
		}
	}

	public class TimeTrigger : Trigger
	{
		private int TriggerTime { get; set; }

		public TimeTrigger(string eventName, bool isPersistent, int player, int triggerTime) : base(eventName, isPersistent, player)
		{
			TriggerTime = triggerTime;
		}

		public override bool CheckTrigger(IntPtr L)
		{
			Lua.lua_getglobal(L, "TimeTriggerCheck");
			Lua.lua_pushnumber(L, TriggerTime);
			Lua.lua_pcall(L, 1, 1, 0);

			var result = Lua.lua_toboolean(L, -1);
			Lua.lua_pop(L, 1);
			return result;
		}
	}

	public class ResourceTrigger : Trigger
	{
		private string ResourceType { get; set; }
		private int ResourceAmount { get; set; }

		public ResourceTrigger(string eventName, bool isPersistent, int player, string resourceType, int resourceAmount) : base(eventName, isPersistent, player)
		{
			ResourceType = resourceType;
			ResourceAmount = resourceAmount;
		}

		public override bool CheckTrigger(IntPtr L)
		{
			Lua.lua_getglobal(L, "ResourceTriggerCheck");
			Lua.lua_pushstring(L, ResourceType);
			Lua.lua_pushnumber(L, ResourceAmount);
			Lua.lua_pcall(L, 2, 1, 0);

			var result = Lua.lua_toboolean(L, -1);
			Lua.lua_pop(L, 1);
			return result;
		}
	}

	public class LocationTrigger : Trigger
	{
		private int AssetId { get; set; }
		private int X { get; set; }
		private int Y { get; set; }

		public LocationTrigger(string eventName, bool isPersistent, int player, int positionX, int positionY) : base(eventName, isPersistent, player)
		{
			AssetId = -1;
			X = positionX;
			Y = positionY;
		}

		public override bool CheckTrigger(IntPtr L)
		{
			Lua.lua_getglobal(L, "LocationTriggerCheck");
			Lua.lua_pushinteger(L, X);
			Lua.lua_pushinteger(L, Y);
			Lua.lua_pcall(L, 2, 1, 0);

			var result = Lua.lua_toboolean(L, -1);
			Lua.lua_pop(L, 1);
			return result;
		}
	}

	public class AssetObtainedTrigger : Trigger
	{
		private int AssetType { get; set; }
		private int AssetAmount { get; set; }

		public AssetObtainedTrigger(string eventName, bool isPersistent, int player, int assetType, int assetAmount) : base(eventName, isPersistent, player)
		{
			AssetType = assetType;
			AssetAmount = assetAmount;
		}

		public override bool CheckTrigger(IntPtr L)
		{
			Lua.lua_getglobal(L, "AssetObtainedTriggerCheck");
			Lua.lua_pushinteger(L, AssetType);
			Lua.lua_pushinteger(L, AssetAmount);
			Lua.lua_pcall(L, 2, 1, 0);

			var result = Lua.lua_toboolean(L, -1);
			Lua.lua_pop(L, 1);
			return result;
		}
	}

	public class AssetLostTrigger : Trigger
	{
		private int AssetType { get; set; }
		private int AssetAmount { get; set; }

		public AssetLostTrigger(string eventName, bool isPersistent, int player, int assetType, int assetAmount) : base(eventName, isPersistent, player)
		{
			AssetType = assetType;
			AssetAmount = assetAmount;
		}

		public override bool CheckTrigger(IntPtr L)
		{
			Lua.lua_getglobal(L, "AssetLostTriggerCheck");
			Lua.lua_pushinteger(L, AssetType);
			Lua.lua_pushinteger(L, AssetAmount);
			Lua.lua_pcall(L, 2, 1, 0);

			var result = Lua.lua_toboolean(L, -1);
			Lua.lua_pop(L, 1);
			return result;
		}
	}
}