using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using SimonsGame.Utility.ObjectAnimations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{

	public enum IfClause
	{
		Dead,
		Active
	}

	public enum ThenClause
	{
		Remove,
		MoveUp,
		MoveDown,
		MoveRight,
		MoveLeft,
	}

	public class GuiFunction : MainGuiObject
	{
		private bool _wasActive = false;

		private TickTimer _checkUpdates;

		private List<GuiIfClause> IfObjects = new List<GuiIfClause>();
		private List<MainGuiObject> ThenObjects = new List<MainGuiObject>();

		private bool _isVisible = true;
		public GuiFunction(Vector2 position, Level level)
			: base(position, new Vector2(40), Group.Passable, level, "Function")
		{
			_checkUpdates = new TickTimer(6, CheckIfActive, true);

			if (MainGame.GameState == MainGame.MainGameState.Game)
				_isVisible = false;


			if (_isVisible)
			{
				Texture2D animationTexture = MainGame.ContentManager.Load<Texture2D>("Test/Hammer");
				_animator.Color = Color.Black;
				_animator.PlayAnimation(new Animation(animationTexture, .1f, false, animationTexture.Width, animationTexture.Height, new Vector2(Size.X / animationTexture.Width, Size.Y / animationTexture.Height)));
				HitBoxColor = Color.White;
			}

		}

		public override void PostUpdate(GameTime gameTime)
		{
			_checkUpdates.Update(gameTime);
		}

		private void CheckIfActive()
		{
			bool allActive = true;
			foreach (GuiIfClause clause in IfObjects.ToList())
			{
				if (clause.CheckActive())
				{
					if (clause.Clause == IfClause.Dead)
						IfObjects.Remove(clause);
				}
				else
					allActive = false;
			}

			if (allActive && !_wasActive)
			{
				foreach (GuiThenClause clause in ThenObjects.ToList())
				{
					clause.PerformAction();
					if (clause.Clause == ThenClause.Remove)
					{
						ThenObjects.Remove(clause);
					}
				}
			}
			else if (!allActive && _wasActive) // If we aren't active this time, but were last time.
			{
				foreach (GuiThenClause clause in ThenObjects)
					clause.UndoAction();
			}

			// If there are no ifs or thens, then this function is useless!
			if (!IfObjects.Any() || !ThenObjects.Any())
				Level.RemoveGuiObject(this);

			_wasActive = allActive;
		}

		public void AddIfConnector(GuiIfClause ifClause)
		{
			IfObjects.Add(ifClause);
		}

		public void AddThenConnector(GuiThenClause ThenClause)
		{
			ThenObjects.Add(ThenClause);
		}

		#region Crap Stuff
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PreUpdate(GameTime gameTime) { }
		protected override bool ShowHitBox() { return _isVisible; }
		#endregion



	}
}
