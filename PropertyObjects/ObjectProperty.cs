#region Header
//   Vorspire    _,-'/-'/  ObjectProperty.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Reflection;

using Server;
#endregion

namespace VitaNex
{
	public sealed class ObjectProperty : PropertyObject
	{
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public string Name { get; set; }

		public Func<object, object> GetHandler { get; set; }
		public Action<object, object> SetHandler { get; set; }

		public ObjectProperty()
			: this(String.Empty)
		{ }

		public ObjectProperty(string name)
		{
			Name = name ?? String.Empty;
		}

		public ObjectProperty(GenericReader reader)
			: base(reader)
		{ }

		public override void Reset()
		{
			Name = String.Empty;
		}

		public override void Clear()
		{
			Name = String.Empty;
		}

		public override string ToString()
		{
			return Name;
		}

		public bool IsSupported(object o)
		{
			return IsSupported(o, null);
		}

		public bool IsSupported(object o, object a)
		{
			Type t = null, v = null;

			if (o is Type)
			{
				t = (Type)o;
			}
			else if (o != null)
			{
				t = o.GetType();
			}

			if (a is Type)
			{
				v = (Type)a;
			}
			else if (a != null)
			{
				v = a.GetType();
			}

			return IsSupported(t, v);
		}

		public bool IsSupported<TObj>()
		{
			return IsSupported(typeof(TObj));
		}

		public bool IsSupported<TObj, TVal>()
		{
			return IsSupported(typeof(TObj), typeof(TVal));
		}

		public bool IsSupported(Type t)
		{
			return IsSupported(t, null);
		}

		public bool IsSupported(Type t, Type v)
		{
			PropertyInfo p;
			return t != null && (p = t.GetProperty(Name)) != null && (v != null && p.PropertyType == v);
		}

		public object GetValue(object o, object def = null)
		{
			if (String.IsNullOrWhiteSpace(Name) || o == null)
			{
				return def;
			}

			try
			{
				if (GetHandler != null)
				{
					return GetHandler(o);
				}
			}
			catch
			{ }

			try
			{
				var t = o as Type ?? o.GetType();
				var f = o is Type ? BindingFlags.Static : BindingFlags.Instance;

				var p = t.GetProperty(Name, f | BindingFlags.Public);

				return p.GetValue(o is Type ? null : o, null);
			}
			catch
			{
				return def;
			}
		}

