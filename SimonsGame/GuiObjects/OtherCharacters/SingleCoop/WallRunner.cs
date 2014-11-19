using Microsoft.Xna.Framework;
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


		public WallRunner(Vector2 position, Vector2 hitbox, Group group, Level level, bool movePositive)
			: base(position, hitbox, group, level)
		{
			MaxSpeedBase = new Vector2(AverageSpeed.Y, AverageSpeed.Y);
			AIStateDirection = movePositive ? WallRunnerDirection.MovePositive : WallRunnerDirection.MoveNegative;
			_healthTotal = 6;
			_healthCurrent = _healthTotal;
			AIStateFace = WallRunnerFace.Top; // ew.
			MaxSpeed = new Vector2(AverageSpeed.Y, AverageSpeed.Y);
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
					return .5f;
				case WallRunnerFace.Right:
					return -.5f;
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
				//if (AIStateFace == WallRunnerFace.Left || AIStateFace == WallRunnerFace.Right)
				//	ToggleDirection();
				AIStateFace = (WallRunnerFace)(((int)AIStateFace + moveFacesNumber) % 4);
			}
			else if (PrimaryOverlapObjects.Any() && _referenceBounds == Vector4.Zero)
			{
				if (AIStateFace == WallRunnerFace.Left || AIStateFace == WallRunnerFace.Right)
					_referenceBounds = PrimaryOverlapObjects[Orientation.Horizontal].Bounds;
				else
					_referenceBounds = PrimaryOverlapObjects[Orientation.Vertical].Bounds;
			}
			// If ran off edge
			//else if (AIStateDirection == WallRunnerDirection.MovePositive)
			//{
			//	if (AIStateFace == WallRunnerFace.Top && PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out ReferenceObject)
			//	&& Position.X > ReferenceObject.Position.X + ReferenceObject.Size.X)
			//	{
			//		Position = new Vector2(ReferenceObject.Position.X + ReferenceObject.Size.X - 1.1f, ReferenceObject.Position.Y - Size.Y + 1.1f);
			//		AIStateFace = WallRunnerFace.Right;
			//	}
			//	else if (AIStateFace == WallRunnerFace.Bottom && PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out ReferenceObject)
			//	&& Position.X > ReferenceObject.Position.X + ReferenceObject.Size.X)
			//	{
			//		ToggleDirection();
			//		AIStateFace = WallRunnerFace.Right;
			//	}
			//	else if (AIStateFace == WallRunnerFace.Left && PrimaryOverlapObjects.TryGetValue(Orientation.Horizontal, out ReferenceObject)
			//	&& Position.Y > ReferenceObject.Position.Y + ReferenceObject.Size.Y)
			//	{
			//		AIStateFace = WallRunnerFace.Bottom;
			//	}
			//	else if (AIStateFace == WallRunnerFace.Right && PrimaryOverlapObjects.TryGetValue(Orientation.Horizontal, out ReferenceObject)
			//	&& Position.Y > ReferenceObject.Position.Y + ReferenceObject.Size.Y)
			//	{
			//		ToggleDirection();
			//		AIStateFace = WallRunnerFace.Bottom;
			//	}
			//}
			//else // if (AIStateDirection == WallRunnerDirection.MoveNegative)
			//{
			//	if (AIStateFace == WallRunnerFace.Top && PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out ReferenceObject)
			//	&& Position.X + Size.X < ReferenceObject.Position.X)
			//	{
			//		Position = new Vector2(ReferenceObject.Position.X - Size.X + 1.1f, ReferenceObject.Position.Y - Size.Y + 1.1f);
			//		ToggleDirection();
			//		AIStateFace = WallRunnerFace.Left;
			//	}
			//	else if (AIStateFace == WallRunnerFace.Bottom && PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out ReferenceObject)
			//	&& Position.X + Size.X < ReferenceObject.Position.X)
			//	{
			//		AIStateFace = WallRunnerFace.Left;
			//	}
			//	else if (AIStateFace == WallRunnerFace.Left && PrimaryOverlapObjects.TryGetValue(Orientation.Horizontal, out ReferenceObject)
			//	&& Position.Y + Size.Y < ReferenceObject.Position.Y)
			//	{
			//		ToggleDirection();
			//		AIStateFace = WallRunnerFace.Top;
			//	}
			//	else if (AIStateFace == WallRunnerFace.Right && PrimaryOverlapObjects.TryGetValue(Orientation.Horizontal, out ReferenceObject)
			//	&& Position.Y + Size.Y < ReferenceObject.Position.Y)
			//	{
			//		AIStateFace = WallRunnerFace.Top;
			//	}
			//}
			base.PreUpdate(gameTime);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		protected override bool ShowHitBox()
		{
			return true;
		}
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			_abilityManager.AddAbility(mb);
		}

		protected override Dictionary<Group, List<MainGuiObject>> GetAllVerticalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		{
			return guiObjects;
		}

		protected override Dictionary<Group, List<MainGuiObject>> GetAllHorizontalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		{
			return guiObjects;
		}

		protected override List<Group> GetIgnoredVerticalGroups(List<Group> suggestedGroups)
		{
			return new List<Group>() { SimonsGame.Utility.Group.Passable };
		}
	}
}
