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
	public class SurroundRangeMagic : PhysicsObject
	{

		// the temp-est of textures...
		private Texture2D _bubble;

		private MainGuiObject _surroundObject;
		public SurroundRangeMagic(Vector2 position, Vector2 hitbox, Group group, Level level, MainGuiObject o)
			: base(position, hitbox, group, level)
		{
			_surroundObject = o;
			_bubble = level.Content.Load<Texture2D>("Test/Bubble");
		}
		public override float GetXMovement()
		{
			return 0;
		}
		public override float GetYMovement()
		{
			return 0;
		}

		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);
			Size = new Vector2(Size.X + 1.2f, Size.Y + 1.2f);
			Position = new Vector2(_surroundObject.Center.X - Size.X / 2, _surroundObject.Center.Y - Size.Y / 2);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			spriteBatch.Begin();
			Rectangle destinationRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

			spriteBatch.Draw(_bubble, destinationRect, Color.White);
			spriteBatch.End();
		}
		public override void SetMovement(GameTime gameTime) { }
		protected override bool ShowHitBox()
		{
			return false;
		}
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
	}
}
