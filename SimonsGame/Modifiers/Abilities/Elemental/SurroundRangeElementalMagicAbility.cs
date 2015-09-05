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
		private int _tickTotal = 101;
		private int _tickCount = 0; // Where we currently are in the ability.
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private SurroundRangeMagic _testMagic;
		private PhysicsObject _character;

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public SurroundRangeElementalMagicAbility(PhysicsObject p, Tuple<Element, float> element)
			: base(ModifyType.Multiply, p, element)
		{
			IsExpiredFunction = IsExpiredFunc;

			Movement = new Vector2(1.8f, 1f);
			MaxSpeed = new Vector2(1.8f, 1f);
			_character = p;
		}

		public bool IsExpiredFunc(GameTime gameTime)
		{
			// When we just start, make the object!
			if (_tickCount == 0)
			{
				// For now, a lot of things are hard coded...
				_testMagic = new SurroundRangeMagic(_character.Center - new Vector2(40, 40), _character, new Vector2(80, 80), Group.Passable, _character.Level, _character);
				_character.Level.AddGuiObject(_testMagic);
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
				_character.Level.RemoveGuiObject(_testMagic);
			}

			return isExpired;
		}
		public override ModifierBase Clone(Guid id)
		{
			SurroundRangeElementalMagicAbility magic = new SurroundRangeElementalMagicAbility(_character, Element);
			magic._guid = id == Guid.Empty ? Guid.NewGuid() : id;
			return magic;
		}
		public override string GetRange() { return string.Format("{0:0.0}", ((_character.Bounds.W + _character.Bounds.Z) / 10.0)); }
		public override string GetSpeed() { return "0"; }
		public override string GetPower() { return "0"; }
		public override string GetHeal() { return "0"; }
	}
}
