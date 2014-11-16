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


		public WallRunner(Vector2 position, Vector2 hitbox, Group group, Level level, bool movePositive)
			: base(position, hitbox, group, level)
		{
			MaxSpeedBase = new Vector2(AverageSpeed.X * 2, AverageSpeed.Y * 2);
			AIStateDirection = movePositive ? WallRunnerDirection.MovePositive : WallRunnerDirection.MoveNegative;
			_healthTotal = 6;
			_healthCurrent = _healthTotal;
			AIStateFace = WallRunnerFace.Top;
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
					return AIStateDirection == WallRunnerDirection.MovePositive ? MaxSpeed.X / 2 : -MaxSpeed.X / 2;
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
					return AIStateDirection == WallRunnerDirection.MovePositive ? MaxSpeed.Y / 2 : -MaxSpeed.Y / 2;
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
			MainGuiObject ReferenceObject;
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
				AIStateFace = (WallRunnerFace)(((int)AIStateFace + moveFacesNumber) % 4);
			}
			else if (!PrimaryOverlapObjects.Any()) { }
			// If ran off edge
			else if (AIStateDirection == WallRunnerDirection.MovePositive)
			{
				if (AIStateFace == WallRunnerFace.Top && PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out ReferenceObject)
				&& Position.X > ReferenceObject.Position.X + ReferenceObject.Size.X)
				{
					Position = new Vector2(ReferenceObject.Position.X + ReferenceObject.Size.X - 1.1f, ReferenceObject.Position.Y - Size.Y + 1.1f);
					AIStateFace = WallRunnerFace.Right;
				}
				else if (AIStateFace == WallRunnerFace.Bottom && PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out ReferenceObject)
				&& Position.X > ReferenceObject.Position.X + ReferenceObject.Size.X)
				{
					ToggleDirection();
					AIStateFace = WallRunnerFace.Right;
				}
				else if (AIStateFace == WallRunnerFace.Left && PrimaryOverlapObjects.TryGetValue(Orientation.Horizontal, out ReferenceObject)
				&& Position.Y > ReferenceObject.Position.Y + ReferenceObject.Size.Y)
				{
					AIStateFace = WallRunnerFace.Bottom;
				}
				else if (AIStateFace == WallRunnerFace.Right && PrimaryOverlapObjects.TryGetValue(Orientation.Horizontal, out ReferenceObject)
				&& Position.Y > ReferenceObject.Position.Y + ReferenceObject.Size.Y)
				{
					ToggleDirection();
					AIStateFace = WallRunnerFace.Bottom;
				}
			}
			else // if (AIStateDirection == WallRunnerDirection.MoveNegative)
			{
				if (AIStateFace == WallRunnerFace.Top && PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out ReferenceObject)
				&& Position.X + Size.X < ReferenceObject.Position.X)
				{
					Position = new Vector2(ReferenceObject.Position.X - Size.X + 1.1f, ReferenceObject.Position.Y - Size.Y + 1.1f);
					ToggleDirection();
					AIStateFace = WallRunnerFace.Left;
				}
				else if (AIStateFace == WallRunnerFace.Bottom && PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out ReferenceObject)
				&& Position.X + Size.X < ReferenceObject.Position.X)
				{
					AIStateFace = WallRunnerFace.Left;
				}
				else if (AIStateFace == WallRunnerFace.Left && PrimaryOverlapObjects.TryGetValue(Orientation.Horizontal, out ReferenceObject)
				&& Position.Y + Size.Y < ReferenceObject.Position.Y)
				{
					ToggleDirection();
					AIStateFace = WallRunnerFace.Top;
				}
				else if (AIStateFace == WallRunnerFace.Right && PrimaryOverlapObjects.TryGetValue(Orientation.Horizontal, out ReferenceObject)
				&& Position.Y + Size.Y < ReferenceObject.Position.Y)
				{
					AIStateFace = WallRunnerFace.Top;
				}
			}
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
	}
}
