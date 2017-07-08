using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Extensions;
using Warcraft.Screens.Components;
using Warcraft.Screens.Manager;

namespace Warcraft.Renderers
{
	public class ButtonRenderer
	{
		/// <summary>
		/// <see cref="RecolorMap"/> used to color the button
		/// </summary>
		private RecolorMap ColorMap { get; }

		/// <summary>
		/// <see cref="Bevel"/> used for the button
		/// </summary>
		private Bevel OuterBevel { get; }

		/// <summary>
		/// <see cref="Bevel"/> used for the button
		/// </summary>
		private Bevel InnerBevel { get; }

		/// <summary>
		/// Indices of textures
		/// </summary>
		private List<int> LightIndices { get; }
		/// <summary>
		/// Indices of textures
		/// </summary>
		private List<int> DarkIndices { get; }

		/// <summary>
		/// Font used for the button text
		/// </summary>
		private FontTileset Font { get; }

		/// <summary>
		/// Color of the button
		/// </summary>
		public PlayerColor ButtonColor { get; set; }

		/// <summary>
		/// The text that is going to be rendered on the current button
		/// </summary>
		private string Text { get; set; }

		/// <summary>
		/// The location and area of the text on the current button
		/// </summary>
		public Rectangle TextArea;

		/// <summary>
		/// White Color Index (for text)
		/// </summary>
		private int WhiteIndex { get; }
		/// <summary>
		/// Gold Color Index (for text)
		/// </summary>
		private int GoldIndex { get; }
		/// <summary>
		/// Black Color Index (for text)
		/// </summary>
		private int BlackIndex { get; }

		public ButtonRenderer(RecolorMap colorMap, Bevel innerBevel, Bevel outerBevel, FontTileset font)
		{
			ColorMap = colorMap;
			OuterBevel = outerBevel;
			InnerBevel = innerBevel;
			Font = font;
			ButtonColor = PlayerColor.None;
			TextArea = new Rectangle(0, 0, OuterBevel.Width * 2, OuterBevel.Width * 2);

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
			DarkIndices[(int)PlayerColor.None] = DarkIndices[(int)PlayerColor.Blue] = ColorMap.FindColor("blue-dark");
			DarkIndices[(int)PlayerColor.Red] = ColorMap.FindColor("red-dark");
			DarkIndices[(int)PlayerColor.Green] = ColorMap.FindColor("green-dark");
			DarkIndices[(int)PlayerColor.Purple] = ColorMap.FindColor("purple-dark");
			DarkIndices[(int)PlayerColor.Orange] = ColorMap.FindColor("orange-dark");
			DarkIndices[(int)PlayerColor.Yellow] = ColorMap.FindColor("yellow-dark");
			DarkIndices[(int)PlayerColor.Black] = ColorMap.FindColor("black-dark");
			DarkIndices[(int)PlayerColor.White] = ColorMap.FindColor("white-dark");

			WhiteIndex = Font.RecolorMap.FindColor("white");
			GoldIndex = Font.RecolorMap.FindColor("gold");
			BlackIndex = Font.RecolorMap.FindColor("black");
		}

		/// <summary>
		/// Sets the text of the button and recalculates the position of the text.
		/// </summary>
		public void SetText(string text, bool minimize = false)
		{
			Text = text;
			var textDimensions = Font.MeasureText(Text);

			var totalHeight = textDimensions.Y - textDimensions.X + 1;
			if (totalHeight + OuterBevel.Width * 2 > TextArea.Height || minimize)
				TextArea.Height = totalHeight + OuterBevel.Width * 2;

			if (textDimensions.Width + OuterBevel.Width * 2 > TextArea.Width || minimize)
				TextArea.Width = textDimensions.Width + OuterBevel.Width * 2;

			TextArea.X = TextArea.Width / 2 - textDimensions.Width / 2;
			TextArea.Y = TextArea.Height / 2 - totalHeight / 2 - textDimensions.X;
		}

		/// <summary>
		/// Sets the width of the button and recalculates the position of the text.
		/// </summary>
		public void SetWidth(int width)
		{
			if (width > TextArea.Width)
			{
				var textDimensions = Font.MeasureText(Text);
				TextArea.Width = width;
				TextArea.X = TextArea.Width / 2 - textDimensions.Width / 2;
			}
		}

		/// <summary>
		/// Sets the height of the button and recalculates the position of the text.
		/// </summary>
		public void SetHeight(int height)
		{
			if (height > TextArea.Height)
			{
				var textDimensions = Font.MeasureText(Text);
				var totalHeight = textDimensions.Y - textDimensions.X + 1;
				TextArea.Height = height;
				TextArea.Y = TextArea.Height / 2 - totalHeight / 2 - textDimensions.X;
			}
		}

		/// <summary>
		/// Renders the <paramref name="button"/> onto the current render target.
		/// </summary>
		public void DrawButton(Button button)
		{
			var x = button.Rectangle.Location.X;
			var y = button.Rectangle.Location.Y;

			switch (button.State)
			{
				case GameButtonState.Pressed:
					{
						var bevelWidth = InnerBevel.Width;
						ScreenManager.SpriteBatch.DrawFilledRectangle(new Rectangle(x, y, TextArea.Width, TextArea.Height), ColorMap.ColorValue(DarkIndices[(int)ButtonColor], 0).ToColor());
						InnerBevel.DrawBevel(new Rectangle(x + bevelWidth, y + bevelWidth, TextArea.Width - bevelWidth * 2, TextArea.Height - bevelWidth * 2));
						Font.DrawTextWithShadow(x + TextArea.X, y + TextArea.Y, WhiteIndex, BlackIndex, 1, Text);
					}
					break;

				case GameButtonState.Inactive:
					{
						var bevelWidth = OuterBevel.Width;
						ScreenManager.SpriteBatch.DrawFilledRectangle(new Rectangle(x, y, TextArea.Width, TextArea.Height), ColorMap.ColorValue(DarkIndices[(int)ButtonColor], 0).ToColor());
						OuterBevel.DrawBevel(new Rectangle(x + bevelWidth, y + bevelWidth, TextArea.Width - bevelWidth * 2, TextArea.Height - bevelWidth * 2));
						Font.DrawTextWithShadow(x + TextArea.X, y + TextArea.Y, BlackIndex, WhiteIndex, 1, Text);
					}
					break;

				default:
					{
						var bevelWidth = OuterBevel.Width;
						ScreenManager.SpriteBatch.DrawFilledRectangle(new Rectangle(x, y, TextArea.Width, TextArea.Height), ColorMap.ColorValue(LightIndices[(int)ButtonColor], 0).ToColor());
						OuterBevel.DrawBevel(new Rectangle(x + bevelWidth, y + bevelWidth, TextArea.Width - bevelWidth * 2, TextArea.Height - bevelWidth * 2));
						Font.DrawTextWithShadow(x + TextArea.X, y + TextArea.Y, button.State == GameButtonState.Hover ? WhiteIndex : GoldIndex, BlackIndex, 1, Text);
					}
					break;
			}
		}
	}
}