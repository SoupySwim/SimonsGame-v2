using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.MainFiles.InGame;

namespace SimonsGame.Menu.MenuScreens
{
	public class GameStatisticsMenu : MainMenuScreen
	{
		private string _endGameStatistics = "";
		private Vector2 _textPosition;
		public GameStatisticsMenu(MenuStateManager manager, Vector2 screenSize)
			: base(manager)
		{
			_menuLayout = new MenuItem[1][];
			_menuLayout[0] = new MenuItem[1];
			_screenSize = screenSize;
			_menuLayout[0][0] = new TextMenuItem(new Action(() => { _manager.NavigateToPreviousScreen(); }), "Done",
				"Done".GetTextBoundsByCenter(MainGame.PlainFont, new Vector2(_screenSize.X / 2, 20)), true);
		}
		public void PopulateGameStatistics(GameStatistics gameStatistics)
		{
			_endGameStatistics = gameStatistics.Highlights.Aggregate("", (endString, highlight) =>
				{
					string currentString = string.Format("At {0} seconds {1} by Player 1.", highlight.TimeOccured, highlight.Description, highlight.Character.Name);
					return endString == "" ? currentString : endString + "\r\n" + currentString;
				});
			_textPosition = _endGameStatistics.GetTextBoundsByCenter(MainGame.PlainFont, _screenSize / 2).GetPosition();
		}
		protected override void DrawExtra(GameTime gameTime, SpriteBatch spriteBatch)
		{
			base.DrawExtra(gameTime, spriteBatch);
			spriteBatch.DrawString(MainGame.PlainFont, _endGameStatistics, _textPosition, Color.Black);
		}
	}
}