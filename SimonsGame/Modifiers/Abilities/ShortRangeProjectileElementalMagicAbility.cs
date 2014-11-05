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
	class ShortRangeProjectileElementalMagicAbility : CustomModifier
	{
		private Player _player;
		private int _tickTotal = 27; // number of ticks the ability will take place.
		private int _tickCount = 0; // Where we currently are in the ability.
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private ShortRangeProjectileMagic _testMagic;

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public ShortRangeProjectileElementalMagicAbility(Player p)
			: base(ModifyType.Multiply)
		{
			_player = p;
			isExpiredFunction = IsExpiredFunc;
			Movement = new Vector2(.7f, 1f);
		}

		public bool IsExpiredFunc(GameTime gameTime)
		{
			// When we just start, make the object!
			if (_tickCount == 0)
			{
				var playerControls = GameStateManager.GetControlsForPlayer(_player.Id);
				Vector2 playerAim = playerControls.GetAim(_player);
				Vector2 speed = playerAim * 7.5f;

				//For other type of magic.
				//// For now, a lot of things are hard coded...
				//float posY = _player.Center.Y - 20;
				//float posX = playerAim.X > 0 ? _player.Center.X : _player.Center.X - 80;
				// For now, a lot of things are hard coded...
				Vector2 projectileHitbox = new Vector2(40, 40);
				_testMagic = new ShortRangeProjectileMagic(_player.Center - (projectileHitbox / 2), projectileHitbox, Group.Passable, _player.Level, speed, _player);
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
		public override ModifierBase Clone()
		{
			ShortRangeProjectileElementalMagicAbility magic = new ShortRangeProjectileElementalMagicAbility(_player);
			return magic;
		}
	}
}
