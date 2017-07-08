using Microsoft.Xna.Framework;
using Warcraft.Screens.Manager;

namespace Warcraft.Screens.Base
{
	/// <summary>
	/// This is the base class for a screen in the game, such as
	/// the main menu, options menu, and modals.
	/// </summary>
	public abstract class GameOverlay
	{
		/// <summary>
		/// Gets the <see cref="ScreenManager"/> this screen belongs to.
		/// </summary>
		public ScreenManager ScreenManager { get; internal set; }

		/// <summary>
		/// If true, this overlay will handle input and prevent
		/// overlays and screen under it to.
		/// </summary>
		public bool HandlesInput { get; set; }

		/// <summary>
		/// Calls <see cref="LoadContent()"/>.
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
		/// Load content necessary for this overlay.
		/// Run once when the overlay is activated.
		/// </summary>
		public virtual void LoadContent() { }

		/// <summary>
		/// Unload content that was loaded for this overlay.
		/// Run once when the overlay is deactivated.
		/// </summary>
		public virtual void UnloadContent() { }

		/// <summary>
		/// Allows the overlay to run logic.
		/// </summary>
		public virtual void Update(GameTime gameTime) { }

		/// <summary>
		/// Allows the overlay to handle user input.
		/// Only run when this overlay is active.
		/// </summary>
		public virtual void HandleInput(InputState input) { }

		/// <summary>
		/// This is called when the overlay should draw itself.
		/// </summary>
		public virtual void Draw(GameTime gameTime) { }

		/// <summary>
		/// Removes the overlay.
		/// </summary>
		public void ExitOverlay()
		{
			ScreenManager.RemoveOverlay(this);
		}
	}
}