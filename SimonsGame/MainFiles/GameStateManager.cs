using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
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
		#endregion

		public static Dictionary<Guid, PlayerControls> AllControls { get; set; }
		public static Dictionary<Guid, PlayerControls> PreviousControls { get; set; }
		private Vector2 _mousePosition;
		public Vector2 MousePosition { get { return _mousePosition; } } // Will have to compensate for camera offset when the time comes.

		public Level Level { get; set; }

		public GameStateManager(IServiceProvider serviceProvider, Vector2 size)
		{
			//TODO change obviously
			ShowHitBoxes = false;
			Level = Test.LevelBuilder.BuildLevel2(serviceProvider, size, this);
			_mousePosition = Vector2.Zero;
		}
		public void Update(GameTime gameTime, Dictionary<Guid, PlayerControls> allControls, Vector2 newMousePosition)
		{
			if (allControls.Values.First().XMovement > 0)
			{
				GameTimer += new TimeSpan(1);
			}
			if (GameTimer > TimeSpan.Zero)
			{
				GameTimer += gameTime.ElapsedGameTime;
			}
			// Update Controls
			PreviousControls = AllControls;
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
	}
}
