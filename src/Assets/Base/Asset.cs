using System.Diagnostics;
using System.IO;

namespace Warcraft.Assets.Base
{
	/// <summary>
	/// Abstract class from which all asset classes derive from.
	/// </summary>
	public abstract class Asset
	{
		/// <summary>
		/// Loads and reads from <paramref name="fileName"/> and creates an asset.
		/// </summary>
		public void Load(string fileName)
		{
			Trace.TraceInformation($"{GetType().Name}: Loading '{fileName}'...");
			var sw = new Stopwatch();
			sw.Start();

			using (var dataFile = new StreamReader(fileName))
				Load(dataFile);

			sw.Stop();
			Trace.TraceInformation($"{GetType().Name}: Finished loading from '{fileName}' in {sw.Elapsed.TotalSeconds:0.000} seconds.");
		}

		/// <summary>
		/// Reads from <paramref name="dataFile"/> and creates an asset.
		/// </summary>
		protected abstract void Load(TextReader dataFile);
	}
}
