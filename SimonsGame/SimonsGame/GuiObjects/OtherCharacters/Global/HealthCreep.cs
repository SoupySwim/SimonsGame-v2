using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SimonsGame.Utility;
using SimonsGame.Modifiers;
using Microsoft.Xna.Framework.Graphics;

namespace SimonsGame.GuiObjects
{
	public class HealthCreep : PhysicsObject
	{
		private enum CreepCharacterAIState
		{
			MoveLeft,
			MoveRight
		}
		private CreepCharacterAIState AIState;

		private int _leftBounds;
		private int _rightBounds;
		private ModifierBase _healModifier;
		protected Animation _idleAnimation;

		public HealthCreep(Vector2 position, Vector2 hitbox, Group group, Level level, bool moveRight, int leftBounds, int rightBounds)
			: base(position, hitbox, group, level, "HealthCreep")
		{
			MaxSpeedBase = new Vector2(AverageSpeed.X / 3, AverageSpeed.Y);
			AIState = moveRight ? CreepCharacterAIState.MoveRight : CreepCharacterAIState.MoveLeft;
			_healthTotal = 6;
			_healthCurrent = _healthTotal;
			_leftBounds = leftBounds;
			_rightBounds = rightBounds;
			_healModifier = new TickModifier(1, ModifyType.Add, this);
			_healModifier.SetHealthTotal(4);
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/HealthCreep"), 1, false, 72, 40, (Size.X / 72.0f));
			_animator.Color = Color.Green;
			_animator.PlayAnimation(_idleAnimation);
		}
		public override float GetXMovement()
		{
			if (AIState == CreepCharacterAIState.MoveRight)
				return MaxSpeed.X;
			else
				return -MaxSpeed.X;
		}
		public override float GetYMovement()
		{
			return MaxSpeed.Y;
		}
		public override void PreUpdate(GameTime gameTime)
		{
			if (_healthCurrent <= 0)
			{
				// I'm dead, heal the *character* that killed me!
				//// Most likely the object that hit you is disposable, get its parent if it has one.
				// *CHANGED* the last hit target is now the owner of disposable object.  You can assume it's the Player that fired the projectile.
				if (_lastTargetHitBy != null)
					_lastTargetHitBy.HitByObject(this, _healModifier);
			}
			MainGuiObject LandedOnPlatform;
			if (_previousPosition == Position)
			{
				AIState = AIState == CreepCharacterAIState.MoveRight ? CreepCharacterAIState.MoveLeft : CreepCharacterAIState.MoveRight;
			}
			else if (PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out LandedOnPlatform)
				&& (Position.X < LandedOnPlatform.Position.X || Position.X + Size.X > LandedOnPlatform.Position.X + LandedOnPlatform.Size.X))
				AIState = AIState == CreepCharacterAIState.MoveRight ? CreepCharacterAIState.MoveLeft : CreepCharacterAIState.MoveRight;
			else if (Position.X <= _leftBounds || Position.X >= _rightBounds)
				AIState = AIState == CreepCharacterAIState.MoveRight ? CreepCharacterAIState.MoveLeft : CreepCharacterAIState.MoveRight;
			base.PreUpdate(gameTime);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
		}
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			_abilityManager.AddAbility(mb);
		}
		public override void SwitchDirections()
		{
			AIState = AIState == CreepCharacterAIState.MoveLeft ? CreepCharacterAIState.MoveRight : CreepCharacterAIState.MoveLeft;
		}
		public override string GetDirectionalText()
		{
			return AIState.ToString();
		}
		public override bool DidSwitchDirection()
		{
			return AIState == CreepCharacterAIState.MoveLeft;
		}
	}
}