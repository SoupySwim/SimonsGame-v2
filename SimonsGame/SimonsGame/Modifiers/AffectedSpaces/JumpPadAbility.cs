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
			: base(ModifyType.Add, owner, Element.Normal)
		{
			_aim = aim;
			ModifyPower(pow);
			IsExpiredFunction = IsExpired;
		}
		public override bool IsExpired(GameTime gameTime)
		{
			if (Owner is PhysicsObject && ((PhysicsObject)Owner).AbilityManager.CurrentAbilities.Any(kv => kv.Value is SingleJump))
			{
				_power = 0;
				_hasStopped = true;
			}

			if (_tickCount == _tickTotal)
				_hasReachedEnd = true;

			if (_hasStopped || _tickCount == _tickTotal || _owner.CurrentMovement.Y > 0)
				StopGravity = false;

			Movement = _aim * ((-_power * (_tickTotal - _tickCount)) + (StopGravity ? _owner.MaxSpeed.Y : 0f));
			_tickCount = Math.Min(_tickCount + 1, _tickTotal);

			return _hasStopped || _tickCount >= _tickTotal;
		}
		public override ModifierBase Clone()
		{
			JumpPadAbility jump = new JumpPadAbility(_owner, PowerBase, _aim);
			jump._guid = Id;
			return jump;
		}
		public void ModifyPower(float pow)
		{
			PowerBase = pow;
			var numberOfUnitsTravelling = ((_tickTotal + 1) * _tickTotal) / 2;
			var distanceAffectedByGravity = _tickTotal * _owner.MaxSpeed.Y;
			var totalDistanceNeededToCover = (PowerBase * _owner.Level.PlatformDifference) + Math.Abs(distanceAffectedByGravity);
			_power = totalDistanceNeededToCover / numberOfUnitsTravelling;
			Movement = _aim * ((-_power * (_tickTotal - _tickCount)) + _owner.MaxSpeed.Y);

			_tickTotal = (int)(PowerBase * 9);
		}
		public void ModifyAim(Vector2 aim)
		{
			_aim = new Vector2(aim.X, aim.Y);
			Movement = _aim * ((-_power * (_tickTotal - _tickCount)) + _owner.MaxSpeed.Y);
		}
	}
}
