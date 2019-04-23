using System;
using System.Linq;
using System.Reflection;

namespace Wist.Core.ExtensionMethods
{
	public static class EnumExtensionMethods
	{
		public static string GetDescription(this Enum genericEnum)
		{
			Type genericEnumType = genericEnum.GetType();
			MemberInfo[] memberInfo = genericEnumType.GetMember(genericEnum.ToString());
			if ((memberInfo != null && memberInfo.Length > 0))
			{
				var attrs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
				if ((attrs != null && attrs.Count() > 0))
				{
					return ((System.ComponentModel.DescriptionAttribute)attrs.ElementAt(0)).Description;
				}
			}
			return genericEnum.ToString();
		}

	}
}
