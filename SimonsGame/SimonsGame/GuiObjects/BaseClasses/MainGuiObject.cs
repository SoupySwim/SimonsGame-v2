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
using SimonsGame.MainFiles.InGame;

namespace SimonsGame.GuiObjects
{
	public enum GuiObjectType
	{
		Environment,
		Character,
		Player,
		Attack,
		Structure
	}

	public abstract class MainGuiObject : GuiVariables
	{
		public string Name { get; set; }
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


		protected MainGuiObject _lastTargetHitBy;

		#region Graphics
		public Vector2 Position { get; set; }
		protected Vector2 _previousPosition;
		public Vector2 PreviousPosition { get { return _previousPosition; } }

		//  _____
		// |     |
		// |  *  |  <--- Center Position
		// |_____|
		public Vector2 Center { get { return new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y / 2); } set { Position = new Vector2(value.X - Size.X / 2, value.Y - Size.Y / 2); } }
		public Vector2 Size { get { return _size; } set { ExtraSizeManipulation(value); _size = value; } }
		private Vector2 _size;
		public Texture2D HitboxImage { get; set; }
		protected Color _hitBoxColor = new Color(1f, 1f, 1f, .8f);
		public Color HitBoxColor { get { return _hitBoxColor; } set { _hitBoxColor = value; } }
		public Vector4 Bounds { get { return new Vector4(Position.X, Position.Y, Size.Y, Size.X); } }
		public virtual Vector4 HitBoxBounds { get { return Bounds; } }//{ get { return new Vector4(Position.X - 5, Position.Y - 5, Size.Y + 10, Size.X + 10); } }
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

		public Group Group
		{
			get { return _group; }
			set
			{
				if (Level != null)
					Level.ChangeGroup(this, value);
				AdditionalGroupChange(_group, value);
				_group = value;
			}
		}

		private Group _group { get; set; }
		public Team Team { get { return _team; } set { SwitchTeam(value); } }
		protected Team _team;

		public Level Level { get; set; }


		// A projectile may have a parent object.
		public MainGuiObject Parent { get; set; }


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
		protected virtual void AdditionalGroupChange(Group _group, Group newGroup) { }
		public virtual void ExtraSizeManipulation(Vector2 newSize) { }
		public virtual void SwitchTeam(Team newTeam)
		{
			_team = newTeam;
		}
		#endregion

		public MainGuiObject(Vector2 position, Vector2 hitbox, Group group, Level level, string name)
		{
			_guid = Guid.NewGuid();
			Position = position;
			_size = hitbox; // only this once do we use _size...!
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
			Name = name;
			_lastTargetHitBy = null;
			_team = Team.Neutral; // default to homeless for now...
		}

		public void Update(GameTime gameTime)
		{
			if (_healthCurrent <= 0)
			{
				Died();
				if (this is MovingCharacter || this is WallRunner)
				{
					if (Player.Sprint3TestScore == 0)
					{
						Level.GameStateManager.AddHighLight(new GameHighlight()
						{
							Description = "First Kill",
							Character = _lastTargetHitBy,
							TimeOccured = GameStateManager.GameTimer
						});
					}
					if (!Level.GetAllUnPassableCharacterObjects().Values.SelectMany(l => l).Any(g => g is MovingCharacter || g is WallRunner)
						&& Level.GameStateManager.WinCondition == MainFiles.WinCondition.DefeatAllEnemies)
					{
						Level.GameStateManager.AddHighLight(new GameHighlight()
						{
							Description = string.Format("Finished with Score = {0}", Player.Sprint3TestScore),
							Character = _lastTargetHitBy,
							TimeOccured = GameStateManager.GameTimer
						});
						Level.FinishedGame((Player)_lastTargetHitBy);
					}
					Player.Sprint3TestScore += (float)GameStateManager.GameTimer.TotalMilliseconds;
				}
				else if (GetType().IsAssignableFrom(typeof(StandardBase))
					&& Level.GetAllStructures().Count(g => g.GetType().IsAssignableFrom(typeof(StandardBase))) < 2 // Assuming one base is left, or the only base was destroyed
					&& Level.GameStateManager.WinCondition == MainFiles.WinCondition.DefeatBase)
				{
					Level.GameStateManager.AddHighLight(new GameHighlight()
					{
						Description = string.Format("Finished with Score = {0}", Player.Sprint3TestScore),
						Character = _lastTargetHitBy,
						TimeOccured = GameStateManager.GameTimer
					});
					Level.FinishedGame((Player)_lastTargetHitBy);
				}
				else if (this is Player)
				{
					Level.GameStateManager.AddHighLight(new GameHighlight()
					{
						Description = this.Name + " Got Defeated",
						Character = _lastTargetHitBy,
						TimeOccured = GameStateManager.GameTimer
					});
				}
				return;
			}
			// Apply modifiers.
			ModifierBase modifyAdd = new EmptyModifier(ModifyType.Add, this);
			ModifierBase modifyMult = new EmptyModifier(ModifyType.Multiply, this);
			AddCustomModifiers(gameTime, modifyAdd);
			MultiplyCustomModifiers(gameTime, modifyMult);

			Movement = MovementBase;
			Acceleration = AccelerationBase;
			MaxSpeed = MaxSpeedBase;
			CurrentMovement = CurrentMovementBase;
			float healthPrior = _healthCurrent;

			// Target hit by has to actually deal damage...
			if (Math.Abs(modifyAdd.HealthTotal) + 1 > modifyMult.HealthTotal) // 1 is not greater than 1
				_lastTargetHitBy = modifyAdd.Owner;
			else if (modifyMult.HealthTotal != 1)
				_lastTargetHitBy = modifyMult.Owner;

			_healthCurrent = MathHelper.Clamp(0, (_healthCurrent + modifyAdd.HealthTotal) * modifyMult.HealthTotal, HealthTotal);
			if (healthPrior > _healthCurrent)
				Level.AddLevelAnimation(new TextAnimation("" + (healthPrior - _healthCurrent), Color.Red, Level, new Vector2(Center.X, Position.Y - 30)));
			else if (healthPrior < _healthCurrent)
				Level.AddLevelAnimation(new TextAnimation("" + (_healthCurrent - healthPrior), Color.Green, Level, new Vector2(Center.X, Position.Y - 30)));

			PreUpdate(gameTime);
			_previousPosition = Position;

			SetMovement(gameTime);
			double xCurMove = (GetXMovement() + modifyAdd.Movement.X) * modifyMult.Movement.X;
			double yCurMove = (GetYMovement() + modifyAdd.Movement.Y) * modifyMult.Movement.Y;
			CurrentMovement = new Vector2((float)xCurMove, (float)yCurMove);
			Position = new Vector2(Position.X + CurrentMovement.X, Position.Y + CurrentMovement.Y);

			_animator.Update(gameTime);

			PostUpdate(gameTime);
		}

