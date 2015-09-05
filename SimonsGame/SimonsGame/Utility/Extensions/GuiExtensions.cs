using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
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

		public static bool Contains(this Vector4 selectedSize, Vector4 panelBounds)
		{
			return selectedSize.X < panelBounds.X && selectedSize.X + selectedSize.W > panelBounds.X + panelBounds.W &&
				selectedSize.Y < panelBounds.Y && selectedSize.Y + selectedSize.Z > panelBounds.Y + panelBounds.Z;
		}

		public static float DistanceBetween(this MainGuiObject self, MainGuiObject target)
		{
			return (self.Center - target.Center).GetDistance();
		}
		public static float GetDistance(this Vector2 size)
		{
			return (float)Math.Sqrt(Math.Pow((double)size.X, 2) + Math.Pow((double)size.Y, 2));
		}
		public static bool IsWithinDistance(this MainGuiObject self, MainGuiObject target, int maxDistance)
		{
			Vector2 distance = target.Center - self.Center;
			return (float)Math.Sqrt(Math.Pow((double)distance.X, 2) + Math.Pow((double)distance.Y, 2)) <= maxDistance;
		}

		public static void DrawLine(this SpriteBatch sb, Vector2 start, Vector2 end, Color color, int width = 1)
		{
			//start.X = width;
			Vector2 edge = end - start;
			// calculate angle to rotate line
			float angle =
				 (float)Math.Atan2(edge.Y, edge.X);
			int xRotationOffset = (int)(width * 2 * (angle / Math.PI));
			sb.Draw(MainGame.SingleColor,
				 new Rectangle(// rectangle defines shape of line and position of start of line
					  (int)(start.X + xRotationOffset),
					  (int)start.Y,
					  (int)edge.Length() + xRotationOffset, //sb will strech the texture to fill this rectangle
					  (int)width), //width of line, change this to make thicker line
				 null,
				 color,
				 angle,     //angle of line (calulated above)
				 new Vector2(0, 0), // point in line about which to rotate
				 SpriteEffects.None,
				 0);

		}

		//public static void Screenie()
		//{
		//		int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
		//		int height = GraphicsDevice.PresentationParameters.BackBufferHeight;

		//		//Force a frame to be drawn (otherwise back buffer is empty) 
		//		Draw(new GameTime());

		//		//Pull the picture from the buffer 
		//		int[] backBuffer = new int[width * height];
		//		GraphicsDevice.GetBackBufferData(backBuffer);

		//		//Copy to texture
		//		Texture2D texture = new Texture2D(GraphicsDevice, width, height, false, GraphicsDevice.PresentationParameters.BackBufferFormat);
		//		texture.SetData(backBuffer);
		//		//Get a date for file name
		//		DateTime date = DateTime.Now; //Get the date for the file name
		//		Stream stream = File.Create(SCREENSHOT FOLDER + date.ToString("MM-dd-yy H;mm;ss") + ".png"); 

		//		//Save as PNG
		//		texture.SaveAsPng(stream, width, height);
		//		stream.Dispose();
		//		texture.Dispose();
		//}

		public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 position, Vector2 size, Color color, float scaleAmount)
		{
			spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y), color);
			spriteBatch.DrawRectangularBorder(position, size, color, scaleAmount);
		}
		public static void DrawRectangularBorder(this SpriteBatch spriteBatch, Vector2 position, Vector2 size, Color color, float scaleAmount)
		{
			int lineThickness = Math.Max(1, (int)((1.0f / scaleAmount) + .0005f));
			lineThickness = 3;
			Color darkColor = new Color(color.R, color.G, color.B, 1);
			Vector2 tempPosition;
			Vector2 botRightCorner = position + size;

			tempPosition = new Vector2(position.X + size.X, position.Y);
			spriteBatch.DrawLine(position, tempPosition, darkColor, lineThickness);
			spriteBatch.DrawLine(tempPosition, botRightCorner, darkColor, lineThickness);
			tempPosition.X = position.X;
			tempPosition.Y = position.Y + size.Y;
			spriteBatch.DrawLine(position, tempPosition, darkColor, lineThickness);
			spriteBatch.DrawLine(tempPosition, botRightCorner, darkColor, lineThickness);
		}


		public static ModifierBase GetKnockbackAbility(this MainGuiObject ability, MainGuiObject mgo, float amount)
		{
			Vector2 knockback = mgo.Center - ability.Center;
			float normalizer = (float)Math.Sqrt(Math.Pow((double)knockback.X, 2) + Math.Pow((double)knockback.Y, 2));
			knockback = (knockback / normalizer) * amount;

			ModifierBase knockBackAbility = new TickModifier(2, ModifyType.Add, mgo, new Tuple<Element, float>(Element.Normal, 0.0f));
			knockBackAbility.KnockBack = knockback;
			return knockBackAbility;
		}
	}
}
