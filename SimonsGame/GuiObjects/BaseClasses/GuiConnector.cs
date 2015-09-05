using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.MapEditor;

namespace SimonsGame.GuiObjects
{
	public abstract class GuiConnector : MainGuiObject
	{
		public enum Corner
		{
			TopLeft = 0,
			TopRight = 1,
			BottomRight = 2,
			BottomLeft = 3,
		}

		private Corner _corner;

		protected Vector2 _basePosition;
		protected Vector2 _endPosition;
		protected bool _isVisible = true;

		protected MainGuiObject _targetObject;
		protected GuiFunction _guiFunction;


		protected abstract void ConnectedFunction();

		public GuiConnector(Vector2 position, Vector2 size, Level level, string name)
			: base(position, size, Group.Passable, level, name)
		{
			_basePosition = position;
			_endPosition = position;


			if (MainGame.GameState == MainGame.MainGameState.Game)
				_isVisible = false;
		}

		public override void Initialize()
		{
			// must be corners, so find the proper corner.

			_basePosition = GetCornerPosition(_corner);
			_endPosition = GetCornerPosition((Corner)(((int)_corner + 2) % 4));
			FinalizeSize();

			if (MainGame.GameState == MainGame.MainGameState.Game)
			{
				Level.RemoveGuiObject(this);
			}
		}

		private Vector2 GetCornerPosition(Corner corner)
		{
			Vector2 returnPosition = Vector2.Zero;
			if (corner == Corner.TopLeft || corner == Corner.TopRight)
				returnPosition.Y = Position.Y;
			else
				returnPosition.Y = Position.Y + Size.Y;

			if (corner == Corner.BottomLeft || corner == Corner.TopLeft)
				returnPosition.X = Position.X;
			else
				returnPosition.X = Position.X + Size.X;

			return returnPosition;
		}

		public override void PostUpdate(GameTime gameTime) { }

		public override void ExtraSizeManipulation(ref Vector2 newSize)
		{
			if (newSize == Vector2.Zero)
				_basePosition = Position;
			_endPosition = _basePosition + newSize;

			if (newSize.X == 0)
				newSize.X = 1;
			if (newSize.Y == 0)
				newSize.Y = 1;
		}

		public override void FinalizeSize() // in map editor
		{
			if (Size.X < 0)
			{
				Position.X = Position.X + Size.X;
				_size.X = _size.X * -1;
			}
			if (Size.Y < 0)
			{
				Position.Y = Position.Y + Size.Y;
				_size.Y = _size.Y * -1;
			}

			if (_basePosition.Y == Position.Y) // top
			{
				if (_basePosition.X == Position.X)
					_corner = Corner.TopLeft;
				else
					_corner = Corner.TopRight;
			}
			else // bottom
			{
				if (_basePosition.X == Position.X)
					_corner = Corner.BottomLeft;
				else
					_corner = Corner.BottomRight;
			}

			Vector4 bounds = new Vector4(_basePosition.X - 2, _basePosition.Y - 2, 4, 4);
			MainGuiObject firstObject = Level.GetOverlappingGuiObjectsInRange(bounds)
				.OrderBy(mgo => mgo.Size.X * mgo.Size.Y).FirstOrDefault(m => m.GetClass() != GuiObjectClass.GuiIfClause && m.GetClass() != GuiObjectClass.GuiThenClause); // Will need a little different function
			if (firstObject == null)
			{
				Level.RemoveGuiObject(this);
				return;
			}

			bounds.X = _endPosition.X - 2;
			bounds.Y = _endPosition.Y - 2;
			MainGuiObject secondObject = Level.GetOverlappingGuiObjectsInRange(bounds)
				.OrderBy(mgo => mgo.Size.X * mgo.Size.Y).FirstOrDefault(m => m.GetClass() != GuiObjectClass.GuiIfClause && m.GetClass() != GuiObjectClass.GuiThenClause); // Will need a little different function

			if (secondObject == null)
			{
				Level.RemoveGuiObject(this);
				return;
			}

			MainGuiObject targetObject = null;
			GuiFunction guiFunction = firstObject as GuiFunction;
			if (firstObject == null)
			{
				guiFunction = secondObject as GuiFunction;
				targetObject = firstObject;
			}
			else if (!(secondObject is GuiFunction))
			{
				targetObject = secondObject;
			}

			if (guiFunction != null && targetObject != null)
			{
				_guiFunction = guiFunction;
				_targetObject = targetObject;
				ConnectedFunction();
			}
			else
			{
				Level.RemoveGuiObject(this);
				return;
			}
		}

		public override void PreDraw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (_isVisible)
			{
				spriteBatch.DrawLine(_basePosition, _endPosition, Color.Black, 1);
			}
		}


		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle4)
				return (int)_corner;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle4)
				_corner = (Corner)value;
			base.SetSpecialValue(bType, value);
		}

	}
}
