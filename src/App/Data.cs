using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Warcraft.Ai;
using Warcraft.Assets;
using Warcraft.Player;
using Warcraft.Renderers;
using Warcraft.Screens.Components;
using Warcraft.Screens.Manager;
using Warcraft.Util;
using ButtonRenderer = Warcraft.Renderers.ButtonRenderer;
using Point = Microsoft.Xna.Framework.Point;

namespace Warcraft.App
{
	/// <summary>
	/// *Global Scope* Application Data
	/// </summary>
	/// <remarks>
	/// Please use with caution!
	/// Only add variables here if *necessary*!
	/// </remarks>
	public static class Data
	{
		#region Variables

		/// <summary>
		/// A task holding the loading of assets.
		/// </summary>
		public static Task LoadingTask { get; set; }

		/// <summary>
		/// Gets or sets whether the game has finished loading.
		/// </summary>
		public static bool GameReady { get; set; }

		/// <summary>
		/// Holds the indices to the different cursor types.
		/// </summary>
		public static int[] CursorIndices { get; set; } = new int[(int)CursorType.Max];
		/// <summary>
		/// The current cursor type draw.
		/// </summary>
		public static CursorType CursorType { get; set; } = CursorType.Pointer;

		/// <summary>
		/// The index of the map selected to play.
		/// </summary>
		public static int SelectedMapIndex { get; set; }
		/// <summary>
		/// The map selected to play.
		/// </summary>
		public static DecoratedMap SelectedMap { get; set; }

		#endregion

		#region Game Constants

		/// <summary>
		/// The minimum size that the mini map can be.
		/// </summary>
		public static readonly Size MiniMapMinimumSize = new Size(128, 128);

		#endregion

		#region Game Assets

		/// <summary>
		/// A shared <see cref="CursorSet"/> for all screens.
		/// </summary>
		public static CursorSet CursorSet { get; private set; }

		/// <summary>
		/// <see cref="RecolorMap"/> used for coloring fonts.
		/// </summary>
		public static RecolorMap FontRecolorMap { get; private set; }

		/// <summary>
		/// A shared set of <see cref="FontTileset"/>s for all screens.
		/// </summary>
		public static Dictionary<FontSize, FontTileset> Fonts { get; private set; }

		/// <summary>
		/// Tileset used for the background of menu screens.
		/// </summary>
		public static Tileset BackgroundTileset { get; private set; }

		/// <summary>
		/// Bevel used for screens and buttons
		/// </summary>
		public static Bevel MiniBevel { get; private set; }
		/// <summary>
		/// Bevel used for screens and buttons
		/// </summary>
		public static Bevel InnerBevel { get; private set; }
		/// <summary>
		/// Bevel used for screens and buttons
		/// </summary>
		public static Bevel OuterBevel { get; private set; }

		/// <summary>
		/// <see cref="RecolorMap"/> used for coloring buttons.
		/// </summary>
		public static RecolorMap ButtonRecolorMap { get; private set; }

		/// <summary>
		/// A shared <see cref="ButtonRenderer"/> for all screens.
		/// </summary>
		public static ButtonRenderer ButtonRenderer { get; private set; }

		/// <summary>
		/// A shared <see cref="TextFieldRenderer"/> for all screens.
		/// </summary>
		public static TextFieldRenderer TextFieldRenderer { get; private set; }

		/// <summary>
		/// <see cref="RecolorMap"/> used for coloring players.
		/// </summary>
		public static RecolorMap PlayerRecolorMap { get; private set; }

		/// <summary>
		/// <see cref="RecolorMap"/> used for coloring assets.
		/// </summary>
		public static RecolorMap AssetRecolorMap { get; private set; }

		/// <summary>
		/// Asset Tilesets
		/// </summary>
		public static List<MulticolorTileset> AssetTilesets { get; private set; }

		/// <summary>
		/// Terrain Tilesets
		/// </summary>
		public static Tileset TerrainTileset { get; private set; }

		/// <summary>
		/// Marker Tilesets
		/// </summary>
		public static Tileset MarkerTileset { get; private set; }

