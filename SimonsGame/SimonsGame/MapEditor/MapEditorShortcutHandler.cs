using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SimonsGame.GuiObjects;
using SimonsGame.Menu.MenuScreens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.MapEditor
{
	public class MapEditorShortcutHandler
	{
		private TimeSpan _buttonPressTimer = TimeSpan.Zero;
		public void Update(MapEditorEditMap manager, GameTime gameTime)
		{
			_buttonPressTimer = _buttonPressTimer - gameTime.ElapsedGameTime <= TimeSpan.Zero ? TimeSpan.Zero : _buttonPressTimer - gameTime.ElapsedGameTime;
			KeyboardState keyboardState = Keyboard.GetState();
			bool hasMoved = false;
			if (keyboardState.IsKeyDown(Keys.Delete))
				manager.RightPanel.TrashCurrentItem();

			if (keyboardState.IsKeyDown(Keys.Q))
				manager.SwitchState(MapEditorState.Select);
			else if (keyboardState.IsKeyDown(Keys.E))
				manager.SwitchState(MapEditorState.AddNew);

			if (keyboardState.IsKeyDown(Keys.A) && _buttonPressTimer == TimeSpan.Zero)
			{
				manager.RightPanel.MoveLeft();
				hasMoved = true;
			}
			if (keyboardState.IsKeyDown(Keys.D) && _buttonPressTimer == TimeSpan.Zero)
			{
				manager.RightPanel.MoveRight();
				hasMoved = true;
			}
			if (keyboardState.IsKeyDown(Keys.W) && _buttonPressTimer == TimeSpan.Zero)
			{
				manager.RightPanel.MoveUp();
				hasMoved = true;
			}
			if (keyboardState.IsKeyDown(Keys.S) && _buttonPressTimer == TimeSpan.Zero)
			{
				manager.RightPanel.MoveDown();
				hasMoved = true;
			}
			if (keyboardState.IsKeyDown(Keys.Tab) && _buttonPressTimer == TimeSpan.Zero)
			{
				manager.SelectItem(manager.LeftPanel.CycleSelectedItem(manager.Level));
				manager.SwitchState(MapEditorState.AddNew);
				hasMoved = true;
			}
			if (hasMoved)
				_buttonPressTimer = new TimeSpan(1500000);
		}
	}
}
