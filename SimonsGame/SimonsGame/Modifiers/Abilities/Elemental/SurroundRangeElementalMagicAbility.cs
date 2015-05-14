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
	class SurroundRangeElementalMagicAbility : AbilityModifier
	{
		private Player _player;
		private int _tickTotal = 81; // number of ticks the ability will take place. This takes one and a third of a second.
		private int _tickCount = 0; // Where we currently are in the ability.
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private SurroundRangeMagic _testMagic;

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public SurroundRangeElementalMagicAbility(Player p, Element element)
			: base(ModifyType.Multiply, p, element)
		{
			_player = p;
			IsExpiredFunction = IsExpiredFunc;

			Movement = new Vector2(1.5f, 1f);
		}

		public bool IsExpiredFunc(GameTime gameTime)
		{
			// When we just start, make the object!
			if (_tickCount == 0)
			{
				PlayerControls playerControls = GameStateManager.GetControlsForPlayer(_player);
				Vector2 playerAim = playerControls.GetAim(_player);
				Vector2 speed = new Vector2(9 * playerAim.X, 9 * playerAim.Y);
				// For now, a lot of things are hard coded...
				_testMagic = new SurroundRangeMagic(_player.Center - new Vector2(40, 40), _player, new Vector2(80, 80), Group.Passable, _player.Level, _player);
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
			SurroundRangeElementalMagicAbility magic = new SurroundRangeElementalMagicAbility(_player, Element);
			return magic;
		}
		public override string GetRange() { return string.Format("{0:0.0}", ((_player.Bounds.W + _player.Bounds.Z) / 10.0)); }
		public override string GetSpeed() { return "0"; }
		public override string GetPower() { return "0"; }
		public override string GetHeal() { return "0"; }
	}
}
