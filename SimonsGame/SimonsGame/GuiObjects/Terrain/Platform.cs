using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class Platform : MainGuiObject
	{
		private Texture2D _background;
		public Platform(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "Platform")
		{
			AdditionalGroupChange(group, group);
			_background = MainGame.ContentManager.Load<Texture2D>("Test/Platform");
		}
		public override float GetXMovement()
		{
			return 0;
		}
		public override float GetYMovement()
		{
			return 0;
		}
		public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PreUpdate(GameTime gameTime) { }
		public override void PostUpdate(GameTime gameTime) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			float sizeX = Math.Abs(Size.X);
			float sizeY = Math.Abs(Size.Y);
			float projectedHeight = (int)(sizeY / _background.Height);
			int remainder = (int)(sizeY % _background.Height);
			if (remainder >= _background.Height / 2)
				projectedHeight++;
			projectedHeight = sizeY / projectedHeight;

			int repeatXCount = (int)(sizeX / projectedHeight);
			remainder = (int)(sizeX % projectedHeight);
			if (remainder >= projectedHeight / 2)
				repeatXCount++;
			float projectedWidth = sizeX / repeatXCount;


			int multX = Size.X < 0 ? -1 : 1;
			int multY = Size.Y < 0 ? -1 : 1;
			int addX = Size.X < 0 ? (int)-projectedWidth : 0;
			int addY = Size.Y < 0 ? (int)-projectedHeight : 0;

			for (float h = 0; h < sizeY - projectedHeight / 2; h += projectedHeight)
				for (int w = 0; w < repeatXCount; w++)
					spriteBatch.Draw(_background, new Rectangle((int)(Position.X + addX + multX * w * projectedWidth), (int)(Position.Y + addY + multY * h), (int)(projectedWidth), (int)(projectedHeight)), _hitBoxColor);
			//spriteBatch.Draw(_background, Position + new Vector2(w * projectedHeight, h * projectedHeight), _hitBoxColor);
		}
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		protected override void AdditionalGroupChange(Group _group, Group newGroup)
		{
			switch (newGroup)
			{
				case Group.ImpassableIncludingMagic:
					_hitBoxColor = Color.SandyBrown;
					break;
				case Group.Impassable:
					_hitBoxColor = Color.Khaki;
					break;
				default:
					_hitBoxColor = Color.Wheat;
					break;
			}
			base.AdditionalGroupChange(_group, newGroup);
		}
	}
}
