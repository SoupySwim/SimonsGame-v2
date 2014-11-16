using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
		public Dictionary<Guid, Player> Players { get { return _players; } }

		// Used to tell how far "one" block in the level is.
		public float PlatformDifference { get; set; }


		private Texture2D _cursor;

		// Level content.        
		public ContentManager Content
		{
			get { return _content; }
		}
		private ContentManager _content;

		private GameStateManager _gameStateManager;
		public GameStateManager GameStateManager { get { return _gameStateManager; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="Size"> Determines the viewport of the Level (What is displayed on the screen). </param>
		public Level(IServiceProvider serviceProvider, Vector2 Size, GameStateManager gameStateManager)
		{
			_environmentObjects = new Dictionary<Group, List<MainGuiObject>>();
			_characterObjects = new Dictionary<Group, List<MainGuiObject>>();
			_players = new Dictionary<Guid, Player>();

			_gameStateManager = gameStateManager;

			// Create a new content manager to load content used just by this level.
			_content = new ContentManager(serviceProvider, "Content");
			_cursor = Content.Load<Texture2D>("Cursor/CursorStar");
		}
		public void AddPlayer(Player player)
		{
			_players.Add(player.Id, player);
		}
		public void AddGuiObject(MainGuiObject guiObject)
		{
			if (guiObject.ObjectType == GuiObjectType.Environment)
				AddEnvironmentObject(guiObject);
			else
				AddCharacterObject(guiObject);
		}
		private void AddEnvironmentObject(MainGuiObject guiObject)
		{
			if (_environmentObjects.ContainsKey(guiObject.Group))
			{
				_environmentObjects[guiObject.Group].Add(guiObject);
			}
			else
			{
				_environmentObjects.Add(guiObject.Group, new List<MainGuiObject>() { guiObject });
			}
		}
		private void AddCharacterObject(MainGuiObject guiObject)
		{
			if (_characterObjects.ContainsKey(guiObject.Group))
			{
				_characterObjects[guiObject.Group].Add(guiObject);
			}
			else
			{
				_characterObjects.Add(guiObject.Group, new List<MainGuiObject>() { guiObject });
			}
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
			Players.Values.ToList().ForEach(p => p.Update(gameTime));
		}

		// Draw will call Draw on all of the objects within the level.
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			// Draw all the players.
			Players.Values.ToList().ForEach(p => p.Draw(gameTime, spriteBatch));

			// Draw all the platforms
			foreach (var kv in _environmentObjects)
			{
				kv.Value.ForEach(p => p.Draw(gameTime, spriteBatch));
			}
			foreach (var kv in _characterObjects)
			{
				kv.Value.ForEach(p => p.Draw(gameTime, spriteBatch));
			}

			spriteBatch.Begin();
			spriteBatch.Draw(_cursor, _gameStateManager.MousePosition - new Vector2(10, 10), Color.Red);
			spriteBatch.End();

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
			var gb = _environmentObjects.GroupBy(kv => kv.Key); // new place in memory... so dumb
			foreach (var kv in gb)
				returnMap.Add(kv.Key, kv.SelectMany(v => v.Value).ToList());

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

		public void RemoveGuiObject(MainGuiObject guiObject)
		{
			if (guiObject.ObjectType == GuiObjectType.Environment)
				RemoveEnvironmentObject(guiObject);
			else
				RemoveCharacterObject(guiObject);
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
	}
}
