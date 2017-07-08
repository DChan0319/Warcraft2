using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Audio;
using Warcraft.Screens.Components;
using Warcraft.Screens.Manager;

namespace Warcraft.Screens.Base
{
	/// <summary>
	/// A screen with buttons on it.
	/// Not the same as <see cref="ButtonMenuScreen"/>.
	/// </summary>
	public class ButtonScreen : GameScreen
	{
		/// <summary>
		/// List of <see cref="Button"/>s on the screen
		/// </summary>
		protected List<Button> Buttons { get; }

		/// <summary>
		/// Represents whether a button was hovered over in the previous frame.
		/// </summary>
		private bool ButtonHovered { get; set; }

		/// <summary>
		/// Gets or sets whether buttons marked "validated" are enabled.
		/// </summary>
		protected bool ButtonsEnabled { get; set; }

		protected ButtonScreen()
		{
			Buttons = new List<Button>();
		}

		public override void HandleInput(InputState input)
		{
			// Reset button states
			foreach (var button in Buttons)
			{
				button.State = GameButtonState.None;

				if (!ButtonsEnabled && button.Validated)
				{
					button.State = GameButtonState.Inactive;
				}
			}

			var hoveredButton = Buttons.FirstOrDefault(b => b.Rectangle.Contains(input.CurrentMouseState.Position));
			if (hoveredButton != null)
			{
				if (hoveredButton.State == GameButtonState.Inactive)
					return;

				// If there was no button hovered before, and now there is, play sfx.
				if (!ButtonHovered)
					AudioManager.PlayWave("tick", Settings.General.SfxVolume);

				hoveredButton.State = GameButtonState.Hover;

				if (input.CurrentMouseState.LeftButton == ButtonState.Pressed)
					hoveredButton.State = GameButtonState.Pressed;

				// Invoke the button click event on the button if the mouse was clicked.
				if (input.LeftClick && !input.LeftDown)
				{
					AudioManager.PlayWave("place", Settings.General.SfxVolume);
					hoveredButton.Click();
				}

				ButtonHovered = true;
			}
			else
			{
				ButtonHovered = false;
			}
		}

		/// <summary>
		/// Draws the menu background, bevel, and title.
		/// </summary>
		protected static void DrawMenuTitle(string title, out int titleHeight, out int screenWidth, out int screenHeight)
		{
			screenWidth = ScreenManager.Graphics.Viewport.Width;
			screenHeight = ScreenManager.Graphics.Viewport.Height;

			for (var y = 0; y < screenHeight; y += Data.BackgroundTileset.TileHeight)
			{
				for (var x = 0; x < screenWidth; x += Data.BackgroundTileset.TileWidth)
				{
					ScreenManager.SpriteBatch.Draw(Data.BackgroundTileset.GetTile(0), new Vector2(x, y));
				}
			}

			var outerBevel = Data.OuterBevel;
			var area = new Rectangle(outerBevel.Width, outerBevel.Width, screenWidth - outerBevel.Width * 2, screenHeight - outerBevel.Width * 2);
			Data.OuterBevel.DrawBevel(area);

			var titleDimension = Data.Fonts[FontSize.Giant].MeasureText(title);
			titleHeight = titleDimension.Height;
			var textColorIndex = Data.FontRecolorMap.FindColor("white");
			var shadowColorIndex = Data.FontRecolorMap.FindColor("black");
			Data.Fonts[FontSize.Giant].DrawTextWithShadow(screenWidth / 2 - titleDimension.Width / 2, outerBevel.Width, textColorIndex, shadowColorIndex, 1, title);
		}

		protected void DrawButtons(int buttonLeft, int buttonTop, int buttonSkip)
		{
			foreach (var button in Buttons)
			{
				if (!string.IsNullOrEmpty(button.Text))
				{
					button.Rectangle = new Rectangle(buttonLeft, buttonTop, Data.ButtonRenderer.TextArea.Width, Data.ButtonRenderer.TextArea.Height);
					Data.ButtonRenderer.SetText(button.Text);
					Data.ButtonRenderer.DrawButton(button);
				}
				else
				{
					button.Rectangle = new Rectangle(0, 0, 0, 0);
				}
				buttonTop += buttonSkip;
			}
		}
	}
}