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
		protected MenuItem[][] _menuLayout;
		protected TimeSpan timeSpentOnScreen = TimeSpan.Zero;
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

		public void PressEnter()
		{
			_menuLayout[Y][X].CallAction();
		}
		public void PressAction()
		{
			PressEnter();
		}
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			foreach (MenuItem[] menuItems in _menuLayout)
			{
				foreach (MenuItem menuItem in menuItems)
				{
					menuItem.Draw(gameTime, spriteBatch);
				}
			}
			DrawExtra(gameTime, spriteBatch);
			timeSpentOnScreen += gameTime.ElapsedGameTime; // TODO add an update function...
		}

		protected abstract void DrawExtra(GameTime gameTime, SpriteBatch spriteBatch);

		public void HandleMouseEvent(Vector2 newMousePosition)
		{
			for (int y = 0; y < _menuLayout.Count(); y++)
			{
				for (int x = 0; x < _menuLayout[y].Count(); x++)
				{
					MenuItem currentItem = _menuLayout[y][x];
					if (currentItem.Bounds.X < newMousePosition.X && currentItem.Bounds.X + currentItem.Bounds.W > newMousePosition.X &&
						currentItem.Bounds.Y < newMousePosition.Y && currentItem.Bounds.Y + currentItem.Bounds.Z > newMousePosition.Y)
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
			timeSpentOnScreen = TimeSpan.Zero;
		}
		// Possibly add a focus screen.

	}
}