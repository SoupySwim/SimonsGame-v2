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

			_menuLayout = new MenuItemButton[3][];
			_menuLayout[0] = new MenuItemButton[1];
			_menuLayout[1] = new MenuItemButton[1];
			_menuLayout[2] = new MenuItemButton[1];

			//Texture2D cog = manager.Content.Load<Texture2D>("Test/Cog");

			Vector2 size = "Resume".GetTextSize(MainGame.PlainFont);
			Vector4 textBounds = new Vector4(_overlayBounds.X + _overlayBounds.W - size.X - 30, _overlayBounds.Y + (size.Y / 2) + 25, size.Y, size.X);
			_menuLayout[0][0] = new TextMenuItemButton(() => { manager.UnPauseGame(); }, "Resume", textBounds, Color.Black, Color.White, new Vector2(40, 40), true);

			size = "Restart".GetTextSize(MainGame.PlainFont);
			textBounds = new Vector4(_overlayBounds.X + _overlayBounds.W - size.X - 30, _overlayBounds.Y + (size.Y / 2) + textBounds.Y + textBounds.Z + 50, size.Y, size.X);
			_menuLayout[1][0] = new TextMenuItemButton(() => { manager.RestartGame(); }, "Restart", textBounds, Color.Black, Color.White, new Vector2(40, 40), false);

			size = "Quit Game".GetTextSize(MainGame.PlainFont);
			textBounds = new Vector4(_overlayBounds.X + _overlayBounds.W - size.X - 30, _overlayBounds.Y + (size.Y / 2) + textBounds.Y + textBounds.Z + 50, size.Y, size.X);
			_menuLayout[2][0] = new TextMenuItemButton(() => { manager.EndGame(); }, "Quit Game", textBounds, Color.Black, Color.White, new Vector2(40, 40), false);
		}
	}
}