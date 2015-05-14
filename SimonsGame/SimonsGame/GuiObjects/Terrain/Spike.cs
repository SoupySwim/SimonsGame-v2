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
	public class Spike : MainGuiObject
	{

		// Here's a list of objects that this object will ignore.
		// That includes collision and any other affect this could have on an object.
		// It is up to the inherited class to use this effectively.
		protected Dictionary<Guid, int> _ignoredIds = new Dictionary<Guid, int>();
		private int _maxInvincibility = 90; // 1.5 seconds.

		private Texture2D _background;
		private bool _isFlipped = false; // Spikes are up OR down if vertical and left OR right if not.
		private bool _isVerticalAffecting = true; // Spikes are up and down VS left and right
		private float _rotation = 0;

		private float _projectedWidth;
		private float _projectedHeight;
		private float _heightCap;
		private int _repeatXCount;

		public Spike(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level, "Spike")
		{
			AdditionalGroupChange(group, group);
			_background = MainGame.ContentManager.Load<Texture2D>("Test/Spikes");
			ExtraSizeManipulation(hitbox);
		}
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void AddCustomModifiers(GameTime gameTime, ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, ModifierBase modifyMult) { }
		protected override bool ShowHitBox() { return false; }
		public override void PreUpdate(GameTime gameTime) { }
		public override void PostUpdate(GameTime gameTime)
		{
			foreach (var kv in _ignoredIds.ToList())
			{
				_ignoredIds[kv.Key]++;
				if (kv.Value > _maxInvincibility)
					_ignoredIds.Remove(kv.Key);
			}
		}
		public override void PreDraw(GameTime gameTime, SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			int multX = Size.X < 0 ? -1 : 1;
			int multY = Size.Y < 0 ? -1 : 1;
			int addX = Size.X < 0 ? (int)-_projectedWidth : 0;
			int addY = Size.Y < 0 ? (int)-_projectedWidth : 0;
			for (float h = 0; h < _heightCap; h += _projectedHeight)
				for (int w = 0; w < _repeatXCount; w++)
				{
					//spriteBatch.Draw(_background, new Rectangle((int)(Position.X + addX + multX * w * _projectedWidth), (int)(Position.Y + addY + multY * h), (int)(_projectedWidth), (int)(_projectedHeight)), _hitBoxColor);
					Rectangle first = new Rectangle((int)(Position.X + (_background.Bounds.Width / 2.0f) + addX + multX * w * _projectedWidth),
						(int)(Position.Y + (_background.Bounds.Height / 2.0f) + addY + multY * h),
						(int)(_projectedWidth),
						(int)(_projectedHeight));
					Vector2 position = new Vector2(Position.X + addX + multX * w * _projectedWidth, Position.Y + addY + multY * h);
					Vector2 spriteSize = new Vector2(_background.Bounds.Width, _background.Bounds.Height);
					Vector2 size = _isVerticalAffecting ? new Vector2(_projectedWidth, _projectedHeight) : new Vector2(_projectedHeight, _projectedWidth);
					float radians = _isVerticalAffecting ? 0 : (float)(Math.PI / 2.0f);
					//spriteBatch.Draw(_background, first, _background.Bounds, _hitBoxColor,
					//	_isVerticalAffecting ? 0 : (float)(Math.PI / 2.0f), new Vector2(((int)_background.Bounds.Width) / 2.0f, ((int)_background.Bounds.Height) / 2.0f), _isFlipped ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
					//spriteBatch.Draw(_background, first + new Vector2(_background.Bounds.Width, _background.Bounds.Height) / 2.0f, source, Color, _radians, Animation.FrameSize / 2.0f, Animation.Scale, spriteEffects, 0.0f);
					spriteBatch.Draw(_background, position + size / 2.0f, _background.Bounds, _hitBoxColor, radians, spriteSize / 2.0f, size / spriteSize, _isFlipped ? SpriteEffects.FlipVertically : SpriteEffects.None, 0.0f);

				}
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
			if (_repeatXCount == 0 || remainder >= _projectedHeight / 2)
				_repeatXCount++;
			_projectedWidth = sizeX / _repeatXCount;


			_heightCap = sizeY - _projectedHeight / 2;
		}
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			if (!_ignoredIds.ContainsKey(mgo.Id))
			{
				_ignoredIds.Add(mgo.Id, 0);
				float horizontalKB = 0;
				float verticalKB = 0;

				if (_isVerticalAffecting)
				{
					if ((mgo.Center.Y - Center.Y) * (_isFlipped ? -1 : 1) > 0)
						return;
					verticalKB = 2 * (_isFlipped ? 1 : -1);
					if (mgo.CurrentMovement.X < 0)
						horizontalKB = 2;
					else if (mgo.CurrentMovement.X > 0)
						horizontalKB = -2;
				}
				else
				{
					if ((mgo.Center.X - Center.X) * (_isFlipped ? 1 : -1) > 0)
						return;
					horizontalKB = 2 * (_isFlipped ? -1 : 1);
					if (mgo.CurrentMovement.Y < 0)
						verticalKB = 2;
					else if (mgo.CurrentMovement.Y > 0)
						verticalKB = -2;
				}

				Vector2 reverseMovement = new Vector2(horizontalKB, verticalKB); // mgo.CurrentMovement / -3.0f;
				ModifierBase spikeKB = new TickModifier(20, ModifyType.Add, this, Element.Normal);
				spikeKB.PreventControls = true;
				spikeKB.Movement = reverseMovement;
				mgo.HitByObject(this, spikeKB);
				TickModifier damageFromSpike = new TickModifier(1, ModifyType.Add, this, Element.Normal);
				damageFromSpike.SetHealthTotal(-150);
				mgo.HitByObject(this, damageFromSpike);
			}
		}

		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Orientation";
			else if (bType == ButtonType.SpecialToggle2)
				return "Flipped";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _isVerticalAffecting ? "Up/Down" : "Left/Right";
			else if (bType == ButtonType.SpecialToggle2)
				return _isFlipped ? "Flipped" : "Normal";
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				_isVerticalAffecting = !_isVerticalAffecting;
			else if (bType == ButtonType.SpecialToggle2)
				_isFlipped = !_isFlipped;
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _isVerticalAffecting ? 1 : 0;
			else if (bType == ButtonType.SpecialToggle2)
				return _isFlipped ? 1 : 0;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_isVerticalAffecting = value == 1;
			else if (bType == ButtonType.SpecialToggle2)
				_isFlipped = value == 1;
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}
}
