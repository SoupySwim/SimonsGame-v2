using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Extensions;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.GuiObjects.Zones;
using SimonsGame.MapEditor;
using SimonsGame.Utility;
using SimonsGame.Utility.ObjectAnimations;
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
		private Dictionary<Guid, MainGuiObject> _environmentObjects;
		private Dictionary<long, HashSet<Guid>> _immovableStaticObjects;
		private Dictionary<long, HashSet<Guid>> _movableDynamicObjects;
		private HashSet<MainGuiObject> _movableStaticObjects;
		private Dictionary<Guid, MainGuiObject> _characterObjects;
		private List<ITeleportable> _teleportObjects;

		// This will store what players are currently in the environment.
		private Dictionary<Guid, Player> _players;
		public Dictionary<Guid, Player> Players { get { return _players; } set { _players = value; } }

		private List<LevelAnimation> _levelAnimations;

		private Dictionary<Guid, GenericZone> _levelZones;

		private TickTimer _levelAnimationTimer;

		#region levelIndexer
		private float _levelXIndexer;
		private float _levelYIndexer;
		#endregion;

		// Used to tell how far "one" block in the level is.
		public float PlatformDifference { get; set; }


		private GameStateManager _gameStateManager;
		public GameStateManager GameStateManager { get { return _gameStateManager; } }
		public Vector2 Size { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size"> Determines the viewport of the Level (What is displayed on the screen). </param>
		public Level(Vector2 size, GameStateManager gameStateManager, int baseHeight = 160)
		{
			_environmentObjects = new Dictionary<Guid, MainGuiObject>();
			_characterObjects = new Dictionary<Guid, MainGuiObject>();
			_immovableStaticObjects = new Dictionary<long, HashSet<Guid>>();
			_movableDynamicObjects = new Dictionary<long, HashSet<Guid>>();
			_movableStaticObjects = new HashSet<MainGuiObject>();

			_teleportObjects = new List<ITeleportable>();
			_players = new Dictionary<Guid, Player>();

			Size = size;

			_gameStateManager = gameStateManager;

			_levelAnimations = new List<LevelAnimation>();
			_levelZones = new Dictionary<Guid, GenericZone>();

			PlatformDifference = baseHeight;
			_levelAnimationTimer = new TickTimer(12, GetCharacterAnimations, true);
		}

		public void Initialize()
		{
			// If we are actually in game, then set up the hash!
			// If not, then keep everything in the movable objects list.
			if (MainGame.GameState == MainGame.MainGameState.Game)
			{
				float levelDividerX = (int)(Size.X / PlatformDifference);
				float levelDividerY = (int)(Size.Y / PlatformDifference);
				_levelXIndexer = Size.X / levelDividerX;
				_levelYIndexer = Size.Y / levelDividerY;
				// Don't do this for things that move and things that are passable.
				foreach (var mo in _environmentObjects.Where(mo => mo.Value.Group != Group.Passable && (mo.Value.ObjectType == GuiObjectType.Environment || mo.Value.ObjectType == GuiObjectType.Structure)))
				{
					if (mo.Value.IsMovable)
						_movableStaticObjects.Add(mo.Value);
					else
						AddHashForObject(mo.Value, mo.Value.Bounds);
				}
				UpdateMovableObjects();
				foreach (MainGuiObject mgo in _characterObjects.Values.ToList())
					mgo.Initialize();
				foreach (MainGuiObject mgo in _environmentObjects.Values.OrderBy(m =>
					{
						GuiObjectClass guiObjectClass = m.GetClass();
						return guiObjectClass != GuiObjectClass.GuiIfClause || guiObjectClass != GuiObjectClass.GuiThenClause;
					}).ToList())
				{
					mgo.Initialize();
				}
				foreach (var kv in _levelZones)
					kv.Value.Initialize(this);
			}
			else if (MainGame.GameState == MainGame.MainGameState.Menu)
			{
				foreach (MainGuiObject mgo in _environmentObjects.Values.Where(m =>
				{
					GuiObjectClass guiObjectClass = m.GetClass();
					return guiObjectClass == GuiObjectClass.GuiIfClause || guiObjectClass == GuiObjectClass.GuiThenClause;
				}).ToList())
				{
					mgo.Initialize();
				}
			}
		}

		public void AddHashForObject(MainGuiObject mgo, Vector4 bounds)
		{
			foreach (var hash in GetObjectHashes(bounds))
			{
				HashSet<Guid> alreadyAdded;
				if (_immovableStaticObjects.TryGetValue(hash, out alreadyAdded))
					alreadyAdded.Add(mgo.Id);
				else
					_immovableStaticObjects.Add(hash, new HashSet<Guid>() { mgo.Id });
			}
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
				_environmentObjects.Add(guiObject.Id, guiObject);
			else if (guiObject is Player)
				AddPlayer(guiObject as Player);
			else if (guiObject.ObjectType == GuiObjectType.Zone)
				_levelZones.Add(guiObject.Id, guiObject as GenericZone);
			else if (guiObject is Player)
				AddPlayer((Player)guiObject);
			else if (guiObject.ObjectType == GuiObjectType.Teleporter)
				_teleportObjects.Add(guiObject as ITeleportable);
			else
				_characterObjects.Add(guiObject.Id, guiObject);
		}

		private void UpdateMovableObjects()
		{
			_movableDynamicObjects.Clear();
			foreach (var kv in _characterObjects.Where(mo => mo.Value.Group != Group.Passable))
			{
				foreach (var hash in GetObjectHashes(kv.Value.Bounds, true))
				{
					HashSet<Guid> alreadyAdded;
					if (_movableDynamicObjects.TryGetValue(hash, out alreadyAdded))
						alreadyAdded.Add(kv.Value.Id);
					else
						_movableDynamicObjects.Add(hash, new HashSet<Guid>() { kv.Value.Id });
				}
			}
		}

		// Update will call Update on all of the objects within the level.
		public void Update(GameTime gameTime)
		{
			UpdateMovableObjects();
			// Update all the platforms
			foreach (var e in _environmentObjects.ToList())
				e.Value.Update(gameTime);
			foreach (var t in _teleportObjects)
				t.Update(gameTime);
			// Update all the platforms
			foreach (var c in _characterObjects.ToList())
				c.Value.Update(gameTime);
			// Update all the players.
			foreach (Player player in Players.Values)
				player.Update(gameTime);
			foreach (LevelAnimation la in _levelAnimations.ToList())
				la.Update(gameTime);

			foreach (var kv in _levelZones)
				kv.Value.Update(gameTime);

			_levelAnimationTimer.Update(gameTime);
		}

		private void GetCharacterAnimations()
		{
			foreach (var c in _characterObjects.ToList())
				c.Value.GetLevelAnimations();
			foreach (var c in GetAllStructures())
				c.GetLevelAnimations();
			Players.Values.Where(p => !p.NotAcceptingControls).ToList().ForEach(p => p.GetLevelAnimations());
		}

		//This has been revamped.  This is only one possibility.  hashes are now smaller.
		// _____________________________
		//|  0 |  1 |  2 |  3 |  4 |  5 |
		//|____|____|____|____|____|____|
		//|  6 |  7 |  8 |  9 | 10 | 11 |
		//|____|____|____|____|____|____|
		//| 12 | 13 | 14 | 15 | 16 | 17 |
		//|____|____|____|____|____|____|
		//| 18 | 19 | 20 | 21 | 22 | 23 |
		//|____|____|____|____|____|____|
		//| 24 | 25 | 26 | 27 | 28 | 29 |
		//|____|____|____|____|____|____|
		//| 30 | 31 | 32 | 33 | 34 | 35 |
		//|____|____|____|____|____|____|
		public IEnumerable<long> GetObjectHashes(Vector4 bounds, bool exaggerate = false)
		{
			List<long> hashes = new List<long>();
			int xSection = (int)(bounds.X / _levelXIndexer) - (exaggerate ? 1 : 0);
			int xSectionEnd = (int)((bounds.X + bounds.W) / _levelXIndexer) + (exaggerate ? 1 : 0);
			int ySection = (int)(bounds.Y / _levelYIndexer) - (exaggerate ? 1 : 0);
			int ySectionEnd = (int)((bounds.Y + bounds.Z) / _levelYIndexer) + (exaggerate ? 1 : 0);
			for (int yNdx = ySection; yNdx <= ySectionEnd; yNdx++)
				for (int xNdx = xSection; xNdx <= xSectionEnd; xNdx++)
					hashes.Add((yNdx * 6) + xNdx);
			return hashes;
		}

		public Vector2 DrawInViewport(GameTime gameTime, SpriteBatch spriteBatch, Vector4 viewport, Vector2 cameraPosition, Player curPlayer)
		{
			Vector2 keyboardPlayerMousePosition = Vector2.Zero;
			Vector4 cameraViewport = viewport;
			cameraViewport.X = cameraPosition.X;
			cameraViewport.Y = cameraPosition.Y;

			spriteBatch.Draw(MainGame.SingleColor, cameraViewport.ToRectangle(), Color.CornflowerBlue);

			IEnumerable<MainGuiObject> itemsToDraw = _environmentObjects.Values.Where(go => MainGuiObject.GetIntersectionDepth(cameraViewport, go.Bounds) != Vector2.Zero)
				.Concat(_levelZones.Values)
				.Concat(_teleportObjects.Cast<MainGuiObject>())
				.Concat(_characterObjects.Values.Where(go => MainGuiObject.GetIntersectionDepth(cameraViewport, go.Bounds) != Vector2.Zero))
				.Concat(Players.Values.Where(go => MainGuiObject.GetIntersectionDepth(cameraViewport, go.Bounds) != Vector2.Zero))
				.OrderBy(di => di.DrawImportant);

			foreach (MainGuiObject mgo in itemsToDraw)
				mgo.Draw(gameTime, spriteBatch, curPlayer);

			// Draw all the platforms
			//foreach (var p in _environmentObjects.Where(go => MainGuiObject.GetIntersectionDepth(cameraViewport, go.Value.Bounds) != Vector2.Zero))
			//	p.Value.Draw(gameTime, spriteBatch, curPlayer);

			//foreach (var kv in _levelZones)
			//	kv.Value.Draw(gameTime, spriteBatch, curPlayer);

			//_teleportObjects.ForEach(t => t.Draw(gameTime, spriteBatch, curPlayer));

			//_characterObjects.Where(go => MainGuiObject.GetIntersectionDepth(cameraViewport, go.Bounds) != Vector2.Zero).ToList().ForEach(p => p.Draw(gameTime, spriteBatch));
			//// Draw all the players.
			//Players.Values.Where(go => MainGuiObject.GetIntersectionDepth(cameraViewport, go.Bounds) != Vector2.Zero).ToList().ForEach(p => p.Draw(gameTime, spriteBatch));

			foreach (LevelAnimation la in _levelAnimations.ToList())
				la.Draw(gameTime, spriteBatch);

			//spriteBatch.Begin();
			//spriteBatch.Draw(_cursor, _gameStateManager.MousePosition - new Vector2(10, 10), Color.Red);
			//if (currentPlayer.UsesMouseAndKeyboard) spriteBatch.Draw(MainGame.Cursor, _gameStateManager.MousePosition + cameraPosition - new Vector2(10, 10), Color.Red);
			if (curPlayer != null && Controls.AllControls != null)
			{
				if (curPlayer.UsesMouseAndKeyboard)
				{
					keyboardPlayerMousePosition = _gameStateManager.MousePosition + cameraPosition - new Vector2(10, 10);
					//spriteBatch.Draw(MainGame.Cursor, keyboardPlayerMousePosition, Color.Red);
				}
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
			return keyboardPlayerMousePosition;
		}

		public void DrawGridInViewport(SpriteBatch spriteBatch, Vector4 viewport, Vector2 cameraPosition, float scaleAmount)
		{
			Vector4 cameraViewport = viewport;
			cameraViewport.X = cameraPosition.X;
			cameraViewport.Y = cameraPosition.Y;

			float lineDifference = PlatformDifference / 4.0f;

			int lineThickness = Math.Max(1, (int)((1.0f / scaleAmount) + .0005f));

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

		public IEnumerable<MainGuiObject> GetAllUnPassableEnvironmentObjects(Vector4 bounds)
		{
			return GetPossiblyHitEnvironmentObjects(bounds).Where(e => e.Group != Group.Passable);
		}


		public IEnumerable<MainGuiObject> GetPossiblyHitEnvironmentObjects(Vector4 bounds)
		{
			IEnumerable<Guid> allIdEnum = GetObjectHashes(bounds).SelectMany(hash => { HashSet<Guid> outls; _immovableStaticObjects.TryGetValue(hash, out outls); return outls ?? new HashSet<Guid>(); });
			HashSet<Guid> allIds = new HashSet<Guid>(allIdEnum);
			IEnumerable<MainGuiObject> returnList = allIds.Select(id =>
			{
				MainGuiObject outobj;
				if (_environmentObjects.TryGetValue(id, out outobj))
					return outobj;
				return null;
			}).Where(a => a != null);
			return returnList.Concat(_movableStaticObjects);
		}

		public IEnumerable<MainGuiObject> GetAllMovableCharacters(Vector4 bounds, bool includePassable = true)
		{
			IEnumerable<MainGuiObject> returnList = new List<MainGuiObject>();
			returnList = _players.Select(pl => pl.Value).ToList();
			IEnumerable<Guid> allIdEnum = GetObjectHashes(bounds).SelectMany(hash => { HashSet<Guid> outls; _movableDynamicObjects.TryGetValue(hash, out outls); return outls ?? new HashSet<Guid>(); });
			HashSet<Guid> allIds = new HashSet<Guid>(allIdEnum);
			returnList = returnList.Concat(allIds.Select(id => { MainGuiObject outMgo; _characterObjects.TryGetValue(id, out outMgo); return outMgo; }).Where(a => a != null));
			return includePassable ? returnList : returnList.Where(a => a.Group != Group.Passable);
		}

		public List<MainGuiObject> GetAllGuiObjects()
		{
			List<MainGuiObject> returnMap = new List<MainGuiObject>();
			foreach (var kv in _environmentObjects.ToList())
				returnMap.Add(kv.Value);
			foreach (Player p in _players.Select(pl => pl.Value))
				returnMap.Add(p);
			foreach (var key in _characterObjects.ToList())
				returnMap.Add(key.Value);
			return returnMap;
		}

		public IEnumerable<MainGuiObject> GetAllStructures()
		{
			return _environmentObjects.Where(mgo => mgo.Value.ObjectType == GuiObjectType.Structure).Select(kv => kv.Value);
		}

		public void RemoveGuiObject(MainGuiObject guiObject)
		{
			if (guiObject.ObjectType == GuiObjectType.Environment || guiObject.ObjectType == GuiObjectType.Structure)
				RemoveEnvironmentObject(guiObject);
			else if (guiObject.ObjectType == GuiObjectType.Character)
				RemoveCharacterObject(guiObject);
			else if (guiObject.ObjectType == GuiObjectType.Player)
				_players.Remove(guiObject.Id);
			else if (guiObject.ObjectType == GuiObjectType.Teleporter)
				_teleportObjects.Remove(guiObject as ITeleportable);
			else if (guiObject.ObjectType == GuiObjectType.Zone)
				_levelZones.Remove(guiObject.Id);
			else
				throw new Exception("Level::RemoveGuiObject, what do I do with this type of object?!");
		}

		private void RemoveEnvironmentObject(MainGuiObject mgo)
		{
			_environmentObjects.Remove(mgo.Id);
			_movableStaticObjects.Remove(mgo);
			var hashes = GetObjectHashes(mgo.Bounds + new Vector4(-mgo.Size.X, -mgo.Size.Y, mgo.Size.Y * 2, mgo.Size.X * 2));
			foreach (var hash in hashes)
			{
				if (_immovableStaticObjects.ContainsKey(hash))
					_immovableStaticObjects[hash].Remove(mgo.Id);
			}
		}

		private void RemoveCharacterObject(MainGuiObject guiObject)
		{
			_characterObjects.Remove(guiObject.Id);
		}

		public void AddLevelAnimation(LevelAnimation la)
		{
			_levelAnimations.Add(la);
		}

		public void RemoveLevelAnimation(LevelAnimation textAnimation)
		{
			_levelAnimations.Remove(textAnimation);
		}

		// Gets the object with matching information.
		// null if doesn't exist.
		public MainGuiObject GetObject(GuiObjectType type, Guid id)
		{
			try
			{
				GenericZone outZone;
				if (type == GuiObjectType.Environment || type == GuiObjectType.Structure)
					return _environmentObjects.FirstOrDefault(obj => obj.Value.Id == id).Value;
				else if (type == GuiObjectType.Character)
					return _characterObjects.Values.FirstOrDefault(obj => obj.Id == id);
				else if (type == GuiObjectType.Player)
					return _players[id];
				else if (type == GuiObjectType.Teleporter)
					return _teleportObjects.FirstOrDefault(mgo => mgo.Id == id) as MainGuiObject;
				else if (type == GuiObjectType.Teleporter && _levelZones.TryGetValue(id, out outZone))
					return outZone as MainGuiObject;
				else
					throw new Exception("Level::RemoveGuiObject, what do I do with this type of object?!");
			}
			catch (Exception)
			{
				return null;
			}
		}

		public MainGuiObject GetGuiObjectAtPosition(Vector2 mousePosition)
		{
			List<MainGuiObject> hitObjects = new List<MainGuiObject>();
			hitObjects.AddRange(_environmentObjects.Where(mgo => mousePosition.IsInBounds(mgo.Value.Bounds)).Select(kv => kv.Value));
			hitObjects.AddRange(_characterObjects.Values.Where(mgo => mousePosition.IsInBounds(mgo.Bounds)));
			hitObjects.AddRange(_players.Values.Where(mgo => mousePosition.IsInBounds(mgo.Bounds)));
			hitObjects.AddRange(_teleportObjects.Select(t => t as MainGuiObject).Where(mgo => mousePosition.IsInBounds(mgo.Bounds)));
			hitObjects.AddRange(_levelZones.Values.Select(t => t as MainGuiObject).Where(mgo => mousePosition.IsInBounds(mgo.Bounds)));
			return hitObjects.OrderBy(mgo => mgo.Size.X * mgo.Size.Y).FirstOrDefault(); // smallest gets picked!
		}

		public List<MainGuiObject> GetGuiObjectsInRange(Vector4 position)
		{
			List<MainGuiObject> hitObjects = new List<MainGuiObject>();
			hitObjects.AddRange(_environmentObjects.Where(mgo => position.Contains(mgo.Value.Bounds)).Select(kv => kv.Value));
			hitObjects.AddRange(_characterObjects.Values.Where(mgo => position.Contains(mgo.Bounds)));
			hitObjects.AddRange(_players.Values.Where(mgo => position.Contains(mgo.Bounds)));
			hitObjects.AddRange(_teleportObjects.Select(t => t as MainGuiObject).Where(mgo => position.Contains(mgo.Bounds)));
			hitObjects.AddRange(_levelZones.Values.Select(t => t as MainGuiObject).Where(mgo => position.Contains(mgo.Bounds)));
			return hitObjects; // smallest gets picked!
		}

		public List<MainGuiObject> GetOverlappingGuiObjectsInRange(Vector4 bounds)
		{
			List<MainGuiObject> hitObjects = new List<MainGuiObject>();
			hitObjects.AddRange(_environmentObjects.Where(mgo => MainGuiObject.GetIntersectionDepth(bounds, mgo.Value.Bounds) != Vector2.Zero).Select(kv => kv.Value));
			hitObjects.AddRange(_characterObjects.Values.Where(mgo => MainGuiObject.GetIntersectionDepth(bounds, mgo.Bounds) != Vector2.Zero));
			hitObjects.AddRange(_players.Values.Where(mgo => MainGuiObject.GetIntersectionDepth(bounds, mgo.Bounds) != Vector2.Zero));
			hitObjects.AddRange(_teleportObjects.Select(t => t as MainGuiObject).Where(mgo => MainGuiObject.GetIntersectionDepth(bounds, mgo.Bounds) != Vector2.Zero));
			hitObjects.AddRange(_levelZones.Values.Select(t => t as MainGuiObject).Where(mgo => MainGuiObject.GetIntersectionDepth(bounds, mgo.Bounds) != Vector2.Zero));
			return hitObjects; // smallest gets picked!
		}

		public void FinishedGame(MainGuiObject winner)
		{
			_gameStateManager.FinishedGame(winner);
		}

		public IEnumerable<ITeleportable> GetMatchingTeleporters(ITeleportable teleporter)
		{
			return _teleportObjects.Where(obj => obj.GetTeleportId() == teleporter.GetTeleportId());
		}
		public IEnumerable<ITeleportable> GetAllTeleporters()
		{
			return _teleportObjects;
		}


		public Dictionary<Guid, GenericZone> GetAllZones()
		{
			return _levelZones;
		}
	}
}
