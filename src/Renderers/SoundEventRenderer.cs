using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Audio;
using Warcraft.GameModel;
using Warcraft.Player;

namespace Warcraft.Renderers
{
	/// <summary>
	/// Processes game event sounds and sound effects.
	/// </summary>
	public class SoundEventRenderer
	{
		private PlayerData Player { get; }
		public float Volume { get; set; }
		private Random Random { get; }

		private List<GameEvent> DelayedEvents { get; }
		private const string PlaceClip = "place";
		private const string TickClip = "tick";
		private List<int> DelayedSelectionIndices { get; }
		private List<int> DelayedAcknowledgementIndices { get; }
		private List<string> ConstructIndices { get; }
		private Dictionary<AssetType, List<string>> HarvestIndices { get; }
		private Dictionary<AssetType, List<string>> QuarryIndices { get; }
		private Dictionary<AssetType, List<string>> WorkCompleteIndices { get; }
		private Dictionary<AssetType, List<string>> SelectionIndices { get; }
		private Dictionary<AssetType, List<string>> AcknowledgeIndices { get; }
		private Dictionary<AssetType, List<string>> ReadyIndices { get; }
		private Dictionary<AssetType, List<string>> DeathIndices { get; }
		private Dictionary<AssetType, List<string>> AttackedIndices { get; }
		private Dictionary<AssetType, List<string>> MissileFireIndices { get; }
		private Dictionary<AssetType, List<string>> MissileHitIndices { get; }
		private Dictionary<AssetType, List<string>> MeleeHitIndices { get; }

