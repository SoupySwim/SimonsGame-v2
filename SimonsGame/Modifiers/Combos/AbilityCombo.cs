using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers.Combos
{
	// May or may not use this class... 
	public class AbilityCombo
	{
		private List<ModifierBase> _modifiers;
		public List<ModifierBase> Modifiers { get { return _modifiers; } }

		//private ModifierBase _activeModifier;

		public List<ModifierBase> ActiveModifiers { get; set; }

		public AbilityCombo(List<ModifierBase> modifiers)
		{
			_modifiers = modifiers;
		}

		//public ModifierBase GetActiveModifier(GameTime gameTime)
		//{
		//	return _activeModifier;
		//}


	}
}
