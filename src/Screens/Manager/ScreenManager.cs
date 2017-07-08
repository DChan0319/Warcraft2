using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Warcraft.App;
using Warcraft.Screens.Base;

namespace Warcraft.Screens.Manager
{  /// <summary>
   /// The screen manager is a component which manages one or more GameScreen
   /// instances. It maintains a stack of screens, calls their Update and Draw
   /// methods at the appropriate times, and automatically routes input to the
   /// topmost active screen.
   /// </summary>
	public class ScreenManager : DrawableGameComponent
	{
		/// <summary>
		/// List of screens being managed by this <see cref="ScreenManager"/>.
		/// </summary>
		private readonly List<GameScreen> screens = new List<GameScreen>();
		/// <summary>
		/// List of screens that will be updated on the next call to <see cref="Update"/>.
		/// </summary>
		private readonly List<GameScreen> screensToUpdate = new List<GameScreen>();

		/// <summary>
		/// List of overlays being managed by this <see cref="ScreenManager"/>.
		/// </summary>
		private readonly List<GameOverlay> overlays = new List<GameOverlay>();
		/// <summary>
		/// List of overlays that will be updated on the next call to <see cref="Update"/>.
		/// </summary>
		private readonly List<GameOverlay> overlaysToUpdate = new List<GameOverlay>();

		/// <summary>
		/// Determines whether the game should perform calculations
		/// on this frame.
		/// </summary>
		/// <remarks>
		/// See <see cref="Settings.UpdateInterval"/> for more information.
		/// </remarks>
		private bool shouldCalculate;

		/// <summary>
		/// Input State
		/// </summary>
		public readonly InputState Input = new InputState();

		/// <summary>
		/// Shortcut to <see cref="GraphicsDevice"/>.
		/// </summary>
		public static GraphicsDevice Graphics => instance.GraphicsDevice;

		/// <summary>
		/// A shared <see cref="SpriteBatch"/> for all screens.
		/// </summary>
		public static SpriteBatch SpriteBatch { get; private set; }

		/// <summary>
		/// A single while pixel used for drawing.
		/// </summary>
		public static Texture2D Pixel { get; private set; }

		/// <summary>
		/// If true, a list of screens and overlays will be printed to the trace log
		/// on every update. For debugging purposes only.
		/// </summary>
		public bool TraceEnabled { get; set; }

		private bool isInitialized;

		private static ScreenManager instance;

		/// <summary>
		/// Creates a new screen manager component.
		/// </summary>
		private ScreenManager(Game game) : base(game)
		{ }

		/// <summary>
		/// Creates a new ScreenManager instance if it is
		/// not already instantiated.
		/// </summary>
		public static ScreenManager Create(Game game)
		{
			if (instance != null)
				throw new Exception("The screen manager has already been initialized.");

			return instance = new ScreenManager(game);
		}

		/// <summary>
		/// Initializes the screen manager component.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			isInitialized = true;
		}

		/// <summary>
		/// Loads necessary contents for this screen manager
		/// and its screens.
		/// </summary>
		protected override void LoadContent()
		{
			SpriteBatch = new SpriteBatch(Graphics);

			Pixel = new Texture2D(Graphics, 1, 1);
			Pixel.SetData(new[] { Color.White });

			// Load all screens contents.
			foreach (var gameScreen in screens)
				gameScreen.Load();

			foreach (var gameOverlay in overlays)
				gameOverlay.Load();
		}

		/// <summary>
		/// Unloads content that was loaded for this screen manager
		/// and its screens.
		/// </summary>
		protected override void UnloadContent()
		{
			foreach (var gameScreen in screens)
				gameScreen.Unload();

			foreach (var gameOverlay in overlays)
				gameOverlay.UnloadContent();
		}

