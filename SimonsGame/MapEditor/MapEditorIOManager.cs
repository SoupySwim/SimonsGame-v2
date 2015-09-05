using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Threading;
using SimonsGame.Menu.MenuScreens;
using SimonsGame.MainFiles;
using SimonsGame.Test;
using SimonsGame.GuiObjects.Zones;
using SimonsGame.GuiObjects.OtherCharacters.Global;

namespace SimonsGame.MapEditor
{
	public enum OrderLevelByCategory
	{
		LastModifiedOn,
		Alphabetical,
		LevelArea,
		TeamCount,
	}
	public class MapEditorIOManager
	{
		public const int ASCENDING = 0;
		public const int DESCENDING = 1;
		public static object fileLock = new object();
		public static string LevelLocation = Environment.CurrentDirectory + @"\Levels\";
		public static string LevelMetaDataPathName = LevelLocation + @"LevelMetaData.xml"; // Perhaps later I will zip and lock this
		public static Dictionary<string, LevelFileMetaData> LoadedFileMetaData;
		public static bool IsLoaded = false; // We are loading until otherwise informed.
		public static OrderLevelByCategory CurrentLevelOrderByCategory = OrderLevelByCategory.LastModifiedOn;
		public static Dictionary<OrderLevelByCategory, Tuple<int, Func<LevelFileMetaData, object>>> OrderByActions = new Dictionary<OrderLevelByCategory, Tuple<int, Func<LevelFileMetaData, object>>>()
		{
			{OrderLevelByCategory.LastModifiedOn, new Tuple<int, Func<LevelFileMetaData, object>>(DESCENDING,fmd => fmd.LastModifiedOn) },
			{OrderLevelByCategory.Alphabetical,  new Tuple<int, Func<LevelFileMetaData, object>>(ASCENDING,fmd => fmd.LevelName) },
			{OrderLevelByCategory.LevelArea,  new Tuple<int, Func<LevelFileMetaData, object>>(ASCENDING,fmd => Math.Sqrt(Math.Pow(fmd.LevelSize.X,2)+Math.Pow(fmd.LevelSize.Y,2))) },
			{OrderLevelByCategory.TeamCount,  new Tuple<int, Func<LevelFileMetaData, object>>(ASCENDING,fmd => fmd.TeamCount) }
		};

		public static void Initialize()
		{
			Console.WriteLine(LevelLocation);
			Task.Factory.StartNew(() =>
			{
				int pathSize = LevelLocation.Count();
				bool doResave = false;
				XDocument levelMetaData = null;
				try
				{
					lock (fileLock)
					{
						levelMetaData = XDocument.Load(LevelMetaDataPathName);
					}
					//LevelMetaData = XDocument.Load(@"C:\Users\Simon\Desktop\test.xml"); // test time.
				}
				catch (Exception)// Don't care, if it's faulty, I make a new one.
				{
					levelMetaData = new XDocument();
				}
				var allLevelsInXml = levelMetaData.Descendants("Level");
				int allLevelsCount = allLevelsInXml.Count();
				LoadedFileMetaData = levelMetaData.Descendants("Level")
					.Select(xElem => LevelFileMetaData.FromXml(xElem))
					.Where(fmd => fmd != null && !fmd.LevelName.Contains("..") && !fmd.LevelRelativePath.Contains("..")) // Don't want to get hacksed.
					.GroupBy(fmd => fmd.LevelRelativePath + fmd.LevelName)
					.ToDictionary(group => group.Key, group => group.First());
				doResave = doResave || allLevelsCount != LoadedFileMetaData.Count(); // If we sifted out some levels, then we want to save.
				// I know that the file name is at least 6 characters at the end and is stored in "LevelLocation".
				List<string> files = Directory.GetFiles(LevelLocation, "*.level", SearchOption.AllDirectories).Select(f => f.Substring(pathSize, f.Count() - pathSize - 6)).ToList();

				List<string> intersection = LoadedFileMetaData.Keys.Intersect(files).ToList();
				List<string> deleteFiles = LoadedFileMetaData.Keys.Except(intersection).ToList();
				foreach (string fileName in deleteFiles)
					LoadedFileMetaData.Remove(fileName);
				List<string> addFiles = files.Except(intersection).ToList();
				// Load all levels right now.  Later I probably don't want to and will only load them when needed...
				foreach (string fileName in addFiles)
				{
					Level level = DeserializeLevelFromFile(fileName);
					int indexOfLastSlash = fileName.LastIndexOf('\\');
					LoadedFileMetaData.Add(fileName, new LevelFileMetaData()
					{
						LastModifiedOn = File.GetLastWriteTime(LevelLocation + fileName + ".level"),
						LevelName = fileName.Substring(indexOfLastSlash + 1),
						LevelRelativePath = fileName.Substring(0, indexOfLastSlash + 1),
						LevelSize = level.Size,
						TeamCount = level.GetAllGuiObjects().GroupBy(mgo => mgo.Team).Count(),
						ScenarioType = ScenarioType.Unknown,
						WinCondition = WinCondition.Unknown
					});
				}
				doResave = doResave || deleteFiles.Any() || addFiles.Any();

				if (doResave)
					SaveAllFiles(LoadedFileMetaData, LevelMetaDataPathName);
				IsLoaded = true;
			});
		}