		public SoundEventRenderer(PlayerData player)
		{
			Trace.TraceInformation($"{GetType().Name}: Setting up...");
			var sw = new Stopwatch();
			sw.Start();

			Player = player;
			// Linux: The Linux version seems to keep this at 1.0f, and is never
			//        updated. That means it will always play at default volume.
			Volume = Settings.General.SfxVolume;
			Random = new Random();

			var names = new Dictionary<AssetType, string>
			{
				[AssetType.None] = "basic",
				[AssetType.Wall] = "",
				[AssetType.Peasant] = "peasant",
				[AssetType.Footman] = "footman",
				[AssetType.Knight] = "knight",
				[AssetType.Archer] = "archer",
				[AssetType.Ranger] = "archer",
				[AssetType.GoldMine] = "gold-mine",
				[AssetType.TownHall] = "town-hall",
				[AssetType.Keep] = "keep",
				[AssetType.Castle] = "castle",
				[AssetType.Farm] = "farm",
				[AssetType.Barracks] = "barracks",
				[AssetType.LumberMill] = "lumber-mill",
				[AssetType.Blacksmith] = "blacksmith",
				[AssetType.ScoutTower] = "scout-tower",
				[AssetType.GuardTower] = "guard-tower",
				[AssetType.CannonTower] = "cannon-tower"
			};

			DelayedEvents = new List<GameEvent>();

			DelayedSelectionIndices = new List<int>(new int[(int)AssetType.Max]);
			for (var i = 0; i < DelayedSelectionIndices.Count; i++)
				DelayedSelectionIndices[i] = -1;

			DelayedAcknowledgementIndices = new List<int>(new int[(int)AssetType.Max]);
			for (var i = 0; i < DelayedAcknowledgementIndices.Count; i++)
				DelayedAcknowledgementIndices[i] = -1;

			HarvestIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				HarvestIndices[typeIndex] = new List<string>();

			QuarryIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				QuarryIndices[typeIndex] = new List<string>();

			WorkCompleteIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				WorkCompleteIndices[typeIndex] = new List<string>();

			SelectionIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				SelectionIndices[typeIndex] = new List<string>();

			AcknowledgeIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				AcknowledgeIndices[typeIndex] = new List<string>();

			ReadyIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				ReadyIndices[typeIndex] = new List<string>();

			DeathIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				DeathIndices[typeIndex] = new List<string>();

			AttackedIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				AttackedIndices[typeIndex] = new List<string>();

			MissileFireIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				MissileFireIndices[typeIndex] = new List<string>();

			MissileHitIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				MissileHitIndices[typeIndex] = new List<string>();

			MeleeHitIndices = new Dictionary<AssetType, List<string>>();
			for (var typeIndex = AssetType.None; typeIndex < AssetType.Max; typeIndex++)
				MeleeHitIndices[typeIndex] = new List<string>();

			// Construct
			ConstructIndices = new List<string>();
			var clipIndex = 1;
			var clipName = "construct";
			if (AudioManager.ClipData.ContainsKey(clipName))
				ConstructIndices.Add(clipName);
			else
			{
				clipName = "construct" + clipIndex;
				while (AudioManager.ClipData.ContainsKey(clipName))
				{
					ConstructIndices.Add(clipName);
					clipName = "construct" + ++clipIndex;
				}
			}

			for (AssetType typeIndex = 0; typeIndex < AssetType.Max; typeIndex++)
			{
				PlayerAssetData assetType;
				if (!Player.AssetDatas.TryGetValue(PlayerAssetData.TypeToName(typeIndex), out assetType))
					continue;

				// Work Complete
				clipName = names[typeIndex] + "-work-completed";
				if (AudioManager.ClipData.ContainsKey(clipName))
					WorkCompleteIndices[typeIndex].Add(clipName);
				else if (WorkCompleteIndices[AssetType.None].Count != 0)
					WorkCompleteIndices[typeIndex].Add(WorkCompleteIndices[AssetType.None].First());

				// Selected
				clipName = names[typeIndex] + "-selected";
				if (AudioManager.ClipData.ContainsKey(clipName))
					SelectionIndices[typeIndex].Add(clipName);
				else
				{
					clipIndex = 1;
					clipName = names[typeIndex] + "-selected" + clipIndex;
					while (AudioManager.ClipData.ContainsKey(clipName))
					{
						SelectionIndices[typeIndex].Add(clipName);
						clipName = names[typeIndex] + "-selected" + ++clipIndex;
					}

					if (SelectionIndices[typeIndex].Count == 0 && assetType.Speed != 0 && SelectionIndices[AssetType.None].Count != 0)
						SelectionIndices[typeIndex] = SelectionIndices[AssetType.None];
				}

				// Acknowledge
				clipName = names[typeIndex] + "-acknowledge";
				if (AudioManager.ClipData.ContainsKey(clipName))
					AcknowledgeIndices[typeIndex].Add(clipName);
				else
				{
					clipIndex = 1;
					clipName = names[typeIndex] + "-acknowledge" + clipIndex;
					while (AudioManager.ClipData.ContainsKey(clipName))
					{
						AcknowledgeIndices[typeIndex].Add(clipName);
						clipName = names[typeIndex] + "-acknowledge" + ++clipIndex;
					}

					if (AcknowledgeIndices[typeIndex].Count == 0 && AcknowledgeIndices[AssetType.None].Count != 0)
						AcknowledgeIndices[typeIndex] = AcknowledgeIndices[AssetType.None];
				}

				// Ready
				clipName = names[typeIndex] + "-ready";
				if (AudioManager.ClipData.ContainsKey(clipName))
					ReadyIndices[typeIndex].Add(clipName);
				else if (typeIndex == AssetType.Footman)
				{
					clipName = names[AssetType.None] + "-ready";
					if (AudioManager.ClipData.ContainsKey(clipName))
						ReadyIndices[typeIndex].Add(clipName);
				}

				// Death
				var unitBuildingName = assetType.Speed != 0 ? "unit" : "building";
				clipName = unitBuildingName + "-death";
				if (AudioManager.ClipData.ContainsKey(clipName))
					DeathIndices[typeIndex].Add(clipName);
				else
				{
					clipIndex = 1;
					clipName = unitBuildingName + "-death" + clipIndex;
					while (AudioManager.ClipData.ContainsKey(clipName))
					{
						DeathIndices[typeIndex].Add(clipName);
						clipName = unitBuildingName + "-death" + ++clipIndex;
					}

					if (DeathIndices[typeIndex].Count == 0 && DeathIndices[AssetType.None].Count != 0)
						DeathIndices[typeIndex] = DeathIndices[AssetType.None];
				}

				// Help
				clipName = unitBuildingName + "-help";
				if (AudioManager.ClipData.ContainsKey(clipName))
					AttackedIndices[typeIndex].Add(clipName);
				else
				{
					clipIndex = 1;
					clipName = unitBuildingName + "-help" + clipIndex;
					while (AudioManager.ClipData.ContainsKey(clipName))
					{
						AttackedIndices[typeIndex].Add(clipName);
						clipName = unitBuildingName + "-help" + ++clipIndex;
					}

					if (AttackedIndices[typeIndex].Count == 0 && AttackedIndices[AssetType.None].Count != 0)
						AttackedIndices[typeIndex] = AttackedIndices[AssetType.None];
				}

				// Ranged Attack
				if (typeIndex == AssetType.Archer || typeIndex == AssetType.Ranger || typeIndex == AssetType.GuardTower
					|| typeIndex == AssetType.TownHall || typeIndex == AssetType.Keep || typeIndex == AssetType.Castle)
				{
					MissileFireIndices[typeIndex].Add("bowfire");
					MissileHitIndices[typeIndex].Add("bowhit");
				}
				else if (typeIndex == AssetType.CannonTower)
				{
					MissileFireIndices[typeIndex].Add("cannonfire");
					MissileHitIndices[typeIndex].Add("cannonhit");
				}

				// Harvest
				clipName = "harvest";
				if (AudioManager.ClipData.ContainsKey(clipName))
					AttackedIndices[typeIndex].Add(clipName);
				else
				{
					clipIndex = 1;
					clipName = "harvest" + clipIndex;
					while (AudioManager.ClipData.ContainsKey(clipName))
					{
						HarvestIndices[typeIndex].Add(clipName);
						clipName = "harvest" + ++clipIndex;
					}

					if (HarvestIndices[typeIndex].Count == 0 && HarvestIndices[AssetType.None].Count != 0)
						HarvestIndices[typeIndex] = HarvestIndices[AssetType.None];
				}

				// Quarry
				clipName = "quarry";
				if (AudioManager.ClipData.ContainsKey(clipName))
					AttackedIndices[typeIndex].Add(clipName);
				else
				{
					clipIndex = 1;
					clipName = "quarry" + clipIndex;
					while (AudioManager.ClipData.ContainsKey(clipName))
					{
						QuarryIndices[typeIndex].Add(clipName);
						clipName = "quarry" + ++clipIndex;
					}

					if (QuarryIndices[typeIndex].Count == 0 && QuarryIndices[AssetType.None].Count != 0)
						QuarryIndices[typeIndex] = QuarryIndices[AssetType.None];
				}

				// Melee Hit
				if (assetType.Range == 1)
				{
					clipName = "melee-hit";
					if (AudioManager.ClipData.ContainsKey(clipName))
						MeleeHitIndices[typeIndex].Add(clipName);
					else
					{
						clipIndex = 1;
						clipName = "melee-hit" + clipIndex;
						while (AudioManager.ClipData.ContainsKey(clipName))
						{
							MeleeHitIndices[typeIndex].Add(clipName);
							clipName = "melee-hit" + ++clipIndex;
						}

						if (MeleeHitIndices[typeIndex].Count == 0 && MeleeHitIndices[AssetType.None].Count != 0)
							MeleeHitIndices[typeIndex] = MeleeHitIndices[AssetType.None];
					}
				}
			}

			sw.Stop();
			Trace.TraceInformation($"{GetType().Name}: Finished setting up in {sw.Elapsed.TotalSeconds:0.000} seconds.");
		}

