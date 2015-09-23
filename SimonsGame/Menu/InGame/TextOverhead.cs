using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.GuiObjects;

namespace SimonsGame.Dialogue
{
	public enum TextOverheadBehavior
	{
		Always,
		Proximity, // Might want to tell what the proximity is...
		NotMoving
	}

	public class TextOverhead
	{
		public string Text;
		private Vector4 _bounds;
		private Vector2 _textOffset;
		private PhysicsObject _character;
		private int _tickCountTotal = 0;
		private int _tickCount;
		private bool _isShowing = true;
		private bool _showOnce = false;
		private TextOverheadBehavior _behavior;
		private bool _hasShown = false;
		private bool _pressToContinue;

		private int _updateHoldup = 0;

		public TextOverhead(string textToDisplay, PhysicsObject character, TextOverheadBehavior behavior, bool showOnce, bool pressToContinue = false)
		{
			Text = textToDisplay;
			_character = character;
			_bounds = Vector4.Zero;

			Vector2 textSize = Text.GetTextSize(MainGame.PlainFont);
			_bounds.W = textSize.X + 20;
			_bounds.Z = textSize.Y + 20;
			_textOffset = new Vector2(10);
			_bounds.X = _character.Center.X - (_bounds.W / 2);
			_bounds.Y = _character.Position.Y - _bounds.Z - 10;
			_tickCountTotal = textToDisplay.Count() * 10; // idk...
			_tickCount = _tickCountTotal;
			_behavior = behavior;
			_showOnce = showOnce;
			_pressToContinue = pressToContinue;

			_isShowing = behavior == TextOverheadBehavior.Always;
		}

		public void SetPressToContine(bool pressToContinue = true)
		{
			_pressToContinue = pressToContinue;
		}

		public void Update(GameTime gameTime)
		{
			if (_updateHoldup == 3) // only do this 15 times a second...
			{
				if (_isShowing)
				{
					_hasShown = true;
					if (_behavior != TextOverheadBehavior.Always)
					{
						_tickCount -= 4;
						if (_tickCount <= 0)
							_isShowing = false;
					}
				}
				else if (!_showOnce || !_hasShown)
				{
					// Check to see if it should be showing.
					if (_behavior == TextOverheadBehavior.Proximity)
					{
						foreach (Player player in _character.Level.Players.Values)
						{
							if (!player.IsAi && player.Id != _character.Id) // Not sure if needed.
							{
								if ((player.Center - _character.Center).GetDistance() <= 320)
									_isShowing = true;
							}
						}
					}
					else if (_behavior == TextOverheadBehavior.NotMoving)
					{
						_isShowing = _character.PreviousPosition == _character.Position;
					}

					if (_isShowing)
					{
						_tickCount = _tickCountTotal;
					}
				}
			}
			_updateHoldup = (_updateHoldup + 1) % 4;
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			if (_isShowing)
			{
				_bounds.X = _character.Center.X - (_bounds.W / 2);
				_bounds.Y = _character.Position.Y - _bounds.Z - 10;
				Vector2 boundsPosition = _bounds.GetPosition();
				Vector2 nextPosition = _bounds.GetPosition();
				spriteBatch.Draw(MainGame.SingleColor, _bounds.ToRectangle(), Color.White);
				spriteBatch.DrawRectangularBorder(boundsPosition, _bounds.GetSize(), Color.Black, 1); // not working?
				spriteBatch.DrawString(MainGame.PlainFont, Text, boundsPosition + _textOffset, Color.Black);

				if (_pressToContinue)
				{
					nextPosition.X += _bounds.W - 6;
					nextPosition.Y += _bounds.Z - 12;
					spriteBatch.DrawString(MainGame.PlainFontSmall, "x", nextPosition, Color.Black);
				}
			}
		}
	}
}
