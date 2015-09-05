using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects.Zones;
using SimonsGame.Modifiers;
using SimonsGame.Modifiers.Abilities;
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
		private bool _overrideJump = false;
		public MovingCharacter(Vector2 position, Vector2 hitbox, Group group, Level level, bool moveRight)
			: base(position, hitbox, group, level, "Moving Character")
		{
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X, AverageSpeed.Y);
			AIState = moveRight ? MoveCharacterAIState.MoveRight : MoveCharacterAIState.MoveLeft;
			_healthTotal = 200;
			_healthCurrent = _healthTotal;
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/Mover"), 1, false, 80, 160, (Size.X / 80.0f));
			_animator.Color = Color.LightPink;
			_animator.PlayAnimation(_idleAnimation);

			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();

			List<PlayerAbilityInfo> jumpInfos = new List<PlayerAbilityInfo>();
			jumpInfos.Add(AbilityBuilder.GetJumpAbility(this, 1.6f));

			abilities.Add(KnownAbility.Jump, jumpInfos);
			PlayerAbilityInfo jumpPai = jumpInfos.First(ei => ei.Name == "Jump");
			SingleJump jump = jumpPai.Modifier as SingleJump;
			jump.CheckStopped = () => jump.HasReachedEnd;//Position.X != PreviousPosition.X; // Stop after you've moved again!
			jumpPai.IsUsable = (abilityManager) =>
			{
				// If we already have a jump active, don't jump again.
				if (abilityManager.CurrentAbilities.ContainsKey(jumpPai.Id))
					return false;
				return _overrideJump || (Position.X == PreviousPosition.X && PrimaryOverlapObjects[Orientation.Horizontal].Any(mgo => mgo.ObjectType == GuiObjectType.Environment));
			};


			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();
			elementalInfos.Add(AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/Fireball"));
			abilities.Add(KnownAbility.Elemental, elementalInfos);
			PlayerAbilityInfo attack = elementalInfos.First(ei => ei.Name == "Ball2");
			attack.Modifier.Damage = -100;
			attack.IsUsable = (abilityManager) =>
			{
				if (abilityManager.CurrentAbilities.ContainsKey(attack.Id))
					return false;
				bool ret = Math.Abs(Position.X - _previousPosition.X) < (Math.Abs(GetXMovement()) - .0001f);
				return ret;
				//return (AIState == MoveCharacterAIState.MoveRight ? -1 : 1) * (_previousPosition.X + GetXMovement()) < Position.X;
			};
			_abilityManager.SetAbility(attack, AvailableButtons.RightTrigger);

			_abilityManager = new AbilityManager(this, abilities, AvailableButtons.None);
		}
		public override float GetXMovement()
		{
			if (AIState == MoveCharacterAIState.MoveRight)
				return MaxSpeed.X / 2f;
			else
				return -MaxSpeed.X / 2f;
		}
		public override float GetYMovement()
		{
			return MaxSpeed.Y;
		}


		public override void PreUpdate(GameTime gameTime)
		{
			//List<MainGuiObject> landedOnPlatforms = PrimaryOverlapObjects[Orientation.Vertical];
			//MainGuiObject landedOnPlatform = landedOnPlatforms.FirstOrDefault();
			if (_previousPosition == Position)
			{
				//AIState = AIState == MoveCharacterAIState.MoveRight ? MoveCharacterAIState.MoveLeft : MoveCharacterAIState.MoveRight;
			}
			////else 
			//if (landedOnPlatform != null)
			//{
			//	if (AIState == MoveCharacterAIState.MoveLeft && (Position.X < landedOnPlatform.Position.X + (MaxSpeed.Y * -2)))
			//		AIState = MoveCharacterAIState.MoveRight;
			//	else if (AIState == MoveCharacterAIState.MoveRight && Position.X + Size.X > landedOnPlatform.Position.X + landedOnPlatform.Size.X + (MaxSpeed.Y * 2))
			//		AIState = MoveCharacterAIState.MoveLeft;
			//}
			base.PreUpdate(gameTime);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
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

		public override Vector2 GetAim()
		{
			return new Vector2(AIState == MoveCharacterAIState.MoveLeft ? -1 : 1, 0);
		}

		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);

			VerticalPass = false;
			_overrideJump = false;
		}

		// This happens in PreUpdate.
		public override void TriggerBehavior(BehaviorZone zone)
		{
			if (zone.BehaviorModifier == BehaviorModifier.Jump)
				_overrideJump = true;
			else if (zone.BehaviorModifier == BehaviorModifier.DropDown)
				VerticalPass = true;
		}
	}
}
