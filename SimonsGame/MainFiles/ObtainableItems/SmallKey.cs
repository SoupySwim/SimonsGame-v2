using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.MainFiles
{
	public class SmallKey : ObtainableItem
	{
		public static Dictionary<byte, Color> KeyColors = new Dictionary<byte, Color>() { { 0, Color.Gray }, { 1, Color.White }, { 2, Color.Black },
			{ 3, Color.Red }, { 4, Color.Green }, { 5, Color.Blue }, { 6, Color.Purple }, { 7, Color.Orange }, { 8, Color.Yellow }, { 9, Color.Pink } };
		public byte KeyType { get; private set; }
		public SmallKey(byte keyType)
			: base("SK_" + keyType)
		{
			KeyType = keyType;
		}
	}
}
