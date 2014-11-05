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
	class ShortRangeMeleeElementalMagicAbility : CustomModifier
	{
		private Player _player;
		private int _tickTotal = 12; // number of ticks the ability will take place. This takes third of a second.
		private int _tickCount = 0; // Where we currently are in the ability.
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private ShortRangeMeleeMagic _testMagic;

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public ShortRangeMeleeElementalMagicAbility(Player p)
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

				// For now, a lot of things are hard coded...
				float posY = _player.Center.Y - 20;
				float posX = playerAim.X > 0 ? _player.Center.X : _player.Center.X - 80;
				_testMagic = new ShortRangeMeleeMagic(new Vector2(posX, posY), new Vector2(80, 40), Group.Passable, _player.Level, playerAim.X > 0);
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
			ShortRangeMeleeElementalMagicAbility magic = new ShortRangeMeleeElementalMagicAbility(_player);
			return magic;
		}
	}
}
