using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Utility.ObjectAnimations;
using SimonsGame.Utility;
using SimonsGame.Modifiers;
using SimonsGame.MapEditor;
//using SimonsGame.Extensions;

namespace SimonsGame.GuiObjects
{
	class Button : AffectedSpace
	{
		// This is when a button is pressed, it will stay down forever.
		private bool _neverRestart = false;
		private int _minTimer = 3;
		private TickTimer _timeActive;
		public Button(Vector2 position, Vector2 size, Level level)
			: base(position, size, level, "Button")
		{
			// Default is if stepping on it, then active, otherwise not.
			_timeActive = new TickTimer(_minTimer, () => { SetInactive(); }, false);

			HitBoxColor = Color.DarkRed; // We'll see...!
		}
		public override void PostUpdate(GameTime gameTime)
		{
			base.PostUpdate(gameTime);

			if (!_neverRestart)
				_timeActive.Update(gameTime);
		}
		private void SetInactive()
		{
			if (IsActiveForFunction)
			{
				IsActiveForFunction = false;
				Position.Y -= Size.Y / 2;
			}
		}
		protected override void HitObject(MainGuiObject mgo, Vector2 bounds)
		{
			if (!IsActiveForFunction)
			{
				Position.Y += Size.Y / 2;
				IsActiveForFunction = true;
			}
			// If you are supposed to restart it, then restart it.
			if (!_neverRestart)
				_timeActive.Restart();
		}

		public override IEnumerable<Tuple<Vector2, MainGuiObject>> GetAffectedObjects()
		{
			return GetHitObjects(Level.GetAllMovableCharacters(Bounds).Concat(Level.GetAllUnPassableEnvironmentObjects(Bounds).Where(mgo => mgo.IsMovable)), Bounds).Where(kv => kv.Item2.Id != Id);
		}

		#region Crap Stuff

		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PreUpdate(GameTime gameTime) { base.PreUpdate(gameTime); }
		protected override bool ShowHitBox() { return true; }
		#endregion


		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Active Time";
			if (bType == ButtonType.SpecialToggle2)
				return "One Hit Button";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				if (_timeActive.TickTotal == _minTimer)
					return "When Hit";
				return string.Format("{0:0.0}", (_timeActive.TickTotal / 60.0f));
			}
			if (bType == ButtonType.SpecialToggle2)
				return _neverRestart ? "Yes" : "No";
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				_timeActive.TickTotal = MathHelper.Clamp(_timeActive.TickTotal + (moveRight ? 300 : -300), _minTimer, 3600);
				if (_timeActive.TickTotal == (300 + _minTimer))
					_timeActive.TickTotal = 300;
			}
			if (bType == ButtonType.SpecialToggle2)
			{
				_neverRestart = !_neverRestart;
			}
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _timeActive.TickTotal;
			if (bType == ButtonType.SpecialToggle2)
				return _neverRestart ? 1 : 0;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_timeActive.TickTotal = value;
			if (bType == ButtonType.SpecialToggle2)
				_neverRestart = value == 1;
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}
}
