using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Extensions;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// To be implemented later.

namespace SimonsGame.GuiObjects
{
	/// <summary>
	/// Level will contain all of the GUI objects in the game.
	/// It will go through every object and "Draw" and "Update" them.
	/// </summary>
	public class Level
	{

		// This will store the objects that make up the environment.
		private Dictionary<Group, List<MainGuiObject>> _environmentObjects;
		private Dictionary<Group, List<MainGuiObject>> _characterObjects;

		// This will store what players are currently in the environment.
		private Dictionary<Guid, Player> _players;
		public Dictionary<Guid, Player> Players { get { return _players; } set { _players = value; } }

		private List<LevelAnimation> _levelAnimations;

		// Used to tell how far "one" block in the level is.
		public float PlatformDifference { get; set; }


		private GameStateManager _gameStateManager;
		public GameStateManager GameStateManager { get { return _gameStateManager; } }
		public Vector2 Size { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size"> Determines the viewport of the Level (What is displayed on the screen). </param>
		public Level(Vector2 size, GameStateManager gameStateManager, int baseHeight)
		{
			_environmentObjects = new Dictionary<Group, List<MainGuiObject>>();
			_characterObjects = new Dictionary<Group, List<MainGuiObject>>();
			_players = new Dictionary<Guid, Player>();

			Size = size;

			_gameStateManager = gameStateManager;

			_levelAnimations = new List<LevelAnimation>();
			PlatformDifference = baseHeight;
		}
		public void AddPlayer(Player player)
		{
			if (_players.ContainsKey(player.Id))
				_players.Remove(player.Id);
			_players.Add(player.Id, player);
		}
		public void AddGuiObject(MainGuiObject guiObject)
		{
			if (guiObject.ObjectType == GuiObjectType.Environment || guiObject.ObjectType == GuiObjectType.Structure)
				AddEnvironmentObject(guiObject);
			else if (guiObject is Player)
				AddPlayer((Player)guiObject);
			else
				AddCharacterObject(guiObject);
		}
		private void AddEnvironmentObject(MainGuiObject guiObject, Group? g = null)
		{
			Group group = g == null ? guiObject.Group : (Group)g;
			if (_environmentObjects.ContainsKey(group))
				_environmentObjects[group].Add(guiObject);
			else
				_environmentObjects.Add(group, new List<MainGuiObject>() { guiObject });
		}
		private void AddCharacterObject(MainGuiObject guiObject, Group? g = null)
		{
			Group group = g == null ? guiObject.Group : (Group)g;
			if (_characterObjects.ContainsKey(group))
				_characterObjects[group].Add(guiObject);
			else
				_characterObjects.Add(group, new List<MainGuiObject>() { guiObject });
		}

		// Update will call Update on all of the objects within the level.
		public void Update(GameTime gameTime)
		{
			// Update all the platforms
			foreach (var kv in _environmentObjects)
			{
				kv.Value.ForEach(p => p.Update(gameTime));
			}
			// Update all the platforms
			foreach (var kv in _characterObjects)
			{
				kv.Value.ForEach(p => p.Update(gameTime));
			}
			// Update all the players.
			Players.Values.Where(p => !p.NotAcceptingControls).ToList().ForEach(p => p.Update(gameTime));
		}

		// Draw will call Draw on all of the objects within the level.
		//public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		//{
		//	// Draw all the players.
		//	Players.Values.ToList().ForEach(p => p.Draw(gameTime, spriteBatch));

		//	// Draw all the platforms
		//	foreach (var kv in _environmentObjects)
		//	{
		//		kv.Value.ForEach(p => p.Draw(gameTime, spriteBatch));
		//	}
		//	foreach (var kv in _characterObjects)
		//	{
		//		kv.Value.ForEach(p => p.Draw(gameTime, spriteBatch));
		//	}
		//	foreach (LevelAnimation la in _levelAnimations.ToList())
		//	{
		//		la.Draw(gameTime, spriteBatch);
		//	}

		//	spriteBatch.Begin();
		//	spriteBatch.Draw(_cursor, _gameStateManager.MousePosition - new Vector2(10, 10), Color.Red);
		//	spriteBatch.End();

		//}
		public void DrawInViewport(GameTime gameTime, SpriteBatch spriteBatch, Vector4 viewport, Vector2 cameraPosition, Player curPlayer)/* Might want current player later... */ //, Player currentPlayer)
		{
			Vector4 cameraViewport = viewport;
			cameraViewport.X = cameraPosition.X;
			cameraViewport.Y = cameraPosition.Y;

			spriteBatch.Draw(MainGame.SingleColor, cameraViewport.ToRectangle(), Color.CornflowerBlue);


			// Draw all the platforms
			foreach (var kv in _environmentObjects)
			{
				kv.Value.Where(go => MainGuiObject.GetIntersectionDepth(cameraViewport, go.Bounds) != DoubleVector2.Zero).ToList().ForEach(p => p.Draw(gameTime, spriteBatch));
			}
			foreach (var kv in _characterObjects)
			{
				kv.Value.Where(go => MainGuiObject.GetIntersectionDepth(cameraViewport, go.Bounds) != DoubleVector2.Zero).ToList().ForEach(p => p.Draw(gameTime, spriteBatch));
			}
			// Draw all the players.
			Players.Values.Where(go => MainGuiObject.GetIntersectionDepth(cameraViewport, go.Bounds) != DoubleVector2.Zero).ToList().ForEach(p => p.Draw(gameTime, spriteBatch));

			foreach (LevelAnimation la in _levelAnimations.ToList())
			{
				la.Draw(gameTime, spriteBatch);
			}

			//spriteBatch.Begin();
			//spriteBatch.Draw(_cursor, _gameStateManager.MousePosition - new Vector2(10, 10), Color.Red);
			//if (currentPlayer.UsesMouseAndKeyboard) spriteBatch.Draw(MainGame.Cursor, _gameStateManager.MousePosition + cameraPosition - new Vector2(10, 10), Color.Red);
			if (curPlayer != null && GameStateManager.AllControls != null)
			{
				if (curPlayer.UsesMouseAndKeyboard)
					spriteBatch.Draw(MainGame.Cursor, _gameStateManager.MousePosition + cameraPosition - new Vector2(10, 10), Color.Red);
				else if (!curPlayer.IsAi)
				{
					Vector2 viewportSize = cameraViewport.GetSize();
					PlayerControls playerControls = GameStateManager.GetControlsForPlayer(curPlayer);
					if (playerControls != null)
					{
						Vector2 playerAim = playerControls.GetAim(curPlayer);
						float aimDistance = (float)Math.Sqrt(Math.Pow(viewportSize.X, 2) + Math.Pow(viewportSize.Y, 2)) / 6.0f;
						spriteBatch.Draw(MainGame.Cursor, curPlayer.Center + (playerAim * aimDistance) - new Vector2(10, 10), Color.Red);
					}
				}
			}
			//spriteBatch.End();
		}

		public void DrawGridInViewport(SpriteBatch spriteBatch, Vector4 viewport, Vector2 cameraPosition, float scaleAmount)
		{
			Vector4 cameraViewport = viewport;
			cameraViewport.X = cameraPosition.X;
			cameraViewport.Y = cameraPosition.Y;

			float lineDifference = PlatformDifference / 4.0f;

			int lineThickness = Math.Max(1, (int)(1.0f / scaleAmount));

			int heightOfLine = (int)Math.Min(cameraViewport.Z, Size.Y - cameraViewport.Y);
			if (cameraViewport.Y < 0)
				heightOfLine += (int)(cameraViewport.Y);
			int widthOfLine = (int)Math.Min(cameraViewport.W, Size.X - cameraViewport.X);
			if (cameraViewport.X < 0)
				widthOfLine += (int)(cameraViewport.X);

			float currentHorizontalPosition = Math.Max(0, ((cameraViewport.Y + lineDifference) / lineDifference) * lineDifference);
			while (currentHorizontalPosition < cameraViewport.Y + cameraViewport.Z && currentHorizontalPosition <= Size.Y)
			{
				spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)Math.Max(0, cameraViewport.X), (int)currentHorizontalPosition, widthOfLine, lineThickness), new Color(0, 0, 0, .5f));
				currentHorizontalPosition += lineDifference;
			}

