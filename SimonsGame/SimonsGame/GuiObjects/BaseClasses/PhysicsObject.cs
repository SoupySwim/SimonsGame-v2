using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.GuiObjects
{
	public enum Orientation
	{
		Horizontal,
		Vertical
	}

	public abstract class PhysicsObject : MainGuiObject
	{
		protected AbilityManager _abilityManager;
		public AbilityManager AbilityManager { get { return _abilityManager; } }

		private bool _isLandedOverride = false;
		public bool IsOnLadder { get { return _isLandedOverride; } set { _isLandedOverride = value; } }
		public bool IsLanded { get { return _isLandedOverride || PrimaryOverlapObjects[Orientation.Vertical].Any(mgo => BasePosition.Y <= mgo.Position.Y + 20); } set { _isLandedOverride = value; } }
		private Dictionary<Orientation, List<MainGuiObject>> _primaryOverlapObjects;
		public Dictionary<Orientation, List<MainGuiObject>> PrimaryOverlapObjects { get { return _primaryOverlapObjects; } }
		protected bool StopGravity { get; set; }
		public bool VerticalPass { get; protected set; }

		public float PassiveExperienceGain = 0;
		public float PassiveExperienceGainMultiplier = 1;

		//  _____
		// |     |
		// |     |
		// |__*__|   <--- Base Position
		public Vector2 _basePositionBuffer = new Vector2(.5f, 1);
		public Vector2 BasePosition
		{
			get { return Position + (_size * _basePositionBuffer); }//new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y); }
			set { Position = new Vector2(value.X - _size.X / 2, value.Y - _size.Y); }
		}

		public PhysicsObject(Vector2 position, Vector2 hitbox, Group group, Level level, string name)
			: base(position, hitbox, group, level, name)
		{
			MaxSpeedBase = IsMovable ? new Vector2(AverageSpeed.X / 2.8f, AverageSpeed.Y) : Vector2.Zero;
			AccelerationBase = new Vector2(.035f, .03f);
			StopGravity = false;
			VerticalPass = false;
			_abilityManager = new AbilityManager(this, new Dictionary<KnownAbility, List<PlayerAbilityInfo>>(), AvailableButtons.None);
			_objectType = GuiObjectType.Character;
			_primaryOverlapObjects = new Dictionary<Orientation, List<MainGuiObject>>()
				{
					{Orientation.Vertical, new List<MainGuiObject>()},
					{Orientation.Horizontal, new List<MainGuiObject>()},
				};
			DrawImportant = 3;
		}

		// This is where all the physics logic gets set.
		public override void PreUpdate(GameTime gameTime)
		{
			_abilityManager.CheckKnownAbilities(gameTime);
		}

		public override void PostUpdate(GameTime gameTime)
		{
			IEnumerable<MainGuiObject> guiObjects = Level.GetPossiblyHitEnvironmentObjects(this.Bounds).Where(mgo => mgo.IsHitBy(this));
			/////////////////////////////////////////////
			// how to stop me from hitting the ceiling //
			/////////////////////////////////////////////
			_primaryOverlapObjects = new Dictionary<Orientation, List<MainGuiObject>>()
				{
					{Orientation.Vertical, new List<MainGuiObject>()},
					{Orientation.Horizontal, new List<MainGuiObject>()},
				};
			//IEnumerable<MainGuiObject> guiObjects = Level.GetAllUnPassableEnvironmentObjects();

			Vector2 currentMovement = Position - PreviousPosition;
			ApplyVerticalCollision(guiObjects);
			ApplyHorizontalCollision(guiObjects);
			if (_objState != GuiObjectState.Teleport)
				ApplyCharacterCollision(guiObjects); // Other character collision:

			foreach (var kv in _primaryOverlapObjects)
				kv.Value.ForEach(mgo => mgo.HitByObject(this, null));

			// If you are off the screen, then you are dead!
			if (Position.X + _size.X < 0 || Position.Y + _size.Y < 0 || Position.X > Level.Size.X || Position.Y > Level.Size.Y)
				Died();
			IsLanded = false;
		}
		public void ApplyVerticalCollision(IEnumerable<MainGuiObject> guiObjects)
		{
			//// If we already have a key, then we don't need to look for another.
			//if (_primaryOverlapObjects[Orientation.Vertical].Any())
			//	return;
			Vector4 allBounds = this.HitBoxBounds;
			float curMovY = Position.Y - _previousPosition.Y;
			if (curMovY > 0)
			{
				allBounds.Y -= curMovY;
				allBounds.Z += curMovY;
			}
			else if (curMovY < 0)
				allBounds.Z -= curMovY;

			IEnumerable<Tuple<Vector2, MainGuiObject>> verticallyHitPlatforms = GetHitObjects(GetAllVerticalPassableGroups(guiObjects), this.HitBoxBounds, true).Where(tup => (!VerticalPass || !_IgnoredVerticalDownGroups.Contains(tup.Item2.Group)) && tup.Item2.Id != this.Id && tup.Item2.Group != Group.Passable);
			Tuple<Vector2, MainGuiObject> verticallyHitPlatformTuple = verticallyHitPlatforms.OrderBy(p => Math.Abs(p.Item1.Y)).FirstOrDefault(); // Will unfortunately have to do some better logic later.
			//.Where(v => !_primaryOverlapObjects.ContainsKey(Orientation.Horizontal) || v.Item2.Id != _primaryOverlapObjects[Orientation.Horizontal].Id);
			if (verticallyHitPlatformTuple != null)
			{
				MainGuiObject verticallyHitPlatform = verticallyHitPlatformTuple.Item2;
				Vector2 bounds = verticallyHitPlatformTuple.Item1; // GetIntersectionDepth(verticallyHitPlatform);

				//float hitObjectMoveAmount = !verticallyHitPlatform.IsMovable ? 0 : Math.Abs(verticallyHitPlatform.PreviousPosition.Y - verticallyHitPlatform.Position.Y);

				// If the object is moving downwards, and is below the top of the platform, push it back up.
				float boundsDiff = bounds.Y - verticallyHitPlatform.CurrentMovement.Y;
				float boundsDiffAbs = Math.Abs(boundsDiff);
				if ((!StopGravity || true)
					&& boundsDiffAbs > 0
					&& boundsDiffAbs - .0005f <= Math.Abs(curMovY)
					&& Math.Sign(boundsDiff) != Math.Sign(curMovY)) // (hitObjectMoveAmount + Math.Abs(_previousPosition.Y - Position.Y)))
				{
					// fix offset
					Position = new Vector2(Position.X + (verticallyHitPlatform.Position.X - verticallyHitPlatform.PreviousPosition.X), (float)Math.Round(Position.Y + bounds.Y, 4));
					_primaryOverlapObjects[Orientation.Vertical].Add(verticallyHitPlatform);
					CurrentMovement = new Vector2(CurrentMovement.X, CurrentMovement.Y / 10000000);
				}
			}
		}
		public void ApplyHorizontalCollision(IEnumerable<MainGuiObject> guiObjects)
		{
			//// If we already have a key, then we don't need to look for another.
			//if (_primaryOverlapObjects[Orientation.Horizontal].Any())
			//	return;
			Vector4 allBounds = this.HitBoxBounds;
			float curMovX = Position.X - _previousPosition.X;
			if (curMovX > 0)
			{
				allBounds.X -= curMovX;
				allBounds.W += curMovX;
			}
			else if (curMovX < 0)
				allBounds.W -= curMovX;
			IEnumerable<Tuple<Vector2, MainGuiObject>> horizontallyHitPlatforms = GetHitObjects(GetAllHorizontalPassableGroups(guiObjects), allBounds, false).Where(tup => tup.Item2.Id != this.Id && !tup.Item2.IsMovable);
			Tuple<Vector2, MainGuiObject> horizontallyHitPlatformTuple = horizontallyHitPlatforms.OrderBy(p => Math.Abs(p.Item1.X)).FirstOrDefault(); // Will unfortunately have to do some better logic later.
			if (horizontallyHitPlatformTuple != null)
			{
				MainGuiObject horizontallyHitPlatform = horizontallyHitPlatformTuple.Item2;
				Vector2 bounds = horizontallyHitPlatformTuple.Item1;


				float boundsDiff = bounds.X - horizontallyHitPlatform.CurrentMovement.X;
				float boundsDiffAbs = Math.Abs(boundsDiff);

				if (boundsDiffAbs > 0
					&& boundsDiffAbs - .0005f <= Math.Abs(curMovX)
					&& Math.Sign(boundsDiff) != Math.Sign(curMovX))
				{
					// fix offset
					Position = new Vector2((float)Math.Round(Position.X + bounds.X, 4), Position.Y);
					_primaryOverlapObjects[Orientation.Horizontal].Add(horizontallyHitPlatform);
					CurrentMovement = new Vector2(CurrentMovement.X / 10000000, CurrentMovement.Y);
				}
			}
		}

		public void ApplyCharacterCollision(IEnumerable<MainGuiObject> guiObjects)
		{
			//// If we already have a key, then we don't need to look for another.
			if (_primaryOverlapObjects[Orientation.Horizontal].Any(mgo => !mgo.IsMovable))
				return;
			if (CanPushObjects())
			{
				IEnumerable<MainGuiObject> characterObjects = Level.GetAllUnPassableMovableObjects(Bounds).Concat(Level.GetPossiblyHitEnvironmentObjects(Bounds).Where(mgo => mgo.IsMovable));
				IEnumerable<Tuple<Vector2, MainGuiObject>> horizontallyHitCharacters = GetHitObjects(GetAllHitCharacterGroups(characterObjects), this.HitBoxBounds, false).Where(tup => tup.Item2.Id != Id && (tup.Item2.Team == Team.None || tup.Item2.Team != this.Team) && !tup.Item2.GetType().IsSubclassOf(typeof(AffectedSpace)));
				foreach (Tuple<Vector2, MainGuiObject> horizontallyHitCharacterTuple in horizontallyHitCharacters)
				{
					// Up to the bigger object to decide the new location of each object.
					if (HealthTotal > horizontallyHitCharacterTuple.Item2.HealthTotal
						|| (horizontallyHitCharacterTuple.Item2.HealthTotal == HealthTotal && horizontallyHitCharacterTuple.Item2.Id.ToString().CompareTo(Id.ToString()) > 0))
					{
						PhysicsObject mgo = horizontallyHitCharacterTuple.Item2 as PhysicsObject;
						if (mgo != null && mgo._objState != GuiObjectState.Teleport)
						{
							// TODO when stuff goes bad, then don't let it go bad!
							float adjustedMovement = (mgo.CurrentMovement.X * .25f) + (CurrentMovement.X * .75f);
							if (mgo.HealthTotal == HealthTotal)
								adjustedMovement = (mgo.CurrentMovement.X * .5f) + (CurrentMovement.X * .5f);
							//if(!mgo.IsMovable && !IsMovable) // Get them snugly next to each other.
							//	adjustedMovement
							//else 
							if (!mgo.IsMovable)
								adjustedMovement = mgo.CurrentMovement.X;
							//else if (!IsMovabl))
							//	adjustedMovement = Position.X - PreviousPosition.X;

							Vector2 thisStartingPos = Position;
							thisStartingPos.X = Position.X - CurrentMovement.X;
							Vector2 startingPos = mgo.Position;
							startingPos.X -= mgo.CurrentMovement.X; // Go back to where you started!
							thisStartingPos.X += adjustedMovement;
							// If the two weren't hitting before, then they just hit now
							bool justHitting = MainGuiObject.GetIntersectionDepth(mgo.Bounds - new Vector4(mgo.CurrentMovement.X, 0, 0, 0), Bounds - new Vector4(CurrentMovement.X, 0, 0, 0)) == Vector2.Zero;
							var currentOverlapBounds = MainGuiObject.GetIntersectionDepth(mgo.Bounds, Bounds);
							float adjustedMovementForMgo = adjustedMovement;
							if (!justHitting) // find closest "exit" and get out of there.
								adjustedMovementForMgo = ((startingPos.X + (mgo.Size.X / 2) > thisStartingPos.X + (_size.X / 2)) ? _size.X / 8 : -_size.X / 8) + adjustedMovement;
							else if (adjustedMovement != 0) // I'm assuming that since they overlap, they have a smaller distance from endpoints than they should have.
								/*fix this*/
								//(adjustedMovement < 0 ? -1 : 1) * Math.Abs((_size.X + mgo.Size.X) / 2);
								adjustedMovementForMgo = (adjustedMovement < 0 ? -1 : 1) * (Math.Min(Math.Abs(adjustedMovement), Math.Abs(currentOverlapBounds.X)));//(adjustedMovement < 0 ? -1 : 1) * (Math.Abs((_size.X + mgo.Size.X) / 2) - Math.Abs(Center.X - mgo.Center.X) + .05f);
							//CurrentMovement = new Vector2(adjustedMovement, CurrentMovement.Y);
							Position = thisStartingPos;

							// If this isn't moving, but we are colliding, then light object is pushing
							bool movingWithLighterObject = CurrentMovement.X == 0
								|| ((mgo.CurrentMovement.X < 0 && adjustedMovementForMgo < 0) || (mgo.CurrentMovement.X > 0 && adjustedMovementForMgo > 0));


							if (movingWithLighterObject)
							{
								ApplyHorizontalCollision(guiObjects);
								if (PrimaryOverlapObjects[Orientation.Horizontal].Any())
									IsMovable = false; //adjustedMovementForMgo = mgo.CurrentMovement.X + currentOverlapBounds.X;
							}

							startingPos.X += adjustedMovementForMgo;
							// Don't mess with this guy! (if it is not movable)
							// OR if the object is standing on this.
							if (mgo.IsMovable && !(mgo.PrimaryOverlapObjects[Orientation.Vertical].Contains(this) && mgo.IsLanded))
							{
								mgo.Position = startingPos;
								IEnumerable<MainGuiObject> guiObjectsForMgo = Level.GetPossiblyHitEnvironmentObjects(mgo.Bounds).Where(mgo2 => mgo2.IsHitBy(this));
								//mgo.CurrentMovement = new Vector2(adjustedMovementForMgo, mgo.CurrentMovement.Y);
								mgo.ApplyHorizontalCollision(guiObjectsForMgo);
								mgo.ApplyCharacterCollision(guiObjectsForMgo);
							}
							IsMovable = true;

							List<MainGuiObject> horizontallyHitObject = null;
							mgo.PrimaryOverlapObjects.TryGetValue(Orientation.Horizontal, out horizontallyHitObject);
							if (horizontallyHitObject.Any(obj => obj != this))
							{
								PrimaryOverlapObjects[Orientation.Horizontal].Add(mgo);
								Position = new Vector2(Position.X - (startingPos.X - mgo.Position.X), Position.Y);
							}
							//CurrentMovement = new Vector2(CurrentMovement.X - (Position.X - thisStartingPos.X), CurrentMovement.Y);
							mgo.HitByObject(this, null);
							HitByObject(mgo, null);
						}
					}
				}
			}
		}

		protected override IEnumerable<MainGuiObject> GetAllVerticalPassableGroups(IEnumerable<MainGuiObject> guiObjects)
		{
			if (_objState == GuiObjectState.Normal)
				return guiObjects.Where(mgo => mgo.ObjectType != GuiObjectType.Structure || mgo.Team != this.Team);
			//else if in teleport state.
			return guiObjects.Where(mgo => (mgo.Group == SimonsGame.Utility.Group.Impassable || mgo.Group == Group.ImpassableIncludingMagic) && (mgo.ObjectType != GuiObjectType.Structure || mgo.Team != this.Team));
		}

		protected virtual IEnumerable<MainGuiObject> GetAllHitCharacterGroups(IEnumerable<MainGuiObject> guiObjects)
		{
			return guiObjects.ToList().Where(mgo => mgo.ObjectType != GuiObjectType.Structure || mgo.Team != this.Team);
		}

		protected virtual IEnumerable<MainGuiObject> GetAllHorizontalPassableGroups(IEnumerable<MainGuiObject> guiObjects)
		{
			return guiObjects.Where(mgo => !GetIgnoredHorizontalGroups(_IgnoredHorizontalGroups).Contains(mgo.Group) && (mgo.ObjectType != GuiObjectType.Structure || mgo.Team != this.Team));
		}

		// Add custom modifiers from the ability manager.
		public override List<ModifierBase> AddCustomModifiers(GameTime gameTime, ModifierBase modifyAdd)
		{
			List<ModifierBase> attacks = new List<ModifierBase>();
			StopGravity = false;
			List<Guid> expiredModifiers = new List<Guid>();
			foreach (KeyValuePair<Guid, ModifierBase> pair in _abilityManager.CurrentAbilities.Where(m => m.Value.Type == ModifyType.Add).ToList())
			{
				ModifierBase mod = pair.Value;
				if (mod.IsExpired(gameTime))
					_abilityManager.HasExpired(pair.Key);
				//expiredModifiers.Add(pair.Key);
				modifyAdd += mod;
				if (mod.HealthTotal < 0)
				{
					attacks.Add(mod);
					modifyAdd.SetHealthTotal(modifyAdd.HealthTotal - mod.HealthTotal); // undo the damage, that is calculated elsewhere!
				}
			}
			//foreach (Guid pair in expiredModifiers)
			//	_abilityManager.HasExpired(pair);
			StopGravity = StopGravity || modifyAdd.StopGravity;
			return attacks;
		}

		public override void MultiplyCustomModifiers(GameTime gameTime, ModifierBase modifyMult)
		{
			List<Guid> expiredModifiers = new List<Guid>();
			foreach (KeyValuePair<Guid, ModifierBase> pair in _abilityManager.CurrentAbilities.Where(m => m.Value.Type == ModifyType.Multiply))
			{
				ModifierBase mod = pair.Value;
				if (mod.IsExpired(gameTime))
				{
					expiredModifiers.Add(pair.Key);
				}
				modifyMult *= mod;
			}
			foreach (Guid pair in expiredModifiers)
			{
				_abilityManager.HasExpired(pair);
			}
			StopGravity = StopGravity || modifyMult.StopGravity;
		}
		public void ForceAbility(GameTime gameTime) { }
		protected override bool ShowHitBox()
		{
			return false;
		}

		public virtual Vector2 GetAim()
		{
			return new Vector2(CurrentMovement.X < 0 ? -1 : 1, 0);
		}

		public void GainExperience(float amount)
		{
			_abilityManager.Experience += amount;
		}

	}
}
