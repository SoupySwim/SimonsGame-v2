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
	public class Platform : MainGuiObject
	{
		private Texture2D _background;

		private float _projectedWidth;
		private float _projectedHeight;
		private float _heightCap;
		private int _repeatXCount;

		#region Drop Logic
		private bool _doesDrop;
		private float _yOffsetDueToDrop = 0;
		private float _xOffsetDueToDrop = 0;
		private int _dropTimeTotal;
		private int _dropTimeCurrent;
		private int _respawnTimeTotal = -1;
		private int _respawnTimeCurrent = 0;
		private Group _storeGroup;
		#endregion

		public Platform(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "Platform")
		{
			AdditionalGroupChange(group, group);
			_background = MainGame.ContentManager.Load<Texture2D>("Test/Platform");
			ExtraSizeManipulation(hitbox);
			_doesDrop = false;
			_dropTimeTotal = 30;
			_dropTimeCurrent = 0;
			_respawnTimeTotal = -1;
		}
		public override float GetXMovement()
		{
			return 0;
		}
		public override float GetYMovement()
		{
			return 0;
		}
		public override void AddCustomModifiers(GameTime gameTime, ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, ModifierBase modifyMult) { }
		public override void PreUpdate(GameTime gameTime) { }
		public override void PostUpdate(GameTime gameTime)
		{
			if (_doesDrop)
			{
				if (_dropTimeCurrent > 0)
				{
					_dropTimeCurrent++;
					if (_dropTimeCurrent >= _dropTimeTotal)
					{
						_respawnTimeCurrent = 1;
						_dropTimeCurrent = 0;
						if (_respawnTimeTotal == -1)
							Level.RemoveGuiObject(this);
						else // We will respawn soon.
						{
							_storeGroup = Group;
							Group = Group.Passable;
						}
					}
					else
					{
						if (_dropTimeCurrent % 5 == 0)
							_xOffsetDueToDrop *= -1;
						if (_dropTimeCurrent % 6 == 0)
							_yOffsetDueToDrop += 1;
					}
				}
				else if (_respawnTimeCurrent > 0)
				{
					_respawnTimeCurrent++;
					if (_respawnTimeCurrent >= _respawnTimeTotal)
					{
						Group = _storeGroup;
						_respawnTimeCurrent = 0;
						_yOffsetDueToDrop = 0;
						_xOffsetDueToDrop = 0;
					}
				}
			}
		}
		public override void PreDraw(GameTime gameTime, SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (_xOffsetDueToDrop != 0)
			{
			}
			if (_respawnTimeCurrent > 0) // If we aren't currently drawn on screen, return!
				return;
			int multX = Size.X < 0 ? -1 : 1;
			int multY = Size.Y < 0 ? -1 : 1;
			int addX = Size.X < 0 ? (int)-_projectedWidth : 0;
			int addY = Size.Y < 0 ? (int)-_projectedWidth : 0;
			for (float h = 0; h < _heightCap; h += _projectedHeight)
				for (int w = 0; w < _repeatXCount; w++)
					spriteBatch.Draw(_background, new Rectangle((int)(Position.X + +_xOffsetDueToDrop + addX + multX * w * _projectedWidth), (int)(Position.Y + _yOffsetDueToDrop + addY + multY * h), (int)(_projectedWidth), (int)(_projectedHeight)), _hitBoxColor);
			//spriteBatch.Draw(_background, Position + new Vector2(w * projectedHeight, h * projectedHeight), _hitBoxColor);
		}
		public override void ExtraSizeManipulation(Vector2 newSize)
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
			if (_doesDrop && _dropTimeCurrent == 0)
			{
				_dropTimeCurrent = 1;
				_xOffsetDueToDrop = 2;
			}
		}
		protected override void AdditionalGroupChange(Group _group, Group newGroup)
		{
			switch (newGroup)
			{
				case Group.ImpassableIncludingMagic:
					_hitBoxColor = Color.SandyBrown;
					break;
				case Group.Impassable:
					_hitBoxColor = Color.Khaki;
					break;
				default:
					_hitBoxColor = Color.Wheat;
					break;
			}
			base.AdditionalGroupChange(_group, newGroup);
		}
		protected override bool ShowHitBox() { return false; }

		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Dropping";
			if (bType == ButtonType.SpecialToggle2)
				return "Respawn Time";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _doesDrop ? "Yes" : "No";
			if (bType == ButtonType.SpecialToggle2)
				return _respawnTimeTotal == -1 ? "Never" : (_respawnTimeTotal / 60).ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				_doesDrop = !_doesDrop;
			if (bType == ButtonType.SpecialToggle2)
			{
				if (_respawnTimeTotal < 0)
					_respawnTimeTotal = moveRight ? 300 : 3600;
				else if (!moveRight && _respawnTimeTotal == 300)
					_respawnTimeTotal = -1;
				else if (moveRight && _respawnTimeTotal == 3600)
					_respawnTimeTotal = -1;
				else
					_respawnTimeTotal = MathHelper.Clamp(_respawnTimeTotal + (moveRight ? 300 : -300), 300, 3600);
			}
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _doesDrop ? 1 : 0;
			if (bType == ButtonType.SpecialToggle2)
				return _respawnTimeTotal;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_doesDrop = value == 1;
			if (bType == ButtonType.SpecialToggle2)
				_respawnTimeTotal = value;
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}
}