			float currentVerticalPosition = Math.Max(0, ((cameraViewport.X + lineDifference) / lineDifference) * lineDifference);
			while (currentVerticalPosition < cameraViewport.X + cameraViewport.W && currentVerticalPosition <= Size.X)
			{
				spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)currentVerticalPosition, (int)Math.Max(0, cameraViewport.Y), lineThickness, heightOfLine), new Color(0, 0, 0, .5f));
				currentVerticalPosition += lineDifference;
			}
		}
		public Dictionary<Group, List<MainGuiObject>> GetAllUnPassableEnvironmentObjects()
		{
			return _environmentObjects.Where(kv => kv.Key != Group.Passable).ToDictionary(kv => kv.Key, kv => kv.Value);
		}

		public Dictionary<Group, List<MainGuiObject>> GetAllUnPassableCharacterObjects()
		{
			return _characterObjects.Where(kv => kv.Key != Group.Passable).ToDictionary(kv => kv.Key, kv => kv.Value);
		}
		public Dictionary<Group, List<MainGuiObject>> GetAllGuiObjects()
		{
			Dictionary<Group, List<MainGuiObject>> returnMap = new Dictionary<Group, List<MainGuiObject>>();
			var gbenv = _environmentObjects.GroupBy(kv => kv.Key); // new place in memory... so dumb
			var gbplayers = _players.GroupBy(kv => kv.Key); // new place in memory... so dumb
			foreach (var kv in gbenv)
				returnMap.Add(kv.Key, kv.SelectMany(v => v.Value).ToList());
			foreach (Player p in _players.Select(pl => pl.Value))
			{
				List<MainGuiObject> mgoList;
				if (returnMap.TryGetValue(p.Group, out mgoList))
				{
					mgoList.Add(p);
				}
				else
				{
					returnMap.Add(p.Group, new List<MainGuiObject>() { p });
				}
			}

			foreach (Group key in _characterObjects.Keys)
			{
				List<MainGuiObject> outList;
				if (returnMap.TryGetValue(key, out outList))
					outList.AddRange(_characterObjects[key]);
				else
					returnMap[key] = _characterObjects[key];
			}
			return returnMap;
		}

		public List<MainGuiObject> GetAllStructures()
		{
			return _environmentObjects.SelectMany(kv => kv.Value).Where(mgo => mgo.ObjectType == GuiObjectType.Structure).ToList();
		}

		public void RemoveGuiObject(MainGuiObject guiObject)
		{
			if (guiObject.ObjectType == GuiObjectType.Environment || guiObject.ObjectType == GuiObjectType.Structure)
				RemoveEnvironmentObject(guiObject);
			else if (guiObject.ObjectType == GuiObjectType.Character)
				RemoveCharacterObject(guiObject);
			else if (guiObject.ObjectType == GuiObjectType.Player)
				_players.Remove(guiObject.Id);
			else
				throw new Exception("Level::RemoveGuiObject, what do I do with this type of object?!");
		}

		private void RemoveEnvironmentObject(MainGuiObject guiObject)
		{
			List<MainGuiObject> groupGuiObjects;
			if (_environmentObjects.TryGetValue(guiObject.Group, out groupGuiObjects))
				groupGuiObjects.Remove(guiObject);
		}

		private void RemoveCharacterObject(MainGuiObject guiObject)
		{
			List<MainGuiObject> groupGuiObjects;
			if (_characterObjects.TryGetValue(guiObject.Group, out groupGuiObjects))
				groupGuiObjects.Remove(guiObject);
		}
		public void AddLevelAnimation(LevelAnimation la)
		{
			_levelAnimations.Add(la);
		}

		public void RemoveLevelAnimation(TextAnimation textAnimation)
		{
			_levelAnimations.Remove(textAnimation);
		}

		// Gets the object with matching information.
		// null if doesn't exist.
		public MainGuiObject GetObject(GuiObjectType type, Guid id)
		{
			try
			{
				if (type == GuiObjectType.Environment || type == GuiObjectType.Structure)
					return _environmentObjects.SelectMany(m => m.Value).FirstOrDefault(obj => obj.Id == id);
				else if (type == GuiObjectType.Character)
					return _characterObjects.SelectMany(m => m.Value).FirstOrDefault(obj => obj.Id == id);
				else if (type == GuiObjectType.Player)
					return _players[id];
				else
					throw new Exception("Level::RemoveGuiObject, what do I do with this type of object?!");
			}
			catch (Exception)
			{
				return null;
			}
		}
		public void ChangeGroup(MainGuiObject mainGuiObject, Group newGroup)
		{
			GuiObjectType type = mainGuiObject.ObjectType;
			try
			{
				if (type == GuiObjectType.Environment || type == GuiObjectType.Structure)
				{
					_environmentObjects[mainGuiObject.Group].Remove(mainGuiObject);
					AddEnvironmentObject(mainGuiObject, newGroup);
				}
				else if (type == GuiObjectType.Character)
				{
					_characterObjects[mainGuiObject.Group].Remove(mainGuiObject);
					AddCharacterObject(mainGuiObject, newGroup);
				}
			}
			catch (Exception)
			{
			}
		}

		public MainGuiObject GetGuiObjectAtPosition(Vector2 mousePosition)
		{
			List<MainGuiObject> hitObjects = new List<MainGuiObject>();
			hitObjects.AddRange(_environmentObjects.SelectMany(kv => kv.Value).Where(mgo => mousePosition.IsInBounds(mgo.Bounds)));
			hitObjects.AddRange(_characterObjects.SelectMany(kv => kv.Value).Where(mgo => mousePosition.IsInBounds(mgo.Bounds)));
			hitObjects.AddRange(_players.Values.Where(mgo => mousePosition.IsInBounds(mgo.Bounds)));
			return hitObjects.OrderBy(mgo => mgo.Size.X * mgo.Size.Y).FirstOrDefault(); // smallest gets picked!
		}

		public void FinishedGame(Player winner)
		{
			_gameStateManager.FinishedGame(winner);
			GameStateManager.GameTimerRunning = false;
		}
	}
}
