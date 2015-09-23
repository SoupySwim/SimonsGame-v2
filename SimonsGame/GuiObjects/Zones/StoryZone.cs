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
	public class StoryZone : GenericZone
	{
		public byte StoryPoint;

		public StoryZone(Vector2 position, Level level)
			: base(position, new Vector2(40), level, "StoryZone")
		{
			HitBoxColor = Color.Black;
		}

		public override void Initialize(Level level) { }

		public override void CharacterDied(MainGuiObject mgo) { }

		public override void PreUpdate(GameTime gameTime) { }

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
				return "Story Point";
			return base.GetSpecialTitle(bType);
		}

		public override string GetSpecialText(ButtonType bType)
		{
			if (bType == ButtonType.SpecialToggle1)
				return StoryPoint.ToString();
			return base.GetSpecialText(bType);
		}

		public override void ModifySpecialText(ButtonType bType, bool moveRight)
		{
			if (bType == ButtonType.SpecialToggle1)
			{
				if (StoryPoint == 0 && !moveRight)
					StoryPoint = 10;
				if (StoryPoint == 10 && moveRight)
					StoryPoint = 0;
				else
					StoryPoint = (byte)(StoryPoint + (moveRight ? 1 : -1));
			}
			base.ModifySpecialText(bType, moveRight);
		}
		public override int GetSpecialValue(ButtonType bType) // For Saving the object
		{
			if (bType == ButtonType.SpecialToggle1)
				return (int)StoryPoint;
			return base.GetSpecialValue(bType);
		}
		public override void SetSpecialValue(ButtonType bType, int value) // For Loading the object
		{
			if (bType == ButtonType.SpecialToggle1)
				StoryPoint = (byte)value;
			base.SetSpecialValue(bType, value);
		}

		#endregion
	}
}