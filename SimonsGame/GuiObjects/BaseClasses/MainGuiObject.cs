﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Modifiers;
using Microsoft.Xna.Framework.Content;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Utility;

namespace SimonsGame.GuiObjects
{
	public enum GuiObjectType
	{
		Environment,
		Character,
		Attack
	}

	public abstract class MainGuiObject : GuiVariables
	{
		// Return an empty MainGuiObject, void of any important data.  It's a placeholder.
		public static MainGuiObject EmptyVessel { get { return null; } }


		// In the future, this will be used to animate the object.
		protected Animator _animator;

		// How much mana you have currently
		protected float _manaCurrent;
		public float ManaCurrent { get { return _manaCurrent; } }

		// How much health you have currently
		protected float _healthCurrent;
		public float HealthCurrent { get { return _healthCurrent; } }

		protected GuiObjectType _objectType;
		public GuiObjectType ObjectType { get { return _objectType; } }

		#region Graphics
		public Vector2 Position { get; set; }
		protected Vector2 _previousPosition;

		//  _____
		// |     |
		// |  *  |  <--- Center Position
		// |_____|
		public Vector2 Center { get { return new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y / 2); } }
		public Vector2 Size { get; set; }
		public Texture2D HitboxImage { get; set; }
		protected Color _hitBoxColor = new Color(1f, 1f, 1f, .8f);
		public Color HitBoxColor { get { return _hitBoxColor; } set { _hitBoxColor = value; } }
		public Vector4 Bounds { get { return new Vector4(Position.X, Position.Y, Size.Y, Size.X); } }
		public Vector4 HitBoxBounds { get { return new Vector4(Position.X - 5, Position.Y - 5, Size.Y + 10, Size.X + 10); } }
		#endregion

		/////////////////////
		// Need a base for //
		// all  modifiable //
		//    variables    //
		/////////////////////
		#region Base Variables
		public float ScaleBase { get; set; }

		#region MovementBase
		// Percentage of MaxSpeeds an object will move in one tick.
		public Vector2 MovementBase { get; set; }

		// Percentage of movement an object can gain in one tick.  Base is 1
		public Vector2 AccelerationBase { get; set; }

		// Max speed one can achieve (right now, only utilizing X direction
		public Vector2 MaxSpeedBase { get; set; }

		// Speed at which the object is currently moving.
		protected Vector2 CurrentMovementBase { get; set; }
		#endregion
		#endregion

		public Group Group { get; set; }
		public Level Level { get; set; }



		#region Abstract Functions
		public abstract void PreUpdate(GameTime gameTime);
		public abstract void PostUpdate(GameTime gameTime);
		public abstract void PreDraw(GameTime gameTime, SpriteBatch spriteBatch);
		public abstract void PostDraw(GameTime gameTime, SpriteBatch spriteBatch);
		public abstract void SetMovement(GameTime gameTime);
		public abstract float GetXMovement();
		public abstract float GetYMovement();
		public abstract void AddCustomModifiers(GameTime gameTime, ModifierBase modifyAdd);
		public abstract void MultiplyCustomModifiers(GameTime gameTime, ModifierBase modifyMult);
		public abstract void HitByObject(MainGuiObject mgo, ModifierBase mb);
		#endregion

		public MainGuiObject(Vector2 position, Vector2 hitbox, Group group, Level level)
		{
			_guid = Guid.NewGuid();
			Position = position;
			Size = hitbox;
			IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)Program.Game.Services.GetService(typeof(IGraphicsDeviceService));
			HitboxImage = new Texture2D(graphicsService.GraphicsDevice, 1, 1);
			HitboxImage.SetData(new[] { _hitBoxColor });
			Group = group;
			Level = level;
			_previousPosition = Vector2.Zero;

