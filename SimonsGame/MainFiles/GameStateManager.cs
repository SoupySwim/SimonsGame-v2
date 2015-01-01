using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.MainFiles;
using SimonsGame.MainFiles.InGame;

namespace SimonsGame
{
	public enum GameStateManagerGameState
	{
		InGame,
		Paused,
		StartingGame
	}
	public class GameStateManager
	{

		#region DegubArea
		public static bool ShowHitBoxes { get; set; }
		public static TimeSpan GameTimer = TimeSpan.Zero;
		public static bool GameTimerRunning = false;
		#endregion

		#region GameState
		private GameStateManagerGameState _gameState;

		private TimeSpan _countdownToStartGameMax = new TimeSpan(0, 0, 1);
		private TimeSpan _countdownToStartGame;

		#endregion

		#region Controls
		public static Dictionary<Guid, PlayerControls> AllControls { get; set; }
		public static Dictionary<Guid, PlayerControls> PreviousControls { get; set; }
		private static Dictionary<Guid, AIUtility> _aiUtilityMap { get; set; }
		#endregion

		#region Graphics
		public Level Level { get; set; }

		private Texture2D _solidColor;
		private Viewport _baseViewport;

		private Vector2 _mousePosition;
		public Vector2 MousePosition { get { return _mousePosition; } } // Will have to compensate for camera offset when the time comes.

		private Dictionary<Guid, PlayerViewport> _playerViewports;
		public PlayerViewport GetPlayerViewport(Player player)
		{
			PlayerViewport value = null;
			_playerViewports.TryGetValue(player.Id, out value);
			return value;
		}

		private IServiceProvider _serviceProvider;
		#endregion

		private GameStatistics _gameStatistics;
		private MainGame _game;

		public GameStateManager(IServiceProvider serviceProvider, MainGame game, Viewport baseViewPort)
		{
			IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)Program.Game.Services.GetService(typeof(IGraphicsDeviceService));
			//TODO change obviously
			ShowHitBoxes = false;
			_mousePosition = Vector2.Zero;

			_baseViewport = baseViewPort;

