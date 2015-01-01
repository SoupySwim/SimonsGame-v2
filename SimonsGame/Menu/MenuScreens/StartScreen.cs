using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Menu.MenuScreens
{
	public class StartScreen : MainMenuScreen
	{
		public StartScreen(MenuStateManager manager, Vector2 screenSize)
			: base(manager)
		{
			_menuLayout = new MenuItem[1][];
			_menuLayout[0] = new MenuItem[1];
			_screenSize = screenSize;
			_menuLayout[0][0] = new TextMenuItem(new Action(() => { _manager.NavigateToScreen(MenuStateManager.ScreenType.MainGameMenu); }), "Press Start",
				"Press Start".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2, _screenSize.Y / 2)), true);
		}

	}
}