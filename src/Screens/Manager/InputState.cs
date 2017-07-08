using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Warcraft.Screens.Manager
{
	/// <summary>
	/// Helper for reading input from the player. This class tracks both
	/// the current and previous state of input devices.
	/// </summary>
	public class InputState
	{
		/// <summary>
		/// Holds the current keyboard state.
		/// </summary>
		public KeyboardState CurrentKeyboardState;
		/// <summary>
		/// Holds the previous keyboard state.
		/// </summary>
		public KeyboardState PreviousKeyboardState;

		/// <summary>
		/// Holds the current mouse state.
		/// </summary>
		public MouseState CurrentMouseState;
		/// <summary>
		/// Holds the previous mouse state.
		/// </summary>
		public MouseState PreviousMouseState;

		/// <summary>
		/// Holds whether the left mouse button was clicked (pressed or released).
		/// </summary>
		public bool LeftClick;
		/// <summary>
		/// Holds whether the right mouse button was held down (not currently held down).
		/// </summary>
		public bool LeftDown;

		/// <summary>
		/// Holds whether the left mouse button was double clicked within 300ms, without moving.
		/// </summary>
		public bool DoubleClick;

		/// <summary>
		/// Holds the time at which the mouse button was last left clicked.
		/// </summary>
		public DateTime LastClickTime;

		/// <summary>
		/// Holds the position where the mouse button was last left clicked.
		/// </summary>
		public Point LastClickPosition;

		/// <summary>
		/// Holds whether the right mouse button was clicked (pressed or released).
		/// </summary>
		public bool RightClick;
		/// <summary>
		/// Holds whether the right mouse button was held down (not currently held down).
		/// </summary>
		public bool RightDown;

		/// <summary>
		/// Holds the position where the mouse is held down (not updated here!).
		/// </summary>
		public Point MouseDown;

		/// <summary>
		/// Updates the keyboard and mouse states.
		/// </summary>
		public void Update()
		{
			PreviousKeyboardState = CurrentKeyboardState;
			CurrentKeyboardState = Keyboard.GetState();

			PreviousMouseState = CurrentMouseState;
			CurrentMouseState = Mouse.GetState();

			LeftClick = PreviousMouseState.LeftButton != CurrentMouseState.LeftButton;
			if (LeftClick)
			{
				LeftDown = CurrentMouseState.LeftButton == ButtonState.Pressed;
				if (!LeftDown)
				{
					// Check if double clicked on the same position within 300 ms.
					var now = DateTime.Now;
					DoubleClick = CurrentMouseState.Position == LastClickPosition && (now - LastClickTime).TotalMilliseconds <= 300;

					// Update last click time
					// If it was a double click, set the last click time to min value
					// so that the next click is not treated as a double click.
					LastClickTime = DoubleClick ? DateTime.MinValue : now;
					LastClickPosition = CurrentMouseState.Position;
				}
			}

			RightClick = PreviousMouseState.RightButton != CurrentMouseState.RightButton;
			if (RightClick) RightDown = CurrentMouseState.RightButton == ButtonState.Pressed;
		}

		/// <summary>
		/// Returns whether the key was pressed and released.
		/// </summary>
		/// <remarks>
		/// This will check if the key was pressed last frame and released this frame,
		/// resulting in a keystroke.
		/// </remarks>
		public bool WasKeyPressed(Keys keys)
		{
			return PreviousKeyboardState.IsKeyDown(keys) && CurrentKeyboardState.IsKeyUp(keys);
		}

		/// <summary>
		/// Returns a list of keys that were pressed and released since the last update.
		/// </summary>
		public IEnumerable<Keys> GetPressedKeys()
		{
			var previousPressedKeys = PreviousKeyboardState.GetPressedKeys();
			var currentPressedKeys = CurrentKeyboardState.GetPressedKeys();
			return currentPressedKeys.Except(previousPressedKeys);
		}

		/// <summary>
		/// Returns a list of keys that were pressed and released since the last update.
		/// </summary>
		public IEnumerable<Keys> GetReleasedKeys()
		{
			var previousPressedKeys = PreviousKeyboardState.GetPressedKeys();
			var currentPressedKeys = CurrentKeyboardState.GetPressedKeys();
			return previousPressedKeys.Except(currentPressedKeys);
		}

		/// <summary>
		/// Returns whether <paramref name="key"/> is an alphabetical key.
		/// </summary>
		public static bool IsAlpha(Keys key)
		{
			return key >= Keys.A && key <= Keys.Z;
		}

		/// <summary>
		/// Returns whether <paramref name="key"/> is a numerical key.
		/// </summary>
		public static bool IsDigit(Keys key)
		{
			return key >= Keys.D0 && key <= Keys.D9 || key >= Keys.NumPad0 && key <= Keys.NumPad9;
		}

		/// <summary>
		/// Returns whether <paramref name="key"/> is an alphabetical or numerical key.
		/// </summary>
		public static bool IsAlphanumeric(Keys key)
		{
			return IsAlpha(key) || IsDigit(key);
		}

		/// <summary>
		/// Converts the <paramref name="keyPressed"/> into a char.
		/// </summary>
		/// <remarks>
		/// Adapted from http://roy-t.nl/2010/02/11/code-snippet-converting-keyboard-input-to-text-in-xna.html.
		/// </remarks>
		public static char KeyToChar(Keys keyPressed, bool shift)
		{
			var key = '\0';

			switch (keyPressed)
			{
				case Keys.A: key = shift ? 'A' : 'a'; break;
				case Keys.B: key = shift ? 'B' : 'b'; break;
				case Keys.C: key = shift ? 'C' : 'c'; break;
				case Keys.D: key = shift ? 'D' : 'd'; break;
				case Keys.E: key = shift ? 'E' : 'e'; break;
				case Keys.F: key = shift ? 'F' : 'f'; break;
				case Keys.G: key = shift ? 'G' : 'g'; break;
				case Keys.H: key = shift ? 'H' : 'h'; break;
				case Keys.I: key = shift ? 'I' : 'i'; break;
				case Keys.J: key = shift ? 'J' : 'j'; break;
				case Keys.K: key = shift ? 'K' : 'k'; break;
				case Keys.L: key = shift ? 'L' : 'l'; break;
				case Keys.M: key = shift ? 'M' : 'm'; break;
				case Keys.N: key = shift ? 'N' : 'n'; break;
				case Keys.O: key = shift ? 'O' : 'o'; break;
				case Keys.P: key = shift ? 'P' : 'p'; break;
				case Keys.Q: key = shift ? 'Q' : 'q'; break;
				case Keys.R: key = shift ? 'R' : 'r'; break;
				case Keys.S: key = shift ? 'S' : 's'; break;
				case Keys.T: key = shift ? 'T' : 't'; break;
				case Keys.U: key = shift ? 'U' : 'u'; break;
				case Keys.V: key = shift ? 'V' : 'v'; break;
				case Keys.W: key = shift ? 'W' : 'w'; break;
				case Keys.X: key = shift ? 'X' : 'x'; break;
				case Keys.Y: key = shift ? 'Y' : 'y'; break;
				case Keys.Z: key = shift ? 'Z' : 'z'; break;

				case Keys.D0: key = shift ? ')' : '0'; break;
				case Keys.D1: key = shift ? '!' : '1'; break;
				case Keys.D2: key = shift ? '@' : '2'; break;
				case Keys.D3: key = shift ? '#' : '3'; break;
				case Keys.D4: key = shift ? '$' : '4'; break;
				case Keys.D5: key = shift ? '%' : '5'; break;
				case Keys.D6: key = shift ? '^' : '6'; break;
				case Keys.D7: key = shift ? '&' : '7'; break;
				case Keys.D8: key = shift ? '*' : '8'; break;
				case Keys.D9: key = shift ? '(' : '9'; break;

				case Keys.NumPad0: key = '0'; break;
				case Keys.NumPad1: key = '1'; break;
				case Keys.NumPad2: key = '2'; break;
				case Keys.NumPad3: key = '3'; break;
				case Keys.NumPad4: key = '4'; break;
				case Keys.NumPad5: key = '5'; break;
				case Keys.NumPad6: key = '6'; break;
				case Keys.NumPad7: key = '7'; break;
				case Keys.NumPad8: key = '8'; break;
				case Keys.NumPad9: key = '9'; break;

				case Keys.OemTilde: key = shift ? '~' : '`'; break;
				case Keys.OemSemicolon: key = shift ? ':' : ';'; break;
				case Keys.OemQuotes: key = shift ? '"' : '\''; break;
				case Keys.OemQuestion: key = shift ? '?' : '/'; break;
				case Keys.OemPlus: key = shift ? '+' : '='; break;
				case Keys.OemPipe: key = shift ? '|' : '\\'; break;
				case Keys.OemPeriod: key = shift ? '>' : '.'; break;
				case Keys.OemOpenBrackets: key = shift ? '{' : '['; break;
				case Keys.OemCloseBrackets: key = shift ? '}' : ']'; break;
				case Keys.OemMinus: key = shift ? '_' : '-'; break;
				case Keys.OemComma: key = shift ? '<' : ','; break;
				case Keys.Space: key = ' '; break;
			}

			return key;
		}
	}
}