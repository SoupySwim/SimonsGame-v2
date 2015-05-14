using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.Extensions;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Modifiers.Abilities;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public class Teleporter : AffectedSpace, ITeleportable
	{

		// Here's a list of objects that this object will ignore.
		// That includes collision and any other affect this could have on an object.
		// It is up to the inherited class to use this effectively.
		protected List<Guid> _ignoredIds = new List<Guid>();

		private byte _teleportPairId;
		private bool _canTeleportTo;

		public Teleporter(Vector2 position, Vector2 hitbox, Level level)
			: base(position, hitbox, level, "Teleporter")
		{
			_collisionModifier = new EmptyModifier(ModifyType.Add, null);
			HitBoxColor = Color.Orange;
			_objectType = GuiObjectType.Teleporter;
			_teleportPairId = 0;
			_canTeleportTo = true;
		}

		// For now, if you hit this teleporter, it will transfer you to it's bounding pair.
		protected override void HitObject(MainGuiObject mgo, Vector2 bounds)
		{
			// If we are ignoring this object, then ignore it!
			if (_ignoredIds.Contains(mgo.Id))
				return;
			// Get All Teleporters
			IEnumerable<ITeleportable> matchingTeleporters = mgo.Level.GetMatchingTeleporters(this);

			// Then arranges them so that the first one hit will be the next one in order.
			var beginningTeleporters = matchingTeleporters.TakeWhile(t => this != t);
			matchingTeleporters = matchingTeleporters.Skip(beginningTeleporters.Count() + 1).Concat(beginningTeleporters);
			matchingTeleporters = matchingTeleporters.Where(t => t.CanTeleportTo());
			ITeleportable teleporter = matchingTeleporters.FirstOrDefault();
			if (teleporter != null)
				teleporter.TeleportObject(mgo);
		}
		public GuiObjectStore GetGuiObjectStore()
		{
			Func<MainGuiObject, GuiObjectStore> func = MainGuiObjectExtensions.GetGuiObjectStore;
			return func(this);
		}
		public byte GetTeleportId()
		{
			return _teleportPairId;
		}
		public bool CanTeleportTo()
		{
			return _canTeleportTo;
		}
		public void TeleportObject(MainGuiObject mgo)
		{
			_ignoredIds.Add(mgo.Id);
			mgo.Center = this.Center;
		}
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		public override void PreUpdate(GameTime gameTime) { base.PreUpdate(gameTime); }
		public override void ExtraSizeManipulation(Vector2 newSize) { base.ExtraSizeManipulation(newSize); }
		protected override bool ShowHitBox() { return true; }
		public override IEnumerable<Tuple<Vector2, MainGuiObject>> GetAffectedObjects()
		{
			var hitObjects = GetHitObjects(Level.Players.Values,
				Bounds).Where(tup => tup.Item2.Id != Id);

			// Update Ignored IDs
			_ignoredIds = hitObjects.Select(tup => tup.Item2.Id).Intersect(_ignoredIds).ToList();

			return hitObjects;
		}

		#region Map Editor
		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Pair ID";
			else if (bType == ButtonType.SpecialToggle2)
				return "Can Teleport To";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return string.Format("{0:0}", _teleportPairId);
			else if (bType == ButtonType.SpecialToggle2)
				return _canTeleportTo ? "Yes" : "No";
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				_teleportPairId = (byte)((_teleportPairId + (moveRight ? 1 : 9)) % 10);
			else if (bType == ButtonType.SpecialToggle2)
				_canTeleportTo = !_canTeleportTo;
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return (int)_teleportPairId;
			else if (bType == ButtonType.SpecialToggle2)
				return _canTeleportTo ? 1 : 0;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_teleportPairId = (byte)value;
			else if (bType == ButtonType.SpecialToggle2)
				_canTeleportTo = value == 1;
			base.SetSpecialValue(bType, value);
		}
		#endregion
		//protected override SpriteEffects GetCurrentSpriteEffects() { }
		//public override void SwitchDirections() { }
	}
}