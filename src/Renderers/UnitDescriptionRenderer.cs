using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Extensions;
using Warcraft.Player;
using Warcraft.Screens.Components;
using Warcraft.Screens.Manager;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Warcraft.Renderers
{
	/// <summary>
	/// Renders the unit description box
	/// </summary>
	public class UnitDescriptionRenderer
	{
		private MulticolorTileset Icons { get; }
		private Bevel Bevel { get; }
		private List<int> AssetIndices { get; }
		private List<int> ResearchIndices { get; }
		private List<Color> HealthColors { get; }
		private PlayerColor PlayerColor { get; }
		private Color HealthForegroundColor { get; }
		private Color HealthBackgroundColor { get; }
		private Color ConstructionForegroundColor { get; }
		private Color ConstructionBackgroundColor { get; }
		private Color ConstructionCompletionColor { get; }
		private Color ConstructionShadowColor { get; }
		private Size fullIconSize;
		private Size displayedSize;
		private int DisplayedIcons { get; set; }

		private const int ForegroundColorIndex = 1;
		private const int BackgroundColorIndex = 0;
		private const int MaxHealthColor = 3;
		private const int HealthHeight = 5;

		/// <summary>
		/// Creates a new <see cref="UnitDescriptionRenderer"/>.
		/// </summary>
		/// <param name="icons">Icon tileset to be used</param>
		/// <param name="bevel">Bevel to be used</param>
		/// <param name="playerColor">PlayerColor that this belongs to</param>
		public UnitDescriptionRenderer(MulticolorTileset icons, Bevel bevel, PlayerColor playerColor)
		{
			Icons = icons;
			Bevel = bevel;
			PlayerColor = playerColor;

			var textDimensions = Data.Fonts[FontSize.Small].MeasureText("0123456789");
			fullIconSize.Width = Icons.TileWidth + Bevel.Width * 2;
			fullIconSize.Height = Icons.TileHeight + Bevel.Width * 3 + HealthHeight + 2 + textDimensions.Height;

			HealthColors = new List<Color>(new Color[MaxHealthColor])
			{
				[0] = 0xFF0000FCu.ToColor(),
				[1] = 0xFF00FCFCu.ToColor(),
				[2] = 0xFF007030u.ToColor()
			};

			HealthForegroundColor = 0xFF000000u.ToColor();
			HealthBackgroundColor = 0xFF303030u.ToColor();

			ConstructionForegroundColor = 0xFF60A0A0u.ToColor();
			ConstructionBackgroundColor = 0xFF505050u.ToColor();
			ConstructionCompletionColor = 0xFF007030u.ToColor();
			ConstructionShadowColor = 0xFF000000u.ToColor();

			AssetIndices = new List<int>(new int[(int)AssetType.Max])
			{
				[(int)AssetType.Wall] = Icons.GetIndex("human-wall"),
				[(int)AssetType.Peasant] = Icons.GetIndex("peasant"),
				[(int)AssetType.Footman] = Icons.GetIndex("footman"),
				[(int)AssetType.Knight] = Icons.GetIndex("knight"),
				[(int)AssetType.Archer] = Icons.GetIndex("archer"),
				[(int)AssetType.Ranger] = Icons.GetIndex("ranger"),
				[(int)AssetType.GoldMine] = Icons.GetIndex("gold-mine"),
				[(int)AssetType.TownHall] = Icons.GetIndex("town-hall"),
				[(int)AssetType.Keep] = Icons.GetIndex("keep"),
				[(int)AssetType.Castle] = Icons.GetIndex("castle"),
				[(int)AssetType.Farm] = Icons.GetIndex("chicken-farm"),
				[(int)AssetType.Barracks] = Icons.GetIndex("human-barracks"),
				[(int)AssetType.LumberMill] = Icons.GetIndex("human-lumber-mill"),
				[(int)AssetType.Blacksmith] = Icons.GetIndex("human-blacksmith"),
				[(int)AssetType.ScoutTower] = Icons.GetIndex("scout-tower"),
				[(int)AssetType.GuardTower] = Icons.GetIndex("human-guard-tower"),
				[(int)AssetType.CannonTower] = Icons.GetIndex("human-cannon-tower")
			};

			ResearchIndices = new List<int>(new int[(int)AssetCapabilityType.Max])
			{
				[(int)AssetCapabilityType.WeaponUpgrade1] = icons.GetIndex("human-weapon-1"),
				[(int)AssetCapabilityType.WeaponUpgrade2] = icons.GetIndex("human-weapon-2"),
				[(int)AssetCapabilityType.WeaponUpgrade3] = icons.GetIndex("human-weapon-3"),
				[(int)AssetCapabilityType.ArrowUpgrade1] = icons.GetIndex("human-arrow-1"),
				[(int)AssetCapabilityType.ArrowUpgrade2] = icons.GetIndex("human-arrow-2"),
				[(int)AssetCapabilityType.ArrowUpgrade3] = icons.GetIndex("human-arrow-3"),
				[(int)AssetCapabilityType.ArmorUpgrade1] = icons.GetIndex("human-armor-1"),
				[(int)AssetCapabilityType.ArmorUpgrade2] = icons.GetIndex("human-armor-2"),
				[(int)AssetCapabilityType.ArmorUpgrade3] = icons.GetIndex("human-armor-3"),
				[(int)AssetCapabilityType.Longbow] = icons.GetIndex("longbow"),
				[(int)AssetCapabilityType.RangerScouting] = icons.GetIndex("ranger-scouting"),
				[(int)AssetCapabilityType.Marksmanship] = icons.GetIndex("marksmanship"),
				[(int)AssetCapabilityType.BuildRanger] = icons.GetIndex("ranger"),
				[(int)AssetCapabilityType.BuildKnight] = icons.GetIndex("knight")
			};
		}

		/// <summary>
		/// Renders the unit description box onto the current render target.
		/// </summary>
		/// <param name="selectionList">List of selected units to display</param>
		public void DrawUnitDescription(List<PlayerAsset> selectionList)
		{
			if (selectionList.Count == 0) return;

			DisplayedIcons = 0;
			displayedSize.Width = Data.UnitDescriptionRenderTarget.Width;
			displayedSize.Height = Data.UnitDescriptionRenderTarget.Height;

			var horizontalIcons = displayedSize.Width / fullIconSize.Width;
			var verticalIcons = displayedSize.Height / fullIconSize.Height;
			var horizontalGap = (displayedSize.Width - horizontalIcons * fullIconSize.Width) / (horizontalIcons - 1);
			var verticalGap = (displayedSize.Height - verticalIcons * fullIconSize.Height) / (verticalIcons - 1);

			ScreenManager.SpriteBatch.Begin();

			// Render expanded unit information
			if (selectionList.Count == 1)
			{
				var textTop = 0;
				DisplayedIcons = 1;

				var asset = selectionList.First();

				var healthColor = (asset.Health - 1) * MaxHealthColor / asset.Data.Health;
				var assetName = PlayerAssetData.TypeToName(asset.Data.Type).AddSpaces();

				Bevel.DrawBevel(new Rectangle(Bevel.Width + 1, Bevel.Width + 1, Icons.TileWidth, Icons.TileHeight));
				var colorIndex = (int)(asset.Data.Color != 0 ? asset.Data.Color - 1 : 0);
				ScreenManager.SpriteBatch.Draw(Icons.GetTile(AssetIndices[(int)asset.Data.Type], colorIndex), new Vector2(Bevel.Width + 1, Bevel.Width + 1));

				var nameTextDimensions = Data.Fonts[FontSize.Medium].MeasureText(assetName);

				var textCenter = (displayedSize.Width + Icons.TileWidth + Bevel.Width * 2) / 2;
				var assetNameX = textCenter - nameTextDimensions.Width / 2;
				var assetNameY = Icons.TileHeight / 2 + Bevel.Width - nameTextDimensions.Height / 2;
				Data.Fonts[FontSize.Medium].DrawTextWithShadow(assetNameX, assetNameY, ForegroundColorIndex, BackgroundColorIndex, 1, assetName);

				// Render unit/wall health
				if (asset.Data.Color != PlayerColor.None || asset.Data.Type == AssetType.Wall)
				{
					var healthBarRectangle = new Rectangle(1, Icons.TileHeight + Bevel.Width * 4 + 1, Icons.TileWidth + Bevel.Width * 2, HealthHeight + 2);
					ScreenManager.SpriteBatch.DrawFilledRectangle(healthBarRectangle, HealthForegroundColor);

					var healthRemainingRectangle = new Rectangle(2, Icons.TileHeight + Bevel.Width * 4 + 2, (Icons.TileWidth + Bevel.Width * 2 - 2) * asset.Health / asset.Data.Health, HealthHeight);
					ScreenManager.SpriteBatch.DrawFilledRectangle(healthRemainingRectangle, HealthColors[healthColor]);

					var healthText = asset.Health + " / " + asset.Data.Health;
					var healthTextDimensions = Data.Fonts[FontSize.Small].MeasureText(healthText);
					textTop = Icons.TileHeight + Bevel.Width * 4 + HealthHeight + 2;
					var healthTextPosition = new Point(Icons.TileWidth / 2 + Bevel.Width * 2 - healthTextDimensions.Width / 2 + 1, textTop);
					Data.Fonts[FontSize.Small].DrawTextWithShadow(healthTextPosition.X, healthTextPosition.Y, ForegroundColorIndex, BackgroundColorIndex, 1, healthText);

					textTop += healthTextDimensions.Height;
				}

				// Render unit stats
				if (asset.Data.Color == PlayerColor)
				{
					// Movable Units
					if (asset.Speed != 0)
					{
						// Armor
						var armorText = "Armor: ";
						var armorTextDimensions = Data.Fonts[FontSize.Medium].MeasureText(armorText);
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter - armorTextDimensions.Width, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, armorText);

						// Todo: ArmorUpgrade
						var armorValueText = asset.Data.Armor.ToString();
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, armorValueText);

						var textHeight = armorTextDimensions.Height;
						textTop += textHeight;

						// Damage
						var damageText = "Damage: ";
						var damageTextDimensions = Data.Fonts[FontSize.Medium].MeasureText(damageText);
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter - damageTextDimensions.Width, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, damageText);

						// Todo: DamageUpgrade
						var damageValueText = (asset.Data.PiercingDamage / 2) + "-" + (asset.Data.BasicDamage + asset.Data.PiercingDamage);
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, damageValueText);

						textTop += textHeight;

						// Range
						var rangeText = "Range: ";
						var rangeTextDimensions = Data.Fonts[FontSize.Medium].MeasureText(rangeText);
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter - rangeTextDimensions.Width, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, rangeText);

						// Todo: RangeUpgrade
						var rangeValueText = asset.Data.Range.ToString();
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, rangeValueText);

						textTop += textHeight;

						// Sight
						var sightText = "Sight: ";
						var sightTextDimensions = Data.Fonts[FontSize.Medium].MeasureText(sightText);
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter - sightTextDimensions.Width, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, sightText);

						// Todo: SighUpgrade
						var sightValueText = asset.Data.Sight.ToString();
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, sightValueText);

						textTop += textHeight;

						// Speed
						var speedText = "Speed: ";
						var speedTextDimensions = Data.Fonts[FontSize.Medium].MeasureText(speedText);
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter - speedTextDimensions.Width, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, speedText);

						// Todo: SpeedUpgrade
						var speedValueText = asset.Speed.ToString();
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, speedValueText);
					}
					// Buildings
					else
					{
						// Constructing
						if (asset.GetAction() == AssetAction.Construct)
						{
							var command = asset.CurrentCommand().Target?.CurrentCommand() ?? asset.CurrentCommand();
							var percentComplete = command.ActivatedCapability?.PercentComplete(100) ?? 0;
							DrawCompletionBar(percentComplete);
						}
						// Upgrading capability
						else if (asset.GetAction() == AssetAction.Capability)
						{
							var command = asset.CurrentCommand();
							var percentComplete = command.ActivatedCapability?.PercentComplete(100) ?? 0;

							var horizontalOffset = Bevel.Width * (fullIconSize.Width + horizontalGap);
							var verticalOffset = Bevel.Width + fullIconSize.Height + verticalGap;

							Bevel.DrawBevel(new Rectangle(horizontalOffset, verticalOffset, Icons.TileWidth, Icons.TileHeight));
							if (command.Target != null)
							{
								var tileColorIndex = command.Target.Data.Color != PlayerColor.None ? (int)command.Target.Data.Color - 1 : 0;
								ScreenManager.SpriteBatch.Draw(Icons.GetTile(AssetIndices[(int)command.Target.Data.Type], tileColorIndex), new Vector2(horizontalOffset, verticalOffset));
							}
							else
							{
								var tileColorIndex = asset.Data.Color != PlayerColor.None ? (int)asset.Data.Color - 1 : 0;
								ScreenManager.SpriteBatch.Draw(Icons.GetTile(ResearchIndices[(int)command.Capability], tileColorIndex), new Vector2(horizontalOffset, verticalOffset));
							}

							var trainingResearchingText = command.Target != null ? "Training: " : "Researching: ";
							var trainingResearchingTextDimensions = Data.Fonts[FontSize.Medium].MeasureText(trainingResearchingText);
							var trainingResearchingTextX = horizontalOffset - trainingResearchingTextDimensions.Width - Bevel.Width;
							var trainingResearchingTextY = verticalOffset + Icons.TileHeight / 2 - trainingResearchingTextDimensions.Height / 2;
							Data.Fonts[FontSize.Medium].DrawTextWithShadow(trainingResearchingTextX, trainingResearchingTextY, ForegroundColorIndex, BackgroundColorIndex, 1, trainingResearchingText);

							DrawCompletionBar(percentComplete);
						}
						// Sheltered Peasants
						else if (asset.Data.Type == AssetType.TownHall || asset.Data.Type == AssetType.Keep || asset.Data.Type == AssetType.Castle)
						{
							textTop = Icons.TileHeight + Bevel.Width * 4 + HealthHeight + 2;

							var shelterText = "Shelter: ";
							var shelterTextDimensions = Data.Fonts[FontSize.Medium].MeasureText(shelterText);
							Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter - shelterTextDimensions.Width, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, shelterText);

							var shelteredCountText = asset.ShelteredPeasants.Count + " / " + asset.ShelterCapacity;
							Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, shelteredCountText);
						}
					}
				}
				else
				{
					// Gold Mine
					if (asset.Data.Type == AssetType.GoldMine)
					{
						textTop = Icons.TileHeight + Bevel.Width * 4 + HealthHeight + 2;

						var goldText = "Gold: ";
						var goldTextDimensions = Data.Fonts[FontSize.Medium].MeasureText(goldText);
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter - goldTextDimensions.Width, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, goldText);

						var goldValueText = asset.Gold.ToString("N0");
						Data.Fonts[FontSize.Medium].DrawTextWithShadow(textCenter, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, goldValueText);
					}
				}
			}
			// Render health information for all selected units
			else
			{
				DisplayedIcons = 0;
				var horizontalOffset = Bevel.Width + 1;
				var verticalOffset = Bevel.Width + 1;

				foreach (var asset in selectionList)
				{
					var healthColor = (asset.Health - 1) * MaxHealthColor / asset.Data.Health;

					Bevel.DrawBevel(new Rectangle(horizontalOffset, verticalOffset, Icons.TileWidth, Icons.TileHeight));
					var iconColorIndex = asset.Data.Color != PlayerColor.None ? (int)asset.Data.Color - 1 : 0;
					ScreenManager.SpriteBatch.Draw(Icons.GetTile(AssetIndices[(int)asset.Data.Type], iconColorIndex), new Vector2(horizontalOffset, verticalOffset));

					var healthBarRectangle = new Rectangle(horizontalOffset - Bevel.Width, verticalOffset + Icons.TileHeight + Bevel.Width * 3, Icons.TileWidth + Bevel.Width * 2, HealthHeight + 2);
					ScreenManager.SpriteBatch.DrawFilledRectangle(healthBarRectangle, HealthForegroundColor);

					var healthRemainingRectangle = new Rectangle(horizontalOffset - Bevel.Width + 1, verticalOffset + Icons.TileHeight + Bevel.Width * 3 + 1, (Icons.TileWidth + Bevel.Width * 2 - 2) * asset.Health / asset.Data.Health, HealthHeight);
					ScreenManager.SpriteBatch.DrawFilledRectangle(healthRemainingRectangle, HealthColors[healthColor]);

					var healthText = asset.Health + " / " + asset.Data.Health;
					var healthTextDimensions = Data.Fonts[FontSize.Small].MeasureText(healthText);

					var textTop = verticalOffset + Icons.TileHeight + Bevel.Width * 4 + HealthHeight - 1;
					Data.Fonts[FontSize.Small].DrawTextWithShadow(horizontalOffset + Icons.TileWidth / 2 + Bevel.Width - healthTextDimensions.Width / 2, textTop, ForegroundColorIndex, BackgroundColorIndex, 1, healthText);

					horizontalOffset += fullIconSize.Width + horizontalGap - 1;
					DisplayedIcons++;

					if (DisplayedIcons % horizontalIcons == 0)
					{
						horizontalOffset = Bevel.Width + 1;
						verticalOffset += fullIconSize.Height + verticalGap - 1;
					}
				}
			}

			ScreenManager.SpriteBatch.End();
		}

		/// <summary>
		/// Renders the completion bar for training/research/construction.
		/// </summary>
		/// <param name="percent">Percent of the progress bar to fill</param>
		private void DrawCompletionBar(int percent)
		{
			var textDimensions = Data.Fonts[FontSize.Large].MeasureText("% Complete");

			var textHeight = textDimensions.Y - textDimensions.X + 1;

			ScreenManager.SpriteBatch.DrawFilledRectangle(0, displayedSize.Height - (textHeight + 12), displayedSize.Width, textHeight + 12, Color.Black);
			ScreenManager.SpriteBatch.DrawHallowRectangle(1, displayedSize.Height - (textHeight + 11), displayedSize.Width - 2, textHeight + 10, ConstructionForegroundColor);
			ScreenManager.SpriteBatch.DrawFilledRectangle(3, displayedSize.Height - (textHeight + 9), displayedSize.Width - 6, textHeight + 6, Color.Black);
			ScreenManager.SpriteBatch.DrawFilledRectangle(4, displayedSize.Height - (textHeight + 8), displayedSize.Width - 8, textHeight + 4, ConstructionBackgroundColor);
			ScreenManager.SpriteBatch.DrawFilledRectangle(4, displayedSize.Height - (textHeight + 8), displayedSize.Width - 8, textHeight / 2 + 2, ConstructionShadowColor);

			var progressWidth = percent * (displayedSize.Width - 8) / 100;
			ScreenManager.SpriteBatch.DrawFilledRectangle(4, displayedSize.Height - (textHeight + 8), progressWidth, textHeight + 4, ConstructionCompletionColor);

			Data.Fonts[FontSize.Large].DrawTextWithShadow(displayedSize.Width / 2 - textDimensions.Width / 2, displayedSize.Height - (textHeight + textDimensions.X + 6), ForegroundColorIndex, BackgroundColorIndex, 1, "% Complete");
		}

		/// <summary>
		/// Returns the minimum width of the unit description render target.
		/// </summary>
		public int MinimumWidth()
		{
			return fullIconSize.Width * 3 + Bevel.Width * 2;
		}

		/// <summary>
		/// Returns the minimum height of the unit description render target.
		/// </summary>
		public int MinimumHeight(int width, int count)
		{
			var columns = width / fullIconSize.Width;
			var rows = (count + columns - 1) / columns;
			return rows * fullIconSize.Height + (rows - 1) * Bevel.Width;
		}

		/// <summary>
		/// Gets the index of the unit icon at <paramref name="pos"/>.
		/// </summary>
		public int GetSelection(Point pos)
		{
			var horizontalIcons = displayedSize.Width / fullIconSize.Width;
			var verticalIcons = displayedSize.Height / fullIconSize.Height;
			var horizontalGap = (displayedSize.Width - horizontalIcons * fullIconSize.Width) / (horizontalIcons - 1);
			var verticalGap = (displayedSize.Height - verticalIcons * fullIconSize.Height) / (verticalIcons - 1);

			var selectedIcon = -1;
			if (pos.X % (fullIconSize.Width + horizontalGap) < fullIconSize.Width && pos.Y % (fullIconSize.Height + verticalGap) < fullIconSize.Height)
			{
				selectedIcon = pos.X / (fullIconSize.Width + horizontalGap) + horizontalIcons * (pos.Y / (fullIconSize.Height + verticalGap));
				if (selectedIcon >= DisplayedIcons)
					selectedIcon = -1;
			}

			return selectedIcon;
		}
	}
}