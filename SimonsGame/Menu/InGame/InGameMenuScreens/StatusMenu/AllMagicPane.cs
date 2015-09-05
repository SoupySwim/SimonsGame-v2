using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Modifiers;

namespace SimonsGame.Menu.InGame
{
	public class AllMagicPane : InGameMenuPartialView
	{

		private enum AbilityInformation
		{
			Power,
			Range,
			Type,
			Speed,
			Heal,
			Level,
		}
		private Dictionary<AbilityInformation, Func<AbilityModifier, string>> _abilityFuncMap = new Dictionary<AbilityInformation, Func<AbilityModifier, string>>()
		{
			{AbilityInformation.Power, (mb) => mb.GetPower()},
			{AbilityInformation.Range, (mb) => mb.GetRange()},
			{AbilityInformation.Type, (mb) => mb.GetElement()},
			{AbilityInformation.Level, (mb) => mb.GetElementAmount()},
			{AbilityInformation.Speed, (mb) => mb.GetSpeed()},
			{AbilityInformation.Heal, (mb) => mb.GetHeal()}
		};

		#region Bounds
		private Vector4 _sortPaneBounds;
		private Vector4 _scrollPaneBounds;
		private Vector4 _magicPaneBounds;
		private Vector4 _hoverInfoPaneBounds;
		private Vector2 _rightArrowSize;
		private int _borderWidth = 3;
		private int _itemHeight = 50;
		#endregion

		private Texture2D _rightArrow;
		private InGameStatusMenu _parent;
		private Color _hoverBackgroundColor = new Color(1, .8f, .3f, .9f);

		private int _scrollNumber = 0;
		private int _selectedMagicIndex = 0;
		private int _numberOfMagicVisible = 0;
		private bool _isSelected = false;
		private bool _isUsingMouse = false;
		private bool _wasLevelUpMode = false;
		public bool IsLevelUpMode = false;

