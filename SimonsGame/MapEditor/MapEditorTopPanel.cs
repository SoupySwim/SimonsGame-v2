using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.Menu;
using SimonsGame.Menu.MenuScreens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.MainFiles;
using SimonsGame.Utility;
using SimonsGame.GlobalGameSettings;

namespace SimonsGame.MapEditor
{
	public class MapEditorTopPanel
	{
		private MapEditorEditMap _mapEditorEditMap;
		private Vector4 _topPanelBounds;
		public Vector2 Size { get { return _topPanelBounds.GetSize(); } }
		public Vector4 Bounds { get { return _topPanelBounds; } }
		private TextMenuItemButton _toggleGridButton;

		private string _mapName;
		private Vector2 _namePosition;

		public MapEditorTopPanel(MapEditorEditMap mapEditorEditMap, Vector4 topPanelBounds, string mapName)
		{
			_mapEditorEditMap = mapEditorEditMap;
			_topPanelBounds = topPanelBounds;
			float topButtonWidths = (_topPanelBounds.W - 15) / 2;
			_mapName = mapName;

			string gridButtonName = "Grid";
			Vector4 gridButtonBounds = new Vector4(_topPanelBounds.X + _topPanelBounds.W - 60, 0, _topPanelBounds.Z, 60);
			Vector2 gridButtonpadding = gridButtonName.GetPaddingGivenBounds(MainGame.PlainFont, gridButtonBounds.GetSize());
			gridButtonBounds = new Vector4(gridButtonBounds.GetPosition(), gridButtonBounds.Z - (gridButtonpadding.Y * 2), gridButtonBounds.W);
			_toggleGridButton = new TextMenuItemButton(() =>
			{
				AllGameSettings.MenuEditor_ShowGrid = !AllGameSettings.MenuEditor_ShowGrid;
			}, gridButtonName, gridButtonBounds, gridButtonpadding, false);

			_namePosition = topPanelBounds.GetPosition() + topPanelBounds.GetSize() / 2 - _mapName.GetTextSize(MainGame.PlainFont) / 2;

		}
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.DrawString(MainGame.PlainFont, _mapName, _namePosition, Color.Black);
			_toggleGridButton.Draw(gameTime, spriteBatch);
		}
		public void Update(Vector2 mousePosition)
		{
			// Hard Coded Buttons
			if (mousePosition.IsInBounds(_toggleGridButton.TotalBounds))
				_toggleGridButton.HasBeenHighlighted();
			else
				_toggleGridButton.HasBeenDeHighlighted();
		}

		public void Click(Vector2 mousePosition, Level level)
		{
			// Hard Coded Buttons
			if (_toggleGridButton.IsHighLighted)
				_toggleGridButton.CallAction();
			else
				MirrorMap(level); // Has Clicked in the text area!  Time to start editing!!!
		}

		public void ClickRight(Vector2 mousePosition, Level level)
		{
			// Hard Coded Buttons
			if (!_toggleGridButton.IsHighLighted)
				UnMirrorMap(level);
		}


		// Making some assumptions here:
		//		1) Map will Mirror horizontally.
		//		2) Team1 => Team2 (There will be no team 2 on the map when mirrored)
		//		3) Team3 => Team4 (There will be no team 4 on the map when mirrored)
		//		4) Map will double in size.
		//		5) Anything in the middle will grow horizontally.
		private void MirrorMap(Level level)
		{
			level.Size = new Vector2(level.Size.X * 2, level.Size.Y);
			IEnumerable<MainGuiObject> mgos = level.GetAllGuiObjects().Concat(level.GetAllTeleporters().Cast<MainGuiObject>()).Concat(level.GetAllZones().Values);
			List<MainGuiObject> objectsToInitialize = new List<MainGuiObject>();
			foreach (MainGuiObject mgo in mgos.ToList())
			{
				MainGuiObject clone = (MainGuiObject)mgo.Clone();
				clone.Position = new Vector2(level.Size.X - clone.Position.X - clone.Size.X, clone.Position.Y);

				// If we should be merging here, then merge!
				if (mgo.Position.X + mgo.Size.X + 1 >= clone.Position.X)
					mgo.Size = new Vector2(mgo.Size.X * 2, mgo.Size.Y);
				else // Otherwise add the new one!
				{
					clone.SwitchDirections();
					if (mgo.Team == Team.Team1)
						clone.Team = Team.Team2;
					if (mgo.Team == Team.Team2)
						clone.Team = Team.Team1;
					if (mgo.Team == Team.Team3)
						clone.Team = Team.Team4;
					if (mgo.Team == Team.Team4)
						clone.Team = Team.Team3;
					level.AddGuiObject(clone);

					if (clone is GuiIfClause || clone is GuiThenClause)
					{
						objectsToInitialize.Add(clone);
					}

				}
			}
			foreach (MainGuiObject mgo in objectsToInitialize)
			{
				mgo.Initialize();
			}
			_mapEditorEditMap.HasMirroredMap();
		}
		public void UnMirrorMap(Level level)
		{
			level.Size = new Vector2(level.Size.X / 2.0f, level.Size.Y);
			IEnumerable<MainGuiObject> mgos = level.GetAllGuiObjects().Concat(level.GetAllTeleporters().Cast<MainGuiObject>()).Concat(level.GetAllZones().Values);
			foreach (var mgo in mgos.ToList())
			{
				if (mgo.Position.X > level.Size.X)
					level.RemoveGuiObject(mgo);
				else if (mgo.Position.X < level.Size.X && mgo.Position.X + mgo.Size.X > level.Size.X)
					mgo.Size = new Vector2(mgo.Size.X / 2, mgo.Size.Y);
			}
		}
	}
}
