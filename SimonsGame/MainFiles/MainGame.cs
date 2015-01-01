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

		#region Graphics
		public static SpriteFont PlainFont;
		GraphicsDeviceManager graphics;
		private SpriteBatch _spriteBatch;
		public static Vector2 CurrentWindowSize { get; set; }

		public static Texture2D SingleColor;

		public static Texture2D Cursor;
		#endregion

		private MainGameState _gameState = MainGameState.Menu;

		public MainGame()
		{
			CurrentWindowSize = new Vector2(1580, 1080);

			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = (int)CurrentWindowSize.X;
			graphics.PreferredBackBufferHeight = (int)CurrentWindowSize.Y;
			graphics.IsFullScreen = true;
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			_playerManager.AddPlayer(TempControls.GetPlayerInput(0));
			_playerManager.AddPlayer(new UsableInputMap() { IsAi = true });
			_playerManager.AddPlayer(new UsableInputMap() { IsAi = true });

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
			Content.RootDirectory = "Content";
			PlainFont = Content.Load<SpriteFont>("Fonts/PlainFont");
			_gameStateManager = new GameStateManager(Services, this, GraphicsDevice.Viewport);
			_menuStateManager = new MenuStateManager(this, Content);

			IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));
			SingleColor = new Texture2D(graphicsService.GraphicsDevice, 1, 1);
			SingleColor.SetData(new[] { Color.White });
			Cursor = Content.Load<Texture2D>("Cursor/CursorStar");
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

			if (_gameState == MainGameState.Menu)
				_menuStateManager.Update(gameTime, AllControlsTuple.Item2, AllControlsTuple.Item1.MousePosition);
			else // (_gameState == MainGameState.Game)
				_gameStateManager.Update(gameTime, AllControlsTuple.Item2, AllControlsTuple.Item1.MousePosition);

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

		public void StartGame(GameSettings gameSettings)
		{
			_gameState = MainGameState.Game;
			_gameStateManager.StartNewGame(gameSettings);
		}

		public void EndGame(GameStatistics gameStatistics)
		{
			_gameState = MainGameState.Menu;
			_menuStateManager.ShowGameStatistics(gameStatistics);
		}
	}
}
