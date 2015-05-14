using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.MainFiles;

namespace SimonsGame.GuiObjects
{
	public class LockedBarrier : MainGuiObject
	{
		private byte _keyType;
		//private Texture2D _background;

		public LockedBarrier(Vector2 position, Vector2 hitbox, Level level)
			: base(position, hitbox, Group.ImpassableIncludingMagic, level, "LockedBarrier")
		{
			//_background = MainGame.ContentManager.Load<Texture2D>("Test/SingleColor");
			ChangeKeyType(0);
		}
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void AddCustomModifiers(GameTime gameTime, ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, ModifierBase modifyMult) { }
		protected override bool ShowHitBox() { return true; }
		public override void PreUpdate(GameTime gameTime) { }
		public override void PostUpdate(GameTime gameTime) { }
		public override void PreDraw(GameTime gameTime, SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, SpriteBatch spriteBatch) { }
		public override void ExtraSizeManipulation(Vector2 newSize) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			IEnumerable<SmallKey> keys = mgo.GetObtainableItemsOfType<SmallKey>();
			SmallKey key = keys.FirstOrDefault(k => k.KeyType == _keyType);
			if (key != null)
			{
				mgo.UseKey(key);
				Level.RemoveGuiObject(this);
			}
			// Check if mgo has the key to opening this barrier!
		}
		protected override void AdditionalGroupChange(Group _group, Group newGroup) { newGroup = Group.ImpassableIncludingMagic; } // NEVER CHANGE!!!!!!

		private void ChangeKeyType(byte newType)
		{
			_keyType = newType;
			_hitBoxColor = SmallKey.KeyColors[_keyType];
		}

		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Channel";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _keyType.ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
				ChangeKeyType((byte)((_keyType + (moveRight ? 1 : 9)) % 10));
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _keyType;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				ChangeKeyType((byte)value);
			base.SetSpecialValue(bType, value);
		}

		#endregion

	}
}
