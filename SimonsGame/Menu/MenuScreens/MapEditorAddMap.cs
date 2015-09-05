using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.GuiObjects;

namespace SimonsGame.Menu.MenuScreens
{
	public class MapEditorAddMap : MainMenuScreen
	{
		private List<Tuple<string, Vector2>> _additionalText;
		private double _width = 18;
		private Vector2 _widthPosition;
		private double _height = 13;
		private Vector2 _heightPosition;
		public MapEditorAddMap(MenuStateManager manager, Vector2 screenSize)
			: base(manager, screenSize)
		{
			_additionalText = new List<Tuple<string, Vector2>>();

			string text = "Add New Map";
			Vector2 textPosition = text.GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2, 50)).GetPosition();
			_additionalText.Add(new Tuple<string, Vector2>(text, textPosition));

			string widthText = "Width = ";
			Vector2 widthTextSize = widthText.GetTextSize(MainGame.PlainFont);
			string heightText = "Height = ";
			Vector2 heightTextSize = heightText.GetTextSize(MainGame.PlainFont);

			float averageHeight = (widthTextSize.Y + heightTextSize.Y) / 2;
			Vector2 widthTextPosition = new Vector2(screenSize.X / 2 - widthTextSize.X / 2 - (averageHeight * 2), screenSize.Y / 2 - widthTextSize.Y - 5);
			_additionalText.Add(new Tuple<string, Vector2>(widthText, widthTextPosition));

			Vector2 heightTextPosition = new Vector2(screenSize.X / 2 - heightTextSize.X / 2 - (averageHeight * 2), screenSize.Y / 2 + 5);
			_additionalText.Add(new Tuple<string, Vector2>(heightText, heightTextPosition));


			//_additionalText.Add(new Tuple<string, Vector2>(text, ));


			// Menu Layout initialize
			// Continue , Start
			// Challenge
			_menuLayout = new MenuItemButton[3][];
			_menuLayout[0] = new MenuItemButton[2]; // Width
			_menuLayout[1] = new MenuItemButton[2]; // Height
			_menuLayout[2] = new MenuItemButton[1]; // add

			Texture2D rightArrow = manager.Content.Load<Texture2D>("Test/Menu/RightArrow");
			float widthArrowStartingPosition = widthTextPosition.X + widthTextSize.X + averageHeight + 10;
			float heightArrowStartingPosition = heightTextPosition.X + heightTextSize.X + averageHeight + 10;

			_widthPosition = new Vector2(widthArrowStartingPosition - averageHeight - 10, widthTextPosition.Y);
			_heightPosition = new Vector2(heightArrowStartingPosition - averageHeight - 10, heightTextPosition.Y);

			_menuLayout[0][0] = new ImageMenuItemButton(() => { _width--; }, rightArrow, new Vector4(heightArrowStartingPosition, widthTextPosition.Y, averageHeight, averageHeight), Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally);
			_menuLayout[0][1] = new ImageMenuItemButton(() => { _width++; }, rightArrow, new Vector4(heightArrowStartingPosition + averageHeight, widthTextPosition.Y, averageHeight, averageHeight), Color.Orange, Color.Red, false);
			_menuLayout[1][0] = new ImageMenuItemButton(() => { _height--; }, rightArrow, new Vector4(heightArrowStartingPosition, heightTextPosition.Y, averageHeight, averageHeight), Color.Orange, Color.Red, false, SpriteEffects.FlipHorizontally);
			_menuLayout[1][1] = new ImageMenuItemButton(() => { _height++; }, rightArrow, new Vector4(heightArrowStartingPosition + averageHeight, heightTextPosition.Y, averageHeight, averageHeight), Color.Orange, Color.Red, false);

			_menuLayout[2][0] = new TextMenuItemButton(() =>
			{
				Level level = new Level(new Vector2((float)(160.0 * _width), (float)(160.0 * _height)), null, 160);
				manager.AddLevelToLevelEditor(level, new MapEditor.LevelFileMetaData()
					{
						LastModifiedOn = DateTime.Now,
						LevelName = "Test Map",
						LevelRelativePath = "",
						LevelSize = level.Size,
						ScenarioType = MainFiles.ScenarioType.MultiPlayerChallenge,
						TeamCount = 0,
						WinCondition = MainFiles.WinCondition.DefeatAllEnemies
					});
				manager.NavigateToScreen(MenuStateManager.ScreenType.MapEditorEditMap);
			}, "Add Map",
				"Add Map".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2, _screenSize.Y - 100)), Color.Black, Color.White, new Vector2(40, 40), true);
			Y = 2;
		}
		protected override void DrawExtra(GameTime gameTime, SpriteBatch spriteBatch)
		{
			base.DrawExtra(gameTime, spriteBatch);
			_additionalText.ForEach(tup =>
				{
					spriteBatch.DrawString(MainGame.PlainFont, tup.Item1, tup.Item2, Color.Black);
				});
			spriteBatch.DrawString(MainGame.PlainFont, _width.ToString(), _widthPosition, Color.Black);
			spriteBatch.DrawString(MainGame.PlainFont, _height.ToString(), _heightPosition, Color.Black);
		}
	}
}
