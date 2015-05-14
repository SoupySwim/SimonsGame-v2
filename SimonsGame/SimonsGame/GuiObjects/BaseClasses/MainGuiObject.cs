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
using SimonsGame.MapEditor;
using SimonsGame.MainFiles;
using SimonsGame.GuiObjects.Zones;

namespace SimonsGame.GuiObjects
{
	public enum GuiObjectType
	{
		Environment,
		Character,
		Player,
		Attack,
		Structure,
		Teleporter,
		Zone,
	}

	public abstract class MainGuiObject : GuiVariables
	{
		public string Name;
		// Return an empty MainGuiObject, void of any important data.  It's a placeholder.
		public static MainGuiObject EmptyVessel { get { return null; } }



		// How much mana you have currently
		protected float _manaCurrent;
		public float ManaCurrent { get { return _manaCurrent; } }

		// How much health you have currently
		protected float _healthCurrent;
		public float HealthCurrent { get { return _healthCurrent; } }

		protected GuiObjectType _objectType;
		public GuiObjectType ObjectType { get { return _objectType; } }

		public bool IsStunned; // Similar to NotAcceptingControls.  However, broader spectrum and not to do with menus...

		// This will be created during initialization of a level.
		public HashSet<Guid> ZoneIds;

		protected MainGuiObject _lastTargetHitBy;

		#region Graphics
		public Vector2 Position;
		protected Vector2 _previousPosition;
		public Vector2 PreviousPosition { get { return _previousPosition; } }
		protected bool _showHealthBar = false;

		// In the future, this will be used to animate the object.
		protected Animator _animator;

		//  _____
		// |     |
		// |  *  |  <--- Center Position
		// |_____|
		public Vector2 Center { get { return Position + (_size / 2); } set { Position = new Vector2(value.X - _size.X / 2, value.Y - _size.Y / 2); } }
		public Vector2 Size { get { return _size; } set { ExtraSizeManipulation(value); _size = value; } }
		protected Vector2 _size;
		public Texture2D HitboxImage;
		protected Color _hitBoxColor = new Color(1f, 1f, 1f, .8f);
		public Color HitBoxColor { get { return _hitBoxColor; } set { _hitBoxColor = value; } }

		private Vector4 _boundsBase = new Vector4();
		public Vector4 Bounds
		{
			get
			{
				_boundsBase.X = Position.X;
				_boundsBase.Y = Position.Y;
				_boundsBase.W = _size.X;
				_boundsBase.Z = _size.Y;
				return _boundsBase;
			}
		}
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

		// This is a list of items that the object has accrued.
		public List<ObtainableItem> ObtainableItems { get; set; }
		public bool IsMovable;


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
			IsStunned = false;
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
			ObtainableItems = new List<ObtainableItem>();
			IsMovable = (ObjectType != GuiObjectType.Environment && ObjectType != GuiObjectType.Structure) || this.GetType().IsSubclassOf(typeof(PhysicsObject));
			ZoneIds = new HashSet<Guid>();
		}

		public object Clone()
		{
			MainGuiObject mgo = MemberwiseClone() as MainGuiObject;
			mgo._guid = Guid.NewGuid();
			return mgo;
		}

