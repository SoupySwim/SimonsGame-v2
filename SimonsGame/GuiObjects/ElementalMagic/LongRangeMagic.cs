using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects.ElementalMagic
{
	// First draft of Long Range Magic.
	// First draft will not include type of magic as that comes at a later sprint.
	public class LongRangeMagic : PhysicsObject
	{
		// the temp-est of textures...
		private Texture2D _fireball;
		private float radians = 0;

		public LongRangeMagic(Vector2 position, Vector2 hitbox, Group group, Level level, Vector2 speed)
			: base(position, hitbox, group, level)
		{
			MaxSpeedBase = speed;
			_fireball = level.Content.Load<Texture2D>("Test/Fireball");
		}
		public override float GetXMovement()
		{
			return MaxSpeed.X;
		}
		public override float GetYMovement()
		{
			return MaxSpeed.Y;
		}

		public override void AddCustomModifiers(GameTime gameTime, ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, ModifierBase modifyMult) { }
		public override void PreUpdate(GameTime gameTime) { }
		public override void PostUpdate(GameTime gameTime) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			spriteBatch.Begin();

			float scale = Size.Y / _fireball.Height;
			spriteBatch.Draw(_fireball, Position + (Size / 2), null, Color.White, radians, new Vector2(_fireball.Width / 2, _fireball.Height / 2), scale, SpriteEffects.None, 0);

			radians += (float)(Math.PI / 22.5f);
			spriteBatch.End();
		}
		public override void SetMovement(GameTime gameTime) { }
		public override void PostPhysicsPreUpdate(GameTime gameTime) { }
		protected override bool ShowHitBox()
		{
			return false;
		}
	}
}