		protected virtual void Died()
		{
			Level.RemoveGuiObject(this);
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			PreDraw(gameTime, spriteBatch);
			//spriteBatch.Begin();
			//if (ShowHitBox())
			//{
			//	Rectangle destinationRect = new Rectangle((int)Math.Round(Position.X), (int)Math.Round(Position.Y),
			//		(int)Math.Round(Size.X), (int)Math.Round(Size.Y)); //casting to int takes the floor
			//	spriteBatch.Draw(HitboxImage, destinationRect, _hitBoxColor);
			//}

			_animator.Draw(gameTime, spriteBatch, Position, GetCurrentSpriteEffects());
			//spriteBatch.End();

			PostDraw(gameTime, spriteBatch);
		}

		protected virtual SpriteEffects GetCurrentSpriteEffects()
		{
			return CurrentMovement.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
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

		public IEnumerable<Tuple<DoubleVector2, MainGuiObject>> GetHitPlatforms(Dictionary<Group, List<MainGuiObject>> guiObjects, Vector4 nextBounds, bool? isVertical = null)
		{
			return GetHitObjects(guiObjects, nextBounds, (p) => false, isVertical);
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
		public IEnumerable<Tuple<DoubleVector2, MainGuiObject>> GetHitObjects(Dictionary<Group, List<MainGuiObject>> guiObjects, Vector4 nextBounds, Func<MainGuiObject, bool> shouldSkip, bool? isVertical = null)
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
						bool movingDown = _previousPosition.Y < nextBounds.Y; // if this is the case, we only care about the bottom part of the object given
						if (movingDown)
						{
							// if the object is moving down, then we don't want to push anything downwards with it.
							// unless it's impassable.
							if (/*mgo.CurrentMovement.Y > 0 &&*/ _IgnoredVerticalUpGroups.Contains(mgo.Group) && bounds.Y > 0)
							{
								bounds = DoubleVector2.Zero;
							}
							//nextBounds.Y = nextBounds.Y + nextBounds.Z / 2;
							//nextBounds.Z = nextBounds.Z / 2;
							//bounds = MainGuiObject.GetIntersectionDepth(nextBounds, mgo.Bounds);
						}
						// If we are moving down, then we want to land (unless otherwise told), otherwise we are going to move through the groups we are allowed to pass.
						//select = bounds != Vector2.Zero && bounds.Y <= 0 && bounds.Y >= -platHeight; // <- old

						select = (bounds != DoubleVector2.Zero && Math.Abs(bounds.Y) > 0 /*&& Math.Abs(bounds.Y) <= platHeight*/)
							&& (movingDown || !GetIgnoredVerticalGroups(_IgnoredVerticalUpGroups).Contains(mgo.Group));
					}
					else // !isVertical, aka isHorizontal
					{
						float platWidth = mgo.Size.X;
						bool movingLeft = _previousPosition.X > nextBounds.X;
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
		protected virtual Dictionary<Group, List<MainGuiObject>> GetAllVerticalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		{
			return guiObjects;
		}

		protected virtual List<Group> GetIgnoredHorizontalGroups(List<Group> suggestedGroups)
		{
			return suggestedGroups;
		}

		protected virtual List<Group> GetIgnoredVerticalGroups(List<Group> suggestedGroups)
		{
			return suggestedGroups;
		}
		#endregion


		public virtual void SwitchDirections() { }
		public virtual string GetDirectionalText() { return ""; }
		public virtual bool DidSwitchDirection() { return false; }
	}
}
