using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Menu;
using SimonsGame.Menu.MenuScreens;
using SimonsGame.Menu.InGame;
using SimonsGame.Story;

namespace SimonsGame.Utility
{
	public class PlayerViewport
	{
		private enum PlayerState
		{
			PlayGame, // If no menus are open, then we are in this state.
			InMenu, // If we are in the main in game menu, then we are in this state.
			//ShortcutMenu // If we activated the shortcut menu, then we are in this state.
			ShortcutMenu
		}

		#region Overlay
		private InGameOverlay _countdownOverlay;
		private InGameOverlay _pauseOverlay;
		private InGameMenu _menuOverlay;
		private InGameOverlay _wonOverlay;
		private TimeSpan _showWonOverlay = TimeSpan.Zero;
		private ShortcutMenu _shortcutMenu;
		private InGameMenu _startupOverlay;
		public StartupChoiceMenu StartupOverlay;
		#endregion

		private Player _player;
		public Player Player { get { return _player; } }

		private PlayerHUD _playerHUD;
		private Texture2D _backgroundColor; // TODO change to layers of images.

		private PlayerState _playerState;

		#region Boundries
		private Vector2 _cameraPosition;
		public Vector2 CameraPosition { get { return _cameraPosition; } }
		private Vector4 _viewportBounds;
		public Vector4 ViewportBounds { get { return _viewportBounds; } set { _viewportBounds = value; } }
		private Viewport _viewport;
		private Vector2 _levelSize;
		private float _scaledAmount;
		public float Scale { get { return _scaledAmount; } }
		#endregion

		public PlayerViewport(Player player, Vector4 viewportBounds, Vector2 levelSize, float scale, GameStateManager manager)
		{
			IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)Program.Game.Services.GetService(typeof(IGraphicsDeviceService));

			_player = player;
			_viewportBounds = viewportBounds;
			_cameraPosition = Vector2.Zero;
			_levelSize = levelSize;
			_playerHUD = new PlayerHUD(player, _viewportBounds);
			_scaledAmount = scale;
			_backgroundColor = new Texture2D(graphicsService.GraphicsDevice, 1, 1);
			_backgroundColor.SetData(new[] { Color.CornflowerBlue });
			_viewport = new Viewport(viewportBounds.ToRectangle());
			_countdownOverlay = new InGameOverlay("Ready?\r\n", new Vector4(viewportBounds.W / 3, viewportBounds.Z / 3, viewportBounds.Z / 3, viewportBounds.W / 3));
			_pauseOverlay = new InGameOverlay("Paused", new Vector4(viewportBounds.W / 3, viewportBounds.Z / 3, viewportBounds.Z / 3, viewportBounds.W / 3));
			Vector4 overlaySize = "You Won!".GetTextBoundsByCenter(MainGame.PlainFontLarge, new Vector2(viewportBounds.X + viewportBounds.W / 2, viewportBounds.Y + viewportBounds.Z / 2)) + new Vector4(-20, -20, 40, 40);
			overlaySize.Y = 10;
			_wonOverlay = new InGameOverlay("You Won!", overlaySize, MainGame.PlainFontLarge);
			_menuOverlay = new InGameMenu(_player, new Vector4(10, 10, viewportBounds.Z - 20, viewportBounds.W - 20), manager);


			// So, the hover bounds must be width of 100, but we gotta make sure it meets with the
			// end of the all magic pane and doesn't extend past 50px from the end of the screen.
			Vector4 inGameStatusMenuBounds = new Vector4(_menuOverlay.Bounds.X, _menuOverlay.Bounds.Y, _menuOverlay.Bounds.Z, _menuOverlay.Bounds.W * .85f);
			float hoverX = _menuOverlay.Bounds.X + _menuOverlay.Bounds.W - 220;
			float hoverX2 = _menuOverlay.Bounds.X + inGameStatusMenuBounds.W - 20;
			hoverX = hoverX > hoverX2 ? hoverX2 : hoverX;
			Vector4 allMagicHoverBounds = new Vector4(hoverX, _menuOverlay.Bounds.Y, _menuOverlay.Bounds.Z, 180);
			InGameStatusMenu statusMenu = new InGameStatusMenu(inGameStatusMenuBounds, _player, allMagicHoverBounds);
			_menuOverlay.SetupScreen(new MainInGameMenu(_menuOverlay, _menuOverlay.Bounds, statusMenu));

