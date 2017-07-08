using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FluidSynthWrapper;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Audio;
using Warcraft.Util;

namespace Warcraft.Audio
{
	/// <summary>
	/// Manages music and sound effect playback.
	/// </summary>
	public static class AudioManager
	{
		/// <summary>
		/// Contains preloaded wave file data, indexed by the clip name.
		/// </summary>
		public static readonly Dictionary<string, SoundEffect> ClipData = new Dictionary<string, SoundEffect>();
		/// <summary>
		/// A list of <see cref="SoundEffectInstance"/>s for disposing done by <see cref="cleanupTimer"/>.
		/// </summary>
		private static readonly List<SoundEffectInstance> SoundEffectInstances = new List<SoundEffectInstance>();

		private static readonly Dictionary<string, string> MidiFiles = new Dictionary<string, string>();

		// FluidSynth components
		private static Synthesizer fluidSynth;
		private static Settings fluidSettings;
		[UsedImplicitly]
		private static AudioDriver audioDriver;
		private static FluidSynthWrapper.Player fluidPlayer;

		/// <summary>
		/// Timer for cleaning up <see cref="SoundEffectInstances"/>.
		/// </summary>
		[UsedImplicitly]
		private static Timer cleanupTimer;

		static AudioManager()
		{
			// Initialize midi synthesizer
			fluidSettings = new Settings
			{
				AudioDriver = "dsound",
				SynthAudioChannels = 256,
				SynthSampleRate = 44100
			};
			fluidSynth = new Synthesizer(fluidSettings);
			audioDriver = new AudioDriver(fluidSettings, fluidSynth);

			SetupCleanupTimer();
		}

		private static void SetupCleanupTimer()
		{
			cleanupTimer = new Timer(_ =>
			{
				// Get all instances that have finished playing
				var cleanupList = SoundEffectInstances.FindAll(sfxi => sfxi.State == SoundState.Stopped && !sfxi.IsDisposed);
				SoundEffectInstances.RemoveAll(sfxi => sfxi.State == SoundState.Stopped && !sfxi.IsDisposed);

				// Dispose those instances
				cleanupList.ForEach(sfxi => sfxi.Dispose());
			}, null, 1000, 1000);
		}

		/// <summary>
		/// Loads all wave (.wav) files from the into <see cref="ClipData"/>.
		/// </summary>
		public static void LoadFiles()
		{
			ClipData.Clear();

			Trace.TraceInformation("AudioManager: Loading audio files...");
			var sw = new Stopwatch();
			sw.Start();

			using (var dataFile = new StreamReader(Path.Combine(Paths.Sound, "SoundClips.dat")))
			{
				int numWavFiles;
				if (!int.TryParse(dataFile.ReadLine(), out numWavFiles))
					throw new FormatException("Invalid data file format.");

				for (var i = 0; i < numWavFiles; i++)
				{
					var clipName = dataFile.ReadLine();
					var clipFile = dataFile.ReadLine();

					if (clipFile == null || clipName == null)
						throw new FormatException("Invalid data file format.");

					using (var ms = new MemoryStream(File.ReadAllBytes(Path.Combine(Paths.Sound, clipFile))))
						ClipData[clipName] = SoundEffect.FromStream(ms);
				}

				var sf2FilePath = dataFile.ReadLine();
				if (sf2FilePath == null)
					throw new FormatException("Invalid data file format.");

				var sf2File = Path.Combine(Paths.Sound, sf2FilePath);
				if (!File.Exists(sf2File))
					throw new FileNotFoundException("SoundFont2 file not found.", sf2File);
				if (Synthesizer.IsSoundFont(sf2File))
					fluidSynth.SFontLoad(sf2File);

				int numMidFiles;
				if (!int.TryParse(dataFile.ReadLine(), out numMidFiles))
					throw new FormatException("Invalid data file format.");

				for (var i = 0; i < numMidFiles; i++)
				{
					var clipName = dataFile.ReadLine();
					var clipFile = dataFile.ReadLine();

					if (clipFile == null || clipName == null)
						throw new FormatException("Invalid data file format.");

					MidiFiles.Add(clipName, clipFile);
				}
			}

			sw.Stop();
			Trace.TraceInformation($"AudioManager: Finished loading audio files in {sw.Elapsed.TotalSeconds:0.000} seconds.");
		}

		/// <summary>
		/// Plays a wave (.wav) file.
		/// </summary>
		/// <param name="clipName">Name of the wave clip (not the file name).</param>
		/// <param name="volume">Volume level to play the clip at.</param>
		/// <param name="bias">Changes the volume balance between speakers (stereo volume)</param>
		/// <param name="loop">If true, playback will loop.</param>
		public static void PlayWave(string clipName, float volume = 1.0f, float bias = 0.0f, bool loop = false)
		{
			if (!ClipData.ContainsKey(clipName))
				throw new FileNotFoundException("The specified wave is not loaded.", clipName);

			var sfxi = ClipData[clipName].CreateInstance();
			sfxi.IsLooped = loop;
			sfxi.Volume = volume;
			sfxi.Pan = bias;
			sfxi.Play();
			SoundEffectInstances.Add(sfxi);
		}

		/// <summary>
		/// Plays a midi (.mid) file.
		/// </summary>
		/// <param name="clipName">Name of the midi clip (not the file name).</param>
		/// <param name="loop">If true, playback will loop.</param>
		/// <param name="volume">The volume level to play the song at</param>
		public static void PlayMidi(string clipName, bool loop = true, float volume = 1.0f)
		{
			if (fluidPlayer != null)
			{
				fluidPlayer.Stop();
				fluidPlayer.Dispose();
			}

			fluidSettings.SynthGain = 0.2f * volume;
			fluidPlayer = new FluidSynthWrapper.Player(fluidSynth, Path.Combine(Paths.Sound, MidiFiles[clipName]));
			fluidPlayer.SetLoop(loop ? -1 : 1);
			fluidPlayer.Play();
		}

		/// <summary>
		/// Stops the midi player.
		/// </summary>
		public static void StopMidi()
		{
			fluidSettings.SynthGain = 0;
			fluidPlayer?.Stop();
		}

		/// <summary>
		/// Sets the volume of the midi player.
		/// </summary>
		public static void SetVolume(float volume)
		{
			fluidSettings.SynthGain = 0.2 * volume;
		}
	}
}
