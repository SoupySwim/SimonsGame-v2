using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
		public bool IsLanded { get { return PrimaryOverlapObjects.ContainsKey(Orientation.Vertical); } }
		private Dictionary<Orientation, MainGuiObject> _primaryOverlapObjects;
		public Dictionary<Orientation, MainGuiObject> PrimaryOverlapObjects { get { return _primaryOverlapObjects; } }
		protected bool StopGravity { get; set; }
		protected bool VerticalPass { get; set; }

		//  _____
		// |     |
		// |     |
		// |__*__|   <--- Base Position
		public Vector2 BasePosition { get { return new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y); } set { Position = new Vector2(value.X - Size.X / 2, value.Y - Size.Y); } }

		public PhysicsObject(Vector2 position, Vector2 hitbox, Group group, Level level, string name)
			: base(position, hitbox, group, level, name)
		{
			StopGravity = false;
			VerticalPass = false;
			_abilityManager = new AbilityManager(this, new Dictionary<KnownAbility, List<PlayerAbilityInfo>>());
			_objectType = GuiObjectType.Character;
			_primaryOverlapObjects = new Dictionary<Orientation, MainGuiObject>();
		}

		// This is where all the physics logic gets set.
		public override void PreUpdate(GameTime gameTime)
		{
			_abilityManager.CheckKnownAbilities(gameTime);
		}
		public override void PostUpdate(GameTime gameTime)
		{
			/////////////////////////////////////////////
			// how to stop me from hitting the ceiling //
			/////////////////////////////////////////////
			_primaryOverlapObjects = new Dictionary<Orientation, MainGuiObject>();
			Dictionary<Group, List<MainGuiObject>> guiObjects = Level.GetAllUnPassableEnvironmentObjects();



			IEnumerable<Tuple<DoubleVector2, MainGuiObject>> verticallyHitPlatforms = GetHitObjects(GetAllVerticalPassableGroups(guiObjects), this.HitBoxBounds, (p) => VerticalPass && _IgnoredVerticalDownGroups.Contains(p.Group), true).Where(tup => tup.Item2.Id != this.Id);
			Tuple<DoubleVector2, MainGuiObject> verticallyHitPlatformTuple = verticallyHitPlatforms.OrderBy(p => Math.Abs(p.Item1.Y)).FirstOrDefault(); // Will unfortunately have to do some better logic later.
			//.Where(v => !_primaryOverlapObjects.ContainsKey(Orientation.Horizontal) || v.Item2.Id != _primaryOverlapObjects[Orientation.Horizontal].Id);
			if (verticallyHitPlatformTuple != null)
			{
				MainGuiObject verticallyHitPlatform = verticallyHitPlatformTuple.Item2;
				DoubleVector2 bounds = verticallyHitPlatformTuple.Item1; // GetIntersectionDepth(verticallyHitPlatform);

				// If the object is moving downwards, and is below the top of the platform, push it back up.
				if (!StopGravity && Math.Abs(bounds.Y - verticallyHitPlatform.CurrentMovement.Y) > 0 && Math.Abs(bounds.Y - verticallyHitPlatform.CurrentMovement.Y) - .00005 <= Math.Abs(_previousPosition.Y - Position.Y))
				{
					// fix offset
					Position = new Vector2(Position.X, Position.Y + (float)bounds.Y);
					_primaryOverlapObjects.Add(Orientation.Vertical, verticallyHitPlatform);
				}
			}

			IEnumerable<Tuple<DoubleVector2, MainGuiObject>> horizontallyHitPlatforms = GetHitObjects(GetAllHorizontalPassableGroups(guiObjects), this.HitBoxBounds, (p) => false, false).Where(tup => tup.Item2.Id != this.Id);
			Tuple<DoubleVector2, MainGuiObject> horizontallyHitPlatformTuple = horizontallyHitPlatforms.OrderBy(p => Math.Abs(p.Item1.X)).FirstOrDefault(); // Will unfortunately have to do some better logic later.
			if (horizontallyHitPlatformTuple != null)
			{
				MainGuiObject horizontallyHitPlatform = horizontallyHitPlatformTuple.Item2;
				DoubleVector2 bounds = horizontallyHitPlatformTuple.Item1;
				if (Math.Abs(bounds.X - horizontallyHitPlatform.CurrentMovement.X) > 0 && Math.Abs(bounds.X - horizontallyHitPlatform.CurrentMovement.X) <= Math.Abs(_previousPosition.X - Position.X))
				{
					// fix offset
					Position = new Vector2(Position.X + (float)bounds.X, Position.Y);
					_primaryOverlapObjects.Add(Orientation.Horizontal, horizontallyHitPlatform);
				}
			}
			if (Position.X + Size.X < 0 || Position.Y + Size.Y < 0 || Position.X > Level.Size.X || Position.Y > Level.Size.Y)
				Died();
		}

		protected override Dictionary<Group, List<MainGuiObject>> GetAllVerticalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		{
			var allObjects = guiObjects;
			foreach (var kv in allObjects.ToList())
				allObjects[kv.Key] = kv.Value.Where(mgo => mgo.ObjectType != GuiObjectType.Structure || mgo.Team != this.Team).ToList();
			return allObjects;
		}

		protected virtual Dictionary<Group, List<MainGuiObject>> GetAllHorizontalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		{
			var allObjects = guiObjects.Where(g => !GetIgnoredHorizontalGroups(_IgnoredHorizontalGroups).Contains(g.Key)).ToDictionary(o => o.Key, o => o.Value);
			foreach (var kv in allObjects.ToList())
				allObjects[kv.Key] = kv.Value.Where(mgo => mgo.ObjectType != GuiObjectType.Structure || mgo.Team != this.Team).ToList();
			return allObjects;
		}

		// Add custom modifiers from the ability manager.
		public override void AddCustomModifiers(GameTime gameTime, ModifierBase modifyAdd)
		{
			StopGravity = false;
			List<Guid> expiredModifiers = new List<Guid>();
			foreach (KeyValuePair<Guid, ModifierBase> pair in _abilityManager.CurrentAbilities.Where(m => m.Value.Type == ModifyType.Add))
			{
				ModifierBase mod = pair.Value;
				if (mod.IsExpired(gameTime))
				{
					expiredModifiers.Add(pair.Key);
				}
				modifyAdd += mod;
			}
			foreach (Guid pair in expiredModifiers)
				_abilityManager.HasExpired(pair);
			StopGravity = StopGravity || modifyAdd.StopGravity;
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
	}
}