		/// <summary>
		/// Fire Tilesets
		/// </summary>
		public static List<Tileset> FireTilesets { get; private set; }

		/// <summary>
		/// Building Death Tileset
		/// </summary>
		public static Tileset BuildingDeathTileset { get; private set; }

		/// <summary>
		/// Arrow Tilesets
		/// </summary>
		public static Tileset ArrowTileset { get; private set; }

		public static Tileset FogTileset { get; private set; }

		public static Tileset MiniIconTileset { get; private set; }

		public static MulticolorTileset IconTileset { get; private set; }

		/// <summary>
		/// Render target used for list views.
		/// </summary>
		public static RenderTarget2D ListViewRenderTarget { get; set; }

		/// <summary>
		/// Viewport Renderer
		/// </summary>
		public static ViewportRenderer ViewportRenderer { get; private set; }

		/// <summary>
		/// Render target used for the map/viewport.
		/// </summary>
		public static RenderTarget2D ViewportRenderTarget { get; private set; }

		/// <summary>
		/// Render target used to hold tile type information.
		/// </summary>
		public static RenderTarget2D TypeRenderTarget { get; private set; }

		/// <summary>
		/// Render target for the mini map.
		/// </summary>
		public static RenderTarget2D MiniMapRenderTarget { get; private set; }

		/// <summary>
		/// Render target for the player's resource information.
		/// </summary>
		public static RenderTarget2D ResourceRenderTarget { get; private set; }

		/// <summary>
		/// Render target for the unit description.
		/// </summary>
		public static RenderTarget2D UnitDescriptionRenderTarget { get; private set; }

		/// <summary>
		/// Render target for the unit action.
		/// </summary>
		public static RenderTarget2D UnitActionRenderTarget { get; private set; }

		/// <summary>
		/// The distance from the top left corner of the game window
		/// to the mini map.
		/// </summary>
		public static Point MiniMapOffset;

		/// <summary>
		/// The distance from the top left corner of the game window
		/// to the unit description box.
		/// </summary>
		public static Point UnitDescriptionOffset;

		/// <summary>
		/// The distance from the top left corner of the game window
		/// to the unit action box.
		/// </summary>
		public static Point UnitActionOffset;

		/// <summary>
		/// The distance from the top left corner of the game window
		/// to the menu button.
		/// </summary>
		public static Point MenuButtonOffset;

		/// <summary>
		/// The offset of the viewport.
		/// </summary>
		public static Point ViewportOffset;

		/// <summary>
		/// Stopwatch for measuring game loading time.
		/// </summary>
		public static Stopwatch LoadStopwatch { get; } = new Stopwatch();

		#endregion

		#region Game Model

		public static GameModel.GameModel GameModel { get; set; }

		public static List<AiPlayer> AiPlayers { get; set; } = new List<AiPlayer>(new AiPlayer[(int)PlayerColor.Max]);
		public static List<PlayerType> LoadingPlayerTypes { get; set; } = new List<PlayerType>(new PlayerType[(int)PlayerColor.Max]);
		public static List<PlayerColor> LoadingPlayerColors { get; set; } = new List<PlayerColor>(new PlayerColor[(int)PlayerColor.Max]);

		public static MapRenderer MapRenderer { get; set; }

		public static AssetRenderer AssetRenderer { get; set; }

		public static FogRenderer FogRenderer { get; set; }

		public static MiniMapRenderer MiniMapRenderer { get; set; }

		public static ResourceRenderer ResourceRenderer { get; set; }

		public static UnitDescriptionRenderer UnitDescriptionRenderer { get; set; }

		public static UnitActionRenderer UnitActionRenderer { get; set; }

		public static SoundEventRenderer SoundEventRenderer { get; private set; }

		public static ButtonRenderer MenuButtonRenderer { get; private set; }

		#endregion

		#region Player Information

		public static PlayerColor PlayerColor { get; set; } = PlayerColor.Blue;

		public static AssetCapabilityType CurrentAssetCapabilityType { get; set; } = AssetCapabilityType.None;

