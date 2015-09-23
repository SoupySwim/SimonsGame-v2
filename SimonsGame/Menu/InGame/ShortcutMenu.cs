using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Menu.InGame;
using SimonsGame.GuiObjects;
using SimonsGame.Modifiers;

namespace SimonsGame.Menu.MenuScreens
{
	public class ShortcutMenu
	{
		private Vector4 _overlayBounds;
		private TextMenuItemButton[] _magicButtons;
		private Player _player;
		private Vector2 _centerOfBounds;
		private Vector2 _currentCenter;
		private Vector2[] _originalPositions;
		private int selectedIndex = 0;
		float _radius;

		public ShortcutMenu(Player player, Vector4 overlayBounds)
		{
			_player = player;
			_overlayBounds = overlayBounds;
			_magicButtons = new TextMenuItemButton[8];
			_originalPositions = new Vector2[8];
			_radius = overlayBounds.Z / 4.5f;
			_centerOfBounds = overlayBounds.GetPosition() + (overlayBounds.GetSize() / 2);
			float radians = 0;

			for (int i = 0; i < 8; i++)
			{
				Vector2 angleVector = _radius * new Vector2(-(float)Math.Cos(radians), (float)Math.Sin(radians));

				Vector4 bounds = new Vector4(_centerOfBounds.X + (angleVector.X - 40), _centerOfBounds.Y + (angleVector.Y - 15), 30, 80);
				Vector2 padding = new Vector2(10, 10);

				_magicButtons[i] = new TextMenuItemButton(() => { }, "", bounds, padding, false);
				_originalPositions[i] = bounds.GetPosition();
				radians += (float)(Math.PI / 4);
			}
		}

		public void OpenShortcutMenu(Vector2 playerOffsetFromMiddle)
		{
			AbilityManager abilityManager = _player.AbilityManager;
			IEnumerable<PlayerAbilityInfo> magics = abilityManager.KnownAbilityIds.Select(id => abilityManager.GetAbilityInfo(id)).Where(pai => pai.KnownAbility != KnownAbility.Jump);
			int magicIndex = 0;
			foreach (PlayerAbilityInfo pai in magics.Take(8))
			{
				_magicButtons[magicIndex].Text = pai.Name;
				_magicButtons[magicIndex].Bounds.X = _originalPositions[magicIndex].X + playerOffsetFromMiddle.X;
				_magicButtons[magicIndex].Bounds.Y = _originalPositions[magicIndex].Y + playerOffsetFromMiddle.Y;
				magicIndex++;
			}
			for (int i = magicIndex; magicIndex < 8; magicIndex++)
			{
				_magicButtons[magicIndex].Bounds.X = _originalPositions[magicIndex].X + playerOffsetFromMiddle.X;
				_magicButtons[magicIndex].Bounds.Y = _originalPositions[magicIndex].Y + playerOffsetFromMiddle.Y;
			}
			_currentCenter = _centerOfBounds + playerOffsetFromMiddle;
		}

		public void Update(GameTime gameTime)
		{
			// Get Selected Button

			Vector2 aim = _player.GetAim();
			float angleFromVector = (float)(Math.Atan2(-aim.X, -aim.Y) - (3 * Math.PI / 8));

			if (angleFromVector < 0)
				angleFromVector += (float)(Math.PI * 2);
			int index = (int)(angleFromVector / (Math.PI / 4));

			if (selectedIndex != index)
			{
				_magicButtons[selectedIndex].HasBeenDeHighlighted();
				_magicButtons[index].HasBeenHighlighted();
				selectedIndex = index;
			}

			foreach (AvailableButtons button in Controls.ButtonEnumerate())
			{
				if (Controls.PressedDown(_player.Id, button))
				{
					AbilityManager abilityManager = _player.AbilityManager;
					IEnumerable<PlayerAbilityInfo> magics = abilityManager.KnownAbilityIds.Select(id => abilityManager.GetAbilityInfo(id)).Where(pai => pai.KnownAbility != KnownAbility.Jump);
					if (magics.Skip(index).Any())
						abilityManager.SetAbility(magics.ElementAt(index), button);
				}
			}
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			foreach (TextMenuItemButton button in _magicButtons)
			{
				float radians = (float)(selectedIndex * (Math.PI / 4));
				Vector2 angleVector = _radius * new Vector2(-(float)Math.Cos(radians), (float)Math.Sin(radians));
				spriteBatch.DrawLine(_currentCenter, _currentCenter + angleVector, Color.Black);
				button.Draw(gameTime, spriteBatch);
			}
		}
	}
}