		public void Update(GameTime gameTime)
		{
			if (_healthCurrent <= 0)
			{
				Died();
				if (ObjectType == GuiObjectType.Character && !(this is HealthCreep)) // All characters except health creeps.
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
					if (!Level.GetAllCharacterObjects().Any(g => !(g is HealthCreep) && !(g is Player))
						&& Level.GameStateManager.WinCondition == MainFiles.WinCondition.DefeatAllEnemies)
					{
						Level.GameStateManager.AddHighLight(new GameHighlight()
						{
							Description = string.Format("Finished with Score = {0}", Player.Sprint3TestScore),
							Character = _lastTargetHitBy,
							TimeOccured = GameStateManager.GameTimer
						});
						Level.FinishedGame(_lastTargetHitBy);
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
					Level.FinishedGame(_lastTargetHitBy);
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
			IsStunned = modifyAdd.PreventControls || modifyMult.PreventControls;

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

			_healthCurrent = MathHelper.Clamp((_healthCurrent + modifyAdd.HealthTotal) * modifyMult.HealthTotal, 0, HealthTotal);
			float healthDifference = healthPrior - _healthCurrent;
			if (healthDifference > 0)
				Level.AddLevelAnimation(new TextAnimation("" + (healthPrior - _healthCurrent), Color.Red, Level, new Vector2(Center.X, Position.Y - 30)));
			else if (healthDifference < -20) // Don't show the little guys  Should probably add them up...
				Level.AddLevelAnimation(new TextAnimation("" + (_healthCurrent - healthPrior), Color.Green, Level, new Vector2(Center.X, Position.Y - 30)));

			PreUpdate(gameTime);
			_previousPosition = Position;

			SetMovement(gameTime);
			float xCurMove = ((IsStunned ? 0 : GetXMovement()) + modifyAdd.Movement.X) * modifyMult.Movement.X;
			float yCurMove = ((IsStunned ? 0 : GetYMovement()) + modifyAdd.Movement.Y) * modifyMult.Movement.Y;
			CurrentMovement = new Vector2((float)xCurMove, (float)yCurMove); // only do this if you aren't stunned!
			Position = new Vector2(Position.X + CurrentMovement.X, Position.Y + CurrentMovement.Y);

			_animator.Update(gameTime);

			PostUpdate(gameTime);
		}

		protected virtual void Died()
		{
			Level.RemoveGuiObject(this);
			GenericZone zone = Level.GetAllZones().FirstOrDefault(z => ZoneIds.Contains(z.Id)); // Can't have behavior zones within jungle zones...
			if (zone != null)
				zone.CharacterDied(this);
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			PreDraw(gameTime, spriteBatch);
			//spriteBatch.Begin();
			if (ShowHitBox())
			{
				Rectangle destinationRect = new Rectangle((int)Math.Round(Position.X), (int)Math.Round(Position.Y),
					(int)Math.Round(_size.X), (int)Math.Round(_size.Y)); //casting to int takes the floor
				spriteBatch.Draw(HitboxImage, destinationRect, _hitBoxColor);
			}

			_animator.Draw(gameTime, spriteBatch, Position, GetCurrentSpriteEffects());
			if (_showHealthBar)
				GlobalGuiObjects.DrawRatioBar(gameTime, spriteBatch, GetHealthBarBounds(), HealthCurrent, HealthTotal, Color.Green, GetHealthTickAmount(), false, ShowHealthTicks());

			//spriteBatch.End();

			PostDraw(gameTime, spriteBatch);
		}

		#region HealthBarInfo
		protected virtual float GetHealthTickAmount()
		{
			return 100;
		}
		protected virtual bool ShowHealthTicks()
		{
			return true;
		}

		protected virtual Vector4 GetHealthBarBounds()
		{
			return new Vector4(Bounds.X, Bounds.Y, 10, Bounds.W);
		}
		#endregion

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

		public IEnumerable<Tuple<Vector2, MainGuiObject>> GetHitPlatforms(IEnumerable<MainGuiObject> guiObjects, Vector4 nextBounds, bool? isVertical = null)
		{
			return GetHitObjects(guiObjects, nextBounds, isVertical);
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
		public IEnumerable<Tuple<Vector2, MainGuiObject>> GetHitObjects(IEnumerable<MainGuiObject> guiObjects, Vector4 nextBounds, bool? isVertical = null)
		{
			return guiObjects.Select(mgo =>
			{
				var bounds = MainGuiObject.GetIntersectionDepth(nextBounds, mgo.Bounds);
				//return bounds != Vector2.Zero && bounds.Y <= 0 && bounds.Y >= -platHeight;
				bool select = bounds != Vector2.Zero; // if isVertical == null, then select this
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
								bounds = Vector2.Zero;
							}
							//nextBounds.Y = nextBounds.Y + nextBounds.Z / 2;
							//nextBounds.Z = nextBounds.Z / 2;
							//bounds = MainGuiObject.GetIntersectionDepth(nextBounds, mgo.Bounds);
						}
						// If we are moving down, then we want to land (unless otherwise told), otherwise we are going to move through the groups we are allowed to pass.
						//select = bounds != Vector2.Zero && bounds.Y <= 0 && bounds.Y >= -platHeight; // <- old

						select = (bounds != Vector2.Zero && Math.Abs(bounds.Y) > 0 /*&& Math.Abs(bounds.Y) <= platHeight*/)
							&& (movingDown || !GetIgnoredVerticalGroups(_IgnoredVerticalUpGroups).Contains(mgo.Group));
					}
					else // !isVertical, aka isHorizontal
					{
						float platWidth = mgo.Size.X;
						bool movingLeft = _previousPosition.X > nextBounds.X;
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
			float halfWidthB = rectB.W / 2.0f;

			// Calculate centers.
			float centerAX = (float)rectA.X + halfWidthA;
			float centerBX = (float)rectB.X + halfWidthB;

			// Calculate current and minimum-non-intersecting distances between centers.
			float distanceX = centerAX - centerBX;
			float minDistanceX = halfWidthA + halfWidthB;

			// If we are not intersecting at all, return (0, 0).
			if (Math.Abs(distanceX) >= minDistanceX)
				return Vector2.Zero;

			float halfHeightA = rectA.Z / 2.0f;
			float halfHeightB = rectB.Z / 2.0f;
			float centerAY = (float)rectA.Y + halfHeightA;
			float centerBY = (float)rectB.Y + halfHeightB;
			float distanceY = centerAY - centerBY;
			float minDistanceY = halfHeightA + halfHeightB;

			if (Math.Abs(distanceY) >= minDistanceY)
				return Vector2.Zero;

			// Calculate and return intersection depths.
			float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
			float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
			//return new Vector2((float)Math.Round(depthX, 2), (float)Math.Round(depthY, 2));
			return new Vector2(depthX, depthY);
		}
		protected virtual IEnumerable<MainGuiObject> GetAllVerticalPassableGroups(IEnumerable<MainGuiObject> guiObjects)
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


		public virtual bool CanPushObjects()
		{
			return IsMovable && Group != Group.Passable;
		}

		public virtual void SwitchDirections() { }
		public virtual string GetDirectionalText() { return ""; }
		public virtual bool DidSwitchDirection() { return false; }
		public virtual string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.Direction)
				return GetDirectionalText();
			return "";
		}
		public virtual string GetSpecialTitle(ButtonType bType) { return ""; }
		public virtual void ModifySpecialText(ButtonType bType, bool moveRight /* as opposed to move left*/)
		{
			if (bType == ButtonType.Direction)
				SwitchDirections();
		}
		public virtual int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.Direction)
				return DidSwitchDirection() ? 1 : 0;
			return 0;
		}
		public virtual void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.Direction && value == 1)
				SwitchDirections();
		}
		public IEnumerable<T> GetObtainableItemsOfType<T>() where T : ObtainableItem
		{
			return ObtainableItems.OfType<T>();//.Where(oi => oi.GetType() == obtainableItemType);
		}

		public void UseKey(SmallKey key)
		{
			key.DecrementQuantity();
			if (key.IsGone())
				ObtainableItems.Remove(key);
		}

		public void ObtainItem(ObtainableItem newOI)
		{
			ObtainableItem availableOI = ObtainableItems.FirstOrDefault(oi => oi.Id == newOI.Id);
			if (availableOI != null)
				availableOI.IncrementQuantity(newOI.Quantity);
			else
				ObtainableItems.Add(newOI);
		}

		public virtual void TriggerBehavior(BehaviorZone _behaviorZone) { }
	}
}
