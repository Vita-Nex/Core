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
			return (p = t.GetProperty(Name)) != null && (v != null && p.PropertyType == v);
		}

		public object GetValue(object o, object def = null)
		{
			if (String.IsNullOrWhiteSpace(Name) || o == null)
			{
				return def;
			}

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
				return SetValue(o, unchecked((char)cur + (char)val));
			}

			if (cur is sbyte)
			{
				return SetValue(o, unchecked((sbyte)cur + (sbyte)val));
			}

			if (cur is byte)
			{
				return SetValue(o, unchecked((byte)cur + (byte)val));
			}

			if (cur is short)
			{
				return SetValue(o, unchecked((short)cur + (short)val));
			}

			if (cur is ushort)
			{
				return SetValue(o, unchecked((ushort)cur + (ushort)val));
			}

			if (cur is int)
			{
				return SetValue(o, unchecked((int)cur + (int)val));
			}

			if (cur is uint)
			{
				return SetValue(o, unchecked((uint)cur + (uint)val));
			}

			if (cur is long)
			{
				return SetValue(o, unchecked((long)cur + (long)val));
			}

			if (cur is ulong)
			{
				return SetValue(o, unchecked((ulong)cur + (ulong)val));
			}

			if (cur is float)
			{
				return SetValue(o, unchecked((float)cur + (float)val));
			}

			if (cur is decimal)
			{
				return SetValue(o, unchecked((decimal)cur + (decimal)val));
			}

			if (cur is double)
			{
				return SetValue(o, unchecked((double)cur + (double)val));
			}

			if (cur is TimeSpan)
			{
				return SetValue(o, unchecked((TimeSpan)cur + (TimeSpan)val));
			}

			if (cur is DateTime)
			{
				return SetValue(o, unchecked((DateTime)cur + (TimeSpan)val));
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
				return (!limit || (char)cur >= (char)val) && SetValue(o, unchecked((char)cur - (char)val));
			}

			if (cur is sbyte)
			{
				return (!limit || (sbyte)cur >= (sbyte)val) && SetValue(o, unchecked((sbyte)cur - (sbyte)val));
			}

			if (cur is byte)
			{
				return (!limit || (byte)cur >= (byte)val) && SetValue(o, unchecked((byte)cur - (byte)val));
			}

			if (cur is short)
			{
				return (!limit || (short)cur >= (short)val) && SetValue(o, unchecked((short)cur - (short)val));
			}

			if (cur is ushort)
			{
				return (!limit || (ushort)cur >= (ushort)val) && SetValue(o, unchecked((ushort)cur - (ushort)val));
			}

			if (cur is int)
			{
				return (!limit || (int)cur >= (int)val) && SetValue(o, unchecked((int)cur - (int)val));
			}

			if (cur is uint)
			{
				return (!limit || (uint)cur >= (uint)val) && SetValue(o, unchecked((uint)cur - (uint)val));
			}

			if (cur is long)
			{
				return (!limit || (long)cur >= (long)val) && SetValue(o, unchecked((long)cur - (long)val));
			}

			if (cur is ulong)
			{
				return (!limit || (ulong)cur >= (ulong)val) && SetValue(o, unchecked((ulong)cur - (ulong)val));
			}

			if (cur is float)
			{
				return (!limit || (float)cur >= (float)val) && SetValue(o, unchecked((float)cur - (float)val));
			}

			if (cur is decimal)
			{
				return (!limit || (decimal)cur >= (decimal)val) && SetValue(o, unchecked((decimal)cur - (decimal)val));
			}

			if (cur is double)
			{
				return (!limit || (double)cur >= (double)val) && SetValue(o, unchecked((double)cur - (double)val));
			}

			if (cur is TimeSpan)
			{
				return (!limit || (TimeSpan)cur >= (TimeSpan)val) && SetValue(o, unchecked((TimeSpan)cur - (TimeSpan)val));
			}

			if (cur is DateTime)
			{
				return (!limit || (DateTime)cur >= (DateTime)val) && SetValue(o, unchecked((DateTime)cur - (TimeSpan)val));
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