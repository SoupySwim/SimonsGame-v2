using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class StandardBase : PhysicsObject
	{
		protected Animation _baseImage;
		public StandardBase(Vector2 position, Vector2 hitbox, Group group, Level level, Team team)
			: base(position, hitbox, group, level, "StandardBase")
		{
			SwitchTeam(team);
			AdditionalGroupChange(group, group);
			_baseImage = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/Base"), 1, false, 400, 400, new Vector2(Size.X / 400.0f, Size.Y / 400.0f));
			_healthTotal = 6;
			_healthCurrent = _healthTotal;
			_objectType = GuiObjectType.Structure;
			_animator.Color = _hitBoxColor;
			_animator.PlayAnimation(_baseImage);
			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();

			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();
			//elementalInfos.Add(AbilityBuilder.GetTurretAttackAbility(this));
			//elementalInfos.Add(AbilityBuilder.GetLongRangeElementalAbility1(this));
			//elementalInfos.Add(AbilityBuilder.GetShortRangeMeleeElementalAbility1(this));
			//elementalInfos.Add(AbilityBuilder.GetShortRangeProjectileElementalAbility1(this));
			//elementalInfos.Add(AbilityBuilder.GetSurroundRangeElementalAbility1(this));

			//abilities.Add(KnownAbility.Elemental, elementalInfos);

			_abilityManager = new AbilityManager(this, abilities);
		}
		public override float GetXMovement()
		{
			return 0;
		}
		public override float GetYMovement()
		{
			return 0;
		}
		protected override SpriteEffects GetCurrentSpriteEffects()
		{
			return _team == Team.Team1 || _team == Team.Team3 ? SpriteEffects.None : SpriteEffects.FlipHorizontally; // Assumes right facing turrets.
		}
		public override void PreDraw(GameTime gameTime, SpriteBatch spriteBatch)
		{
		}
		public override void SetMovement(GameTime gameTime)
		{
		}
		public override void PostDraw(GameTime gameTime, SpriteBatch spriteBatch)
		{
		}
		public override void HitByObject(MainGuiObject mgo, Modifiers.ModifierBase mb)
		{
			_abilityManager.AddAbility(mb);
		}
		public override void ExtraSizeManipulation(Vector2 newSize)
		{
			_baseImage.Scale = new Vector2(Size.X / _baseImage.FrameWidth, Size.Y / _baseImage.FrameHeight);
			//= new Animation(MainGame.Content.Load<Texture2D>("Test/Turret"), 1, false, 300, 500, (Size.X / 300.0f));
			base.ExtraSizeManipulation(newSize);
		}
		public override void SwitchTeam(Team newTeam)
		{
			_team = newTeam;
			_hitBoxColor = TeamColorMap[newTeam];
			_animator.Color = _hitBoxColor;
		}
	}
}
