using System.IO;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Screens.Base;
using Warcraft.Screens.Components;

namespace Warcraft.Screens
{
	public class MainMenuScreen : ButtonMenuScreen
	{
		private readonly Button loadGameButton;

		public MainMenuScreen()
		{
			Title = "The Game";

			loadGameButton = new Button("Load Game");
			loadGameButton.OnClick += LoadGameButton_OnClick;
			if (File.Exists(Settings.SaveFileName))
				Buttons.Add(loadGameButton);

			var singlePlayerGameButton = new Button("Single Player Game");
			singlePlayerGameButton.OnClick += SinglePlayerGameButton_OnClick;
			Buttons.Add(singlePlayerGameButton);

			var multiPlayerGameButton = new Button("Multi Player Game");
			multiPlayerGameButton.OnClick += MultiPlayerGameButton_OnClick;
			Buttons.Add(multiPlayerGameButton);

			var optionsButton = new Button("Options");
			optionsButton.OnClick += OptionsButton_OnClick;
			Buttons.Add(optionsButton);

			Buttons.Add(new Button());

			var exitGameButton = new Button("Exit Game");
			exitGameButton.OnClick += ExitGameButton_OnClick;
			Buttons.Add(exitGameButton);
		}

		private void LoadGameButton_OnClick()
		{
			if (!File.Exists(Settings.SaveFileName)) return;

			if (Data.LoadSavedGame())
				ScreenManager.AddScreen(new TestScreen());
		}

		private void SinglePlayerGameButton_OnClick() { ScreenManager.AddScreen(new MapSelectionScreen()); }

		private void MultiPlayerGameButton_OnClick() { ScreenManager.AddScreen(new MultiPlayerMenuScreen()); }

		private void OptionsButton_OnClick() { ScreenManager.AddScreen(new OptionsMenuScreen()); }

		private void ExitGameButton_OnClick() { ScreenManager.Game.Exit(); }

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

			if (Buttons.Contains(loadGameButton) && !File.Exists(Settings.SaveFileName))
				Buttons.Remove(loadGameButton);
			else if (!Buttons.Contains(loadGameButton) && File.Exists(Settings.SaveFileName))
				Buttons.Insert(0, loadGameButton);
		}
	}
}