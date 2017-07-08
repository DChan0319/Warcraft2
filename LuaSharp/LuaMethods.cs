using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace LuaSharp
{
	public static partial class Lua
	{
		public static void lua_register(IntPtr L, string n, LuaNativeFunction f)
		{
			lua_pushcfunction(L, f);
			lua_setglobal(L, n);
		}

		public static int lua_pcall(IntPtr L, int nargs, int nresults, int errfunc)
		{
			return lua_pcallk(L, nargs, nresults, errfunc, 0, IntPtr.Zero);
		}

		public static int luaL_dofile(IntPtr L, string fn)
		{
			return luaL_loadfilex(L, fn, null) != 0 || lua_pcall(L, 0, LUA_MULTRET, 0) != 0 ? 1 : 0;
		}

		public static int luaL_dostring(IntPtr L, string s)
		{
			return (luaL_loadstring(L, s) != 0 || lua_pcall(L, 0, LUA_MULTRET, 0) != 0) ? 1 : 0;
		}

		public static int luaL_loadstring(IntPtr L, string s)
		{
			return luaL_loadbufferx(L, s, s.Length, s, null);
		}

		public static void lua_pushcfunction(IntPtr L, LuaNativeFunction f)
		{
			lua_pushcclosure(L, f, 0);
		}

		public static void lua_pop(IntPtr L, int n)
		{
			lua_settop(L, -n - 1);
		}

		public static int lua_tointeger(IntPtr L, int i)
		{
			return lua_tointegerx(L, i, IntPtr.Zero);
		}

		public static string lua_tostring(IntPtr L, int i)
		{
			var ptr = lua_tolstring(L, i, IntPtr.Zero);
			var val = Marshal.PtrToStringAnsi(ptr);
			return val;
		}

		public static bool lua_isstring(IntPtr L, int index)
		{
			var type = lua_type(L, index);
			return type == LUA_TSTRING || type == LUA_TNUMBER;
		}
	}
}