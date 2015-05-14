using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects.BaseClasses;
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
	public class CreepBoss : GenericBoss
	{

		protected enum CreepBossAttackBehavior
		{
			WaitBetweenAttacks = 0,
			CircleAttack,
			StandardAttack,
			FlyAttack,
			TeleportAttack,
			PushAway,
		}

		protected enum CreepBossWalkBehavior
		{
			CloseIntoPlayer = 0,
			StayDisatanceAway,
			StayPut
		}

		private int _behaviorTickCurrent = 0; // Counts down to zero!
		private int _universalTickCurrent = 0;
		private int _walkTickCurrent = 0;
		private bool _inEmergency { get { return _healthCurrent < (_healthTotal * (_overdrivePercent / 100.0f)); } } // Should change to static number...
		private int _intensity = 5; // How "intense" the boss is. -6 through 26 scale.
		private int _intensityChange = 0; // How much the boss will change after entering overdrive.
		private int _overdrivePercent = 30; // What percentage the boss will enter overdrive.

		private int _maxIntensity = 25;
		private int _maxIntensityChange = 5;

		#region Attack Specfic Attributes
		private float _radians = 0;
		private int _circleAttackLimit = 24;
		private int _flyHeight = 0; // For Flying

		private int _ticksIntensity;
		private float _ticksIntensityPercentage;
		#endregion

		protected CreepBossAttackBehavior _attackBehavior;
		protected CreepBossWalkBehavior _walkBehavior;
		protected Animation _idleAnimation;

		public CreepBoss(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "LargeCreep")
		{
			Team = Team.Neutral;
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X / 2, AverageSpeed.Y);
			_healthTotal = 3200;
			_healthCurrent = _healthTotal;

			_attackBehavior = CreepBossAttackBehavior.WaitBetweenAttacks;
			_walkBehavior = CreepBossWalkBehavior.StayPut;
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/LargeCreep"), 1, false, 300, 400, (Size.X / 300));
			_animator.Color = Color.White;
			_animator.PlayAnimation(_idleAnimation);

			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();
			// Jumps.
			List<PlayerAbilityInfo> jumpInfos = new List<PlayerAbilityInfo>();
			jumpInfos.Add(AbilityBuilder.GetJumpAbility(this, .5f));
			PlayerAbilityInfo jumpPai = jumpInfos.First(ei => ei.Name == "Jump");
			SingleJump jump = jumpPai.Modifier as SingleJump;
			jump.CheckStopped = () => jump.HasReachedEnd || ((MainGame.Randomizer.Next(30) <= 1) && (_targetedObject != null && _targetedObject.Center.Y > Center.Y));
			jumpPai.IsUsable = (abilityManager) =>
			{
				if (_attackBehavior == CreepBossAttackBehavior.FlyAttack || _attackBehavior == CreepBossAttackBehavior.CircleAttack) // Don't jump while flying.
					return false;
				if (_targetedObject != null)
				{
					// If we already have a jump active, don't jump again.
					if (abilityManager.CurrentAbilities.ContainsKey(jumpPai.Id))
						return false;
					return MainGame.Randomizer.Next(300) <= 1;
				}
				return false;
			};
			abilities.Add(KnownAbility.Jump, jumpInfos);
			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();


			MultiPlayerAbilityInfo pai = AbilityBuilder.GetBossProjectile(this, "Test/Fireball");

			pai.IsUsable = (manager) =>
			{
				var set = manager.CurrentAbilities.Keys.Intersect(pai.GetAbilityIds());
				if (set.Count() == pai.GetAbilityIds().Count())
					return false;
				return _targetedObject != null && CanAttack(manager, pai);
			};

			pai.Modifier.Damage = -160;
			pai.Modifier.SetSize(new Vector2(60, 60));

			elementalInfos.Add(pai);


			abilities.Add(KnownAbility.Elemental, elementalInfos);

			// Miscellaneous Magic
			List<PlayerAbilityInfo> miscellaneousInfos = new List<PlayerAbilityInfo>();

			abilities.Add(KnownAbility.Miscellaneous, miscellaneousInfos);

			_abilityManager = new AbilityManager(this, abilities,
				AvailableButtons.RightTrigger | AvailableButtons.RightBumper | AvailableButtons.LeftTrigger | AvailableButtons.LeftBumper);

			// Will Change when we level up stuff and things...
		}

		public override float GetXMovement()
		{
			if (_targetedObject != null)
			{
				float speed = MaxSpeed.X * (.5f + (_ticksIntensity * .1f));//((_inEmergency ? 1.4f : 1f));
				if (_walkBehavior == CreepBossWalkBehavior.StayDisatanceAway)
				{
					float distance = Center.X - _targetedObject.Center.X;
					if (distance < -500) // to the left of player
						return speed;
					if (distance < 0)
						return -speed;
					if (distance > 500)
						return -speed;
					if (distance > 0)
						return speed;
					return 0;
				}
				if (_walkBehavior == CreepBossWalkBehavior.CloseIntoPlayer)
				{
					float distance = Center.X - _targetedObject.Center.X;
					if (distance < -100) // to the left of player
						return speed;
					if (distance > 100)
						return -speed;
					return 0;
				}
			}
			return 0;
		}
		public override float GetYMovement()
		{
			if (_attackBehavior == CreepBossAttackBehavior.FlyAttack)
			{
				_flyHeight += 8;
				if (_flyHeight >= 240)
					return 0;
				return -8;
			}
			return base.GetYMovement();
		}
		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);

			// Do some stuff!
			if (_targetedObject != null)
			{
				if (_behaviorTickCurrent == 0)
				{
					if (_attackBehavior == CreepBossAttackBehavior.WaitBetweenAttacks)
					{
						_attackBehavior = (CreepBossAttackBehavior)(MainGame.Randomizer.Next(5) + 1); // Assign a random  attacking pattern for it's next attack.
						switch (_attackBehavior)
						{
							case CreepBossAttackBehavior.CircleAttack:
								_radians = 0;
								_circleAttackLimit = (int)(8 + (132 * _ticksIntensityPercentage)); // Used to not be there.
								_behaviorTickCurrent = _circleAttackLimit + 1;
								break;
							case CreepBossAttackBehavior.FlyAttack:
								_flyHeight = 0;
								_behaviorTickCurrent = (int)(18 + (42 * _ticksIntensityPercentage)); //_inEmergency ? 30 : 20;
								break;
							case CreepBossAttackBehavior.StandardAttack:
								_behaviorTickCurrent = 2;
								break;
							case CreepBossAttackBehavior.TeleportAttack:
								Vector2 offset = new Vector2((Size.X + 20) * (Center.X < _targetedObject.Center.X ? -1 : 1), _targetedObject.Size.Y - Size.Y);
								Center = _targetedObject.Center + offset;
								break;
							case CreepBossAttackBehavior.PushAway:
								Vector2 distance = Center - _targetedObject.Center;
								float normalizer = (float)Math.Sqrt(Math.Pow((double)distance.X, 2) + Math.Pow((double)distance.Y, 2));
								float power = 1.9f + (.5f * _ticksIntensityPercentage); // might get a little crazy up in her'
								_targetedObject.HitByObject(this, new JumpPadAbility(this, power, distance / normalizer));
								break;
							default:
								_behaviorTickCurrent = 0;
								break;
						}
					}
					else
					{
						_attackBehavior = CreepBossAttackBehavior.WaitBetweenAttacks;
						int min = (int)(50 - (49 * _ticksIntensityPercentage));
						int ran = (int)(160 - (150 * _ticksIntensityPercentage));
						_behaviorTickCurrent = MainGame.Randomizer.Next(120) + min; // Next move will be anywhere between 2/3 to 8/3 seconds.
					}
				}
				else if (_attackBehavior == CreepBossAttackBehavior.WaitBetweenAttacks) // others will count down elsewhere.
				{
					_behaviorTickCurrent--;
				}

				if (_universalTickCurrent > 0)
					_universalTickCurrent--;
				if (_walkTickCurrent > 0)
					_walkTickCurrent--;
				if (_walkTickCurrent == 0)
				{
					_walkBehavior = (CreepBossWalkBehavior)(MainGame.Randomizer.Next(3));
					_walkTickCurrent = MainGame.Randomizer.Next(100) + 60;
				}
			}
			_ticksIntensity = _intensity + (_inEmergency ? _intensityChange : 0);
			_ticksIntensityPercentage = Math.Max((float)_ticksIntensity / (float)(_maxIntensity), 0);
		}

		// At this point, we know we have a target.
		private bool CanAttack(AbilityManager abilityManager, PlayerAbilityInfo pai)
		{
			if (_attackBehavior == CreepBossAttackBehavior.CircleAttack && _universalTickCurrent == 0)
			{
				pai.Modifier.Damage = -100 - (300 * _ticksIntensityPercentage); //_inEmergency ? -180 : -160;
				pai.Modifier.SetSize(new Vector2(25 + (40 * _ticksIntensityPercentage))); //_inEmergency ? 40 : 30));
				pai.Modifier.Speed = 5.25f + (5 * _ticksIntensityPercentage); //_inEmergency ? 7f : 6.25f;
				_universalTickCurrent = (int)(10 - (9 * _ticksIntensityPercentage)); //_inEmergency ? 7 : 8;
				_behaviorTickCurrent--;
				return _behaviorTickCurrent > 0;
			}
			else if (_attackBehavior == CreepBossAttackBehavior.StandardAttack)
			{
				pai.Modifier.Damage = -200 - (400 * _ticksIntensityPercentage); // _inEmergency ? -320 : -280;
				pai.Modifier.SetSize(new Vector2(55 + (25 * _ticksIntensityPercentage))); //_inEmergency ? 80 : 60));
				pai.Modifier.Speed = 9f + (5 * _ticksIntensityPercentage); //10f;
				_behaviorTickCurrent--;
				return _behaviorTickCurrent > 0;
			}
			else if (_attackBehavior == CreepBossAttackBehavior.FlyAttack && _universalTickCurrent == 0)
			{
				pai.Modifier.Damage = -80 - (100 * _ticksIntensityPercentage); //-100;
				pai.Modifier.SetSize(new Vector2(15 + (25 * _ticksIntensityPercentage))); //_inEmergency ? 30 : 20));
				pai.Modifier.Speed = 10f + (10 * _ticksIntensityPercentage); // _inEmergency ? 14f : 12f;
				_universalTickCurrent = (int)(24 - (20 * _ticksIntensityPercentage)); // _inEmergency ? 16 : 20;
				_behaviorTickCurrent--;
				return _behaviorTickCurrent > 0;
			}
			return false;
		}

		public override Vector2 GetAim()
		{
			if (_attackBehavior == CreepBossAttackBehavior.CircleAttack)
			{
				Vector2 aim = new Vector2(-(float)Math.Sin(_radians), (float)Math.Cos(_radians));
				float timesRound = 5.1f;
				if (_ticksIntensityPercentage < .1f)
					timesRound = 1;
				else if (_ticksIntensityPercentage < .45f)
					timesRound = 2.1f;
				else if (_ticksIntensityPercentage < .65f)
					timesRound = 3.1f;
				else if (_ticksIntensityPercentage < .8f)
					timesRound = 4.1f;
				_radians = (float)((_radians + ((float)((Math.PI * 2) / _circleAttackLimit) * timesRound)) % (Math.PI * 2));
				return aim;
			}

			if (_targetedObject != null)
			{
				Vector2 distance = _targetedObject.Center - Center;
				var normal = (float)Math.Sqrt(Math.Pow((double)distance.X, 2) + Math.Pow((double)distance.Y, 2));
				return distance / normal;
			}
			return Vector2.Zero;
		}
		protected override SpriteEffects GetCurrentSpriteEffects()
		{
			if (_targetedObject != null)
				return _targetedObject.Position.X < Position.X ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			return base.GetCurrentSpriteEffects();
		}


		#region Map Editor
		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Intensity";
			if (bType == ButtonType.SpecialToggle2)
				return "% HP for Overdrive";
			if (bType == ButtonType.SpecialToggle3)
				return "Overdrive Intensity Change";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _intensity.ToString();
			if (bType == ButtonType.SpecialToggle2)
				return _overdrivePercent.ToString();
			if (bType == ButtonType.SpecialToggle3)
				return _intensityChange.ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				_intensity = MathHelper.Clamp(_intensity + (moveRight ? 1 : -1), 0, _maxIntensity - _maxIntensityChange);
			if (bType == ButtonType.SpecialToggle2)
				_overdrivePercent = MathHelper.Clamp(_overdrivePercent + (moveRight ? 5 : -5), 0, 100);
			if (bType == ButtonType.SpecialToggle3)
				_intensityChange = MathHelper.Clamp(_intensityChange + (moveRight ? 1 : -1), -_maxIntensityChange, _maxIntensityChange);
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _intensity;
			if (bType == ButtonType.SpecialToggle2)
				return _overdrivePercent;
			if (bType == ButtonType.SpecialToggle3)
				return _intensityChange;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_intensity = value;
			if (bType == ButtonType.SpecialToggle2)
				_overdrivePercent = value;
			if (bType == ButtonType.SpecialToggle3)
				_intensityChange = value;
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}
}
