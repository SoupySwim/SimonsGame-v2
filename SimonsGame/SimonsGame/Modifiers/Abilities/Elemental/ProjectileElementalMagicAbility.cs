﻿using SimonsGame.GuiObjects.ElementalMagic;
using Microsoft.Xna.Framework;
using SimonsGame;
using SimonsGame.GuiObjects;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers.Abilities
{
	public class ProjectileElementalMagicAbility : AbilityModifier
	{
		private PhysicsObject _character;
		private int _tickTotal; // number of ticks the ability will take place.
		private int _tickCount = 0; // Where we currently are in the ability.
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private PlayerMagicObject _testMagic;
		public PlayerMagicObject TestMagic { get { return _testMagic; } }
		private Animation _animation;
		public Guid PlayerInfoId { get; private set; }

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public ProjectileElementalMagicAbility(PhysicsObject p, AbilityAttributes abilityAttributes, Animation animation, Element element, Guid playerInfoId, float speed = 9.5f, float damage = -200, int duration = 81)
			: base(ModifyType.Add, p, element)
		{
			_character = p;
			IsExpiredFunction = IsExpiredFunc;
			Speed = speed;
			Damage = damage;
			_animation = animation;
			AbilityAttributes = abilityAttributes;
			PlayerInfoId = playerInfoId;
			_tickTotal = duration;
		}
		public override void LevelUpMagic(float speed, float damage, AbilityAttributes newAbilityAttributes = AbilityAttributes.None)
		{
			base.LevelUpMagic(speed, damage, newAbilityAttributes);
			PlayerAbilityInfo pai = _character.AbilityManager.GetAbilityInfo(PlayerInfoId);
			pai.AbilityAttributes = newAbilityAttributes;
		}
		public void LevelUpMagicDuration(int tickTotal)
		{
			int _tickTotal = tickTotal;
		}

		public bool IsExpiredFunc(GameTime gameTime)
		{
			// When we just start, make the object!
			if (_tickCount == 0)
			{
				Vector2 characterAim = _character.GetAim();
				if (AbilityAttributes.HasFlag(AbilityAttributes.OnlyHorizontal))
					characterAim = new Vector2(characterAim.X < 0 ? -1 : 1, 0); // If you only fire the ability horizontally, then make it that way, yo.
				Vector2 speed = characterAim * Speed;

				_testMagic = new ProjectileElementalMagic(_character.Center - (_animation.ActualSize / 2), _animation.ActualSize,
					AbilityAttributes.HasFlag(AbilityAttributes.CanPush) ? Group.BothPassable : Group.Passable,
					_character.Level, speed, _character, Element, Damage, "name", _animation, this);
				_character.Level.AddGuiObject(_testMagic);
			}
			if (isStopped() || _tickCount == _tickTotal)
			{
				_hasStopped = true;
				_hasReachedEnd = true;
			}
			_tickCount = Math.Min(_tickCount + 1, _tickTotal);

			if (_hasStopped)
				_testMagic.Expire();

			return _hasStopped;
		}
		public override ModifierBase Clone()
		{
			ProjectileElementalMagicAbility magic = new ProjectileElementalMagicAbility(_character, AbilityAttributes, _animation, Element, PlayerInfoId, Speed, Damage, _tickTotal);
			magic.SetTickCount(GetTickCount());
			return magic;
		}
		private bool isStopped()
		{
			// If the user is clicking and there is click to detonate, then detonate the ability!
			return AbilityAttributes.HasFlag(AbilityAttributes.ClickToDetonate) && Controls.PressedDown(_character.Id, _character.AbilityManager.AbilityButtonMap[PlayerInfoId]);
		}
		public override string GetRange() { return string.Format("{0:0.0}", ((Speed * _tickTotal) / 10)); }

		public override void SetSize(Vector2 size)
		{
			_animation.Scale = new Vector2(size.X / _animation.FrameWidth, size.Y / _animation.FrameHeight);
		}
		public override long GetTickCount() { return _tickTotal; }
		public override void SetTickCount(long value) { _tickTotal = (int)value; }
	}
}
