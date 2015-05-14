using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility;

namespace SimonsGame.Modifiers
{
	public class TickModifier : ModifierBase
	{
		private long _gameTickLimit;
		private long _currentGameTick;

		public TickModifier(long gameTicks, ModifyType type, MainGuiObject owner, Element element)
			: base(type, owner, element)
		{
			_gameTickLimit = gameTicks;
			_currentGameTick = 0;
		}
		public override bool IsExpired(GameTime gameTime)
		{
			_currentGameTick++;
			return _currentGameTick >= _gameTickLimit;
		}
		public override void Reset()
		{
			_currentGameTick = 0;
		}
		public override ModifierBase Clone()
		{
			TickModifier mod = new TickModifier(_gameTickLimit, Type, _owner, Element);
			if (Type == ModifyType.Add)
				mod = (TickModifier)(mod + this);
			else
				mod = (TickModifier)(mod * this);
			return mod;
		}
		public override long GetTickCount() { return _gameTickLimit; }
		public override void SetTickCount(long value) { _gameTickLimit = value; }
	}
}
