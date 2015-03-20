using Microsoft.Xna.Framework;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using SimonsGame.Modifiers;
using Microsoft.Xna.Framework.Graphics;

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

		public bool NotAcceptingControls { get; set; } // Used when viewing In-Game Menus.

		public Player(Guid guid, Vector2 position, Vector2 hitbox, Group group, Level level, string name, Team team, bool isAi = false)
			: base(position, hitbox, group, level, name)
		{
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
			elementalInfos.Add(AbilityBuilder.GetLongRangeElementalAbility1(this));
			//elementalInfos.Add(AbilityBuilder.GetShortRangeMeleeElementalAbility1(this));
			elementalInfos.Add(AbilityBuilder.GetShortRangeProjectileElementalAbility1(this));
			elementalInfos.Add(AbilityBuilder.GetSurroundRangeElementalAbility1(this));

			abilities.Add(KnownAbility.Elemental, elementalInfos);

			// Miscellaneous Magic
			List<PlayerAbilityInfo> miscellaneousInfos = new List<PlayerAbilityInfo>();

			abilities.Add(KnownAbility.Miscellaneous, miscellaneousInfos);

			_abilityManager = new AbilityManager(this, abilities);

			_healthTotal = 10;
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
			VerticalPass = GameStateManager.AllControls[_guid].YMovement > .5;
		}
		// If there are player specific modifiers, I will add these.
		//public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		//public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			if (GameStateManager.AllControls != null && GameStateManager.AllControls.ContainsKey(_guid))
			{
				PlayerControls controls = GameStateManager.AllControls[_guid];
				if (controls.XMovement == 0)
					_animator.PlayAnimation(_idleAnimation);
				else
					_animator.PlayAnimation(_runAnimation);
			}
		}
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime)
		{
			Movement = new Vector2(GameStateManager.AllControls[_guid].XMovement, GameStateManager.AllControls[_guid].YMovement);
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
	}
}
