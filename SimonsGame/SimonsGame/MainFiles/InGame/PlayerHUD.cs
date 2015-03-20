using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.GuiObjects;
using SimonsGame.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Utility
{
	public class PlayerHUD
	{
		private Player _player;
		private Vector4 _playerViewportBounds;
		private Texture2D _singleColor;
		public PlayerHUD(Player player, Vector4 playerViewportBounds)
		{
			IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)Program.Game.Services.GetService(typeof(IGraphicsDeviceService));
			_player = player;
			_playerViewportBounds = playerViewportBounds;
			_singleColor = new Texture2D(graphicsService.GraphicsDevice, 1, 1);
			_singleColor.SetData(new[] { Color.CornflowerBlue });

		}
		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			string scoreAndTimerText = "score = " + string.Format("{0:0.00}", Player.Sprint3TestScore / 1000) + "\r\n" + "Time = " + string.Format("{0:0.00}", GameStateManager.GameTimer.TotalSeconds);
			Vector2 textPosition = new Vector2(10, 3);
			spriteBatch.DrawString(MainGame.PlainFont, scoreAndTimerText, textPosition, Color.Black);

			Rectangle bottomHUDBounds = new Rectangle((int)(_playerViewportBounds.W / 3), (int)(_playerViewportBounds.Z * .91), (int)(_playerViewportBounds.W / 3), (int)(_playerViewportBounds.Z * 087));
			spriteBatch.Draw(_singleColor, bottomHUDBounds, new Color(0, 0, 0, .8f));
			IEnumerable<PlayerAbilityInfo> abilities = _player.AbilityManager.KnownAbilities.SelectMany(kv => kv.Value).Where(i => i.Cooldown != TimeSpan.Zero);
			int abilityCount = abilities.Count() + 1;
			int ndx = 1;
			float itemWidth = bottomHUDBounds.Width / abilityCount;


			spriteBatch.DrawString(MainGame.PlainFont, "Health\r\n" + _player.HealthCurrent, new Vector2(bottomHUDBounds.X + 6, bottomHUDBounds.Y + 2), Color.White);

			foreach (PlayerAbilityInfo info in abilities)
			{
				string text = string.Format("{0:0.00}", _player.AbilityManager.CoolDownTimer(info) / 1000f);
				//spriteBatch.DrawString(_singleColor, bottomHUDBounds, Color.Black);
				spriteBatch.DrawString(MainGame.PlainFont, text, new Vector2(bottomHUDBounds.X + 10 + (ndx++ * itemWidth), bottomHUDBounds.Y + 5), Color.White);
			}

		}
	}
}
