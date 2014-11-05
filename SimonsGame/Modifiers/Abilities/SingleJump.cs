using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers.Abilities
{
	class SingleJump : CustomModifier
	{
		protected Player _player;
		private int _tickTotal = 21; // number of ticks the ability will take place. This takes a third of a second.
		private int _tickCount = 0; // Where we currently are in the ability.
		private float _powerBase; // The amount of platforms this jump could cover.
		public float AmountOfPlatformsPossible { get { return _powerBase; } }
		private float _power;
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private Func<bool> _checkStopped;
		private Func<bool> _forceStop;

		/// <summary>
		/// Creates a new Jump ability.
		/// </summary>
		/// <param name="p"> The Player the ability is tied to</param>
		/// <param name="pow"> How far the Player will jump upwards. Given by number of Platforms the Player can jump.</param>
		/// <param name="checkStopped"> A function that checks if the ability is done. </param>
		/// <param name="forceStop"> A function that checks if an ability can no longer be used. </param>
		public SingleJump(Player p, float pow, Func<bool> checkStopped, Func<bool> forceStop)
			: base(ModifyType.Add)
		{
			_player = p;
			_powerBase = pow;
			//_power = pow;
			_power = (((.85f * _player.MaxSpeed.Y) * _tickTotal) + ((p.Level.PlatformDifference + 10) * pow)) /
				((_tickTotal * (_tickTotal + 1)) / 2);
			Movement = new Vector2(0, (-_power * (_tickTotal - _tickCount)) + (.85f * _player.MaxSpeed.Y));
			_checkStopped = checkStopped;
			_forceStop = forceStop;
			isExpiredFunction = IsExpiredFunc;
			StopGravity = true;
		}
		public bool IsExpiredFunc(GameTime gameTime)
		{
			if (_checkStopped())
			{
				_power = 0;
				_hasStopped = true;
			}

			if (_tickCount == _tickTotal)
				_hasReachedEnd = true;

			_tickCount = Math.Min(_tickCount + 1, _tickTotal);
			if (_hasStopped || _tickCount == _tickTotal || _player.CurrentMovement.Y > 0)
			{
				StopGravity = false;
			}
			Movement = new Vector2(0, (-_power * (_tickTotal - _tickCount)) + (StopGravity ? (.85f * _player.MaxSpeed.Y) : 0f));
			return _forceStop() || (_hasStopped && _tickCount >= _tickTotal && _player.IsLanded);
		}
		public override void Reset()
		{
			_tickCount = 0;
			_hasStopped = false;
			StopGravity = true;
			_power = _powerBase;
			Movement = new Vector2(0, -_power * (_tickTotal - _tickCount));
			base.Reset();
		}
		public override ModifierBase Clone()
		{
			SingleJump jump = new SingleJump(_player, _powerBase, _checkStopped, _forceStop);
			return jump;
		}
	}
}