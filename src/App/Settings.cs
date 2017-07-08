using Newtonsoft.Json;

namespace Warcraft.App
{
	/// <summary>
	/// Settings File
	/// </summary>
	/// <remarks>
	/// Variables in here are saved to <see cref="SettingsFileName"/>.
	/// </remarks>
	public class Settings
	{
		/// <summary>
		/// The time between each update.
		/// </summary>
		/// <remarks>
		/// In the Linux version, the update interval is set at 50 ms.
		/// This limits the frame rate to 20 FPS.
		/// 
		/// However, we don't want to limit the frame rate to such a low FPS.
		/// Unfortunately, we're also tied down to a 1:1 Update/Draw call ratio
		/// (i.e. one draw per update).
		/// 
		/// A hacky workaround to this is to half the update interval
		/// and skip every other Calculate call. This way, each Draw call is
		/// 25 ms apart, while each Calculate call is essentially 50 ms apart.
		/// This allows us to achieve 40 FPS while keeping the timestep in
		/// sync with the Linux version.
		/// </remarks>
		public const int UpdateInterval = 25;

		/// <summary>
		/// The number of update per second.
		/// </summary>
		/// <remarks>
		/// We have to double the denominator to account for the
		/// skipping of every other Calculate call.
		/// </remarks>
		public const int UpdateFrequency = 1000 / (2 * UpdateInterval);

		/// <summary>
		/// The name of the file where the game will save to/load from.
		/// </summary>
		public const string SettingsFileName = "settings.json";

		/// <summary>
		/// The name of the file where the game will save to/load from.
		/// </summary>
		public const string SaveFileName = "saved-game.dat";

		public class GeneralSettings
		{
			/// <summary>
			/// Sound Effect Volume Level
			/// </summary>
			public float SfxVolume = 1.0f;

			/// <summary>
			/// Music Volume Level
			/// </summary>
			public float MusicVolume = 1.0f;

			private GeneralSettings() { }
			public static GeneralSettings Instance { get; } = new GeneralSettings();
		}

		public class DebugSettings
		{
			/// <summary>
			/// If true, the entire map will be visible to the player.
			/// </summary>
			public bool Flash = false;

			/// <summary>
			/// Determines the game speed factor.
			/// </summary>
			/// <example>
			/// SpeedFactor = 1 => Normal Speed
			/// SpeedFactor = 2 => Double Speed
			/// SpeedFactor = 4 => Quadruple Speed
			/// </example>
			/// <remarks>
			/// Setting this too high will result in problems with the game.
			/// </remarks>
			public int SpeedFactor = 1;

			/// <summary>
			/// If true, allows the player to control
			/// enemy units (in single player mode).
			/// </summary>
			public bool ControlEnemies = false;

			/// <summary>
			/// If true, AI players will be enabled.
			/// </summary>
			public bool EnableAi = true;

			/// <summary>
			/// If true, AI players will use lua scripts,
			/// otherwise, the built in AiPlayer.
			/// </summary>
			public bool UseAiScripts = true;

			private DebugSettings() { }
			public static DebugSettings Instance { get; } = new DebugSettings();
		}

		[JsonProperty]
		public static GeneralSettings General { get; } = GeneralSettings.Instance;

		[JsonProperty]
		public static DebugSettings Debug { get; } = DebugSettings.Instance;

		public static Settings Default
		{
			get { return instance ?? (instance = new Settings()); }
			set { instance = value; }
		}
		private static Settings instance;

		private Settings() { }
	}
}