using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers.Abilities
{
	public class SingleJump : AbilityModifier
	{
		protected PhysicsObject _character;
		private int _tickTotal = 8; // number of ticks the ability will take place. This takes a third of a second.
		private int _tickCount = 0; // Where we currently are in the ability.
		private float _powerBase; // The amount of platforms this jump could cover.
		public float AmountOfPlatformsPossible { get { return _powerBase; } }
		private float _power;
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private Func<bool> _checkStopped;
		public Func<bool> CheckStopped { get { return _checkStopped; } set { _checkStopped = value; } }
		private Func<bool> _forceStop;

		/// <summary>
		/// Creates a new Jump ability.
		/// </summary>
		/// <param name="character"> The Player the ability is tied to</param>
		/// <param name="pow"> How far the Player will jump upwards. Given by number of Platforms the Player can jump.</param>
		/// <param name="checkStopped"> A function that checks if the ability is done. </param>
		/// <param name="forceStop"> A function that checks if an ability can no longer be used. </param>
		public SingleJump(PhysicsObject character, float pow, Func<bool> checkStopped, Func<bool> forceStop)
			: base(ModifyType.Add, character, new Tuple<Element, float>(Utility.Element.Normal, 0))
		{
			Acceleration = character.AccelerationBase;
			//var accelerationBase = character.AccelerationBase.Y * 2;
			var acceleration = character.Acceleration.Y * 2;
			_character = character;
			_powerBase = pow;
			//_power = pow;
			if (character.Acceleration.Y != 0)
			{
				var totalDistanceNeededToCover = (_powerBase * character.Level.PlatformDifference) + Math.Abs(_tickTotal * _character.MaxSpeed.Y * _knockBackRecoveryAcceleration);
				float whatIShallCallN = (float)((-1 + Math.Sqrt(1 + 8 * (totalDistanceNeededToCover / acceleration))) / 2);
				_power = whatIShallCallN * acceleration / _tickTotal;
			}
			_checkStopped = checkStopped;
			_forceStop = forceStop;
			IsExpiredFunction = IsExpiredFunc;
			//StopGravity = true;
			KnockBack.Y = -_power + (-_power * (((_tickTotal / 2.0f) - (_tickCount + .5f)) / _tickTotal));// * (_tickTotal - _tickCount);// -_power + (-_power * (((_tickTotal / 2.0f) - _tickCount) / _tickTotal));
		}
		public bool IsExpiredFunc(GameTime gameTime)
		{
			if (_checkStopped())
			{
				_power = 0;
				KnockBack.Y = 0;
				Acceleration = Vector2.Zero;
				_hasStopped = true;
			}

			if (_tickCount >= _tickTotal)
			{
				KnockBack.Y = 0;
				Acceleration = Vector2.Zero;
				_hasReachedEnd = true;
			}

			//if (_hasStopped || _tickCount == _tickTotal || _character.CurrentMovement.Y > 0)
			//	StopGravity = false;

			if (_character.IsOnLadder && (_tickCount == 0 || _tickCount > _tickTotal / 4.0f) && !_hasStopped)
			{
				Movement = new Vector2(0, -4.5f);
				KnockBack.Y = 0;
				_tickCount = _tickTotal - 1;
			}
			else if (!_hasStopped)
			{
				if (_tickTotal > _tickCount)
					KnockBack.Y = -_power + (-_power * (((_tickTotal / 2.0f) - (_tickCount + .5f)) / _tickTotal));// * (_tickTotal - _tickCount);// -_power + (-_power * (((_tickTotal / 2.0f) - _tickCount) / _tickTotal));
				//Movement = new Vector2(0, (-_power * (_tickTotal - _tickCount)) + (StopGravity ? _character.MaxSpeed.Y : 0f));
				_tickCount = Math.Min(_tickCount + 1, _tickTotal + 1);
			}
			// If you've forced stopped or
			// If you've stopped or reached the end of the jump while be landed, then we are done!
			return _forceStop() || ((_hasStopped || _hasReachedEnd) && _character.IsLanded);
		}
		public override void Reset()
		{
			_tickCount = 0;
			_hasStopped = false;
			StopGravity = true;
			_power = _powerBase;
			//Movement = new Vector2(0, -_power * (_tickTotal - _tickCount));
			KnockBack.Y = -_power;// -_power + (-_power * (((_tickTotal / 2.0f) - _tickCount) / _tickTotal));
			base.Reset();
		}
		public override ModifierBase Clone(Guid id)
		{
			SingleJump jump = new SingleJump(_character, _powerBase, _checkStopped, _forceStop);
			jump._guid = id == Guid.Empty ? Guid.NewGuid() : id;
			return jump;
		}
	}
}