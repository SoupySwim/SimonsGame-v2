using SimonsGame.GuiObjects.ElementalMagic;
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
	class ShortRangeProjectileElementalMagicAbility : AbilityModifier
	{
		private Player _player;
		private int _tickTotal = 27; // number of ticks the ability will take place.
		private int _tickCount = 0; // Where we currently are in the ability.
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private ShortRangeProjectileMagic _testMagic;

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public ShortRangeProjectileElementalMagicAbility(Player p, Tuple<Element, float> element)
			: base(ModifyType.Multiply, p, element)
		{
			_player = p;
			IsExpiredFunction = IsExpiredFunc;
			Movement = new Vector2(.7f, 1f);
			Speed = 7.5f;
			Damage = -40;
		}

		public bool IsExpiredFunc(GameTime gameTime)
		{
			// When we just start, make the object!
			if (_tickCount == 0)
			{
				PlayerControls playerControls = GameStateManager.GetControlsForPlayer(_player);
				Vector2 playerAim = playerControls.GetAim(_player);
				Vector2 speed = playerAim * Speed;

				//For other type of magic.
				//// For now, a lot of things are hard coded...
				//float posY = _player.Center.Y - 20;
				//float posX = playerAim.X > 0 ? _player.Center.X : _player.Center.X - 80;
				// For now, a lot of things are hard coded...
				Vector2 projectileHitbox = new Vector2(40, 40);
				_testMagic = new ShortRangeProjectileMagic(_player.Center - (projectileHitbox / 2), projectileHitbox, Group.Passable, _player.Level, speed, _player, Element, Damage);
				_player.Level.AddGuiObject(_testMagic);
			}
			if (_tickCount == _tickTotal)
			{
				_hasStopped = true;
				_hasReachedEnd = true;
			}
			_tickCount = Math.Min(_tickCount + 1, _tickTotal);

			bool isExpired = _hasStopped;

			if (isExpired)
			{
				_player.Level.RemoveGuiObject(_testMagic);
			}

			return isExpired;
		}

		public override void LevelUpMagicSpeed(float speed)
		{
			float oldSpeed = Speed;
			Speed += speed;
			_tickTotal = (int)(_tickTotal * (oldSpeed / Speed));
		}

		public override ModifierBase Clone(Guid id)
		{
			ShortRangeProjectileElementalMagicAbility magic = new ShortRangeProjectileElementalMagicAbility(_player, Element);
			magic._guid = id == Guid.Empty ? Guid.NewGuid() : id;
			return magic;
		}
		public override string GetRange() { return string.Format("{0:0.0}", ((Speed * _tickTotal) / 10)); }
	}
}
