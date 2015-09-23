using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Utility;
using SimonsGame.Modifiers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SimonsGame.Modifiers.Abilities;
using SimonsGame.MapEditor;

namespace SimonsGame.GuiObjects.OtherCharacters.Global
{
	class NeutralCreep : CreepBase
	{
		private enum CreepBehavior
		{
			Aggressive = 0,
			Passive = 1
		}

		private CreepBehavior _creepBehavior;
		private int _idleCounterCurrent = -1;
		private int _idleCounterTotal = 600; // Wait for 10 seconds.  If no one has made contact, then go back to being idle.

		private Vector2 _startingPosition;

		private ModifierBase _defeatedModifier;
		protected Animation _idleAnimation;
		private MainGuiObject _targetedObject;

		public NeutralCreep(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "Neutral Creep")
		{
			Team = Team.Neutral;
			_creepBehavior = CreepBehavior.Aggressive;

			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X / 2, AverageSpeed.Y);
			_healthTotal = 800;
			_healthCurrent = _healthTotal;
			_defeatedModifier = new TickModifier(1, ModifyType.Add, this, new Tuple<Element, float>(Element.Fire, .3f));

			_startingPosition = position;

			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/Mover"), 1, false, 80, 160, (Size.X / 80.0f));
			_animator.Color = Color.LightGreen;
			_animator.PlayAnimation(_idleAnimation);

			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();
			// Jumps.
			List<PlayerAbilityInfo> jumpInfos = new List<PlayerAbilityInfo>();
			jumpInfos.Add(AbilityBuilder.GetJumpAbility(this, 1.6f));
			PlayerAbilityInfo jumpPai = jumpInfos.First(ei => ei.Name == "Jump");
			SingleJump jump = jumpPai.Modifier as SingleJump;
			jump.CheckStopped = () => jump.HasReachedEnd || ((MainGame.Randomizer.Next(30) <= 1) && (_lastTargetHitBy != null && _lastTargetHitBy.Center.Y > Center.Y));
			jumpPai.IsUsable = (abilityManager) =>
			{
				if (_lastTargetHitBy != null)
				{
					// If we already have a jump active, don't jump again.
					if (abilityManager.CurrentAbilities.ContainsKey(jumpPai.Id))
						return false;
					return ((MainGame.Randomizer.Next(40) <= 1) && (_lastTargetHitBy.Center.Y + 20) < Center.Y);
				}
				return false;
			};

			abilities.Add(KnownAbility.Jump, jumpInfos);
			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();
			PlayerAbilityInfo pai = AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/Fireball");
			pai.Name = "Ball";
			pai.AbilityAttributes = AbilityAttributes.Explosion;
			pai.Modifier.Damage = -25;
			pai.Modifier.Speed = 10.5f;
			pai.Cooldown = new TimeSpan(0, 0, 0, 1);
			pai.Modifier.SetSize(new Vector2(20, 20));
			pai.IsUsable = (abilityManager) =>
			{
				if (abilityManager.CurrentAbilities.ContainsKey(pai.Id))
					return false;

				return _lastTargetHitBy != null && _idleCounterCurrent >= 0;
				//return (AIState == MoveCharacterAIState.MoveRight ? -1 : 1) * (_previousPosition.X + GetXMovement()) < Position.X;
			};
			elementalInfos.Add(pai);


			PlayerAbilityInfo paiMelee = AbilityBuilder.GetShortRangeMeleeElementalAbility1(this);
			paiMelee.IsUsable = (abilityManager) =>
			{
				if (abilityManager.CurrentAbilities.ContainsKey(paiMelee.Id))
					return false;

				return _lastTargetHitBy != null && Math.Abs(_lastTargetHitBy.Center.X - Center.X) < 60;
				//return (AIState == MoveCharacterAIState.MoveRight ? -1 : 1) * (_previousPosition.X + GetXMovement()) < Position.X;
			};
			paiMelee.Modifier.Damage = -30;
			elementalInfos.Add(paiMelee);


			abilities.Add(KnownAbility.Elemental, elementalInfos);

			// Miscellaneous Magic
			List<PlayerAbilityInfo> miscellaneousInfos = new List<PlayerAbilityInfo>();

			abilities.Add(KnownAbility.Miscellaneous, miscellaneousInfos);

			_abilityManager = new AbilityManager(this, abilities,
				AvailableButtons.RightTrigger | AvailableButtons.RightBumper | AvailableButtons.LeftTrigger | AvailableButtons.LeftBumper);

			// Will Change when we level up stuff and things...
			_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Melee"), AvailableButtons.RightTrigger);
			_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Ball"), AvailableButtons.RightBumper);

			_abilityManager.Experience = 25;
		}
		public override float GetXMovement()
		{
			if (_lastTargetHitBy != null)
			{
				if (_creepBehavior == CreepBehavior.Aggressive)
				{
					if (_lastTargetHitBy.Center.X > Center.X)
						return MaxSpeed.X;
					else
						return -MaxSpeed.X;
				}
				else if (_creepBehavior == CreepBehavior.Passive)
				{
					if (_lastTargetHitBy.Center.X > Center.X)
						return 0;
					else
						return 0;
				}
			}
			return 0;
		}
		public override float GetYMovement()
		{
			return MaxSpeed.Y;
		}

		protected override void Died()
		{
			if (_lastTargetHitBy != null)
				_lastTargetHitBy.HitByObject(this, _defeatedModifier);
			base.Died();
		}

		public override void PreUpdate(GameTime gameTime)
		{
			if (_idleCounterCurrent > 0) _idleCounterCurrent--;
			if (_idleCounterCurrent == 0)
			{
				Position = _startingPosition; // For now, just teleport back.
				_lastTargetHitBy = null; // reset the targeted character...
				_idleCounterCurrent--; // Just so we don't do this again... It makes sense.
			}
			base.PreUpdate(gameTime);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			if (mgo.ObjectType == GuiObjectType.Character || mgo.ObjectType == GuiObjectType.Player)
			{
				_idleCounterCurrent = _idleCounterTotal;
				_targetedObject = mgo;
			}
			_abilityManager.AddAbility(mb);
		}
		public override Vector2 GetAimOverride()
		{
			if (_lastTargetHitBy != null)
			{
				Vector2 distance = _lastTargetHitBy.Center - Center;
				var normal = (float)Math.Sqrt(Math.Pow((double)distance.X, 2) + Math.Pow((double)distance.Y, 2));
				return distance / normal;
			}
			return Vector2.Zero;
		}

		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Behavior";
			//if (bType == ButtonType.SpecialToggle2)
			//	return "";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _creepBehavior.ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				int behaviorLength = Enum.GetNames(typeof(CreepBehavior)).Length;
				_creepBehavior = (CreepBehavior)(((int)_creepBehavior + (moveRight ? 1 : behaviorLength - 1)) % behaviorLength);
			}
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return (int)_creepBehavior;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_creepBehavior = (CreepBehavior)value;
			base.SetSpecialValue(bType, value);
		}
		#endregion
	}
}