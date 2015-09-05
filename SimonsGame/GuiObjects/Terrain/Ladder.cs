using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class Ladder : AffectedSpace
	{
		public Ladder(Vector2 position, Vector2 hitbox, Level level)
			: base(position, hitbox, level, "Ladder")
		{
			_collisionModifier = new TickModifier(1, ModifyType.Add, this, new Tuple<Element, float>(Element.Fire, .3f));
			_collisionModifier.StopGravity = true;
			HitBoxColor = Color.DarkOrange;
			_objectType = GuiObjectType.Structure;
			Group = Group.BothPassable;
		}
		protected override void HitObject(MainGuiObject mgo, Vector2 bounds)
		{
			ModifierBase collision = _collisionModifier.Clone();
			if (mgo.GetType().IsSubclassOf(typeof(PhysicsObject)))
			{
				PhysicsObject pmgo = mgo as PhysicsObject;
				if (pmgo.VerticalPass)
				{
					float movementAmount = 1.75f;
					collision.Movement = new Vector2(CurrentMovement.X, movementAmount);
					//pmgo.Position = new Vector2(pmgo.Position.X, pmgo.Position.Y + movementAmount);
				}
				pmgo.IsLanded = true;
				//collision.MaxSpeed = new Vector2(0, -mgo.MaxSpeed.Y * .25f);
			}
			mgo.HitByObject(this, collision);
		}
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }

		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PreUpdate(GameTime gameTime) { base.PreUpdate(gameTime); }
		protected override bool ShowHitBox() { return true; }
	}
}
