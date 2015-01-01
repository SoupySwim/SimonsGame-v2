using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MainFiles;
using SimonsGame.MainFiles.InGame;
using SimonsGame.Menu.MenuScreens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Menu
{
	public class MenuStateManager
	{
		private Vector2 _screenSize;
		private MainGame _game;
		private ContentManager _content;
		public ContentManager Content { get { return _content; } }
		public static Dictionary<Guid, PlayerControls> AllControls { get; set; }
		public static Dictionary<Guid, PlayerControls> PreviousControls { get; set; }

		private MenuScreen _currentMenuScreen;
		private Vector2 _mousePosition;

		public enum ScreenType // perhaps redundant?
		{
			StartScreen,
			MainGameMenu,
			SinglePlayerMenu,
			MultiPlayerMenu,
			MapEditorMenu,
			GameStatistics
		}

		private Dictionary<ScreenType, MenuScreen> _allScreens = new Dictionary<ScreenType, MenuScreen>();
		Stack<MenuScreen> PreviousScreens = new Stack<MenuScreen>();

		public MenuStateManager(MainGame game, ContentManager content)
		{
			_game = game;
			_screenSize = MainGame.CurrentWindowSize;
			_content = content;

			// Initialize Menu Screens
			_allScreens.Add(ScreenType.StartScreen, new StartScreen(this, MainGame.CurrentWindowSize));
			_allScreens.Add(ScreenType.MainGameMenu, new MainGameMenu(this, MainGame.CurrentWindowSize));
			_allScreens.Add(ScreenType.SinglePlayerMenu, new SinglePlayerMenu(this, MainGame.CurrentWindowSize));
			_allScreens.Add(ScreenType.MultiPlayerMenu, new MultiPlayerMenu(this, MainGame.CurrentWindowSize));
			_allScreens.Add(ScreenType.MapEditorMenu, new MapEditorMenu(this, MainGame.CurrentWindowSize));
			_allScreens.Add(ScreenType.GameStatistics, new GameStatisticsMenu(this, MainGame.CurrentWindowSize));
			//MenuScreen mainGameMenuScreen = new MainGameMenu(this, _screenSize);

			_currentMenuScreen = _allScreens[ScreenType.StartScreen];

			_mousePosition = Vector2.Zero;
		}

		public void NavigateToScreen(ScreenType type)
		{
			BlurScreen();
			_currentMenuScreen = _allScreens[type];
		}
		public void NavigateToPreviousScreen()
		{
			if (PreviousScreens.Any())
				_currentMenuScreen = PreviousScreens.Pop();
		}
		public void Update(GameTime gameTime, Dictionary<Guid, PlayerControls> allControls, Vector2 newMousePosition)
		{
			PreviousControls = AllControls;
			AllControls = allControls;
			_mousePosition = newMousePosition;
			HandleKeyboardEvent();
			_currentMenuScreen.HandleMouseEvent(newMousePosition);
		}
		public void HandleKeyboardEvent()
		{
			foreach (KeyValuePair<Guid, PlayerControls> kv in AllControls)
			{
				PlayerControls playerControls = kv.Value;
				PlayerControls previousControls = PreviousControls == null ? new PlayerControls() : PreviousControls[kv.Key];

				if (Controls.PressedDown(playerControls, previousControls, AvailableButtons.Secondary))
				{
					_currentMenuScreen.DeselectCurrent();
					_currentMenuScreen.MoveBack();
					_currentMenuScreen.SelectCurrent();
				}
				else if (Controls.PressedDown(playerControls, previousControls, AvailableButtons.Start))
				{
					_currentMenuScreen.DeselectCurrent();
					_currentMenuScreen.PressEnter();
					_currentMenuScreen.SelectCurrent();
				}
				else if (Controls.PressedDown(playerControls, previousControls, AvailableButtons.Action))
				{
					_currentMenuScreen.DeselectCurrent();
					_currentMenuScreen.PressEnter();
					_currentMenuScreen.SelectCurrent();
				}
				else if (Controls.PressedDown(playerControls, previousControls, AvailableButtons.LeftBumper)) // TODO don't hack to use the left bumper as a click...
				{
					_currentMenuScreen.DeselectCurrent();
					_currentMenuScreen.PressEnter();
					_currentMenuScreen.SelectCurrent();
				}
				else if (Controls.PressedDown(playerControls, previousControls, AvailableButtons.RightBumper)) // TODO don't hack to use the right bumper as a click...
				{
					_currentMenuScreen.DeselectCurrent();
					_currentMenuScreen.MoveBack();
					_currentMenuScreen.SelectCurrent();
				}
				else if (Controls.PressedDirectionDown(playerControls, previousControls, Direction.Down))// playerControls.YMovement > .5)
				{
					_currentMenuScreen.DeselectCurrent();
					_currentMenuScreen.MoveDown();
					_currentMenuScreen.SelectCurrent();
				}
				else if (Controls.PressedDirectionDown(playerControls, previousControls, Direction.Up))//playerControls.YMovement < -.5)
				{
					_currentMenuScreen.DeselectCurrent();
					_currentMenuScreen.MoveUp();
					_currentMenuScreen.SelectCurrent();
				}
				else if (Controls.PressedDirectionDown(playerControls, previousControls, Direction.Left))//playerControls.XMovement < -.5)
				{
					_currentMenuScreen.DeselectCurrent();
					_currentMenuScreen.MoveLeft();
					_currentMenuScreen.SelectCurrent();
				}
				else if (Controls.PressedDirectionDown(playerControls, previousControls, Direction.Right))//playerControls.XMovement > .5)
				{
					_currentMenuScreen.DeselectCurrent();
					_currentMenuScreen.MoveRight();
					_currentMenuScreen.SelectCurrent();
				}
			}
		}
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Begin();
			_currentMenuScreen.Draw(gameTime, spriteBatch);
			spriteBatch.Draw(MainGame.Cursor, _mousePosition - new Vector2(10, 10), Color.Red);
			spriteBatch.End();
		}
		public void StartGame(GameSettings gameSettings)
		{
			BlurScreen();
			_game.StartGame(gameSettings);
		}
		public void BlurScreen()
		{
			_currentMenuScreen.BlurScreen();
			PreviousScreens.Push(_currentMenuScreen);
		}
		public void FocusScreen()
		{
		}
		public void NavigateToGameSettings()
		{
			// more to come!
		}

		public void ShowGameStatistics(GameStatistics gameStatistics)
		{
			_currentMenuScreen = _allScreens[ScreenType.GameStatistics];
			((GameStatisticsMenu)_currentMenuScreen).PopulateGameStatistics(gameStatistics);
		}
	}
}
