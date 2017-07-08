using System;
using Microsoft.Xna.Framework;
using Warcraft.Screens.Manager;

namespace Warcraft.Screens.Base
{
	/// <summary>
	/// This is the base class for a screen in the game, such as
	/// the main menu, options menu, and modals.
	/// </summary>
	public abstract class GameScreen
	{
		/// <summary>
		/// If true, this screen will not cause the screen underneath
		/// it to be transitioned off when it appears. This screen will
		/// be displayed on top, with the background dimmed and must be
		/// interacted with before it is removed.
		/// </summary>
		/// <example>
		/// Game Paused and Quit Confirmation are examples
		/// of modal windows.
		/// </example>
		public bool IsModal { get; protected set; }

		/// <summary>
		/// The time it takes for the screen to appear when it is activated.
		/// </summary>
		public TimeSpan TransitionOnTime { get; protected set; } = TimeSpan.Zero;
		/// <summary>
		/// The time it takes for the screen to disappear when it is deactivated
		/// </summary>
		public TimeSpan TransitionOffTime { get; protected set; } = TimeSpan.Zero;

		/// <summary>
		/// The current position of the screen transition.
		/// 0 indicates the screen has been transitioned on completely (present).
		/// 1 indicates the screen has been transitioned off completely (gone).
		/// </summary>
		public float TransitionPosition { get; protected set; } = 1.0f;

		/// <summary>
		/// Gets the current alpha of the screen.
		/// 0 indicates the screen has completely disappeared.
		/// 1 indicates the screen has completely appeared.
		/// </summary>
		public float TransitionAlpha => 1.0f - TransitionPosition;

		/// <summary>
		/// Gets the current screen transition state.
		/// </summary>
		public ScreenState ScreenState { get; protected set; } = ScreenState.TransitionOn;

		/// <summary>
		/// If true, the screen will automatically remove itself as soon as the
		/// transition finishes.
		/// </summary>
		public bool IsExiting { get; protected internal set; }

		/// <summary>
		/// Checks whether this screen is active and can respond to user input.
		/// </summary>
		public bool IsActive => !otherScreenHasFocus && (ScreenState == ScreenState.TransitionOn || ScreenState == ScreenState.Active);
		private bool otherScreenHasFocus;

		/// <summary>
		/// Gets the <see cref="ScreenManager"/> this screen belongs to.
		/// </summary>
		public ScreenManager ScreenManager { get; internal set; }

		/// <summary>
		/// Sets the viewport then calls <see cref="LoadContent()"/>.
		/// </summary>
		public void Load()
		{
			LoadContent();
		}

		/// <summary>
		/// Does something then calls <see cref="UnloadContent()"/>.
		/// </summary>
		public void Unload()
		{
			UnloadContent();
		}

		/// <summary>
		/// Load content necessary for this screen.
		/// Run once when the screen is activated.
		/// </summary>
		public virtual void LoadContent() { }

		/// <summary>
		/// Unload content that was loaded for this screen.
		/// Run once when the screen is deactivated.
		/// </summary>
		public virtual void UnloadContent() { }

		public virtual void Calculate(GameTime gameTime, bool coveredByOtherScreen) { }

		/// <summary>
		/// Allows the screen to run logic, such as updating transition position.
		/// This is called regardless of whether the screen is active or not.
		/// </summary>
		public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			this.otherScreenHasFocus = otherScreenHasFocus;

			if (IsExiting)
			{
				// If this screen is exiting, start transitioning off.
				ScreenState = ScreenState.TransitionOff;

				if (!UpdateTransition(gameTime, TransitionOffTime, 1))
				{
					// When the transition finishes, remove this screen.
					ScreenManager.RemoveScreen(this);
				}
			}
			else if (coveredByOtherScreen)
			{
				// If this screen is covered by another, start transitioning off.
				if (UpdateTransition(gameTime, TransitionOffTime, 1))
				{
					// Still transitioning
					ScreenState = ScreenState.TransitionOff;
				}
				else
				{
					// Finished transitioning
					ScreenState = ScreenState.Hidden;
				}
			}
			else
			{
				// Otherwise the screen is activating.
				if (UpdateTransition(gameTime, TransitionOnTime, -1))
				{
					// Still transitioning
					ScreenState = ScreenState.TransitionOn;
				}
				else
				{
					// Finished transitioning
					ScreenState = ScreenState.Active;
				}
			}
		}

		/// <summary>
		/// Helper function for updating the screen transition position.
		/// </summary>
		/// <returns>
		/// Returns whether the screen is still transitioning.
		/// </returns>
		private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
		{
			// How much to transition by
			float transitionDelta;

			if (time == TimeSpan.Zero)
				transitionDelta = 1;
			else
				transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);

			// Update the transition position
			TransitionPosition += transitionDelta * direction;

			// Check if transition finished
			if ((direction < 0 && TransitionPosition <= 0) || (direction > 0 && TransitionPosition >= 1))
			{
				TransitionPosition = MathHelper.Clamp(TransitionPosition, 0, 1);
				return false;
			}

			// Still transitioning the screen
			return true;
		}

		/// <summary>
		/// Allows the screen to handle user input.
		/// Only run when this screen is active.
		/// </summary>
		public virtual void HandleInput(InputState input) { }

		/// <summary>
		/// This is called before the screen should draw itself.
		/// Use this to render assets on different render targets.
		/// </summary>
		public virtual void BeginDraw(GameTime gameTime) { }

		/// <summary>
		/// This is called when the screen should draw itself.
		/// </summary>
		public virtual void Draw(GameTime gameTime) { }

		/// <summary>
		/// Removes the screen by either transitioning off if <see cref="TransitionOffTime"/>
		/// is non-zero or removing it immediately otherwise.
		/// </summary>
		public void ExitScreen()
		{
			if (TransitionOffTime == TimeSpan.Zero)
			{
				// Remove immediately if there is no transition time.
				ScreenManager.RemoveScreen(this);
			}
			else
			{
				// Set exit flag for it to be removed by update.
				IsExiting = true;
			}
		}
	}

	/// <summary>
	/// The screen transition state
	/// </summary>
	public enum ScreenState
	{
		/// <summary>
		/// The screen is transitioning on.
		/// </summary>
		TransitionOn,

		/// <summary>
		/// The screen is completely displayed.
		/// </summary>
		Active,

		/// <summary>
		/// The screen is transitioning off.
		/// </summary>
		TransitionOff,

		/// <summary>
		/// The screen is completely removed.
		/// </summary>
		Hidden
	}
}