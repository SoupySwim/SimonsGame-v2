using System;
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

	public abstract class MainGuiObject : GuiVariables
	{
		protected Guid _guid;
		public Guid Id { get { return _guid; } }

		// Return an empty MainGuiObject, void of any important data.  It's a placeholder.
		public static MainGuiObject EmptyVessel { get { return null; } }


		// In the future, this will be used to animate the object.
		protected Animator _animator;

		// How much mana you have total (used for certain magic)
		protected float _manaTotal;
		public float ManaTotal { get { return _manaTotal; } }
		// How much mana you have currently
		protected float _manaCurrent;
		public float ManaCurrent { get { return _manaCurrent; } }

		#region Graphics
		public Vector2 Position { get; set; }

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

		#region Modifiers
		protected List<ModifierBase> Modifiers;
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

			Modifiers = new List<ModifierBase>();


			// Init to 0 (non-movable objects)
			MovementBase = new Vector2(0f, 0f);
			AccelerationBase = new Vector2(0f, 0f);
			CurrentMovementBase = new Vector2(0f, 0f);
			MaxSpeedBase = new Vector2(0);
			AccelerationBase = new Vector2(1f);
		}

		public void Update(GameTime gameTime)
		{
			// Apply modifiers.
			ModifierBase modifyAdd = new EmptyModifier(ModifyType.Add);
			ModifierBase modifyMult = new EmptyModifier(ModifyType.Multiply);
			Modifiers.Where(m => m.Type == ModifyType.Add).ToList().ForEach(m => modifyAdd += m);
			Modifiers.Where(m => m.Type == ModifyType.Multiply).ToList().ForEach(m => modifyMult *= m);
			Modifiers.Where(m => m.IsExpired(gameTime)).ToList().ForEach(m => Modifiers.Remove(m));
			AddCustomModifiers(gameTime, modifyAdd);
			MultiplyCustomModifiers(gameTime, modifyMult);

			Movement = MovementBase;
			Acceleration = AccelerationBase;
			MaxSpeed = MaxSpeedBase;
			CurrentMovement = CurrentMovementBase;

			PreUpdate(gameTime);

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
				Rectangle destinationRect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
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

		public static IEnumerable<Tuple<Vector2, MainGuiObject>> GetHitPlatforms(Dictionary<Group, List<MainGuiObject>> guiObjects, Vector4 nextBounds, bool? isVertical = null)
		{
			return GetHitPlatforms(guiObjects, nextBounds, (p) => false, isVertical);
		}
		public static IEnumerable<Tuple<Vector2, MainGuiObject>> GetHitPlatforms(Dictionary<Group, List<MainGuiObject>> guiObjects, Vector4 nextBounds, Func<MainGuiObject, bool> shouldSkip, bool? isVertical = null)
		{
			return guiObjects.SelectMany(kv => kv.Value).Select(mgo =>
			{
				if (shouldSkip(mgo))
					return new { bounds = Vector2.Zero, select = false, obj = MainGuiObject.EmptyVessel };
				var bounds = MainGuiObject.GetIntersectionDepth(nextBounds, mgo.Bounds);
				//return bounds != Vector2.Zero && bounds.Y <= 0 && bounds.Y >= -platHeight;
				bool select = bounds != Vector2.Zero; // if isVertical == null, then select this
				if (isVertical != null)
				{
					if ((bool)isVertical)
					{

						float platHeight = mgo.Size.Y;
						select = bounds != Vector2.Zero && bounds.Y <= 0 && bounds.Y >= -platHeight;
					}
					else // !isVertical, aka isHorizontal
					{
						float platWidth = mgo.Size.X;
						select = bounds != Vector2.Zero && Math.Abs(bounds.X) > 0 && Math.Abs(bounds.X) <= platWidth;
					}
				}
				return new { bounds = bounds, select = select, obj = mgo };
			}).Where(o => o.select).Select(o => new Tuple<Vector2, MainGuiObject>(o.bounds, o.obj));
		}
		public Vector2 GetIntersectionDepth(MainGuiObject obj)
		{
			Vector4 thisBounds = this.Bounds;
			Vector4 thatBounds = obj.Bounds;
			return GetIntersectionDepth(thisBounds, thatBounds);
		}
		public static Vector2 GetIntersectionDepth(Vector4 rectA, Vector4 rectB)
		{
			// Calculate half sizes.
			float halfWidthA = rectA.W / 2.0f;
			float halfHeightA = rectA.Z / 2.0f;
			float halfWidthB = rectB.W / 2.0f;
			float halfHeightB = rectB.Z / 2.0f;

			// Calculate centers.
			Vector2 centerA = new Vector2(rectA.X + halfWidthA, rectA.Y + halfHeightA);
			Vector2 centerB = new Vector2(rectB.X + halfWidthB, rectB.Y + halfHeightB);

			// Calculate current and minimum-non-intersecting distances between centers.
			float distanceX = centerA.X - centerB.X;
			float distanceY = centerA.Y - centerB.Y;
			float minDistanceX = halfWidthA + halfWidthB;
			float minDistanceY = halfHeightA + halfHeightB;

			// If we are not intersecting at all, return (0, 0).
			if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
				return Vector2.Zero;

			// Calculate and return intersection depths.
			float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
			float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
			return new Vector2(depthX, depthY);
		}
		#endregion
	}
}