			StartupOverlay = new StartupChoiceMenu(player, new Vector4(10, 10, viewportBounds.Z - 20, viewportBounds.W - 20));
			_startupOverlay = new InGameMenu(_player, new Vector4(10, 10, viewportBounds.Z - 20, viewportBounds.W - 20), manager);
			_startupOverlay.SetupScreen(StartupOverlay);


			_shortcutMenu = new ShortcutMenu(_player, new Vector4(10, 10, viewportBounds.Z - 20, viewportBounds.W - 20));
			_startupOverlay.SetupScreen(StartupOverlay);

			_playerState = PlayerState.PlayGame;

		}

		public void SetCountdown(TimeSpan timeLeft)
		{
			_countdownOverlay.Text = string.Format("Ready?\r\n{0:0.0} seconds", timeLeft.TotalSeconds);
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Level level, GameStateManagerGameState gameState, StoryBoard storyBoard)
		{
			spriteBatch.GraphicsDevice.Viewport = _viewport;
			Matrix scaleMatrix = Matrix.CreateScale(_scaledAmount);

			spriteBatch.Begin();
			spriteBatch.Draw(MainGame.SingleColor, new Rectangle(0, 0, (int)_viewportBounds.W, (int)_viewportBounds.Z), Color.CornflowerBlue);
			spriteBatch.End();

			ScrollCamera(_viewportBounds / _scaledAmount);
			Matrix cameraTransform = Matrix.CreateTranslation(-_cameraPosition.X, -_cameraPosition.Y, 0.0f) * scaleMatrix;

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, cameraTransform);
			Vector2 keyboardPlayerMousePosition = level.DrawInViewport(gameTime, spriteBatch, _viewportBounds / _scaledAmount, _cameraPosition, _player);
			if (storyBoard != null)
				storyBoard.Draw(gameTime, spriteBatch);
			spriteBatch.End();

			spriteBatch.Begin();
			_playerHUD.Draw(gameTime, spriteBatch);

			if (gameState == GameStateManagerGameState.PreGame)
				_startupOverlay.Draw(gameTime, spriteBatch);
			else if (gameState == GameStateManagerGameState.StartingGame)
				_countdownOverlay.Draw(gameTime, spriteBatch);

			if (_playerState == PlayerState.InMenu)
				_menuOverlay.Draw(gameTime, spriteBatch);
			else if (_playerState == PlayerState.ShortcutMenu)
				_shortcutMenu.Draw(gameTime, spriteBatch);

			if (_showWonOverlay != TimeSpan.Zero)
			{
				_wonOverlay.Draw(gameTime, spriteBatch);
				_showWonOverlay -= gameTime.ElapsedGameTime;
				if (_showWonOverlay < TimeSpan.Zero)
					_showWonOverlay = TimeSpan.Zero;
			}

			// If the player is THE mouse and keyboard player, then show the cursor on the top of everything.
			if (_player != null && _player.UsesMouseAndKeyboard && keyboardPlayerMousePosition != Vector2.Zero)
				spriteBatch.Draw(MainGame.Cursor, keyboardPlayerMousePosition - _cameraPosition, Color.Red);

