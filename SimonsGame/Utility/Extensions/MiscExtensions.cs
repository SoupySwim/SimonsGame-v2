using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Extensions
{
	public static class MiscExtensions
	{
		public static T GetNextEnum<T>(object currentlySelectedEnum)
		{
			Type enumList = typeof(T);
			if (!enumList.IsEnum)
				throw new InvalidOperationException("Object is not an Enum.");

			Array enums = Enum.GetValues(enumList);
			int index = Array.IndexOf(enums, currentlySelectedEnum);
			index = (index + 1) % enums.Length;
			return (T)enums.GetValue(index);
		}
		public static T GetPreviousEnum<T>(object currentlySelectedEnum)
		{
			Type enumList = typeof(T);
			if (!enumList.IsEnum)
				throw new InvalidOperationException("Object is not an Enum.");

			Array enums = Enum.GetValues(enumList);
			int index = Array.IndexOf(enums, currentlySelectedEnum);
			index = (index + enums.Length - 1) % enums.Length;
			return (T)enums.GetValue(index);
		}
		public static T ToEnum<T>(this string value)
		{
			return (T)Enum.Parse(typeof(T), value, true);
		}
	}
}
