using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Extensions;
using Warcraft.Screens.Manager;

namespace Warcraft.Renderers
{
	public class ListViewRenderer
	{
		protected readonly Tileset IconTileset;
		protected readonly FontTileset Font;
		protected int FontHeight;
		protected int LastItemCount;
		protected int LastItemOffset;
		protected int LastViewWidth;
		protected int LastViewHeight;
		protected bool LastUndisplayed;

		public ListViewRenderer(Tileset icons, FontTileset font)
		{
			IconTileset = icons;
			Font = font;
			FontHeight = 1;
			LastItemCount = 0;
			LastItemOffset = 0;
			LastViewWidth = 0;
			LastViewHeight = 0;
			LastUndisplayed = false;
		}

		/// <summary>
		/// Returns the index of the item at (<paramref name="x"/>, <paramref name="y"/>) in the list view.
		/// </summary>
		public int ItemAt(int x, int y)
		{
			if (x < 0 || y < 0) return (int)ListViewObject.None;
			if (x >= LastViewWidth || y >= LastViewHeight) return (int)ListViewObject.None;

			if (x < LastViewWidth - IconTileset.TileWidth)
			{
				if (y / FontHeight < LastItemCount)
					return LastItemOffset + y / FontHeight;
			}
			else if (y < IconTileset.TileHeight)
			{
				if (LastItemOffset != 0)
					return (int)ListViewObject.UpArrow;
			}
			else if (y > LastViewHeight - IconTileset.TileHeight)
			{
				if (LastUndisplayed)
					return (int)ListViewObject.DownArrow;
			}
			return (int)ListViewObject.None;
		}

		/// <summary>
		/// Draws the list view on to the current render target
		/// </summary>
		public void Draw(int selectedIndex, int offsetIndex, List<string> items)
		{
			var blackIndex = Font.RecolorMap.FindColor("black");
			var whiteIndex = Font.RecolorMap.FindColor("white");
			var goldIndex = Font.RecolorMap.FindColor("gold");
			var textYOffset = 0;

			LastViewWidth = Data.ListViewRenderTarget?.Width ?? 0;
			LastViewHeight = Data.ListViewRenderTarget?.Height ?? 0;

			LastItemCount = 0;
			LastItemOffset = offsetIndex;

			ScreenManager.SpriteBatch.Begin();

			var maxTextWidth = LastViewWidth - IconTileset.TileWidth;
			const uint color = 0x404C0400; // Linux: Swapped R and B bytes (MonoGame is ABGR)
			ScreenManager.SpriteBatch.DrawFilledRectangle(0, 0, LastViewWidth, LastViewHeight, color.ToColor());
			ScreenManager.SpriteBatch.Draw(IconTileset.GetTile(IconTileset.GetIndex(offsetIndex != 0 ? "up-active" : "up-inactive")), new Vector2(maxTextWidth, 0));

			LastUndisplayed = false;
			while (offsetIndex < items.Count && textYOffset < LastViewHeight)
			{
				var temp = items[offsetIndex];

				var textDimensions = Font.MeasureText(temp);
				if (textDimensions.Width >= maxTextWidth)
				{
					while (temp.Length != 0)
					{
						temp = temp.Substring(0, temp.Length - 1);
						textDimensions = Font.MeasureText(temp);
						if (textDimensions.Width < maxTextWidth)
						{
							temp += "...";
							break;
						}
					}
				}

				Font.DrawTextWithShadow(0, textYOffset, offsetIndex == selectedIndex ? whiteIndex : goldIndex, blackIndex, 1, temp);
				FontHeight = textDimensions.Height;
				textYOffset += FontHeight;
				LastItemCount++;
				offsetIndex++;
			}

			if (LastItemCount + LastItemOffset < items.Count)
				LastUndisplayed = true;

			ScreenManager.SpriteBatch.Draw(IconTileset.GetTile(IconTileset.GetIndex(LastUndisplayed ? "down-active" : "down-inactive")), new Vector2(maxTextWidth, LastViewHeight - IconTileset.TileWidth));

			ScreenManager.SpriteBatch.End();
		}
	}

	public enum ListViewObject
	{
		UpArrow = -1,
		DownArrow = -2,
		None = -3
	}
}