			_solidColor = new Texture2D(graphicsService.GraphicsDevice, 1, 1);
			_solidColor.SetData(new[] { Color.Green });
			_game = game;
			_serviceProvider = serviceProvider;
		}
		public void StartNewGame(GameSettings gameSettings)
		{
			//TODO move level to somewhere more meaningful...
			Level = Test.LevelBuilder.BuildLevel3(_serviceProvider, MainGame.CurrentWindowSize + new Vector2(MainGame.CurrentWindowSize.X * .8f, MainGame.CurrentWindowSize.Y * .75f), this);
			_aiUtilityMap = Level.Players.Where(p => p.Value.IsAi).Select(p => new { key = p.Key, value = new AIUtility(p.Value) }).ToDictionary(p => p.key, p => p.value);


			_gameState = GameStateManagerGameState.StartingGame;
			_countdownToStartGame = _countdownToStartGameMax;
			GameTimer = TimeSpan.Zero;
			Player.Sprint3TestScore = 0;

			if (gameSettings.AllowAIScreens)
			{
				float viewportW = MainGame.CurrentWindowSize.X / 2 - 10;
				float viewportH = MainGame.CurrentWindowSize.Y / 2 - 10;
				Vector4[] viewportBounds = new Vector4[]
			{
				new Vector4(5, 5, viewportH, viewportW),
				new Vector4(MainGame.CurrentWindowSize.X / 2 + 5, 5, viewportH, viewportW),
				new Vector4(5, MainGame.CurrentWindowSize.Y / 2 + 5, viewportH, viewportW)
			};
				_playerViewports = Level.Players./*Where(p => !p.Value.IsAi).*/Select((player, ndx) =>
				{
					/* TODO add logic to pick correct screen size... */
					return new
					{
						key = player.Key,
						value = new PlayerViewport(player.Value, viewportBounds[ndx], Level.Size, .7f, this)
					};
				}).ToDictionary(obj => obj.key, obj => obj.value);
			}
			else // Only show human player
			{
				_playerViewports = Level.Players.Where(p => !p.Value.IsAi).Select((player, ndx) =>
				{
					return new
					{
						key = player.Key,
						value = new PlayerViewport(player.Value, new Vector4(0, 0, MainGame.CurrentWindowSize.Y, MainGame.CurrentWindowSize.X), Level.Size, 1f, this)
					};
				}).ToDictionary(obj => obj.key, obj => obj.value);
			}
			_gameStatistics = new GameStatistics();
		}
		public void Update(GameTime gameTime, Dictionary<Guid, PlayerControls> allControls, Vector2 newMousePosition)
		{
			// Update Controls
			PreviousControls = AllControls;
			SetAiControls(allControls);
			AllControls = allControls;
			_mousePosition = newMousePosition;

			foreach (KeyValuePair<Guid, PlayerViewport> playerViewport in _playerViewports)
				playerViewport.Value.Update(gameTime, _gameState);

			switch (_gameState)
			{
				case GameStateManagerGameState.InGame:
					if (GameTimerRunning)
						GameTimer += gameTime.ElapsedGameTime;
					else if (GameTimer == TimeSpan.Zero && allControls.Values.First().XMovement > 0)
						GameTimerRunning = true;

					Level.Update(gameTime);
					// Test if paused.
					//if (TestIfPaused())
					//{
					//	_gameState = GameStateManagerGameState.Paused;
					//}
					break;
				case GameStateManagerGameState.Paused:
					// Nothing to update yet... Probably will update some animation in the future.
					//if (TestIfPaused())
					//{
					//	_gameState = GameStateManagerGameState.InGame;
					//}
					break;
				case GameStateManagerGameState.StartingGame:
					_countdownToStartGame -= gameTime.ElapsedGameTime;
					if (_countdownToStartGame <= TimeSpan.Zero)
					{
						_gameState = GameStateManagerGameState.InGame;
						GameTimerRunning = true;
					}
					foreach (KeyValuePair<Guid, PlayerViewport> playerViewport in _playerViewports)
						playerViewport.Value.SetCountdown(_countdownToStartGame);

					break;
			}
		}
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.GraphicsDevice.Viewport = _baseViewport;

			foreach (KeyValuePair<Guid, PlayerViewport> playerViewport in _playerViewports)
				playerViewport.Value.Draw(gameTime, spriteBatch, Level, _gameState);
			spriteBatch.GraphicsDevice.Viewport = _baseViewport;
			//switch (_gameState)
			//{
			//	case GameStateManagerGameState.InGame:
			//	case GameStateManagerGameState.StartingGame:
			//	case GameStateManagerGameState.Paused:
			//		break;
			//}
		}


		public static PlayerControls GetControlsForPlayer(Player player)
		{
			PlayerControls playerControls;
			AllControls.TryGetValue(player.Id, out playerControls);
			return playerControls;
		}
		public static PlayerControls GetPreviousControlsForPlayer(Player player)
		{
			PlayerControls playerControls;
			PreviousControls.TryGetValue(player.Id, out playerControls);
			return playerControls;
		}
		private void SetAiControls(Dictionary<Guid, PlayerControls> allControls)
		{
			if (PreviousControls != null)
			{
				foreach (Guid guid in allControls.Keys.ToList())
				{
					Player currentPlayer;
					if (Level.Players.TryGetValue(guid, out currentPlayer) && currentPlayer.IsAi)
					{
						allControls[guid] = _aiUtilityMap[guid].GetAiControls(PreviousControls[guid]);
					}
				}
			}
		}

		public bool TestIfPaused()
		{
			return PreviousControls != null && AllControls.Any(pcTuple => Controls.PressedDown(pcTuple.Value, PreviousControls[pcTuple.Key], AvailableButtons.Start));
		}

		public void EndGame()
		{
			_game.EndGame(_gameStatistics);
		}


		public void AddHighLight(GameHighlight gameHighlight)
		{
			_gameStatistics.AddHightlight(gameHighlight);
		}
	}
}
