using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Extensions;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{

	public class GuiThenClause : GuiConnector
	{
		public ThenClause Clause = ThenClause.Remove;
		private Vector2 _originalCenter;

		public GuiThenClause(Vector2 position, Vector2 size, Level level)
			: base(position, size, level, "GuiIfClause")
		{
		}
		public override void Initialize()
		{
			base.Initialize();
			if (_targetObject != null)
			{
				Vector4 bounds = _targetObject.Bounds;
				_originalCenter = _targetObject.Center;

				if (Clause == ThenClause.MoveDown)
					bounds.Y = _targetObject.Position.Y + _targetObject.Size.Y;
				else if (Clause == ThenClause.MoveUp)
					bounds.Y = _targetObject.Position.Y - _targetObject.Size.Y;
				else if (Clause == ThenClause.MoveRight)
					bounds.X = _targetObject.Position.X + _targetObject.Size.X;
				else if (Clause == ThenClause.MoveLeft)
					bounds.X = _targetObject.Position.X - _targetObject.Size.X;
				Level.AddHashForObject(_targetObject, bounds);
			}
		}
		protected override void ConnectedFunction()
		{
			if (_guiFunction != null)
				_guiFunction.AddThenConnector(this);
		}

		public void PerformAction()
		{
			if (_targetObject == null)
				return;

			if (Clause == ThenClause.Remove)
				_targetObject.Level.RemoveGuiObject(_targetObject);
			else if (Clause == ThenClause.MoveDown)
				_targetObject.TeleportTo(new Vector2(_targetObject.Center.X, _targetObject.Center.Y + _targetObject.Size.Y));
			else if (Clause == ThenClause.MoveUp)
				_targetObject.TeleportTo(new Vector2(_targetObject.Center.X, _targetObject.Center.Y - _targetObject.Size.Y));
			else if (Clause == ThenClause.MoveRight)
				_targetObject.TeleportTo(new Vector2(_targetObject.Center.X + _targetObject.Size.X, _targetObject.Center.Y));
			else if (Clause == ThenClause.MoveLeft)
				_targetObject.TeleportTo(new Vector2(_targetObject.Center.X - _targetObject.Size.X, _targetObject.Center.Y));
		}

		public void UndoAction()
		{
			if (_targetObject == null)
				return;

			if (Clause == ThenClause.Remove)
				_targetObject.Level.RemoveGuiObject(_targetObject);
			else if (Clause == ThenClause.MoveDown || Clause == ThenClause.MoveUp
				||Clause == ThenClause.MoveRight || Clause == ThenClause.MoveLeft)
				_targetObject.TeleportTo(_originalCenter);
		}

		#region Crap Stuff
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PreUpdate(GameTime gameTime) { }
		protected override bool ShowHitBox() { return false; }
		#endregion


		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Cluase";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return Clause.ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				Clause = moveRight ? MiscExtensions.GetNextEnum<ThenClause>(Clause) : MiscExtensions.GetPreviousEnum<ThenClause>(Clause);
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return (int)Clause;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				Clause = (ThenClause)value;
			base.SetSpecialValue(bType, value);
		}

		#endregion

	}
}
