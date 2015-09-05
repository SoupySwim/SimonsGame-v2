using SimonsGame.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.MainFiles
{
	public enum WinCondition
	{
		Unknown = -1,
		DefeatAllEnemies = 0,
		DefeatBase,
		TimeLimit,
		ReachGoal
	}
	public enum ScenarioType
	{
		Unknown = -1,
		SinglePlayerStory = 0,
		SinglePlayerChallenge,
		MultiPlayerChallenge,
		MultiPlayerCompetitive
	}

	public class ExperienceGain
	{
		public float Amount;
		public TimeSpan StartTime;
	}

	public class GameSettings
	{
		public bool AllowAIScreens { get; set; }
		public bool PauseStopsGame { get; set; }
		public string MapName { get; set; }
		public LevelFileMetaData LevelFileMetaData { get; set; }
		public List<ExperienceGain> ExperienceGainIntervals { get; set; }

		public GameSettings()
		{
			PauseStopsGame = true;
			AllowAIScreens = false;
			MapName = "Test Map";
			LevelFileMetaData = new LevelFileMetaData();
			ExperienceGainIntervals = new List<ExperienceGain>();
		}
	}
}
