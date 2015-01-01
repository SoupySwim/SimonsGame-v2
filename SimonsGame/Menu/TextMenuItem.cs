using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Menu
{
	public class TextMenuItem : MenuItem
	{
		public string Text { get; set; }
		private Color _currentColor;
		public Color _defaultColor;
		public Color DefaultColor { get { return _defaultColor; } }
		public Color _selectedColor;
		public Color SelectedColor { get { return _selectedColor; } }
		public TextMenuItem(Action selectedAcion, string text, Vector4 bounds, bool isSelected = false)
			: this(selectedAcion, text, bounds, new Color(1f, 1f, 1f), new Color(240, 50, 50), isSelected) { }
		public TextMenuItem(Action selectedAcion, string text, Vector4 bounds, Color defaultColor, Color selectedColor, bool isSelected = false)
			: base(selectedAcion, bounds)
		{
			_defaultColor = defaultColor;
			_selectedColor = selectedColor;
			_currentColor = isSelected ? selectedColor : defaultColor;
			IsHighLighted = isSelected;
			Text = text;
		}
		public override void HasBeenHighlighted()
		{
			_currentColor = _selectedColor;
		}
		public override void HasBeenDeHighlighted()
		{
			_currentColor = _defaultColor;
		}
		public override void Update(GameTime gameTime)
		{
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			//spriteBatch.Draw(MainGame.SingleColor, Bounds.ToRectangle(), Color.Black);
			spriteBatch.DrawString(MainGame.PlainFont, Text, new Vector2(Bounds.X, Bounds.Y), _currentColor);
		}
	}
}
