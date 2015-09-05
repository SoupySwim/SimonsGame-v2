using Microsoft.Xna.Framework;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Extensions;

namespace SimonsGame.GuiObjects
{
	public abstract class PlayerMagicObject : PhysicsObject
	{
		protected Animation _magicAnimation;
		protected PhysicsObject _character;

		public List<ModifierBase> ModifierList { get { return _modifierList; } }
		protected List<ModifierBase> _modifierList = new List<ModifierBase>();

		protected float _knockbackBase;

		public Vector4 BufferVector = new Vector4(-5, -5, 10, 10);
		public override Vector4 HitBoxBounds { get { return BufferVector + Bounds; } }//new Vector4(Position.X - 5, Position.Y - 5, Size.Y + 10, Size.X + 10); } }
		//public Action _expire = null;

		private bool _hasExpiredAlready = false;

		public PlayerMagicObject(Vector2 position, Vector2 hitbox, Group group, Level level, PhysicsObject player, string name, Animation magicAnimation)
			: base(position, hitbox, group, level, name)
		{
			AccelerationBase = new Vector2(1);
			_character = player;
			Team = player.Team;
			_magicAnimation = magicAnimation;
			_animator.PlayAnimation(_magicAnimation);
		}

		public void Expire(MainGuiObject hitObject = null)
		{
			if (!_hasExpiredAlready)
			{
				ExtraExpireFunction(hitObject);

				Level.RemoveGuiObject(this);
				_hasExpiredAlready = true;
			}
		}

		public void AddSpeedManipulation(Vector2 amount, int tickCount)
		{
			ModifierBase speedEffect = new TickModifier(tickCount, ModifyType.Multiply, _character, new Tuple<Element, float>(Element.Normal, 0.0f));
			speedEffect.MaxSpeed = amount;
			speedEffect.Movement = amount;
			_modifierList.Add(speedEffect);
		}

		public void AddStun(int tickCount)
		{
			ModifierBase speedEffect = new TickModifier(tickCount, ModifyType.Multiply, _character, new Tuple<Element, float>(Element.Normal, 0.0f));
			speedEffect.PreventControls = true;
			_modifierList.Add(speedEffect);
		}

		public void AddKnockback(float amount)
		{
			_knockbackBase = amount;
		}

		public virtual void ExtraExpireFunction(MainGuiObject hitObject = null)
		{
			if (hitObject != null)
			{
				foreach (ModifierBase mod in _modifierList)
					hitObject.HitByObject(this, mod);
				hitObject.HitByObject(this, this.GetKnockbackAbility(hitObject, _knockbackBase));
			}
		}
	}
}
