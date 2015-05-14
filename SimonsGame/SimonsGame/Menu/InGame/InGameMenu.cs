using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.Menu.InGame;
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
			// So, the hover bounds must be width of 100, but we gotta make sure it meets with the
			// end of the all magic pane and doesn't extend past 50px from the end of the screen.
			Vector4 inGameStatusMenuBounds = new Vector4(bounds.X, bounds.Y, bounds.Z, bounds.W * .85f);
			float hoverX = bounds.X + bounds.W - 220;
			float hoverX2 = bounds.X + inGameStatusMenuBounds.W - 20;
			hoverX = hoverX > hoverX2 ? hoverX2 : hoverX;
			Vector4 allMagicHoverBounds = new Vector4(hoverX, bounds.Y, bounds.Z, 180);
			InGameStatusMenu statusMenu = new InGameStatusMenu(inGameStatusMenuBounds, _player, allMagicHoverBounds);
			_allScreens.Add(InGameScreenType.InGameMenuScreen, new MainInGameMenu(this, bounds, statusMenu));

			_currentMenuScreen = _allScreens[InGameScreenType.InGameMenuScreen];
			_mousePosition = Vector2.Zero;
			_manager = manager;
		}
		public void Update(GameTime gameTime, Vector2 newMousePosition)
		{
			HandleKeyboardEvent();
			_currentMenuScreen.HandleMouseEvent(gameTime, newMousePosition);
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
			else if (Controls.IsClickingLeftMouse()) // TODO don't hack to use the left bumper as a click...
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
			else if (Controls.PressedDirectionDown(playerControls, previousControls, Direction2D.Down))// playerControls.YMovement > .5)
			{
				_currentMenuScreen.DeselectCurrent();
				_currentMenuScreen.MoveDown();
				_currentMenuScreen.SelectCurrent();
			}
			else if (Controls.PressedDirectionDown(playerControls, previousControls, Direction2D.Up))//playerControls.YMovement < -.5)
			{
				_currentMenuScreen.DeselectCurrent();
				_currentMenuScreen.MoveUp();
				_currentMenuScreen.SelectCurrent();
			}
			else if (Controls.PressedDirectionDown(playerControls, previousControls, Direction2D.Left))//playerControls.XMovement < -.5)
			{
				_currentMenuScreen.DeselectCurrent();
				_currentMenuScreen.MoveLeft();
				_currentMenuScreen.SelectCurrent();
			}
			else if (Controls.PressedDirectionDown(playerControls, previousControls, Direction2D.Right))//playerControls.XMovement > .5)
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
			_player.Level.Initialize();
		}
		public void UnPauseGame()
		{
			_manager.TogglePause = true;
		}
	}
}
