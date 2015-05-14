#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using SimonsGame.GuiObjects;
using SimonsGame.Test;
using SimonsGame.Menu;
using SimonsGame.MainFiles;
using SimonsGame.MainFiles.InGame;
using SimonsGame.MapEditor;
using System.Windows;
#endregion

namespace SimonsGame
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class MainGame : Game
	{
		public enum MainGameState
		{
			Menu,
			Game
		}

		GameStateManager _gameStateManager;
		GameStateManager GameStateManager { get { return _gameStateManager; } }
		MenuStateManager _menuStateManager;
		MenuStateManager MenuStateManager { get { return _menuStateManager; } }

		private static PlayerManager _playerManager = new PlayerManager();
		public static PlayerManager PlayerManager { get { return _playerManager; } }

		public static Random Randomizer { get; private set; }

		#region Graphics
		public static SpriteFont PlainFont;
		public static SpriteFont PlainFontLarge;
		GraphicsDeviceManager graphics;
		private SpriteBatch _spriteBatch;
		public static Vector2 CurrentWindowSize { get; set; }

		public static Texture2D SingleColor;

		public static Texture2D Cursor;

		public static ContentManager ContentManager
		{ get { return _content; } }
		private static ContentManager _content;
		#endregion

		private static MainGameState _gameState = MainGameState.Menu;
		public static MainGameState GameState { get { return _gameState; } }

		public MainGame()
		{
			Randomizer = new Random();
			CurrentWindowSize = new Vector2((float)SystemParameters.WorkArea.Width - 30, (float)SystemParameters.WorkArea.Height - 50);
			//CurrentWindowSize = new Vector2((float)SystemParameters.PrimaryScreenWidth, (float)SystemParameters.PrimaryScreenHeight);
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = (int)CurrentWindowSize.X;
			graphics.PreferredBackBufferHeight = (int)CurrentWindowSize.Y;
			//graphics.IsFullScreen = true;

			Type type = typeof(OpenTKGameWindow);
			System.Reflection.FieldInfo field = type.GetField("window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			if (field != null)
			{
				OpenTK.GameWindow openTKWindow = field.GetValue(Window) as OpenTK.GameWindow;
				if (openTKWindow != null)
				{
					openTKWindow.X = 10;
					openTKWindow.Y = 10;
				}

				//openTKWindow.TopMost


				// Uncomment for full screen
				//openTKWindow.CursorVisible = false;
				//openTKWindow.WindowState = OpenTK.WindowState.Maximized;
				//Window.AllowUserResizing = false;
				//Window.IsBorderless = true;

				//openTKWindow.WindowBorder = OpenTK.WindowBorder.Hidden;
				//openTKWindow.WindowBorder = OpenTK.WindowBorder.Fixed;
				//Window.IsBorderless = false;
				//Window.Title = "";

			}
			//graphics.ApplyChanges();
			_content = new ContentManager(Services, "Content");
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			//_playerManager.AddPlayer(TempControls.GetPlayerInput(0));
			//_playerManager.AddPlayer(new UsableInputMap() { IsAi = true });
			//_playerManager.AddPlayer(new UsableInputMap() { IsAi = true });

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			ContentManager.RootDirectory = "Content";
			PlainFont = ContentManager.Load<SpriteFont>("Fonts/PlainFont");
			PlainFontLarge = ContentManager.Load<SpriteFont>("Fonts/PlainFontLarge");
			_gameStateManager = new GameStateManager(Services, this, GraphicsDevice.Viewport);
			_menuStateManager = new MenuStateManager(this, ContentManager);

			IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));
			SingleColor = new Texture2D(graphicsService.GraphicsDevice, 1, 1);
			SingleColor.SetData(new[] { Color.White });
			Cursor = ContentManager.Load<Texture2D>("Cursor/CursorStar");
			MapEditorIOManager.Initialize();
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Get all the controls of the players
			Tuple<MouseProperties, Dictionary<Guid, PlayerControls>> AllControlsTuple = Controls.GetControls(_playerManager);
			Controls.Update(AllControlsTuple.Item2);
			if (_gameState == MainGameState.Menu)
				_menuStateManager.Update(gameTime, AllControlsTuple.Item1.MousePosition);
			else // (_gameState == MainGameState.Game)
				_gameStateManager.Update(gameTime, AllControlsTuple.Item1.MousePosition);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			if (_gameState == MainGameState.Menu)
			{
				GraphicsDevice.Clear(Color.CornflowerBlue);
				_menuStateManager.Draw(gameTime, _spriteBatch);
			}
			else // (_gameState == MainGameState.Game)
			{
				GraphicsDevice.Clear(Color.Black);
				_gameStateManager.Draw(gameTime, _spriteBatch);
			}

			base.Draw(gameTime);
		}

		public bool StartGame(GameSettings gameSettings)
		{
			_gameState = MainGameState.Game;
			bool didLoadSuccessfully = _gameStateManager.StartNewGame(gameSettings);
			if (didLoadSuccessfully)// If it loaded, then initialize and start the game already!
				_gameStateManager.Level.Initialize();
			else
				_gameState = MainGameState.Menu;
			return didLoadSuccessfully;
		}

		public void EndGame(GameStatistics gameStatistics)
		{
			_gameState = MainGameState.Menu;
			_menuStateManager.ShowGameStatistics(gameStatistics);
			_playerManager.RemoveAllAiPlayers();
		}
	}
}
