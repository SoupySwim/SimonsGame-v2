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
	class LongRangeElementalMagicAbility : AbilityModifier
	{
		private Player _player;
		private int _tickTotal = 81; // number of ticks the ability will take place.
		private int _tickCount = 0; // Where we currently are in the ability.
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private Func<bool> _checkStopped;
		private LongRangeMagic _testMagic;
		public LongRangeMagic TestMagic { get { return _testMagic; } }

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public LongRangeElementalMagicAbility(Player p, Func<bool> checkStopped, Tuple<Element, float> element)
			: base(ModifyType.Add, p, element)
		{
			_player = p;
			_checkStopped = checkStopped;
			IsExpiredFunction = IsExpiredFunc;
			Speed = 9.5f;
			Damage = -200;
		}

		public bool IsExpiredFunc(GameTime gameTime)
		{
			// When we just start, make the object!
			if (_tickCount == 0)
			{
				PlayerControls playerControls = GameStateManager.GetControlsForPlayer(_player);
				Vector2 playerAim = playerControls.GetAim(_player);
				Vector2 speed = playerAim * Speed;
				// For now, a lot of things are hard coded...
				Vector2 projectileHitbox = new Vector2(45, 45);
				_testMagic = new LongRangeMagic(_player.Center - (projectileHitbox / 2), projectileHitbox, Group.Passable, _player.Level, speed, Damage, Element, _player);
				_player.Level.AddGuiObject(_testMagic);
			}
			if (_checkStopped() || _tickCount == _tickTotal)
			{
				_hasStopped = true;
				_hasReachedEnd = true;
			}
			_tickCount = Math.Min(_tickCount + 1, _tickTotal);

			bool isExpired = _hasStopped;

			if (isExpired)
				_testMagic.Detonate();

			return isExpired;
		}
		public override ModifierBase Clone(Guid id)
		{
			LongRangeElementalMagicAbility magic = new LongRangeElementalMagicAbility(_player, _checkStopped, Element);
			magic._guid = id == Guid.Empty ? Guid.NewGuid() : id;
			return magic;
		}
		public override string GetRange() { return string.Format("{0:0.0}", ((Speed * _tickTotal) / 10)); }
	}
}
