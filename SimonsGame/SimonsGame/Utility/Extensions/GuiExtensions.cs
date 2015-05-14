﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.MapEditor;
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
			return text.GetTextBoundsByCenter(font, centerOfText, Vector2.Zero);
		}
		public static Vector4 GetTextBoundsByCenter(this string text, SpriteFont font, Vector2 centerOfText, Vector2 padding)
		{
			Vector2 textSize = text.GetTextSize(font);
			return new Vector4(centerOfText.X - textSize.X / 2 - padding.X / 2, centerOfText.Y - textSize.Y / 2 - padding.Y / 2, textSize.Y + padding.Y, textSize.X + padding.X);
		}
		public static Vector2 GetPaddingGivenBounds(this string text, SpriteFont font, Vector2 totalSize)
		{
			Vector2 padding = totalSize - text.GetTextSize(font);
			return padding;
		}
		public static Tuple<Vector4, Vector2> GetSizeAndPadding(this string text, SpriteFont font, Vector4 totalBounds)
		{
			Vector2 paddingBounds = totalBounds.GetSize() - text.GetTextSize(font);
			Vector2 buttonSize = totalBounds.GetSize() - paddingBounds;
			Vector4 buttonBounds = new Vector4(totalBounds.GetPosition() + (paddingBounds / 2), buttonSize.Y, buttonSize.X);
			return new Tuple<Vector4, Vector2>(buttonBounds, paddingBounds);
		}
		public static Vector2 GetPosition(this Vector4 bounds)
		{
			return new Vector2(bounds.X, bounds.Y);
		}
		public static Vector2 GetSize(this Vector4 bounds)
		{
			return new Vector2(bounds.W, bounds.Z);
		}
		public static Color ToColor(this Vector4 v4)
		{
			return new Color(GetColorValue(v4.X),
							GetColorValue(v4.Y),
							GetColorValue(v4.Z),
							v4.W); // Not sure...
		}
		public static Color ToColor(this Vector3 v3)
		{
			return new Color(GetColorValue(v3.X),
				GetColorValue(v3.Y),
				GetColorValue(v3.Z));
		}
		private static float GetColorValue(float rgb)
		{
			if (rgb < 0)
				return rgb + 1.0f;
			if (rgb > 1.0f)
				return rgb - 1.0f;
			return rgb;
		}
		public static bool IsInBounds(this Vector2 mousePosition, Vector4 panelBounds)
		{
			return mousePosition.X > panelBounds.X && mousePosition.X < panelBounds.X + panelBounds.W &&
				mousePosition.Y > panelBounds.Y && mousePosition.Y < panelBounds.Y + panelBounds.Z;
		}
		public static void DrawLine(this SpriteBatch sb, Vector2 start, Vector2 end, Color color, int width = 1)
		{
			start.X = width;
			Vector2 edge = end - start;
			// calculate angle to rotate line
			float angle =
				 (float)Math.Atan2(edge.Y, edge.X);
			sb.Draw(MainGame.SingleColor,
				 new Rectangle(// rectangle defines shape of line and position of start of line
					  (int)start.X / 2,
					  (int)start.Y,
					  (int)edge.Length(), //sb will strech the texture to fill this rectangle
					  width), //width of line, change this to make thicker line
				 null,
				 color,
				 angle,     //angle of line (calulated above)
				 new Vector2(0, 0), // point in line about which to rotate
				 SpriteEffects.None,
				 0);

		}
	}
}