			// Init to 0 (non-movable objects)
			MovementBase = new Vector2(0f, 0f);
			AccelerationBase = new Vector2(0f, 0f);
			CurrentMovementBase = new Vector2(0f, 0f);
			MaxSpeedBase = new Vector2(0);
			AccelerationBase = new Vector2(1f);
			_healthTotal = 1;
			_healthCurrent = _healthTotal;
			_objectType = GuiObjectType.Environment;
		}

		public void Update(GameTime gameTime)
		{
			if (_healthCurrent <= 0)
			{
				if (!(this is Player))
				{
					Player.Sprint3TestScore += (float)GameStateManager.GameTimer.TotalMilliseconds;
				}
				if (_objectType == GuiObjectType.Environment)
					Level.RemoveGuiObject(this);
				else
					Level.RemoveGuiObject(this);

				if (!Level.GetAllUnPassableCharacterObjects().Values.SelectMany(l => l).Any(g => g is MovingCharacter))
				{
					Console.WriteLine("score = " + (Player.Sprint3TestScore / 1000));
					Console.WriteLine("Time = " + GameStateManager.GameTimer.TotalSeconds);
				}
				return;
			}
			// Apply modifiers.
			ModifierBase modifyAdd = new EmptyModifier(ModifyType.Add);
			ModifierBase modifyMult = new EmptyModifier(ModifyType.Multiply);
			AddCustomModifiers(gameTime, modifyAdd);
			MultiplyCustomModifiers(gameTime, modifyMult);

			Movement = MovementBase;
			Acceleration = AccelerationBase;
			MaxSpeed = MaxSpeedBase;
			CurrentMovement = CurrentMovementBase;
			_healthCurrent = MathHelper.Clamp(0, (_healthCurrent + modifyAdd.HealthTotal) * modifyMult.HealthTotal, HealthTotal);

			PreUpdate(gameTime);
			_previousPosition = Position;

			SetMovement(gameTime);
			float xCurMove = (GetXMovement() + modifyAdd.Movement.X) * modifyMult.Movement.X;
			float yCurMove = (GetYMovement() + modifyAdd.Movement.Y) * modifyMult.Movement.Y;
			CurrentMovement = new Vector2(xCurMove, yCurMove);
			Position = new Vector2(Position.X + CurrentMovement.X, Position.Y + CurrentMovement.Y);

			PostUpdate(gameTime);
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			PreDraw(gameTime, spriteBatch);
			if (ShowHitBox())
			{
				spriteBatch.Begin();
				Rectangle destinationRect = new Rectangle((int)Math.Round(Position.X), (int)Math.Round(Position.Y),
					(int)Math.Round(Size.X), (int)Math.Round(Size.Y)); //casting to int takes the floor
				spriteBatch.Draw(HitboxImage, destinationRect, _hitBoxColor);
				spriteBatch.End();
			}

			PostDraw(gameTime, spriteBatch);
		}

		// on a base to base case for debugging purposes.
		protected virtual bool ShowHitBox() { return true; }

		#region Mana Usage
		public void UseMana(float amount)
		{
			_manaCurrent = Math.Max(_manaCurrent - amount, 0); // Currently cannot go below 0.
		}
		public void RestoreMana(float amount)
		{
			_manaCurrent = Math.Min(_manaCurrent + amount, _manaTotal); // Cannot go above max.
		}
		#endregion

		#region Static Intersection

		public static IEnumerable<Tuple<DoubleVector2, MainGuiObject>> GetHitPlatforms(Dictionary<Group, List<MainGuiObject>> guiObjects, Vector2 prevPosition, Vector4 nextBounds, bool? isVertical = null)
		{
			return GetHitObjects(guiObjects, prevPosition, nextBounds, (p) => false, isVertical);
		}

		protected static List<Group> _IgnoredVerticalUpGroups = new List<Group>()
		{
			Group.BothPassable,
			Group.Passable,
			Group.PassableFromBottom, // Included becasuse we only care about moving "upwards"
			Group.VerticalPassable
		};
		protected static List<Group> _IgnoredVerticalDownGroups = new List<Group>()
		{
			Group.BothPassable,
			Group.Passable,
			Group.PassableFromTop, // Included becasuse we only care about moving "upwards"
			Group.VerticalPassable
		};

		protected static List<Group> _IgnoredHorizontalGroups = new List<Group>()
		{
			Group.BothPassable,
			Group.Passable,
			Group.HorizontalPassable
		};

		// if optional parameter isVertical is null, then 
		public static IEnumerable<Tuple<DoubleVector2, MainGuiObject>> GetHitObjects(Dictionary<Group, List<MainGuiObject>> guiObjects, Vector2 prevPosition, Vector4 nextBounds, Func<MainGuiObject, bool> shouldSkip, bool? isVertical = null)
		{
			return guiObjects.SelectMany(kv => kv.Value).Select(mgo =>
			{
				if (shouldSkip(mgo))
					return new { bounds = DoubleVector2.Zero, select = false, obj = MainGuiObject.EmptyVessel };
				var bounds = MainGuiObject.GetIntersectionDepth(nextBounds, mgo.Bounds);
				//return bounds != Vector2.Zero && bounds.Y <= 0 && bounds.Y >= -platHeight;
				bool select = bounds != DoubleVector2.Zero; // if isVertical == null, then select this
				if (isVertical != null)
				{
					if ((bool)isVertical)
					{
						float platHeight = mgo.Size.Y;
						// We need to figure out if we are landing on something (which has exceptions), or we ar hitting the ceiling.
						bool movingDown = prevPosition.Y < nextBounds.Y; // if this is the case, we only care about the bottom part of the object given
						if (movingDown)
						{
							nextBounds.Y = nextBounds.Y + nextBounds.Z / 2;
							nextBounds.Z = nextBounds.Z / 2;
							bounds = MainGuiObject.GetIntersectionDepth(nextBounds, mgo.Bounds);
						}
						// If we are moving down, then we want to land (unless otherwise told), otherwise we are going to move through the groups we are allowed to pass.
						//select = bounds != Vector2.Zero && bounds.Y <= 0 && bounds.Y >= -platHeight; // <- old

						select = (bounds != DoubleVector2.Zero && Math.Abs(bounds.Y) > 0 /*&& Math.Abs(bounds.Y) <= platHeight*/)
							&& (movingDown || !_IgnoredVerticalUpGroups.Contains(mgo.Group));
					}
					else // !isVertical, aka isHorizontal
					{
						float platWidth = mgo.Size.X;
						bool movingLeft = prevPosition.X > nextBounds.X;
						select = bounds != DoubleVector2.Zero && Math.Abs(bounds.X) > 0 && Math.Abs(bounds.X) <= platWidth;
					}
				}
				return new { bounds = bounds, select = select, obj = mgo };
			}).Where(o => o.select).Select(o => new Tuple<DoubleVector2, MainGuiObject>(o.bounds, o.obj));
		}

		public DoubleVector2 GetIntersectionDepth(MainGuiObject obj)
		{
			Vector4 thisBounds = this.Bounds;
			Vector4 thatBounds = obj.Bounds;
			return GetIntersectionDepth(thisBounds, thatBounds);
		}

		public static DoubleVector2 GetIntersectionDepth(Vector4 rectA, Vector4 rectB)
		{
			// Calculate half sizes.
			double halfWidthA = (double)rectA.W / 2.0;
			double halfHeightA = (double)rectA.Z / 2.0;
			double halfWidthB = (double)rectB.W / 2.0;
			double halfHeightB = (double)rectB.Z / 2.0;

			// Calculate centers.
			double centerAX = (double)rectA.X + halfWidthA;// new Vector2(rectA.X + halfWidthA, rectA.Y + halfHeightA);
			double centerAY = (double)rectA.Y + halfHeightA;// new Vector2(rectA.X + halfWidthA, rectA.Y + halfHeightA);
			double centerBX = (double)rectB.X + halfWidthB;// new Vector2(rectA.X + halfWidthA, rectA.Y + halfHeightA);
			double centerBY = (double)rectB.Y + halfHeightB;// new Vector2(rectA.X + halfWidthA, rectA.Y + halfHeightA);
			//double centerA = new Vector2(rectA.X + halfWidthA, rectA.Y + halfHeightA);
			//Vector2 centerB = new Vector2(rectB.X + halfWidthB, rectB.Y + halfHeightB);

			// Calculate current and minimum-non-intersecting distances between centers.
			double distanceX = centerAX - centerBX;
			double distanceY = centerAY - centerBY;
			double minDistanceX = halfWidthA + halfWidthB;
			double minDistanceY = halfHeightA + halfHeightB;

			// If we are not intersecting at all, return (0, 0).
			if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
				return DoubleVector2.Zero;

			// Calculate and return intersection depths.
			double depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
			double depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
			//return new Vector2((float)Math.Round(depthX, 2), (float)Math.Round(depthY, 2));
			return new DoubleVector2(depthX, depthY);
		}
		#endregion
	}
}
