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
	public enum BehaviorModifier // We'll see if we need this.
	{
		Jump = 0,
		DropDown = 1
	}
	public class BehaviorZone : GenericZone
	{
		private byte _behaviorChannel;
		private BehaviorModifier _behaviorModifier;
		public BehaviorModifier BehaviorModifier { get { return _behaviorModifier; } }

		public BehaviorZone(Vector2 position, Vector2 hitbox, Level level, Team team)
			: base(position, hitbox, level, "BehaviorZone")
		{
			_behaviorModifier = BehaviorModifier.Jump;
			_behaviorChannel = 0;
			Team = team;
			HitBoxColor = Color.Lerp(Color.LightGray, TeamColorMap[team], .35f);
		}
		public override void Initialize(Level level)
		{
			foreach (var character in level.GetAllGuiObjects())
			{
				ObjectSpawner os = character as ObjectSpawner; // maybe change.
				if (os != null && _behaviorChannel == os.BehaviorChannel)
					os.ZoneIds.Add(_guid);
			}
		}

		public override void CharacterDied(MainGuiObject mgo) { }

		public override void PreUpdate(GameTime gameTime)
		{
			foreach (var character in Level.GetAllMovableCharacters(Bounds).Where(c => c.Team == Team && c.ZoneIds.Contains(Id) && MainGuiObject.GetIntersectionDepth(c.Bounds, Bounds) != Vector2.Zero))
				character.TriggerBehavior(this);
		}

		#region Extra Crap
		public override float GetXMovement() { return 0; }
		public override float GetYMovement() { return 0; }
		public override void PostUpdate(GameTime gameTime) { }
		public override void PostDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Player curPlayer) { }

		public override void PreDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
		public override void SetMovement(GameTime gameTime) { }
		public override void HitByObject(MainGuiObject mgo, ModifierBase mb) { }
		protected override bool ShowHitBox() { return MainGame.GameState == MainGame.MainGameState.Menu; }
		#endregion

		#region Map Editor

		public override string GetSpecialTitle(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return "Behavior Type";
			if (bType == ButtonType.SpecialToggle2)
				return "Behavior Channel";
			//if (bType == ButtonType.SpecialToggle2)
			//	return "";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return _behaviorModifier.ToString();
			if (bType == ButtonType.SpecialToggle2)
				return _behaviorChannel.ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				_behaviorModifier = moveRight ? MiscExtensions.GetNextEnum<BehaviorModifier>(_behaviorModifier) : MiscExtensions.GetPreviousEnum<BehaviorModifier>(_behaviorModifier);
			}
			if (bType == ButtonType.SpecialToggle2)
				_behaviorChannel = (byte)((_behaviorChannel + (moveRight ? 1 : 9)) % 10);
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return (int)_behaviorModifier;
			if (bType == ButtonType.SpecialToggle2)
				return _behaviorChannel;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				_behaviorModifier = (BehaviorModifier)value;
			if (bType == ButtonType.SpecialToggle2)
				_behaviorChannel = (byte)value;
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}
}