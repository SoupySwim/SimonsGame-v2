using Microsoft.Xna.Framework;
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
		public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			_tickCount++;
			//spriteBatch.Begin();
			spriteBatch.DrawString(MainGame.PlainFont, Text, Position, TextColor);
			//spriteBatch.End();
			if (_tickCount >= _tickTotal)
			{
				Level.RemoveLevelAnimation(this);
			}
			else if (_animate && _tickCount % 3 == 0)
			{
				Position = new Vector2(Position.X, Position.Y - 1);
			}
		}
	}
}
