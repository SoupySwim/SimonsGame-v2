using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Menu
{
	public class InGameOverlay
	{
		public string Text;
		private Vector4 _bounds;
		private SpriteFont _usedFont;
		public InGameOverlay(string textToDisplay, Vector4 bounds, SpriteFont usedFont = null)
		{
			Text = textToDisplay;
			_bounds = bounds;
			_usedFont = usedFont == null ? MainGame.PlainFont : usedFont;
		}

		public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			Vector2 stringSize = _usedFont.MeasureString(Text);
			Vector2 stringPosition = new Vector2(_bounds.X + _bounds.W / 2 - stringSize.X / 2, _bounds.Y + _bounds.Z / 2 - stringSize.Y / 2);
			spriteBatch.Draw(MainGame.SingleColor, _bounds.ToRectangle(), new Color(.1f, .1f, .1f, .75f));
			if (Text != "")
				spriteBatch.DrawString(_usedFont, Text, stringPosition, Color.White);
		}
	}
}
