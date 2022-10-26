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
		private const BindingFlags _CommonFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private static readonly Delegate[] _EmptyDelegates = new Delegate[0];

		public static Delegate[] GetEventDelegates(this object obj, string eventName)
		{
			var t = obj as Type ?? obj.GetType();

            var f = _CommonFlags;

            if (t.IsSealed && t.IsAbstract)
            {
                f &= ~BindingFlags.Instance;
            }

            var ei = t.GetEvent(eventName, f);

			if (ei == null)
			{
				return _EmptyDelegates;
			}

			var efi = t.GetField(ei.Name, f | BindingFlags.GetField);

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

            var f = _CommonFlags;

            if (t.IsSealed && t.IsAbstract)
            {
                f &= ~BindingFlags.Instance;
            }

            var o = t.GetField(name, f);

			try
			{
				value = (T)o.GetValue(obj is Type ? null : obj);
				return true;
			}
			catch
			{ }

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

            var f = _CommonFlags;

            if (t.IsSealed && t.IsAbstract)
            {
                f &= ~BindingFlags.Instance;
            }

            var o = t.GetField(name, f);

			try
			{
				o.SetValue(obj is Type ? null : obj, value);

				return true;
			}
			catch
			{ }

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

            var f = _CommonFlags;

            if (t.IsSealed && t.IsAbstract)
            {
                f &= ~BindingFlags.Instance;
            }

            var o = t.GetProperty(name, f, null, typeof(T), Type.EmptyTypes, null);

			try
			{
				value = (T)o.GetValue(obj is Type ? null : obj, null);

				return true;
			}
			catch
			{
				return false;
			}
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

            var f = _CommonFlags;

            if (t.IsSealed && t.IsAbstract)
            {
                f &= ~BindingFlags.Instance;
            }

            var o = t.GetProperty(name, f, null, typeof(T), Type.EmptyTypes, null);

			try
			{
				o.SetValue(obj is Type ? null : obj, value, null);

				return true;
			}
			catch
			{
				return false;
			}
		}

		public static object CallMethod(this object obj, string name, params object[] args)
		{
			if (obj == null || String.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			var t = obj as Type ?? obj.GetType();

            var f = _CommonFlags;

            if (t.IsSealed && t.IsAbstract)
            {
                f &= ~BindingFlags.Instance;
            }

			var a = args != null ? Type.GetTypeArray(args) : Type.EmptyTypes;

            var o = t.GetMethod(name, f, null, a, null);

			try
			{
				if (o.ReturnType == typeof(void))
				{
					return o.Invoke(obj is Type ? null : obj, args) ?? true;
				}

				return o.Invoke(obj is Type ? null : obj, args);
			}
			catch
			{
				return null;
			}
		}

		public static T CallMethod<T>(this object obj, string name, params object[] args)
		{
			obj = CallMethod(obj, name, args);

			return obj is T ? (T)obj : default(T);
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

		public static FieldList<T> GetFields<T>(this T obj, BindingFlags flags = BindingFlags.Default, Func<FieldInfo, bool> filter = null)
		{
			return new FieldList<T>(obj, flags, filter);
		}

		public static PropertyList<T> GetProperties<T>(this T obj, BindingFlags flags = BindingFlags.Default, Func<PropertyInfo, bool> filter = null)
		{
			return new PropertyList<T>(obj, flags, filter);
		}
	}
}