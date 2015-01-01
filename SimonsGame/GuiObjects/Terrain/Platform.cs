using Microsoft.Xna.Framework;
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
		public Platform(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "Platform")
		{

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
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
	}
}
