using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Menu;
using SimonsGame.Menu.MenuScreens;

namespace SimonsGame.MapEditor
{
	public class MapLoaderTopPanel
	{
		public Vector4 Bounds { get { return _topPanelBounds; } }
		private Vector4 _topPanelBounds;
		private Vector2 _titlePosition;
		private string _title = "Load Levels";

		#region Sorting
		private List<TextMenuItemButton> _sortButtons;
		private MapEditorLoadMap _manager;
		private Texture2D _sortArrow;
		private Vector4 _arrowBounds;
		#endregion

		public MapLoaderTopPanel(Vector4 bounds, MapEditorLoadMap manager)
		{
			_sortArrow = MainGame.ContentManager.Load<Texture2D>("Test/Menu/RightArrow");

			_topPanelBounds = bounds;
			Vector2 textSize = _title.GetTextSize(MainGame.PlainFontLarge);
			_titlePosition = new Vector2(10, (bounds.Z - textSize.Y) / 2);
			_manager = manager;
			string[] categoryNames = Enum.GetNames(typeof(OrderLevelByCategory));
			float categoryHeight = _topPanelBounds.Z / categoryNames.Count() - 1;
			float categoryWidth = 200;
			_sortButtons = categoryNames.Select((category, ndx) =>
				{
					Vector4 buttonTotalBounds = new Vector4(_topPanelBounds.W - categoryWidth, 2 + ndx * categoryHeight, categoryHeight, categoryWidth);
					//Vector2 paddingBounds = category.GetPaddingGivenBounds(MainGame.PlainFont, buttonTotalBounds.GetSize());
					//Vector2 buttonSize = buttonTotalBounds.GetSize() - paddingBounds;
					//Vector4 buttonBounds = new Vector4(buttonTotalBounds.GetPosition() + (paddingBounds / 2), buttonSize.Y, buttonSize.X);
					var tuple = category.GetSizeAndPadding(MainGame.PlainFont, buttonTotalBounds);
					return new TextMenuItemButton(() => { MapEditorIOManager.CurrentLevelOrderByCategory = (OrderLevelByCategory)Enum.Parse(typeof(OrderLevelByCategory), category); _manager.RefreshLevels(); },
						category, tuple.Item1, tuple.Item2, false);
				}).ToList();
			GetArrowBounds(_sortButtons.FirstOrDefault());
		}

		private void GetArrowBounds(TextMenuItemButton button)
		{
			float buttonHeight = button.TotalBounds.Z;
			_arrowBounds = new Vector4(button.TotalBounds.GetPosition() - new Vector2(buttonHeight + 6, 0), buttonHeight, buttonHeight);
		}
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.DrawString(MainGame.PlainFontLarge, _title, _titlePosition, Color.Black);
			spriteBatch.Draw(MainGame.SingleColor, new Rectangle(0, (int)_topPanelBounds.Z, (int)_topPanelBounds.W, 2), Color.Black);
			_sortButtons.ForEach(button => button.Draw(gameTime, spriteBatch));
			spriteBatch.Draw(_sortArrow, _arrowBounds.ToRectangle(), Color.Orange);
		}
		public void Update(GameTime gameTime, Vector2 newMousePosition)
		{
			_sortButtons.ForEach(button =>
			{
				if (newMousePosition.IsInBounds(button.TotalBounds))
					button.HasBeenHighlighted();
				else
					button.HasBeenDeHighlighted();
			});
		}
		public void PressEnter(Vector2 newMousePosition)
		{
			_sortButtons.ForEach(button =>
				{
					if (newMousePosition.IsInBounds(button.TotalBounds))
					{
						button.CallAction();
						GetArrowBounds(button);
						return;
					}
				});
		}
	}
}
