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

namespace SimonsGame.GuiObjects
{
	public class Player : PhysicsObject
	{
		protected Animation _idleAnimation;
		protected Animation _runAnimation;

		public static float Sprint3TestScore = 0;

		private Vector2 _startingPosition;
		private bool _isAi;
		public bool IsAi { get { return _isAi; } }
		public bool UsesMouseAndKeyboard { get; set; }

		public bool NotAcceptingControls { get; set; } // Used when viewing In-Game Menus... and when stunned?!

		public Player(Guid guid, Vector2 position, Vector2 hitbox, Group group, Level level, string name, Team team, bool isAi = false)
			: base(position, hitbox, group, level, name)
		{
			_showHealthBar = true;
			_guid = guid;
			_isAi = isAi;

			NotAcceptingControls = false;

			MaxSpeedBase = new Vector2(AverageSpeed.X, AverageSpeed.Y);

			_objectType = GuiObjectType.Player;

			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();
			// Jumps.
			List<PlayerAbilityInfo> jumpInfos = new List<PlayerAbilityInfo>();
			jumpInfos.Add(AbilityBuilder.GetJumpAbility(this, 1.5f));

			abilities.Add(KnownAbility.Jump, jumpInfos);

			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();
			PlayerAbilityInfo pai = AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/Fireball");
			pai.Name = "Ball";
			pai.AbilityAttributes = AbilityAttributes.ClickToDetonate | AbilityAttributes.Explosion | AbilityAttributes.PassWall | AbilityAttributes.PassCharacters;
			elementalInfos.Add(pai);
			elementalInfos.Add(AbilityBuilder.GetShortRangeMeleeElementalAbility1(this));

			pai = AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/leaf", 0, 20);
			pai.Name = "Daft";
			pai.Cooldown = new TimeSpan(0, 0, 0, 0, 50);
			pai.Modifier.Speed = 7.5f;
			pai.Modifier.Damage = -40;
			pai.Modifier.SetSize(new Vector2(40, 40));
			ProjectileElementalMagicAbility mod = pai.Modifier as ProjectileElementalMagicAbility;
			elementalInfos.Add(pai);
			//elementalInfos.Add(AbilityBuilder.GetShortRangeProjectileElementalAbility1(this));
			elementalInfos.Add(AbilityBuilder.GetSurroundRangeElementalAbility1(this));
			elementalInfos.Add(AbilityBuilder.GetBaseLongRangeElementAbility(this, "Test/Fireball"));
			elementalInfos.Add(AbilityBuilder.GetBasePushElementAbility(this, "Test/PushingWave"));
			elementalInfos.Add(AbilityBuilder.GetSelfHealAbility(this));
			//elementalInfos.Add(AbilityBuilder.GetBlinkMiscAbility(this));


			abilities.Add(KnownAbility.Elemental, elementalInfos);

			// Miscellaneous Magic
			List<PlayerAbilityInfo> miscellaneousInfos = new List<PlayerAbilityInfo>();

			abilities.Add(KnownAbility.Miscellaneous, miscellaneousInfos);

			_abilityManager = new AbilityManager(this, abilities,
				AvailableButtons.RightTrigger | AvailableButtons.RightBumper | AvailableButtons.LeftTrigger | AvailableButtons.LeftBumper);

			// Will Change when we level up stuff and things...
			_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Melee"), AvailableButtons.RightTrigger);
			_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Ball"), AvailableButtons.RightBumper);
			//_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Blink"), AvailableButtons.LeftTrigger);
			_abilityManager.SetAbility(elementalInfos.First(ei => ei.Name == "Heal"), AvailableButtons.LeftTrigger);

			_healthTotal = 1000;
			_healthCurrent = _healthTotal;

			// Temp Animations
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Sprites/PlayerSprites/TempPlayer/IdleFull"), 0.15f, true, 100, 200, (Size.X / 100));
			_runAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Sprites//PlayerSprites/TempPlayer/RunFull"), 0.1f, true, 100, 200, (Size.X / 100));

			_animator.Color = isAi ? new Color(50, 50, 50) : Color.Black;
			_animator.PlayAnimation(_idleAnimation);

			UsesMouseAndKeyboard = guid != Guid.Empty && MainGame.PlayerManager.PlayerInputMap[guid] is KeyboardUsableInputMap;
			Team = team;
			_startingPosition = position;
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
		// If there are player specific modifiers, I will add these.
		//public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		//public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			if (Controls.AllControls != null && Controls.AllControls.ContainsKey(_guid))
			{
				PlayerControls controls = Controls.AllControls[_guid];
				if (IsStunned || NotAcceptingControls || controls.XMovement == 0)
					_animator.PlayAnimation(_idleAnimation);
				else
					_animator.PlayAnimation(_runAnimation);
			}
		}
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime)
		{
			Movement = new Vector2(Controls.AllControls[_guid].XMovement, Controls.AllControls[_guid].YMovement);
		}

		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			_abilityManager.AddAbility(mb);
		}

		protected override void Died()
		{
			if (IsAi)
				base.Died();
			else
			{
				_healthCurrent = _healthTotal;
				Position = _startingPosition;
			}
			//Level.RemoveGuiObject(this);
		}
		public override Vector2 GetAim()
		{
			PlayerControls playerControls = GameStateManager.GetControlsForPlayer(this);
			return playerControls.GetAim(this);
		}
	}
}
