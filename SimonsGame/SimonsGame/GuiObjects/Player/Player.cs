using Microsoft.Xna.Framework;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using SimonsGame.Modifiers;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers.Abilities;
using SimonsGame.MainFiles;
using SimonsGame.Menu.MenuScreens;

namespace SimonsGame.GuiObjects
{
	public class Player : PhysicsObject
	{
		protected Animation _idleAnimation;
		protected Animation _runAnimation;
		protected Animation _teleportAnimation;
		public bool IsMovingRight = true;
		public List<float> _experienceMultipliers;

		private Vector2 _startingPosition;
		private bool _isAi;
		public bool IsAi { get { return _isAi; } }
		public bool UsesMouseAndKeyboard { get; set; }

		public bool NotAcceptingControls { get; set; } // Used when viewing In-Game Menus... and when stunned?!

		public Player(Guid guid, Vector2 position, Vector2 hitbox, Group group, Level level, string name, Team team, bool isAi = false)
			: base(position, new Vector2(60, 120)/*hitbox*/, group, level, name)
		{
			AccelerationBase = new Vector2(.1f, .045f);
			_showHealthBar = true;
			_guid = guid;
			_isAi = isAi;

			NotAcceptingControls = false;

			MaxSpeedBase = new Vector2(AverageSpeed.X * (3.0f / 4.0f), AverageSpeed.Y);

			_objectType = GuiObjectType.Player;

			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();
			// Jumps.
			List<PlayerAbilityInfo> jumpInfos = new List<PlayerAbilityInfo>();
			jumpInfos.Add(AbilityBuilder.GetJumpAbility(this, 1.6f));

			abilities.Add(KnownAbility.Jump, jumpInfos);

			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();
			//PlayerAbilityInfo pai = AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/Fireball");
			//pai.Name = "Ball";
			//pai.AbilityAttributes = AbilityAttributes.ClickToDetonate | AbilityAttributes.Explosion | AbilityAttributes.PassWall | AbilityAttributes.PassCharacters;
			//pai.Modifier.SetSize(new Vector2(62, 62));
			//pai.Modifier.Speed = 7;
			//pai.Modifier.LevelUpMagicHitBoxBuffer(new Vector4(-10, -10, 20, 20));
			//pai.Modifier.LevelUpMagicKnockback(6);
			//elementalInfos.Add(pai);

			//PlayerAbilityInfo paiLeaf = AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/leaf", 0, 30);
			//paiLeaf.Name = "Daft";
			//paiLeaf.Cooldown = new TimeSpan(0, 0, 0, 0, 500);
			//paiLeaf.Modifier.Speed = 6f;
			//paiLeaf.Modifier.Damage = -40;
			//paiLeaf.Modifier.Element = new Tuple<Element, float>(Element.Plant, .25f);
			//paiLeaf.Modifier.SetSize(new Vector2(30, 30));
			//paiLeaf.Modifier.Type = ModifyType.Multiply;
			//paiLeaf.Modifier.Movement = new Vector2(.7f, 1f);
			//paiLeaf.Modifier.MaxSpeed = new Vector2(.7f, 1f);
			//ProjectileElementalMagicAbility mod = paiLeaf.Modifier as ProjectileElementalMagicAbility;
			//elementalInfos.Add(paiLeaf);

			//elementalInfos.Add(AbilityBuilder.GetShortRangeProjectileElementalAbility1(this));
			//elementalInfos.Add(AbilityBuilder.GetSurroundRangeElementalAbility1(this));
			//elementalInfos.Add(AbilityBuilder.GetPullMiscAbility(this));
			//elementalInfos.Add(AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/Fireball"));
			//elementalInfos.Add(AbilityBuilder.GetBasePushElementAbility(this, "Test/PushingWave"));
			//elementalInfos.Add(AbilityBuilder.GetSelfHealAbility(this));
			//elementalInfos.Add(AbilityBuilder.GetBlinkMiscAbility(this));


			abilities.Add(KnownAbility.Elemental, elementalInfos);

			// Miscellaneous Magic
			List<PlayerAbilityInfo> miscellaneousInfos = new List<PlayerAbilityInfo>();

			abilities.Add(KnownAbility.Miscellaneous, miscellaneousInfos);

			_abilityManager = new AbilityManager(this, abilities,
				AvailableButtons.RightTrigger | AvailableButtons.RightBumper | AvailableButtons.LeftTrigger | AvailableButtons.Third | AvailableButtons.Fourth);

			// Will Change when we level up stuff and things...
			//_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Ball"), AvailableButtons.RightBumper);
			//_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Pull"), AvailableButtons.LeftTrigger);
			//_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Heal"), AvailableButtons.LeftTrigger);

			_healthTotal = 1000;
			_healthCurrent = _healthTotal;

			// Temp Animations
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Sprites/PlayerSprites/TempPlayer/IdleFull"), 0.15f, true, 100, 200, (Size.X / 100));
			_runAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Sprites//PlayerSprites/TempPlayer/RunFull"), 0.1f, true, 100, 200, (Size.X / 100));
			_teleportAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Sprites//PlayerSprites/TempPlayer/Blob"), 0.1f, false, 50, 50, (Size.X / 50));

			_animator.Color = isAi ? new Color(50, 50, 50) : Color.Black;
			_animator.PlayAnimation(_idleAnimation);

			UsesMouseAndKeyboard = guid != Guid.Empty && MainGame.PlayerManager.PlayerInputMap[guid] is KeyboardUsableInputMap;
			Team = team;
			_startingPosition = position;
			RegenAmount = .05f;

			DrawImportant = 5;

			_animator.Color = TeamColorMap[team];

			_abilityManager.Experience = 200;

			PassiveExperienceGain = 0;
		}

