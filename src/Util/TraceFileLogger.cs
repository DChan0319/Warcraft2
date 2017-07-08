using System;
using System.Diagnostics;
using System.IO;

namespace Warcraft.Util
{
	/// <summary>
	/// TraceListener Rollover to log <see cref="Trace"/> to a text file.
	/// </summary>
	internal class TraceFileLogger : TraceListener
	{
		private readonly StreamWriter sw;

		private static readonly object Lock = new object();

		public TraceFileLogger(string filename)
		{
			sw = new StreamWriter(filename);
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			WriteLine($"{DateTime.Now} [{(eventType == TraceEventType.Information ? "Info" : eventType.ToString())}] - {message}");
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			TraceEvent(eventCache, source, eventType, id, string.Format(format, args));
		}

		public override void Write(string message)
		{
			sw.Write(message);
		}

		public override void WriteLine(string message)
		{
			sw.WriteLine(message);
		}

		public override void Flush()
		{
			lock (Lock)
				sw.Flush();
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			sw.Close();
			sw.Dispose();
		}
	}
}
