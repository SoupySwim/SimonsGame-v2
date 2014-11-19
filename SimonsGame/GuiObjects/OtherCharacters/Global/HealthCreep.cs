using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SimonsGame.Utility;
using SimonsGame.Modifiers;

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
		private MainGuiObject _lastTargetHitBy;
		private ModifierBase _healModifier;

		public HealthCreep(Vector2 position, Vector2 hitbox, Group group, Level level, bool moveRight, int leftBounds, int rightBounds)
			: base(position, hitbox, group, level)
		{
			MaxSpeedBase = new Vector2(AverageSpeed.X / 3, AverageSpeed.Y);
			AIState = moveRight ? CreepCharacterAIState.MoveRight : CreepCharacterAIState.MoveLeft;
			_healthTotal = 6;
			_healthCurrent = _healthTotal;
			_leftBounds = leftBounds;
			_rightBounds = rightBounds;
			_healModifier = new TickModifier(1, ModifyType.Add);
			_healModifier.SetHealthTotal(4);
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
				// Most likely the object that hit you is disposable, get its parent if it has one.
				if (_lastTargetHitBy.Parent != null)
					_lastTargetHitBy.Parent.HitByObject(this, _healModifier);
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
		protected override bool ShowHitBox()
		{
			return true;
		}
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			_lastTargetHitBy = mgo;
			_abilityManager.AddAbility(mb);
		}
	}
}