using System.Linq;
using Microsoft.Xna.Framework;
using Warcraft.App;

namespace Warcraft.Screens.Base
{
	public class ButtonMenuScreen : ButtonScreen
	{
		/// <summary>
		/// Title of the screen
		/// </summary>
		protected string Title { get; set; }

		public override void Draw(GameTime gameTime)
		{
			int titleHeight, bufferWidth, bufferHeight;
			DrawMenuTitle(Title, out titleHeight, out bufferWidth, out bufferHeight);

			Data.ButtonRenderer.SetText(Buttons.First().Text, true);
			foreach (var button in Buttons)
				Data.ButtonRenderer.SetText(button.Text);

			Data.ButtonRenderer.SetHeight(Data.ButtonRenderer.TextArea.Height * 3 / 2);
			Data.ButtonRenderer.SetWidth(Data.ButtonRenderer.TextArea.Width * 5 / 4);

			var buttonSkip = Data.ButtonRenderer.TextArea.Height * 3 / 2;
			var buttonLeft = bufferWidth / 2 - Data.ButtonRenderer.TextArea.Width / 2;
			var buttonTop = (bufferHeight - titleHeight) / 2 - (buttonSkip * (Buttons.Count - 1) + Data.ButtonRenderer.TextArea.Height) / 2 + Data.OuterBevel.Width;
			DrawButtons(buttonLeft, buttonTop, buttonSkip);
		}
	}
}