		public void RenderEvents(Rectangle area)
		{
			var mainRandomNumber = Random.Next();

			var selections = new List<int>(new int[(int)AssetType.Max]);
			var acknowledges = new List<int>(new int[(int)AssetType.Max]);

			var events = DelayedEvents.ToList();
			DelayedEvents.Clear();

			events.AddRange(Player.GameEvents);
			foreach (var @event in events)
			{
				if (@event.Type == EventType.ButtonTick)
				{
					AudioManager.PlayWave(TickClip, Volume);
				}
				else if (@event.Asset != null)
				{
					switch (@event.Type)
					{
						case EventType.Selection:
							if (@event.Asset.Data.Color == PlayerColor.None || @event.Asset.Data.Color == Player.Color)
							{
								if (@event.Asset.GetAction() == AssetAction.Construct)
								{
									if (ConstructIndices.Count != 0)
									{
										var randomClip = mainRandomNumber % ConstructIndices.Count;
										AudioManager.PlayWave(ConstructIndices[randomClip], Volume, CalculateBias(area, @event.Asset.Position));
									}
								}
								else if (SelectionIndices[@event.Asset.Data.Type].Count != 0)
								{
									if (selections[(int)@event.Asset.Data.Type] < 1)
									{
										var randomClip = mainRandomNumber % SelectionIndices[@event.Asset.Data.Type].Count;
										if (DelayedSelectionIndices[(int)@event.Asset.Data.Type] < 0)
											DelayedSelectionIndices[(int)@event.Asset.Data.Type] = randomClip;
										else
											randomClip = DelayedSelectionIndices[(int)@event.Asset.Data.Type];

										AudioManager.PlayWave(SelectionIndices[@event.Asset.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
										selections[(int)@event.Asset.Data.Type]++;
									}
									else if ((mainRandomNumber & 0x3) == 0)
										DelayedEvents.Add(@event);
								}
							}
							break;

						case EventType.Acknowledge:
							if (@event.Asset.Data.Color == PlayerColor.None || @event.Asset.Data.Color == Player.Color)
							{
								if (AcknowledgeIndices[@event.Asset.Data.Type].Count != 0)
								{
									if (acknowledges[(int)@event.Asset.Data.Type] < 1)
									{
										var randomClip = mainRandomNumber % AcknowledgeIndices[@event.Asset.Data.Type].Count;
										if (DelayedAcknowledgementIndices[(int)@event.Asset.Data.Type] < 0)
											DelayedAcknowledgementIndices[(int)@event.Asset.Data.Type] = randomClip;
										else
											randomClip = DelayedAcknowledgementIndices[(int)@event.Asset.Data.Type];

										AudioManager.PlayWave(AcknowledgeIndices[@event.Asset.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
										acknowledges[(int)@event.Asset.Data.Type]++;
									}
									else if ((mainRandomNumber & 0x3) == 0)
										DelayedEvents.Add(@event);
								}
							}
							break;

						case EventType.WorkComplete:
							if (Player.Color == @event.Asset.Data.Color)
							{
								if (WorkCompleteIndices[@event.Asset.Data.Type].Count != 0)
								{
									var randomClip = mainRandomNumber % WorkCompleteIndices[@event.Asset.Data.Type].Count;
									AudioManager.PlayWave(WorkCompleteIndices[@event.Asset.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
								}
							}
							break;

						case EventType.Ready:
							if (@event.Asset.Data.Color == PlayerColor.None || @event.Asset.Data.Color == Player.Color)
							{
								if (ReadyIndices[@event.Asset.Data.Type].Count != 0)
								{
									var randomClip = mainRandomNumber % ReadyIndices[@event.Asset.Data.Type].Count;
									AudioManager.PlayWave(ReadyIndices[@event.Asset.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
								}
							}
							break;

						case EventType.Death:
							if (area.Contains(@event.Asset.Position.ToPoint()))
							{
								if (DeathIndices[@event.Asset.Data.Type].Count != 0)
								{
									var randomClip = mainRandomNumber % DeathIndices[@event.Asset.Data.Type].Count;
									AudioManager.PlayWave(DeathIndices[@event.Asset.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
								}
							}
							break;

						case EventType.Attacked:
							if (!area.Contains(@event.Asset.Position.ToPoint()))
							{
								if (AttackedIndices[@event.Asset.Data.Type].Count != 0)
								{
									var randomClip = mainRandomNumber % AttackedIndices[@event.Asset.Data.Type].Count;
									AudioManager.PlayWave(AttackedIndices[@event.Asset.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
								}
							}
							break;

						case EventType.MissileFire:
							if (area.Contains(@event.Asset.Position.ToPoint()))
							{
								if (MissileFireIndices[@event.Asset.Data.Type].Count != 0)
								{
									var randomClip = mainRandomNumber % MissileFireIndices[@event.Asset.Data.Type].Count;
									AudioManager.PlayWave(MissileFireIndices[@event.Asset.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
								}
							}
							break;

						case EventType.MissileHit:
							if (area.Contains(@event.Asset.Position.ToPoint()))
							{
								var creationCommand = @event.Asset.NextCommand(); // To find out what time of missile
								if (creationCommand.Action == AssetAction.Construct && creationCommand.Target != null)
								{
									if (MissileHitIndices[creationCommand.Target.Data.Type].Count != 0)
									{
										var randomClip = mainRandomNumber % MissileHitIndices[creationCommand.Target.Data.Type].Count;
										AudioManager.PlayWave(MissileHitIndices[creationCommand.Target.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
									}
								}
							}
							break;

						case EventType.Harvest:
							if (area.Contains(@event.Asset.Position.ToPoint()) && @event.Asset.Data.AttackSteps - 1 == @event.Asset.Step % @event.Asset.Data.AttackSteps)
							{
								if (HarvestIndices[@event.Asset.Data.Type].Count != 0)
								{
									var randomClip = mainRandomNumber % HarvestIndices[@event.Asset.Data.Type].Count;
									AudioManager.PlayWave(HarvestIndices[@event.Asset.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
								}
							}
							break;

						case EventType.Quarry:
							if (area.Contains(@event.Asset.Position.ToPoint()) && @event.Asset.Data.AttackSteps - 1 == @event.Asset.Step % @event.Asset.Data.AttackSteps)
							{
								if (QuarryIndices[@event.Asset.Data.Type].Count != 0)
								{
									var randomClip = mainRandomNumber % QuarryIndices[@event.Asset.Data.Type].Count;
									AudioManager.PlayWave(QuarryIndices[@event.Asset.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
								}
							}
							break;

						case EventType.MeleeHit:
							if (area.Contains(@event.Asset.Position.ToPoint()))
							{
								if (MeleeHitIndices[@event.Asset.Data.Type].Count != 0)
								{
									var randomClip = mainRandomNumber % MeleeHitIndices[@event.Asset.Data.Type].Count;
									AudioManager.PlayWave(MeleeHitIndices[@event.Asset.Data.Type][randomClip], Volume, CalculateBias(area, @event.Asset.Position));
								}
							}
							break;

						case EventType.PlaceAction:
							AudioManager.PlayWave(PlaceClip, Volume, CalculateBias(area, @event.Asset.Position));
							break;
					}
				}
			}

			for (var i = 0; i < (int)AssetType.Max; i++)
			{
				if (selections[i] == 0)
					DelayedSelectionIndices[i] = -1;
				if (acknowledges[i] == 0)
					DelayedAcknowledgementIndices[i] = -1;
			}
		}

		/// <summary>
		/// Calculates and returns the ratio of the horizontal distance
		/// <paramref name="pos"/> is from the center of <paramref name="area"/>.
		/// </summary>
		/// <remarks>
		/// -1.0f = Left
		/// 0.0f = Center
		/// 1.0f = Right
		/// </remarks>
		private static float CalculateBias(Rectangle area, Position pos)
		{
			var leftX = area.X;
			var rightX = area.X + area.Width - 1;
			var centerX = (leftX + rightX) / 2;

			if (pos.X <= leftX) return -1.0f;
			if (pos.X >= rightX) return 1.0f;
			if (leftX == centerX) return 0.0f;
			// Linux: This line might be wrong in the Linux version.
			if (pos.X < centerX) return -(1.0f - (float)(pos.X - leftX) / (centerX - leftX));
			return (float)(pos.X - centerX) / (rightX - centerX);
		}
	}
}