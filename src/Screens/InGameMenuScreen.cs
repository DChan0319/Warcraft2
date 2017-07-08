using Warcraft.App;
using Warcraft.Screens.Base;
using Warcraft.Screens.Components;

namespace Warcraft.Screens
{
	public class InGameMenuScreen : ButtonMenuScreen
	{
		private GameScreen ParentScreen { get; }

		public InGameMenuScreen(GameScreen parentScreen)
		{
			Title = "Menu";

			ParentScreen = parentScreen;

			var saveAndExitButton = new Button("Save & Exit Game");
			saveAndExitButton.OnClick += SaveAndExitButton_OnClick;
			Buttons.Add(saveAndExitButton);

			var optionsButton = new Button("Options");
			optionsButton.OnClick += OptionsButton_OnClick;
			Buttons.Add(optionsButton);

			Buttons.Add(new Button());

			var resumeButton = new Button("Resume Game");
			resumeButton.OnClick += ResumeButton_OnClick;
			Buttons.Add(resumeButton);
		}

		private void SaveAndExitButton_OnClick()
		{
			Data.SaveGame();
			ScreenManager.RemoveScreen(ParentScreen);
			ExitScreen();
		}

		private void OptionsButton_OnClick() { ScreenManager.AddScreen(new OptionsMenuScreen()); }

		private void ResumeButton_OnClick() { IsExiting = true; }
	}
}