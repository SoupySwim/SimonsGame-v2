using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using SimonsGame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.MapEditor
{
	public class MapEditorItem
	{
		public Vector4 Bounds { get; set; }
		public Guid ItemId { get; set; }
		public Group Group { get; set; }
		public GuiObjectType GuiObjectType { get; set; }
		public MapEditorItem(Vector4 bounds, Guid itemId, Group group, GuiObjectType guiObjectType)
		{
			Bounds = bounds;
			ItemId = itemId;
			Group = group;
			GuiObjectType = guiObjectType;
		}
	}
}
