using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Utility;

namespace SimonsGame.Modifiers
{
	public class TimeModifier : ModifierBase
	{
		private TimeSpan _gameTimeLimit;
		private TimeSpan _currentGameCount;
		public TimeModifier(TimeSpan gameTicks, ModifyType type, MainGuiObject owner, Element element)
			: base(type, owner, element)
		{
			_gameTimeLimit = gameTicks;
			_currentGameCount = TimeSpan.Zero;
		}
		public override bool IsExpired(GameTime gameTime)
		{
			_currentGameCount += gameTime.ElapsedGameTime;
			return _currentGameCount >= _gameTimeLimit;
		}
		public override void Reset()
		{
			_currentGameCount = TimeSpan.Zero;
		}
		public override ModifierBase Clone()
		{
			TimeModifier mod = new TimeModifier(_gameTimeLimit, Type, _owner, Element);
			if (Type == ModifyType.Add)
				mod = (TimeModifier)(mod + this);
			else
				mod = (TimeModifier)(mod * this);
			return mod;
		}
		public override long GetTickCount() { return (long)(_gameTimeLimit.Milliseconds * (60.0f / 1000)); }
		public override void SetTickCount(long value) { _gameTimeLimit = new TimeSpan(0, 0, 0, 0, (int)((1000.0f / 60) * value)); }
	}
}
