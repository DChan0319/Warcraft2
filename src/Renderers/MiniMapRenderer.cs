using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Warcraft.App;
using Warcraft.Extensions;
using Warcraft.Screens.Manager;

namespace Warcraft.Renderers
{
	public class MiniMapRenderer
	{
		public int VisibleWidth { get; set; }
		public int VisibleHeight { get; set; }
		public Vector2 Scale { get; set; }

		private MapRenderer MapRenderer { get; }
		private AssetRenderer AssetRenderer { get; }
		private FogRenderer FogRenderer { get; }

		public MiniMapRenderer(MapRenderer mapRenderer, AssetRenderer assetRenderer, FogRenderer fogRenderer)
		{
			MapRenderer = mapRenderer;
			AssetRenderer = assetRenderer;
			FogRenderer = fogRenderer;
		}

		/// <summary>
		/// Draws a the minimap of the <see cref="MapRenderer"/> and <see cref="AssetRenderer"/> on to the current render target.
		/// </summary>
		/// <remarks>
		/// Draw the minimap onto a <see cref="RenderTarget2D"/> so that it can be
		/// easily manipulated without changing other logic.
		/// </remarks>
		public void DrawMiniMap(Rectangle? viewport)
		{
			var miniMapWidth = Data.MiniMapRenderTarget.Width;
			var miniMapHeight = Data.MiniMapRenderTarget.Height;

			var sx = miniMapWidth / (float)MapRenderer.MapWidth;
			var sy = miniMapHeight / (float)MapRenderer.MapHeight;

			if (sx < sy) sy = sx;
			else if (sx > sy) sx = sy;

			Scale = new Vector2(sx, sy);

			ScreenManager.SpriteBatch.Begin(transformMatrix: Matrix.CreateScale(new Vector3(Scale.X, Scale.Y, 1)));

			MapRenderer.DrawMiniMap();
			AssetRenderer.DrawMiniAssets();
			FogRenderer?.DrawMiniMap();

			// Draw viewport outline
			if (viewport.HasValue)
			{
				var miniMapWidthMh = miniMapWidth * MapRenderer.MapHeight;
				var miniMapHeightMw = miniMapHeight * MapRenderer.MapWidth;
				if (miniMapHeightMw > miniMapWidthMh)
				{
					VisibleWidth = MapRenderer.MapWidth;
					VisibleHeight = MapRenderer.MapHeight * MapRenderer.MapWidth / MapRenderer.MapWidth;
				}
				else if (miniMapHeightMw < miniMapWidthMh)
				{
					VisibleWidth = MapRenderer.MapWidth * MapRenderer.MapHeight / MapRenderer.MapHeight;
					VisibleHeight = MapRenderer.MapHeight;
				}
				else
				{
					VisibleWidth = MapRenderer.MapWidth;
					VisibleHeight = MapRenderer.MapHeight;
				}

				var miniMapViewportArea = new Rectangle
				{
					X = (int)(viewport.Value.X * VisibleWidth / (float)MapRenderer.MapDetailedWidth + 0.5),
					Y = (int)(viewport.Value.Y * VisibleHeight / (float)MapRenderer.MapDetailedHeight + 0.5),
					Width = (int)(viewport.Value.Width * VisibleWidth / (float)MapRenderer.MapDetailedWidth + 0.5),
					Height = (int)(viewport.Value.Height * VisibleHeight / (float)MapRenderer.MapDetailedHeight + 0.5)
				};
				ScreenManager.SpriteBatch.DrawHallowRectangle(miniMapViewportArea, Color.White);
			}

			ScreenManager.SpriteBatch.End();
		}
	}
}