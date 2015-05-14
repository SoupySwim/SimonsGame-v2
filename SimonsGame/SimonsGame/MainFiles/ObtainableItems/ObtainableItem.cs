using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.MainFiles
{
	public abstract class ObtainableItem
	{
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
