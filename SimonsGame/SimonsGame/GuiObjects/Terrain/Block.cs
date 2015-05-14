using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.GuiObjects
{
	public class Block : PhysicsObject
	{
		private Texture2D _background;
		public Block(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "Block")
		{
			AdditionalGroupChange(group, group);
			_background = MainGame.ContentManager.Load<Texture2D>("Test/Block");
			_objectType = GuiObjectType.Environment;
			_team = Team.None;
			IsMovable = true;
		}
		// It never moves by itself.
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return AverageSpeed.Y; }

		public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(_background, Bounds.ToRectangle(), _hitBoxColor);
		}
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		protected override void AdditionalGroupChange(Group oldgroup, Group newGroup)
		{
			newGroup = oldgroup == Group.Impassable ? Group.ImpassableIncludingMagic : Group.Impassable; // Should toggle between passable and impassable including magic.
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
			base.AdditionalGroupChange(oldgroup, newGroup);
		}
	}
}
