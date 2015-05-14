using Microsoft.Xna.Framework;
using SimonsGame.Modifiers;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public abstract class PlayerMagicObject : PhysicsObject
	{
		protected Animation _magicAnimation;
		protected PhysicsObject _character;
		public ModifierBase Modifier { get { return _modifier; } }
		protected ModifierBase _modifier;
		protected Vector4 _bufferVector = new Vector4(-5, -5, 10, 10);
		public override Vector4 HitBoxBounds { get { return _bufferVector + Bounds; } }//new Vector4(Position.X - 5, Position.Y - 5, Size.Y + 10, Size.X + 10); } }
		//public Action _expire = null;

		private bool _hasExpiredAlready = false;


		public PlayerMagicObject(Vector2 position, Vector2 hitbox, Group group, Level level, PhysicsObject player, string name, Animation magicAnimation)
			: base(position, hitbox, group, level, name)
		{
			_character = player;
			Team = player.Team;
			_magicAnimation = magicAnimation;
			_animator.PlayAnimation(_magicAnimation);
		}

		public virtual void Expire(MainGuiObject hitObject = null)
		{
			if (!_hasExpiredAlready)
			{
				//if (_expire != null)
				//	Level.RemoveGuiObject(this);
				//else
				//	_expire();
				ExtraExpireFunction(hitObject);

				Level.RemoveGuiObject(this);
				_hasExpiredAlready = true;
			}
		}
		public virtual void ExtraExpireFunction(MainGuiObject hitObject = null)
		{
			if (hitObject != null)
				hitObject.HitByObject(this, _modifier);
		}
	}
}
