using Warcraft.Screens.Base;
using Warcraft.Screens.Components;

namespace Warcraft.Screens
{
	public class OptionsMenuScreen : ButtonMenuScreen
	{
		public OptionsMenuScreen()
		{
			Title = "Options";

			var soundOptionsButton = new Button("Sound Options");
			soundOptionsButton.OnClick += SoundOptionsButton_OnClick;
			Buttons.Add(soundOptionsButton);

			var networkOptionsButton = new Button("Network Options");
			networkOptionsButton.OnClick += NetworkOptionsButton_OnClick;
			Buttons.Add(networkOptionsButton);

			Buttons.Add(new Button());

			var backButton = new Button("Back");
			backButton.OnClick += BackButtonOnOnClick;
			Buttons.Add(backButton);
		}

		private void SoundOptionsButton_OnClick() { ScreenManager.AddScreen(new SoundOptionsScreen()); }

		private void NetworkOptionsButton_OnClick() { }

		private void BackButtonOnOnClick() { IsExiting = true; }
	}
}