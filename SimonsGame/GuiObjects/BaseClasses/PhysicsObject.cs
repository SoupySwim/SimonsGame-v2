using Microsoft.Xna.Framework;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public abstract class PhysicsObject : MainGuiObject
	{
		protected AbilityManager _abilityManager;
		public AbilityManager AbilityManager { get { return _abilityManager; } }
		public bool IsLanded { get; set; }
		protected bool StopGravity { get; set; }
		protected bool VerticalPass { get; set; }

		//  _____
		// |     |
		// |     |
		// |__*__|   <--- Base Position
		public Vector2 BasePosition { get { return new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y); } }

		public PhysicsObject(Vector2 position, Vector2 hitbox, Group group, Level level)
			: base(position, hitbox, group, level)
		{
			StopGravity = false;
			VerticalPass = false;
		}
		public abstract void PostPhysicsPreUpdate(GameTime gameTime);

		// This is where all the physics logic gets set.
		public override void PreUpdate(GameTime gameTime)
		{
			// projectedVerticalSpeed is assumed to be gravity unless otherwise determined.
			float projectedVerticalSpeed = AverageSpeed.Y;

			// Assume not moving in a direction horizontally.
			float projectedHorizontalSpeed = 0;

			Vector4 nextBounds = this.Bounds;
			nextBounds.Y += AverageSpeed.Y;
			Dictionary<Group, List<MainGuiObject>> guiObjects = Level.GetAllUnPassableGuiObjects();

			// WARNING DUPLICATE CODE COMING UP, WILL FIX IN NEXT SPRINT.


			// Vertical.
			IEnumerable<Tuple<Vector2, MainGuiObject>> verticallyHitPlatforms = //MainGuiObject.GetHitPlats(guiObjects, nextBounds, (p) => VerticalPass && p.Group == Group.VerticalPassable, true);
			guiObjects.SelectMany(kv => kv.Value).Select(p =>
			{
				if (VerticalPass && p.Group == Group.VerticalPassable)
					return new { bounds = Vector2.Zero, select = false, obj = MainGuiObject.EmptyVessel };
				var bounds = MainGuiObject.GetIntersectionDepth(nextBounds, p.Bounds);
				float platHeight = p.Size.Y;
				//return bounds != Vector2.Zero && bounds.Y <= 0 && bounds.Y >= -platHeight;

				return new { bounds = bounds, select = bounds != Vector2.Zero && bounds.Y <= 0 && bounds.Y >= -platHeight, obj = p };
			}).Where(o => o.select).Select(o => new Tuple<Vector2, MainGuiObject>(o.bounds, o.obj));

			// Pick shortest depth as that's the one we care about presumably.
			Tuple<Vector2, MainGuiObject> verticallyHitPlatformTuple = verticallyHitPlatforms.OrderBy(p => Math.Abs(p.Item1.Y)).FirstOrDefault(); // Will unfortunately have to do some better logic later.

			if (verticallyHitPlatformTuple != null)
			{
				MainGuiObject verticallyHitPlatform = verticallyHitPlatformTuple.Item2;
				float platHeight = verticallyHitPlatform.Size.Y;
				float bumpLeeway = Position.Y + Size.Y - platHeight;
				Vector2 bounds = GetIntersectionDepth(verticallyHitPlatform);
				// If the object is moving downwards, and is below the top of the platform, push it back up.
				if (!StopGravity && bounds.Y <= 0 && bounds.Y >= -AverageSpeed.Y * 2)
				{
					// fix offset
					Position = new Vector2(Position.X, Position.Y + bounds.Y + 1);
					projectedVerticalSpeed = 0;
					IsLanded = true;
				}
			}


			// Horizontal

			// Make this check more groups... in an elegant way.
			IEnumerable<Tuple<Vector2, MainGuiObject>> horizontallyHitPlatforms = MainGuiObject.GetHitPlatforms(guiObjects.Where(g => g.Key == Group.ImpassableIncludingMagic).ToDictionary(o => o.Key, o => o.Value), nextBounds, (p) => false, false);

			//IEnumerable<Tuple<Vector2, MainGuiObject>> horizontallyHitPlatforms = guiObjects.Where(g => g.Key == Group.ImpassableIncludingMagic).SelectMany(kv => kv.Value).Select(p =>
			//{
			//	var bounds = MainGuiObject.GetIntersectionDepth(nextBounds, p.Bounds);
			//	float platWidth = p.Size.X;
			//	return new { bounds = bounds, select = bounds != Vector2.Zero && Math.Abs(bounds.X) > 0 && Math.Abs(bounds.X) <= platWidth, obj = p };
			//})
			//.Where(o => o.select).Select(o => new Tuple<Vector2, MainGuiObject>(o.bounds, o.obj));


			//Pick shortest depth as that's the one we care about presumably.
			Tuple<Vector2, MainGuiObject> horizontallyHitPlatformTuple = horizontallyHitPlatforms.OrderBy(p => Math.Abs(p.Item1.X)).FirstOrDefault(); // Will unfortunately have to do some better logic later.
			if (horizontallyHitPlatformTuple != null)
			{
				MainGuiObject horizontallyHitPlatform = horizontallyHitPlatformTuple.Item2;
				float platWidth = horizontallyHitPlatform.Size.X;
				float bumpLeeway = Position.X + Size.X - platWidth;
				Vector2 bounds = GetIntersectionDepth(horizontallyHitPlatform);
				// If the object is moving downwards, and is below the top of the platform, push it back up.
				if (Math.Abs(bounds.X) > 0 && Math.Abs(bounds.X) <= MaxSpeed.X + .001f /* some arbitrary number to alleviate rounding */)
				{
					// fix offset
					Position = new Vector2(Position.X + bounds.X, Position.Y);
					projectedHorizontalSpeed = 0;
				}
			}

			// Move object if needed.
			CurrentMovement = new Vector2(projectedHorizontalSpeed, projectedVerticalSpeed);

			PostPhysicsPreUpdate(gameTime);
			_abilityManager.CheckKnownAbilities(gameTime);
		}
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
			{
				_abilityManager.HasExpired(pair);
			}
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
	}
}
