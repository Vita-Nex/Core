#region Header
//   Vorspire    _,-'/-'/  EnumExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
using System.Linq;

using Server;
#endregion

namespace System
{
	public static class EnumExtUtility
	{
		public static string ToString(this Enum e, bool friendly)
		{
			var s = e.ToString();

			return friendly ? s.SpaceWords() : s;
		}

		public static string GetDescription(this Enum e)
		{
			var type = e.GetType();
			var info = type.GetMember(e.ToString());

			if (info.IsNullOrEmpty() || info[0] == null)
			{
				return String.Empty;
			}

			var attributes = info[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes.IsNullOrEmpty())
			{
				return String.Empty;
			}

			var desc = attributes.OfType<DescriptionAttribute>().Where(o => !String.IsNullOrWhiteSpace(o.Description));

			return String.Join("\n", desc);
		}

		public static TEnum Normalize<TEnum>(this Enum e)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var type = typeof(TEnum);
			var flag = default(TEnum);

			if (!type.IsEnum)
			{
				return flag;
			}

			if (!Enum.TryParse(e.ToString(), out flag) ||
				(!type.HasCustomAttribute<FlagsAttribute>(true) && !Enum.IsDefined(type, flag)))
			{
				flag = default(TEnum);
			}

			return flag;
		}

		public static bool IsValid(this Enum e)
		{
			return Enum.IsDefined(e.GetType(), e);
		}

		public static TCast[] Split<TCast>(this Enum e)
		{
			return GetValues<TCast>(e, true);
		}

		public static TCast[] GetAbsoluteValues<TCast>(this Enum e)
		{
			return EnumerateValues<TCast>(e, false).ToArray();
		}

		public static TCast[] GetAbsoluteValues<TCast>(this Enum e, bool local)
		{
			return EnumerateValues<TCast>(e, local).Where(o => o != null && !Equals(o, 0) && !Equals(o, String.Empty)).ToArray();
		}

		public static TCast[] GetValues<TCast>(this Enum e)
		{
			return EnumerateValues<TCast>(e, false).ToArray();
		}

		public static TCast[] GetValues<TCast>(this Enum e, bool local)
		{
			return EnumerateValues<TCast>(e, local).ToArray();
		}

		public static IEnumerable<TCast> EnumerateValues<TCast>(this Enum e, bool local)
		{
			var eType = e.GetType();
			var vType = typeof(TCast);
			var vals = Enum.GetValues(eType).Cast<Enum>();

			if (local)
			{
				vals = vals.Where(e.HasFlag);
			}

			if (vType.IsEqual(typeof(char)))
			{
				return vals.Select(Convert.ToChar).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(sbyte)))
			{
				return vals.Select(Convert.ToSByte).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(byte)))
			{
				return vals.Select(Convert.ToByte).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(short)))
			{
				return vals.Select(Convert.ToInt16).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(ushort)))
			{
				return vals.Select(Convert.ToUInt16).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(int)))
			{
				return vals.Select(Convert.ToInt32).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(uint)))
			{
				return vals.Select(Convert.ToUInt32).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(long)))
			{
				return vals.Select(Convert.ToInt64).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(ulong)))
			{
				return vals.Select(Convert.ToUInt64).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(string)))
			{
				return vals.Select(Convert.ToString).Cast<TCast>();
			}

			return vals.Cast<TCast>();
		}

		public static bool AnyFlags<TEnum>(this Enum e, IEnumerable<TEnum> flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!e.GetType().HasCustomAttribute<FlagsAttribute>(true))
			{
				return flags != null && flags.Any(o => Equals(e, o));
			}

			return flags != null && flags.Cast<Enum>().Any(e.HasFlag);
		}

		public static bool AnyFlags<TEnum>(this Enum e, params TEnum[] flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!e.GetType().HasCustomAttribute<FlagsAttribute>(true))
			{
				return flags != null && flags.Any(o => Equals(e, o));
			}

			return flags != null && flags.Cast<Enum>().Any(e.HasFlag);
		}

		public static bool AllFlags<TEnum>(this Enum e, IEnumerable<TEnum> flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!e.GetType().HasCustomAttribute<FlagsAttribute>(true))
			{
				return flags != null && flags.All(o => Equals(e, o));
			}

			return flags != null && flags.Cast<Enum>().All(e.HasFlag);
		}

		public static bool AllFlags<TEnum>(this Enum e, params TEnum[] flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!e.GetType().HasCustomAttribute<FlagsAttribute>(true))
			{
				return flags != null && flags.All(o => Equals(e, o));
			}

			return flags != null && flags.Cast<Enum>().All(e.HasFlag);
		}
	}
}