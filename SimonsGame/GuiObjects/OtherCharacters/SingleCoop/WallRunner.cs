using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		// Here's a list of objects that this object will ignore.
		// That includes collision and any other affect this could have on an object.
		// It is up to the inherited class to use this effectively.
		protected Dictionary<Guid, int> _ignoredIds = new Dictionary<Guid, int>();
		private int _maxInvincibility = 90; // 1.5 seconds.

		public WallRunner(Vector2 position, Vector2 hitbox, Level level, bool movePositive)
			: base(position, hitbox, Group.BothPassable, level, "Wall Runner")
		{
			_showHealthBar = true;
			MaxSpeedBase = new Vector2(AverageSpeed.Y / 3, AverageSpeed.Y / 3);
			AIStateDirection = movePositive ? WallRunnerDirection.MovePositive : WallRunnerDirection.MoveNegative;
			_healthTotal = 400;
			_healthCurrent = _healthTotal;
			AIStateFace = WallRunnerFace.Top; // ew.
			MaxSpeed = new Vector2(AverageSpeed.Y, AverageSpeed.Y);
			_idleAnimation = new Animation(MainGame.ContentManager.Load<Texture2D>("Test/WallRunner"), 1, false, 80, 80, (Size.X / 80.0f));
			_animator.Color = Color.Red;
			IsMovable = false;
			AccelerationBase = new Vector2(1);
			_animator.PlayAnimation(_idleAnimation);
			Team = Team.Neutral;
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

		public override void PostUpdate(GameTime gameTime)
		{
			if (_previousPosition != Vector2.Zero && _referenceBounds != Vector4.Zero && !PrimaryOverlapObjects[AIStateFace == WallRunnerFace.Bottom || AIStateFace == WallRunnerFace.Top ? Orientation.Vertical : Orientation.Horizontal].Any())
			{
				if (AIStateFace == WallRunnerFace.Bottom || AIStateFace == WallRunnerFace.Top)
				{
					float newX = AIStateDirection == WallRunnerDirection.MovePositive ? _referenceBounds.X + _referenceBounds.W - .05f : _referenceBounds.X - Size.X + .05f;
					Position = new Vector2(newX, Position.Y);
					_referenceBounds = Vector4.Zero;
				}
				else //if (AIStateFace == WallRunnerFace.Left || AIStateFace == WallRunnerFace.Right)
				{
					float newY = AIStateDirection == WallRunnerDirection.MovePositive ? _referenceBounds.Y + _referenceBounds.Z - .05f : _referenceBounds.Y - Size.Y + .05f;
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

			base.PostUpdate(gameTime);
			bool isStuck = _previousPosition == Position;
			// If hit a wall
			if (isStuck)
			{
				var stuff = PrimaryOverlapObjects.SelectMany(a => a.Value);
				if (stuff.Count() > 1) // Then we hit a platform to move on!
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
					base.PostUpdate(gameTime); // hack!
				}
			}

			foreach (var kv in _ignoredIds.ToList())
			{
				//Debug.WriteLine(_ignoredIds[kv.Key]);
				_ignoredIds[kv.Key]++;
				if (kv.Value > _maxInvincibility)
					_ignoredIds.Remove(kv.Key);
			}

			IEnumerable<MainGuiObject> hitObjects = Level.GetAllUnPassableMovableObjects(Bounds).Where(mgo => mgo.Team != Team || mgo.Team == Team.None);
		}
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			if (mb == null && mgo.IsMovable && !_ignoredIds.ContainsKey(mgo.Id))
			{
				ToggleDirection();
				_ignoredIds.Add(mgo.Id, 0);
				float horizontalKB = 0;
				float verticalKB = 0;

				if (AIStateFace == WallRunnerFace.Left || AIStateFace == WallRunnerFace.Right)
				{
					verticalKB = 1f * (AIStateDirection == WallRunnerDirection.MovePositive ? 1 : -1);
					if (mgo.CurrentMovement.X < 0)
						horizontalKB = .65f;
					else if (mgo.CurrentMovement.X > 0)
						horizontalKB = -.65f;
				}
				else // top or bottom
				{
					horizontalKB = .65f * (AIStateDirection == WallRunnerDirection.MovePositive ? -1 : 1);
					if (mgo.CurrentMovement.Y < 0)
						verticalKB = 1f;
					else if (mgo.CurrentMovement.Y > 0)
						verticalKB = -1f;
				}

				Vector2 reverseMovement = new Vector2(horizontalKB, verticalKB); // mgo.CurrentMovement / -3.0f;
				ModifierBase spikeKB = new TickModifier(20, ModifyType.Add, this, new Tuple<Element, float>(Element.Normal, .3f));
				spikeKB.PreventControls = true;
				spikeKB.KnockBack = reverseMovement;
				mgo.HitByObject(this, spikeKB);
				TickModifier smallKnockback = new TickModifier(1, ModifyType.Add, this, new Tuple<Element, float>(Element.Normal, 0));
				smallKnockback.SetHealthTotal(-150);
				mgo.HitByObject(this, smallKnockback);
			}
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
