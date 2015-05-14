using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects.Utility;
using SimonsGame.MapEditor;
using SimonsGame.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.GuiObjects.Zones
{
	class JungleCreepZone : GenericZone
	{
		private Dictionary<Guid, Vector2> _creepsStartingPosition;
		private Dictionary<Guid, bool> _creepsLivingStatus;
		private Dictionary<Guid, GuiObjectStore> _creepObjectStore;

		private int _tickTotal = 900;
		private int _tickCurrent;

		public JungleCreepZone(Vector2 position, Vector2 hitbox, Level level)
			: base(position, hitbox, level, "JungleCreepZone")
		{
			_creepsStartingPosition = new Dictionary<Guid, Vector2>();
			_creepObjectStore = new Dictionary<Guid, GuiObjectStore>();
			_creepsLivingStatus = new Dictionary<Guid, bool>();
			HitBoxColor = Color.Purple;
		}
		public override void Initialize(Level level)
		{
			var guiObjects = level.GetAllCharacterObjects().Where(mgo => MainGuiObject.GetIntersectionDepth(mgo.Bounds, Bounds) != Vector2.Zero);
			_creepsStartingPosition.Clear();
			_creepsLivingStatus.Clear();
			_creepObjectStore.Clear();
			foreach (var mgo in guiObjects)
			{
				_creepsStartingPosition.Add(mgo.Id, mgo.Position);
				_creepsLivingStatus.Add(mgo.Id, true);
				mgo.ZoneIds.Add(_guid);
			}
		}

		public override void CharacterDied(MainGuiObject mgo)
		{
			if (_creepsLivingStatus.ContainsKey(mgo.Id))
			{
				_creepsLivingStatus[mgo.Id] = false;
				GuiObjectStore gos = mgo.GetGuiObjectStore();
				Vector2 originalPosition = _creepsStartingPosition[mgo.Id];
				gos.X = originalPosition.X;
				gos.Y = originalPosition.Y;
				_creepObjectStore.Add(mgo.Id, gos);
			}
		}

		public override void PostUpdate(GameTime gameTime)
		{
			// If at least one object is alive.
			if (_tickCurrent == 0)
			{
				if (!_creepsLivingStatus.Aggregate(false, (all, one) => all || one.Value)) // If all of them are dead, then start the respawn counter!
					_tickCurrent = _tickTotal;

			}
			else
			{
				_tickCurrent--;
				if (_tickCurrent == 0) // If we are respawning the creeps
				{
					_creepsStartingPosition.Clear();
					_creepsLivingStatus.Clear();
					foreach (var kv in _creepObjectStore)
					{
						MainGuiObject mgo = kv.Value.GetObject(Level);
						Level.AddGuiObject(mgo);
						mgo.ZoneIds.Add(_guid);
						_creepsStartingPosition.Add(mgo.Id, mgo.Position);
						_creepsLivingStatus.Add(mgo.Id, true);
					}
					_creepObjectStore.Clear();
				}
			}
		}

		#region Extra Crap
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void AddCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyAdd) { }
		public override void MultiplyCustomModifiers(GameTime gameTime, Modifiers.ModifierBase modifyMult) { }
		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void PreUpdate(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		protected override bool ShowHitBox() { return MainGame.GameState == MainGame.MainGameState.Menu; }
		public override void ExtraSizeManipulation(Vector2 newSize) { }
		#endregion

		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Respawn Counter";
			//if (bType == ButtonType.SpecialToggle2)
			//	return "";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _tickTotal == -1 ? "Never" : string.Format("{0:0}", _tickTotal / 60);
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				if (_tickTotal < 0)
					_tickTotal = moveRight ? 900 : 18000;
				else if (!moveRight && _tickTotal == 900)
					_tickTotal = -1;
				else if (moveRight && _tickTotal == 18000)
					_tickTotal = -1;
				else
					_tickTotal = MathHelper.Clamp(_tickTotal + (moveRight ? 900 : -900), 900, 18000);
			}
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return _tickTotal;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_tickTotal = value;
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}
}