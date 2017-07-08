using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Renderers;
using Warcraft.Screens.Base;
using Warcraft.Screens.Components;
using Warcraft.Screens.Manager;

namespace Warcraft.Screens
{
	public class PlayerAiColorSelectScreen : ButtonScreen
	{
		private string Title { get; set; }

		private List<Button> ColorButtons { get; set; }
		private List<Button> PlayerTypeButtons { get; set; }
		private PlayerColor PlayerColorRequestingChange { get; set; }
		private PlayerColor PlayerColorChangeRequest { get; set; }
		private PlayerColor PlayerColorRequestTypeChange { get; set; }

		public PlayerAiColorSelectScreen()
		{
			ColorButtons = new List<Button>();
			PlayerTypeButtons = new List<Button>();

			Title = "Select Colors/Difficulty";

			var playGameButton = new Button("Play Game");
			playGameButton.OnClick += PlayGameButton_OnClick;
			Buttons.Add(playGameButton);

			var cancelButton = new Button("Cancel");
			cancelButton.OnClick += CancelButton_OnClick;
			Buttons.Add(cancelButton);
		}

		private void PlayGameButton_OnClick()
		{
			// So the player goes back to the
			// main menu when exiting a game
			ScreenManager.PopScreen(); // Pop this screen
			ScreenManager.PopScreen(); // Pop map selection screen

			ScreenManager.AddScreen(new TestScreen());
		}

		private void CancelButton_OnClick() { IsExiting = true; }

		public override void HandleInput(InputState input)
		{
			base.HandleInput(input);

			if (input.LeftClick && !input.LeftDown)
			{
				for (var index = 0; index < ColorButtons.Count; index++)
				{
					if (!ColorButtons[index].Rectangle.Contains(input.CurrentMouseState.Position)) continue;

					var playerSelecting = 1 + index / ((int)PlayerColor.Max - 1);
					var colorSelecting = 1 + index % ((int)PlayerColor.Max - 1);

					if (playerSelecting == 1 /* || Todo: Multiplayer */ || true)
					{
						if (playerSelecting == 1 || Data.LoadingPlayerTypes[playerSelecting] != PlayerType.Human)
						{
							PlayerColorRequestingChange = (PlayerColor)playerSelecting;
							PlayerColorChangeRequest = (PlayerColor)colorSelecting;
						}
					}
				}

				for (var index = 0; index < PlayerTypeButtons.Count; index++)
				{
					if (!PlayerTypeButtons[index].Rectangle.Contains(input.CurrentMouseState.Position)) continue;

					PlayerColorRequestTypeChange = (PlayerColor)(index + 2);
					break;
				}
			}
		}

		public override void Calculate(GameTime gameTime, bool coveredByOtherScreen)
		{
			if (PlayerColorRequestingChange != PlayerColor.None)
			{
				var newColorInUse = PlayerColor.None;

				for (var index = 1; index < (int)PlayerColor.Max; index++)
				{
					if (index == (int)PlayerColorRequestingChange) continue;
					if (Data.LoadingPlayerTypes[index] == PlayerType.None) continue;
					if (Data.LoadingPlayerColors[index] != PlayerColorChangeRequest) continue;

					newColorInUse = (PlayerColor)index;
					break;
				}

				if (newColorInUse != PlayerColor.None)
				{
					Data.LoadingPlayerColors[(int)newColorInUse] = Data.LoadingPlayerColors[(int)PlayerColorRequestingChange];
				}

				if ((int)PlayerColorRequestingChange == 1)
					Data.PlayerColor = PlayerColorChangeRequest;
				Data.LoadingPlayerColors[(int)PlayerColorRequestingChange] = PlayerColorChangeRequest;
				Data.SelectedMap = DecoratedMap.DuplicateMap(Data.SelectedMapIndex, Data.LoadingPlayerColors);

				// Create a new asset renderer so that the colors on the minimap preview changes.
				Data.AssetRenderer = new AssetRenderer(Data.AssetRecolorMap, Data.AssetTilesets, Data.MarkerTileset, Data.FireTilesets, Data.BuildingDeathTileset, Data.ArrowTileset, null, Data.SelectedMap);
				Data.MiniMapRenderer = new MiniMapRenderer(Data.MapRenderer, Data.AssetRenderer, null);
			}

			if (PlayerColorRequestTypeChange != PlayerColor.None)
			{
				// Todo: Multiplayer
				switch (Data.LoadingPlayerTypes[(int)PlayerColorRequestTypeChange])
				{
					case PlayerType.AiEasy: Data.LoadingPlayerTypes[(int)PlayerColorRequestTypeChange] = PlayerType.AiMedium; break;
					case PlayerType.AiMedium: Data.LoadingPlayerTypes[(int)PlayerColorRequestTypeChange] = PlayerType.AiHard; break;
					default: Data.LoadingPlayerTypes[(int)PlayerColorRequestTypeChange] = PlayerType.AiEasy; break;
				}
			}

			// Put this here, instead of HandleInput, otherwise it will
			// get reset when the Calculate gets skipped before it
			// reaches here and gets applied.
			PlayerColorRequestingChange = PlayerColor.None;
			PlayerColorChangeRequest = PlayerColor.None;
			PlayerColorRequestTypeChange = PlayerColor.None;
		}

