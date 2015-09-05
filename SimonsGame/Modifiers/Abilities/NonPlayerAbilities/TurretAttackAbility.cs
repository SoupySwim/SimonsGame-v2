using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.GuiObjects.ElementalMagic;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.Modifiers.Abilities
{
	public class TurretAttackAbility : AbilityModifier
	{
		private StandardTurret _turret;
		private int _tickTotal = 35; // number of ticks the ability will take place. This takes one and a third of a second.
		private int _tickCount = 0; // Where we currently are in the ability.
		private TurretAttack _turretAttack;
		public TurretAttackAbility(StandardTurret t, Tuple<Element, float> element)
			: base(ModifyType.Add, t, element)
		{
			_turret = t;
			_tickTotal = 37;
			IsExpiredFunction = IsExpiredFunc;
		}
		public bool IsExpiredFunc(GameTime gameTime)
		{
			// When we just start, make the object!
			if (_tickCount == 0)
			{
				// For now, a lot of things are hard coded...
				Vector2 projectileHitbox = new Vector2(18, 18);
				Vector2 projectilePosition = _turret.Center - (projectileHitbox / 2);
				//IEnumerable<PhysicsObject> characters = _turret.Level.Players.Values.Where(p => p.Team != _turret.Team);
				List<MainGuiObject> characters = _turret.Level.GetAllCharacterObjects(new Vector4(projectilePosition, projectileHitbox.X, projectileHitbox.Y)).Where(p => p.Team != _turret.Team && p.Team > Team.Neutral).ToList();
				MainGuiObject targetedCharacter = characters.OrderBy(p => (p.Center - _turret.Center).GetDistance()).FirstOrDefault();

				Vector2 aim = targetedCharacter == null ? _turret.GetAim() : targetedCharacter.Center - _turret.Center;
				float normalizer = (float)Math.Sqrt(Math.Pow((double)aim.X, 2) + Math.Pow((double)aim.Y, 2));
				aim = aim / normalizer;

				_turretAttack = new TurretAttack(projectilePosition, projectileHitbox, Group.Passable, _turret.Level, aim * 15, _turret, Element);
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
		public override ModifierBase Clone(Guid id)
		{
			TurretAttackAbility magic = new TurretAttackAbility(_turret, Element);
			magic._guid = id == Guid.Empty ? Guid.NewGuid() : id;
			return magic;
		}
	}
}
