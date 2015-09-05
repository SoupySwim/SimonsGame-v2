using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimonsGame.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects
{
	// An interface to define that an object could be teleported to.
	public interface ITeleportable
	{
		Guid Id { get; }
		byte GetTeleportId();
		bool CanTeleportTo();
		void TeleportObject(MainGuiObject mgo);
		void Draw(GameTime gameTime, SpriteBatch spriteBatch, Player curPlayer);
		void Update(GameTime gameTime);
		GuiObjectStore GetGuiObjectStore();
	}
}