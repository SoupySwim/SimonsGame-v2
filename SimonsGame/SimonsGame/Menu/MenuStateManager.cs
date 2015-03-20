using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimonsGame.GuiObjects;
using SimonsGame.MainFiles;
using SimonsGame.MainFiles.InGame;
using SimonsGame.MapEditor;
using SimonsGame.Menu.MenuScreens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

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
		public static MouseState PreviousMouse;
		public static MouseState CurrentMouse;

		private MenuScreen _currentMenuScreen;
		private Vector2 _mousePosition;
		private bool _showMessage = false;
		private string _messageText = "";

		public enum ScreenType // perhaps redundant?
		{
			StartScreen,
			MainGameMenu,
			SinglePlayerMenu,
			MultiPlayerMenu,
			MapEditorMenu,
			GameStatistics,
			MapEditorAddMap,
			MapEditorEditMap,
			MapEditorLoadMap
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
			_allScreens.Add(ScreenType.MapEditorAddMap, new MapEditorAddMap(this, MainGame.CurrentWindowSize));
			_allScreens.Add(ScreenType.MapEditorEditMap, new MapEditorEditMap(this, MainGame.CurrentWindowSize));
			_allScreens.Add(ScreenType.MapEditorLoadMap, new MapEditorLoadMap(this, MainGame.CurrentWindowSize));
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
			PreviousMouse = CurrentMouse;
			CurrentMouse = Mouse.GetState();
			PreviousControls = AllControls;
			AllControls = allControls;
			_mousePosition = newMousePosition;
			HandleKeyboardEvent();
			_currentMenuScreen.HandleMouseEvent(gameTime, newMousePosition);
		}
		public void HandleKeyboardEvent()
		{
			if (IsClickingLeftMouse()) // TODO don't hack to use the left bumper as a click...
			{
				if (_showMessage)
					_showMessage = false;
				else
				{
					_currentMenuScreen.DeselectCurrent();
					_currentMenuScreen.PressEnter();
					_currentMenuScreen.SelectCurrent();
				}
			}
			else if (IsClickingRightMouse()) // TODO don't hack to use the right bumper as a click...
			{
				_currentMenuScreen.DeselectCurrent();
				_currentMenuScreen.MoveBack();
				_currentMenuScreen.SelectCurrent();
			}
			else
			{
				foreach (KeyValuePair<Guid, PlayerControls> kv in AllControls)
				{
					PlayerControls playerControls = kv.Value;
					PlayerControls previousControls = (PreviousControls == null || !PreviousControls.ContainsKey(kv.Key)) ? new PlayerControls() : PreviousControls[kv.Key];

					if (Controls.PressedDown(playerControls, previousControls, AvailableButtons.Secondary))
					{
						if (_showMessage)
							_showMessage = false;
						else
						{
							_currentMenuScreen.DeselectCurrent();
							_currentMenuScreen.MoveBack();
							_currentMenuScreen.SelectCurrent();
						}
					}
					else if (Controls.PressedDown(playerControls, previousControls, AvailableButtons.Start))
					{
						if (_showMessage)
							_showMessage = false;
						else
						{
							_currentMenuScreen.DeselectCurrent();
							_currentMenuScreen.PressEnter();
							_currentMenuScreen.SelectCurrent();
						}
					}
					else if (Controls.PressedDown(playerControls, previousControls, AvailableButtons.Action))
					{
						if (_showMessage)
							_showMessage = false;
						else
						{
							_currentMenuScreen.DeselectCurrent();
							_currentMenuScreen.PressEnter();
							_currentMenuScreen.SelectCurrent();
						}
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
		}

		// TODO don't hack to use the left/right bumper as a click...
		public static bool IsClickingLeftMouse()
		{
			return PreviousMouse != null &&
				PreviousMouse.LeftButton == ButtonState.Released &&
				CurrentMouse.LeftButton == ButtonState.Pressed;
		}
		public static bool IsReleasingLeftMouse()
		{
			//return PreviousControls != null && Controls.ReleasedDown(AllControls.First().Value, PreviousControls.First().Value, AvailableButtons.LeftBumper);
			return PreviousMouse != null &&
				PreviousMouse.LeftButton == ButtonState.Pressed &&
				CurrentMouse.LeftButton == ButtonState.Released;
		}
		public static bool IsHoldingLeftMouse()
		{
			//return PreviousControls != null && Controls.PressingDown(AllControls.First().Value, PreviousControls.First().Value, AvailableButtons.LeftBumper);
			return PreviousMouse != null &&
				PreviousMouse.LeftButton == ButtonState.Pressed &&
				CurrentMouse.LeftButton == ButtonState.Pressed;
		}
		public static bool IsClickingRightMouse()
		{
			//return PreviousControls != null && Controls.PressedDown(AllControls.First().Value, PreviousControls.First().Value, AvailableButtons.RightBumper);
			return PreviousMouse != null &&
				PreviousMouse.RightButton == ButtonState.Released &&
				CurrentMouse.RightButton == ButtonState.Pressed;
		}
		public static bool IsReleasingRightMouse()
		{
			//return PreviousControls != null && Controls.ReleasedDown(AllControls.First().Value, PreviousControls.First().Value, AvailableButtons.RightBumper);
			return PreviousMouse != null &&
				PreviousMouse.RightButton == ButtonState.Pressed &&
				CurrentMouse.RightButton == ButtonState.Released;
		}
		public static bool IsHoldingRightMouse()
		{
			//return PreviousControls != null && Controls.PressingDown(AllControls.First().Value, PreviousControls.First().Value, AvailableButtons.RightBumper);
			return PreviousMouse != null &&
				PreviousMouse.RightButton == ButtonState.Pressed &&
				CurrentMouse.RightButton == ButtonState.Pressed;
		}
		public static bool IsClickingMiddleMouse()
		{
			//return PreviousControls != null && Controls.PressedDown(AllControls.First().Value, PreviousControls.First().Value, AvailableButtons.RightBumper);
			return PreviousMouse != null &&
				PreviousMouse.MiddleButton == ButtonState.Released &&
				CurrentMouse.MiddleButton == ButtonState.Pressed;
		}
		public static bool IsReleasingMiddleMouse()
		{
			//return PreviousControls != null && Controls.ReleasedDown(AllControls.First().Value, PreviousControls.First().Value, AvailableButtons.RightBumper);
			return PreviousMouse != null &&
				PreviousMouse.MiddleButton == ButtonState.Pressed &&
				CurrentMouse.MiddleButton == ButtonState.Released;
		}
		public static bool IsHoldingMiddleMouse()
		{
			//return PreviousControls != null && Controls.PressingDown(AllControls.First().Value, PreviousControls.First().Value, AvailableButtons.RightBumper);
			return PreviousMouse != null &&
				PreviousMouse.MiddleButton == ButtonState.Pressed &&
				CurrentMouse.MiddleButton == ButtonState.Pressed;
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Begin();
			_currentMenuScreen.Draw(gameTime, spriteBatch);
			if (_showMessage)
			{
				spriteBatch.Draw(MainGame.SingleColor, new Rectangle(0, 0, (int)MainGame.CurrentWindowSize.X, (int)MainGame.CurrentWindowSize.Y), new Color(0, 0, 0, 0.4f));
				Vector4 textBounds = _messageText.GetTextBoundsByCenter(MainGame.PlainFontLarge, MainGame.CurrentWindowSize / 2);
				spriteBatch.Draw(MainGame.SingleColor, (textBounds + new Vector4(-30, -20, 40, 60)).ToRectangle(), new Color(0, 0, 0, .95f));
				spriteBatch.DrawString(MainGame.PlainFontLarge, _messageText, textBounds.GetPosition(), Color.White);
			}
			spriteBatch.Draw(MainGame.Cursor, _mousePosition - new Vector2(10, 10), Color.Red);
			spriteBatch.End();
		}
		public bool StartGame(GameSettings gameSettings)
		{
			bool didLoadSuccessfully = _game.StartGame(gameSettings);
			if (didLoadSuccessfully)
				BlurScreen();
			else
				ShowMessage("Level does not have a player.");
			return didLoadSuccessfully;
		}
		public void AddLevelToLevelEditor(Level level, LevelFileMetaData levelMetaData)
		{
			((MapEditorEditMap)_allScreens[ScreenType.MapEditorEditMap]).AddLevel(level, levelMetaData);
		}
		public void BlurScreen()
		{
			_currentMenuScreen.BlurScreen();
			PreviousScreens.Push(_currentMenuScreen);
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

		public void RefreshLevels()
		{
			((MapEditorLoadMap)_allScreens[ScreenType.MapEditorLoadMap]).RefreshLevels();
		}
		public void ShowMessage(string text)
		{
			_showMessage = true;
			_messageText = text;
		}
	}
}
