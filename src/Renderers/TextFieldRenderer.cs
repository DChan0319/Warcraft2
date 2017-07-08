using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Extensions;
using Warcraft.Screens.Components;
using Warcraft.Screens.Manager;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Warcraft.Renderers
{
	public class TextFieldRenderer
	{
		protected RecolorMap ColorMap { get; }
		protected Bevel InnerBevel { get; }
		protected List<int> LightIndices { get; }
		protected List<int> DarkIndices { get; }
		protected FontTileset Font { get; }
		protected PlayerColor BackgroundColor { get; set; }
		protected string Text { get; private set; }
		protected bool IsValid { get; set; }
		protected int MinimumCharacters { get; set; }
		protected Size MinimumSize;
		public Size Size;
		protected int WhiteIndex { get; }
		protected int GoldIndex { get; }
		protected int RedIndex { get; }
		protected int BlackIndex { get; }

		public TextFieldRenderer(RecolorMap colorMap, Bevel innerBevel, FontTileset font)
		{
			ColorMap = colorMap;
			InnerBevel = innerBevel;
			Font = font;
			BackgroundColor = PlayerColor.None;

			LightIndices = new List<int>(new int[(int)PlayerColor.Max]);
			LightIndices[(int)PlayerColor.None] = LightIndices[(int)PlayerColor.Blue] = ColorMap.FindColor("blue-light");
			LightIndices[(int)PlayerColor.Red] = ColorMap.FindColor("red-light");
			LightIndices[(int)PlayerColor.Green] = ColorMap.FindColor("green-light");
			LightIndices[(int)PlayerColor.Purple] = ColorMap.FindColor("purple-light");
			LightIndices[(int)PlayerColor.Orange] = ColorMap.FindColor("orange-light");
			LightIndices[(int)PlayerColor.Yellow] = ColorMap.FindColor("yellow-light");
			LightIndices[(int)PlayerColor.Black] = ColorMap.FindColor("black-light");
			LightIndices[(int)PlayerColor.White] = ColorMap.FindColor("white-light");

			DarkIndices = new List<int>(new int[(int)PlayerColor.Max]);
			DarkIndices[(int)PlayerColor.None] = LightIndices[(int)PlayerColor.Blue] = ColorMap.FindColor("blue-dark");
			DarkIndices[(int)PlayerColor.Red] = ColorMap.FindColor("red-dark");
			DarkIndices[(int)PlayerColor.Green] = ColorMap.FindColor("green-dark");
			DarkIndices[(int)PlayerColor.Purple] = ColorMap.FindColor("purple-dark");
			DarkIndices[(int)PlayerColor.Orange] = ColorMap.FindColor("orange-dark");
			DarkIndices[(int)PlayerColor.Yellow] = ColorMap.FindColor("yellow-dark");
			DarkIndices[(int)PlayerColor.Black] = ColorMap.FindColor("black-dark");
			DarkIndices[(int)PlayerColor.White] = ColorMap.FindColor("white-dark");

			WhiteIndex = Font.RecolorMap.FindColor("white");
			GoldIndex = Font.RecolorMap.FindColor("gold");
			RedIndex = Font.RecolorMap.FindColor("red");
			BlackIndex = Font.RecolorMap.FindColor("black");

			Size = new Size(0, 0);
			SetMinimumCharacters(16);
		}

		public void SetText(string text, bool valid)
		{
			Text = text;
			IsValid = valid;
		}

		public void SetMinimumCharacters(int minCharacters)
		{
			if (minCharacters > 0)
			{
				var sb = new StringBuilder();
				for (var i = 0; i < minCharacters; i++)
				{
					sb.Append("X");
				}

				sb.Append("|");
				var tempTextDimensions = Font.MeasureText(sb.ToString());
				MinimumCharacters = minCharacters;

				MinimumSize.Width = tempTextDimensions.Width + InnerBevel.Width * 2;
				MinimumSize.Height = tempTextDimensions.Height + InnerBevel.Width * 2;

				Size.Width = MathHelper.Max(Size.Width, MinimumSize.Width);
				Size.Height = MathHelper.Max(Size.Height, MinimumSize.Height);
			}
		}

		public void SetWidth(int width)
		{
			if (width >= MinimumSize.Width)
				Size.Width = width;
		}

		public void SetHeight(int height)
		{
			if (height >= MinimumSize.Height)
				Size.Height = height;
		}

		public void DrawTextField(int x, int y, int cursorPosition)
		{
			ScreenManager.SpriteBatch.DrawFilledRectangle(x, y, Size.Width, Size.Height, ColorMap.ColorValue(cursorPosition >= 0 ? LightIndices[(int)BackgroundColor] : DarkIndices[(int)BackgroundColor], 0).ToColor());

			string renderText;
			Rectangle textDimensions;
			if (cursorPosition >= 0)
			{
				if (Text.Length != 0)
				{
					var removeCharacters = false;

					renderText = Text.Substring(0, cursorPosition);
					renderText += "|";
					do
					{
						textDimensions = Font.MeasureText(renderText);
						textDimensions.Width += InnerBevel.Width * 2;
						if (textDimensions.Width > Size.Width)
						{
							renderText = renderText.Substring(1, renderText.Length - 1);
							removeCharacters = true;
						}
					} while (Size.Width < textDimensions.Width);

					if (!removeCharacters)
					{
						if (cursorPosition < Text.Length)
						{
							renderText += Text.Substring(cursorPosition);
						}
					}
				}
				else
				{
					renderText = "|";
				}
			}
			else
			{
				renderText = Text;
			}

			do
			{
				textDimensions = Font.MeasureText(renderText);
				textDimensions.Width += InnerBevel.Width * 2;
				if (textDimensions.Width > Size.Width)
				{
					renderText = renderText.Substring(0, renderText.Length - 1);
				}
			}
			while (Size.Width < textDimensions.Width);

			int textColorIndex;
			if (IsValid)
			{
				textColorIndex = cursorPosition >= 0 ? WhiteIndex : GoldIndex;
			}
			else
			{
				textColorIndex = RedIndex;
			}

			Font.DrawTextWithShadow(x + InnerBevel.Width, y + InnerBevel.Width, textColorIndex, BlackIndex, 1, renderText);
			InnerBevel.DrawBevel(new Rectangle(x + InnerBevel.Width, y + InnerBevel.Width, Size.Width - InnerBevel.Width * 2, Size.Height - InnerBevel.Width * 2));
		}
	}
}