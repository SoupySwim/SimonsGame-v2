using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Menu
{
	public abstract class MenuItemButton
	{
		public bool IsHighLighted { get; set; }
		private Action _selectedAcion;
		public Action SelectedAcion { get { return _selectedAcion; } }
		public Vector4 Bounds { get; set; }
		public Vector4 TotalBounds { get { return Bounds + new Vector4(-Padding.X / 2, -Padding.Y / 2, Padding.Y, Padding.X); } }
		public Vector2 Padding { get; set; }
		public MenuItemButton(Action selectedAcion, Vector4 bounds) : this(selectedAcion, bounds, Vector2.Zero) { }
		public MenuItemButton(Action selectedAcion, Vector4 bounds, Vector2 padding)
		{
			_selectedAcion = selectedAcion;
			Bounds = bounds;
			Padding = padding;
		}
		public abstract void HasBeenHighlighted();
		public abstract void HasBeenDeHighlighted();
		public abstract void Update(GameTime gameTime);
		public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
		public void CallAction()
		{
			_selectedAcion();
		}

		public virtual bool IsStuck(Direction2D direction) { return false; }
	}
}