			spriteBatch.End();
		}

		public void Update(GameTime gameTime, GameStateManagerGameState gameState, Vector2 newMousePosition)
		{
			Vector2 mousePos = newMousePosition - new Vector2(10, 10);
			if (_playerState == PlayerState.InMenu)
				_menuOverlay.Update(gameTime, mousePos);

			if (gameState == GameStateManagerGameState.PreGame)
			{
				if (GameStateManager.GetPreviousControlsForPlayer(_player) != null)
					_startupOverlay.Update(gameTime, mousePos);
			}
			else if (gameState != GameStateManagerGameState.StartingGame)
			{
				PlayerControls controls = GameStateManager.GetControlsForPlayer(_player);
				PlayerControls prevControls = GameStateManager.GetControlsForPlayer(_player);
				bool toggleMenuState = Controls.PressedDown(_player.Id, AvailableButtons.Start | AvailableButtons.Start2);

				if (toggleMenuState && _playerState == PlayerState.InMenu)
				{
					_playerState = PlayerState.PlayGame;
					_player.NotAcceptingControls = false;
				}
				else if (toggleMenuState && _playerState == PlayerState.PlayGame)
				{
					_playerState = PlayerState.InMenu;
					_player.NotAcceptingControls = true;
					_menuOverlay.OpenMenu();
				}

				if (_playerState == PlayerState.PlayGame && Controls.AllControls[_player.Id].OpenShortcutMenu)
				{
					_playerState = PlayerState.ShortcutMenu;
					Vector2 centerOfScreen = (_cameraPosition * _scaledAmount) + (_viewportBounds.GetSize() / 2);
					Vector2 distanceFromMiddle = (_player.Center * _scaledAmount) - centerOfScreen;
					_shortcutMenu.OpenShortcutMenu(distanceFromMiddle);
				}

				if (_playerState == PlayerState.ShortcutMenu)
				{
					_player.NotAcceptingControls = true;
					if (!Controls.AllControls[_player.Id].OpenShortcutMenu)
					{
						_player.NotAcceptingControls = false;
						_playerState = PlayerState.PlayGame;
					}
					_shortcutMenu.Update(gameTime);
				}
			}
		}

		private void ScrollCamera(Vector4 viewportBounds)
		{
			const float ViewMargin = 0.4f;
			const float ViewMarginTop = 0.4f;
			const float ViewMarginBottom = 0.4f;

			// Calculate the edges of the screen.
			float marginWidth = viewportBounds.W * ViewMargin;
			float marginLeft = _cameraPosition.X + marginWidth;
			float marginRight = _cameraPosition.X + viewportBounds.W - marginWidth;
			float marginTop = _cameraPosition.Y + viewportBounds.Z * ViewMarginTop;
			float marginBottom = _cameraPosition.Y + viewportBounds.Z - (viewportBounds.Z * ViewMarginBottom);

			// Calculate how far to scroll when the player is near the edges of the screen.
			Vector2 cameraMovement = new Vector2(0.0f, 0.0f);
			if (_player.Position.X < marginLeft)
				cameraMovement.X = _player.Position.X - marginLeft;
			else if (_player.Position.X > marginRight)
				cameraMovement.X = _player.Position.X - marginRight;

			if (_player.Position.Y < marginTop) // player is above the top margin
				cameraMovement.Y = _player.Position.Y - marginTop;
			else if (_player.Position.Y > marginBottom)
				cameraMovement.Y = _player.Position.Y - marginBottom;


			// Update the camera position, but prevent scrolling off the ends of the level.
			Vector2 maxCameraPosition = new Vector2(_levelSize.X - viewportBounds.W, _levelSize.Y - viewportBounds.Z);
			_cameraPosition.X = MathHelper.Clamp(_cameraPosition.X + cameraMovement.X, 0.0f, maxCameraPosition.X);
			_cameraPosition.Y = MathHelper.Clamp(_cameraPosition.Y + cameraMovement.Y, 0.0f, maxCameraPosition.Y);
		}

		public void WonGame()
		{
			_showWonOverlay = new TimeSpan(0, 0, 15);
		}
	}
}
