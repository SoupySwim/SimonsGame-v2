using Microsoft.Xna.Framework;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	public abstract class PlayerMagicObject : PhysicsObject
	{
		protected Player _player;
		public override Vector4 HitBoxBounds { get { return new Vector4(Position.X - 5, Position.Y - 5, Size.Y + 10, Size.X + 10); } }
		public PlayerMagicObject(Vector2 position, Vector2 hitbox, Group group, Level level, Player player, string name)
			: base(position, hitbox, group, level, name)
		{
			_player = player;
			Team = player.Team;
		}
	}
}
