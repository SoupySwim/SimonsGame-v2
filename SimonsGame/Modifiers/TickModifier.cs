﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SimonsGame.Modifiers
{
	public class TickModifier : ModifierBase
	{
		private long gameTickLimit;
		private long currentGameTick;

		public TickModifier(long gameTicks, ModifyType type)
			: base(type)
		{
			gameTickLimit = gameTicks;
			currentGameTick = 0;
		}
		public override bool IsExpired(GameTime gameTime)
		{
			currentGameTick++;
			return currentGameTick >= gameTickLimit;
		}
		public override void Reset()
		{
			currentGameTick = 0;
		}
		public override ModifierBase Clone()
		{
			TickModifier mod = new TickModifier(gameTickLimit, Type);
			if (Type == ModifyType.Add)
				mod = (TickModifier)(mod + this);
			else
				mod = (TickModifier)(mod * this);
			return mod;
		}
	}
}