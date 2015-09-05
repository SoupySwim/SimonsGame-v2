using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Utility
{
	public class GlobalGuiObjects
	{

		public static void DrawRatioBar(GameTime gameTime, SpriteBatch spriteBatch, Vector4 boundryBounds, float current,
			float total, Color fillColor, float tickInterval, bool showRatioText = true, bool showTicks = false)
		{
			float ratio = current / total;
			spriteBatch.Draw(MainGame.SingleColor, (boundryBounds + new Vector4(-1, -1, 2, 2)).ToRectangle(), Color.Black);
			spriteBatch.Draw(MainGame.SingleColor, boundryBounds.ToRectangle(), Color.Lerp(fillColor, Color.White, .5f));
			Rectangle fillRect = boundryBounds.ToRectangle();
			fillRect.Width = (int)(fillRect.Width * ratio);
			spriteBatch.Draw(MainGame.SingleColor, fillRect, fillColor);

			if (showTicks)
			{
				int tickNumber = (int)Math.Floor(total / tickInterval) - 1;
				float tickWidth = boundryBounds.W / (tickNumber + 1);
				for (int ndx = 0; ndx < tickNumber; ndx++)
					spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)(boundryBounds.X + tickWidth + (tickWidth * ndx)), (int)boundryBounds.Y, 1, (int)boundryBounds.Z), Color.LightGray);
			}

			string fractionString = current + " / " + total;
			Vector4 fractionStringBounds = fractionString.GetTextBoundsByCenter(MainGame.PlainFont, boundryBounds.GetPosition() + boundryBounds.GetSize() / 2);
			if (showRatioText && boundryBounds.Z >= fractionStringBounds.Z)
				spriteBatch.DrawString(MainGame.PlainFont, fractionString, fractionStringBounds.GetPosition(), Color.Black);
		}
	}
}
