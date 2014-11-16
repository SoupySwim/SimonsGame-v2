using Microsoft.Xna.Framework;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	class MovingCharacter : PhysicsObject
	{
		private enum MoveCharacterAIState
		{
			MoveLeft,
			MoveRight
		}
		private MoveCharacterAIState AIState;
		public MovingCharacter(Vector2 position, Vector2 hitbox, Group group, Level level, bool moveRight)
			: base(position, hitbox, group, level)
		{
			MaxSpeedBase = new Vector2(AverageSpeed.X, AverageSpeed.Y);
			AIState = moveRight ? MoveCharacterAIState.MoveRight : MoveCharacterAIState.MoveLeft;
			_healthTotal = 6;
			_healthCurrent = _healthTotal;
		}
		public override float GetXMovement()
		{
			if (AIState == MoveCharacterAIState.MoveRight)
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
			MainGuiObject LandedOnPlatform;
			if (_previousPosition == Position)
			{
				AIState = AIState == MoveCharacterAIState.MoveRight ? MoveCharacterAIState.MoveLeft : MoveCharacterAIState.MoveRight;
			}
			else if (PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out LandedOnPlatform)
				&& (Position.X < LandedOnPlatform.Position.X || Position.X + Size.X > LandedOnPlatform.Position.X + LandedOnPlatform.Size.X))
				AIState = AIState == MoveCharacterAIState.MoveRight ? MoveCharacterAIState.MoveLeft : MoveCharacterAIState.MoveRight;
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
			_abilityManager.AddAbility(mb);
		}
	}
}
