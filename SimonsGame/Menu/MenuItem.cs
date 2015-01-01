using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Menu
{
	public abstract class MenuItem
	{
		public bool IsHighLighted { get; set; }
		private Action _selectedAcion;
		public Action SelectedAcion { get { return _selectedAcion; } }
		public Vector4 Bounds { get; set; }
		public MenuItem(Action selectedAcion, Vector4 bounds)
		{
			_selectedAcion = selectedAcion;
			Bounds = bounds;
		}
		public abstract void HasBeenHighlighted();
		public abstract void HasBeenDeHighlighted();
		public abstract void Update(GameTime gameTime);
		public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
		public void CallAction()
		{
			_selectedAcion();
		}
	}
}