		/// <summary>
		/// Updates the logic for each screen of this screen manager.
		/// </summary>
		public override void Update(GameTime gameTime)
		{
			// Skip every other calculation (essentially doubling the timestep)
			shouldCalculate = !shouldCalculate;

			// Update Input State
			Input.Update();

			// Reset Mouse Cursor
			Data.CursorType = CursorType.Pointer;

			var coveredByOtherScreen = false;
			var otherScreenHasFocus = !Game.IsActive;

			// Make a copy of the overlay list, in case an update
			// causes an overlay to be added to or removed from it.
			overlaysToUpdate.Clear();
			overlaysToUpdate.AddRange(overlays);

			while (overlaysToUpdate.Count > 0)
			{
				// Pop off top-most overlay.
				var overlay = overlaysToUpdate[overlaysToUpdate.Count - 1];
				overlaysToUpdate.RemoveAt(overlaysToUpdate.Count - 1);

				// Update overlay
				overlay.Update(gameTime);

				// If this is the first overlay that handles input, let it.
				if (!otherScreenHasFocus && overlay.HandlesInput)
				{
					overlay.HandleInput(Input);
					otherScreenHasFocus = true;
				}
			}

			// Make a copy of the screen list, in case an update
			// causes a screen to be added to or removed from it.
			screensToUpdate.Clear();
			screensToUpdate.AddRange(screens);

			// Continue updating as long as there are screens
			// that need to be updated.
			while (screensToUpdate.Count > 0)
			{
				// Pop off top-most screen.
				var screen = screensToUpdate[screensToUpdate.Count - 1];
				screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

				if (shouldCalculate)
					screen.Calculate(gameTime, coveredByOtherScreen);

				// Update screen
				screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

				if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active)
				{
					// If this is the first active screen, let it handle input.
					if (!otherScreenHasFocus)
					{
						screen.HandleInput(Input);
						otherScreenHasFocus = true;
					}

					// If this screen is modal, block screens that are
					// covered by it.
					if (!screen.IsModal)
						coveredByOtherScreen = true;
				}
			}

			// If enabled, print the list of screens to the trace file.
			if (TraceEnabled)
			{
				Trace.TraceInformation(string.Join(", ", overlays.Select(a => a.GetType().Name)));
				Trace.TraceInformation(string.Join(", ", screens.Select(a => a.GetType().Name)));
			}
		}

		/// <summary>
		/// Tells each screen to draw.
		/// </summary>
		public override void Draw(GameTime gameTime)
		{
			foreach (var gameScreen in screens)
			{
				if (gameScreen.ScreenState == ScreenState.Hidden)
					continue;

				gameScreen.BeginDraw(gameTime);
			}

			Graphics.SetRenderTarget(null);
			SpriteBatch.Begin();

			foreach (var gameScreen in screens)
			{
				if (gameScreen.ScreenState == ScreenState.Hidden)
					continue;

				gameScreen.Draw(gameTime);
			}

			foreach (var gameOverlay in overlays)
			{
				gameOverlay.Draw(gameTime);
			}

			if (Data.GameReady)
			{
				// Draw Cursor
				var clampedMousePosition = new Point(MathHelper.Clamp(Input.CurrentMouseState.X, 0, Graphics.Viewport.Width), MathHelper.Clamp(Input.CurrentMouseState.Y, 0, Graphics.Viewport.Height));
				Data.CursorSet.DrawCursor(clampedMousePosition.X, clampedMousePosition.Y, Data.CursorIndices[(int)Data.CursorType]);
			}

			SpriteBatch.End();
		}

		/// <summary>
		/// Adds <paramref name="screen"/> to this screen manager.
		/// </summary>
		public void AddScreen(GameScreen screen)
		{
			screen.ScreenManager = this;

			if (isInitialized)
				screen.Load();

			screens.Add(screen);
		}

		/// <summary>
		/// Removes <paramref name="screen"/> from this game manager.
		/// </summary>
		public void RemoveScreen(GameScreen screen)
		{
			if (isInitialized)
				screen.Unload();

			screens.Remove(screen);
			screensToUpdate.Remove(screen);
		}

		/// <summary>
		/// Removes the top-most screen from this game manager.
		/// </summary>
		public void PopScreen()
		{
			var screen = screens.LastOrDefault();
			if (screen == null)
				return;

			RemoveScreen(screen);
		}

		/// <summary>
		/// Adds <paramref name="overlay"/> to this screen manager.
		/// </summary>
		public void AddOverlay(GameOverlay overlay)
		{
			overlay.ScreenManager = this;

			if (isInitialized)
				overlay.Load();

			overlays.Add(overlay);
		}

		/// <summary>
		/// Removes <paramref name="overlay"/> from this game manager.
		/// </summary>
		public void RemoveOverlay(GameOverlay overlay)
		{
			if (isInitialized)
				overlay.Unload();

			overlays.Remove(overlay);
			overlaysToUpdate.Remove(overlay);
		}

		/// <summary>
		/// Draws a translucent black full-screen sprite, used for fading
		/// screens in and out, and for darkening the background behind modals.
		/// </summary>
		/// <remarks>
		/// This doesn't work yet.
		/// </remarks>
		public void FadeBackBufferToBlack(float alpha)
		{
			SpriteBatch.Draw(null, new Rectangle(0, 0, Graphics.Viewport.Width, Graphics.Viewport.Height), Color.Black * alpha);
		}
	}
}