		public AllMagicPane(Vector4 bounds, InGameStatusMenu parent, Vector4 hoverInfoPaneBounds)
			: base(bounds)
		{
			_parent = parent;
			_scrollPaneBounds = new Vector4(Bounds.X + Bounds.W - _itemHeight, bounds.Y + _borderWidth, Bounds.Z - (_borderWidth * 2), _itemHeight);
			_sortPaneBounds = new Vector4(Bounds.X + (_borderWidth * 2), bounds.Y + _borderWidth, _itemHeight, Bounds.W - (_borderWidth * 3) - _scrollPaneBounds.W);
			_magicPaneBounds = new Vector4(Bounds.X + (_borderWidth * 2), bounds.Y + (_borderWidth * 2) + _itemHeight, Bounds.Z - _sortPaneBounds.Z - (_borderWidth * 3), Bounds.W - (_borderWidth * 3) - _scrollPaneBounds.W);
			_hoverInfoPaneBounds = hoverInfoPaneBounds;
			_rightArrow = MainGame.ContentManager.Load<Texture2D>("Test/Menu/RightArrow");
			int arrowSize = (int)(_hoverInfoPaneBounds.X - (_magicPaneBounds.X + _magicPaneBounds.W)) + (_borderWidth * 2);
			arrowSize = arrowSize <= 20 ? (int)(_itemHeight * .6f) + (_borderWidth * 2) : arrowSize;
			_rightArrowSize = new Vector2(arrowSize);
			_isUsingMouse = _parent.Player.UsesMouseAndKeyboard;
		}
		public override void Update(GameTime gameTime, Vector2 newMousePosition)
		{
			if (!IsLevelUpMode)
			{
				if (!_wasLevelUpMode)
				{
					if (_isUsingMouse && newMousePosition.IsInBounds(_magicPaneBounds))
					{
						if (Controls.CurrentMouse.ScrollWheelValue > Controls.PreviousMouse.ScrollWheelValue)
							_scrollNumber = MathHelper.Clamp(_scrollNumber - 1, 0, _numberOfMagicVisible);
						else if (Controls.CurrentMouse.ScrollWheelValue < Controls.PreviousMouse.ScrollWheelValue)
							_scrollNumber = MathHelper.Clamp(_scrollNumber + 1, 0, _numberOfMagicVisible - 3); // Let 3 be visible no matter how far you scroll.
						float relativeMouseY = newMousePosition.Y - (_magicPaneBounds.Y);
						_selectedMagicIndex = MathHelper.Clamp(_scrollNumber + (int)(relativeMouseY / (_itemHeight + _borderWidth)), 0, _numberOfMagicVisible - 1);
					}
					else if (_isSelected && _isUsingMouse)
						_isSelected = false;

					if (_isSelected)
					{
						foreach (AvailableButtons button in Controls.ButtonEnumerate())
						{
							if (Controls.PressedDown(_parent.Player.Id, button))
							{
								var abilityManager = _parent.Player.AbilityManager;
								var magics = abilityManager.KnownAbilityIds.Select(id => abilityManager.GetAbilityInfo(id)).Where(pai => pai.KnownAbility != KnownAbility.Jump);
								abilityManager.SetAbility(magics.ElementAt(_selectedMagicIndex), button);
							}
						}
					}
				}
				else
				{
					_wasLevelUpMode = false;
				}
			}
			else
			{
				_wasLevelUpMode = true;
			}
			// Scroll logic
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)Bounds.X + 3, (int)Bounds.Y, _borderWidth, (int)Bounds.Z), Color.Black);
			spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)Bounds.X + 3, (int)Bounds.Y, (int)Bounds.W - 3, _borderWidth), Color.Black);
			spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)(Bounds.X + Bounds.W) - _borderWidth + 3, (int)Bounds.Y, _borderWidth, (int)Bounds.Z), Color.Black);
			spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)Bounds.X + 3, (int)(Bounds.Y + Bounds.Z) - _borderWidth, (int)Bounds.W - 3, _borderWidth), Color.Black);

			Rectangle sortDivider = new Rectangle((int)(_sortPaneBounds.X - _borderWidth), (int)(_sortPaneBounds.Y + _sortPaneBounds.Z), (int)(_sortPaneBounds.W + (_borderWidth * 2)), _borderWidth);
			spriteBatch.Draw(MainGame.SingleColor, sortDivider, Color.Black);

			Rectangle scrollDivider = new Rectangle((int)(_scrollPaneBounds.X - _borderWidth), (int)(_scrollPaneBounds.Y - _borderWidth), _borderWidth, (int)(_scrollPaneBounds.Z + (_borderWidth * 2)));
			spriteBatch.Draw(MainGame.SingleColor, scrollDivider, Color.Black);


			//spriteBatch.Draw(MainGame.SingleColor, _scrollPaneBounds.ToRectangle(), new Color(1, 0, 0, .8f));
			//spriteBatch.Draw(MainGame.SingleColor, _sortPaneBounds.ToRectangle(), new Color(0, 1, 0, .8f));
			//spriteBatch.Draw(MainGame.SingleColor, _MagicPaneBounds.ToRectangle(), new Color(0, 0, 1, .8f));
			int yOffset = 0;
			AbilityManager abilityManager = _parent.Player.AbilityManager;
			int currentItemNdx = _scrollNumber;
			var magics = abilityManager.KnownAbilityIds.Select(id => abilityManager.GetAbilityInfo(id)).Where(pai => pai.KnownAbility != KnownAbility.Jump);
			_numberOfMagicVisible = magics.Count();
			PlayerAbilityInfo selectedPai = null;
			foreach (PlayerAbilityInfo pai in magics.Skip(_scrollNumber)) // Probably should store this somewhere
			{
				PlayerAbilityInfo tempPai = DrawItem(pai, spriteBatch, yOffset, currentItemNdx == _selectedMagicIndex);
				selectedPai = selectedPai ?? tempPai;
				yOffset += _itemHeight + _borderWidth;
				currentItemNdx++;
			}
			if (selectedPai != null && _isSelected)
			{
				int selectedYOffset = (_selectedMagicIndex + 1) * (_itemHeight + _borderWidth) - ((_itemHeight + _borderWidth) / 2);
				DrawHelperWindow(spriteBatch, selectedYOffset, selectedPai);
			}
		}
		public PlayerAbilityInfo DrawItem(PlayerAbilityInfo pai, SpriteBatch spriteBatch, int yOffset, bool selected)
		{
			int currentX = (int)_magicPaneBounds.X;
			int itemY = (int)(_magicPaneBounds.Y + yOffset);
			spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)_magicPaneBounds.X, itemY, (int)_magicPaneBounds.W, _itemHeight), selected ? new Color(.6f, .6f, .9f, .95f) : new Color(1, 1, 1, .95f));
			Rectangle selectedBounds = new Rectangle(currentX, itemY, 20, _itemHeight);
			spriteBatch.Draw(MainGame.SingleColor, selectedBounds, _parent.Player.AbilityManager.ActiveAbilities.Contains(pai.Id) ? new Color(0, 1, 0, .9f) : new Color(1, 0, 0, .8f));
			currentX += selectedBounds.Width;
			spriteBatch.Draw(MainGame.SingleColor, new Rectangle((int)_magicPaneBounds.X, itemY + _itemHeight, (int)_magicPaneBounds.W, _borderWidth), Color.Black);

			Vector2 textSize = pai.Name.GetTextSize(MainGame.PlainFont);
			spriteBatch.DrawString(MainGame.PlainFont, pai.Name, new Vector2(currentX + _borderWidth + 2, itemY + (textSize.Y / 2)), Color.Black);
			currentX += 300;

			foreach (var kv in _abilityFuncMap)
			{
				spriteBatch.DrawString(MainGame.PlainFont, kv.Key.ToString(), new Vector2(currentX, itemY), Color.Black);
				spriteBatch.DrawString(MainGame.PlainFont, kv.Value(pai.Modifier), new Vector2(currentX, itemY + (_itemHeight / 2.0f)), Color.Black);
				currentX += 80;
			}
			int abilityCount = 0;
			float yOffset2 = 0;
			float currentXEnd = _magicPaneBounds.X + _magicPaneBounds.W - 180;
			foreach (AbilityAttributes aa in Enum.GetValues(typeof(AbilityAttributes)))
			{
				if (pai.AbilityAttributes.HasFlag(aa))
				{
					spriteBatch.DrawString(MainGame.PlainFont, aa.ToString(), new Vector2(currentXEnd, itemY + yOffset2), Color.Black);
					if (abilityCount % 2 == 0)
						yOffset2 = _itemHeight / 2.0f;
					else
					{
						currentXEnd -= 180;
						yOffset2 = 0;
					}
					abilityCount++;
				}
			}
			return selected ? pai : null;
		}
		public void DrawHelperWindow(SpriteBatch spriteBatch, int yOffset, PlayerAbilityInfo pai)
		{
			//spriteBatch.Draw(_rightArrow, new Rectangle((int)(_hoverInfoPaneBounds.X - _rightArrowSize.X) + (_borderWidth * 4) + 1 /*for kicks*/, (int)(_magicPaneBounds.Y + yOffset - (_rightArrowSize.Y / 2)), (int)_rightArrowSize.X, (int)_rightArrowSize.Y), null, _hoverBackgroundColor, 0, new Vector2(_rightArrowSize.X / 2), SpriteEffects.FlipHorizontally, 0);
			//spriteBatch.Draw(MainGame.SingleColor, _hoverInfoPaneBounds.ToRectangle(), _hoverBackgroundColor);
			//Vector4 textBounds = pai.Name.GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_hoverInfoPaneBounds.X + _hoverInfoPaneBounds.W / 2, _hoverInfoPaneBounds.Y));
			//Vector2 textPosition = textBounds.GetPosition() + new Vector2(0, textBounds.Z / 2);
			//spriteBatch.DrawString(MainGame.PlainFont, pai.Name, textPosition, Color.Black);
			// Basically copy DrawItem.  Will not do now.
		}

		public override bool MoveUp()
		{
			if (IsLevelUpMode)
				return _parent.SelectedPane.MoveUp();
			bool stayInBounds = true;
			if (_selectedMagicIndex == 0)
				stayInBounds = false;
			_selectedMagicIndex = MathHelper.Clamp(_selectedMagicIndex - 1, 0, _numberOfMagicVisible - 1);
			if (_selectedMagicIndex < _scrollNumber)
				_scrollNumber = _selectedMagicIndex;
			return stayInBounds;
		}
		public override bool MoveDown()
		{
			if (IsLevelUpMode)
				return _parent.SelectedPane.MoveDown();
			_selectedMagicIndex = MathHelper.Clamp(_selectedMagicIndex + 1, 0, _numberOfMagicVisible - 1);
			return true;
		}
		public override bool MoveLeft()
		{
			if (IsLevelUpMode)
				return _parent.SelectedPane.MoveLeft();
			return true;
		}
		public override bool MoveRight()
		{
			if (IsLevelUpMode)
				return _parent.SelectedPane.MoveRight();
			_isSelected = false;
			return false;
		}
		public override void HasBeenHighlighted()
		{
			if (!_isSelected)
			{
				_isSelected = true;
				//_selectedMagicIndex = _scrollNumber;
			}
		}
	}
}
