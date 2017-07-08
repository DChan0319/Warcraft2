using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace LuaSharp
{
	public static partial class Lua
	{
		public const int LUA_MULTRET = -1;

		public const int LUA_TNUMBER = 3;
		public const int LUA_TSTRING = 4;

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int LuaNativeFunction(IntPtr L);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int lua_pcallk(IntPtr L, int nargs, int nresults, int errfunc, int ctx, IntPtr k);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr luaL_newstate();

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void luaL_openlibs(IntPtr L);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void lua_close(IntPtr L);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int luaL_loadbufferx(IntPtr L, [MarshalAs(UnmanagedType.LPStr)] string buff, int size, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string mode);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int luaL_loadfilex(IntPtr L, [MarshalAs(UnmanagedType.LPStr)] string filename, [MarshalAs(UnmanagedType.LPStr)] string mode);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int lua_pushcclosure(IntPtr L, LuaNativeFunction fn, int n);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int lua_type(IntPtr L, int idx);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void lua_createtable(IntPtr L, int narray, int nrec);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int lua_getglobal(IntPtr L, [MarshalAs(UnmanagedType.LPStr)] string name);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int lua_setglobal(IntPtr L, [MarshalAs(UnmanagedType.LPStr)] string name);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void lua_rawseti(IntPtr L, int idx, long n);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int lua_gettop(IntPtr L);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void lua_settop(IntPtr L, int idx);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern bool lua_toboolean(IntPtr L, int index);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void lua_pushinteger(IntPtr L, long n);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void lua_pushnumber(IntPtr L, double n);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern IntPtr lua_tolstring(IntPtr L, int idx, IntPtr len);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern void lua_pushstring(IntPtr L, [MarshalAs(UnmanagedType.LPStr)] string s);

		[DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern int lua_tointegerx(IntPtr L, int index, IntPtr isnum);
	}
}