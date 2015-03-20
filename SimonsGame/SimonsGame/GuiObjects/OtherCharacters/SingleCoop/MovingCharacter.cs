using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
		protected Animation _idleAnimation;
		public MovingCharacter(Vector2 position, Vector2 hitbox, Group group, Level level, bool moveRight)
			: base(position, hitbox, group, level, "MovingCharacter")
		{
			MaxSpeedBase = new Vector2(AverageSpeed.X, AverageSpeed.Y);
			AIState = moveRight ? MoveCharacterAIState.MoveRight : MoveCharacterAIState.MoveLeft;
			_healthTotal = 6;
			_healthCurrent = _healthTotal;
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/Mover"), 1, false, 80, 160, (Size.X / 80.0f));
			_animator.Color = Color.LightPink;
			_animator.PlayAnimation(_idleAnimation);
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
			else if (PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out LandedOnPlatform))
			{
				if (AIState == MoveCharacterAIState.MoveLeft && (Position.X < LandedOnPlatform.Position.X + (MaxSpeed.Y * -2)))
					AIState = MoveCharacterAIState.MoveRight;
				else if (AIState == MoveCharacterAIState.MoveRight && Position.X + Size.X > LandedOnPlatform.Position.X + LandedOnPlatform.Size.X + (MaxSpeed.Y * 2))
					AIState = MoveCharacterAIState.MoveLeft;
			}
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
			AIState = AIState == MoveCharacterAIState.MoveLeft ? MoveCharacterAIState.MoveRight : MoveCharacterAIState.MoveLeft;
		}
		public override string GetDirectionalText()
		{
			return AIState.ToString();
		}
		public override bool DidSwitchDirection()
		{
			return AIState == MoveCharacterAIState.MoveLeft;
		}
	}
}
