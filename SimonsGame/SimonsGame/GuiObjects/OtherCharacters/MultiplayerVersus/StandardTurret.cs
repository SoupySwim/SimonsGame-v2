﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class StandardTurret : PhysicsObject
	{
		private bool _isTurned = false;
		protected Animation _turretImage;
		public Vector4 _sensorBoundsBuffer = new Vector4();
		public Vector4 SensorBounds
		{
			get
			{
				_sensorBoundsBuffer.X = Position.X - Size.X * 3;
				_sensorBoundsBuffer.Y = Position.Y - Size.Y * 2;
				_sensorBoundsBuffer.W = Size.X * 6;
				_sensorBoundsBuffer.Z = Size.Y * 4;
				return _sensorBoundsBuffer;
			}
		}
		public StandardTurret(Vector2 position, Vector2 hitbox, Group group, Level level, Team team)
			: base(position, hitbox, group, level, "StandardTurret")
		{
			_showHealthBar = true;
			SwitchTeam(team);
			AdditionalGroupChange(group, group);
			_turretImage = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/Turret"), 1, false, 300, 500, new Vector2(Size.X / 300.0f, Size.Y / 500));
			_healthTotal = 4000;
			_healthCurrent = _healthTotal;
			_objectType = GuiObjectType.Structure;
			_animator.Color = _hitBoxColor;
			_animator.PlayAnimation(_turretImage);

			Dictionary<KnownAbility, List<PlayerAbilityInfo>> abilities = new Dictionary<KnownAbility, List<PlayerAbilityInfo>>();

			// Elemental Magic
			List<PlayerAbilityInfo> elementalInfos = new List<PlayerAbilityInfo>();
			elementalInfos.Add(AbilityBuilder.GetTurretAttackAbility(this));
			abilities.Add(KnownAbility.Elemental, elementalInfos);

			_abilityManager = new AbilityManager(this, abilities, AvailableButtons.None);
			IsMovable = false;
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
			return _isTurned ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			//return _team == Team.Team1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally; // Assumes right facing turrets.
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
			_turretImage.Scale = new Vector2(newSize.X / _turretImage.FrameWidth, newSize.Y / _turretImage.FrameHeight);
			//= new Animation(MainGame.Content.Load<Texture2D>("Test/Turret"), 1, false, 300, 500, (Size.X / 300.0f));
			base.ExtraSizeManipulation(newSize);
		}
		public override void SwitchTeam(Team newTeam)
		{
			_team = newTeam;
			_hitBoxColor = TeamColorMap[newTeam];
			_animator.Color = _hitBoxColor;
			//_isTurned = !(_team == Team.Team1 || _team == Team.Team3);
		}
		public override void SwitchDirections()
		{
			_isTurned = !_isTurned;
		}
		public override string GetDirectionalText()
		{
			return _isTurned ? "FacingLeft" : "FacingRight";
		}
		public override bool DidSwitchDirection()
		{
			return _isTurned;
		}
		public override Vector2 GetAim()
		{
			return new Vector2(_isTurned ? -1 : 1, 0);
		}
	}
}
