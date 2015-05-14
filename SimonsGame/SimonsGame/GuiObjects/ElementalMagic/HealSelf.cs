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

		// This type of modification MAY not do anything to the player.  In the future, it is set up to be possible :D
		public HealSelf(PhysicsObject character, Element element)
			: base(ModifyType.Add, character, element)
		{
			_character = character;
			IsExpiredFunction = IsExpiredFunc;
		}

		public bool IsExpiredFunc(GameTime gameTime)
		{
			TickModifier healSelf = new TickModifier(120, ModifyType.Add, _character, Element.Normal);
			healSelf.SetHealthTotal(150f / 120f);
			_character.HitByObject(_character, healSelf);
			return true;
		}
		public override ModifierBase Clone()
		{
			HealSelf magic = new HealSelf(_character, Element);
			return magic;
		}
		public override string GetRange() { return "0"; }
	}
}