		#endregion

		#region Miscellaneous Settings

		/// <summary>
		/// Border Width
		/// </summary>
		/// <remarks>
		/// This only gets set once to 32 in the Linux version.
		/// </remarks>
		public const int BorderWidth = 32;

		#endregion

		public static void Load()
		{
			LoadCursors();
			LoadFonts();
			LoadAssets();
			CreateGame();
			GameModel = null;
		}

		/// <summary>
		/// Loads all cursors into <see cref="CursorSet"/>.
		/// </summary>
		private static void LoadCursors()
		{
			CursorSet = new CursorSet();
			CursorSet.Load(Path.Combine(Paths.Image, "Cursors.dat"));

			CursorIndices[(int)CursorType.Pointer] = CursorSet.FindCursor("pointer");
			CursorIndices[(int)CursorType.Inspect] = CursorSet.FindCursor("magnifier");
			CursorIndices[(int)CursorType.ArrowN] = CursorSet.FindCursor("arrow-n");
			CursorIndices[(int)CursorType.ArrowE] = CursorSet.FindCursor("arrow-e");
			CursorIndices[(int)CursorType.ArrowS] = CursorSet.FindCursor("arrow-s");
			CursorIndices[(int)CursorType.ArrowW] = CursorSet.FindCursor("arrow-w");
			CursorIndices[(int)CursorType.TargetOff] = CursorSet.FindCursor("target-off");
			CursorIndices[(int)CursorType.TargetOn] = CursorSet.FindCursor("target-on");
		}

		/// <summary>
		/// Loads all fonts into <see cref="Fonts"/>.
		/// </summary>
		private static void LoadFonts()
		{
			FontRecolorMap = new RecolorMap();
			FontRecolorMap.Load(Path.Combine(Paths.Image, "FontColors.dat"));
			Fonts = new Dictionary<FontSize, FontTileset>();

			var smallFont = new FontTileset(FontRecolorMap);
			smallFont.Load(Path.Combine(Paths.Image, "FontKingthings10.dat"));
			Fonts.Add(FontSize.Small, smallFont);

			var mediumFont = new FontTileset(FontRecolorMap);
			mediumFont.Load(Path.Combine(Paths.Image, "FontKingthings12.dat"));
			Fonts.Add(FontSize.Medium, mediumFont);

			var largeFont = new FontTileset(FontRecolorMap);
			largeFont.Load(Path.Combine(Paths.Image, "FontKingthings16.dat"));
			Fonts.Add(FontSize.Large, largeFont);

			var giantFont = new FontTileset(FontRecolorMap);
			giantFont.Load(Path.Combine(Paths.Image, "FontKingthings24.dat"));
			Fonts.Add(FontSize.Giant, giantFont);
		}

