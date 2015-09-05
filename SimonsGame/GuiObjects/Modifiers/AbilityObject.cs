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
	public class AbilityObject : AffectedSpace
	{
		protected Animation _availableAnimation; // Animation shown when a player can grab it.
		protected Func<PhysicsObject, PlayerAbilityInfo> _getAbility;

		public AbilityObject(Vector2 position, Vector2 size, Level level, Animation animation, Func<PhysicsObject, PlayerAbilityInfo> getAbility)
			: base(position, size, level, "AbilityObject")
		{
			Group = Group.Passable;
			_availableAnimation = animation;
			_animator.PlayAnimation(animation);
			//_getAbility = getAbility;
			_getAbility = AbilityBuilder.GetBlinkMiscAbility; // As a test.
		}
		protected override void HitObject(MainGuiObject mgo, Vector2 bounds)
		{
			PhysicsObject pmgo = mgo as PhysicsObject;
			if (pmgo != null)
			{
				PlayerAbilityInfo pai = _getAbility(pmgo);
				pmgo.AbilityManager.AddKnownAbility(pai.KnownAbility, pai);
				Level.RemoveGuiObject(this);
			}
		}

		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PreUpdate(GameTime gameTime) { base.PreUpdate(gameTime); }
		protected override bool ShowHitBox() { return false; }
		public override IEnumerable<Tuple<Vector2, MainGuiObject>> GetAffectedObjects()
		{
			return GetHitObjects(Level.Players.Values, Bounds); // ID won't be the same as this is not a player.
		}
		public override void ExtraSizeManipulation(ref Vector2 newSize)
		{
			_availableAnimation.Scale = new Vector2(Size.X / _availableAnimation.FrameWidth, Size.Y / _availableAnimation.FrameHeight);
			//= new Animation(MainGame.Content.Load<Texture2D>("Test/Turret"), 1, false, 300, 500, (Size.X / 300.0f));
			base.ExtraSizeManipulation(ref newSize);
		}
	}
}
