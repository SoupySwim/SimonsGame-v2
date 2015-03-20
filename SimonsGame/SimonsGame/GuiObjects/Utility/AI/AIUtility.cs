using Microsoft.Xna.Framework;
using SimonsGame.Modifiers.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects.Utility
{
	public class AIUtility
	{
		private int _sprint3TickCounter = 0;
		private int _sprint3TickTilShoot = 240;
		private bool _movePositive = true;

		public Player _player;

		public AIUtility(Player player)
		{
			_player = player;
		}

		public PlayerControls GetAiControls(PlayerControls previousControls)
		{
			MainGuiObject LandedOnPlatform;
			if (_player.Position == _player.PreviousPosition)
				_movePositive = !_movePositive;
			else if (_player.PrimaryOverlapObjects.TryGetValue(Orientation.Vertical, out LandedOnPlatform)
				&& (_player.Position.X < LandedOnPlatform.Position.X || _player.Position.X + _player.Size.X > LandedOnPlatform.Position.X + LandedOnPlatform.Size.X))
				_movePositive = !_movePositive;
			int xMovement = _movePositive ? 1 : -1;

			AvailableButtons pressedButtons = AvailableButtons.Default;
			_sprint3TickCounter++;

			Player closestPlayer = _player.Level.Players.Where(pl => pl.Key != _player.Id && (_player.Team == Team.None || pl.Value.Team != _player.Team)).Select(pl => pl.Value).OrderBy(pl => { Vector2 diff = (pl.Center - _player.Center); return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y); }).FirstOrDefault();

			LongRangeElementalMagicAbility lrm = (LongRangeElementalMagicAbility)_player.AbilityManager.CurrentAbilities.Select(kv => kv.Value).FirstOrDefault(m => m is LongRangeElementalMagicAbility);

			if (lrm != null && lrm.TestMagic != null && closestPlayer != null
				&& MainGuiObject.GetIntersectionDepth(closestPlayer.Bounds, lrm.TestMagic.HitBoxBounds) != DoubleVector2.Zero)
			{
				pressedButtons = pressedButtons | AvailableButtons.RightBumper;
			}
			else if (_sprint3TickCounter >= _sprint3TickTilShoot && closestPlayer != null)
			{
				pressedButtons = pressedButtons | AvailableButtons.RightBumper;
				_sprint3TickCounter = 0;
			}
			return new PlayerControls()
			{
				GetAim = (p) =>
				{
					Vector2 mousePosition = closestPlayer == null ? p.Center : closestPlayer.Center - p.Center;
					float normalizer = (float)Math.Sqrt(Math.Pow((double)mousePosition.X, 2) + Math.Pow((double)mousePosition.Y, 2));
					return new Vector2(mousePosition.X / normalizer, mousePosition.Y / normalizer);
				},
				PressedButtons = pressedButtons,
				XMovement = xMovement,
				YMovement = 0
			};
		}

	}
}
