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
	class MinionNormal : CreepBase
	{
		private enum MinionNormalAIState
		{
			MoveLeft,
			MoveRight
		}
		private MainGuiObject _targetedPlayer = null;
		private MinionNormalAIState AIState;
		protected Animation _idleAnimation;
		private bool _overrideJump = false;
		public MinionNormal(Vector2 position, Vector2 hitbox, Group group, Level level, bool moveRight)
			: base(position, hitbox, group, level, "Minion Normal")
		{
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.X / 6.3f, AverageSpeed.Y / 1.25f);
			AccelerationBase = new Vector2(.04f, .03f);
			AIState = moveRight ? MinionNormalAIState.MoveRight : MinionNormalAIState.MoveLeft;
			_healthTotal = 600;
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
				bool ret = _targetedPlayer != null;
				_targetedPlayer = null;
				return ret;
			};
			_abilityManager.SetAbility(attack, AvailableButtons.RightTrigger);

			_abilityManager = new AbilityManager(this, abilities, AvailableButtons.None);
			_abilityManager.Experience = 10;
		}
		public override float GetXMovement()
		{
			if (AIState == MinionNormalAIState.MoveRight)
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
			if (mb == null && mgo != null && mgo.ObjectType == GuiObjectType.Character || mgo.ObjectType == GuiObjectType.Player) // you've hit something
				_targetedPlayer = mgo;
			_abilityManager.AddAbility(mb);
		}

		public override void SwitchDirections()
		{
			AIState = AIState == MinionNormalAIState.MoveLeft ? MinionNormalAIState.MoveRight : MinionNormalAIState.MoveLeft;
		}

		public override string GetDirectionalText()
		{
			return AIState.ToString();
		}

		public override bool DidSwitchDirection()
		{
			return AIState == MinionNormalAIState.MoveLeft;
		}

		public override Vector2 GetAimOverride()
		{
			return new Vector2(AIState == MinionNormalAIState.MoveLeft ? -1 : 1, 0);
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
		public override void SwitchTeam(Team newTeam)
		{
			_team = newTeam;
			_hitBoxColor = TeamColorMap[newTeam];
			_animator.Color = Color.Lerp(_hitBoxColor, Color.White, .25f);
		}
	}
}
