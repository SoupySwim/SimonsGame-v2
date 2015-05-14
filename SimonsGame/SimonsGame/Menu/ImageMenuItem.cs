using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Menu
{
	public class ImageMenuItemButton : MenuItemButton
	{
		public Texture2D Image { get; set; }
		private Color _currentColor;
		public Color _defaultColor;
		public Color DefaultColor { get { return _defaultColor; } set { _defaultColor = value; } }
		public Color _selectedColor;
		public Color SelectedColor { get { return _selectedColor; } set { _selectedColor = value; } }

		private SpriteEffects _spriteEffects;
		public ImageMenuItemButton(Action selectedAcion, Texture2D image, Vector4 bounds, bool isSelected = false, SpriteEffects spriteEffects = SpriteEffects.None)
			: this(selectedAcion, image, bounds, new Color(1f, 1f, 1f), new Color(240, 50, 50), isSelected, spriteEffects) { }
		public ImageMenuItemButton(Action selectedAcion, Texture2D image, Vector4 bounds, Color defaultColor, Color selectedColor, bool isSelected = false, SpriteEffects spriteEffects = SpriteEffects.None)
			: base(selectedAcion, bounds)
		{
			_defaultColor = defaultColor;
			_selectedColor = selectedColor;
			_currentColor = isSelected ? selectedColor : defaultColor;
			IsHighLighted = isSelected;
			Image = image;
			_spriteEffects = spriteEffects;
		}
		public override void HasBeenHighlighted()
		{
			_currentColor = _selectedColor;
			IsHighLighted = true;
		}
		public override void HasBeenDeHighlighted()
		{
			_currentColor = _defaultColor;
			IsHighLighted = false;
		}
		public override void Update(GameTime gameTime)
		{
		}
		public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			//spriteBatch.Draw(Image, Bounds.ToRectangle(), _currentColor);
			spriteBatch.Draw(Image, TotalBounds.ToRectangle(), null, _currentColor, 0, Vector2.Zero, _spriteEffects, 0);
		}
	}
}
