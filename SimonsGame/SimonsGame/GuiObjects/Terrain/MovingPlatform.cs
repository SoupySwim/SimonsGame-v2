using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class MovingPlatform : MainGuiObject
	{
		// Platform either moves vertically, or horizontally for now.
		private bool _verticalMoving = false;
		// Decides which direction the platform will move (positive is "down" or "right")
		private bool _goingPositiveDirection = true;

		private int _maxTravelDistance = 600;
		private float _travelDistance = 0;

		private float _projectedWidth;
		private float _projectedHeight;
		private float _heightCap;
		private int _repeatXCount;

		private Texture2D _background;

		public MovingPlatform(Vector2 position, Vector2 hitbox, Group group, Level level,
			bool isVerticalMoving = false, int maxTravelDistance = 600, bool goingPositiveDirection = true)
			: base(position, hitbox, group, level, "Moving Platform")
		{
			_verticalMoving = isVerticalMoving;
			if (goingPositiveDirection == false)
				_travelDistance = maxTravelDistance;
			_goingPositiveDirection = !goingPositiveDirection;
			_maxTravelDistance = maxTravelDistance;
			_background = MainGame.ContentManager.Load<Texture2D>("Test/Platform");
			AccelerationBase = new Vector2(1);
			MaxSpeedBase = new Vector2(3, 2.5f);
			IsMovable = true;
		}
		public override float GetXMovement()
		{
			if (_verticalMoving)
				return 0;

			return _goingPositiveDirection ? 3 : -3;
		}
		public override float GetYMovement()
		{
			if (!_verticalMoving)
				return 0;
			return _goingPositiveDirection ? MaxSpeedBase.Y : -MaxSpeedBase.Y;
		}

		public override void PostUpdate(GameTime gameTime) { }
		public override void PreUpdate(GameTime gameTime)
		{
			if (_hitBoxColor == Color.Purple)
			{
			}
			if (_travelDistance <= 0 || _travelDistance >= _maxTravelDistance)
				_goingPositiveDirection = !_goingPositiveDirection;
			_travelDistance += (_goingPositiveDirection ? 1 : -1) * Math.Abs(CurrentMovement.X != 0 ? CurrentMovement.X : CurrentMovement.Y);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }

		public override void PostDraw(GameTime gameTime, SpriteBatch spriteBatch, Player curPlayer)
		{

			int multX = Size.X < 0 ? -1 : 1;
			int multY = Size.Y < 0 ? -1 : 1;
			int addX = Size.X < 0 ? (int)-_projectedWidth : 0;
			int addY = Size.Y < 0 ? (int)-_projectedWidth : 0;
			for (float h = 0; h < _heightCap; h += _projectedHeight)
				for (int w = 0; w < _repeatXCount; w++)
					spriteBatch.Draw(_background, new Rectangle((int)(Position.X + addX + multX * w * _projectedWidth), (int)(Position.Y + addY + multY * h), (int)(_projectedWidth), (int)(_projectedHeight)), _hitBoxColor);
			//spriteBatch.Draw(_background, Position + new Vector2(w * projectedHeight, h * projectedHeight), _hitBoxColor);
		}
		public override void ExtraSizeManipulation(ref Vector2 newSize)
		{
			float sizeX = Math.Abs(newSize.X);
			float sizeY = Math.Abs(newSize.Y);
			_projectedHeight = (int)(sizeY / _background.Height);
			int remainder = (int)(sizeY % _background.Height);
			if (_projectedHeight == 0 || remainder >= _background.Height / 2)
				_projectedHeight++;
			_projectedHeight = sizeY / _projectedHeight;

			_repeatXCount = (int)(sizeX / _projectedHeight);
			remainder = (int)(sizeX % _projectedHeight);
			if (remainder >= _projectedHeight / 2)
				_repeatXCount++;
			_projectedWidth = sizeX / _repeatXCount;


			_heightCap = sizeY - _projectedHeight / 2;
		}
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			//if (!_verticalMoving && (mgo as PhysicsObject).IsLanded)
			//	mgo.Position = new Vector2(mgo.Position.X + (_goingPositiveDirection ? 3 : -3), mgo.Position.Y);
		}
		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Axis";
			else if (bType == ButtonType.SpecialToggle2)
				return "Distance";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _verticalMoving ? "Vertical" : "Horizontal";
			else if (bType == ButtonType.SpecialToggle2)
				return (_maxTravelDistance / 40).ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				_verticalMoving = !_verticalMoving;
			else if (bType == ButtonType.SpecialToggle2)
				_maxTravelDistance = MathHelper.Clamp(_maxTravelDistance + (moveRight ? 40 : -40), 40, 2400);
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _verticalMoving ? 1 : 0;
			else if (bType == ButtonType.SpecialToggle2)
				return _maxTravelDistance;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_verticalMoving = value == 1;
			else if (bType == ButtonType.SpecialToggle2)
				_maxTravelDistance = value;
			base.SetSpecialValue(bType, value);
		}
	}
}
