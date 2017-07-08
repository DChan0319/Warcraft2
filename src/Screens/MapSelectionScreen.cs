using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Warcraft.Screens.Components;
using Warcraft.Screens.Manager;
using Microsoft.Xna.Framework.Graphics;
using Warcraft.App;
using Warcraft.Assets;
using Warcraft.Renderers;
using Warcraft.Screens.Base;
using Warcraft.Util;

namespace Warcraft.Screens
{
	public class MapSelectionScreen : ButtonScreen
	{
		private string Title;

		private Tileset listViewIconTileset;
		private ListViewRenderer mapListViewRenderer;
		private Point mapListViewOffset;
		private List<string> mapNames;
		private int mapOffset;

		public MapSelectionScreen()
		{
			Title = "Select Map";

			// Get Map Names
			mapNames = DecoratedMap.AllMaps.Select(m => m.MapName).ToList();

			// Load List View Tileset
			listViewIconTileset = new Tileset();
			listViewIconTileset.Load(Path.Combine(Paths.Image, "ListViewIcons.dat"));
			mapListViewRenderer = new ListViewRenderer(listViewIconTileset, Data.Fonts[FontSize.Large]);

			// Reset Player Colors
			Data.ResetPlayerColors();

			// Initialized Selected Map
			Data.SelectedMapIndex = 0;
			Data.SelectedMap = DecoratedMap.GetMap(Data.SelectedMapIndex);

			Data.MapRenderer = new MapRenderer(Path.Combine(Paths.Image, "MapRendering.dat"), Data.TerrainTileset, Data.SelectedMap);
			Data.AssetRenderer = new AssetRenderer(Data.AssetRecolorMap, Data.AssetTilesets, Data.MarkerTileset, Data.FireTilesets, Data.BuildingDeathTileset, Data.ArrowTileset, null, Data.SelectedMap);
			Data.MiniMapRenderer = new MiniMapRenderer(Data.MapRenderer, Data.AssetRenderer, null);

			var selectButton = new Button("Select");
			selectButton.OnClick += SelectButton_OnClick;
			Buttons.Add(selectButton);

			var cancelButton = new Button("Cancel");
			cancelButton.OnClick += CancelButton_OnClick;
			Buttons.Add(cancelButton);
		}

		private void SelectButton_OnClick()
		{
			for (var index = 1; index < (int)PlayerColor.Max; index++)
			{
				if (index == 1)
					Data.LoadingPlayerTypes[index] = PlayerType.Human;
				else if (index <= Data.SelectedMap.PlayerCount)
				{
					// Todo: Multiplayer
					Data.LoadingPlayerTypes[index] = PlayerType.AiEasy;
				}
			}

			Data.PlayerColor = PlayerColor.Blue;

			ScreenManager.AddScreen(new PlayerAiColorSelectScreen());
		}

		private void CancelButton_OnClick() { IsExiting = true; }

		public override void HandleInput(InputState input)
		{
			base.HandleInput(input);

			if (input.LeftClick && !input.LeftDown)
			{
				var pos = input.CurrentMouseState.Position - mapListViewOffset;
				var itemSelected = mapListViewRenderer.ItemAt(pos.X, pos.Y);

				if (itemSelected == (int)ListViewObject.UpArrow)
				{
					if (mapOffset != 0) mapOffset--;
				}
				else if (itemSelected == (int)ListViewObject.DownArrow)
				{
					mapOffset++;
				}
				else if (itemSelected != (int)ListViewObject.None)
				{
					Data.SelectedMapIndex = itemSelected;
					Data.SelectedMap = DecoratedMap.GetMap(Data.SelectedMapIndex);

					Data.MapRenderer.SetMap(Data.SelectedMap);
					Data.AssetRenderer.SetMap(Data.SelectedMap);
				}
			}
		}

		public override void BeginDraw(GameTime gameTime)
		{
			// Draw the map listview
			ScreenManager.Graphics.SetRenderTarget(Data.ListViewRenderTarget);
			ScreenManager.Graphics.Clear(Color.Transparent);
			mapListViewRenderer.Draw(Data.SelectedMapIndex, mapOffset, mapNames);

			// Draw the mini map
			ScreenManager.Graphics.SetRenderTarget(Data.MiniMapRenderTarget);
			Data.MiniMapRenderer.DrawMiniMap(null);
		}

