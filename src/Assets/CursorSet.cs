using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Warcraft.Screens.Manager;

namespace Warcraft.Assets
{
	public class CursorSet : Tileset
	{
		List<int> cursorXPoint = new List<int>();
		List<int> cursorYPoint = new List<int>();

		protected override void Load(TextReader dataFile)
		{
			base.Load(dataFile);

			for (var i = 0; i < Count; i++)
			{
				var temp = dataFile.ReadLine();
				if (temp == null)
					throw new FormatException("Invalid cursor data file format.");

				var tokens = temp.Split();
				if (tokens.Length != 2)
					throw new FormatException("Invalid cursor data file format.");

				cursorXPoint.Add(int.Parse(tokens[0]));
				cursorYPoint.Add(int.Parse(tokens[1]));
			}
		}

		public int FindCursor(string cursorName)
		{
			return GetIndex(cursorName);
		}

		public void DrawCursor(int xPos, int yPos, int cursorIndex)
		{
			if (cursorIndex < 0 || cursorIndex >= Count)
				return;

			ScreenManager.SpriteBatch.Draw(GetTile(cursorIndex), new Vector2(xPos - cursorXPoint[cursorIndex], yPos - cursorYPoint[cursorIndex]));
		}
	}
}