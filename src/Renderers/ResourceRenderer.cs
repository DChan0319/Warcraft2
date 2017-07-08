using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.GameModel;
using Warcraft.Screens.Manager;

namespace Warcraft.Renderers
{
	/// <summary>
	/// Renders resource information in-game.
	/// </summary>
	public class ResourceRenderer
	{
		protected Tileset Icons { get; set; }
		protected FontTileset Font { get; set; }
		protected PlayerData Player { get; set; }
		protected List<int> IconIndices { get; set; }

		protected int TextHeight { get; set; }
		protected int ForegroundColorIndex { get; set; }
		protected int BackgroundColorIndex { get; set; }
		protected int InsufficientColorIndex { get; set; }
		protected int LastGoldDisplay { get; set; }
		protected int LastLumberDisplay { get; set; }
		protected int LastStoneDisplay { get; set; }

		/// <summary>
		/// Creates a new <see cref="ResourceRenderer"/>.
		/// </summary>
		/// <param name="icons">The resource icon tileset</param>
		/// <param name="font">The font used for text</param>
		/// <param name="player">The <see cref="PlayerData"/> to which this resource renderer belongs to</param>
		public ResourceRenderer(Tileset icons, FontTileset font, PlayerData player)
		{
			Icons = icons;
			Font = font;
			Player = player;
			ForegroundColorIndex = Font.RecolorMap.FindColor("white");
			BackgroundColorIndex = Font.RecolorMap.FindColor("black");
			InsufficientColorIndex = Font.RecolorMap.FindColor("red");
			LastGoldDisplay = 0;
			LastLumberDisplay = 0;
			LastStoneDisplay = 0;

			IconIndices = new List<int>(new int[(int)MiniIconTypes.Max])
			{
				[(int)MiniIconTypes.Gold] = Icons.GetIndex("gold"),
				[(int)MiniIconTypes.Lumber] = Icons.GetIndex("lumber"),
				[(int)MiniIconTypes.Food] = Icons.GetIndex("food"),
				[(int)MiniIconTypes.Stone] = Icons.GetIndex("stone")
			};

			var textDimensions = Font.MeasureText("0123456789");
			TextHeight = textDimensions.Height;
		}

		/// <summary>
		/// Renders the resource information onto the current render target.
		/// </summary>
		public void DrawResources()
		{
			var deltaGold = (Player.Gold - LastGoldDisplay) / 5;
			var deltaLumber = (Player.Lumber - LastLumberDisplay) / 5;
			var deltaStone = (Player.Stone - LastStoneDisplay) / 5;

			if (deltaGold > -3 && deltaGold < 3)
				LastGoldDisplay = Player.Gold;
			else
				LastGoldDisplay += deltaGold;

			if (deltaLumber > -3 && deltaLumber < 3)
				LastLumberDisplay = Player.Lumber;
			else
				LastLumberDisplay += deltaLumber;

			if (deltaStone > -3 && deltaStone < 3)
				LastStoneDisplay = Player.Stone;
			else
				LastStoneDisplay += deltaStone;

			var textYOffset = Data.ResourceRenderTarget.Height / 2 - TextHeight / 2;
			var iconYOffset = Data.ResourceRenderTarget.Height / 2 - Icons.TileHeight / 2;
			var widthSeparation = Data.ResourceRenderTarget.Width / 4;
			var xOffset = Data.ResourceRenderTarget.Width / 8;

			ScreenManager.SpriteBatch.Begin();

			// Gold
			ScreenManager.SpriteBatch.Draw(Icons.GetTile(IconIndices[(int)MiniIconTypes.Gold]), new Vector2(xOffset, iconYOffset));
			Font.DrawTextWithShadow(xOffset + Icons.TileWidth, textYOffset, ForegroundColorIndex, BackgroundColorIndex, 1, LastGoldDisplay.ToString("N0"));
			xOffset += widthSeparation;

			// Lumber
			ScreenManager.SpriteBatch.Draw(Icons.GetTile(IconIndices[(int)MiniIconTypes.Lumber]), new Vector2(xOffset, iconYOffset));
			Font.DrawTextWithShadow(xOffset + Icons.TileWidth, textYOffset, ForegroundColorIndex, BackgroundColorIndex, 1, LastLumberDisplay.ToString("N0"));
			xOffset += widthSeparation;

			// Stone
			ScreenManager.SpriteBatch.Draw(Icons.GetTile(IconIndices[(int)MiniIconTypes.Stone]), new Vector2(xOffset, iconYOffset));
			Font.DrawTextWithShadow(xOffset + Icons.TileWidth, textYOffset, ForegroundColorIndex, BackgroundColorIndex, 1, LastStoneDisplay.ToString("N0"));
			xOffset += widthSeparation;

			// Food
			ScreenManager.SpriteBatch.Draw(Icons.GetTile(IconIndices[(int)MiniIconTypes.Food]), new Vector2(xOffset, iconYOffset));
			if (Player.FoodConsumption > Player.FoodProduction)
			{
				var foodConsumptionText = $" {Player.FoodConsumption}";
				var foodProductionText = $" / {Player.FoodProduction}";
				var foodConsumptionTextDimensions = Font.MeasureText(foodConsumptionText);
				Font.DrawTextWithShadow(xOffset + Icons.TileWidth, textYOffset, InsufficientColorIndex, BackgroundColorIndex, 1, foodConsumptionText);
				Font.DrawTextWithShadow(xOffset + Icons.TileWidth + foodConsumptionTextDimensions.Width, textYOffset, ForegroundColorIndex, BackgroundColorIndex, 1, foodProductionText);
			}
			else
			{
				Font.DrawTextWithShadow(xOffset + Icons.TileWidth, textYOffset, ForegroundColorIndex, BackgroundColorIndex, 1, $" {Player.FoodConsumption} / {Player.FoodProduction}");
			}

			ScreenManager.SpriteBatch.End();
		}

		private enum MiniIconTypes
		{
			Gold,
			Lumber,
			Stone,
			Food,
			Max
		}
	}
}