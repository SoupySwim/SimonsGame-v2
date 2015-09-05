using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.GuiObjects;
using SimonsGame.MapEditor;

namespace SimonsGame.Menu.MenuScreens
{
	public class MapEditorMenu : MainMenuScreen
	{
		public MapEditorMenu(MenuStateManager manager, Vector2 screenSize)
			: base(manager, screenSize)
		{
			_screenSize = screenSize;

			// Menu Layout initialize
			// Continue , Start
			// Challenge
			_menuLayout = new MenuItemButton[2][];
			_menuLayout[0] = new MenuItemButton[1];
			_menuLayout[1] = new MenuItemButton[2];

			Texture2D cog = manager.Content.Load<Texture2D>("Test/Cog");


			_menuLayout[0][0] = new ImageMenuItemButton(_manager.NavigateToGameSettings, cog, new Vector4(_screenSize.X - 50, 10, 40, 40), Color.Black, Color.White, false);

			_menuLayout[1][0] = new TextMenuItemButton(() =>
			{
				_manager.RefreshLevels();
				_manager.NavigateToScreen(MenuStateManager.ScreenType.MapEditorLoadMap);
				//Level level = MapEditorIOManager.DeserializeLevelFromFile("Test Map");
				//manager.AddLevelToLevelEditor(level);
				//_manager.NavigateToScreen(MenuStateManager.ScreenType.MapEditorEditMap);
			}, "Load Map",
				"Load Map".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 - 70, _screenSize.Y / 2)), Color.Black, Color.White, new Vector2(40, 40), true);
			_menuLayout[1][1] = new TextMenuItemButton(() => { _manager.NavigateToScreen(MenuStateManager.ScreenType.MapEditorAddMap); }, "New Map",
				"New Map".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2 + 70, _screenSize.Y / 2)), Color.Black, Color.White, new Vector2(40, 40), false);
			Y = 1;
		}
	}
}