using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Menu.MenuScreens
{
	public class MapEditorMenu : MainMenuScreen
	{
		public MapEditorMenu(MenuStateManager manager, Vector2 screenSize)
			: base(manager)
		{
			_screenSize = screenSize;

			// Menu Layout initialize
			// Continue , Start
			// Challenge
			_menuLayout = new MenuItem[2][];
			_menuLayout[0] = new MenuItem[1];
			_menuLayout[1] = new MenuItem[2];

			Texture2D cog = manager.Content.Load<Texture2D>("Test/Cog");


			_menuLayout[0][0] = new ImageMenuItem(_manager.NavigateToGameSettings, cog, new Vector4(_screenSize.X - 50, 10, 40, 40), Color.Black, Color.White, false);

			_menuLayout[1][0] = new TextMenuItem(() => { }, "Load Map",
				"Load Map".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 - 50, _screenSize.Y / 2)), Color.Black, Color.White, true);
			_menuLayout[1][1] = new TextMenuItem(() => { }, "New Map",
				"Load Map".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 + 50, _screenSize.Y / 2)), Color.Black, Color.White, false);
			Y = 1;
		}
	}
}