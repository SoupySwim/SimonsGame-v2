using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects.Utility
{
	public class DoubleVector2
	{
		public static DoubleVector2 Zero { get { return new DoubleVector2(); } }
		public double X { get; set; }
		public double Y { get; set; }
		public DoubleVector2()
		{
			X = 0;
			Y = 0;
		}
		public DoubleVector2(double x, double y)
		{
			X = x;
			Y = y;
		}
		public static bool operator ==(DoubleVector2 a, DoubleVector2 b)
		{
			return a.X == b.X && a.Y == b.Y;
		}
		public static bool operator !=(DoubleVector2 a, DoubleVector2 b)
		{
			return !(a == b);
		}
		public override int GetHashCode() { return base.GetHashCode(); }
		public override bool Equals(object obj)
		{
			if (obj != null && obj.GetType().IsAssignableFrom(typeof(DoubleVector2)))
			{
				DoubleVector2 db2 = obj as DoubleVector2;
				return this == db2;
			}
			return false;
		}
	}
}
