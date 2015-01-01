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
	class LongRangeElementalMagicAbility : CustomModifier
	{
		private Player _player;
		private int _tickTotal = 81; // number of ticks the ability will take place. This takes one and a third of a second.
		private int _tickCount = 0; // Where we currently are in the ability.
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private Func<bool> _checkStopped;
		private LongRangeMagic _testMagic;
		public LongRangeMagic TestMagic { get { return _testMagic; } }

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public LongRangeElementalMagicAbility(Player p, Func<bool> checkStopped)
			: base(ModifyType.Add)
		{
			_player = p;
			_checkStopped = checkStopped;
			isExpiredFunction = IsExpiredFunc;

		}

		public bool IsExpiredFunc(GameTime gameTime)
		{
			// When we just start, make the object!
			if (_tickCount == 0)
			{
				PlayerControls playerControls = GameStateManager.GetControlsForPlayer(_player);
				Vector2 playerAim = playerControls.GetAim(_player);
				Vector2 speed = playerAim * 9.5f;
				// For now, a lot of things are hard coded...
				Vector2 projectileHitbox = new Vector2(45, 45);
				_testMagic = new LongRangeMagic(_player.Center - (projectileHitbox / 2), projectileHitbox, Group.Passable, _player.Level, speed, _player);
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
			{
				_testMagic.Detonate();
				_player.Level.RemoveGuiObject(_testMagic);
			}

			return isExpired;
		}
		public override ModifierBase Clone()
		{
			LongRangeElementalMagicAbility magic = new LongRangeElementalMagicAbility(_player, _checkStopped);
			return magic;
		}
	}
}
