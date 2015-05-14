using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers
{
	public class CustomModifier : ModifierBase
	{
		protected Func<GameTime, bool> IsExpiredFunction;

		public CustomModifier(Func<GameTime, bool> isExpiredFunc, ModifyType type, MainGuiObject owner, Element element)
			: base(type, owner, element)
		{
		}
		public CustomModifier(ModifyType type, MainGuiObject owner, Element element)
			: base(type, owner, element)
		{
			IsExpiredFunction = (g) => false;
		}
		public override bool IsExpired(GameTime gameTime)
		{
			return IsExpiredFunction(gameTime);
		}
		public override void Reset()
		{
			//nothing needed here.  Must be handled in children.
		}
		public override ModifierBase Clone()
		{
			CustomModifier mod = new CustomModifier(IsExpiredFunction, Type, _owner, Element);
			if (Type == ModifyType.Add)
				mod = (CustomModifier)(mod + this);
			else
				mod = (CustomModifier)(mod * this);
			return mod;
		}

		public override long GetTickCount() { return 0; }
		public override void SetTickCount(long value) { }
	}
}