		public static IEnumerable<LevelFileMetaData> GetLevels(int takeAmount = int.MaxValue, int skipAmount = 0)
		{
			if (IsLoaded)
			{
				var currentSort = OrderByActions[CurrentLevelOrderByCategory];
				if (currentSort.Item1 == ASCENDING)
					return LoadedFileMetaData.Values.OrderBy(currentSort.Item2).Skip(skipAmount).Take(takeAmount);// If we are done loading, then do our business, if not, then return what we know; nothing.
				return LoadedFileMetaData.Values.OrderByDescending(currentSort.Item2).Skip(skipAmount).Take(takeAmount);// If we are done loading, then do our business, if not, then return what we know; nothing.
			}
			return new LevelFileMetaData[0];
		}
		public static LevelFileMetaData GetMetadataForLevel(string levelFullName)
		{
			LevelFileMetaData metaData;
			if (LoadedFileMetaData.TryGetValue(levelFullName, out metaData))
				return metaData;
			return null;
		}

		public static void SerializeLevelToFile(Level level, LevelFileMetaData fileMetadata)
		{
			LevelBuilder levelBuilder = new LevelBuilder();
			levelBuilder.LevelWidth = level.Size.X;
			levelBuilder.LevelHeight = level.Size.Y;
			levelBuilder.PlatformDiffernce = level.PlatformDifference;
			levelBuilder.Objects = new List<GuiObjectStore>();
			levelBuilder.Objects.AddRange(level.GetAllGuiObjects().Select(mgo => mgo.GetGuiObjectStore()));
			levelBuilder.Objects.AddRange(level.GetAllTeleporters().Select(mgo => mgo.GetGuiObjectStore()));
			levelBuilder.Objects.AddRange(level.GetAllZones().Select(kv => kv.Value.GetGuiObjectStore()));

			IFormatter formatter = new BinaryFormatter();
			if (!Directory.Exists(LevelLocation))
				Directory.CreateDirectory(LevelLocation);
			Stream stream = new FileStream(LevelLocation + fileMetadata.FullName + ".level", FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, levelBuilder);
			stream.Close();

			fileMetadata.LastModifiedOn = DateTime.Now;
			LoadedFileMetaData[fileMetadata.FullName] = fileMetadata;
			LoadedFileMetaData[fileMetadata.FullName].LevelSize = level.Size;
			SaveAllFiles(LoadedFileMetaData, LevelMetaDataPathName);
		}

		public static Level DeserializeLevelFromFile(string mapName, GameStateManager manager = null)
		{
			IFormatter formatter = new BinaryFormatter();
			Stream fromStream = new FileStream(LevelLocation + mapName + ".level", FileMode.Open, FileAccess.Read, FileShare.Read);
			LevelBuilder levelBuilder = (LevelBuilder)formatter.Deserialize(fromStream);
			fromStream.Close();
			Level level = new Level(new Vector2(levelBuilder.LevelWidth, levelBuilder.LevelHeight), manager, (int)levelBuilder.PlatformDiffernce);
			MainGame.PlayerManager.RemoveAllPlayers();
			levelBuilder.Objects.ForEach(os => level.AddGuiObject(os.GetObject(level)));

			return level;
		}

		[Serializable]
		public class LevelBuilder
		{
			public float LevelWidth { get; set; }
			public float LevelHeight { get; set; }
			public float PlatformDiffernce { get; set; }
			public List<GuiObjectStore> Objects { get; set; }
		}


		public static void SaveAllFiles(Dictionary<string, LevelFileMetaData> loadedFiles, string fullPath)
		{
			XDocument xDoc = new XDocument();
			XElement xLevels = new XElement("Levels");
			foreach (var kv in loadedFiles)
			{
				LevelFileMetaData fmd = kv.Value;
				xLevels.Add(fmd.ToXml());
			}
			xDoc.Add(xLevels);

			lock (fileLock)
			{
				xDoc.Save(fullPath);
			}
		}
	}
	public class LevelFileMetaData
	{
		public LevelFileMetaData() { }

