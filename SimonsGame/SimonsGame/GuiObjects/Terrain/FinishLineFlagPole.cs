using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Modifiers;

namespace SimonsGame.GuiObjects
{
	public class FinishLineFlagPole : MainGuiObject
	{
		private Texture2D _background;
		public FinishLineFlagPole(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "FinishLineFlagPole")
		{
			_team = Team.None;
			_background = MainGame.ContentManager.Load<Texture2D>("Test/FlagPole");
			_hitBoxColor = Color.LightGray;
		}
		public override float GetXMovement()
		{
			return 0;
		}
		public override float GetYMovement()
		{
			return 0;
		}
		public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PreUpdate(GameTime gameTime) { }
		public override void PostUpdate(GameTime gameTime)
		{
			if (Level.GameStateManager.WinCondition == MainFiles.WinCondition.ReachGoal)
			{
				foreach (Player player in Level.Players.Values)
				{
					if (MainGuiObject.GetIntersectionDepth(player.HitBoxBounds, Bounds) != DoubleVector2.Zero)
						Level.FinishedGame(player);
				}
			}
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(_background, Bounds.ToRectangle(), _hitBoxColor);
		}
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
	}
}
