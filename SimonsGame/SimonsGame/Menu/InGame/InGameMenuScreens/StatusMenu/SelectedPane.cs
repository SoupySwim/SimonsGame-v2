using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Utility;
using SimonsGame.Modifiers;
namespace SimonsGame.Menu.InGame
{
	public class SelectedPane : InGameMenuPartialView
	{
		private InGameStatusMenu _parent;
		private Vector4 _healthBounds;
		private Vector4 _manaBounds;
		private ImageMenuItemButton _levelUpButton;
		private List<Vector4> _magicsBounds;
		private bool useLargeFont = false;
		private bool _isSelected = false;
		private bool _isUsingMouse = false;

		public SelectedPane(Vector4 bounds, InGameStatusMenu parent)
			: base(bounds)
		{
			_parent = parent;
			float padding = 10;
			float statusBarHeight = 30;
			int statusBarAmount = 2;
			float levelUpButtonWidth = 100;

			float leftSideStatusWidth = (Bounds.W - levelUpButtonWidth - (padding * (statusBarAmount + 1))) / statusBarAmount;
			_healthBounds = new Vector4(Bounds.X + padding, Bounds.Y + padding, statusBarHeight, leftSideStatusWidth);
			_manaBounds = new Vector4(_healthBounds.X + leftSideStatusWidth + padding, Bounds.Y + padding, statusBarHeight, leftSideStatusWidth);

			_levelUpButton = new ImageMenuItemButton(() =>
			{
				_parent.AllMagicPane.IsLevelUpMode = !_parent.AllMagicPane.IsLevelUpMode;
				if (_parent.AllMagicPane.IsLevelUpMode) { _levelUpButton.DefaultColor = Color.LightBlue; _levelUpButton.SelectedColor = Color.Lerp(Color.LightBlue, Color.Blue, .3f); }
				else { _levelUpButton.DefaultColor = Color.White; _levelUpButton.SelectedColor = Color.LightGray; }
			}, MainGame.ContentManager.Load<Texture2D>("Test/LevelUpButton"),
				new Vector4(Bounds.X + Bounds.W - levelUpButtonWidth, Bounds.Y + padding, Bounds.Z - padding, levelUpButtonWidth), Color.White, Color.LightGray);

			float leftSideButtonWidth = (Bounds.W - levelUpButtonWidth - (padding * 3)) / 2; // There's only 2 buttons per row for now...!

			float currentMagicHeight = (Bounds.Z - statusBarHeight - (padding * 3)) / 2;
			_magicsBounds = new List<Vector4>()
			{
				new Vector4(Bounds.X + padding, Bounds.Y + (padding * 2) + statusBarHeight , currentMagicHeight, leftSideButtonWidth),
				new Vector4(Bounds.X + padding + leftSideButtonWidth + padding, Bounds.Y + (padding * 2) + statusBarHeight, currentMagicHeight, leftSideButtonWidth),
				new Vector4(Bounds.X + padding, Bounds.Y + statusBarHeight + (padding * 3) + currentMagicHeight, currentMagicHeight, leftSideButtonWidth),
				new Vector4(Bounds.X + padding + leftSideButtonWidth + padding, Bounds.Y + statusBarHeight + (padding * 3) + currentMagicHeight, currentMagicHeight, leftSideButtonWidth)
			};
			useLargeFont = currentMagicHeight > "l".GetTextSize(MainGame.PlainFont).Y;
		}
		public override void Update(GameTime gameTime, Vector2 newMousePosition)
		{
			// If you aren't using the mouse, but the position of the mouse has changed, then use the mouse.
			if (!_isUsingMouse && Controls.PreviousMouse.Position != Controls.CurrentMouse.Position)
				_isUsingMouse = true;
			if (_isUsingMouse)
			{
				if (newMousePosition.IsInBounds(this.Bounds))
				{
					if (!_isSelected)
					{
						_isSelected = true;
						_levelUpButton.HasBeenHighlighted();
					}
					if (Controls.ButtonEnumerate().Aggregate(false, (press, btn) => press || Controls.PressedDown(_parent.Player.Id, btn))) // returns true if a button is pressed.
					{
						_levelUpButton.CallAction();
					}
				}
				else if (_isSelected)
				{
					_isSelected = false;
					_levelUpButton.HasBeenDeHighlighted();
				}
			}
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(MainGame.SingleColor, (_levelUpButton.Bounds + new Vector4(-2, -2, 4, 4)).ToRectangle(), Color.Black);
			_levelUpButton.Draw(gameTime, spriteBatch);
			//DrawHealth(gameTime, spriteBatch);
			GlobalGuiObjects.DrawRatioBar(gameTime, spriteBatch, _healthBounds, _parent.Player.HealthCurrent, _parent.Player.HealthTotal, Color.Green, 100);
			GlobalGuiObjects.DrawRatioBar(gameTime, spriteBatch, _manaBounds, _parent.Player.ManaCurrent, _parent.Player.ManaTotal, Color.Blue, 100);
			int ndx = 0;
			AbilityManager abilityManager = _parent.Player.AbilityManager;
			var sortedActiveAbilities = abilityManager.ActiveAbilities.OrderBy(id => (int)abilityManager.AbilityButtonMap[id]).ToList();

			_magicsBounds.ForEach(mb =>
			{
				spriteBatch.Draw(MainGame.SingleColor, (mb + new Vector4(-2, -2, 4, 4)).ToRectangle(), Color.Black);
				spriteBatch.Draw(MainGame.SingleColor, mb.ToRectangle(), Color.White);
				string magicName = "None";
				if (sortedActiveAbilities.Count() > ndx)
				{
					Guid abilityId = sortedActiveAbilities.ElementAt(ndx);
					var pai = abilityManager.GetAbilityInfo(abilityId);
					magicName = pai.Name;
				}
				Vector2 magicNamePosition = magicName.GetTextBoundsByCenter(useLargeFont ? MainGame.PlainFontLarge : MainGame.PlainFont, mb.GetPosition() + (mb.GetSize() / 2)).GetPosition();
				spriteBatch.DrawString(useLargeFont ? MainGame.PlainFontLarge : MainGame.PlainFont, magicName, magicNamePosition, Color.Black);
				ndx++;
			});
		}
		public override bool MoveDown()
		{
			_levelUpButton.HasBeenDeHighlighted();
			return false;
		}
		public override bool MoveUp()
		{
			_levelUpButton.HasBeenDeHighlighted();
			return false;
		}
		public override bool MoveLeft() { return true; }
		public override bool MoveRight()
		{
			_levelUpButton.HasBeenDeHighlighted();
			return false;
		}
		public override void HasBeenHighlighted()
		{
			_levelUpButton.HasBeenHighlighted();
			if (!_isSelected)
			{
				_isSelected = true;
				_levelUpButton.HasBeenHighlighted();
			}
		}
	}
}
