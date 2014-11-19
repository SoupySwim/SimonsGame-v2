using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Utility
{
	public abstract class LevelAnimation
	{
		public Vector2 Position { get; set; }
		public Level Level { get; set; }

		public LevelAnimation(GuiObjects.Level level, Vector2 position)
		{
			this.Level = level;
			this.Position = position;
		}
		public abstract void Draw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch);

	}
}
