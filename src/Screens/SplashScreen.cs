using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Warcraft.App;
using Warcraft.Audio;
using Warcraft.Screens.Base;
using Warcraft.Screens.Manager;
using Warcraft.Util;

namespace Warcraft.Screens
{
	/// <summary>
	/// The Splash Screen... Screen
	/// This is the first window that shows up when starting the game.
	/// After some time, or pressing the Escape key, the splash screen
	/// will disappear, and the game's main menu will show up.
	/// </summary>
	public class SplashScreen : GameScreen
	{
		/// <summary>
		/// Splash Screen Texture
		/// Contains a faded and non-faded splash screen image.
		/// </summary>
		private Texture2D splashScreenTexture;
		/// <summary>
		/// Defines the area of the <see cref="splashScreenTexture"/>
		/// where the non-faded splash image lies.
		/// </summary>
		private Rectangle splashScreenRectangle = new Rectangle(0, 0, 800, 600);
		/// <summary>
		/// Defines the area of the <see cref="splashScreenTexture"/>
		/// where the faded splash image lies.
		/// </summary>
		private Rectangle splashScreenFadedRectangle = new Rectangle(0, 600, 800, 600);
		/// <summary>
		/// Controls the transparency of the faded splash screen image.
		/// </summary>
		private float splashScreenAlpha = 1.0f;

		/// <summary>
		/// If set to true, the splash screen will be skipped on the next update.
		/// </summary>
		/// <remarks>
		/// Pressing the Escape key will set this to true.
		/// </remarks>
		private bool SkipSplashScreen { get; set; }

		public override void LoadContent()
		{
			AudioManager.PlayMidi("intro", true, Settings.General.MusicVolume);

			// Load the splash screen
			using (var splashScreenStream = new FileStream(Path.Combine(Paths.Image, "Splash.png"), FileMode.Open))
				splashScreenTexture = Texture2D.FromStream(ScreenManager.Graphics, splashScreenStream);
		}

		public override void HandleInput(InputState input)
		{
			base.HandleInput(input);

			if (input.WasKeyPressed(Keys.Escape))
			{
				// Set the flag to skip the splash screen on the next update.
				SkipSplashScreen = true;
			}
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

			// Check if the loading task has thrown any exceptions.
			// If so, throw them back up to the main try/catch.
			if (Data.LoadingTask.IsFaulted && Data.LoadingTask.Exception != null)
				throw Data.LoadingTask.Exception.Flatten();

			// After 5 seconds or hitting the escape key,
			// exit the splash screen and start the main menu.
			if (Data.LoadingTask.IsCompleted && (gameTime.TotalGameTime > TimeSpan.FromSeconds(5) || SkipSplashScreen) && !IsExiting)
			{
				Data.LoadStopwatch.Stop();
				Trace.TraceInformation($"Warcraft: Finished loading game in {Data.LoadStopwatch.Elapsed.TotalSeconds:0.000} seconds.");

				splashScreenAlpha = 0;
				IsExiting = true;
				ScreenManager.Game.Window.IsBorderless = false;
				ScreenManager.Game.Window.AllowUserResizing = true;
				ScreenManager.AddScreen(new MainMenuScreen());
				ScreenManager.AddOverlay(new FpsOverlay());
				Data.GameReady = true;
			}

			// Make the faded image disappear slowly
			if (splashScreenAlpha > 0)
				splashScreenAlpha -= 0.005f;
		}

		public override void Draw(GameTime gameTime)
		{
			// Draw the faded image over the non-faded.
			// Slowly, the faded image disappears, and only the non-faded will be shown.
			ScreenManager.SpriteBatch.Draw(splashScreenTexture, Vector2.Zero, splashScreenRectangle, Color.White);
			if (splashScreenAlpha > 0)
				ScreenManager.SpriteBatch.Draw(splashScreenTexture, Vector2.Zero, splashScreenFadedRectangle, Color.White * splashScreenAlpha);
		}
	}
}