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
		public MainMenuScreen(MenuStateManager manager)
		{
			_manager = manager;
		}
		public override void MoveBack()
		{
			if (timeSpentOnScreen.TotalMilliseconds >= 500)
				_manager.NavigateToPreviousScreen();
		}
		protected override void DrawExtra(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { } // Default to nothing unless otherwise specified.
	}
}
