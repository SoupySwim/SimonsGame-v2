using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SimonsGame.Utility;
using SimonsGame.Modifiers;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MapEditor;

namespace SimonsGame.GuiObjects
{
	public class HealthCreep : CreepBase
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

		private int _healthGain;

		public HealthCreep(Vector2 position, Vector2 hitbox, Group group, Level level, bool moveRight, int leftBounds, int rightBounds)
			: base(position, hitbox, group, level, "Health Creep")
		{
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X / 3, AverageSpeed.Y);
			AIState = moveRight ? CreepCharacterAIState.MoveRight : CreepCharacterAIState.MoveLeft;
			_healthTotal = 120;
			_healthCurrent = _healthTotal;
			_leftBounds = leftBounds;
			_rightBounds = rightBounds;
			_healModifier = new TickModifier(1, ModifyType.Add, this, new Tuple<Element, float>(Element.Fire, .3f));

			_healthGain = 200;
			_healModifier.SetHealthTotal(_healthGain);
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/HealthCreep"), 1, false, 72, 40, (Size.X / 72.0f));
			_animator.Color = Color.Green;
			_animator.PlayAnimation(_idleAnimation);
			_abilityManager.Experience = 5;
			Team = Team.Neutral;
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
			List<MainGuiObject> landedOnPlatforms = PrimaryOverlapObjects[Orientation.Vertical];
			MainGuiObject landedOnPlatform = landedOnPlatforms.FirstOrDefault();
			if (_previousPosition == Position)
				AIState = AIState == CreepCharacterAIState.MoveRight ? CreepCharacterAIState.MoveLeft : CreepCharacterAIState.MoveRight;
			else if (landedOnPlatform != null
				&& ((Position.X < landedOnPlatform.Position.X && AIState == CreepCharacterAIState.MoveLeft)
				|| (Position.X + Size.X > landedOnPlatform.Position.X + landedOnPlatform.Size.X && AIState == CreepCharacterAIState.MoveRight)))
				AIState = AIState == CreepCharacterAIState.MoveRight ? CreepCharacterAIState.MoveLeft : CreepCharacterAIState.MoveRight;
			else if (Position.X <= _leftBounds || Position.X >= _rightBounds)
				AIState = AIState == CreepCharacterAIState.MoveRight ? CreepCharacterAIState.MoveLeft : CreepCharacterAIState.MoveRight;
			base.PreUpdate(gameTime);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer)
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
		protected override Vector4 GetHealthBarBounds()
		{
			return new Vector4(Bounds.X, Bounds.Y - 5, 5, Bounds.W);
		}


		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Health Gain";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _healthGain.ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				_healthGain = MathHelper.Clamp(_healthGain + (moveRight ? 50 : -50), 50, 500);
				_healModifier.SetHealthTotal(_healthGain);
			}
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _healthGain;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				_healthGain = value;
				_healModifier.SetHealthTotal(_healthGain);
			}
			base.SetSpecialValue(bType, value);
		}

		#endregion


	}
}