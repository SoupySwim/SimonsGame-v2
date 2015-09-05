using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonsGame.Utility
{
	public enum Element
	{
		Normal = -1,

		Plant = 0,
		Water = 1,
		Fire = 2,

		Lightning = 3,
		Metal = 4,
		Rock = 5,
	}

	public static class ElementHelper
	{
		public static Element GetRelatedElement(this Element element)
		{
			if (element == Element.Normal)
				return Element.Normal;
			return (Element)(((int)element + 3) % 6);
		}

		public static Element GetCounterElement(this Element element)
		{
			if (element == Element.Normal)
				return Element.Normal;
			int elementNumber = (int)element;
			return (Element)(((elementNumber % 3) + 2) % 3) + (elementNumber / 3 >= 1 ? 3 : 0);
		}

		public static Element GetSoftCounterElement(this Element element)
		{
			return GetCounterElement(GetRelatedElement(element));
		}
	}
}
