using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.LevelMaker
{
	public class LevelSettings
	{
		public int SingleRoomDiameter;
		public int RoomDividerLength;
		public int TotalRooms;
		public int FloorDifficulty;
	}

	public class RandomLevelProgress
	{
		public LevelMakerGuiRoom[][] LevelRooms;
		private LevelSettings _levelSettings;
		public RandomLevelProgress(LevelSettings levelSettings)
		{
			_levelSettings = levelSettings;
			LevelRooms = new LevelMakerGuiRoom[levelSettings.TotalRooms][];
			for (int i = 0; i < LevelRooms.Count(); i++)
			{
				LevelRooms[i] = new LevelMakerGuiRoom[levelSettings.TotalRooms];
				//for (int j = 0; j < LevelRooms[i].Count(); j++)
				//{
				//	LevelRooms[i][j] = null;
				//}
			}
		}

		public void AddRoom(LevelMakerGuiRoom currentRoom, LevelMakerGuiRoomSettings roomSettings)
		{
			int i = roomSettings.Xcoordinate;
			int j = roomSettings.Ycoordinate;

			List<Point> surroundingRooms = new List<Point>();

			if (roomSettings.RoomType == RoomType.OneByOne)
			{
				LevelRooms[i][j] = currentRoom;
				surroundingRooms.Add(new Point(i - 1, j));
				surroundingRooms.Add(new Point(i + 1, j));
				surroundingRooms.Add(new Point(i, j - 1));
				surroundingRooms.Add(new Point(i, j + 1));
			}
			else if (roomSettings.RoomType == RoomType.TwoByOne)
			{
				LevelRooms[i][j] = currentRoom;
				LevelRooms[i + 1][j] = currentRoom;

				surroundingRooms.Add(new Point(i - 1, j));
				surroundingRooms.Add(new Point(i + 2, j));
				surroundingRooms.Add(new Point(i, j - 1));
				surroundingRooms.Add(new Point(i, j + 1));
				surroundingRooms.Add(new Point(i + 1, j - 1));
				surroundingRooms.Add(new Point(i + 1, j + 1));
			}
			else if (roomSettings.RoomType == RoomType.OneByTwo)
			{
				LevelRooms[i][j] = currentRoom;
				LevelRooms[i][j + 1] = currentRoom;
				surroundingRooms.Add(new Point(i - 1, j));
				surroundingRooms.Add(new Point(i + 1, j));
				surroundingRooms.Add(new Point(i - 1, j + 1));
				surroundingRooms.Add(new Point(i + 1, j + 1));
				surroundingRooms.Add(new Point(i, j - 1));
				surroundingRooms.Add(new Point(i, j + 2));
			}
			else if (roomSettings.RoomType == RoomType.TwoByTwo)
			{
				LevelRooms[i][j] = currentRoom;
				LevelRooms[i + 1][j] = currentRoom;
				LevelRooms[i][j + 1] = currentRoom;
				LevelRooms[i + 1][j + 1] = currentRoom;
				surroundingRooms.Add(new Point(i - 1, j));
				surroundingRooms.Add(new Point(i + 2, j));
				surroundingRooms.Add(new Point(i - 1, j + 1));
				surroundingRooms.Add(new Point(i + 2, j + 1));
				surroundingRooms.Add(new Point(i, j - 1));
				surroundingRooms.Add(new Point(i, j + 2));
				surroundingRooms.Add(new Point(i + 1, j - 1));
				surroundingRooms.Add(new Point(i + 1, j + 2));
			}

			foreach (Point point in surroundingRooms)
			{
				if (point.X >= 0 && point.X < _levelSettings.TotalRooms
					&& point.Y >= 0 && point.Y < _levelSettings.TotalRooms)
				{
					LevelMakerGuiRoom adjacentRoom = LevelRooms[point.X][point.Y];
					if (adjacentRoom != null)
					{
						Vector2 initialPosition = new Vector2(point.X, point.Y) * (_levelSettings.SingleRoomDiameter + _levelSettings.RoomDividerLength);
						if (point.X < i && (point.Y == j || (point.Y == j + 1 && (roomSettings.RoomType == RoomType.OneByTwo || roomSettings.RoomType == RoomType.TwoByTwo))))
						{
							adjacentRoom.MakeOpening(Direction2D.Right, _levelSettings, initialPosition);
							currentRoom.MakeOpening(Direction2D.Left, _levelSettings, initialPosition);
						}
						if (point.X > i && (point.Y == j || (point.Y == j + 1 && (roomSettings.RoomType == RoomType.OneByTwo || roomSettings.RoomType == RoomType.TwoByTwo))))
						{
							adjacentRoom.MakeOpening(Direction2D.Left, _levelSettings, initialPosition);
							currentRoom.MakeOpening(Direction2D.Right, _levelSettings, initialPosition);
						}
						if (point.Y < j && (point.X == i || (point.X == i + 1 && (roomSettings.RoomType == RoomType.TwoByOne || roomSettings.RoomType == RoomType.TwoByTwo))))
						{
							adjacentRoom.MakeOpening(Direction2D.Down, _levelSettings, initialPosition);
							currentRoom.MakeOpening(Direction2D.Up, _levelSettings, initialPosition);
						}
						if (point.Y > j && (point.X == i || (point.X == i + 1 && (roomSettings.RoomType == RoomType.TwoByOne || roomSettings.RoomType == RoomType.TwoByTwo))))
						{
							adjacentRoom.MakeOpening(Direction2D.Up, _levelSettings, initialPosition);
							currentRoom.MakeOpening(Direction2D.Down, _levelSettings, initialPosition);
						}
					}
				}

			}
		}
	}

	public enum RoomType
	{
		OneByOne,
		OneByTwo,
		TwoByOne,
		TwoByTwo
	}
	public class LevelMakerGuiRoomSettings
	{
		public RoomType RoomType;
		public Vector2 Position;
		public int Xcoordinate;
		public int Ycoordinate;
	}

	public class LevelMakerGuiRoom
	{
		public List<MainGuiObject> GuiObjects;
		public Dictionary<Direction2D, List<Platform>> Perimeter;
		private Level _level;

		public LevelMakerGuiRoom(Dictionary<Direction2D, List<Platform>> perimeter, Level level)
		{
			Perimeter = perimeter;
			GuiObjects = new List<MainGuiObject>();
			_level = level;
		}

		public void MakeOpening(Direction2D direction, LevelSettings levelSettings, Vector2 initialPosition)
		{
			List<Platform> platforms = Perimeter[direction];
			float distanceToPlatFromInitialLocation = (levelSettings.SingleRoomDiameter + (_level.PlatformDifference / 4) - levelSettings.RoomDividerLength) / 2.0f;

			if (direction == Direction2D.Up || direction == Direction2D.Down)
			{
				//Platform platform = platforms.FirstOrDefault(p => p.Position.X <= initialPosition.X && p.Position.X + p.Size.X >= initialPosition.X + levelSettings.RoomDividerLength);
				//platform.Size = new Vector2((platform.Size.X - _level.PlatformDifference) / 2.0f, platform.Size.Y);
				//Platform newPlatform = new Platform(new Vector2(platform.Position.X + platform.Size.X + _level.PlatformDifference, platform.Position.Y), platform.Size, Group.ImpassableIncludingMagic, _level);
				//Perimeter[direction].Add(newPlatform);
				//_level.AddGuiObject(newPlatform);


				Platform platform = platforms.FirstOrDefault(p => p.Position.X <= initialPosition.X && p.Position.X + p.Size.X >= initialPosition.X + levelSettings.RoomDividerLength);
				//platform.Size = new Vector2(platform.Size.X, (platform.Size.Y - _level.PlatformDifference) / 2.0f);
				float newPlatformSize = platform.Size.X;
				platform.Size = new Vector2((initialPosition.X + distanceToPlatFromInitialLocation) - platform.Position.X, platform.Size.Y);
				newPlatformSize = newPlatformSize - platform.Size.X - _level.PlatformDifference;
				Platform newPlatform = new Platform(new Vector2(platform.Position.X + platform.Size.X + _level.PlatformDifference, platform.Position.Y), new Vector2(newPlatformSize, platform.Size.Y), Group.ImpassableIncludingMagic, _level);
				Perimeter[direction].Add(newPlatform);
				_level.AddGuiObject(newPlatform);
			}
			else // if (direction == Direction2D.Left || direction == Direction2D.Right)
			{
				Platform platform = platforms.FirstOrDefault(p => p.Position.Y <= initialPosition.Y && p.Position.Y + p.Size.Y >= initialPosition.Y + levelSettings.RoomDividerLength);
				//platform.Size = new Vector2(platform.Size.X, (platform.Size.Y - _level.PlatformDifference) / 2.0f);
				float newPlatformSize = platform.Size.Y;
				platform.Size = new Vector2(platform.Size.X, (initialPosition.Y + distanceToPlatFromInitialLocation) - platform.Position.Y);
				newPlatformSize = newPlatformSize - platform.Size.Y - _level.PlatformDifference;
				Platform newPlatform = new Platform(new Vector2(platform.Position.X, platform.Position.Y + platform.Size.Y + _level.PlatformDifference), new Vector2(platform.Size.X, newPlatformSize), Group.ImpassableIncludingMagic, _level);
				Perimeter[direction].Add(newPlatform);
				_level.AddGuiObject(newPlatform);
			}
		}

		public void AddObject(MainGuiObject mgo)
		{
			_level.AddGuiObject(mgo);
			GuiObjects.Add(mgo);
		}
	}
}