		public void SelectPassiveExperienceGain(ExperienceGainChoice experienceChoice)
		{
			if (experienceChoice == ExperienceGainChoice.Early)
			{
				_experienceMultipliers = new List<float> { 4, 1 };
				_abilityManager.Experience = 0;
			}
			else if (experienceChoice == ExperienceGainChoice.Late)
				_experienceMultipliers = new List<float> { 1, 2, 2, 2, 1 };
		}

		public void SelectBaseAttack(BaseAttackChoice attackChoice)
		{
			PlayerAbilityInfo pai = null;
			if (attackChoice == BaseAttackChoice.Melee)
			{
				pai = AbilityBuilder.GetShortRangeMeleeElementalAbility1(this);
			}
			else if (attackChoice == BaseAttackChoice.ShortRange)
			{
				pai = AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/leaf", 0, 40);
				pai.Name = "Daft";
				pai.Cooldown = new TimeSpan(0, 0, 0, 0, 500);
				pai.Modifier.Speed = 6f;
				pai.Modifier.Damage = -40;
				pai.Modifier.Element = new Tuple<Element, float>(Element.Plant, .25f);
				pai.Modifier.SetSize(new Vector2(30, 30));
			}

			_abilityManager.AddKnownAbility(KnownAbility.Elemental, pai);
			_abilityManager.SetAbility(pai, AvailableButtons.RightTrigger);
		}

		public void SelectSelfUpgrade(SelfUpgradeChoice selfChoice)
		{
			if (selfChoice == SelfUpgradeChoice.Health)
			{
				_healthTotal = _healthTotal * 1.2f;
				HealthCurrent = _healthTotal;
				MaxSpeedBase = new Vector2(MaxSpeedBase.X * .9f, MaxSpeedBase.Y);
			}
			else if (selfChoice == SelfUpgradeChoice.Speed)
			{
				MaxSpeedBase = new Vector2(MaxSpeedBase.X * 1.15f, MaxSpeedBase.Y);
				_healthTotal = _healthTotal * .85f;
				HealthCurrent = _healthTotal;
			}
		}

		public override float GetXMovement()
		{
			return Movement.X * MaxSpeed.X; // For now, no "physics"
		}

		public override float GetYMovement()
		{
			return StopGravity ? 0f : AverageSpeed.Y; // Add gravity at a different time...
		}

		public override void PreUpdate(GameTime gameTime)
		{
			base.PreUpdate(gameTime);
			VerticalPass = Controls.AllControls[_guid].YMovement > .5;
		}
		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);
		}

		// If there are player specific modifiers, I will add these.
		//public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		//public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			if (_objState == GuiObjectState.Normal && Controls.AllControls != null && Controls.AllControls.ContainsKey(_guid))
			{
				PlayerControls controls = Controls.AllControls[_guid];
				if (IsStunned || NotAcceptingControls || controls.XMovement == 0)
					_animator.PlayAnimation(_idleAnimation);
				else
					_animator.PlayAnimation(_runAnimation);
			}
		}

		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime)
		{
			if (!NotAcceptingControls)
				Movement = new Vector2(Controls.AllControls[_guid].XMovement, Controls.AllControls[_guid].YMovement);
			if (Movement.X > 0)
				IsMovingRight = true;
			else if (Movement.X < 0)
				IsMovingRight = false;
		}

		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			_abilityManager.AddAbility(mb);
		}

		protected override void Died()
		{
			var otherPlayers = Level.Players.Where(p => _lastTargetHitBy != null && p.Value.Team == _lastTargetHitBy.Team);
			if (otherPlayers.Any())
			{
				int experience = 120 / otherPlayers.Count();
				foreach (var player in otherPlayers.Select(t => t.Value))
					player.GainExperience(experience);
			}

			if (IsAi)
				base.Died();
			else
			{
				_healthCurrent = _healthTotal;
				Position = _startingPosition;
				TickModifier respawn = new TickModifier(0, ModifyType.Add, this, new Tuple<Element, float>(Element.Normal, 0));
				respawn.PreventControls = true;
				HitByObject(null, respawn);
			}
			//Level.RemoveGuiObject(this);
		}

		public override Vector2 GetAim()
		{
			PlayerControls playerControls = GameStateManager.GetControlsForPlayer(this);
			return playerControls.GetAim(this);
		}

		protected override SpriteEffects GetCurrentSpriteEffects()
		{
			return IsMovingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		}

		public override void PlayTeleportAnimation()
		{
			_animator.PlayAnimation(_teleportAnimation);
		}

		public void UpdatePassiveExperienceGain(ExperienceGain newInterval)
		{
			if (_experienceMultipliers.Any())
			{
				PassiveExperienceGainMultiplier = _experienceMultipliers.FirstOrDefault();
				_experienceMultipliers.RemoveAt(0);
			}
			PassiveExperienceGain = newInterval.Amount;
		}
	}
}
