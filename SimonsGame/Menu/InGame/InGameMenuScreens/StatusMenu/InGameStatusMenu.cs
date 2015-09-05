using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Menu.InGame
{
	public class InGameStatusMenu : InGameMenuPartialView
	{
		public SelectedPane SelectedPane;
		public AllMagicPane AllMagicPane;
		public Player Player { get; set; }
		public InGameStatusMenu(Vector4 bounds, Player player, Vector4 hoverInfoPaneBounds)
			: base(bounds)
		{
			Player = player;
			// top 30% goes to current stuff,
			// bottom goes to all stuff.
			// Leftmost 85% goes to health, mana, and current abilities.
			// Rightmost 15% goes to other buttons (level up).
			SelectedPane = new SelectedPane(new Vector4(bounds.X, bounds.Y, bounds.Z * .2f, bounds.W), this);
			AllMagicPane = new AllMagicPane(new Vector4(bounds.X, bounds.Y + bounds.Z * .2f + 5, bounds.Z * .8f - 8, bounds.W), this, hoverInfoPaneBounds);
		}
		public void OpenMenu()
		{
		}
		public override void Update(GameTime gameTime, Vector2 newMousePosition)
		{
			SelectedPane.Update(gameTime, newMousePosition);
			AllMagicPane.Update(gameTime, newMousePosition);
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			AllMagicPane.Draw(gameTime, spriteBatch);
			SelectedPane.Draw(gameTime, spriteBatch);
		}
		public override bool MoveLeft() { return true; }
		public override bool MoveRight() { return true; }
		public override bool MoveUp() { return true; }
		public override bool MoveDown() { return true; }
		public override void HasBeenHighlighted() { }
	}
}