		public bool SetValue(object o, object val)
		{
			if (String.IsNullOrWhiteSpace(Name) || o == null)
			{
				return false;
			}

			try
			{
				if (SetHandler != null)
				{
					SetHandler(o, val);
					return true;
				}
			}
			catch
			{ }

			try
			{
				var t = o as Type ?? o.GetType();
				var f = o is Type ? BindingFlags.Static : BindingFlags.Instance;

				var p = t.GetProperty(Name, f | BindingFlags.Public);

				p.SetValue(o is Type ? null : o, val, null);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool Add(object o, object val)
		{
			var cur = GetValue(o);

			if (cur is char)
			{
				return val is char && SetValue(o, unchecked((char)cur + (char)val));
			}

			if (cur is sbyte)
			{
				return val is sbyte && SetValue(o, unchecked((sbyte)cur + (sbyte)val));
			}

			if (cur is byte)
			{
				return val is byte && SetValue(o, unchecked((byte)cur + (byte)val));
			}

			if (cur is short)
			{
				return val is short && SetValue(o, unchecked((short)cur + (short)val));
			}

			if (cur is ushort)
			{
				return val is ushort && SetValue(o, unchecked((ushort)cur + (ushort)val));
			}

			if (cur is int)
			{
				return val is int && SetValue(o, unchecked((int)cur + (int)val));
			}

			if (cur is uint)
			{
				return val is uint && SetValue(o, unchecked((uint)cur + (uint)val));
			}

			if (cur is long)
			{
				return val is long && SetValue(o, unchecked((long)cur + (long)val));
			}

			if (cur is ulong)
			{
				return val is ulong && SetValue(o, unchecked((ulong)cur + (ulong)val));
			}

			if (cur is float)
			{
				return val is float && SetValue(o, (float)cur + (float)val);
			}

			if (cur is decimal)
			{
				return val is decimal && SetValue(o, (decimal)cur + (decimal)val);
			}

			if (cur is double)
			{
				return val is double && SetValue(o, (double)cur + (double)val);
			}

			if (cur is TimeSpan)
			{
				return val is TimeSpan && SetValue(o, (TimeSpan)cur + (TimeSpan)val);
			}

			if (cur is DateTime)
			{
				return val is TimeSpan && SetValue(o, (DateTime)cur + (TimeSpan)val);
			}

			return false;
		}

		public bool Subtract(object o, object val)
		{
			return Subtract(o, val, false);
		}

		public bool Subtract(object o, object val, bool limit)
		{
			var cur = GetValue(o);

			if (cur is char)
			{
				return val is char && (!limit || (char)cur >= (char)val) && SetValue(o, unchecked((char)cur - (char)val));
			}

			if (cur is sbyte)
			{
				return val is sbyte && (!limit || (sbyte)cur >= (sbyte)val) && SetValue(o, unchecked((sbyte)cur - (sbyte)val));
			}

			if (cur is byte)
			{
				return val is byte && (!limit || (byte)cur >= (byte)val) && SetValue(o, unchecked((byte)cur - (byte)val));
			}

			if (cur is short)
			{
				return val is short && (!limit || (short)cur >= (short)val) && SetValue(o, unchecked((short)cur - (short)val));
			}

			if (cur is ushort)
			{
				return val is ushort && (!limit || (ushort)cur >= (ushort)val) && SetValue(o, unchecked((ushort)cur - (ushort)val));
			}

			if (cur is int)
			{
				return val is int && (!limit || (int)cur >= (int)val) && SetValue(o, unchecked((int)cur - (int)val));
			}

			if (cur is uint)
			{
				return val is uint && (!limit || (uint)cur >= (uint)val) && SetValue(o, unchecked((uint)cur - (uint)val));
			}

			if (cur is long)
			{
				return val is long && (!limit || (long)cur >= (long)val) && SetValue(o, unchecked((long)cur - (long)val));
			}

			if (cur is ulong)
			{
				return val is ulong && (!limit || (ulong)cur >= (ulong)val) && SetValue(o, unchecked((ulong)cur - (ulong)val));
			}

			if (cur is float)
			{
				return val is float && (!limit || (float)cur >= (float)val) && SetValue(o, (float)cur - (float)val);
			}

			if (cur is decimal)
			{
				return val is decimal && (!limit || (decimal)cur >= (decimal)val) && SetValue(o, (decimal)cur - (decimal)val);
			}

			if (cur is double)
			{
				return val is double && (!limit || (double)cur >= (double)val) && SetValue(o, (double)cur - (double)val);
			}

			if (cur is TimeSpan)
			{
				if (val is TimeSpan)
				{
					return (!limit || (TimeSpan)cur >= (TimeSpan)val) && SetValue(o, (TimeSpan)cur - (TimeSpan)val);
				}

				return false;
			}

			if (cur is DateTime)
			{
				if (val is TimeSpan)
				{
					return (!limit || (DateTime)cur >= DateTime.MinValue + (TimeSpan)val) && SetValue(o, (DateTime)cur - (TimeSpan)val);
				}

				return false;
			}

			return false;
		}

		public bool Consume(object o, object val)
		{
			return Subtract(o, val, true);
		}

		public bool SetDefault(object o)
		{
			var cur = GetValue(o);

			if (cur is char)
			{
				return SetValue(o, ' ');
			}

			if (cur is sbyte)
			{
				return SetValue(o, (sbyte)0);
			}

			if (cur is byte)
			{
				return SetValue(o, (byte)0);
			}

			if (cur is short)
			{
				return SetValue(o, (short)0);
			}

			if (cur is ushort)
			{
				return SetValue(o, (ushort)0);
			}

			if (cur is int)
			{
				return SetValue(o, 0);
			}

			if (cur is uint)
			{
				return SetValue(o, (uint)0);
			}

			if (cur is long)
			{
				return SetValue(o, (long)0);
			}

			if (cur is ulong)
			{
				return SetValue(o, (ulong)0);
			}

			if (cur is float)
			{
				return SetValue(o, (float)0);
			}

			if (cur is decimal)
			{
				return SetValue(o, (decimal)0);
			}

			if (cur is double)
			{
				return SetValue(o, (double)0);
			}

			if (cur is TimeSpan)
			{
				return SetValue(o, TimeSpan.Zero);
			}

			if (cur is DateTime)
			{
				return SetValue(o, DateTime.MinValue);
			}

			if (cur is string)
			{
				return SetValue(o, String.Empty);
			}

			return SetValue(o, null);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(Name);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			Name = reader.ReadString();
		}
	}
}