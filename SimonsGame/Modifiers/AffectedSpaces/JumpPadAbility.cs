using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Modifiers.Abilities;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers
{
	public class JumpPadAbility : CustomModifier
	{
		private int _tickTotal = 20; // number of ticks the ability will take place. This takes a third of a second.
		private int _tickCount = 0; // Where we currently are in the ability.
		public float PowerBase { get; private set; }// The amount of platforms this jump could cover.
		public float AmountOfPlatformsPossible { get { return PowerBase; } }
		private float _power;
		private bool _hasStopped = false;
		public bool HasStopped { get { return _hasStopped; } }
		private Vector2 _aim;


		public JumpPadAbility(MainGuiObject owner, float pow, Vector2 aim)
			: base(ModifyType.Add, owner, new Tuple<Element, float>(Utility.Element.Normal, 0))
		{
			_aim = aim;
			ModifyPower(pow);
			IsExpiredFunction = IsExpired;
			KnockBack = _aim * (-_power + (-_power * (((_tickTotal / 2.0f) - (_tickCount + .5f)) / _tickTotal)));
		}

		public override bool IsExpired(GameTime gameTime)
		{
			if (_tickCount == _tickTotal)
				_hasReachedEnd = true;

			if (_hasStopped || _tickCount == _tickTotal || _owner.CurrentMovement.Y > 0)
				StopGravity = false;

			//KnockBack = _aim * ((-_power * (_tickTotal - _tickCount)) + (StopGravity ? _owner.MaxSpeed.Y : 0f));
			KnockBack = _aim * (-_power + (-_power * (((_tickTotal / 2.0f) - (_tickCount + .5f)) / _tickTotal)));
			_tickCount = Math.Min(_tickCount + 1, _tickTotal);

			return _hasStopped || _tickCount >= _tickTotal;
		}

		public override ModifierBase Clone(Guid id)
		{
			JumpPadAbility jump = new JumpPadAbility(_owner, PowerBase, _aim);
			jump._guid = id == Guid.Empty ? Guid.NewGuid() : id;
			return jump;
		}

		public void ModifyPower(float pow)
		{
			PowerBase = pow;
			float acceleration = _owner.MaxSpeed.Y * _knockBackRecoveryAcceleration;
			var totalDistanceNeededToCover = (PowerBase * _owner.Level.PlatformDifference) + Math.Abs(_tickTotal * acceleration);
			float whatIShallCallN = (float)((-1 + Math.Sqrt(1 + 8 * (totalDistanceNeededToCover / acceleration))) / 2);
			_power = whatIShallCallN * acceleration / _tickTotal;

			_tickTotal = (int)(PowerBase * 3);
		}

		public void ModifyAim(Vector2 aim)
		{
			_aim = new Vector2(aim.X, aim.Y);
			Movement = _aim * ((-_power * (_tickTotal - _tickCount)) + _owner.MaxSpeed.Y);
		}
	}
}
