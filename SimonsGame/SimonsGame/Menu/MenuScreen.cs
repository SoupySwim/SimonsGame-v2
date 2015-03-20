using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Menu
{
	public abstract class MenuScreen
	{
		protected MenuItemButton[][] _menuLayout;
		protected TimeSpan _timeSpentOnScreen = TimeSpan.Zero;
		protected static TimeSpan _DELAY_ON_CLICK = new TimeSpan(0, 0, 0, 0, 400);
		protected int X, Y;
		public MenuScreen()
		{
			X = 0;
			Y = 0;
		}
		public Vector2 Position { get; set; }
		public Vector2 Size { get; set; }
		public void MoveUp()
		{
			Y = Y == 0 ? _menuLayout.Count() - 1 : Y - 1; // wrap around
			X = _menuLayout[Y].Count() > X ? X : _menuLayout[Y].Count() - 1; // If there's a spot then use it, otherwise go to the end of row.
		}
		public void MoveDown()
		{
			Y = Y == _menuLayout.Count() - 1 ? 0 : Y + 1; // wrap around
			X = _menuLayout[Y].Count() > X ? X : _menuLayout[Y].Count() - 1; // If there's a spot then use it, otherwise go to the end of row.
		}
		public void MoveLeft()
		{
			X = X == 0 ? _menuLayout[Y].Count() - 1 : X - 1; // wrap around
		}
		public void MoveRight()
		{
			X = X == _menuLayout[Y].Count() - 1 ? 0 : X + 1; // wrap around
		}
		public abstract void MoveBack();

		public void DeselectCurrent()
		{
			_menuLayout[Y][X].HasBeenDeHighlighted();
		}
		public void SelectCurrent()
		{
			_menuLayout[Y][X].HasBeenHighlighted();
		}

		public virtual void PressEnter()
		{
			if (_timeSpentOnScreen >= _DELAY_ON_CLICK)
				_menuLayout[Y][X].CallAction();
		}
		public void PressAction()
		{
			PressEnter();
		}
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			foreach (MenuItemButton[] menuItems in _menuLayout)
			{
				foreach (MenuItemButton menuItem in menuItems)
				{
					menuItem.Draw(gameTime, spriteBatch);
				}
			}
			DrawExtra(gameTime, spriteBatch);
			_timeSpentOnScreen += gameTime.ElapsedGameTime; // TODO add an update function...
		}

		protected abstract void DrawExtra(GameTime gameTime, SpriteBatch spriteBatch);

		public virtual void HandleMouseEvent(GameTime gameTime, Vector2 newMousePosition)
		{
			if (_timeSpentOnScreen < _DELAY_ON_CLICK)
				return;
			for (int y = 0; y < _menuLayout.Count(); y++)
			{
				for (int x = 0; x < _menuLayout[y].Count(); x++)
				{
					MenuItemButton currentItem = _menuLayout[y][x];
					if (currentItem.TotalBounds.X < newMousePosition.X && currentItem.TotalBounds.X + currentItem.TotalBounds.W > newMousePosition.X &&
						currentItem.TotalBounds.Y < newMousePosition.Y && currentItem.TotalBounds.Y + currentItem.TotalBounds.Z > newMousePosition.Y)
					{
						DeselectCurrent();
						X = x;
						Y = y;
						SelectCurrent();
					}
				}
			}
		}

		public void BlurScreen()
		{
			_timeSpentOnScreen = TimeSpan.Zero;
		}
		// Possibly add a focus screen.

	}
}