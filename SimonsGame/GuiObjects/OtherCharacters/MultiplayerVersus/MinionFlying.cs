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
using System.Diagnostics;

namespace SimonsGame.GuiObjects
{
	class MinionFlying : CreepBase
	{

		private enum FlyingState
		{
			MovingUp,
			MovingDown,
			Attacking,
			KnockedDown
		}

		private enum MinionFlyingAIState
		{
			MoveLeft,
			MoveRight
		}

		private MinionFlyingAIState _AIState;
		private FlyingState _flyingState;
		private FlyingState _storeFlyingState;

		private int _distanceFromGroundMax = 120;
		private int _pulseDistance = 20;

		private int _targetDistance = 300;

		private Vector4 _hitBounds;

		protected Animation _idleAnimation;

		private bool _overrideJump = false;
		private bool _canAttack = false;

		private TickTimer _canAttackTimer;
		private TickTimer _knockedDownTimer;

		private MainGuiObject _targetedObject;

		public MinionFlying(Vector2 position, Vector2 hitbox, Group group, Level level, bool moveRight)
			: base(position, hitbox, group, level, "Minion Flying")
		{
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X / 5.75f, AverageSpeed.Y / 10.4f);
			AccelerationBase = new Vector2(.2f, 1);

			_AIState = moveRight ? MinionFlyingAIState.MoveRight : MinionFlyingAIState.MoveLeft;
			_flyingState = FlyingState.MovingDown;

			_healthTotal = 400;
			_healthCurrent = _healthTotal;
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/MinionFlying"), .1f, true, 80, 80, (Size.X / 80.0f));
			_animator.Color = Color.LightPink;
			_animator.PlayAnimation(_idleAnimation);

			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();

			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();

			PlayerAbilityInfo attack = AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/Fireball");
			attack.Name = "Ball";
			attack.Modifier.Damage = -45;
			attack.Modifier.Speed = 15;
			attack.Cooldown = new TimeSpan(0, 0, 0, 0, 575);
			attack.Modifier.SetTickCount(35);
			attack.Modifier.SetSize(new Vector2(16, 16));

			attack.IsUsable = (abilityManager) =>
			{
				if (_targetedObject != null && _flyingState == FlyingState.Attacking && !abilityManager.CurrentAbilities.ContainsKey(attack.Id))
				{
					_canAttack = false;
					return true;
				}
				return false;
			};

			elementalInfos.Add(attack);

			abilities.Add(KnownAbility.Elemental, elementalInfos);

			_abilityManager = new AbilityManager(this, abilities, AvailableButtons.None);

			_canAttackTimer = new TickTimer(100, () => _canAttack = true, true); // Could easily not let this loop...
			_knockedDownTimer = new TickTimer(1000, () => _flyingState = FlyingState.MovingUp, false);

			_hitBounds = new Vector4((float)-_targetDistance, (float)-_targetDistance, (float)_targetDistance, (float)_targetDistance);

			_abilityManager.Experience = 10;
		}
		public override float GetXMovement()
		{
			if (_flyingState == FlyingState.Attacking || _flyingState == FlyingState.KnockedDown)
				return 0;
			if (_AIState == MinionFlyingAIState.MoveRight)
				return MaxSpeed.X;
			else
				return -MaxSpeed.X;
		}
		public override float GetYMovement()
		{
			if (_overrideJump || _flyingState == FlyingState.MovingUp)
				return -MaxSpeed.Y;
			else if (_flyingState == FlyingState.MovingDown)
				return MaxSpeed.Y;
			return 0;
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
			//_flyingState = FlyingState.KnockedDown;
			//_knockedDownTimer.Restart();
			_abilityManager.AddAbility(mb);
		}

		public override void SwitchDirections()
		{
			_AIState = _AIState == MinionFlyingAIState.MoveLeft ? MinionFlyingAIState.MoveRight : MinionFlyingAIState.MoveLeft;
		}

		public override string GetDirectionalText()
		{
			return _AIState.ToString();
		}

		public override bool DidSwitchDirection()
		{
			return _AIState == MinionFlyingAIState.MoveLeft;
		}

		public override Vector2 GetAim()
		{
			if (_targetedObject != null)
			{
				Vector2 distance = _targetedObject.Center - Center;
				var normal = (float)Math.Sqrt(Math.Pow((double)distance.X, 2) + Math.Pow((double)distance.Y, 2));
				return distance / normal;
			}
			return Vector2.Zero;
		}

		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);

			if (!_canAttack)
				_canAttackTimer.Update(gameTime);

			_targetedObject = Level.GetAllCharacterObjects(Bounds).Concat(Level.GetPossiblyHitEnvironmentObjects(Bounds + _hitBounds).Where(mgo => mgo.ObjectType == GuiObjectType.Structure && mgo.Team != Team)).Where(c => c.Team != Team && c.Team > Team.Neutral)
				.Select(mgo => new { mgo = mgo, distance = this.DistanceBetween(mgo) })
				.Where(tup => tup.distance <= _targetDistance).OrderBy(tup => tup.distance).Select(tup => tup.mgo).FirstOrDefault();

			if (_flyingState == FlyingState.Attacking && _targetedObject == null)
				_flyingState = _storeFlyingState;

			if (_targetedObject != null && _canAttack)
			{
				_storeFlyingState = _flyingState == FlyingState.Attacking ? _storeFlyingState : _flyingState;
				_flyingState = FlyingState.Attacking;
			}

			VerticalPass = false;
			_overrideJump = false;
			var tempBounds = GetFromFloorBounds();
			var hitPlatforms = GetHitObjects(Level.GetPossiblyHitEnvironmentObjects(tempBounds).OrderBy(mgo => mgo.Position.Y).Where(mgo => mgo.ObjectType != GuiObjectType.Structure || mgo.Team != Team), tempBounds);

			if (!hitPlatforms.Any())
				_flyingState = FlyingState.MovingDown;
			else
			{
				MainGuiObject firstHit = hitPlatforms.Select(tup => tup.Item2).FirstOrDefault(mgo => mgo.Group == Group.Impassable || mgo.Group == Group.ImpassableIncludingMagic) ?? hitPlatforms.Select(tup => tup.Item2).FirstOrDefault();
				if (firstHit.Position.Y < Center.Y + _distanceFromGroundMax - _pulseDistance)
					_flyingState = FlyingState.MovingUp;
			}
			if (_flyingState == FlyingState.KnockedDown)
				_knockedDownTimer.Update(gameTime);
		}

		private Vector4 GetFromFloorBounds()
		{
			Vector4 bounds = Bounds;
			bounds.Z += _flyingState == FlyingState.MovingDown ? _distanceFromGroundMax - _pulseDistance : _distanceFromGroundMax;
			return bounds;
		}

		// This happens in PreUpdate.
		public override void TriggerBehavior(BehaviorZone zone)
		{
			if (zone.BehaviorModifier == BehaviorModifier.Jump)
				_overrideJump = true;
		}

		public override void SwitchTeam(Team newTeam)
		{
			_team = newTeam;
			_hitBoxColor = TeamColorMap[newTeam];
			_animator.Color = Color.Lerp(_hitBoxColor, Color.White, .25f);
		}

		protected override SpriteEffects GetCurrentSpriteEffects()
		{
			if (_targetedObject != null)
				return _targetedObject.Position.X < Position.X ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			return _AIState == MinionFlyingAIState.MoveRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		}

		protected override Vector4 GetHealthBarBounds()
		{
			return new Vector4(Bounds.X, Bounds.Y - 5, 5, Bounds.W);
		}
	}
}
