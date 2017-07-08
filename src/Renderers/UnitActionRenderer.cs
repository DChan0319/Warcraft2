using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.GameModel;
using Warcraft.Player;
using Warcraft.Screens.Components;
using Warcraft.Screens.Manager;
using Warcraft.Player.Capabilities;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Warcraft.Renderers
{
	/// <summary>
	/// Renders the unit action box
	/// </summary>
	public class UnitActionRenderer
	{
		private Tileset Icons { get; }
		private Bevel Bevel { get; }
		private PlayerData PlayerData { get; }
		private List<int> CommandIndices { get; }
		private List<AssetCapabilityType> DisplayedCommands { get; }
		private PlayerColor PlayerColor { get; }
		private readonly Size fullIconSize;
		private int DisabledIndex { get; }

		private readonly AssetCapabilityType[] capabilityList = {
			AssetCapabilityType.BuildFarm,
			AssetCapabilityType.BuildTownHall,
			AssetCapabilityType.BuildBarracks,
			AssetCapabilityType.BuildLumberMill,
			AssetCapabilityType.BuildBlacksmith,
			AssetCapabilityType.BuildKeep,
			AssetCapabilityType.BuildCastle,
			AssetCapabilityType.BuildScoutTower,
			AssetCapabilityType.BuildGuardTower,
			AssetCapabilityType.BuildCannonTower,
			AssetCapabilityType.BuildWall
		};

		/// <summary>
		/// Creates a new <see cref="UnitActionRenderer"/>.
		/// </summary>
		public UnitActionRenderer(Tileset icons, Bevel bevel, PlayerData player)
		{
			Icons = icons;
			Bevel = bevel;
			PlayerData = player;
			PlayerColor = player.Color;
			fullIconSize.Width = Icons.TileWidth + Bevel.Width * 2;
			fullIconSize.Height = Icons.TileHeight + Bevel.Width * 2;

			DisplayedCommands = new List<AssetCapabilityType>(new AssetCapabilityType[9]);
			for (var i = 0; i < DisplayedCommands.Count; i++)
				DisplayedCommands[i] = AssetCapabilityType.None;

			CommandIndices = new List<int>(new int[(int)AssetCapabilityType.Max])
			{
				[(int)AssetCapabilityType.None] = -1,
				[(int)AssetCapabilityType.BuildPeasant] = Icons.GetIndex("peasant"),
				[(int)AssetCapabilityType.BuildFootman] = Icons.GetIndex("footman"),
				[(int)AssetCapabilityType.BuildKnight] = Icons.GetIndex("knight"),
				[(int)AssetCapabilityType.BuildArcher] = Icons.GetIndex("archer"),
				[(int)AssetCapabilityType.BuildRanger] = Icons.GetIndex("ranger"),
				[(int)AssetCapabilityType.BuildFarm] = Icons.GetIndex("chicken-farm"),
				[(int)AssetCapabilityType.BuildTownHall] = Icons.GetIndex("town-hall"),
				[(int)AssetCapabilityType.BuildBarracks] = Icons.GetIndex("human-barracks"),
				[(int)AssetCapabilityType.BuildLumberMill] = Icons.GetIndex("human-lumber-mill"),
				[(int)AssetCapabilityType.BuildBlacksmith] = Icons.GetIndex("human-blacksmith"),
				[(int)AssetCapabilityType.BuildKeep] = Icons.GetIndex("keep"),
				[(int)AssetCapabilityType.BuildCastle] = Icons.GetIndex("castle"),
				[(int)AssetCapabilityType.BuildScoutTower] = Icons.GetIndex("scout-tower"),
				[(int)AssetCapabilityType.BuildGuardTower] = Icons.GetIndex("human-guard-tower"),
				[(int)AssetCapabilityType.BuildCannonTower] = Icons.GetIndex("human-cannon-tower"),
				[(int)AssetCapabilityType.Move] = Icons.GetIndex("human-move"),
				[(int)AssetCapabilityType.Repair] = Icons.GetIndex("repair"),
				[(int)AssetCapabilityType.Mine] = Icons.GetIndex("mine"),
				[(int)AssetCapabilityType.BuildSimple] = Icons.GetIndex("build-simple"),
				[(int)AssetCapabilityType.BuildAdvanced] = Icons.GetIndex("build-advanced"),
				[(int)AssetCapabilityType.Convey] = Icons.GetIndex("human-convey"),
				[(int)AssetCapabilityType.Shelter] = Icons.GetIndex("shelter"),
				[(int)AssetCapabilityType.Unshelter] = Icons.GetIndex("shelter"),
				[(int)AssetCapabilityType.Cancel] = Icons.GetIndex("cancel"),
				[(int)AssetCapabilityType.BuildWall] = Icons.GetIndex("human-wall"),
				[(int)AssetCapabilityType.Attack] = Icons.GetIndex("human-weapon-1"),
				[(int)AssetCapabilityType.StandGround] = Icons.GetIndex("human-armor-1"),
				[(int)AssetCapabilityType.Patrol] = Icons.GetIndex("human-patrol"),
				[(int)AssetCapabilityType.WeaponUpgrade1] = Icons.GetIndex("human-weapon-1"),
				[(int)AssetCapabilityType.WeaponUpgrade2] = Icons.GetIndex("human-weapon-2"),
				[(int)AssetCapabilityType.WeaponUpgrade3] = Icons.GetIndex("human-weapon-3"),
				[(int)AssetCapabilityType.ArrowUpgrade1] = Icons.GetIndex("human-arrow-1"),
				[(int)AssetCapabilityType.ArrowUpgrade2] = Icons.GetIndex("human-arrow-2"),
				[(int)AssetCapabilityType.ArrowUpgrade3] = Icons.GetIndex("human-arrow-3"),
				[(int)AssetCapabilityType.ArmorUpgrade1] = Icons.GetIndex("human-armor-1"),
				[(int)AssetCapabilityType.ArmorUpgrade2] = Icons.GetIndex("human-armor-2"),
				[(int)AssetCapabilityType.ArmorUpgrade3] = Icons.GetIndex("human-armor-3"),
				[(int)AssetCapabilityType.Longbow] = Icons.GetIndex("longbow"),
				[(int)AssetCapabilityType.RangerScouting] = Icons.GetIndex("ranger-scouting"),
				[(int)AssetCapabilityType.Marksmanship] = Icons.GetIndex("marksmanship")
			};

			DisabledIndex = Icons.GetIndex("disabled");
		}

		/// <summary>
		/// Renders the unit action box onto the current render target.
		/// </summary>
		/// <param name="selectionList">List of selected units to display</param>
		/// <param name="currentAction">The current player capability</param>
		public void DrawUnitAction(List<PlayerAsset> selectionList, AssetCapabilityType currentAction)
		{
			var isFirst = true;
			var moveable = true;
			var hasCargo = false;

			for (var i = 0; i < DisplayedCommands.Count; i++)
				DisplayedCommands[i] = AssetCapabilityType.None;

			if (selectionList.Count == 0)
				return;

			foreach (var selection in selectionList)
			{
				if (selection.Data.Color != PlayerColor)
				{
					// Skip if the selection is not a wall being built by one of the player's units
					if (selection.Data.Type != AssetType.Wall || selection.GetAction() != AssetAction.Construct || selection.CurrentCommand().Target.Data.Color != PlayerColor)
						return;
				}

				if (isFirst)
				{
					isFirst = false;
					moveable = selection.Data.Speed > 0;
				}

				if (selection.Lumber > 0 || selection.Gold > 0)
					hasCargo = true;
			}

			var asset = selectionList.First();
			if (currentAction == AssetCapabilityType.None)
			{
				// Unit
				if (moveable)
				{
					DisplayedCommands[0] = hasCargo ? AssetCapabilityType.Convey : AssetCapabilityType.Move;
					DisplayedCommands[1] = AssetCapabilityType.StandGround;
					DisplayedCommands[2] = AssetCapabilityType.Attack;

					if (asset.HasCapability(AssetCapabilityType.Repair))
						DisplayedCommands[3] = AssetCapabilityType.Repair;

					if (asset.HasCapability(AssetCapabilityType.Patrol))
						DisplayedCommands[3] = AssetCapabilityType.Patrol;

					if (asset.HasCapability(AssetCapabilityType.Mine))
						DisplayedCommands[4] = AssetCapabilityType.Mine;

					if (asset.HasCapability(AssetCapabilityType.Shelter))
						DisplayedCommands[5] = AssetCapabilityType.Shelter;

					if (selectionList.Count == 1 && asset.HasCapability(AssetCapabilityType.BuildSimple))
						DisplayedCommands[6] = AssetCapabilityType.BuildSimple;
				}
				// Building
				else
				{
					if (asset.GetAction() == AssetAction.Construct || asset.GetAction() == AssetAction.Capability)
						DisplayedCommands[DisplayedCommands.Count - 1] = AssetCapabilityType.Cancel;
					else
					{
						var index = 0;
						foreach (var capability in asset.Data.GetCapabilities())
						{
							DisplayedCommands[index] = capability;
							index++;

							if (DisplayedCommands.Count <= index)
								break;
						}
					}
				}
			}
			// Constructing
			else if (currentAction == AssetCapabilityType.BuildSimple)
			{
				var index = 0;

				foreach (var capability in capabilityList)
				{
					if (!asset.HasCapability(capability)) continue;

					DisplayedCommands[index] = capability;
					index++;

					if (DisplayedCommands.Count <= index)
						break;
				}

				DisplayedCommands[DisplayedCommands.Count - 1] = AssetCapabilityType.Cancel;
			}
			else
			{
				DisplayedCommands[DisplayedCommands.Count - 1] = AssetCapabilityType.Cancel;
			}

			var xOffset = Bevel.Width + 1;
			var yOffset = Bevel.Width + 1;
			var iconIndex = 0;

			ScreenManager.SpriteBatch.Begin();

			// Draw icons
			foreach (var capability in DisplayedCommands)
			{
				if (capability != AssetCapabilityType.None)
				{
					Bevel.DrawBevel(new Rectangle(xOffset, yOffset, Icons.TileWidth, Icons.TileHeight));

					var position = new Vector2(xOffset, yOffset);
					ScreenManager.SpriteBatch.Draw(Icons.GetTile(CommandIndices[(int)capability]), position);

					var playerCapability = PlayerCapability.FindCapability(capability);
					if (playerCapability != null && !playerCapability.CanInitiate(selectionList.First(), PlayerData))
						ScreenManager.SpriteBatch.Draw(Icons.GetTile(DisabledIndex), position);
				}

				xOffset += fullIconSize.Width + Bevel.Width - 1;
				iconIndex++;

				if (iconIndex % 3 == 0)
				{
					xOffset = Bevel.Width + 1;
					yOffset += fullIconSize.Height + Bevel.Width - 1;
				}
			}

			ScreenManager.SpriteBatch.End();
		}

		/// <summary>
		/// Returns the minimum width of the unit action render target.
		/// </summary>
		public int MinimumWidth()
		{
			return fullIconSize.Width * 3 + Bevel.Width * 2;
		}

		/// <summary>
		/// Returns the minimum height of the unit action render target.
		/// </summary>
		public int MinimumHeight()
		{
			return fullIconSize.Height * 3 + Bevel.Width * 2;
		}

		/// <summary>
		/// Returns the <see cref="AssetCapabilityType"/> at <see cref="pos"/>.
		/// </summary>
		public AssetCapabilityType GetSelection(Point pos)
		{
			if (pos.X % (fullIconSize.Width + Bevel.Width) < fullIconSize.Width && pos.Y % (fullIconSize.Height + Bevel.Width) < fullIconSize.Height)
			{
				var index = pos.X / (fullIconSize.Width + Bevel.Width) + pos.Y / (fullIconSize.Height + Bevel.Width) * 3;
				return DisplayedCommands[index];
			}

			return AssetCapabilityType.None;
		}
	}
}