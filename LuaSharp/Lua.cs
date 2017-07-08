using System;
using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace LuaSharp
{
	/// <summary>
	/// A C# Lua-proxy library through pinvoke.
	/// </summary>
	public static partial class Lua
	{
		/// <summary>
		/// File name (without extension) of the library to read from.
		/// </summary>
		private const string Lib = "lua53";

		private static readonly Dictionary<IntPtr, List<LuaNativeFunction>> Functions = new Dictionary<IntPtr, List<LuaNativeFunction>>();

		/// <summary>
		/// Creates a function reference to a function and stores it in <see cref="Functions"/>.
		/// </summary>
		public static LuaNativeFunction GetFunctionRef(IntPtr L, LuaNativeFunction function)
		{
			List<LuaNativeFunction> list;
			if (!Functions.TryGetValue(L, out list) || list == null)
				list = Functions[L] = new List<LuaNativeFunction>();

			var func = new LuaNativeFunction(function);
			list.Add(func);

			return func;
		}
	}
}
