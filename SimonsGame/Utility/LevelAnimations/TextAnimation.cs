using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Utility
{
	public class TextAnimation : LevelAnimation
	{
		public string Text { get; set; }
		public Color TextColor { get; set; }
		private int _tickTotal = 60;
		private int _tickCount = 0;
		private bool _animate;

		public TextAnimation(string text, Color textColor, Level level, Vector2 position, int tickTotal = 60, bool animate = true)
			: base(level, position)
		{
			_tickTotal = tickTotal;
			Text = text;
			TextColor = textColor;
			_animate = animate;
		}
		public override void Update(GameTime gameTime)
		{
			_tickCount++;
			if (_tickCount >= _tickTotal)
				Level.RemoveLevelAnimation(this);
			else if (_animate && _tickCount % 3 == 0)
				Position = new Vector2(Position.X, Position.Y - 1);
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.DrawString(MainGame.PlainFont, Text, Position, TextColor);
		}
	}
}