		public override void BeginDraw(GameTime gameTime)
		{
			// Draw the mini map
			ScreenManager.Graphics.SetRenderTarget(Data.MiniMapRenderTarget);
			Data.MiniMapRenderer.DrawMiniMap(null);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			var playerNames = new List<string>(new string[(int)PlayerColor.Max]);

			int titleHeight, bufferWidth, bufferHeight;
			DrawMenuTitle(Title, out titleHeight, out bufferWidth, out bufferHeight);

			// Draw Mini Map
			var miniMapLeft = bufferWidth - Data.MiniMapRenderTarget.Width - Data.BorderWidth;
			ScreenManager.SpriteBatch.Draw(Data.MiniMapRenderTarget, new Vector2(miniMapLeft, titleHeight + Data.InnerBevel.Width));
			Data.InnerBevel.DrawBevel(new Rectangle(miniMapLeft, titleHeight + Data.InnerBevel.Width, Data.MiniMapRenderTarget.Width, Data.MiniMapRenderTarget.Height));

			var textTop = titleHeight + Data.MiniMapRenderTarget.Height + Data.InnerBevel.Width * 3;
			var miniMapCenter = miniMapLeft + Data.MiniMapRenderTarget.Width / 2;
			var whiteIndex = Data.Fonts[FontSize.Large].RecolorMap.FindColor("white");
			var goldIndex = Data.Fonts[FontSize.Large].RecolorMap.FindColor("gold");
			var shadowIndex = Data.Fonts[FontSize.Large].RecolorMap.FindColor("black");

			// Draw Player Count Text
			var playerCountText = Data.SelectedMap.PlayerCount + " Players";
			var playerCountTextDimensions = Data.Fonts[FontSize.Large].MeasureText(playerCountText);
			Data.Fonts[FontSize.Large].DrawTextWithShadow(miniMapCenter - playerCountTextDimensions.Width / 2, textTop, whiteIndex, shadowIndex, 1, playerCountText);
			textTop += playerCountTextDimensions.Height;

			// Draw Map Dimensions Text
			var mapDimensionsText = $"{Data.SelectedMap.MapWidth} x {Data.SelectedMap.MapHeight}";
			var mapDimensionsTextDimensions = Data.Fonts[FontSize.Large].MeasureText(mapDimensionsText);
			Data.Fonts[FontSize.Large].DrawTextWithShadow(miniMapCenter - mapDimensionsTextDimensions.Width / 2, textTop, whiteIndex, shadowIndex, 1, mapDimensionsText);

			textTop = titleHeight;

			// Draw Player Text
			var playerText = "Player";
			var playerTextDimensions = Data.Fonts[FontSize.Large].MeasureText(playerText);
			Data.Fonts[FontSize.Large].DrawTextWithShadow(Data.BorderWidth, textTop, whiteIndex, shadowIndex, 1, playerText);
			textTop += playerTextDimensions.Height;

			Data.ButtonRenderer.SetText("AI Easy", true);
			var colorButtonHeight = Data.ButtonRenderer.TextArea.Height;
			var rowHeight = Data.ButtonRenderer.TextArea.Height + Data.InnerBevel.Width * 2;
			if (rowHeight < playerTextDimensions.Height)
				rowHeight = playerTextDimensions.Height;

			Data.ButtonRenderer.SetText("X", true);
			Data.ButtonRenderer.SetHeight(colorButtonHeight);
			var columnWidth = Data.ButtonRenderer.TextArea.Width + Data.InnerBevel.Width * 2;
			var maxTextWidth = 0;
			var tempString = string.Empty;
			for (var index = 1; index <= Data.SelectedMap.PlayerCount; index++)
			{
				if (index == 1)
				{
					tempString = index + ". You";
					playerNames[index] = tempString;
				}
				else if (Data.LoadingPlayerTypes[index] != PlayerType.Human)
				{
					tempString = index + ". Player " + index;
					playerNames[index] = tempString;
				}

				var playerNameTextDimensions = Data.Fonts[FontSize.Large].MeasureText(tempString);
				if (maxTextWidth < playerNameTextDimensions.Width)
					maxTextWidth = playerNameTextDimensions.Width;
			}

			tempString = "Color";
			var tempStringDimensions = Data.Fonts[FontSize.Large].MeasureText(tempString);
			Data.Fonts[FontSize.Large].DrawTextWithShadow(Data.BorderWidth + maxTextWidth + columnWidth * ((int)PlayerColor.Max + 1) / 2 - tempStringDimensions.Width / 2, titleHeight, whiteIndex, shadowIndex, 1, tempString);
			ColorButtons.Clear();
			var aiButtonLeft = 0;

			for (var index = 1; index <= Data.SelectedMap.PlayerCount; index++)
			{
				tempString = playerNames[index];
				Data.Fonts[FontSize.Large].DrawTextWithShadow(Data.BorderWidth, textTop, index == 1 ? goldIndex : whiteIndex, shadowIndex, 1, tempString);

				for (var colorIndex = 1; colorIndex < (int)PlayerColor.Max; colorIndex++)
				{
					var buttonLeft = Data.BorderWidth + maxTextWidth + colorIndex * columnWidth;
					var colorButton = new Button
					{
						Rectangle = new Rectangle(buttonLeft, textTop, Data.ButtonRenderer.TextArea.Width, Data.ButtonRenderer.TextArea.Height)
					};

					if (colorButton.Rectangle.Contains(ScreenManager.Input.CurrentMouseState.Position))
					{
						if (ScreenManager.Input.CurrentMouseState.LeftButton == ButtonState.Pressed)
							colorButton.State = GameButtonState.Pressed;
						else
							colorButton.State = GameButtonState.Hover;
					}

					Data.ButtonRenderer.SetText((int)Data.LoadingPlayerColors[index] == colorIndex ? "X" : "");
					Data.ButtonRenderer.ButtonColor = (PlayerColor)colorIndex;
					Data.ButtonRenderer.DrawButton(colorButton);
					ColorButtons.Add(colorButton);

					aiButtonLeft = buttonLeft + columnWidth;
				}

				textTop += rowHeight;
			}

			Data.ButtonRenderer.ButtonColor = PlayerColor.None;
			var aiText = "AI Easy";
			Data.ButtonRenderer.SetText(aiText);
			Data.ButtonRenderer.SetWidth(Data.ButtonRenderer.TextArea.Width * 3 / 2);

			textTop = titleHeight;
			var difficultyText = "Difficulty";
			var difficultyTextDimensions = Data.Fonts[FontSize.Large].MeasureText(difficultyText);
			Data.Fonts[FontSize.Large].DrawTextWithShadow(aiButtonLeft + (Data.ButtonRenderer.TextArea.Width - difficultyTextDimensions.Width) / 2, textTop, whiteIndex, shadowIndex, 1, difficultyText);
			textTop += rowHeight + difficultyTextDimensions.Height;

			PlayerTypeButtons.Clear();
			for (var index = 2; index <= Data.SelectedMap.PlayerCount; index++)
			{
				var playerTypeButton = new Button
				{
					Rectangle = new Rectangle(aiButtonLeft, textTop, Data.ButtonRenderer.TextArea.Width, Data.ButtonRenderer.TextArea.Height)
				};
				if (playerTypeButton.Rectangle.Contains(ScreenManager.Input.CurrentMouseState.Position))
				{
					if (ScreenManager.Input.CurrentMouseState.LeftButton == ButtonState.Pressed)
						playerTypeButton.State = GameButtonState.Pressed;
					else
						playerTypeButton.State = GameButtonState.Hover;
				}

				switch (Data.LoadingPlayerTypes[index])
				{
					case PlayerType.Human: Data.ButtonRenderer.SetText("Human"); break;
					case PlayerType.AiEasy: Data.ButtonRenderer.SetText("AI Easy"); break;
					case PlayerType.AiMedium: Data.ButtonRenderer.SetText("AI Medium"); break;
					case PlayerType.AiHard: Data.ButtonRenderer.SetText("AI Hard"); break;
					default: Data.ButtonRenderer.SetText("Closed"); break;
				}

				Data.ButtonRenderer.DrawButton(playerTypeButton);
				PlayerTypeButtons.Add(playerTypeButton);

				textTop += rowHeight;
			}

			Data.ButtonRenderer.SetText(Buttons.First().Text, true);
			Data.ButtonRenderer.SetHeight(Data.ButtonRenderer.TextArea.Height * 3 / 2);
			Data.ButtonRenderer.SetWidth(Data.MiniMapRenderTarget.Width);
			var buttonSkip = Data.ButtonRenderer.TextArea.Height * 5 / 4;
			var okButtonLeft = bufferWidth - Data.ButtonRenderer.TextArea.Width - Data.BorderWidth;
			var okButtonTop = bufferHeight - Data.ButtonRenderer.TextArea.Height * 9 / 4 - Data.BorderWidth;
			DrawButtons(okButtonLeft, okButtonTop, buttonSkip);
		}
	}
}