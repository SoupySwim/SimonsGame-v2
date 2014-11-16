using Microsoft.Xna.Framework;
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

		public MovingPlatform(Vector2 position, Vector2 hitbox, Group group, Level level,
			bool isVerticalMoving = false, int maxTravelDistance = 600, bool goingPositiveDirection = true)
			: base(position, hitbox, group, level)
		{
			_verticalMoving = isVerticalMoving;
			if (goingPositiveDirection == false)
				_travelDistance = maxTravelDistance;
			_goingPositiveDirection = !goingPositiveDirection;
			_maxTravelDistance = maxTravelDistance;
		}
		public override float GetXMovement()
		{
			if (_verticalMoving)
				return 0;

			float distance = _goingPositiveDirection ? 3 : -3;
			_travelDistance += distance;
			return distance;
		}
		public override float GetYMovement()
		{
			if (!_verticalMoving)
				return 0;
			float distance = _goingPositiveDirection ? AverageSpeed.Y / 2 : -AverageSpeed.Y / 2;
			_travelDistance += distance;
			return distance;
		}
		public override void AddCustomModifiers(GameTime gameTime, ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, ModifierBase modifyMult) { }
		public override void PreUpdate(GameTime gameTime)
		{
			if (_travelDistance <= 0 || _travelDistance >= _maxTravelDistance)
			{
				_goingPositiveDirection = !_goingPositiveDirection;
			}
		}
		public override void PostUpdate(GameTime gameTime) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
	}
}