		public LevelFileMetaData(LevelFileMetaData levelFileMetaData)
		{
			LastModifiedOn = levelFileMetaData.LastModifiedOn;
			LevelName = levelFileMetaData.LevelName;
			LevelRelativePath = levelFileMetaData.LevelRelativePath;
			LevelSize = levelFileMetaData.LevelSize;
			TeamCount = levelFileMetaData.TeamCount;
			WinCondition = levelFileMetaData.WinCondition;
			ScenarioType = levelFileMetaData.ScenarioType;
		}
		public string FullName { get { return LevelRelativePath + LevelName; } } // Path + level name, not .level
		public DateTime LastModifiedOn { get; set; }
		public string LevelName { get; set; } // Just the map name, not ".level"
		public string LevelRelativePath { get; set; } // So two levels can be named the same thing IF they are in separate paths.
		public Vector2 LevelSize { get; set; }
		public int TeamCount { get; set; }
		public WinCondition WinCondition { get; set; }
		public ScenarioType ScenarioType { get; set; }
		public XElement ToXml()
		{
			XElement elem = new XElement("Level",
				new XAttribute("LastModifiedOn", LastModifiedOn.ToString()),
				new XAttribute("LevelName", LevelName),
				new XAttribute("LevelRelativePath", LevelRelativePath),
				new XAttribute("TeamCount", TeamCount),
				new XAttribute("WinCondition", (int)WinCondition),
				new XAttribute("ScenarioType", (int)ScenarioType),
				new XAttribute("X", LevelSize.X),
				new XAttribute("Y", LevelSize.Y));
			return elem;
		}
		public static LevelFileMetaData FromXml(XElement metaData)
		{
			try
			{
				DateTime lastModifiedOn = DateTime.Parse(metaData.Attribute("LastModifiedOn").Value);
				string levelName = metaData.Attribute("LevelName").Value;
				string levelRelativePath = metaData.Attribute("LevelRelativePath").Value;
				int teamCount = int.Parse(metaData.Attribute("TeamCount").Value);
				WinCondition winCondition = (WinCondition)int.Parse(metaData.Attribute("WinCondition").Value);
				ScenarioType scenarioType = (ScenarioType)int.Parse(metaData.Attribute("ScenarioType").Value);
				Vector2 levelSize = new Vector2(float.Parse(metaData.Attribute("X").Value), float.Parse(metaData.Attribute("Y").Value));
				return new LevelFileMetaData()
					{
						LastModifiedOn = lastModifiedOn,
						LevelName = levelName,
						LevelRelativePath = levelRelativePath,
						LevelSize = levelSize,
						TeamCount = teamCount,
						WinCondition = winCondition,
						ScenarioType = scenarioType
					};
			}
			catch (Exception) { return null; } // If it's even a little off, We delete it!
		}
		public override bool Equals(object obj)
		{
			if (obj != null && obj is LevelFileMetaData)
			{
				LevelFileMetaData levelFileMetaData = (LevelFileMetaData)obj;
				return LevelName == levelFileMetaData.LevelName && LevelRelativePath == levelFileMetaData.LevelRelativePath;
			}
			return false;
		}
		public static bool operator ==(LevelFileMetaData a, LevelFileMetaData b) { return a.Equals(b); }
		public static bool operator !=(LevelFileMetaData a, LevelFileMetaData b) { return !a.Equals(b); }
		public override int GetHashCode() { return base.GetHashCode(); }
	}
	[Serializable]
	public class GuiObjectStore
	{
		public GuiObjectClass Class { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }
		public Group Group { get; set; }
		public Team Team { get; set; }
		public float MaxSpeedX { get; set; }
		public float MaxSpeedY { get; set; }
		public int HealthTotal { get; set; }
		public Dictionary<ButtonType, int> ExtraSavedInformation { get; set; }

