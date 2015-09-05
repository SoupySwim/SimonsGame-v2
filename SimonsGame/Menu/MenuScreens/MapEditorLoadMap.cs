using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Menu.MenuScreens
{
	public class MapEditorLoadMap : MainMenuScreen
	{
		private static int ItemMaxWidth = 800;
		private static int ItemHeight = 100;
		private int _itemsPerColumn = 1;
		private int _itemsPerRow = 1;
		private float _itemWidth;
		private MapLoaderTopPanel _topPanel;
		private Vector2 currentMousePosition = Vector2.Zero;
		//private List<MapEditorLevelItem> levels = new List<MapEditorLevelItem>();
		public MapEditorLoadMap(MenuStateManager manager, Vector2 screenSize)
			: base(manager, screenSize)
		{
			_topPanel = new MapLoaderTopPanel(new Vector4(0, 0, screenSize.Y / 10.0f, screenSize.X), this);
			// For now, we use a dummy menuitem so we can override everything else.
			_menuLayout = new MenuItemButton[1][];
			_menuLayout[0] = new MenuItemButton[1];
			_menuLayout[0][0] = new TextMenuItemButton(() => { /* Does nothing */ }, "", new Vector4(-10, -10, 0, 0), Color.Orange, Color.Red, Vector2.Zero, true);
			_itemsPerColumn = (int)Math.Ceiling(screenSize.X / ItemMaxWidth);
			_itemsPerRow = (int)Math.Ceiling((screenSize.Y - (_topPanel.Bounds.Z + 10) - 9) / (ItemHeight + 10));
			_itemWidth = screenSize.X / _itemsPerColumn;
		}
		public void RefreshLevels()
		{
			int curXndx = 0;
			int curYndx = 0;
			IEnumerable<LevelFileMetaData> levels = MapEditorIOManager.GetLevels(_itemsPerColumn * _itemsPerRow);
			int levelsCount = levels.Count();
			_menuLayout = new MenuItemButton[(int)Math.Ceiling(((double)levelsCount) / ((double)_itemsPerRow))][];
			_menuLayout[0] = new MenuItemButton[Math.Min(_itemsPerRow, levelsCount - (curXndx * _itemsPerRow))];
			float curX = 10;
			float curY = _topPanel.Bounds.Z + 10;
			//levels.Clear();
			foreach (LevelFileMetaData metaData in levels)
			{
				_menuLayout[curXndx][curYndx] = new MapEditorLevelItem(_manager, metaData, new Vector4(curX, curY, ItemHeight, _itemWidth - 20), Color.Black, Color.White, Vector2.Zero);

				curY += ItemHeight + 10;
				if (curYndx >= _itemsPerRow - 1) // if (curY + ItemHeight + 9 >= _screenSize.Y)
				{
					curXndx++;
					_menuLayout[curXndx] = new MenuItemButton[Math.Min(_itemsPerRow, levelsCount - (curXndx * _itemsPerRow))];
					curYndx = 0;
					curX += _itemWidth;
					curY = _topPanel.Bounds.Z + 10;
					if (curXndx >= _itemsPerColumn)
						break;
					else
						_menuLayout[curXndx] = new MenuItemButton[Math.Min(_itemsPerRow, levelsCount - (curXndx * _itemsPerRow))];
				}
				else
					curYndx++;
			}
		}
		public override void HandleMouseEvent(GameTime gameTime, Vector2 newMousePosition)
		{
			currentMousePosition = newMousePosition;
			if (newMousePosition.Y <= _topPanel.Bounds.Z)
				_topPanel.Update(gameTime, newMousePosition);
			else
				base.HandleMouseEvent(gameTime, newMousePosition);
		}
		protected override void DrawExtra(GameTime gameTime, SpriteBatch spriteBatch)
		{
			//levels.ForEach(l => l.Draw(gameTime, spriteBatch));
			_topPanel.Draw(gameTime, spriteBatch);
		}
		public override void PressEnter()
		{
			if (currentMousePosition.Y <= _topPanel.Bounds.Z)
				_topPanel.PressEnter(currentMousePosition);
			else
				base.PressEnter();
		}
	}
	public class MapEditorLevelItem : TextMenuItemButton
	{
		public LevelFileMetaData LevelFileMetaData { get; set; }
		public MapEditorLevelItem(MenuStateManager manager, LevelFileMetaData levelFileMetaData, Vector4 bounds, Color defaultColor, Color selectedColor, Vector2 padding)
			: base(() =>
			{
				Level levelToEdit = MapEditorIOManager.DeserializeLevelFromFile(levelFileMetaData.FullName, null);
				levelToEdit.Initialize();
				manager.AddLevelToLevelEditor(levelToEdit, levelFileMetaData);
				manager.NavigateToScreen(MenuStateManager.ScreenType.MapEditorEditMap);
			}, levelFileMetaData.FullName, bounds, defaultColor, selectedColor, padding, false)
		{
			LevelFileMetaData = levelFileMetaData;
		}
	}
}