		public override void Draw(GameTime gameTime)
		{
			int titleHeight, bufferWidth, bufferHeight;
			DrawMenuTitle(Title, out titleHeight, out bufferWidth, out bufferHeight);

			int listViewWidth = 0, listViewHeight = 0;
			if (Data.ListViewRenderTarget != null)
			{
				listViewWidth = Data.ListViewRenderTarget.Width;
				listViewHeight = Data.ListViewRenderTarget.Height;
			}

			if (listViewWidth != bufferWidth - Data.ViewportOffset.X - Data.BorderWidth - Data.InnerBevel.Width * 2
				|| listViewHeight != bufferHeight - titleHeight - Data.InnerBevel.Width - Data.BorderWidth
				|| Data.ListViewRenderTarget == null)
			{
				listViewWidth = bufferWidth - Data.ViewportOffset.X - Data.BorderWidth - Data.InnerBevel.Width * 2;
				listViewHeight = bufferHeight - titleHeight - Data.InnerBevel.Width - Data.BorderWidth;
				Data.ListViewRenderTarget = new RenderTarget2D(ScreenManager.GraphicsDevice, listViewWidth, listViewHeight);
			}

			mapListViewOffset.X = Data.BorderWidth;
			mapListViewOffset.Y = titleHeight + Data.InnerBevel.Width;

			ScreenManager.SpriteBatch.Draw(Data.ListViewRenderTarget, new Rectangle(mapListViewOffset, Data.ListViewRenderTarget.Bounds.Size), Color.White);
			Data.InnerBevel.DrawBevel(new Rectangle(mapListViewOffset, Data.ListViewRenderTarget.Bounds.Size));

			// Draw Mini Map
			var miniMapLeft = mapListViewOffset.X + listViewWidth + Data.InnerBevel.Width * 4;
			ScreenManager.SpriteBatch.Draw(Data.MiniMapRenderTarget, new Vector2(miniMapLeft, mapListViewOffset.Y));
			Data.InnerBevel.DrawBevel(new Rectangle(miniMapLeft, mapListViewOffset.Y, Data.MiniMapRenderTarget.Width, Data.MiniMapRenderTarget.Height));

			var textTop = mapListViewOffset.Y + Data.MiniMapRenderTarget.Height + Data.InnerBevel.Width * 2;
			var miniMapCenter = miniMapLeft + Data.MiniMapRenderTarget.Width / 2;
			var textColor = Data.Fonts[FontSize.Large].RecolorMap.FindColor("white");
			var shadowColor = Data.Fonts[FontSize.Large].RecolorMap.FindColor("black");

			// Draw Player Count Text
			var playerCountText = Data.SelectedMap.PlayerCount + " Players";
			var playerCountTextDimensions = Data.Fonts[FontSize.Large].MeasureText(playerCountText);
			Data.Fonts[FontSize.Large].DrawTextWithShadow(miniMapCenter - playerCountTextDimensions.Width / 2, textTop, textColor, shadowColor, 1, playerCountText);
			textTop += playerCountTextDimensions.Height;

			// Draw Map Dimensions Text
			var mapDimensionsText = $"{Data.SelectedMap.MapWidth} x {Data.SelectedMap.MapHeight}";
			var mapDimensionsTextDimensions = Data.Fonts[FontSize.Large].MeasureText(mapDimensionsText);
			Data.Fonts[FontSize.Large].DrawTextWithShadow(miniMapCenter - mapDimensionsTextDimensions.Width / 2, textTop, textColor, shadowColor, 1, mapDimensionsText);

			Data.ButtonRenderer.SetText(Buttons[0].Text, true);
			Data.ButtonRenderer.SetHeight(Data.ButtonRenderer.TextArea.Height * 3 / 2);
			Data.ButtonRenderer.SetWidth(Data.MiniMapRenderTarget.Width + Data.InnerBevel.Width * 2);
			miniMapLeft -= Data.InnerBevel.Width;

			// Draw Select Button
			textTop = bufferHeight - Data.BorderWidth - Data.ButtonRenderer.TextArea.Height * 9 / 4 + Data.InnerBevel.Width;
			Buttons[0].Rectangle = new Rectangle(miniMapLeft, textTop, Data.ButtonRenderer.TextArea.Width, Data.ButtonRenderer.TextArea.Height);
			Data.ButtonRenderer.DrawButton(Buttons[0]);

			// Draw Cancel Button
			textTop = bufferHeight - Data.BorderWidth - Data.ButtonRenderer.TextArea.Height + Data.InnerBevel.Width;
			Data.ButtonRenderer.SetText(Buttons[1].Text);
			Buttons[1].Rectangle = new Rectangle(miniMapLeft, textTop, Data.ButtonRenderer.TextArea.Width, Data.ButtonRenderer.TextArea.Height);
			Data.ButtonRenderer.DrawButton(Buttons[1]);
		}
	}
}