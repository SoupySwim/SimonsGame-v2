using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Menu.InGame;

namespace SimonsGame.Menu.MenuScreens
{
	public class MainInGameMenu : InGameScreen
	{
		private Vector4 _overlayBounds;
		private InGameStatusMenu _inGameStatusMenu;
		public MainInGameMenu(InGameMenu manager, Vector4 overlayBounds, InGameStatusMenu inGameStatusMenu)
			: base(manager)
		{
			_overlayBounds = overlayBounds;

			_menuLayout = new MenuItemButton[3][];
			_menuLayout[0] = new MenuItemButton[2];
			_menuLayout[1] = new MenuItemButton[2];
			_menuLayout[2] = new MenuItemButton[2];
			_inGameStatusMenu = inGameStatusMenu;

			//Texture2D cog = manager.Content.Load<Texture2D>("Test/Cog");

			Vector2 size = "Resume".GetTextSize(MainGame.PlainFont);
			Vector4 textBounds = new Vector4(_overlayBounds.X + _overlayBounds.W - size.X - 30, _overlayBounds.Y + (size.Y / 2) + 25, size.Y, size.X);
			_menuLayout[0][0] = new TextMenuItemButton(() => { manager.UnPauseGame(); }, "Resume", textBounds, Color.Black, Color.White, new Vector2(40, 40), true);
			_menuLayout[0][1] = new InvisibleInGameOverlayMenuItem(_inGameStatusMenu.SelectedPane);

			size = "Restart".GetTextSize(MainGame.PlainFont);
			textBounds = new Vector4(_overlayBounds.X + _overlayBounds.W - size.X - 30, _overlayBounds.Y + (size.Y / 2) + textBounds.Y + textBounds.Z + 50, size.Y, size.X);
			_menuLayout[1][0] = new TextMenuItemButton(() => { manager.RestartGame(); }, "Restart", textBounds, Color.Black, Color.White, new Vector2(40, 40), false);
			_menuLayout[1][1] = new InvisibleInGameOverlayMenuItem(_inGameStatusMenu.AllMagicPane);

			size = "Quit Game".GetTextSize(MainGame.PlainFont);
			textBounds = new Vector4(_overlayBounds.X + _overlayBounds.W - size.X - 30, _overlayBounds.Y + (size.Y / 2) + textBounds.Y + textBounds.Z + 50, size.Y, size.X);
			_menuLayout[2][0] = new TextMenuItemButton(() => { manager.EndGame(); }, "Quit Game", textBounds, Color.Black, Color.White, new Vector2(40, 40), false);
			_menuLayout[2][1] = new InvisibleInGameOverlayMenuItem(_inGameStatusMenu.AllMagicPane);
		}
		public override void HandleMouseEvent(GameTime gameTime, Vector2 newMousePosition)
		{
			base.HandleMouseEvent(gameTime, newMousePosition);
			_inGameStatusMenu.Update(gameTime, newMousePosition);
		}

		protected override void DrawExtra(GameTime gameTime, SpriteBatch spriteBatch)
		{
			base.DrawExtra(gameTime, spriteBatch);
			_inGameStatusMenu.Draw(gameTime, spriteBatch);
		}
	}
}