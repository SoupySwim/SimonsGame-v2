using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.GuiObjects.Utility
{
	public class DoubleVector2 : IComparable
	{
		private static DoubleVector2 _zero = new DoubleVector2();
		public static DoubleVector2 Zero { get { return _zero; } }
		public double X;
		public double Y;
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
			return CompareTo(obj) == 0;
		}
		public int CompareTo(object obj)
		{
			if (obj != null && (obj is DoubleVector2 || obj.GetType().IsAssignableFrom(typeof(DoubleVector2))))
			{
				DoubleVector2 db2 = obj as DoubleVector2;
				double difference = Math.Sqrt(this.X * this.X + this.Y * this.Y) - Math.Sqrt(db2.X * db2.X + db2.Y * db2.Y);
				return (int)(difference < 0 ? Math.Floor(difference) : Math.Ceiling(difference));
			}
			return 1; // If the paramter isn't a Vector2, then this is greater than it.
		}
	}
}
