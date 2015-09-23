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
	public abstract class AffectedSpace : MainGuiObject
	{
		public ModifierBase CollisionModifier { get { return _collisionModifier; } }
		protected ModifierBase _collisionModifier;
		protected HashSet<GuiObjectType> _hitTypes = new HashSet<GuiObjectType>()
		{
			GuiObjectType.Character,
			GuiObjectType.Attack,
			GuiObjectType.Player
		};

		public AffectedSpace(Vector2 position, Vector2 hitbox, Level level, string name)
			: base(position, hitbox, Group.Passable, level, name)
		{
			_collisionModifier = new EmptyModifier(ModifyType.Add, this);
		}

		public override void PreUpdate(GameTime gameTime)
		{
		}

		public override void PostUpdate(GameTime gameTime)
		{
			foreach (var mgoTuple in GetAffectedObjects())
				HitObject(mgoTuple.Item2, mgoTuple.Item1);
		}

		protected virtual void HitObject(MainGuiObject mgo, Vector2 bounds)
		{
			mgo.HitByObject(this, _collisionModifier);
		}

		public virtual IEnumerable<Tuple<Vector2, MainGuiObject>> GetAffectedObjects()
		{
			return GetHitObjects(Level.GetAllMovableCharacters(Bounds), Bounds).Where(kv => kv.Item2.Id != Id);
		}

		//protected override Dictionary<Group, List<MainGuiObject>> GetAllVerticalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		//{
		//	var allObjects = Level.GetAllGuiObjects();
		//	foreach (var kv in allObjects.ToList())
		//		allObjects[kv.Key] = kv.Value.Where(mgo => _hitTypes.Contains(mgo.ObjectType)).ToList();
		//	return allObjects;
		//}

		//protected override Dictionary<Group, List<MainGuiObject>> GetAllHorizontalPassableGroups(Dictionary<Group, List<MainGuiObject>> guiObjects)
		//{
		//	var allObjects = Level.GetAllGuiObjects();
		//	foreach (var kv in allObjects.ToList())
		//		allObjects[kv.Key] = kv.Value.Where(mgo => _hitTypes.Contains(mgo.ObjectType)).ToList();
		//	return allObjects;
		//}
		//protected override List<Group> GetIgnoredVerticalGroups(List<Group> suggestedGroups)
		//{
		//	return new List<Group>();
		//}
		//protected override List<Group> GetIgnoredHorizontalGroups(List<Group> suggestedGroups)
		//{
		//	return new List<Group>();
		//}
	}
}
