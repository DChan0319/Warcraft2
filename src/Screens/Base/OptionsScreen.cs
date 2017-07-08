using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Screens.Components;
using Warcraft.Screens.Manager;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Warcraft.Screens.Base
{
	public class OptionsScreen : ButtonScreen
	{
		/// <summary>
		/// Title of the screen
		/// </summary>
		protected string Title { get; set; }

		protected List<TextField> TextFields { get; set; }

		protected TextField SelectedTextField;
		protected int SelectedTextFieldCharacter;

		protected OptionsScreen()
		{
			TextFields = new List<TextField>();
		}

		public override void HandleInput(InputState input)
		{
			base.HandleInput(input);

			if (input.LeftClick && !input.LeftDown)
			{
				var clickedTextField = false;
				foreach (var textField in TextFields)
				{
					if (!textField.Bounds.Contains(input.CurrentMouseState.Position))
						continue;

					SelectedTextField = textField;
					SelectedTextFieldCharacter = SelectedTextField.Text.Length;
					clickedTextField = true;
					break;
				}

				if (!clickedTextField)
				{
					SelectedTextField = null;
				}
			}

			foreach (var key in input.GetReleasedKeys())
			{
				if (key == Keys.Escape)
				{
					SelectedTextField = null;
				}
				else if (SelectedTextField != null)
				{
					switch (key)
					{
						case Keys.Delete:
						case Keys.Back:
							if (SelectedTextFieldCharacter != 0)
							{
								SelectedTextField.Text = SelectedTextField.Text.Remove(SelectedTextFieldCharacter - 1, 1);
								SelectedTextFieldCharacter--;
							}
							else if (SelectedTextField.Text.Length != 0)
							{
								SelectedTextField.Text = SelectedTextField.Text.Substring(1);
							}
							break;

						case Keys.Left:
							if (SelectedTextFieldCharacter != 0)
								SelectedTextFieldCharacter--;
							break;

						case Keys.Right:
							if (SelectedTextFieldCharacter < SelectedTextField.Text.Length)
								SelectedTextFieldCharacter++;
							break;

						default:
							if (InputState.IsAlphanumeric(key) || key == Keys.OemPeriod)
							{
								var shift = input.CurrentKeyboardState.IsKeyDown(Keys.LeftShift) || input.CurrentKeyboardState.IsKeyDown(Keys.RightShift);
								SelectedTextField.Text = SelectedTextField.Text.Insert(SelectedTextFieldCharacter, InputState.KeyToChar(key, shift).ToString());
								SelectedTextFieldCharacter++;
							}
							break;
					}
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			ButtonsEnabled = TextFields.All(textField => textField.ValidateText());

			int titleHeight, bufferWidth, bufferHeight;
			DrawMenuTitle(Title, out titleHeight, out bufferWidth, out bufferHeight);

			// Buttons
			Data.ButtonRenderer.SetText(Buttons.First().Text, true);
			foreach (var button in Buttons)
				Data.ButtonRenderer.SetText(button.Text);

			var firstButton = true;
			foreach (var button in Buttons)
			{
				Data.ButtonRenderer.SetText(button.Text, firstButton);
				firstButton = false;
			}

			Data.ButtonRenderer.SetWidth(Data.ButtonRenderer.TextArea.Width * 3 / 2);
			Data.ButtonRenderer.SetHeight(Data.ButtonRenderer.TextArea.Height * 3 / 2);

			var buttonSkip = Data.ButtonRenderer.TextArea.Height * 3 / 2;
			var buttonLeft = bufferWidth - Data.BorderWidth - Data.ButtonRenderer.TextArea.Width;
			var buttonTop = bufferHeight - Data.BorderWidth - (Buttons.Count * buttonSkip - Data.ButtonRenderer.TextArea.Height / 2);
			DrawButtons(buttonLeft, buttonTop, buttonSkip);

			// Text Fields
			var whiteIndex = Data.Fonts[FontSize.Large].RecolorMap.FindColor("white");
			var shadowIndex = Data.Fonts[FontSize.Large].RecolorMap.FindColor("black");

			var bufferCenter = bufferWidth / 2;
			var optionSkip = Data.TextFieldRenderer.Size.Height * 3 / 2;
			var optionTop = (bufferHeight + titleHeight) / 2 - optionSkip * TextFields.Count / 2;

			foreach (var textField in TextFields)
			{
				var textDimensions = Data.Fonts[FontSize.Large].MeasureText(textField.Title);
				var textOffsetY = Data.TextFieldRenderer.Size.Height / 2 - textDimensions.Height / 2;
				Data.Fonts[FontSize.Large].DrawTextWithShadow(bufferCenter - textDimensions.Width, optionTop + textOffsetY, whiteIndex, shadowIndex, 1, textField.Title);

				Data.TextFieldRenderer.SetText(textField.Text, textField.ValidateText());
				Data.TextFieldRenderer.DrawTextField(bufferCenter, optionTop, SelectedTextField == textField ? SelectedTextFieldCharacter : -1);
				textField.Bounds = new Rectangle(bufferCenter, optionTop, Data.TextFieldRenderer.Size.Width, Data.TextFieldRenderer.Size.Height);

				optionTop += optionSkip;
			}
		}
	}
}