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
using SimonsGame.Menu.MenuScreens;
using SimonsGame.Test;
using SimonsGame.Story;
using SimonsGame.LevelMaker;

namespace SimonsGame
{
	public enum GameStateManagerGameState
	{
		InGame,
		Paused,
		StartingGame,
		PreGame
	}

	public class GameStateManager
	{

		#region DegubArea
		public static bool ShowHitBoxes { get; set; }
		public static TimeSpan GameTimer = TimeSpan.Zero;
		public static bool SlowMotionDebug = false;
		private static int _slowMotionCounter = 0;
		#endregion

		#region GameState
		private GameStateManagerGameState _gameState;

		private ScenarioType ScenarioType { get { return _gameSettings.LevelFileMetaData.ScenarioType; } }
		private bool _isMultiplayer = false;

		private bool _containsStory = false;
		private StoryBoard StoryBoard;


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
		public List<ExperienceGain> ExperienceGainIntervals { get; set; }
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
			_isMultiplayer = _gameSettings.LevelFileMetaData.ScenarioType == MainFiles.ScenarioType.MultiPlayerChallenge || _gameSettings.LevelFileMetaData.ScenarioType == MainFiles.ScenarioType.MultiPlayerCompetitive;

			_containsStory = _gameSettings.LevelFileMetaData.ScenarioType == MainFiles.ScenarioType.MultiPlayerChallenge
				|| _gameSettings.LevelFileMetaData.ScenarioType == MainFiles.ScenarioType.SinglePlayerStory
				|| _gameSettings.LevelFileMetaData.ScenarioType == MainFiles.ScenarioType.SinglePlayerChallenge;
			_containsStory = false;
			ExperienceGainIntervals = _gameSettings.ExperienceGainIntervals.ToList();

			//TODO move level to somewhere more meaningful...
			Level = gameSettings.IsRandomLevel ? RandomLevelMaker.MakeRandomLevel(this, gameSettings.LevelSettings, 1)
				: MapEditorIOManager.DeserializeLevelFromFile(gameSettings.MapName, this);// Test.LevelBuilder.BuildLevel3(MainGame.CurrentWindowSize + new Vector2(MainGame.CurrentWindowSize.X * .8f, MainGame.CurrentWindowSize.Y * .75f), this);

			_aiUtilityMap = Level.Players.Where(p => p.Value.IsAi).Select(p => new { key = p.Key, value = new AIUtility(p.Value) }).ToDictionary(p => p.key, p => p.value);

			if (_isMultiplayer && gameSettings.AllowAIScreens)
			{
				float viewportW = Level.Players.Count() > 2 ? MainGame.CurrentWindowSize.X / 2 - 10 : MainGame.CurrentWindowSize.X - 10;
				float viewportH = MainGame.CurrentWindowSize.Y / 2 - 10;
				Vector4[] viewportBounds = new Vector4[]
			{
				new Vector4(5, 5, viewportH, viewportW),
				Level.Players.Count() > 2 ?new Vector4(MainGame.CurrentWindowSize.X / 2 + 5, 5, viewportH, viewportW) : new Vector4( 5, MainGame.CurrentWindowSize.Y / 2 + 5, viewportH, viewportW),
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
				_playerViewports = new Dictionary<Guid, PlayerViewport>() { { player.Id, new PlayerViewport(player, new Vector4(0, 0, MainGame.CurrentWindowSize.Y, MainGame.CurrentWindowSize.X), Level.Size, .25f/*1.2945f*/, this) } };
			}

			_gameState = _gameSettings.LevelFileMetaData.ScenarioType == MainFiles.ScenarioType.MultiPlayerCompetitive ? GameStateManagerGameState.PreGame : GameStateManagerGameState.StartingGame;
			_countdownToStartGame = _countdownToStartGameMax;
			GameTimer = TimeSpan.Zero;

			TogglePause = false;
			_gameStatistics = new GameStatistics(Level);
			return true;
		}

		public void InitializeLevel()
		{
			Level.Initialize();

			if (_containsStory)
				StoryBoard = TempStory.GetTempStoryBoard(Level);
		}

		private void InitializePlayerChoices()
		{
			foreach (var kv in _playerViewports)
			{
				Player player = kv.Value.Player;
				StartupChoiceMenu startChoices = kv.Value.StartupOverlay;

				player.SelectPassiveExperienceGain(startChoices.SelectedExperienceGain);
				player.SelectSelfUpgrade(startChoices.SelectedSelfUpgrade);
				player.SelectBaseAttack(startChoices.SelectedBaseAttack);
			}
		}


		public void Update(GameTime gameTime, Vector2 newMousePosition)
		{
			if (SlowMotionDebug)
			{
				_slowMotionCounter++;
				if (_slowMotionCounter == 61)
					_slowMotionCounter = 0;
			}
			if (!SlowMotionDebug || _slowMotionCounter == 60) // Everything in game goes in here!
			{
				// Update Controls
				SetAiControls(Controls.PreviousControls);
				_mousePosition = newMousePosition;

				foreach (KeyValuePair<Guid, PlayerViewport> playerViewport in _playerViewports)
					playerViewport.Value.Update(gameTime, _gameState, newMousePosition);

				switch (_gameState)
				{
					case GameStateManagerGameState.InGame:
						GameTimer += gameTime.ElapsedGameTime;

						if (_containsStory)
						{
							StoryBoard.Update(gameTime);
							if (StoryBoard.IsStoryComplete()) // If the story is done, then let the players go for it!
								_containsStory = false;
						}

						ExperienceGain newInterval = null;
						foreach (ExperienceGain egInterval in ExperienceGainIntervals)
						{
							if (egInterval.StartTime < GameTimer)
								newInterval = egInterval;
						}
						if (newInterval != null)
						{
							ExperienceGainIntervals.Remove(newInterval);
							foreach (var kv in Level.Players)
								kv.Value.UpdatePassiveExperienceGain(newInterval);
						}

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
						}
						foreach (KeyValuePair<Guid, PlayerViewport> playerViewport in _playerViewports)
							playerViewport.Value.SetCountdown(_countdownToStartGame);

						break;
					case GameStateManagerGameState.PreGame:
						if (_playerViewports.Aggregate(true, (seed, kv) => seed && kv.Value.StartupOverlay.IsReady))
						{
							InitializePlayerChoices();
							_gameState = GameStateManagerGameState.StartingGame;
							foreach (var viewport in _playerViewports)
								viewport.Value.StartupOverlay.IsReady = false; // In case you restart the game...
						}
						break;
				}
			}
		}
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.GraphicsDevice.Viewport = _baseViewport;

			foreach (KeyValuePair<Guid, PlayerViewport> playerViewport in _playerViewports)
				playerViewport.Value.Draw(gameTime, spriteBatch, Level, _gameState, _containsStory ? StoryBoard : null);
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
			bool togglePuase = TogglePause || (Controls.PreviousControls != null && Controls.AllControls.Any(pcTuple => Controls.PressedDown(pcTuple.Value, Controls.PreviousControls[pcTuple.Key], AvailableButtons.Start | AvailableButtons.Start2)));
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
