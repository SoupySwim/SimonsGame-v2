using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Menu.MenuScreens
{
	public class MainInGameMenu : InGameScreen
	{
		private Vector4 _overlayBounds;
		public MainInGameMenu(InGameMenu manager, Vector4 overlayBounds)
			: base(manager)
		{
			_overlayBounds = overlayBounds;

			_menuLayout = new MenuItem[1][];
			_menuLayout[0] = new MenuItem[1];
			//_menuLayout[1] = new MenuItem[2];

			//Texture2D cog = manager.Content.Load<Texture2D>("Test/Cog");

			Vector2 size = "Quit Game".GetTextSize(MainGame.PlainFont);
			Vector4 textBounds = new Vector4(_overlayBounds.X + _overlayBounds.W - size.X - 5, _overlayBounds.Y + size.Y + 5, size.X, size.Y);
			_menuLayout[0][0] = new TextMenuItem(() => { manager.EndGame(); }, "Quit Game", textBounds, Color.Black, Color.White, true);

			//_menuLayout[1][0] = new TextMenuItem(() => { }, "Load Map",
			//	"Load Map".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 - 50, _screenSize.Y / 2)), Color.Black, Color.White, true);
			//_menuLayout[1][1] = new TextMenuItem(() => { }, "New Map",
			//	"Load Map".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 + 50, _screenSize.Y / 2)), Color.Black, Color.White, false);
			//Y = 1;
		}
	}
}