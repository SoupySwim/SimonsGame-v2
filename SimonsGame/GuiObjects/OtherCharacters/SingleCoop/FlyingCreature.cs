using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class FlyingCreature : CreepBase
	{
		private enum FlyingCreatureBehavior
		{
			Idle = 0,
			Flying,
			Attacking
		}
		protected Animation _idleAnimation;
		protected Animation _flyAnimation;
		private MainGuiObject _targetedObject;
		private FlyingCreatureBehavior _behavior;
		private Vector4 _idleSensorRange;
		private Vector2 _flyTravelRange;

		private Guid _attackId;

		private bool _flyingRight;
		private bool _flyingUp;
		private bool _doesAttack;
		private int _attackingTickTotal = 90; // 1.5 seconds
		private int _attackingTickCurrent;

		private int _intensity = 5; // How "intense" the character is. -6 through 26 scale.
		private float _ticksIntensityPercentage;

		private int _maxIntensity = 20;
		private int _maxIntensityChange = 0;


		public FlyingCreature(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "Flying Creature")
		{
			Team = Team.Neutral;

			AccelerationBase = new Vector2(1);

			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X / 2, AverageSpeed.Y / 2.5f);
			_healthTotal = 400;
			_healthCurrent = _healthTotal;
			_idleSensorRange = new Vector4(Center.X - 140, Position.Y, 700, Size.X + 280); // We'll see...

			_behavior = FlyingCreatureBehavior.Idle;
			_flyTravelRange = Vector2.Zero;
			_flyingRight = true;
			_flyingUp = false;
			_doesAttack = true;

			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/BatIdle"), 1, false, 100, 100, (Size.X / 100));
			_flyAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/BatFly"), 1, false, 100, 100, (Size.X / 100));
			_animator.Color = Color.Gray;
			_animator.PlayAnimation(_idleAnimation);


			// Ability info follows


			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();
			//// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();
			PlayerAbilityInfo pai = AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/Fireball");
			pai.Name = "Ball";
			pai.Modifier.Damage = -25;
			pai.Modifier.Speed = 15;
			pai.Cooldown = new TimeSpan(120 * TimeSpan.TicksPerMillisecond);
			pai.Modifier.SetTickCount(20);
			pai.Modifier.SetSize(new Vector2(16, 16));
			pai.IsUsable = (abilityManager) =>
			{
				return _targetedObject != null && _behavior == FlyingCreatureBehavior.Attacking && !abilityManager.CurrentAbilities.ContainsKey(pai.Id);
			};
			elementalInfos.Add(pai);
			_attackId = pai.Id;

			abilities.Add(KnownAbility.Elemental, elementalInfos);

			_abilityManager = new AbilityManager(this, abilities,
				AvailableButtons.RightTrigger | AvailableButtons.RightBumper | AvailableButtons.LeftTrigger | AvailableButtons.LeftBumper);

			_abilityManager.Experience = (int)((_intensity + 1) * 4.5f);
		}


		public override float GetXMovement()
		{
			if (_behavior == FlyingCreatureBehavior.Idle || _behavior == FlyingCreatureBehavior.Attacking)
				return 0;
			return (_flyingRight ? 1 : -1) * MaxSpeed.X; // for now, we are just idle.
		}
		public override float GetYMovement()
		{
			if (_behavior == FlyingCreatureBehavior.Idle)
				return -.25f; // Go upwards.
			else if (_behavior == FlyingCreatureBehavior.Attacking)
				return 0;
			return (_flyingUp ? -1 : 1) * MaxSpeed.Y; // for now, we are just idle.
		}

		public override void PreUpdate(GameTime gameTime)
		{
			if (_behavior == FlyingCreatureBehavior.Idle)
			{
				IEnumerable<Player> players = Level.Players.Select(kv => kv.Value);

				foreach (Player player in players)
				{
					if (MainGuiObject.GetIntersectionDepth(_idleSensorRange, player.HitBoxBounds) != Vector2.Zero)
					{
						ChangeBehavior(FlyingCreatureBehavior.Flying);
						_targetedObject = player;
						_flyTravelRange = Vector2.Zero;
						_flyingRight = _targetedObject.Center.X > Center.X;
						_flyingUp = false;
					}
				}
			}
			else if (_behavior == FlyingCreatureBehavior.Flying)
			{
				// Stay positive folks!
				Vector2 tickDistance = (Position - PreviousPosition);
				_flyTravelRange.X += Math.Abs(tickDistance.X);
				_flyTravelRange.Y += Math.Abs(tickDistance.Y);

				if (!_flyingUp) // If flying downwards, then do this
				{
					if (Math.Sqrt(Math.Pow(_flyTravelRange.X, 2) + Math.Pow(_flyTravelRange.Y, 2)) > 300 || PreviousPosition.Y >= Position.Y)
					{
						if (_doesAttack)
						{
							ChangeBehavior(FlyingCreatureBehavior.Attacking);
							_attackingTickCurrent = 0;
						}
						else
						{
							_flyingUp = true;
							_flyTravelRange = Vector2.Zero;
						}
					}
				}
				else if (PrimaryOverlapObjects[Orientation.Vertical].Any())
				{
					ChangeBehavior(FlyingCreatureBehavior.Idle); // Probably have to include some counter here...
					_idleSensorRange.X = Center.X - 140;
					_idleSensorRange.Y = Position.Y;
				}
			}
			else if (_behavior == FlyingCreatureBehavior.Attacking)
			{
				_attackingTickCurrent++;
				if (_attackingTickCurrent > _attackingTickTotal)
				{
					ChangeBehavior(FlyingCreatureBehavior.Flying);
					_flyingUp = true;
					_flyTravelRange = Vector2.Zero;
				}
			}
			base.PreUpdate(gameTime);
		}

		private void ChangeBehavior(FlyingCreatureBehavior flyingCreatureBehavior)
		{
			_behavior = flyingCreatureBehavior;
			if (flyingCreatureBehavior == FlyingCreatureBehavior.Idle)
				_animator.PlayAnimation(_idleAnimation);
			else
				_animator.PlayAnimation(_flyAnimation);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			_abilityManager.AddAbility(mb);
		}
		public override Vector2 GetAimOverride()
		{
			if (_targetedObject != null)
			{
				Vector2 distance = _targetedObject.Center - Center;
				var normal = (float)Math.Sqrt(Math.Pow((double)distance.X, 2) + Math.Pow((double)distance.Y, 2));
				return distance / normal;
			}
			return Vector2.Zero;
		}



		private void ChangeIntesity(int newIntensity)
		{
			_intensity = newIntensity;
			_ticksIntensityPercentage = (float)_intensity / (float)_maxIntensity;

			PlayerAbilityInfo pai = _abilityManager.GetAbilityInfo(_attackId);
			pai.Modifier.Damage = -10 - (3 * _ticksIntensityPercentage);
			pai.Modifier.Speed = 10f + (.75f * _ticksIntensityPercentage);
		}

		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Does Attack";
			if (bType == ButtonType.SpecialToggle2)
				return "Intensity";
			//if (bType == ButtonType.SpecialToggle2)
			//	return "";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _doesAttack ? "Yes" : "No";
			if (bType == ButtonType.SpecialToggle2)
				return _intensity.ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				_doesAttack = !_doesAttack;
			if (bType == ButtonType.SpecialToggle2)
				ChangeIntesity(MathHelper.Clamp(_intensity + (moveRight ? 1 : -1), 0, _maxIntensity - _maxIntensityChange));
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _doesAttack ? 1 : 0;
			if (bType == ButtonType.SpecialToggle2)
				return _intensity;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_doesAttack = value == 1;
			if (bType == ButtonType.SpecialToggle2)
				ChangeIntesity(value);
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}
}
