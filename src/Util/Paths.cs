using System;
using System.IO;

namespace Warcraft.Util
{
	/// <summary>
	/// Static class for containing data directory paths.
	/// </summary>
	public static class Paths
	{
		/// <summary>
		/// Data Directory Path
		/// </summary>
		public const string Data = "data";

		/// <summary>
		/// Image Directory Path
		/// </summary>
		public static readonly string Image = Path.Combine(Data, "img");

		/// <summary>
		/// Map Directory Path
		/// </summary>
		public static readonly string Map = Path.Combine(Data, "map");

		/// <summary>
		/// Resource Directory Path
		/// </summary>
		public static readonly string Resource = Path.Combine(Data, "res");

		/// <summary>
		/// Scripts Directory Path
		/// </summary>
		public static readonly string Scripts = Path.Combine(Data, "scripts");

		/// <summary>
		/// Triggers Directory Path
		/// </summary>
		public static readonly string Triggers = Path.Combine(Data, "triggers");

		/// <summary>
		/// Sound Directory Path
		/// </summary>
		public static readonly string Sound = Path.Combine(Data, "snd");

		/// <summary>
		/// Upgrade Directory Path
		/// </summary>
		public static readonly string Upgrade = Path.Combine(Data, "upg");

		/// <summary>
		/// Returns the relative path of <paramref name="file"/> to the current working directory of the application.
		/// </summary>
		public static string GetRelativePath(string file)
		{
			var folder = Environment.CurrentDirectory;
			if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
				folder += Path.DirectorySeparatorChar;

			var pathUri = new Uri(file);
			var folderUri = new Uri(folder);

			return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}
	}
}