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
#endregion

namespace SimonsGame
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class MainGame : Game
	{
		GraphicsDeviceManager graphics;
		private SpriteBatch _spriteBatch;
		private Vector2 CurrentWindowSize { get; set; }
		GameStateManager _gameStateManager;
		GameStateManager GameStateManager { get { return _gameStateManager; } }

		private static PlayerManager _playerManager = new PlayerManager();
		public static PlayerManager PlayerManager { get { return _playerManager; } }

		public MainGame()
		{
			CurrentWindowSize = new Vector2(1580, 1080);

			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = (int)CurrentWindowSize.X;
			graphics.PreferredBackBufferHeight = (int)CurrentWindowSize.Y;
			graphics.IsFullScreen = true;

			//graphics.GraphicsDevice.Viewport.X;
			Content.RootDirectory = "Content";
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
			_gameStateManager = new GameStateManager(Services, CurrentWindowSize);
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
			Tuple<MouseProperties, Dictionary<Guid, PlayerControls>> AllControlsTuple = Controls.GetControls(_playerManager);
			_gameStateManager.Update(gameTime, AllControlsTuple.Item2, AllControlsTuple.Item1.MousePosition);
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			_gameStateManager.Draw(gameTime, _spriteBatch);

			base.Draw(gameTime);
		}
	}
}
