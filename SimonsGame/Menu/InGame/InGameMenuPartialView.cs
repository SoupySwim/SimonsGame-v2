using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Menu.InGame
{
	public abstract class InGameMenuPartialView
	{
		public Vector4 Bounds { get; protected set; }
		public InGameMenuPartialView(Vector4 bounds)
		{
			Bounds = bounds;
		}
		public abstract void Update(GameTime gameTime, Vector2 newMousePosition);
		public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

		// Returns if moving this direction leaves the view.
		public abstract bool MoveLeft();
		public abstract bool MoveRight();
		public abstract bool MoveUp();
		public abstract bool MoveDown();
		public abstract void HasBeenHighlighted();
	}
}
