using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;
using SimonsGame.Utility;
using Microsoft.Xna.Framework.Graphics;

namespace SimonsGame.Modifiers.Abilities
{
	public class BlinkAbility : AbilityModifier
	{
		protected PhysicsObject _character;
		private int _tickTotal = 41; // number of ticks the ability will take place. This takes two third of a second.
		private int _tickCount = 0; // Where we currently are in the ability.
		public float AmountOfPlatformsPossible { get { return _powerMax; } }
		private float _power; // The current number of platforms this teleport could cover.
		private float _powerMin; // The minimum number of platforms this teleport could cover.
		private float _powerMax; // The maximum number of platforms this teleport could cover.
		private Func<bool> _checkStopped;
		private Animation _waypointAnimation;

		/// <summary>
		/// Creates a new Jump ability.
		/// </summary>
		/// <param name="p"> The Player the ability is tied to</param>
		/// <param name="pow"> How far the Player will teleport</param>
		/// <param name="checkStopped"> A function that checks if the ability is done. </param>
		/// <param name="forceStop"> A function that checks if an ability can no longer be used. </param>
		public BlinkAbility(PhysicsObject p, float pow, Func<bool> checkStopped)
			: base(ModifyType.Add, p, Utility.Element.Normal)
		{
			_character = p;
			_powerMin = 1;
			_power = _powerMin; // For now, start at 1, and move up to the _powerMax
			_powerMax = pow;
			_checkStopped = checkStopped;
			IsExpiredFunction = IsExpiredFunc;
			Texture2D waypointTexture = MainGame.ContentManager.Load<Texture2D>("Test/PlayerStill");
			_waypointAnimation = new Animation(waypointTexture, 1, false, waypointTexture.Bounds.Width, waypointTexture.Bounds.Height, new Vector2(_character.Size.X / waypointTexture.Bounds.Width, _character.Size.Y / waypointTexture.Bounds.Height));
		}
		public bool IsExpiredFunc(GameTime gameTime)
		{
			if (_checkStopped())
				_hasReachedEnd = true;

			if (_tickCount == _tickTotal)
				_hasReachedEnd = true;

			_power = _powerMin + ((_powerMax - _powerMin) * ((float)_tickCount / _tickTotal));

			_tickCount = Math.Min(_tickCount + 1, _tickTotal);

			//PlayerControls playerControls = GameStateManager.GetControlsForPlayer(_character);
			//Vector2 playerAim = playerControls.GetAim(_character);
			Vector2 playerAim = _character.GetAim();
			Vector2 teleportSize = playerAim * _power * _character.Level.PlatformDifference;

			Vector2 newPosition = _character.Center;
			Vector2 newSize = teleportSize;
			if (playerAim.X < 0)
			{
				newPosition.X = _character.Center.X + newSize.X;
				newSize.X = Math.Abs(newSize.X);
			}
			if (playerAim.Y < 0)
			{
				newPosition.Y = _character.Center.Y + newSize.Y;
				newSize.Y = Math.Abs(newSize.Y);
			}
			Vector4 teleportBounds = new Vector4(newPosition, newSize.X, newSize.Y);

			// If we overlap with something we can't pass, then we check if it overlaps all of either X or Y.
			// If it does, then we can't teleport that far.  Get overlap bounds and lower _power to that position.

			_character.Level.AddLevelAnimation(new AnimatedLevelAnimation(_waypointAnimation, _character.Level, _character.Center + teleportSize - (_character.Size / 2), new Color(0, 0, 0, .5f)));

			// If we are done teleporting, then teleport!
			if (_hasReachedEnd)
			{
				_character.Center = _character.Center + teleportSize;
			}

			return _hasReachedEnd;
		}
		public override void Reset()
		{
			_tickCount = 0;
			_power = _powerMin;
			base.Reset();
		}
		public override ModifierBase Clone()
		{
			BlinkAbility blink = new BlinkAbility(_character, _powerMax, _checkStopped);
			return blink;
		}
	}
}
