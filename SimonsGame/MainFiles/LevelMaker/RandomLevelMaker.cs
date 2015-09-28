using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Test;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.LevelMaker
{
	public class RandomLevelMaker
	{
		public Level Level;
		private LevelSettings _levelSettings;
		private RandomLevelProgress _progress;
		public static Level MakeRandomLevel(GameStateManager manager, LevelSettings levelSettings, int playerTotal = 1)
		{
			RandomLevelMaker maker = new RandomLevelMaker(manager, levelSettings, playerTotal);
			return maker.Level;
		}
		public RandomLevelMaker(GameStateManager manager, LevelSettings levelSettings, int playerTotal)
		{
			Level = new Level(new Vector2(levelSettings.TotalRooms * levelSettings.SingleRoomDiameter), manager);
			_progress = new RandomLevelProgress(levelSettings);
			_levelSettings = levelSettings;

			for (int ndx = 0; ndx < _levelSettings.TotalRooms; ndx++)
				MakeRoom();

			for (int playerNdx = 0; playerNdx < playerTotal; playerNdx++)
			{
				int playerCount = MainGame.PlayerManager.PlayerInputMap.Count(kv => !kv.Value.IsAi);
				Guid playerId = playerCount > playerNdx ? MainGame.PlayerManager.PlayerInputMap.Where(kv => !kv.Value.IsAi).ElementAt(playerNdx).Key
					: MainGame.PlayerManager.AddPlayer(TempControls.GetPlayerInput(playerCount));
				Level.AddGuiObject(new Player(playerId, Level.Size / 2, new Vector2(50, 100), Group.BothPassable, Level, "Player " + MainGame.PlayerManager.PlayerInfoMap.Count(), Team.Team1, false));
			}
			// ShortenLevel(); // Get rid of space on all ends of the level.
		}

		private void MakeRoom()
		{
			LevelMakerGuiRoomSettings roomSettings = GetNextRoomPosition();

			LevelMakerGuiRoom currentRoom = new LevelMakerGuiRoom(MakePerimeter(roomSettings), Level);

			_progress.AddRoom(currentRoom, roomSettings);
		}

		private Dictionary<Direction2D, List<Platform>> MakePerimeter(LevelMakerGuiRoomSettings roomSettings)
		{
			float tileLength = Level.PlatformDifference / 4;
			int TBmult = roomSettings.RoomType == RoomType.TwoByOne || roomSettings.RoomType == RoomType.TwoByTwo ? 2 : 1;
			int LRmult = roomSettings.RoomType == RoomType.OneByTwo || roomSettings.RoomType == RoomType.TwoByTwo ? 2 : 1;

			Vector2 rightOffset = roomSettings.Position + new Vector2(_levelSettings.SingleRoomDiameter * TBmult + (TBmult == 2 ? _levelSettings.RoomDividerLength : 0), 0);
			Vector2 bottomOffset = roomSettings.Position + new Vector2(0, _levelSettings.SingleRoomDiameter * LRmult + (LRmult == 2 ? _levelSettings.RoomDividerLength : 0));

			Dictionary<Direction2D, List<Platform>> perimeter = new Dictionary<Direction2D, List<Platform>>();
			perimeter[Direction2D.Left] = new List<Platform>() { new Platform(roomSettings.Position, new Vector2(tileLength, (_levelSettings.SingleRoomDiameter * LRmult) + tileLength + (LRmult == 2 ? _levelSettings.RoomDividerLength : 0)), Utility.Group.ImpassableIncludingMagic, Level) };
			perimeter[Direction2D.Right] = new List<Platform>() { new Platform(rightOffset, new Vector2(tileLength, (_levelSettings.SingleRoomDiameter * LRmult) + tileLength + (LRmult == 2 ? _levelSettings.RoomDividerLength : 0)), Utility.Group.ImpassableIncludingMagic, Level) };
			perimeter[Direction2D.Up] = new List<Platform>() { new Platform(roomSettings.Position, new Vector2((_levelSettings.SingleRoomDiameter * TBmult) + tileLength + (TBmult == 2 ? _levelSettings.RoomDividerLength : 0), tileLength), Utility.Group.ImpassableIncludingMagic, Level) };
			perimeter[Direction2D.Down] = new List<Platform>() { new Platform(bottomOffset, new Vector2((_levelSettings.SingleRoomDiameter * TBmult) + tileLength + (TBmult == 2 ? _levelSettings.RoomDividerLength : 0), tileLength), Utility.Group.ImpassableIncludingMagic, Level) };

			foreach (var kv in perimeter)
				Level.AddGuiObject(kv.Value.First());

			return perimeter;
		}

		private LevelMakerGuiRoomSettings GetNextRoomPosition()
		{
			LevelMakerGuiRoomSettings roomSettings = new LevelMakerGuiRoomSettings();
			int midI = _levelSettings.TotalRooms / 2;
			int midJ = midI;

			// This'll change to favor 1x1 the most and 2x2 the least.
			roomSettings.RoomType = (RoomType)MainGame.Randomizer.Next(4);
			roomSettings.RoomType = RoomType.TwoByTwo;

			int i = midI;
			int j = midJ;
			LevelMakerGuiRoom pickedRoom = _progress.LevelRooms[i][j];
			if (pickedRoom == null)
			{
				roomSettings.RoomType = RoomType.OneByOne;
			}
			else
			{
				// First check all areas off the middle section
				int walls = 4;
				while (walls > 0 && pickedRoom != null)
				{
					if (walls == 4) // Right
					{
						i = midI + 1;
						j = midJ;
					}
					if (walls == 3) // Bottom
					{
						i = midI;
						j = midJ + 1;
					}
					if (walls == 2) // Left
					{
						i = midI - 1;
						j = midJ;
					}
					if (walls == 1) // Top
					{
						i = midI;
						j = midJ - 1;
					}
					pickedRoom = _progress.LevelRooms[i][j];
					walls--;
				}

				// If all the walls are placed, it's a free for all.
				if (pickedRoom != null)
				{
				}
			}

			if (roomSettings.RoomType == RoomType.TwoByTwo)
			{
				bool leftBad = _progress.LevelRooms[i - 1][j] != null;
				bool rightBad = _progress.LevelRooms[i + 1][j] != null;
				bool upBad = _progress.LevelRooms[i][j - 1] != null;
				bool downBad = _progress.LevelRooms[i][j + 1] != null;
				bool leftUpBad = _progress.LevelRooms[i - 1][j - 1] != null;
				bool rightUpBad = _progress.LevelRooms[i + 1][j - 1] != null;
				bool leftDownBad = _progress.LevelRooms[i - 1][j + 1] != null;
				bool rightDownBad = _progress.LevelRooms[i + 1][j + 1] != null;

				HashSet<Point> availableSpots = new HashSet<Point>();

				if (!rightBad)
				{
					if (!rightUpBad && !upBad)
						availableSpots.Add(new Point(i, j - 1));
					if (!rightDownBad && !downBad)
						availableSpots.Add(new Point(i, j));
				}
				if (!leftBad)
				{
					if (!leftUpBad && !upBad)
						availableSpots.Add(new Point(i - 1, j - 1));
					if (!leftDownBad && !downBad)
						availableSpots.Add(new Point(i - 1, j));
				}

				if (availableSpots.Any())
				{
					Point pickedPoint = availableSpots.ElementAt(MainGame.Randomizer.Next(availableSpots.Count()));
					i = pickedPoint.X;
					j = pickedPoint.Y;
				}
				else
				{
					if (!leftBad || !rightBad)
						roomSettings.RoomType = RoomType.TwoByOne;
					else if (!upBad || !downBad)
						roomSettings.RoomType = RoomType.OneByTwo;
				}
			}


			if (roomSettings.RoomType == RoomType.TwoByOne)
			{
				// If it doesn't fit, move it
				if (_progress.LevelRooms[i + 1][j] != null)
					i = i - 1;

				// If it still doesn't fit, then make it smaller.
				if (_progress.LevelRooms[i][j] != null)
				{
					i = i + 1;
					roomSettings.RoomType = RoomType.OneByOne;
				}
			}

			if (roomSettings.RoomType == RoomType.OneByTwo)
			{
				// If it doesn't fit, move it
				if (_progress.LevelRooms[i][j + 1] != null)
					j = j - 1;


				// If it still doesn't fit, then make it smaller.
				if (_progress.LevelRooms[i][j] != null)
				{
					j = j + 1;
					roomSettings.RoomType = RoomType.OneByOne;
				}
			}

			roomSettings.Position = new Vector2(i * (_levelSettings.SingleRoomDiameter + _levelSettings.RoomDividerLength), j * (_levelSettings.SingleRoomDiameter + _levelSettings.RoomDividerLength));
			roomSettings.Xcoordinate = i;
			roomSettings.Ycoordinate = j;
			return roomSettings;
		}
	}
}
