using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Audio;
using Warcraft.Player;
using Warcraft.Player.Capabilities;
using Warcraft.Screens;
using Warcraft.Screens.Manager;

namespace Warcraft
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Warcraft : Game
	{
		private GraphicsDeviceManager graphics;

		public Warcraft()
		{
			Trace.TraceInformation("Warcraft: Loading game...");

			Data.LoadSettings();

			graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 800,
				PreferredBackBufferHeight = 600
			};
			Window.IsBorderless = true;
			Window.ClientSizeChanged += Window_ClientSizeChanged;

			IsFixedTimeStep = true;
			TargetElapsedTime = TimeSpan.FromMilliseconds(Settings.UpdateInterval);

			// Create the screen manager component
			var screenManager = ScreenManager.Create(this);
			Components.Add(screenManager);

			AudioManager.LoadFiles();
			// Activate the splash screen
			screenManager.AddScreen(new SplashScreen());
		}

		/// <summary>
		/// Invoked when the size of the window is changed.
		/// </summary>
		private void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
			graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
			graphics.ApplyChanges();
			Data.ResizeWindow();
		}

		/// <summary>
		/// Loads content necessary for the entirety of the game.
		/// </summary>
		protected override void LoadContent()
		{
			Data.LoadStopwatch.Start();

			Data.LoadingTask = Task.Run(() =>
			{
				DecoratedMap.LoadMaps();
				PlayerAssetData.LoadTypes();
				PlayerUpgrade.LoadUpgrades();
				RegisterCapabilities();
				Data.Load();
			});
		}

		/// <summary>
		/// Looks for all classes with <see cref="PlayerCapabilityRegistrant"/>
		/// and runs its Register method.
		/// </summary>
		private static void RegisterCapabilities()
		{
			Trace.TraceInformation("Warcraft: Loading capabilities...");
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes().AsParallel().Where(type => !type.IsAbstract))
			{
				var attributes = type.GetCustomAttributes(typeof(PlayerCapabilityRegistrant), true);
				if (attributes.Length <= 0)
					continue;

				Activator.CreateInstance(type);
			}
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// The actual drawing happens in the individual screens.

			base.Draw(gameTime);
		}

		/// <summary>
		/// This is called when the game is instructed to begin exiting.
		/// </summary>
		protected override void OnExiting(object sender, EventArgs args)
		{
			AudioManager.SetVolume(0);
			Data.SaveSettings();
			Data.SaveGame();
		}
	}
}
