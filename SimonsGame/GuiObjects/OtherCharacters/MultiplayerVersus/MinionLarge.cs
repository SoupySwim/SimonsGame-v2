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
using SimonsGame.Extensions;
using SimonsGame.Utility.ObjectAnimations;

namespace SimonsGame.GuiObjects
{
	class MinionLarge : CreepBase
	{
		private enum MinionLargeAIState
		{
			MoveLeft,
			MoveRight
		}
		private MainGuiObject _targetedObject;
		private Vector4 _hitBounds;
		private int _targetDistance = 400;
		private MinionLargeAIState AIState;
		protected Animation _idleAnimation;
		private bool _overrideJump = false;
		private Guid _attackId;
		private TickTimer _attackPrepTimer;

		public MinionLarge(Vector2 position, Vector2 hitbox, Group group, Level level, bool moveRight)
			: base(position, hitbox, group, level, "Minion Large")
		{
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X / 8.0f, AverageSpeed.Y);
			AccelerationBase = new Vector2(.02f, .03f);
			AIState = moveRight ? MinionLargeAIState.MoveRight : MinionLargeAIState.MoveLeft;
			_healthTotal = 1200;
			_healthCurrent = _healthTotal;
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/LargeCreep"), 1, false, 300, 400, (Size.X / 300));
			_animator.Color = Color.LightPink;
			_animator.PlayAnimation(_idleAnimation);
			_attackPrepTimer = new TickTimer(60, () => { }, false);

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
				// Don't jump if we are targetting an object or prepping an attack.
				if (AbilityManager.CoolDownTimer(_attackId) != 0 || abilityManager.CurrentAbilities.ContainsKey(jumpPai.Id) || _attackPrepTimer.IsRunning())
					return false;
				return _overrideJump || (Position.X == PreviousPosition.X && PrimaryOverlapObjects[Orientation.Horizontal].Any(mgo => mgo.ObjectType == GuiObjectType.Environment));
			};

			_hitBounds = new Vector4(Size.X - _targetDistance, 0, Size.X + _targetDistance * 2, 0);

			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();
			elementalInfos.Add(AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/Fireball"));
			abilities.Add(KnownAbility.Elemental, elementalInfos);

			_abilityManager = new AbilityManager(this, abilities, AvailableButtons.None);

			PlayerAbilityInfo attack = elementalInfos.First(ei => ei.Name == "Ball2");
			attack.Modifier.LevelUpMagic(4.2f, -300, AbilityAttributes.Explosion);
			float attackDiameter = Size.X / 1.25f;
			attack.Modifier.SetSize(new Vector2(attackDiameter));
			attack.Modifier.LevelUpMagicHitBoxBuffer(new Vector4(-40, -(Size.Y - attackDiameter), 80, (Size.Y - attackDiameter) * 2));
			attack.Cooldown = new TimeSpan(0, 0, 3);
			attack.IsUsable = (abilityManager) =>
			{
				if (abilityManager.CurrentAbilities.ContainsKey(attack.Id))
					return false;
				if (_targetedObject != null && !_attackPrepTimer.IsRunning()) // If the attack Preperation timer is finished prepping, then attack!
				{
					_targetedObject = null;
					return true;
				}
				return false;
			};
			_attackId = attack.Id;

			_abilityManager.Experience = 30;
		}
		public override float GetXMovement()
		{
			if (AbilityManager.CoolDownTimer(_attackId) != 0 || _attackPrepTimer.IsRunning())
				return 0;

			if (AIState == MinionLargeAIState.MoveRight)
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
			AIState = AIState == MinionLargeAIState.MoveLeft ? MinionLargeAIState.MoveRight : MinionLargeAIState.MoveLeft;
		}

		public override string GetDirectionalText()
		{
			return AIState.ToString();
		}

		public override bool DidSwitchDirection()
		{
			return AIState == MinionLargeAIState.MoveLeft;
		}

		public override Vector2 GetAim()
		{
			if (_targetedObject != null)
				return new Vector2(_targetedObject.Center.X < Center.X ? -1 : 1, 0);
			return new Vector2(AIState == MinionLargeAIState.MoveLeft ? -1 : 1, 0);
		}

		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);

			if (_attackPrepTimer.IsRunning())
				_attackPrepTimer.Update(gameTime);
			else
			{
				bool wasNull = _targetedObject == null;
				_targetedObject = Level.GetAllCharacterObjects(Bounds).Concat(Level.GetPossiblyHitEnvironmentObjects(Bounds + _hitBounds).Where(mgo => mgo.ObjectType == GuiObjectType.Structure && mgo.Team != Team)).Where(c => c.Team != Team && c.Team > Team.Neutral)
					.Select(mgo => new { mgo = mgo, distance = this.DistanceBetween(mgo) })
					.Where(tup => tup.distance <= _targetDistance && tup.mgo.Position.Y < Position.Y + Size.Y && tup.mgo.Position.Y + tup.mgo.Size.Y > Position.Y).OrderBy(tup => tup.distance).Select(tup => tup.mgo).FirstOrDefault();

				if (wasNull && _targetedObject != null)
					_attackPrepTimer.Restart();
			}

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
		public override void SwitchTeam(Team newTeam)
		{
			_team = newTeam;
			_hitBoxColor = TeamColorMap[newTeam];
			_animator.Color = Color.Lerp(_hitBoxColor, Color.White, .35f);
		}
		protected override SpriteEffects GetCurrentSpriteEffects()
		{
			if (_targetedObject != null)
				return _targetedObject.Center.X < Center.X ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			return AIState == MinionLargeAIState.MoveLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		}
	}
}
