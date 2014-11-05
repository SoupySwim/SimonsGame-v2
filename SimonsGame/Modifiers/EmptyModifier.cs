using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers
{
	public class EmptyModifier : ModifierBase
	{
		public EmptyModifier(ModifyType type)
			: base(type)
		{
		}
		public override bool IsExpired(GameTime gameTime)
		{
			return false;
		}
		public override void Reset()
		{
			// nothing happening here...
		}
		public override ModifierBase Clone()
		{
			return new EmptyModifier(Type);
		}
	}
}
