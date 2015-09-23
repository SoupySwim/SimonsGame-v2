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
using SimonsGame.GuiObjects.Utility;

namespace SimonsGame.GuiObjects
{
	public class SmallKeyObject : PhysicsObject
	{
		private byte _keyType;
		private Texture2D _background;

		public SmallKeyObject(Vector2 position, Vector2 hitbox, Level level)
			: base(position, hitbox, Group.Passable, level, "SmallKeyObject")
		{
			_background = MainGame.ContentManager.Load<Texture2D>("Test/SmallKey");
			ChangeKeyType(0);
			AccelerationBase = new Vector2(0, .025f);
			MaxSpeedBase = new Vector2(0, AverageSpeed.Y);
		}

		// As this is a carrier, it must have an initializer like this.
		public SmallKeyObject(MainGuiObject mgo, SmallKey key)
			: this(mgo.Center - (key.DefaultSize / 2), key.DefaultSize, mgo.Level)
		{
			ChangeKeyType(key.KeyType);
		}

		public override void Initialize()
		{
			var carrier = Level.GetAllMovableCharacters(Bounds).Where(mgo => MainGuiObject.GetIntersectionDepth(Bounds, mgo.Bounds) != Vector2.Zero).FirstOrDefault();
			if (carrier != null)
			{
				carrier.ObtainItem(new SmallKey(_keyType));
				Level.RemoveGuiObject(this);
			}
		}

		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return MaxSpeed.Y; }

		protected override bool ShowHitBox() { return false; }
		public override void PreUpdate(GameTime gameTime) { }
		public override void PostUpdate(GameTime gameTime)
		{
			foreach (Player player in Level.Players.Values)
			{
				if (GetIntersectionDepth(player) != Vector2.Zero)
					HitByObject(player, null);
			}
			base.PostUpdate(gameTime);
		}
		public override void PreDraw(GameTime gameTime, SpriteBatch spriteBatch) { }
		public override void PostDraw(GameTime gameTime, SpriteBatch spriteBatch, Player curPlayer)
		{
			spriteBatch.Draw(_background, Bounds.ToRectangle(), _hitBoxColor);
		}
		
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb)
		{
			if (mgo is Player)
			{
				mgo.ObtainItem(new SmallKey(_keyType));
				Level.RemoveGuiObject(this);
			}
		}
		protected override void AdditionalGroupChange(Group _group, Group newGroup) { newGroup = Group.Passable; } // NEVER CHANGE!!!!!!

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
