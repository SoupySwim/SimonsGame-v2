using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Menu.InGame;

namespace SimonsGame.Menu
{
	public class InvisibleInGameOverlayMenuItem : MenuItemButton
	{
		private InGameMenuPartialView _controlledView;
		public InvisibleInGameOverlayMenuItem(InGameMenuPartialView controlledView, bool isSelected = false)
			: base(() => { }, Vector4.Zero)
		{
			IsHighLighted = isSelected;
			_controlledView = controlledView;
			Bounds = controlledView.Bounds;
		}
		public override void HasBeenHighlighted()
		{
			IsHighLighted = true;
			_controlledView.HasBeenHighlighted();
		}
		public override void HasBeenDeHighlighted()
		{
			IsHighLighted = false;
		}
		public override void Update(GameTime gameTime)
		{
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
		}
		public override bool IsStuck(Direction2D direction)
		{
			if (direction == Direction2D.Down)
				return _controlledView.MoveDown();
			if (direction == Direction2D.Up)
				return _controlledView.MoveUp();
			if (direction == Direction2D.Left)
				return _controlledView.MoveLeft();
			if (direction == Direction2D.Right)
				return _controlledView.MoveRight();
			return false;
		}
	}
}
