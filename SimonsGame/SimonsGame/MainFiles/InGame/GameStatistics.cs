using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.MainFiles.InGame
{
	public class GameStatistics
	{
		private List<GameHighlight> _highlights;
		public List<GameHighlight> Highlights { get { return _highlights; } }
		public GameStatistics()
		{
			_highlights = new List<GameHighlight>();
		}
		public void AddHightlight(GameHighlight highlight)
		{
			_highlights.Add(highlight);
		}
	}

	public class GameHighlight
	{
		public TimeSpan TimeOccured;
		public string Description;
		public MainGuiObject Character;
	}
}
