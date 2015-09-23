using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Menu
{
	public class TextMenuItemButton : MenuItemButton
	{
		public string Text { get; set; }
		private Color _currentColor;
		public Color DefaultColor;
		public Color SelectedColor;
		private bool _useSmallFont;
		public TextMenuItemButton(Action selectedAcion, string text, Vector4 bounds, Vector2 padding, bool isSelected = false, bool useSmallFont = false)
			: this(selectedAcion, text, bounds, new Color(1f, 1f, 1f), new Color(240, 50, 50), padding, isSelected, useSmallFont) { }
		public TextMenuItemButton(Action selectedAcion, string text, Vector4 bounds, Color defaultColor, Color selectedColor, Vector2 padding, bool isSelected = false, bool useSmallFont = false)
			: base(selectedAcion, bounds, padding)
		{
			DefaultColor = defaultColor;
			SelectedColor = selectedColor;
			_currentColor = isSelected ? selectedColor : defaultColor;
			IsHighLighted = isSelected;
			Text = text;
			_useSmallFont = useSmallFont;
		}
		public override void HasBeenHighlighted()
		{
			_currentColor = SelectedColor;
			IsHighLighted = true;
		}
		public override void HasBeenDeHighlighted()
		{
			_currentColor = DefaultColor;
			IsHighLighted = false;
		}
		public override void OverrideColor(Color color)
		{
			_currentColor = color;
		}
		public override void Update(GameTime gameTime)
		{
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			//Vector4 realBounds = TotalBounds + new Vector4(-Padding.X / 2, -Padding.Y / 2, Padding.X, Padding.Y);
			spriteBatch.Draw(MainGame.SingleColor, (TotalBounds + new Vector4(-4, -4, 8, 8)).ToRectangle(), Color.Black);
			spriteBatch.Draw(MainGame.SingleColor, (TotalBounds + new Vector4(-2, -2, 4, 4)).ToRectangle(), (_currentColor.ToVector3() + new Vector3(.4f)).ToColor());
			//spriteBatch.Draw(MainGame.SingleColor, Bounds.ToRectangle(), Color.Black);
			spriteBatch.DrawString(_useSmallFont ? MainGame.PlainFontSmall : MainGame.PlainFont, Text, new Vector2(Bounds.X, Bounds.Y), _currentColor);
		}
	}
}
