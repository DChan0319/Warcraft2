using System;
using System.Diagnostics;
using System.Windows.Forms;
using Warcraft.Util;

namespace Warcraft
{
#if WINDOWS || LINUX
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			InitializeLogger();

			using (var game = new Warcraft())
			{
				if (!Debugger.IsAttached)
				{
					try
					{
						game.Run();
					}
					catch (Exception ex)
					{
						Trace.TraceError(ex.ToString());
						MessageBox.Show("An unhandled exception was caught. Please check the error log." + Environment.NewLine + Environment.NewLine + ex.Message, "Warcraft");
					}
				}
				else
				{
					game.Run();
				}
			}
		}

		private static void InitializeLogger()
		{
			Trace.Listeners.Add(new TraceFileLogger("error.log"));
			Trace.AutoFlush = true;
		}
	}
#endif
}