		/// <summary>
		/// Loads all assets.
		/// </summary>
		private static void LoadAssets()
		{
			// Load background tileset
			BackgroundTileset = new Tileset();
			BackgroundTileset.Load(Path.Combine(Paths.Image, "Texture.dat"));

			// Load bevels
			var miniBevelTileset = new Tileset();
			miniBevelTileset.Load(Path.Combine(Paths.Image, "MiniBevel.dat"));
			MiniBevel = new Bevel(miniBevelTileset);

			var innerBevelTileset = new Tileset();
			innerBevelTileset.Load(Path.Combine(Paths.Image, "InnerBevel.dat"));
			InnerBevel = new Bevel(innerBevelTileset);

			var outerBevelTileset = new Tileset();
			outerBevelTileset.Load(Path.Combine(Paths.Image, "OuterBevel.dat"));
			OuterBevel = new Bevel(outerBevelTileset);

			// Load button recolor map
			ButtonRecolorMap = new RecolorMap();
			ButtonRecolorMap.Load(Path.Combine(Paths.Image, "ButtonColors.dat"));

			// Load component renderers
			ButtonRenderer = new ButtonRenderer(ButtonRecolorMap, InnerBevel, OuterBevel, Fonts[FontSize.Large]);
			TextFieldRenderer = new TextFieldRenderer(ButtonRecolorMap, InnerBevel, Fonts[FontSize.Large]);

			// Load terrain tileset
			TerrainTileset = new Tileset();
			TerrainTileset.Load(Path.Combine(Paths.Image, "Terrain.dat"));
			Position.SetTileDimensions(TerrainTileset.TileWidth, TerrainTileset.TileHeight);

			// Load fire tilesets
			FireTilesets = new List<Tileset>();

			var fireTileset = new Tileset();
			fireTileset.Load(Path.Combine(Paths.Image, "FireSmall.dat"));
			FireTilesets.Add(fireTileset);

			fireTileset = new Tileset();
			fireTileset.Load(Path.Combine(Paths.Image, "FireLarge.dat"));
			FireTilesets.Add(fireTileset);

			// Load building death tileset
			BuildingDeathTileset = new Tileset();
			BuildingDeathTileset.Load(Path.Combine(Paths.Image, "BuildingDeath.dat"));

			// Load arrow tileset
			ArrowTileset = new Tileset();
			ArrowTileset.Load(Path.Combine(Paths.Image, "Arrow.dat"));

			// Load marker tileset
			MarkerTileset = new Tileset();
			MarkerTileset.Load(Path.Combine(Paths.Image, "Marker.dat"));

			// Load asset recolor map
			PlayerRecolorMap = new RecolorMap();
			PlayerRecolorMap.Load(Path.Combine(Paths.Image, "Colors.dat"));
			AssetRecolorMap = new RecolorMap();
			AssetRecolorMap.Load(Path.Combine(Paths.Image, "AssetColor.dat"));

			// Load asset tilesets
			AssetTilesets = new List<MulticolorTileset>(new MulticolorTileset[(int)AssetType.Max]);

			// None
			AssetTilesets[(int)AssetType.None] = null;

			// Wall
			AssetTilesets[(int)AssetType.Wall] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Wall].Load(Path.Combine(Paths.Image, "Wall.dat"));

			// Peasant
			AssetTilesets[(int)AssetType.Peasant] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Peasant].Load(Path.Combine(Paths.Image, "Peasant.dat"));

