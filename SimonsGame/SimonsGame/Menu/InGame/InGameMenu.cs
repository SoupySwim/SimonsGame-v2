using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.Menu.MenuScreens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Menu
{
	// Very similar to MenuStateManager.
	public class InGameMenu : InGameOverlay
	{
		public enum InGameScreenType // perhaps redundant?
		{
			InGameMenuScreen
		}
		private GameStateManager _manager;
		private MenuScreen _currentMenuScreen;
		private Vector2 _mousePosition;
		private Dictionary<InGameScreenType, MenuScreen> _allScreens = new Dictionary<InGameScreenType, MenuScreen>();
		Stack<MenuScreen> PreviousScreens = new Stack<MenuScreen>();

		private Player _player;
		public InGameMenu(Player player, Vector4 bounds, GameStateManager manager)
			: base("", bounds)
		{
			_player = player;
			_allScreens.Add(InGameScreenType.InGameMenuScreen, new MainInGameMenu(this, bounds));

			_currentMenuScreen = _allScreens[InGameScreenType.InGameMenuScreen];
			_mousePosition = Vector2.Zero;
			_manager = manager;
		}
		public void Update(GameTime gameTime)
		{
			HandleKeyboardEvent();
			//_currentMenuScreen.HandleMouseEvent(newMousePosition);
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			base.Draw(gameTime, spriteBatch);
			_currentMenuScreen.Draw(gameTime, spriteBatch);
			//if (_player.UsesMouseAndKeyboard)
			//	spriteBatch.Draw(MainGame.Cursor, _mousePosition - new Vector2(10, 10), Color.Red);
		}
		public void HandleKeyboardEvent()
		{
			PlayerControls playerControls = GameStateManager.GetControlsForPlayer(_player);
			PlayerControls previousControls = GameStateManager.GetPreviousControlsForPlayer(_player);

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

		public void NavigateToPreviousScreen()
		{
			if (PreviousScreens.Any())
				_currentMenuScreen = PreviousScreens.Pop();
		}

		public void EndGame()
		{
			_manager.EndGame();
		}
		public void RestartGame()
		{
			_manager.RestartGame();
		}
		public void UnPauseGame()
		{
			_manager.TogglePause = true;
		}
	}
}
