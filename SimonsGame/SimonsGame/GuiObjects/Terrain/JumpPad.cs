using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Modifiers.Abilities;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class JumpPad : AffectedSpace
	{
		private int _degrees = 0;
		public JumpPad(Vector2 position, Vector2 hitbox, Level level)
			: base(position, hitbox, level, "JumpPad")
		{
			_collisionModifier = new JumpPadAbility(this, 3, new Vector2(0, 1));
			_collisionModifier.StopGravity = true;
			HitBoxColor = Color.Green;
			_objectType = GuiObjectType.Structure;
		}
		protected override void HitObject(MainGuiObject mgo, Vector2 bounds)
		{
			if (mgo is PhysicsObject)
			{
				PhysicsObject pmgo = mgo as PhysicsObject;
				if (!pmgo.AbilityManager.CurrentAbilities.Any(kv => kv.Key == _collisionModifier.Id))
				{
					ModifierBase collision = _collisionModifier.Clone();
					collision.Owner = mgo;
					mgo.HitByObject(this, collision);
					var jumpAbilityId = pmgo.AbilityManager.CurrentAbilities.FirstOrDefault(kv => kv.Value is SingleJump).Key;
					if (jumpAbilityId != Guid.Empty) ((PhysicsObject)pmgo).AbilityManager.HasExpired(jumpAbilityId);
				}
			}
		}
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PreUpdate(GameTime gameTime) { base.PreUpdate(gameTime); }
		public override void ExtraSizeManipulation(Vector2 newSize) { base.ExtraSizeManipulation(newSize); }
		protected override bool ShowHitBox() { return true; }


		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Power";
			else if (bType == ButtonType.SpecialToggle2)
				return "Rotation";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return string.Format("{0:0.00}", ((JumpPadAbility)_collisionModifier).PowerBase);
			else if (bType == ButtonType.SpecialToggle2)
				return _degrees + " degrees";
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				JumpPadAbility ability = _collisionModifier as JumpPadAbility;
				ability.ModifyPower(MathHelper.Clamp(ability.PowerBase + (moveRight ? .25f : -.25f), 1, 10));
			}
			else if (bType == ButtonType.SpecialToggle2)
			{
				_degrees = (_degrees + (moveRight ? 15 : -15)) % 360;
				double radians = (Math.PI / 180) * _degrees;
				((JumpPadAbility)_collisionModifier).ModifyAim(new Vector2(-(float)Math.Sin(radians), (float)Math.Cos(radians)));
			}
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return (int)(((JumpPadAbility)_collisionModifier).PowerBase * 4);
			else if (bType == ButtonType.SpecialToggle2)
				return _degrees;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				((JumpPadAbility)_collisionModifier).ModifyPower(value / 4.0f);
			else if (bType == ButtonType.SpecialToggle2)
			{
				_degrees = value;
				double radians = (Math.PI / 180) * _degrees;
				((JumpPadAbility)_collisionModifier).ModifyAim(new Vector2(-(float)Math.Sin(radians), (float)Math.Cos(radians)));
			}
			base.SetSpecialValue(bType, value);
		}
		protected override SpriteEffects GetCurrentSpriteEffects()
		{
			// No, I'll have to rotate... duh!
			if (_degrees <= 90)
				return SpriteEffects.None;
			if (_degrees <= 180)
				return SpriteEffects.FlipVertically;
			if (_degrees <= 270)
				return SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
			return SpriteEffects.FlipHorizontally;
		}
		public override void SwitchDirections()
		{
			_degrees = -_degrees;
			double radians = (Math.PI / 180) * _degrees;
			((JumpPadAbility)_collisionModifier).ModifyAim(new Vector2(-(float)Math.Sin(radians), (float)Math.Cos(radians)));
		}
	}
}