			// Footman
			AssetTilesets[(int)AssetType.Footman] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Footman].Load(Path.Combine(Paths.Image, "Footman.dat"));

			// Knight
			AssetTilesets[(int)AssetType.Knight] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Knight].Load(Path.Combine(Paths.Image, "Knight.dat"));

			// Archer
			AssetTilesets[(int)AssetType.Archer] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Archer].Load(Path.Combine(Paths.Image, "Archer.dat"));

			// Ranger
			AssetTilesets[(int)AssetType.Ranger] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Ranger].Load(Path.Combine(Paths.Image, "Ranger.dat"));

			// GoldMine
			AssetTilesets[(int)AssetType.GoldMine] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.GoldMine].Load(Path.Combine(Paths.Image, "GoldMine.dat"));

			// TownHall
			AssetTilesets[(int)AssetType.TownHall] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.TownHall].Load(Path.Combine(Paths.Image, "TownHall.dat"));

			// Keep
			AssetTilesets[(int)AssetType.Keep] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Keep].Load(Path.Combine(Paths.Image, "Keep.dat"));

			// Castle
			AssetTilesets[(int)AssetType.Castle] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Castle].Load(Path.Combine(Paths.Image, "Castle.dat"));

			// Farm
			AssetTilesets[(int)AssetType.Farm] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Farm].Load(Path.Combine(Paths.Image, "Farm.dat"));

			// Barracks
			AssetTilesets[(int)AssetType.Barracks] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Barracks].Load(Path.Combine(Paths.Image, "Barracks.dat"));

			// Blacksmith
			AssetTilesets[(int)AssetType.Blacksmith] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.Blacksmith].Load(Path.Combine(Paths.Image, "Blacksmith.dat"));

			// LumberMill
			AssetTilesets[(int)AssetType.LumberMill] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.LumberMill].Load(Path.Combine(Paths.Image, "LumberMill.dat"));

			// ScoutTower
			AssetTilesets[(int)AssetType.ScoutTower] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.ScoutTower].Load(Path.Combine(Paths.Image, "ScoutTower.dat"));

			// GuardTower
			AssetTilesets[(int)AssetType.GuardTower] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.GuardTower].Load(Path.Combine(Paths.Image, "GuardTower.dat"));

			// CannonTower
			AssetTilesets[(int)AssetType.CannonTower] = new MulticolorTileset(PlayerRecolorMap);
			AssetTilesets[(int)AssetType.CannonTower].Load(Path.Combine(Paths.Image, "CannonTower.dat"));

			// Fog
			FogTileset = new Tileset();
			FogTileset.Load(Path.Combine(Paths.Image, "Fog.dat"));

			// MiniIcons
			MiniIconTileset = new Tileset();
			MiniIconTileset.Load(Path.Combine(Paths.Image, "MiniIcons.dat"));

			// Icons
			IconTileset = new MulticolorTileset(PlayerRecolorMap);
			IconTileset.Load(Path.Combine(Paths.Image, "Icons.dat"));
		}

		/// <summary>
		/// Recalculates UI component boundaries.
		/// </summary>
		public static void ResizeWindow()
		{
			var drawingSize = new Size(ScreenManager.Graphics.Viewport.Width, ScreenManager.Graphics.Viewport.Height);
			var viewportSize = new Size(drawingSize.Width - ViewportOffset.X - BorderWidth, drawingSize.Height - ViewportOffset.Y - BorderWidth);

			if (MiniMapRenderTarget == null)
			{
				var dimension = ViewportOffset.X - InnerBevel.Width * 5;
				MiniMapRenderTarget = new RenderTarget2D(ScreenManager.Graphics, dimension, dimension);
			}

			if (UnitDescriptionRenderer != null)
			{
				var unitDescriptionWidth = ViewportOffset.X - InnerBevel.Width - OuterBevel.Width * 4;
				var unitDescriptionHeight = UnitDescriptionRenderer.MinimumHeight(unitDescriptionWidth, 9);
				if (UnitDescriptionRenderTarget == null || UnitDescriptionRenderTarget.Width != unitDescriptionWidth || UnitDescriptionRenderTarget.Height != unitDescriptionHeight)
					UnitDescriptionRenderTarget = new RenderTarget2D(ScreenManager.Graphics, unitDescriptionWidth, unitDescriptionHeight);
			}

			if (UnitActionRenderer != null && (UnitActionRenderTarget == null || UnitActionRenderTarget.Width != UnitActionRenderer.MinimumWidth() || UnitActionRenderTarget.Height != UnitActionRenderer.MinimumHeight()))
				UnitActionRenderTarget = new RenderTarget2D(ScreenManager.Graphics, UnitActionRenderer.MinimumWidth(), UnitActionRenderer.MinimumHeight());

			if (ResourceRenderTarget == null || ResourceRenderTarget.Width != viewportSize.Width || ResourceRenderTarget.Height != BorderWidth)
				ResourceRenderTarget = new RenderTarget2D(ScreenManager.Graphics, viewportSize.Width, BorderWidth);

			if (ViewportRenderTarget == null || ViewportRenderTarget.Width != viewportSize.Width || ViewportRenderTarget.Height != viewportSize.Height)
			{
				if (viewportSize.Width < 0) viewportSize.Width = 1;
				if (viewportSize.Height < 0) viewportSize.Height = 1;

				ViewportRenderTarget = new RenderTarget2D(ScreenManager.Graphics, viewportSize.Width, viewportSize.Height);
				TypeRenderTarget = new RenderTarget2D(ScreenManager.Graphics, viewportSize.Width, viewportSize.Height);
			}
		}

		/// <summary>
		/// Creates a new game.
		/// </summary>
		public static void CreateGame()
		{
			if (GameModel == null)
				GameModel = new GameModel.GameModel(SelectedMapIndex, 0x123456789ABCDEFu, LoadingPlayerColors);

			// Todo: This is wrong.
			for (var index = 1; index < (int)PlayerColor.Max; index++)
				GameModel.Player(PlayerColor).IsAi = PlayerType.AiEasy <= LoadingPlayerTypes[index] && LoadingPlayerTypes[index] <= PlayerType.AiHard;

			for (var index = 1; index < (int)PlayerColor.Max; index++)
			{
				if (!GameModel.Player((PlayerColor)index).IsAi) continue;

				int downSample;
				switch (LoadingPlayerTypes[index])
				{
					case PlayerType.AiEasy: downSample = PlayerAsset.UpdateFrequency; break;
					case PlayerType.AiMedium: downSample = PlayerAsset.UpdateFrequency / 2; break;
					default: downSample = PlayerAsset.UpdateFrequency / 4; break;
				}
				AiPlayers[index] = new AiPlayer(GameModel.Player((PlayerColor)index), downSample);
			}

			MapRenderer = new MapRenderer(Path.Combine(Paths.Image, "MapRendering.dat"), TerrainTileset, GameModel.Player(PlayerColor).PlayerMap);
			AssetRenderer = new AssetRenderer(AssetRecolorMap, AssetTilesets, MarkerTileset, FireTilesets, BuildingDeathTileset, ArrowTileset, GameModel.Player(PlayerColor), GameModel.Player(PlayerColor).PlayerMap);
			FogRenderer = new FogRenderer(FogTileset, GameModel.Player(PlayerColor).VisibilityMap);
			ViewportRenderer = new ViewportRenderer(MapRenderer, AssetRenderer, FogRenderer);
			MiniMapRenderer = new MiniMapRenderer(MapRenderer, AssetRenderer, FogRenderer);
			UnitDescriptionRenderer = new UnitDescriptionRenderer(IconTileset, MiniBevel, PlayerColor);
			UnitActionRenderer = new UnitActionRenderer(IconTileset, MiniBevel, GameModel.Player(PlayerColor));
			ResourceRenderer = new ResourceRenderer(MiniIconTileset, Fonts[FontSize.Medium], GameModel.Player(PlayerColor));
			SoundEventRenderer = new SoundEventRenderer(GameModel.Player(PlayerColor));

			MenuButtonRenderer = new ButtonRenderer(ButtonRecolorMap, InnerBevel, OuterBevel, Fonts[FontSize.Medium]);
			MenuButtonRenderer.SetText("Menu");
			MenuButtonRenderer.ButtonColor = PlayerColor;

			// Set up UI offsets
			var leftPanelWidth = Math.Max(UnitDescriptionRenderer.MinimumWidth(), UnitActionRenderer.MinimumWidth()) + OuterBevel.Width * 4;
			leftPanelWidth = Math.Max(leftPanelWidth, MiniMapMinimumSize.Width + InnerBevel.Width * 4);

			MiniMapOffset.X = InnerBevel.Width * 2;
			UnitActionOffset.X = UnitDescriptionOffset.X = OuterBevel.Width * 2;
			ViewportOffset.X = leftPanelWidth + InnerBevel.Width;

			MiniMapOffset.Y = BorderWidth;
			UnitDescriptionOffset.Y = MiniMapOffset.Y + (leftPanelWidth - InnerBevel.Width * 3) + OuterBevel.Width * 2;
			var minUnitDescriptionHeight = UnitDescriptionRenderer.MinimumHeight(leftPanelWidth - OuterBevel.Width * 4, 9);
			UnitActionOffset.Y = UnitDescriptionOffset.Y + minUnitDescriptionHeight + OuterBevel.Width * 3;
			ViewportOffset.Y = BorderWidth;

			ResizeWindow();

			MenuButtonRenderer.SetWidth(ViewportOffset.X / 2);
			MenuButtonOffset.X = ViewportOffset.X / 2 - MenuButtonRenderer.TextArea.Width / 2;
			MenuButtonOffset.Y = (ViewportOffset.Y - OuterBevel.Width) / 2 - MenuButtonRenderer.TextArea.Height / 2;
		}

		/// <summary>
		/// Resets the player colors.
		/// </summary>
		public static void ResetPlayerColors()
		{
			for (var index = 0; index < (int)PlayerColor.Max; index++)
				LoadingPlayerColors[index] = (PlayerColor)index;
		}

		/// <summary>
		/// Serializes the <see cref="Settings"/> and saves it to a file.
		/// </summary>
		public static void SaveSettings()
		{
			Trace.TraceInformation("Saving settings...");

			using (var fs = new FileStream(Settings.SettingsFileName, FileMode.Create))
			using (var sw = new StreamWriter(fs))
			using (var jw = new JsonTextWriter(sw))
			{
				var serializer = new JsonSerializer
				{
					Formatting = Formatting.Indented
				};
				serializer.Serialize(jw, Settings.Default);
			}

			Trace.TraceInformation("Settings saved successfully.");
		}

		/// <summary>
		/// Deserializes the settings file and loads it into <see cref="Settings"/>.
		/// </summary>
		public static void LoadSettings()
		{
			try
			{
				if (!File.Exists(Settings.SettingsFileName)) return;

				using (var fs = new FileStream(Settings.SettingsFileName, FileMode.Open))
				using (var sr = new StreamReader(fs))
				using (var jr = new JsonTextReader(sr))
				{
					Settings.Default = new JsonSerializer().Deserialize<Settings>(jr);
				}
			}
			catch (Exception ex) when (ex is UnauthorizedAccessException || ex is JsonSerializationException || ex is JsonReaderException)
			{
				Trace.TraceInformation(ex.ToString());
				MessageBox.Show("Failed to settings.");
			}
		}

		/// <summary>
		/// Serializes the <see cref="GameModel"/> and saves it to a file.
		/// </summary>
		public static void SaveGame()
		{
			if (GameModel == null) return;

			Trace.TraceInformation("Saving game...");

			using (var fs = new FileStream(Settings.SaveFileName, FileMode.Create))
			using (var zs = new GZipStream(fs, CompressionMode.Compress, false))
			using (var sw = new StreamWriter(zs))
			using (var jw = new JsonTextWriter(sw))
			{
				var serializer = new JsonSerializer
				{
					PreserveReferencesHandling = PreserveReferencesHandling.Objects,
					ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
					TypeNameHandling = TypeNameHandling.Auto,
					ContractResolver = new PrivateSetterContractResolver()
				};
				serializer.Serialize(jw, GameModel);
			}

			GameModel = null;

			Trace.TraceInformation("Game saved successfully.");
		}

		/// <summary>
		/// Deserializes the saved game file and loads it into the <see cref="GameModel"/>.
		/// </summary>
		public static bool LoadSavedGame()
		{
			try
			{
				if (new FileInfo(Settings.SaveFileName).Length == 0)
					throw new FormatException();

				using (var fs = new FileStream(Settings.SaveFileName, FileMode.Open))
				using (var zs = new GZipStream(fs, CompressionMode.Decompress, false))
				using (var sr = new StreamReader(zs))
				using (var jr = new JsonTextReader(sr))
				{
					var serializer = new JsonSerializer
					{
						TypeNameHandling = TypeNameHandling.Auto,
						ContractResolver = new PrivateSetterContractResolver()
					};
					GameModel = serializer.Deserialize<GameModel.GameModel>(jr);
				}

				return true;
			}
			catch (Exception ex) when (ex is UnauthorizedAccessException || ex is JsonSerializationException || ex is JsonReaderException || ex is FormatException)
			{
				Trace.TraceInformation(ex.ToString());
				MessageBox.Show("Failed to load saved game data.");
				return false;
			}
		}

		/// <summary>
		/// A contract resolver that will include properties
		/// with private setters for serialization.
		/// </summary>
		private class PrivateSetterContractResolver : DefaultContractResolver
		{
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var jProperty = base.CreateProperty(member, memberSerialization);
				if (jProperty.Writable) return jProperty;

				jProperty.Writable = (member as PropertyInfo)?.GetSetMethod(true) != null;
				return jProperty;
			}
		}
	}
}
