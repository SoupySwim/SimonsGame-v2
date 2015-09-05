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
using System.Diagnostics;
using SimonsGame.Utility.ObjectAnimations;

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

	public enum GuiObjectState
	{
		// Normal will be as you'd expect.  Things attack you, walk around, you are affected by gravity, ect.
		Normal,

		// Teleport will ignore almost all input and transfer you to your destination... quickly.
		Teleport
	}

	public abstract class MainGuiObject : GuiVariables
	{
		public string Name;
		// Return an empty MainGuiObject, void of any important data.  It's a placeholder.
		public static MainGuiObject EmptyVessel { get { return null; } }

		// TEST
		//public static float PrinterTotal { get; set; }

		// How much mana you have currently
		protected float _manaCurrent;
		public float ManaCurrent { get { return _manaCurrent; } }

		// How much health you have currently
		protected float _healthCurrent;
		public float HealthCurrent { get { return _healthCurrent; } set { _healthCurrent = value; } }

		public float RegenAmount { get; set; }

		protected GuiObjectType _objectType;
		public GuiObjectType ObjectType { get { return _objectType; } }

		public bool IsStunned; // Similar to NotAcceptingControls.  However, broader spectrum and not to do with menus...

		// This will be created during initialization of a level.
		public HashSet<Guid> ZoneIds;

		protected MainGuiObject _lastTargetHitBy;

		protected Color _healthBarColor = new Color(0, 1, 0, .4f);

		private float _levelAnimationDamage;

		protected GuiObjectState _objState = GuiObjectState.Normal;

		// This is for functions in a map.
		public bool IsActiveForFunction = false;

		#region State Variables
		private Vector2 _teleportDestination = Vector2.Zero; // For Center
		private Vector2 _teleportBy = Vector2.Zero;
		public TickTimer _teleportTickTimer;

		#endregion

		#region Graphics
		public Vector2 Position;
		protected Vector2 _previousPosition;
		public Vector2 PreviousPosition { get { return _previousPosition; } }
		protected bool _showHealthBar = false;

		// In the future, this will be used to animate the object.
		protected Animator _animator;

		public int DrawImportant { get; set; }

		//  _____
		// |     |
		// |  *  |  <--- Center Position
		// |_____|
		public Vector2 Center { get { return Position + (_size / 2); } set { Position = value - (_size / 2); } }
		public Vector2 Size { get { return _size; } set { ExtraSizeManipulation(ref value); _size = value; } }
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
		public Vector2 KnockBackBase { get; set; }

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

		// Elemental info!
		public Dictionary<Element, float> ElementLevel { get; set; }
		public Dictionary<Element, float> ElementBase { get; set; }

		// A projectile may have a parent object.
		public MainGuiObject Parent { get; set; }

		// This is a list of items that the object has accrued.
		public List<ObtainableItem> ObtainableItems { get; set; }
		public bool IsMovable;


		#region Abstract Functions
		public abstract void PreUpdate(GameTime gameTime);
		public abstract void PostUpdate(GameTime gameTime);
		public abstract void PreDraw(GameTime gameTime, SpriteBatch spriteBatch);
		public abstract void PostDraw(GameTime gameTime, SpriteBatch spriteBatch, Player curPlayer);
		public abstract void SetMovement(GameTime gameTime);
		public abstract float GetXMovement();
		public abstract float GetYMovement();
		public abstract void HitByObject(MainGuiObject mgo, ModifierBase mb);
		protected virtual void AdditionalGroupChange(Group _group, Group newGroup) { }
		public virtual void ExtraSizeManipulation(ref Vector2 newSize) { }
		public virtual void SwitchTeam(Team newTeam)
		{
			_team = newTeam;
		}
		public virtual List<ModifierBase> AddCustomModifiers(GameTime gameTime, ModifierBase modifyAdd) { return new List<ModifierBase>(); }
		public virtual void MultiplyCustomModifiers(GameTime gameTime, ModifierBase modifyMult) { }
		#endregion

		public MainGuiObject(Vector2 position, Vector2 hitbox, Group group, Level level, string name)
		{
			_teleportTickTimer = new TickTimer(10, () => { _objState = GuiObjectState.Normal; FinishTeleport(); }, false);
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
			CurrentMovementBase = new Vector2(0f, 0f);
			MaxSpeedBase = new Vector2(0);
			AccelerationBase = new Vector2(1);
			_healthTotal = 1;
			_healthCurrent = _healthTotal;
			_objectType = GuiObjectType.Environment;
			Name = name;
			_lastTargetHitBy = null;
			_team = Team.None; // default to homeless for now...
			ObtainableItems = new List<ObtainableItem>();
			IsMovable = (ObjectType != GuiObjectType.Environment && ObjectType != GuiObjectType.Structure) || this.GetType().IsSubclassOf(typeof(PhysicsObject));
			ZoneIds = new HashSet<Guid>();
			ElementBase = new Dictionary<Element, float>()
			{
				{Element.Normal, 0},
				{Element.Plant, 0},
				{Element.Water, 0},
				{Element.Fire, 0},
				{Element.Lightning, 0},
				{Element.Metal, 0},
				{Element.Rock, 0},
			};
			ElementLevel = ElementBase.ToDictionary(kv => kv.Key, kv => kv.Value);
			DrawImportant = 0;
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
				CheckGameConditionsOnDeath();
				return;
			}
			// Apply modifiers.
			ModifierBase modifyAdd = new EmptyModifier(ModifyType.Add, this);
			ModifierBase modifyMult = new EmptyModifier(ModifyType.Multiply, this);
			List<ModifierBase> appliedAttacks = AddCustomModifiers(gameTime, modifyAdd);
			MultiplyCustomModifiers(gameTime, modifyMult);
			IsStunned = modifyAdd.PreventControls || modifyMult.PreventControls;

			Movement = MovementBase;
			MaxSpeed = (MaxSpeedBase + modifyAdd.MaxSpeed) * modifyMult.MaxSpeed;
			KnockBack = (KnockBackBase + modifyAdd.KnockBack) * modifyMult.KnockBack;
			Acceleration = (AccelerationBase + modifyAdd.Acceleration) * modifyMult.Acceleration * MaxSpeed;
			Acceleration.X = Math.Abs(Acceleration.X);
			Acceleration.Y = Math.Abs(Acceleration.Y);

			//CurrentMovement = CurrentMovementBase;
			_healthCurrent = Math.Min(_healthCurrent + RegenAmount, _healthTotal);
			float healthPrior = _healthCurrent;

			// The following line only accounts for healing.  Damage is done below this line.
			_healthCurrent = MathHelper.Clamp(_healthCurrent + modifyAdd.HealthTotal, 0, HealthTotal); // Don't amplify healing... yet!

			float largestDamageDone = 1;
			foreach (ModifierBase attack in appliedAttacks)
			{
				float damageDone = ApplyDamage(attack);
				if (damageDone < largestDamageDone) // damage is a negative number to health... remember that!
				{
					_lastTargetHitBy = attack.Owner;
					largestDamageDone = damageDone;
				}
				_healthCurrent = MathHelper.Clamp(_healthCurrent + damageDone, 0, HealthTotal);
			}

			_levelAnimationDamage += healthPrior - _healthCurrent;

			PreUpdate(gameTime);
			_previousPosition = Position;
			SetMovement(gameTime);
			float xCurMove = ((IsStunned ? 0 : GetXMovement()) + modifyAdd.Movement.X) * modifyMult.Movement.X;
			float yCurMove = ((IsStunned ? 0 : GetYMovement()) + modifyAdd.Movement.Y) * modifyMult.Movement.Y;
			xCurMove = (xCurMove < 0 ? -1 : 1) * Math.Min(Math.Abs(xCurMove), Acceleration.X);
			yCurMove = (yCurMove < 0 ? -1 : 1) * Math.Min(Math.Abs(yCurMove), Acceleration.Y);
			//if (this is Player && xCurMove > 4)
			//{
			//}

			// If you're going faster than the max speed, then all we can do is slow down
			if (Math.Abs(CurrentMovement.X) > Math.Abs(MaxSpeed.X))
			{
				CurrentMovement.X += CurrentMovement.X < 0 ? _knockBackRecoveryAcceleration : -_knockBackRecoveryAcceleration; // ? Acceleration.X : -Acceleration.X;
				if (xCurMove < 0 && CurrentMovement.X > 0 ||
					xCurMove > 0 && CurrentMovement.X < 0)
					CurrentMovement.X = CurrentMovement.X + xCurMove;
			}
			else
			{
				// If we are moving, but not accelerating, we should decelerate.
				if (CurrentMovement.X != 0 && xCurMove == 0)
					xCurMove = xCurMove != 0 ? xCurMove : (CurrentMovement.X < 0 ? -Math.Max(CurrentMovement.X, -Acceleration.X) : -Math.Min(CurrentMovement.X, Acceleration.X));
				CurrentMovement.X = xCurMove + CurrentMovement.X;
				CurrentMovement.X = CurrentMovement.X < 0 ? Math.Max(CurrentMovement.X, MaxSpeed.X < 0 ? MaxSpeed.X : -MaxSpeed.X) : Math.Min(CurrentMovement.X, MaxSpeed.X < 0 ? -MaxSpeed.X : MaxSpeed.X);
			}
			if (Math.Abs(CurrentMovement.Y) > Math.Abs(MaxSpeed.Y))
			{
				CurrentMovement.Y += CurrentMovement.Y < 0 ? _knockBackRecoveryAcceleration : -_knockBackRecoveryAcceleration; //Acceleration.Y : -Acceleration.Y;
				if (yCurMove < 0 && CurrentMovement.Y > 0 ||
					yCurMove > 0 && CurrentMovement.Y < 0)
					CurrentMovement.Y = CurrentMovement.Y + yCurMove;
			}
			else
			{
				// If we are moving, but not accelerating, we should decelerate.
				if (CurrentMovement.Y != 0 && yCurMove == 0)
					yCurMove = yCurMove != 0 ? yCurMove : (CurrentMovement.Y < 0 ? -Math.Max(CurrentMovement.Y, -Acceleration.Y) : -Math.Min(CurrentMovement.Y, Acceleration.Y));
				CurrentMovement.Y = yCurMove + CurrentMovement.Y;
				CurrentMovement.Y = CurrentMovement.Y < 0 ? Math.Max(CurrentMovement.Y, MaxSpeed.Y < 0 ? MaxSpeed.Y : -MaxSpeed.Y) : Math.Min(CurrentMovement.Y, MaxSpeed.Y < 0 ? -MaxSpeed.Y : MaxSpeed.Y);
			}

			// Only knock back objects that can move.
			if (IsMovable)
			{
				if (KnockBack.X != 0)
					CurrentMovement.X = (Math.Sign(CurrentMovement.X) == Math.Sign(KnockBack.X) ? CurrentMovement.X : 0) + KnockBack.X;
				if (KnockBack.Y != 0)
					CurrentMovement.Y = (Math.Sign(CurrentMovement.Y) == Math.Sign(KnockBack.Y) ? CurrentMovement.Y : 0) + KnockBack.Y;
			}

			if (_objState == GuiObjectState.Normal)
			{
				if (IsMovable || MaxSpeedBase != Vector2.Zero) // Added second check for things that are supposed to move (push ability).
					Position = Position + CurrentMovement; //new Vector2(Position.X + CurrentMovement.X, Position.Y + CurrentMovement.Y);
				else
					Movement = Vector2.Zero;
			}
			else if (_objState == GuiObjectState.Teleport)
			{
				Center = Center - _teleportBy;
				_teleportTickTimer.Update(gameTime);
			}

			_animator.Update(gameTime);

			PostUpdate(gameTime);
		}

		private float ApplyDamage(ModifierBase modifier)
		{
			// Don't worry about it if it doesn't matter.  May override for buffs.
			float damageAmount = modifier.HealthTotal;
			if (damageAmount == 0)
				return 0;

			Element affectedElement = modifier.Element.Item1;
			// If the attack is Normal, then ignore modifiers, nothing changes!
			if (affectedElement == Element.Normal)
				return damageAmount;

			float affectedElementDone = modifier.Element.Item2;
			float affectedElementAmount = ElementLevel[affectedElement];
			float affectedElementBase = ElementBase[affectedElement];
			float affectedElementAverage = (affectedElementAmount + affectedElementBase) / 2.0f;

			//Element relatedElement = affectedElement.GetRelatedElement();
			//float relatedElementAmount = ElementLevel[relatedElement];

			Element counterElement = affectedElement.GetCounterElement();
			float counterElementAmount = ElementLevel[counterElement];

			Element softCounterElement = affectedElement.GetSoftCounterElement();
			float softCounterElementAmount = ElementLevel[softCounterElement];


			float damagePercentage = Math.Abs(damageAmount / (_healthTotal / 2.0f)); // 60% of total health attack will be 120% multiplier!

			// First, let's change the magic amount of the affected element.
			// It's unaffected if the attack is less potent.
			float increaseAmount = affectedElementDone > affectedElementAverage ? affectedElementDone - affectedElementAverage : 0;
			ElementLevel[affectedElement] += increaseAmount * damagePercentage;
			float damageMultiplier = 1 + damagePercentage * (affectedElementDone > affectedElementAmount
				? affectedElementDone - affectedElementAmount // .4 - .3 = .1 increase multiplied by damagerPercentage of damage is applied
				: (affectedElementDone - affectedElementAmount) / 2); // (.4 - .5 = -.1) / 2 = .05 descrease multiplied by damagerPercentage of damage is applied

			// Now let's look at it's counters
			float percentDamageCountered = GetPercentageCountered(counterElementAmount, affectedElementDone);
			damageMultiplier -= percentDamageCountered * 2.0f / 3.0f; // Main counter can counter up to 2/3 of percentDamageCountered.
			float percentDamageSoftCountered = GetPercentageCountered(softCounterElementAmount, affectedElementDone);
			damageMultiplier -= percentDamageSoftCountered / 3.0f; // Soft counter can counter up to 1/3 of percentDamageCountered.

			// TODO must affect counters negatively.

			return damageAmount * damageMultiplier;
		}

		private float GetPercentageCountered(float counterElementAmount, float affectedElementDone)
		{
			float percentDamageCountered = 0;
			float baseLargeCountered = Math.Min(counterElementAmount - affectedElementDone, affectedElementDone + 1);
			float baseCountered = counterElementAmount - (affectedElementDone / 2.0f); // Max will be affectedElementDone because of first if statement

			// You've countered this well
			if (baseLargeCountered >= 0)
				percentDamageCountered = (.5f * baseLargeCountered) + .25f; // Can counter up to 75%!
			// You at least counter something...!
			else if (baseCountered >= 0)
				percentDamageCountered = .25f * baseCountered; // Can counter up to 25%!

			return percentDamageCountered;
		}

		private void CheckGameConditionsOnDeath()
		{
			if (ObjectType == GuiObjectType.Character && !(this is HealthCreep)) // All characters except health creeps.
			{
				if (!Level.GetAllCharacterObjects(Bounds).Any(g => !(g is HealthCreep) && !(g is Player))
					&& Level.GameStateManager.WinCondition == MainFiles.WinCondition.DefeatAllEnemies)
				{
					Level.GameStateManager.AddHighLight(new GameHighlight()
					{
						Description = string.Format("had the finishing blow"),
						Character = _lastTargetHitBy,
						TimeOccured = GameStateManager.GameTimer
					});
					Level.FinishedGame(_lastTargetHitBy);
				}
			}
			else if (GetType().IsAssignableFrom(typeof(StandardBase))
				&& Level.GetAllStructures().Count(g => g.GetType().IsAssignableFrom(typeof(StandardBase))) < 2 // Assuming one base is left, or the only base was destroyed
				&& Level.GameStateManager.WinCondition == MainFiles.WinCondition.DefeatBase)
			{
				Level.GameStateManager.AddHighLight(new GameHighlight()
				{
					Description = string.Format("had the finishing blow"),
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
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Player curPlayer = null)
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

			PostDraw(gameTime, spriteBatch, curPlayer);
		}

		public void GetLevelAnimations()
		{
			if (_levelAnimationDamage != 0)
				Level.AddLevelAnimation(new TextAnimation(string.Format("{0:0.0}", Math.Abs(_levelAnimationDamage)), _levelAnimationDamage > 0 ? Color.Red : Color.Green, Level, new Vector2(Center.X, Position.Y - 30)));
			_levelAnimationDamage = 0;
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
			return new Vector4(Bounds.X, (float)Math.Round(Bounds.Y, 3), 8, Bounds.W);
		}
		#endregion

		protected virtual SpriteEffects GetCurrentSpriteEffects()
		{
			return CurrentMovement.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		}

		// On a base to base case for debugging purposes.
		protected virtual bool ShowHitBox() { return true; }

		#region Mana Usage
		public void UseMana(float amount)
		{
			_manaCurrent = Math.Max(_manaCurrent - amount, 0); // Currently cannot go below 0.
		}
		public void RestoreMana(float amount)
		{
			_manaCurrent = Math.Min(_manaCurrent + amount, ManaTotal); // Cannot go above max.
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
						select = bounds != Vector2.Zero && Math.Abs(bounds.X) > 0; // && Math.Abs(bounds.X) <= platWidth; // Removed last check because it's possible to go past the platform, but still need to stop at it.
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
			if ((distanceX < 0 ? -1 * distanceX : distanceX) >= minDistanceX)
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

		public IEnumerable<T> GetObtainableItemsOfType<T>() where T : ObtainableItem
		{
			return ObtainableItems.OfType<T>();//.Where(oi => oi.GetType() == obtainableItemType);
		}

		public override bool Equals(Object obj1)
		{
			if (obj1 == null && this == null)
				return true;
			MainGuiObject mgo1 = obj1 as MainGuiObject;
			if (mgo1 == null || this == null)
				return false;
			return mgo1.Id == this.Id;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region virtual functions
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

		public virtual void PlayTeleportAnimation() { }
		public virtual void FinishTeleport() { }
		public virtual void Initialize() { }

		protected virtual void Died()
		{
			Level.RemoveGuiObject(this);
			GenericZone zone;
			//= Level.GetAllZones().FirstOrDefault(z => ZoneIds.Contains(z.Id)); // Can't have behavior zones within jungle zones...
			//if (zone != null)
			//	zone.CharacterDied(this);
			if (ZoneIds.Any() && Level.GetAllZones().TryGetValue(ZoneIds.FirstOrDefault(), out zone))
				zone.CharacterDied(this);
			foreach (ObtainableItem item in ObtainableItems)
			{
				Type carrierType = item.CarrierType;
				if (carrierType != null)
				{
					Vector2 itemSize = item.DefaultSize;
					Level.AddGuiObject(Activator.CreateInstance(carrierType, new object[] { this, item }) as MainGuiObject);
				}
			}
		}

		public virtual bool IsHitBy(MainGuiObject mgo)
		{
			return true;
		}

		public virtual void FinalizeSize() // in map editor
		{
			Vector2 newPosition = Position;
			Vector2 newSize = Size;
			if (Size.X < 0)
			{
				newPosition.X = Position.X + Size.X;
				newSize.X = Math.Abs(Size.X);
			}
			if (Size.Y < 0)
			{
				newPosition.Y = Position.Y + Size.Y;
				newSize.Y = Math.Abs(Size.Y);
			}
			Position = newPosition;
			Size = newSize;
		}

		#endregion

		public void TeleportTo(Vector2 position, int teleportTickCount = 10, bool showAnimation = false)
		{
			_objState = GuiObjectState.Teleport;
			_teleportBy = (Center - position) / teleportTickCount;
			_teleportDestination = position;
			_teleportTickTimer.TickTotal = teleportTickCount;
			_teleportTickTimer.Restart();
			if (showAnimation)
				PlayTeleportAnimation();
		}

	}
}
