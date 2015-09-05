using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Utility
{
	public class LineAnimation : LevelAnimation
	{
		public Color LineColor { get; set; }
		private int _tickTotal = 60;
		private int _tickCount = 0;
		private Vector2 _endPosition;

		public LineAnimation(Level level, Vector2 position, Vector2 endPosition, int tickTotal = 60)
			: this(level, position, endPosition, Color.Black, tickTotal) { }

		public LineAnimation(Level level, Vector2 position, Vector2 endPosition, Color color, int tickTotal = 60)
			: base(level, position)
		{
			_tickTotal = tickTotal;
			LineColor = color;
			_endPosition = endPosition;
		}
		public override void Update(GameTime gameTime)
		{
			_tickCount++;
			if (_tickCount >= _tickTotal)
				Level.RemoveLevelAnimation(this);
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.DrawLine(Position, _endPosition, LineColor);
		}
	}
}
