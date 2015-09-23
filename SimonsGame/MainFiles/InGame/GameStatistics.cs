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
		public Dictionary<Guid, GameAccomplishment> Accomplishments;
		public GameStatistics(Level level)
		{
			_highlights = new List<GameHighlight>();
			Accomplishments = level.Players.ToDictionary(kv => kv.Key, kv => new GameAccomplishment());
		}
		public void AddHightlight(GameHighlight highlight)
		{
			_highlights.Add(highlight);
		}

		public void DealtDamageToStructure(Guid id, float amount)
		{
			Accomplishments[id].DamageToStructures += amount;
		}

		public void DealtDamageToCharacter(Guid id, float amount)
		{
			Accomplishments[id].DamageToCharacters += amount;
		}

		public void HealedDamage(Guid id, float amount)
		{
			Accomplishments[id].DamageHealed += amount;
		}

		public void DestroyedPlayer(Guid id)
		{
			Accomplishments[id].PlayerDestroyed++;
		}

		public void DestroyedDriod(Guid id)
		{
			Accomplishments[id].DroidDestroyed++;
		}
	}

	public class GameAccomplishment
	{
		public int DroidDestroyed = 0;
		public int PlayerDestroyed = 0;
		public float DamageToCharacters = 0;
		public float DamageToStructures = 0;
		public float DamageHealed = 0;
	}

	public class GameHighlight
	{
		public TimeSpan TimeOccured;
		public string Description;
		public MainGuiObject Character;
	}
}
