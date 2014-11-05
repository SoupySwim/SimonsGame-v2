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
	class SurroundRangeElementalMagicAbility : CustomModifier
	{
		private Player _player;
		private int _tickTotal = 81; // number of ticks the ability will take place. This takes one and a third of a second.
		private int _tickCount = 0; // Where we currently are in the ability.
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private SurroundRangeMagic _testMagic;

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public SurroundRangeElementalMagicAbility(Player p)
			: base(ModifyType.Multiply)
		{
			_player = p;
			isExpiredFunction = IsExpiredFunc;

			Movement = new Vector2(1.15f, 1.75f);
		}

		public bool IsExpiredFunc(GameTime gameTime)
		{
			// When we just start, make the object!
			if (_tickCount == 0)
			{
				var playerControls = GameStateManager.GetControlsForPlayer(_player.Id);
				Vector2 playerAim = playerControls.GetAim(_player);
				Vector2 speed = new Vector2(9 * playerAim.X, 9 * playerAim.Y);
				// For now, a lot of things are hard coded...
				_testMagic = new SurroundRangeMagic(_player.Center - new Vector2(40, 40), new Vector2(80, 80), Group.Passable, _player.Level, _player);
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
			SurroundRangeElementalMagicAbility magic = new SurroundRangeElementalMagicAbility(_player);
			return magic;
		}
	}
}
