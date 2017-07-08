using Warcraft.Screens.Base;
using Warcraft.Screens.Components;

namespace Warcraft.Screens
{
	public class MultiPlayerMenuScreen : ButtonMenuScreen
	{
		public MultiPlayerMenuScreen()
		{
			Title = "Multi Player Game Options";

			var hostMultiPlyerGameButton = new Button("Host Multi Player Game");
			hostMultiPlyerGameButton.OnClick += HostMultiPlyerGameButton_OnClick;
			Buttons.Add(hostMultiPlyerGameButton);

			var joinMultiPlayerGameButton = new Button("Join Multi Player Game");
			joinMultiPlayerGameButton.OnClick += JoinMultiPlayerGameButton_OnClick;
			Buttons.Add(joinMultiPlayerGameButton);

			Buttons.Add(new Button());

			var backButton = new Button("Back");
			backButton.OnClick += BackButton_OnClick;
			Buttons.Add(backButton);
		}

		private void HostMultiPlyerGameButton_OnClick() { }

		private void JoinMultiPlayerGameButton_OnClick() { }

		private void BackButton_OnClick() { IsExiting = true; }
	}
}