using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.MainFiles
{
	public class GameSettings
	{
		public bool AllowAIScreens { get; set; }
		public bool PauseStopsGame { get; set; }
		public GameSettings()
		{
			PauseStopsGame = true;
			AllowAIScreens = false;
		}
	}
}
