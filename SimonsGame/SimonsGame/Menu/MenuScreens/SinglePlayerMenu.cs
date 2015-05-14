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
	public class SinglePlayerMenu : MainMenuScreen
	{
		public SinglePlayerMenu(MenuStateManager manager, Vector2 screenSize)
			: base(manager, screenSize)
		{
			_screenSize = screenSize;

			// Menu Layout initialize
			// Continue , Start
			// Challenge
			_menuLayout = new MenuItemButton[3][];
			_menuLayout[0] = new MenuItemButton[1];
			_menuLayout[1] = new MenuItemButton[2];
			_menuLayout[2] = new MenuItemButton[1];

			Texture2D cog = manager.Content.Load<Texture2D>("Test/Cog");


			_menuLayout[0][0] = new ImageMenuItemButton(_manager.NavigateToGameSettings, cog, new Vector4(_screenSize.X - 50, 10, 40, 40), Color.Black, Color.White, false);

			_menuLayout[1][0] = new TextMenuItemButton(() => { }, "Continue Story",
				"Continue Story".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 - 90, _screenSize.Y / 2 - 40)), Color.Black, Color.White, new Vector2(40, 40), true);
			_menuLayout[1][1] = new TextMenuItemButton(() => { }, "New Story",
				"New Story".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 + 90, _screenSize.Y / 2 - 40)), Color.Black, Color.White, new Vector2(40, 40), false);
			_menuLayout[2][0] = new TextMenuItemButton(() =>
			{
				_manager.StartGame(new GameSettings()
				{
					AllowAIScreens = false,
					PauseStopsGame = true,
					MapName = "Mario1-1",
					LevelFileMetaData = MapEditorIOManager.GetMetadataForLevel("Mario1-1")
				});
			}, "Challenge Mode",
				"Challenge Mode".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2, _screenSize.Y / 2 + 40)), Color.Black, Color.White, new Vector2(40, 40), false);
			Y = 1;
		}
	}
}