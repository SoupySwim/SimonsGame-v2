using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
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
		public TimeModifier(TimeSpan gameTicks, ModifyType type, MainGuiObject owner)
			: base(type, owner)
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
			TimeModifier mod = new TimeModifier(_gameTimeLimit, Type, _owner);
			if (Type == ModifyType.Add)
				mod = (TimeModifier)(mod + this);
			else
				mod = (TimeModifier)(mod * this);
			return mod;
		}
	}
}
