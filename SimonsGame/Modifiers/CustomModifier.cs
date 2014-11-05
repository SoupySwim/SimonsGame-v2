using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers
{
	public class CustomModifier : ModifierBase
	{
		protected Func<GameTime, bool> isExpiredFunction;
		public CustomModifier(Func<GameTime, bool> isExpiredFunc, ModifyType type)
			: base(type)
		{
		}
		public CustomModifier(ModifyType type)
			: base(type)
		{
			isExpiredFunction = (g) => false;
		}
		public override bool IsExpired(GameTime gameTime)
		{
			return isExpiredFunction(gameTime);
		}
		public override void Reset()
		{
			//nothing needed here.  Must be handled in children.
		}
		public override ModifierBase Clone()
		{
			CustomModifier mod = new CustomModifier(isExpiredFunction, Type);
			if (Type == ModifyType.Add)
				mod = (CustomModifier)(mod + this);
			else
				mod = (CustomModifier)(mod * this);
			return mod;
		}
	}
}
