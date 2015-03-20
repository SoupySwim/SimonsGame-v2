using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.GuiObjects.ElementalMagic;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers.Abilities
{
	public class TurretAttackAbility : CustomModifier
	{
		private StandardTurret _turret;
		private int _tickTotal = 31; // number of ticks the ability will take place. This takes one and a third of a second.
		private int _tickCount = 0; // Where we currently are in the ability.
		private TurretAttack _turretAttack;
		public TurretAttackAbility(StandardTurret t)
			: base(ModifyType.Add, t)
		{
			_turret = t;
			IsExpiredFunction = IsExpiredFunc;
		}
		public bool IsExpiredFunc(GameTime gameTime)
		{
			// When we just start, make the object!
			if (_tickCount == 0)
			{
				// For now, a lot of things are hard coded...
				Vector2 projectileHitbox = new Vector2(18, 18);
				IEnumerable<Player> players = _turret.Level.Players.Values.Where(p => p.Team != _turret.Team);
				Player targetedPlayer = players.OrderByDescending(p => MainGuiObject.GetIntersectionDepth(p.Bounds, _turret.SensorBounds)).FirstOrDefault();
				Vector2 aim = targetedPlayer.Center - _turret.Center;
				float normalizer = (float)Math.Sqrt(Math.Pow((double)aim.X, 2) + Math.Pow((double)aim.Y, 2));
				aim = aim / normalizer;

				_turretAttack = new TurretAttack(_turret.Center - (projectileHitbox / 2), projectileHitbox, Group.Passable, _turret.Level, aim * 15, _turret);
				_turret.Level.AddGuiObject(_turretAttack);
			}

			_hasReachedEnd = _tickCount == _tickTotal;
			_tickCount = Math.Min(_tickCount + 1, _tickTotal);

			if (_hasReachedEnd)
			{
				_turretAttack.Detonate();
			}

			return _hasReachedEnd;
		}
		public override ModifierBase Clone()
		{
			TurretAttackAbility magic = new TurretAttackAbility(_turret);
			return magic;
		}
	}
}
