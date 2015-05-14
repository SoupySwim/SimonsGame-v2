using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class WallRunner : PhysicsObject
	{
		private enum WallRunnerDirection
		{
			MovePositive,
			MoveNegative
		}
		public enum WallRunnerFace
		{
			Bottom = 0,
			Right, // 1
			Top, // 2
			Left // 3
		}

		// What direction the character is moving in.
		private WallRunnerDirection AIStateDirection;

		// What face the character is currently on.
		private WallRunnerFace AIStateFace;

		// This will be the coordinate of importance for when we run off the edge.
		private Vector4 _referenceBounds;
		protected Animation _idleAnimation;


		public WallRunner(Vector2 position, Vector2 hitbox, Group group, Level level, bool movePositive)
			: base(position, hitbox, group, level, "WallRunner")
		{
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.Y, AverageSpeed.Y);
			AIStateDirection = movePositive ? WallRunnerDirection.MovePositive : WallRunnerDirection.MoveNegative;
			_healthTotal = 400;
			_healthCurrent = _healthTotal;
			AIStateFace = WallRunnerFace.Top; // ew.
			MaxSpeed = new Vector2(AverageSpeed.Y, AverageSpeed.Y);
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/WallRunner"), 1, false, 80, 80, (Size.X / 80.0f));
			_animator.Color = Color.Red;
			_animator.PlayAnimation(_idleAnimation);
		}

		private void ToggleDirection()
		{
			AIStateDirection = AIStateDirection == WallRunnerDirection.MovePositive ? WallRunnerDirection.MoveNegative : WallRunnerDirection.MovePositive;
		}

		public override float GetXMovement()
		{
			switch (AIStateFace)
			{
				case WallRunnerFace.Top:
				case WallRunnerFace.Bottom:
					return AIStateDirection == WallRunnerDirection.MovePositive ? MaxSpeed.X : -MaxSpeed.X;
				case WallRunnerFace.Left:
					return 1.5f;
				case WallRunnerFace.Right:
					return -1.5f;
			}
			return 0;
		}
		public override float GetYMovement()
		{
			switch (AIStateFace)
			{
				case WallRunnerFace.Left:
				case WallRunnerFace.Right:
					return AIStateDirection == WallRunnerDirection.MovePositive ? MaxSpeed.Y : -MaxSpeed.Y;
				case WallRunnerFace.Top:
					return .5f;
				case WallRunnerFace.Bottom:
					return -.5f;
			}
			return 0;
		}

		public override void PreUpdate(GameTime gameTime)
		{
			bool isStuck = _previousPosition == Position;
			// If hit a wall
			if (isStuck)
			{
				int moveFacesNumber = 0;
				if (AIStateFace == WallRunnerFace.Right || AIStateFace == WallRunnerFace.Top)
				{
					if (AIStateDirection == WallRunnerDirection.MovePositive)
						moveFacesNumber++;
					else
						moveFacesNumber += 3; // 3 is -1 when %4
				}
				else
				{
					if (AIStateDirection == WallRunnerDirection.MoveNegative)
						moveFacesNumber++;
					else
						moveFacesNumber += 3; // 3 is -1 when %4
				}
				if ((AIStateDirection == WallRunnerDirection.MovePositive && (AIStateFace == WallRunnerFace.Top || AIStateFace == WallRunnerFace.Left))
					|| (AIStateDirection == WallRunnerDirection.MoveNegative && (AIStateFace == WallRunnerFace.Bottom || AIStateFace == WallRunnerFace.Right)))
					ToggleDirection();
				//if (AIStateFace == WallRunnerFace.Left || AIStateFace == WallRunnerFace.Right)
				//	ToggleDirection();
				_referenceBounds = Vector4.Zero;
				AIStateFace = (WallRunnerFace)(((int)AIStateFace + moveFacesNumber) % 4);
			}
			else if (_previousPosition != Vector2.Zero && _referenceBounds != Vector4.Zero && !PrimaryOverlapObjects.ContainsKey(AIStateFace == WallRunnerFace.Bottom || AIStateFace == WallRunnerFace.Top ? Orientation.Vertical : Orientation.Horizontal))
			{
				if (AIStateFace == WallRunnerFace.Bottom || AIStateFace == WallRunnerFace.Top)
				{
					float newX = AIStateDirection == WallRunnerDirection.MovePositive ? _referenceBounds.X + _referenceBounds.W : _referenceBounds.X - Size.X;
					Position = new Vector2(newX, Position.Y);
					_referenceBounds = Vector4.Zero;
				}
				else //if (AIStateFace == WallRunnerFace.Left || AIStateFace == WallRunnerFace.Right)
				{
					float newY = AIStateDirection == WallRunnerDirection.MovePositive ? _referenceBounds.Y + _referenceBounds.Z + .5f : _referenceBounds.Y - Size.Y - .5f;
					Position = new Vector2(Position.X, newY);
					_referenceBounds = Vector4.Zero;
				}

				int moveFacesNumber = 0;
				if (AIStateFace == WallRunnerFace.Right || AIStateFace == WallRunnerFace.Top)
				{
					if (AIStateDirection == WallRunnerDirection.MoveNegative)
						moveFacesNumber++;
					else
						moveFacesNumber += 3; // 3 is -1 when %4
				}
				else
				{
					if (AIStateDirection == WallRunnerDirection.MovePositive)
						moveFacesNumber++;
					else
						moveFacesNumber += 3; // 3 is -1 when %4
				}
				if ((AIStateDirection == WallRunnerDirection.MoveNegative && (AIStateFace == WallRunnerFace.Top || AIStateFace == WallRunnerFace.Left))
					|| (AIStateDirection == WallRunnerDirection.MovePositive && (AIStateFace == WallRunnerFace.Bottom || AIStateFace == WallRunnerFace.Right)))
					ToggleDirection();
				AIStateFace = (WallRunnerFace)(((int)AIStateFace + moveFacesNumber) % 4);
			}
			else if ((AIStateFace == WallRunnerFace.Left || AIStateFace == WallRunnerFace.Right)
				&& PrimaryOverlapObjects[Orientation.Horizontal].Any()
				&& (_referenceBounds == Vector4.Zero || PrimaryOverlapObjects[Orientation.Horizontal].First().Bounds != _referenceBounds))
			{
				_referenceBounds = PrimaryOverlapObjects[Orientation.Horizontal].First().Bounds;
			}
			else if ((AIStateFace == WallRunnerFace.Top || AIStateFace == WallRunnerFace.Bottom)
				&& PrimaryOverlapObjects[Orientation.Vertical].Any()
				&& (_referenceBounds == Vector4.Zero || PrimaryOverlapObjects[Orientation.Vertical].First().Bounds != _referenceBounds))
			{
				_referenceBounds = PrimaryOverlapObjects[Orientation.Vertical].First().Bounds;
			}
			base.PreUpdate(gameTime);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			_abilityManager.AddAbility(mb);
		}

		protected override List<Group> GetIgnoredVerticalGroups(List<Group> suggestedGroups)
		{
			return new List<Group>() { SimonsGame.Utility.Group.Passable };
		}
		public override void SwitchDirections()
		{
			AIStateDirection = AIStateDirection == WallRunnerDirection.MoveNegative ? WallRunnerDirection.MovePositive : WallRunnerDirection.MoveNegative;
		}
		public override string GetDirectionalText()
		{
			return AIStateDirection.ToString();
		}
		public override bool DidSwitchDirection()
		{
			return AIStateDirection == WallRunnerDirection.MoveNegative;
		}
	}
}
