using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Utility.ObjectAnimations
{
	public class TickTimer
	{
		public int TickTotal { get; set; }
		public int TickCurrent { get; set; }
		private bool _doesLoop;
		private Action _callbackFunction;

		public TickTimer(int tickTotal, Action callback, bool doesLoop)
		{
			TickTotal = tickTotal;
			TickCurrent = tickTotal;
			_doesLoop = doesLoop;
			_callbackFunction = callback;
		}

		public void Update(GameTime gameTime)
		{
			TickCurrent = Math.Min(TickCurrent + 1, TickTotal);
			if (TickCurrent == TickTotal)
			{
				_callbackFunction();
				if (_doesLoop)
					Restart();
			}
		}

		public bool IsRunning()
		{
			return TickCurrent < TickTotal;
		}

		public void Restart()
		{
			TickCurrent = 0;
		}
	}
}
