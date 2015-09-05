using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Modifiers
{
	public class AbilityModifier : CustomModifier
	{
		public float Damage { get; set; }
		public float Speed { get; set; }
		public AbilityAttributes AbilityAttributes { get; set; } // depending on which attributes are flagged, the ability will do different things.

		public AbilityModifier(Func<GameTime, bool> isExpiredFunc, ModifyType type, MainGuiObject owner, Tuple<Element, float> element, float speed, float damage)
			: base(isExpiredFunc, type, owner, element)
		{
			Speed = 9.5f;
			Damage = -200;
			AbilityAttributes = AbilityAttributes.None;
		}

		public virtual void LevelUpMagicHitBoxBuffer(Vector4 hitBox) { }

		public virtual void LevelUpMagic(float speed, float damage, AbilityAttributes newAbilityAttributes)
		{
			Speed = speed;
			Damage = damage;
			AbilityAttributes = newAbilityAttributes;
		}

		public virtual void LevelUpMagicDamage(float damage)
		{
			Damage += damage;
		}

		public virtual void LevelUpMagicSpeed(float speed)
		{
			Speed += speed;
		}

		public virtual void LevelUpMagicSpeedManipulation(float speedManipulationPercent) { }
		public virtual void LevelUpMagicSpeedManipulationTime(int tickAmount) { }

		public virtual void LevelUpMagicStunTimer(int tickAmount) { }

		public virtual void LevelUpMagicKnockback(float kb) { }

		public virtual void LevelUpMagicDuration(int tickTotal) { }

		public virtual void LevelUpSpecial(int type, float amount) { }


		public virtual void LevelUpMagicAddAbilities(AbilityAttributes newAbilities)
		{
			AbilityAttributes |= newAbilities;
		}

		public virtual void StopRotation() { }

		public AbilityModifier(ModifyType type, MainGuiObject owner, Tuple<Element, float> element)
			: base((g) => false, type, owner, element) { }
		public override void Reset()
		{
			//nothing needed here.  Must be handled in children.
		}
		public override ModifierBase Clone(Guid id)
		{
			AbilityModifier mod = new AbilityModifier(IsExpiredFunction, Type, _owner, Element, Speed, Damage);
			mod._guid = id == Guid.Empty ? Guid.NewGuid() : id;

			if (Type == ModifyType.Add)
				mod = (AbilityModifier)(mod + this);
			else
				mod = (AbilityModifier)(mod * this);
			return mod;
		}

		public virtual void SetSize(Vector2 size) { } // Does nothing off the bat.

		public virtual string GetPower() { return string.Format("{0:0}", Damage >= 0 ? 0 : -Damage); }
		public virtual string GetHeal() { return string.Format("{0:0}", (Damage <= 0 ? 0 : Damage)); }
		public virtual string GetElement() { return Element.Item1.ToString(); }
		public virtual string GetElementAmount() { return Element.Item2.ToString(); }
		public virtual string GetRange() { return (Math.Sqrt(Math.Pow(MaxSpeed.X, 2) + Math.Pow(MaxSpeed.Y, 2))).ToString(); }
		public virtual string GetSpeed() { return string.Format("{0:0.0}", Speed); }

	}
}
