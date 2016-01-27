#region Header
//   Vorspire    _,-'/-'/  TypeExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Server;
#endregion

namespace System
{
	public static class TypeExtUtility
	{
		private static readonly Dictionary<Type, List<Type>> _ChildrenCache = new Dictionary<Type, List<Type>>(0x100);

		private static readonly Dictionary<Type, List<Type>> _ConstructableChildrenCache =
			new Dictionary<Type, List<Type>>(0x100);

		public static Type[] GetTypeCache(this Assembly asm)
		{
			return ScriptCompiler.GetTypeCache(asm).Types;
		}

		public static bool GetCustomAttributes<TAttribute>(this Type t, bool inherit, out TAttribute[] attrs)
			where TAttribute : Attribute
		{
			attrs = GetCustomAttributes<TAttribute>(t, inherit);
			return attrs != null && attrs.Length > 0;
		}

		public static TAttribute[] GetCustomAttributes<TAttribute>(this Type t, bool inherit) where TAttribute : Attribute
		{
			return t != null
				? t.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>().ToArray()
				: new TAttribute[0];
		}

		public static int CompareTo(this Type t, Type other)
		{
			var result = 0;

			if (t.CompareNull(other, ref result))
			{
				return result;
			}

			var lp = t.BaseType;

			while (lp != null)
			{
				if (lp == other)
				{
					return -1;
				}

				lp = lp.BaseType;
			}

			return 1;
		}

		public static bool IsEqual(this Type a, string bName)
		{
			return IsEqual(a, bName, true);
		}

		public static bool IsEqual(this Type a, string bName, bool ignoreCase)
		{
			return IsEqual(a, bName, ignoreCase, bName.ContainsAny('.', '+'));
		}

		public static bool IsEqual(this Type a, string bName, bool ignoreCase, bool fullName)
		{
			var b = Type.GetType(bName) ??
					(fullName ? ScriptCompiler.FindTypeByFullName(bName) : ScriptCompiler.FindTypeByName(bName));

			return IsEqual(a, b);
		}

		public static bool IsEqual(this Type a, Type b)
		{
			return a == b;
		}

		public static bool IsEqual<TObj>(this Type t)
		{
			return IsEqual(t, typeof(TObj));
		}

		public static bool IsEqualOrChildOf(this Type a, string bName)
		{
			return IsEqualOrChildOf(a, bName, true);
		}

		public static bool IsEqualOrChildOf(this Type a, string bName, bool ignoreCase)
		{
			return IsEqualOrChildOf(a, bName, ignoreCase, bName.ContainsAny('.', '+'));
		}

		public static bool IsEqualOrChildOf(this Type a, string bName, bool ignoreCase, bool fullName)
		{
			var b = Type.GetType(bName) ??
					(fullName ? ScriptCompiler.FindTypeByFullName(bName) : ScriptCompiler.FindTypeByName(bName));

			return IsEqualOrChildOf(a, b);
		}

		public static bool IsEqualOrChildOf(this Type a, Type b)
		{
			return IsEqual(a, b) || IsChildOf(a, b);
		}

		public static bool IsEqualOrChildOf<TObj>(this Type t)
		{
			return IsEqualOrChildOf(t, typeof(TObj));
		}

		public static bool IsChildOf(this Type a, Type b)
		{
			return a != null && b != null && a != b && !a.IsInterface && !a.IsEnum &&
				   (b.IsInterface ? HasInterface(a, b) : b.IsAssignableFrom(a));
		}

		public static bool IsChildOf<TObj>(this Type t)
		{
			return IsChildOf(t, typeof(TObj));
		}

		public static bool HasInterface<TObj>(this Type t)
		{
			return HasInterface(t, typeof(TObj));
		}

		public static bool HasInterface(this Type t, Type i)
		{
			return t != null && i != null && i.IsInterface && t.GetInterface(i.FullName) != null;
		}

