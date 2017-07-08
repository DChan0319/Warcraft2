using System.Linq;
using Microsoft.Xna.Framework;

namespace Warcraft.Screens.Components
{
	/// <summary>
	/// Delegate for text field validation
	/// </summary>
	/// <param name="text">The text to validate.</param>
	/// <returns>Returns whether the text passes validation.</returns>
	public delegate bool TextFieldValidator(string text);

	/// <summary>
	/// Text Field Component
	/// </summary>
	public class TextField
	{
		/// <summary>
		/// The title of the text field.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The text in the text field.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// The bounds of the text field.
		/// </summary>
		public Rectangle Bounds { get; set; }

		/// <summary>
		/// An event handle for validation functions to run when the
		/// text in the text field has changed.
		/// </summary>
		public event TextFieldValidator Validator;

		public TextField(string title, string text)
		{
			Title = title;
			Text = text;
		}

		/// <summary>
		/// Invoked when the text has changed.
		/// </summary>
		/// <returns>Returns whether the text passes all subscribed validation methods.</returns>
		public bool ValidateText()
		{
			return Validator?.GetInvocationList().Cast<TextFieldValidator>().All(validator => validator(Text)) ?? true;
		}
	}
}
