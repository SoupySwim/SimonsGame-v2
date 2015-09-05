using Microsoft.Xna.Framework;
using SimonsGame.GuiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.MainFiles
{
	public abstract class ObtainableItem
	{

		// This map must include all Types that extend Obtainable Item.
		// All Carriers must have a constructor that accepts a MainGuiObject and an ObtainableItem.
		private static Dictionary<Type, Type> _carrierMap = new Dictionary<Type, Type>()
		{
			{ typeof(SmallKey), typeof(SmallKeyObject) },
		};
		private static Dictionary<Type, Vector2> _sizeMap = new Dictionary<Type, Vector2>()
		{
			{ typeof(SmallKey), new Vector2(40,40) },
		};

		public Type CarrierType { get { Type outType; _carrierMap.TryGetValue(this.GetType(), out outType); return outType; } }
		public Vector2 DefaultSize { get { Vector2 outSize; _sizeMap.TryGetValue(this.GetType(), out outSize); return outSize; } }

		public string Id { get; protected set; }
		public byte Quantity { get; protected set; }
		public ObtainableItem(string id)
		{
			Quantity = 1;
			Id = id;
		}

		public bool IsGone() { return Quantity <= 0; }
		public void DecrementQuantity(byte amount = 1)
		{
			Quantity -= amount;
		}
		public void IncrementQuantity(byte amount = 1)
		{
			Quantity += amount;
		}
		public override bool Equals(object obj)
		{
			ObtainableItem oi = obj as ObtainableItem;
			if (oi != null)
				return oi.Id == Id;
			return false;
		}
	}
}
