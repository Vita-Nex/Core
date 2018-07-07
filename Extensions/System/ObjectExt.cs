#region Header
//   Vorspire    _,-'/-'/  ObjectExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Linq;
using System.Reflection;

using VitaNex;
using VitaNex.Reflection;
#endregion

namespace System
{
	public static class ObjectExtUtility
	{
		private static readonly Delegate[] _EmptyDelegates = new Delegate[0];

		public static Delegate[] GetEventDelegates(this object obj, string eventName)
		{
			var type = obj as Type ?? obj.GetType();

			var bind = BindingFlags.Public;

			if (type.IsSealed && type.IsAbstract)
			{
				bind |= BindingFlags.Static;
			}
			else
			{
				bind |= BindingFlags.Instance;
			}

			var ei = type.GetEvent(eventName, bind);

			if (ei == null)
			{
				return _EmptyDelegates;
			}

			bind &= ~BindingFlags.Public;
			bind |= BindingFlags.GetField;

			var efi = type.GetField(ei.Name, bind);

			if (efi == null)
			{
				return _EmptyDelegates;
			}

			var efv = (Delegate)efi.GetValue(obj is Type ? null : obj);

			return efv.GetInvocationList();
		}

		public static MethodInfo[] GetEventMethods(this object obj, string eventName)
		{
			return GetEventDelegates(obj, eventName).Select(e => e.Method).ToArray();
		}

		public static bool GetFieldValue(this object obj, string name, out object value)
		{
			return GetFieldValue<object>(obj, name, out value);
		}

		public static bool GetFieldValue<T>(this object obj, string name, out T value)
		{
			value = default(T);

			if (obj == null || String.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			var t = obj as Type ?? obj.GetType();

			var f = BindingFlags.NonPublic;

			if (t.IsSealed && t.IsAbstract)
			{
				f |= BindingFlags.Static;
			}
			else
			{
				f |= BindingFlags.Instance;
			}

			var p = t.GetFields(f);

			foreach (var o in p.Where(o => o.Name == name))
			{
				try
				{
					value = (T)o.GetValue(obj);
					return true;
				}
				catch
				{ }
			}

			return false;
		}

		public static bool SetFieldValue(this object obj, string name, object value)
		{
			return SetFieldValue<object>(obj, name, value);
		}

		public static bool SetFieldValue<T>(this object obj, string name, T value)
		{
			if (obj == null || String.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			var t = obj as Type ?? obj.GetType();

			var f = BindingFlags.NonPublic;

			if (t.IsSealed && t.IsAbstract)
			{
				f |= BindingFlags.Static;
			}
			else
			{
				f |= BindingFlags.Instance;
			}

			var p = t.GetFields(f);

			foreach (var o in p.Where(o => o.Name == name))
			{
				try
				{
					o.SetValue(obj, value);
					return true;
				}
				catch
				{ }
			}

			return false;
		}

		public static bool GetPropertyValue(this object obj, string name, out object value)
		{
			return GetPropertyValue<object>(obj, name, out value);
		}

		public static bool GetPropertyValue<T>(this object obj, string name, out T value)
		{
			value = default(T);

			if (obj == null || String.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			var t = obj as Type ?? obj.GetType();

			var f = BindingFlags.Public;

			if (t.IsSealed && t.IsAbstract)
			{
				f |= BindingFlags.Static;
			}
			else
			{
				f |= BindingFlags.Instance;
			}

			var p = t.GetProperties(f);

			foreach (var o in p.Where(o => o.Name == name).OrderBy(o => o.CanRead))
			{
				try
				{
					value = (T)o.GetValue(obj, null);
					return true;
				}
				catch
				{ }
			}

			return false;
		}

		public static bool SetPropertyValue(this object obj, string name, object value)
		{
			return SetPropertyValue<object>(obj, name, value);
		}

		public static bool SetPropertyValue<T>(this object obj, string name, T value)
		{
			if (obj == null || String.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			var t = obj as Type ?? obj.GetType();

			var f = BindingFlags.Public;

			if (t.IsSealed && t.IsAbstract)
			{
				f |= BindingFlags.Static;
			}
			else
			{
				f |= BindingFlags.Instance;
			}

			var p = t.GetProperties(f);

			foreach (var o in p.Where(o => o.Name == name).OrderBy(o => o.CanWrite))
			{
				try
				{
					o.SetValue(obj, value, null);
					return true;
				}
				catch
				{ }
			}

			return false;
		}

		public static object CallMethod(this object obj, string name, params object[] args)
		{
			if (obj == null || String.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			var t = obj as Type ?? obj.GetType();

			var f = BindingFlags.Public | BindingFlags.NonPublic;

			if (t.IsSealed && t.IsAbstract)
			{
				f |= BindingFlags.Static;
			}
			else
			{
				f |= BindingFlags.Instance;
			}

			var m = t.GetMethods(f);

			foreach (var o in m.Where(o => o.Name == name).OrderBy(o => o.IsPublic))
			{
				try
				{
					return o.Invoke(obj, args);
				}
				catch
				{ }
			}

			return null;
		}

		public static T CallMethod<T>(this object obj, string name, params object[] args)
		{
			return (T)CallMethod(obj, name, args);
		}

		public static int GetTypeHashCode(this object obj)
		{
			if (obj == null)
			{
				return 0;
			}

			return obj.GetType().GetValueHashCode();
		}

		public static string GetTypeName(this object obj, bool raw)
		{
			Type t;

			if (obj is Type)
			{
				t = (Type)obj;
			}
			else if (obj is ITypeSelectProperty)
			{
				t = ((ITypeSelectProperty)obj).ExpectedType;
			}
			else
			{
				t = obj.GetType();
			}

			return raw ? t.Name : t.ResolveName();
		}

		public static bool Is<T>(this object obj)
		{
			return TypeEquals<T>(obj);
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

		public static bool IsNull<T>(this T obj)
			where T : class
		{
			return ReferenceEquals(obj, null);
		}

		public static T IfNull<T>(this T obj, Func<T> resolve)
			where T : class
		{
			if (IsNull(obj) && !IsNull(resolve))
			{
				return resolve();
			}

			return obj;
		}

		public static void IfNull<T>(this T obj, Action action)
			where T : class
		{
			if (IsNull(obj) && !IsNull(action))
			{
				action();
			}
		}

		public static D IfNull<T, D>(this T obj, Func<D> resolve, D def)
			where T : class
		{
			if (IsNull(obj) && !IsNull(resolve))
			{
				return resolve();
			}

			return def;
		}

		public static T IfNotNull<T>(this T obj, Func<T, T> resolve)
			where T : class
		{
			if (!IsNull(obj) && !IsNull(resolve))
			{
				return resolve(obj);
			}

			return obj;
		}

		public static void IfNotNull<T>(this T obj, Action<T> action)
			where T : class
		{
			if (!IsNull(obj) && !IsNull(action))
			{
				action(obj);
			}
		}

		public static D IfNotNull<T, D>(this T obj, Func<T, D> resolve, D def)
			where T : class
		{
			if (!IsNull(obj) && !IsNull(resolve))
			{
				return resolve(obj);
			}

			return def;
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