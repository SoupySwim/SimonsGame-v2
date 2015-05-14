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
	class LargeCreep : PhysicsObject
	{

		private int _idleCounterCurrent = -1;
		private int _idleCounterTotal = 600; // Wait for 10 seconds.  If no one has made contact, then go back to being idle.

		private Vector2 _startingPosition;

		private ModifierBase _defeatedModifier;
		protected Animation _idleAnimation;
		private MainGuiObject _targetedObject;
		private bool _facingLeft = true;

		public LargeCreep(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "LargeCreep")
		{
			Team = Team.Neutral;
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X / 2, AverageSpeed.Y);
			_healthTotal = 1600;
			_healthCurrent = _healthTotal;
			_defeatedModifier = new TickModifier(1, ModifyType.Add, this, Element.Normal);

			_startingPosition = position;

			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/LargeCreep"), 1, false, 300, 400, (Size.X / 300));
			_animator.Color = Color.White;
			_animator.PlayAnimation(_idleAnimation);

			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();
			// Jumps.
			List<PlayerAbilityInfo> jumpInfos = new List<PlayerAbilityInfo>();
			jumpInfos.Add(AbilityBuilder.GetJumpAbility(this, .5f));
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
			pai.Modifier.Damage = -160;
			pai.Modifier.Speed = 8.25f;
			pai.Modifier.SetSize(new Vector2(60, 60));
			pai.Cooldown = new TimeSpan(0, 0, 0, 1, 500);
			pai.IsUsable = (abilityManager) =>
			{
				if (abilityManager.CurrentAbilities.ContainsKey(pai.Id))
					return false;

				return _lastTargetHitBy != null && _idleCounterCurrent >= 0;
				//return (AIState == MoveCharacterAIState.MoveRight ? -1 : 1) * (_previousPosition.X + GetXMovement()) < Position.X;
			};
			elementalInfos.Add(pai);

			abilities.Add(KnownAbility.Elemental, elementalInfos);

			// Miscellaneous Magic
			List<PlayerAbilityInfo> miscellaneousInfos = new List<PlayerAbilityInfo>();

			abilities.Add(KnownAbility.Miscellaneous, miscellaneousInfos);

			_abilityManager = new AbilityManager(this, abilities,
				AvailableButtons.RightTrigger | AvailableButtons.RightBumper | AvailableButtons.LeftTrigger | AvailableButtons.LeftBumper);

			// Will Change when we level up stuff and things...
			_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Ball"), AvailableButtons.RightBumper);
		}
		public override float GetXMovement()
		{
			return 0;
		}
		public override float GetYMovement()
		{
			return MaxSpeed.Y;
		}

		protected override void Died()
		{
			if (_lastTargetHitBy is Player)
				_lastTargetHitBy.HitByObject(this, _defeatedModifier); // Later will check a radius or keep track of all players that helped kill.
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
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
		}
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
		public override Vector2 GetAim()
		{
			if (_lastTargetHitBy != null)
			{
				Vector2 distance = _lastTargetHitBy.Center - Center;
				var normal = (float)Math.Sqrt(Math.Pow((double)distance.X, 2) + Math.Pow((double)distance.Y, 2));
				return distance / normal;
			}
			return Vector2.Zero;
		}

		protected override SpriteEffects GetCurrentSpriteEffects()
		{
			if (CurrentMovement.X == 0)
				return _facingLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			return base.GetCurrentSpriteEffects();
		}

		#region Map Editor

		public override void SwitchDirections()
		{
			_facingLeft = !_facingLeft;
		}
		public override string GetDirectionalText()
		{
			return _facingLeft ? "Left" : "Right";
		}
		public override bool DidSwitchDirection()
		{
			return !_facingLeft;
		}
		//public override string GetSpecialTitle(ButtonType bType)
		//{
		//	if (bType == ButtonType.SpecialToggle1)
		//		return "Behavior";
		//	//if (bType == ButtonType.SpecialToggle2)
		//	//	return "";
		//	return base.GetSpecialTitle(bType);
		//}

		//public override string GetSpecialText(ButtonType bType)
		//{
		//	if (bType == ButtonType.SpecialToggle1)
		//		return _creepBehavior.ToString();
		//	return base.GetSpecialText(bType);
		//}

		//public override void ModifySpecialText(ButtonType bType, bool moveRight)
		//{
		//	if (bType == ButtonType.SpecialToggle1)
		//	{
		//		int behaviorLength = Enum.GetNames(typeof(CreepBehavior)).Length;
		//		_creepBehavior = (CreepBehavior)(((int)_creepBehavior + (moveRight ? 1 : behaviorLength - 1)) % behaviorLength);
		//	}
		//	base.ModifySpecialText(bType, moveRight);
		//}
		//public override int GetSpecialValue(ButtonType bType) // For Saving the object
		//{
		//	if (bType == ButtonType.SpecialToggle1)
		//		return (int)_creepBehavior;
		//	return base.GetSpecialValue(bType);
		//}
		//public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		//{
		//	if (bType == ButtonType.SpecialToggle1)
		//		_creepBehavior = (CreepBehavior)value;
		//	base.SetSpecialValue(bType, value);
		//}
		#endregion
	}
}