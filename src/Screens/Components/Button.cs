using Microsoft.Xna.Framework;

namespace Warcraft.Screens.Components
{
	/// <summary>
	/// Delegate for button click events
	/// </summary>
	public delegate void ButtonClicked();

	public class Button
	{
		/// <summary>
		/// Text shown on the button
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// Location and dimensions of the button
		/// </summary>
		public Rectangle Rectangle;

		/// <summary>
		/// State of the button
		/// </summary>
		public GameButtonState State { get; set; }

		/// <summary>
		/// Gets or sets whether this button is validated
		/// (i.e. disabled when validation fails).
		/// </summary>
		public bool Validated { get; set; }

		/// <summary>
		/// An event handle for functions to run when the button is clicked.
		/// </summary>
		public event ButtonClicked OnClick;

		public Button()
		{
			Text = string.Empty;
		}

		public Button(string text, bool validated = false)
		{
			Text = text;
			Validated = validated;
		}

		/// <summary>
		/// Invoked when a button is clicked.
		/// </summary>
		public void Click()
		{
			OnClick?.Invoke();
		}
	}

	public enum GameButtonState
	{
		None,
		Pressed,
		Hover,
		Inactive
	}
}