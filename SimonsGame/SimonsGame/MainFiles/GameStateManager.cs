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
using SimonsGame.MapEditor;
using SimonsGame.Menu;

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
		private static Dictionary<Guid, AIUtility> _aiUtilityMap { get; set; }

		public bool TogglePause { get; set; }
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

		#region CurrentGameData
		private GameStatistics _gameStatistics;
		private GameSettings _gameSettings;
		public WinCondition WinCondition { get { return _gameSettings.LevelFileMetaData.WinCondition; } }
		#endregion

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
		public bool StartNewGame(GameSettings gameSettings)
		{
			_gameSettings = gameSettings;
			//TODO move level to somewhere more meaningful...
			Level = MapEditorIOManager.DeserializeLevelFromFile(gameSettings.MapName, this);// Test.LevelBuilder.BuildLevel3(MainGame.CurrentWindowSize + new Vector2(MainGame.CurrentWindowSize.X * .8f, MainGame.CurrentWindowSize.Y * .75f), this);
			_aiUtilityMap = Level.Players.Where(p => p.Value.IsAi).Select(p => new { key = p.Key, value = new AIUtility(p.Value) }).ToDictionary(p => p.key, p => p.value);


			if (gameSettings.AllowAIScreens)
			{
				float viewportW = MainGame.CurrentWindowSize.X / 2 - 10;
				float viewportH = MainGame.CurrentWindowSize.Y / 2 - 10;
				Vector4[] viewportBounds = new Vector4[]
			{
				new Vector4(5, 5, viewportH, viewportW),
				new Vector4(MainGame.CurrentWindowSize.X / 2 + 5, 5, viewportH, viewportW),
				new Vector4(5, MainGame.CurrentWindowSize.Y / 2 + 5, viewportH, viewportW),
				new Vector4(MainGame.CurrentWindowSize.X / 2 + 5, MainGame.CurrentWindowSize.Y / 2 + 5, viewportH, viewportW)
			};
				var orderedPlayers = Level.Players.OrderBy(p => p.Value.IsAi);
				if (!orderedPlayers.Any())
					return false;
				_playerViewports = orderedPlayers./*Where(p => !p.Value.IsAi).*/Take(viewportBounds.Count()).Select((player, ndx) =>
				{
					/* TODO add logic to pick correct screen size... */
					return new
					{
						key = player.Key,
						value = new PlayerViewport(player.Value, viewportBounds[ndx], Level.Size, .7f, this)
					};
				}).ToDictionary(obj => obj.key, obj => obj.value);
			}
			else // Only show one human player
			{
				var player = Level.Players.Select(kv => kv.Value).Where(p => !p.IsAi).FirstOrDefault() ?? Level.Players.Select(kv => kv.Value).FirstOrDefault();
				if (player == null)
					return false;
				_playerViewports = new Dictionary<Guid, PlayerViewport>() { { player.Id, new PlayerViewport(player, new Vector4(0, 0, MainGame.CurrentWindowSize.Y, MainGame.CurrentWindowSize.X), Level.Size, .945f, this) } };
			}

			_gameState = GameStateManagerGameState.StartingGame;
			_countdownToStartGame = _countdownToStartGameMax;
			GameTimer = TimeSpan.Zero;
			Player.Sprint3TestScore = 0;

			TogglePause = false;
			_gameStatistics = new GameStatistics();
			return true;
		}
		public void Update(GameTime gameTime, Vector2 newMousePosition)
		{
			// Update Controls
			SetAiControls(Controls.PreviousControls);
			_mousePosition = newMousePosition;

			foreach (KeyValuePair<Guid, PlayerViewport> playerViewport in _playerViewports)
				playerViewport.Value.Update(gameTime, _gameState, newMousePosition);

			switch (_gameState)
			{
				case GameStateManagerGameState.InGame:
					if (GameTimerRunning)
						GameTimer += gameTime.ElapsedGameTime;
					else if (GameTimer == TimeSpan.Zero && Controls.PreviousControls.Values.First().XMovement > 0)
						GameTimerRunning = true;

					Level.Update(gameTime);
					// Test if Game is finished.

					// Test if paused.
					if (_gameSettings.PauseStopsGame && TestIfPaused())
						_gameState = GameStateManagerGameState.Paused;
					break;
				case GameStateManagerGameState.Paused:
					// Nothing to update yet... Probably will update some animation in the future.
					if (_gameSettings.PauseStopsGame && TestIfPaused())
						_gameState = GameStateManagerGameState.InGame;
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
			Controls.AllControls.TryGetValue(player.Id, out playerControls);
			return playerControls;
		}
		public static PlayerControls GetPreviousControlsForPlayer(Player player)
		{
			PlayerControls playerControls;
			Controls.PreviousControls.TryGetValue(player.Id, out playerControls);
			return playerControls;
		}
		private void SetAiControls(Dictionary<Guid, PlayerControls> allControls)
		{
			if (Controls.PreviousControls != null)
			{
				foreach (Guid guid in allControls.Keys.ToList())
				{
					Player currentPlayer;
					if (Level.Players.TryGetValue(guid, out currentPlayer) && currentPlayer.IsAi && Controls.PreviousControls.ContainsKey(guid))
					{
						allControls[guid] = _aiUtilityMap[guid].GetAiControls(Controls.PreviousControls[guid]);
					}
				}
			}
		}

		public bool TestIfPaused()
		{
			bool togglePuase = TogglePause || (Controls.PreviousControls != null && Controls.AllControls.Any(pcTuple => Controls.PressedDown(pcTuple.Value, Controls.PreviousControls[pcTuple.Key], AvailableButtons.Start)));
			TogglePause = false;
			return togglePuase;
		}

		public void EndGame()
		{
			_game.EndGame(_gameStatistics);
		}

		public void RestartGame()
		{
			StartNewGame(_gameSettings);
		}

		public void AddHighLight(GameHighlight gameHighlight)
		{
			_gameStatistics.AddHightlight(gameHighlight);
		}

		public void FinishedGame(MainGuiObject winner)
		{
			foreach (var viewport in _playerViewports.Where(pv => pv.Value.Player.Team == winner.Team))
				viewport.Value.WonGame();
		}
	}
}