		public MainGuiObject GetObject(Level level)
		{
			MainGuiObject mgo = GetBaseObject(level);
			foreach (var kv in ExtraSavedInformation)
				mgo.SetSpecialValue(kv.Key, kv.Value);
			//if (ShouldSwitchDirection)
			//	baseObject.SwitchDirections();
			if (MaxSpeedX > 0 || MaxSpeedY > 0)// && mgo.IsMovable) // May want to remove for special cases, and I did.
				mgo.MaxSpeedBase = new Vector2(MaxSpeedX, MaxSpeedY);
			if (HealthTotal > 0)
			{
				mgo.SetHealthTotal(HealthTotal);
				mgo.HealthCurrent = HealthTotal;
			}
			return mgo;
		}
		private MainGuiObject GetBaseObject(Level level)
		{
			Vector2 position = new Vector2(X, Y);
			Vector2 size = new Vector2(Width, Height);
			MainGuiObject mgo;
			switch (Class)
			{
				case GuiObjectClass.Platform:
					Platform platform = new Platform(position, size, Group, level);
					platform.Team = Team;
					return platform;
				case GuiObjectClass.MovingPlatform:
					return new MovingPlatform(position, size, Group, level, true, (int)level.PlatformDifference, false);
				case GuiObjectClass.Player:
					int playerCount = MainGame.PlayerManager.PlayerInputMap.Count(kv => !kv.Value.IsAi);
					Guid playerId = MainGame.PlayerManager.AddPlayer(TempControls.GetPlayerInput(playerCount));
					return new Player(playerId, position, size, Group, level, "Player" + MainGame.PlayerManager.PlayerInputMap.Count(), Team, false);
				case GuiObjectClass.AIPlayer:
					Guid id = MainGame.PlayerManager.AddPlayer(new UsableInputMap() { IsAi = true });
					return new Player(id, position, size, Group, level, "Player " + MainGame.PlayerManager.PlayerInputMap.Count(), Team, true);
				case GuiObjectClass.HealthCreep:
					return new HealthCreep(position, size, Group, level, true, 0, (int)level.Size.X);
				case GuiObjectClass.ElementalCharacter:
					return new ElementalCharacter(position, size, Group, level);
				case GuiObjectClass.MinionNormal:
					mgo = new MinionNormal(position, size, Group, level, true);
					mgo.Team = Team;
					return mgo;
				case GuiObjectClass.MinionLarge:
					mgo = new MinionLarge(position, size, Group, level, true);
					mgo.Team = Team;
					return mgo;
				case GuiObjectClass.MinionFlying:
					mgo = new MinionFlying(position, size, Group, level, true);
					mgo.Team = Team;
					return mgo;
				case GuiObjectClass.NeutralCreep:
					return new NeutralCreep(position, size, Group, level);
				case GuiObjectClass.FlyingCreature:
					return new FlyingCreature(position, size, Group, level);
				case GuiObjectClass.LargeCreep:
					return new LargeCreep(position, size, Group, level);
				case GuiObjectClass.CreepBoss:
					return new CreepBoss(position, size, Group, level);
				case GuiObjectClass.WallRunner:
					mgo = new WallRunner(position, size, level, true);
					mgo.Team = Team;
					return mgo;
				case GuiObjectClass.StandardTurret:
					return new StandardTurret(position, size, level, Team);
				case GuiObjectClass.StandardBase:
					return new StandardBase(position, size, level, Team);
				case GuiObjectClass.FinishLineFlagPole:
					return new FinishLineFlagPole(position, size, Group, level);
				case GuiObjectClass.Block:
					return new Block(position, size, Group, level);
				case GuiObjectClass.ObjectSpawner:
					mgo = new ObjectSpawner(position, size, Group, level);
					mgo.Team = Team;
					return mgo;
				case GuiObjectClass.Ladder:
					return new Ladder(position, size, level);
				case GuiObjectClass.Button:
					return new Button(position, size, level);
				case GuiObjectClass.GuiFunction:
					return new GuiFunction(position, level);
				case GuiObjectClass.GuiIfClause:
					return new GuiIfClause(position, size, level);
				case GuiObjectClass.GuiThenClause:
					return new GuiThenClause(position, size, level);
				case GuiObjectClass.JumpPad:
					return new JumpPad(position, size, level);
				case GuiObjectClass.HealthPack:
					return PowerUpBuilder.GetHealthPackPU(position, size, level);
				case GuiObjectClass.SuperSpeed:
					return PowerUpBuilder.GetSpeedUpPU(position, size, level);
				case GuiObjectClass.SuperJump:
					return PowerUpBuilder.GetSuperJumpPU(position, size, level);
				case GuiObjectClass.Teleporter:
					return new Teleporter(position, size, level);
				case GuiObjectClass.Spike:
					return new Spike(position, size, Group, level);
				case GuiObjectClass.LockedBarrier:
					return new LockedBarrier(position, size, level);
				case GuiObjectClass.SmallKeyObject:
					return new SmallKeyObject(position, size, level);
				case GuiObjectClass.AbilityObject:
					return PowerUpBuilder.GetBlinkAbilityObject(position, size, level);
				case GuiObjectClass.JungleCreepZone:
					return new JungleCreepZone(position, size, level);
				case GuiObjectClass.BehaviorZone:
					return new BehaviorZone(position, size, level, Team);

			}
			return null;
		}
	}
}
