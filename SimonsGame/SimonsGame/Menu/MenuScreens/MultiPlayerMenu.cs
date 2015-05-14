using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.MainFiles;
using SimonsGame.MapEditor;

namespace SimonsGame.Menu.MenuScreens
{
	public class MultiPlayerMenu : MainMenuScreen
	{
		public MultiPlayerMenu(MenuStateManager manager, Vector2 screenSize)
			: base(manager, screenSize)
		{
			// Menu Layout initialize
			// Continue , Start
			// Challenge
			_menuLayout = new MenuItemButton[3][];
			_menuLayout[0] = new MenuItemButton[1];
			_menuLayout[1] = new MenuItemButton[2];
			_menuLayout[2] = new MenuItemButton[2];

			Texture2D cog = manager.Content.Load<Texture2D>("Test/Cog");


			_menuLayout[0][0] = new ImageMenuItemButton(_manager.NavigateToGameSettings, cog, new Vector4(_screenSize.X - 50, 10, 40, 40), Color.Black, Color.White, false);

			_menuLayout[1][0] = new TextMenuItemButton(() => { }, "Online",
				"Online".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 - 70, _screenSize.Y / 2 - 40)), Color.Black, Color.White, new Vector2(40, 40), true);

			_menuLayout[1][1] = new TextMenuItemButton(() => { }, "Co-op",
				"Co-op".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 + 70, _screenSize.Y / 2 - 40)), Color.Black, Color.White, new Vector2(60, 40), false);

			_menuLayout[2][0] = new TextMenuItemButton(() => { }, "Custom",
				"Custom".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 - 70, _screenSize.Y / 2 + 40)), Color.Black, Color.White, new Vector2(40, 40), false);

			_menuLayout[2][1] = new TextMenuItemButton(() =>
			{
				_manager.StartGame(new GameSettings()
					{
						AllowAIScreens = true,
						PauseStopsGame = false,
						MapName = "Sprint 3 Demo",
						LevelFileMetaData = MapEditorIOManager.GetMetadataForLevel("Sprint 3 Demo")
					});
			}, "Practice",
				"Practice".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 + 70, _screenSize.Y / 2 + 40)), Color.Black, Color.White, new Vector2(28, 40), false);
			Y = 1;
		}
	}
}