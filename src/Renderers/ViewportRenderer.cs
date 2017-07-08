using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Warcraft.App;
using Warcraft.Player;
using Warcraft.Screens.Manager;

namespace Warcraft.Renderers
{
	/// <summary>
	/// Renders the map, assets, fog, etc.
	/// </summary>
	public class ViewportRenderer
	{
		/// <summary>
		/// Map Renderer used for the viewport
		/// </summary>
		protected MapRenderer MapRenderer { get; }

		/// <summary>
		/// Asset Renderer used for the viewport
		/// </summary>
		protected AssetRenderer AssetRenderer { get; }

		/// <summary>
		/// Fog Renderer used for the viewport
		/// </summary>
		protected FogRenderer FogRenderer { get; }

		/// <summary>
		/// The bounds of the current viewport (position + size).
		/// </summary>
		public Rectangle Bounds;

		/// <summary>
		/// Creates a new <see cref="ViewportRenderer"/>.
		/// </summary>
		public ViewportRenderer(MapRenderer mapRenderer, AssetRenderer assetRenderer, FogRenderer fogRenderer)
		{
			MapRenderer = mapRenderer;
			AssetRenderer = assetRenderer;
			FogRenderer = fogRenderer;
			Bounds = Rectangle.Empty;
		}

		/// <summary>
		/// Sets the X position of the viewport,
		/// clamped between 0 and the map width.
		/// </summary>
		public void SetX(int x)
		{
			Bounds.X = MathHelper.Clamp(x, 0, MapRenderer.MapDetailedWidth - Bounds.Width);
		}

		/// <summary>
		/// Sets the Y position of the viewport,
		/// clamped between 0 and the map height.
		/// </summary>
		public void SetY(int y)
		{
			Bounds.Y = MathHelper.Clamp(y, 0, MapRenderer.MapDetailedHeight - Bounds.Height);
		}

		/// <summary>
		/// Sets the position of the viewport to
		/// be centered at <paramref name="pos"/>.
		/// </summary>
		public void CenterViewport(Position pos)
		{
			SetX(pos.X - Bounds.Width / 2);
			SetY(pos.Y - Bounds.Height / 2);
		}

		/// <summary>
		/// Pans the viewport by <paramref name="amount"/>
		/// units up.
		/// </summary>
		public void PanNorth(int amount)
		{
			SetY(Bounds.Y - amount);
		}

		/// <summary>
		/// Pans the viewport by <paramref name="amount"/>
		/// units right.
		/// </summary>
		public void PanEast(int amount)
		{
			SetX(Bounds.X + amount);
		}

		/// <summary>
		/// Pans the viewport by <paramref name="amount"/>
		/// units down.
		/// </summary>
		public void PanSouth(int amount)
		{
			SetY(Bounds.Y + amount);
		}

		/// <summary>
		/// Pans the viewport by <paramref name="amount"/>
		/// units left.
		/// </summary>
		public void PanWest(int amount)
		{
			SetX(Bounds.X - amount);
		}

		/// <summary>
		/// Draws the viewport onto the current render target.
		/// </summary>
		/// <param name="selectedAssetsAndMarkers">List of selected assets and map markers to render</param>
		/// <param name="selectionRectangle">Bounds of the selection rectangle to render</param>
		public void DrawViewport(List<PlayerAsset> selectedAssetsAndMarkers, Rectangle selectionRectangle)
		{
			Bounds.Width = Data.ViewportRenderTarget.Width;
			Bounds.Height = Data.ViewportRenderTarget.Height;

			var placeType = AssetType.None;
			switch (Data.CurrentAssetCapabilityType)
			{
				case AssetCapabilityType.BuildWall: placeType = AssetType.Wall; break;
				case AssetCapabilityType.BuildFarm: placeType = AssetType.Farm; break;
				case AssetCapabilityType.BuildTownHall: placeType = AssetType.TownHall; break;
				case AssetCapabilityType.BuildBarracks: placeType = AssetType.Barracks; break;
				case AssetCapabilityType.BuildLumberMill: placeType = AssetType.LumberMill; break;
				case AssetCapabilityType.BuildBlacksmith: placeType = AssetType.Blacksmith; break;
				case AssetCapabilityType.BuildScoutTower: placeType = AssetType.ScoutTower; break;
			}

			ScreenManager.Graphics.SetRenderTarget(Data.TypeRenderTarget);
			ScreenManager.Graphics.Clear(Color.Transparent);
			ScreenManager.SpriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend);

			MapRenderer.DrawMap(Bounds, 0, true);
			AssetRenderer.DrawAssets(Bounds, true);
			MapRenderer.DrawMap(Bounds, 1, true);

			ScreenManager.SpriteBatch.End();

			ScreenManager.Graphics.SetRenderTarget(Data.ViewportRenderTarget);
			ScreenManager.SpriteBatch.Begin();

			// Draw the map
			MapRenderer.DrawMap(Bounds, 0);
			AssetRenderer.DrawAssets(Bounds);
			MapRenderer.DrawMap(Bounds, 1);
			AssetRenderer.DrawSelections(Bounds, selectedAssetsAndMarkers, selectionRectangle, placeType != AssetType.None);
			AssetRenderer.DrawOverlays(Bounds);

			var builder = selectedAssetsAndMarkers.FirstOrDefault();
			if (placeType != AssetType.Wall)
				AssetRenderer.DrawPlacement(Bounds, new Position(selectionRectangle.X, selectionRectangle.Y), placeType, builder);
			else
			{
				// Draw wall placements from the builder's position to the cursor's position.
				if (builder != null)
				{
					if (Math.Abs(builder.Position.X - selectionRectangle.X) > Math.Abs(builder.Position.Y - selectionRectangle.Y))
					{
						var start = Math.Min(builder.Position.X, selectionRectangle.X);
						var stop = Math.Max(builder.Position.X, selectionRectangle.X);
						for (var x = start; x < stop + Position.HalfTileWidth; x += Position.TileWidth)
							AssetRenderer.DrawPlacement(Bounds, new Position(x, builder.Position.Y), placeType, builder);
					}
					else
					{
						var start = Math.Min(builder.Position.Y, selectionRectangle.Y);
						var stop = Math.Max(builder.Position.Y, selectionRectangle.Y);
						for (var y = start; y < stop + Position.HalfTileHeight; y += Position.TileHeight)
							AssetRenderer.DrawPlacement(Bounds, new Position(builder.Position.X, y), placeType, builder);
					}
				}
			}

			FogRenderer.DrawMap(Bounds);

			ScreenManager.SpriteBatch.End();
		}
	}
}