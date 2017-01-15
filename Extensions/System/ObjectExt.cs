#region Header
//   Vorspire    _,-'/-'/  ObjectExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Reflection;

using VitaNex;
using VitaNex.Reflection;
#endregion

namespace System
{
	public static class ObjectExtUtility
	{
		public static int GetTypeHashCode(this object obj)
		{
			if (obj == null)
			{
				return 0;
			}

			return obj.GetType().GetValueHashCode();
		}

		public static bool TypeEquals<T>(this object obj)
		{
			return TypeEquals<T>(obj, true);
		}

		public static bool TypeEquals<T>(this object obj, bool child)
		{
			return TypeEquals(obj, typeof(T), child);
		}

		public static bool TypeEquals(this object obj, object other)
		{
			return TypeEquals(obj, other, true);
		}

		public static bool TypeEquals(this object obj, object other, bool child)
		{
			if (obj == null || other == null)
			{
				return false;
			}

			if (ReferenceEquals(obj, other))
			{
				return true;
			}

			Type l, r;

			if (obj is ITypeSelectProperty)
			{
				l = ((ITypeSelectProperty)obj).InternalType;
			}
			else
			{
				l = obj as Type ?? obj.GetType();
			}

			if (other is ITypeSelectProperty)
			{
				r = ((ITypeSelectProperty)other).InternalType;
			}
			else
			{
				r = other as Type ?? other.GetType();
			}

			return child ? l.IsEqualOrChildOf(r) : l.IsEqual(r);
		}

		public static T IfNull<T>(this T obj, Func<T> resolve)
		{
			if (IsNull(obj) && !IsNull(resolve))
			{
				return resolve();
			}

			return obj;
		}

		public static void IfNull<T>(this T obj, Action action)
		{
			if (IsNull(obj) && !IsNull(action))
			{
				action();
			}
		}

		public static D IfNull<T, D>(this T obj, Func<D> resolve, D def)
		{
			if (IsNull(obj) && !IsNull(resolve))
			{
				return resolve();
			}

			return def;
		}

		public static T IfNotNull<T>(this T obj, Func<T, T> resolve)
		{
			if (!IsNull(obj) && !IsNull(resolve))
			{
				return resolve(obj);
			}

			return obj;
		}

		public static void IfNotNull<T>(this T obj, Action<T> action)
		{
			if (!IsNull(obj) && !IsNull(action))
			{
				action(obj);
			}
		}

		public static D IfNotNull<T, D>(this T obj, Func<T, D> resolve, D def)
		{
			if (!IsNull(obj) && !IsNull(resolve))
			{
				return resolve(obj);
			}

			return def;
		}

		public static bool IsNull<T>(this T obj)
		{
			return obj == null;
		}

		public static bool CheckNull<T>(this T obj, T other)
		{
			var result = 0;

			return CompareNull(obj, other, ref result);
		}

		public static int CompareNull<T>(this T obj, T other)
		{
			var result = 0;

			CompareNull(obj, other, ref result);

			return result;
		}

		public static bool CompareNull<T>(this T obj, T other, ref int result)
		{
			if (obj == null && other == null)
			{
				return true;
			}

			if (obj == null)
			{
				++result;
				return true;
			}

			if (other == null)
			{
				--result;
				return true;
			}

			return false;
		}
		
		public static FieldList<T> GetFields<T>(
			this T obj,
			BindingFlags flags = BindingFlags.Default,
			Func<FieldInfo, bool> filter = null)
		{
			return new FieldList<T>(obj, flags, filter);
		}

		public static PropertyList<T> GetProperties<T>(
			this T obj,
			BindingFlags flags = BindingFlags.Default,
			Func<PropertyInfo, bool> filter = null)
		{
			return new PropertyList<T>(obj, flags, filter);
		}
	}
}