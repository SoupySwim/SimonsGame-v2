using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.GuiObjects.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame
{
	public class GameStateManager
	{

		#region DegubArea
		public static bool ShowHitBoxes { get; set; }
		public static TimeSpan GameTimer = TimeSpan.Zero;
		public static bool GameTimerRunning = false;
		#endregion

		public static SpriteFont PlainFont;

		public static Dictionary<Guid, PlayerControls> AllControls { get; set; }
		public static Dictionary<Guid, PlayerControls> PreviousControls { get; set; }
		private static Dictionary<Guid, AIUtility> _aiUtilityMap { get; set; }
		private Vector2 _mousePosition;
		public Vector2 MousePosition { get { return _mousePosition; } } // Will have to compensate for camera offset when the time comes.

		public Level Level { get; set; }

		public GameStateManager(IServiceProvider serviceProvider, Vector2 size)
		{
			//TODO change obviously
			ShowHitBoxes = false;
			Level = Test.LevelBuilder.BuildLevel2(serviceProvider, size, this);
			_mousePosition = Vector2.Zero;
			_aiUtilityMap = Level.Players.Where(p => p.Value.IsAi).Select(p => new { key = p.Key, value = new AIUtility(p.Value) }).ToDictionary(p => p.key, p => p.value);
			PlainFont = Level.Content.Load<SpriteFont>("Fonts/PlainFont"); // Shouldn't load from level...
		}
		public void Update(GameTime gameTime, Dictionary<Guid, PlayerControls> allControls, Vector2 newMousePosition)
		{
			if (GameTimerRunning)
				GameTimer += gameTime.ElapsedGameTime;
			else if (GameTimer == TimeSpan.Zero && allControls.Values.First().XMovement > 0)
				GameTimerRunning = true;

			// Update Controls
			PreviousControls = AllControls;
			SetAiControls(allControls);
			AllControls = allControls;
			_mousePosition = newMousePosition;
			Level.Update(gameTime);
		}
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			Level.Draw(gameTime, spriteBatch);
		}


		public static PlayerControls GetControlsForPlayer(Guid playerId)
		{
			PlayerControls playerControls;
			AllControls.TryGetValue(playerId, out playerControls);
			return playerControls;
		}
		private void SetAiControls(Dictionary<Guid, PlayerControls> allControls)
		{
			if (PreviousControls != null)
			{
				foreach (Guid guid in allControls.Keys.ToList())
				{
					Player currentPlayer;
					if (Level.Players.TryGetValue(guid, out currentPlayer) && currentPlayer.IsAi)
					{
						allControls[guid] = _aiUtilityMap[guid].GetAiControls(PreviousControls[guid]);
					}
				}
			}
		}
	}
}
