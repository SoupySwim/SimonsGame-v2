using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Menu
{
	public class MainMenuScreen : MenuScreen
	{
		protected Vector2 _screenSize;
		protected MenuStateManager _manager;
		public MainMenuScreen(MenuStateManager manager, Vector2 screenSize)
		{
			_manager = manager;
			_screenSize = screenSize;
		}
		public override void MoveBack()
		{
			if (_timeSpentOnScreen >= _DELAY_ON_CLICK)
				_manager.NavigateToPreviousScreen();
		}
		protected override void DrawExtra(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { } // Default to nothing unless otherwise specified.
	}
}
