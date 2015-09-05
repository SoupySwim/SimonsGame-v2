using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.GuiObjects
{

	public class GuiIfClause : GuiConnector
	{
		public IfClause Clause = IfClause.Dead;

		public GuiIfClause(Vector2 position, Vector2 size, Level level)
			: base(position, size, level, "GuiIfClause")
		{
		}

		protected override void ConnectedFunction()
		{
			if (_guiFunction != null)
				_guiFunction.AddIfConnector(this);
		}

		public bool CheckActive()
		{
			if (_targetObject == null)
				return true;

			if (Clause == IfClause.Dead)
				return _targetObject.HealthCurrent <= 0;
			else if (Clause == IfClause.Active)
				return _targetObject.IsActiveForFunction;

			return false;
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
				Clause = moveRight ? MiscExtensions.GetNextEnum<IfClause>(Clause) : MiscExtensions.GetPreviousEnum<IfClause>(Clause);
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
				Clause = (IfClause)value;
			base.SetSpecialValue(bType, value);
		}

		#endregion

	}
}
