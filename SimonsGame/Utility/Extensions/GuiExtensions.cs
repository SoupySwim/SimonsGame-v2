using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Extensions
{
	public static class GuiExtensions
	{
		public static Rectangle ToRectangle(this Vector4 vector)
		{
			return new Rectangle((int)vector.X, (int)vector.Y, (int)vector.W, (int)vector.Z);
		}

		public static Vector2 GetTextSize(this string text, SpriteFont font)
		{
			return font.MeasureString(text);
		}
		public static Vector4 GetTextBoundsByCenter(this string text, SpriteFont font, Vector2 centerOfText)
		{
			Vector2 textSize = text.GetTextSize(font);
			return new Vector4(centerOfText.X - textSize.X / 2, centerOfText.Y - textSize.Y / 2, textSize.Y, textSize.X);
		}
		public static Vector2 GetPosition(this Vector4 bounds)
		{
			return new Vector2(bounds.X, bounds.Y);
		}
		public static Vector2 GetSize(this Vector4 bounds)
		{
			return new Vector2(bounds.W, bounds.Z);
		}
	}
}
