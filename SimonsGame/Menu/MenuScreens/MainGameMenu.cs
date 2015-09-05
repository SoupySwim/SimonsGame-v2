using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MainFiles;
using SimonsGame.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Menu.MenuScreens
{
	public class MainGameMenu : MainMenuScreen
	{
		public MainGameMenu(MenuStateManager manager, Vector2 screenSize)
			: base(manager, screenSize)
		{
			// Menu Layout initialize
			// single , settings
			// multi  , quick match
			_menuLayout = new MenuItemButton[3][];
			_menuLayout[0] = new MenuItemButton[1];
			_menuLayout[1] = new MenuItemButton[2];
			_menuLayout[2] = new MenuItemButton[2];

			Texture2D cog = manager.Content.Load<Texture2D>("Test/Cog");
			Texture2D singlePlayerTR = manager.Content.Load<Texture2D>("Test/Menu/SinglePlayer");
			Texture2D QuickMatchTL = manager.Content.Load<Texture2D>("Test/Menu/QuickMatch");
			Texture2D MapEditorBR = manager.Content.Load<Texture2D>("Test/Menu/MapEditor");
			Texture2D MultiPlayerBL = manager.Content.Load<Texture2D>("Test/Menu/MultiPlayer");

			int buttonSize = 300;

			_menuLayout[0][0] = new ImageMenuItemButton(_manager.NavigateToGameSettings, cog, new Vector4(_screenSize.X - 50, 10, 40, 40), Color.Black, Color.White, false);

			_menuLayout[1][0] = new ImageMenuItemButton(() =>
			{
				GameSettings settings = new GameSettings()
				{
					AllowAIScreens = false,
					PauseStopsGame = true
				};
				settings.LevelFileMetaData = MapEditorIOManager.GetMetadataForLevel(settings.MapName); // get the default map for quick match for now.
				_manager.StartGame(settings);
			}, QuickMatchTL, new Vector4(_screenSize.X / 2 - buttonSize - 4, _screenSize.Y / 2 - buttonSize - 4, buttonSize, buttonSize), Color.Gray, Color.White, true);
			_menuLayout[1][1] = new ImageMenuItemButton(() => { _manager.NavigateToScreen(MenuStateManager.ScreenType.SinglePlayerMenu); }, singlePlayerTR, new Vector4(_screenSize.X / 2 + 4, _screenSize.Y / 2 - buttonSize - 4, buttonSize, buttonSize), Color.Gray, Color.White, false);

			_menuLayout[2][0] = new ImageMenuItemButton(() => { _manager.NavigateToScreen(MenuStateManager.ScreenType.MultiPlayerMenu); }, MultiPlayerBL, new Vector4(_screenSize.X / 2 - buttonSize - 4, _screenSize.Y / 2 + 4, buttonSize, buttonSize), Color.Gray, Color.White, false);
			_menuLayout[2][1] = new ImageMenuItemButton(() => { _manager.NavigateToScreen(MenuStateManager.ScreenType.MapEditorMenu); }, MapEditorBR, new Vector4(_screenSize.X / 2 + 4, _screenSize.Y / 2 + 4, buttonSize, buttonSize), Color.Gray, Color.White, false);
			Y = 1;
		}
	}
}