		public static bool IsConstructable(this Type a)
		{
			return IsConstructable(a, Type.EmptyTypes);
		}

		public static bool IsConstructable(this Type a, Type[] argTypes)
		{
			if (a == null || a.IsAbstract || a.IsInterface || a.IsEnum)
			{
				return false;
			}

			return a.GetConstructor(argTypes) != null;
		}

		public static bool IsConstructableFrom(this Type a, Type b)
		{
			if (a == null || b == null || a.IsAbstract || !IsChildOf(a, b))
			{
				return false;
			}

			return a.GetConstructors().Length > 0;
		}

		public static bool IsConstructableFrom<TObj>(this Type t)
		{
			return IsConstructableFrom(t, typeof(TObj));
		}

		public static bool IsConstructableFrom(this Type a, Type b, Type[] argTypes)
		{
			if (a == null || b == null || a.IsAbstract || !IsChildOf(a, b))
			{
				return false;
			}

			return a.GetConstructor(argTypes) != null;
		}

		public static bool IsConstructableFrom<TObj>(this Type t, Type[] argTypes)
		{
			return IsConstructableFrom(t, typeof(TObj), argTypes);
		}

		public static Type[] GetChildren(this Type type, Func<Type, bool> predicate = null)
		{
			return FindChildren(type, predicate).ToArray();
		}

		public static IEnumerable<Type> FindChildren(this Type type, Func<Type, bool> predicate = null)
		{
			if (type == null)
			{
				return Type.EmptyTypes;
			}

			var types = _ChildrenCache.GetValue(type);

			if (types == null)
			{
				var asm = ScriptCompiler.Assemblies.With(Core.Assembly, Assembly.GetCallingAssembly()).ToList();

				asm.Prune();

				types = asm.Select(GetTypeCache).SelectMany(o => o.Where(t => !IsEqual(t, type) && IsChildOf(t, type))).ToList();

				asm.Free(true);

				if (types.Count > 0 && types.Count <= 0x100)
				{
					_ChildrenCache.Add(type, types);
				}
			}

			if (_ChildrenCache.Count >= 0x100)
			{
				_ChildrenCache.Pop().Value.Free(true);
			}

			return predicate != null ? types.Where(predicate) : types.AsEnumerable();
		}

		public static Type[] GetConstructableChildren(this Type type, Func<Type, bool> predicate = null)
		{
			return FindConstructableChildren(type, predicate).ToArray();
		}

		public static IEnumerable<Type> FindConstructableChildren(this Type type, Func<Type, bool> predicate = null)
		{
			if (type == null)
			{
				return Type.EmptyTypes;
			}

			var types = _ConstructableChildrenCache.GetValue(type);

			if (types == null)
			{
				types = FindChildren(type).Where(t => IsConstructableFrom(t, type)).ToList();

				if (types.Count > 0 && types.Count <= 0x100)
				{
					_ConstructableChildrenCache.Add(type, types);
				}
			}

			if (_ConstructableChildrenCache.Count >= 0x100)
			{
				_ConstructableChildrenCache.Pop().Value.Free(true);
			}

			return predicate != null ? types.Where(predicate) : types.AsEnumerable();
		}

		public static TObj CreateInstance<TObj>(this Type t, params object[] args)
		{
			if (t == null || t.IsAbstract || t.IsInterface || t.IsEnum)
			{
				return default(TObj);
			}

			if (args == null || args.Length == 0)
			{
				return (TObj)Activator.CreateInstance(t, true);
			}

			return (TObj)Activator.CreateInstance(t, args);
		}

		public static TObj CreateInstanceSafe<TObj>(this Type t, params object[] args)
		{
			try
			{
				return CreateInstance<TObj>(t, args);
			}
			catch
			{
				return default(TObj);
			}
		}

		public static object CreateInstance(this Type t, params object[] args)
		{
			return CreateInstanceSafe<object>(t, args);
		}
	}
}