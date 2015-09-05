using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimonsGame.Utility;
using SimonsGame.Extensions;

namespace SimonsGame.GuiObjects.Zones
{
	public abstract class GenericZone : MainGuiObject
	{
		public GenericZone(Vector2 position, Vector2 hitbox, Level level, string name)
			: base(position, hitbox, Group.Passable, level, name)
		{
			_objectType = GuiObjectType.Zone;
		}

		public abstract void Initialize(Level level);

		public abstract void CharacterDied(MainGuiObject mgo);
	}
}
