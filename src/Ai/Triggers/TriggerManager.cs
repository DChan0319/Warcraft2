using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Warcraft.Util;
// ReSharper disable InconsistentNaming

namespace Warcraft.Ai.Triggers
{
	public static class TriggerManager
	{
		private static Dictionary<string, Trigger> TriggerMap { get; }
		public static bool IsFileOpened { get; private set; }
		public static bool IsValidTriggerFile { get; private set; }

		static TriggerManager()
		{
			TriggerMap = new Dictionary<string, Trigger>();
			IsFileOpened = false;
			IsValidTriggerFile = true;
		}

		private static void AddTrigger(string triggerName, Trigger trigger)
		{
			if (!TriggerMap.ContainsKey(triggerName))
				TriggerMap.Add(triggerName, trigger);
		}

		public static void CheckTriggers(IntPtr L, int player)
		{
			foreach (var trigger in TriggerMap)
			{
				if (trigger.Value.Player != player && trigger.Value.Player != -1 || trigger.Value.HasBeenTriggered && !trigger.Value.IsPersistent) continue;
				if (!trigger.Value.CheckTrigger(L)) continue;

				trigger.Value.RunEvent(L);
				trigger.Value.HasBeenTriggered = true;
			}
		}

		public static void Reset()
		{
			Trace.TraceInformation("TriggerManager: Resetting Trigger Manager");
			TriggerMap.Clear();

			IsFileOpened = false;
			IsValidTriggerFile = true;
		}

		public static void ParseTriggerCsv(string fileName)
		{
			fileName += ".csv";
			var filePath = Path.Combine(Paths.Triggers, fileName);

			if (!File.Exists(filePath))
				return;

			try
			{
				using (var reader = new StreamReader(filePath))
				{
					while (!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						if (string.IsNullOrEmpty(line)) continue;

						var tokens = line.Split(',');

						var player = int.Parse(tokens[0]);
						var triggerName = tokens[1];
						var triggerType = tokens[2];
						var isPersistent = int.Parse(tokens[3]) != 0;

						switch (triggerType)
						{
							case "TriggerTypeTime":
								{
									var time = int.Parse(tokens[4]);
									// dummy token here
									var eventName = tokens[6];

									var timeTrigger = new TimeTrigger(eventName, isPersistent, player, time);
									for (var i = 7; i < tokens.Length; i++)
									{
										timeTrigger.EventArgs.Add(int.Parse(tokens[i]));
									}

									AddTrigger(triggerName, timeTrigger);
								}
								break;

							case "TriggerTypeResource":
								{
									var resourceType = tokens[4];
									var resourceAmount = int.Parse(tokens[5]);
									var eventName = tokens[6];

									var resourceTrigger = new ResourceTrigger(eventName, isPersistent, player, resourceType, resourceAmount);
									for (var i = 7; i < tokens.Length; i++)
									{
										resourceTrigger.EventArgs.Add(int.Parse(tokens[i]));
									}

									AddTrigger(triggerName, resourceTrigger);
								}
								break;

							case "TriggerTypeLocation":
								{
									var positionX = int.Parse(tokens[4]);
									var positionY = int.Parse(tokens[5]);
									var eventName = tokens[6];

									var locationTrigger = new LocationTrigger(eventName, isPersistent, player, positionX, positionY);
									for (var i = 7; i < tokens.Length; i++)
									{
										locationTrigger.EventArgs.Add(int.Parse(tokens[i]));
									}

									AddTrigger(triggerName, locationTrigger);
								}
								break;

							case "TriggerTypeAssetObtained":
								{
									var assetType = int.Parse(tokens[4]);
									var assetAmount = int.Parse(tokens[5]);
									var eventName = tokens[6];

									var assetObtainedTrigger = new AssetObtainedTrigger(eventName, isPersistent, player, assetType, assetAmount);
									for (var i = 7; i < tokens.Length; i++)
									{
										assetObtainedTrigger.EventArgs.Add(int.Parse(tokens[i]));
									}

									AddTrigger(triggerName, assetObtainedTrigger);
								}
								break;

							case "TriggerTypeAssetLost":
								{
									var assetType = int.Parse(tokens[4]);
									var assetAmount = int.Parse(tokens[5]);
									var eventName = tokens[6];

									var assetLostTrigger = new AssetLostTrigger(eventName, isPersistent, player, assetType, assetAmount);
									for (var i = 7; i < tokens.Length; i++)
									{
										assetLostTrigger.EventArgs.Add(int.Parse(tokens[i]));
									}

									AddTrigger(triggerName, assetLostTrigger);
								}
								break;

							default:
								Trace.TraceInformation($"TriggerManager: Unrecognized trigger type '{triggerType}'.");
								break;
						}
					}

					IsValidTriggerFile = true;
					IsFileOpened = true;
				}
			}
			catch (Exception e)
			{
				Trace.TraceError("An exception occured while trying to parse the trigger file.");
				Trace.TraceError(e.ToString());
				IsFileOpened = false;
				IsValidTriggerFile = false;
			}
		}
	}
}