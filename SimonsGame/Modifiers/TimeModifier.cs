using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers
{
	public class TimeModifier : ModifierBase
	{
		private TimeSpan _gameTimeLimit;
		private TimeSpan _currentGameCount;
		public TimeModifier(TimeSpan gameTicks, ModifyType type)
			: base(type)
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
			TimeModifier mod = new TimeModifier(_gameTimeLimit, Type);
			if (Type == ModifyType.Add)
				mod = (TimeModifier)(mod + this);
			else
				mod = (TimeModifier)(mod * this);
			return mod;
		}
	}
}
