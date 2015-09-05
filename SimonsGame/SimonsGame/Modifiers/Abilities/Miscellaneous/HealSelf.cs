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
	class HealSelf : AbilityModifier
	{
		private PhysicsObject _character;
		public bool HasStopped { get { return true; } }
		private float _healAmount = 130f;
		private int _tickAmount = 240;

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public HealSelf(PhysicsObject character, Tuple<Element, float> element)
			: base(ModifyType.Add, character, element)
		{
			_character = character;
			IsExpiredFunction = IsExpiredFunc;
		}

		public bool IsExpiredFunc(GameTime gameTime)
		{
			TickModifier healSelf = new TickModifier(_tickAmount, ModifyType.Add, _character, Element);
			healSelf.SetHealthTotal(_healAmount / _tickAmount);
			_character.HitByObject(_character, healSelf);
			return true;
		}
		public override ModifierBase Clone(Guid id)
		{
			HealSelf magic = new HealSelf(_character, Element);
			magic._guid = id == Guid.Empty ? Guid.NewGuid() : id;
			return magic;
		}
		public override string GetRange() { return "0"; }

		public override void LevelUpSpecial(int type, float amount)
		{
			if (type == 0)
				_healAmount += amount;
			else if (type == 1)
				_tickAmount -= (int)amount;
			base.LevelUpSpecial(type, amount);
		}

	}
}
