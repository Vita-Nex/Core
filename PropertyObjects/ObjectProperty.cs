#region Header
//   Vorspire    _,-'/-'/  ObjectProperty.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
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
				return val is IConvertible && SetValue(o, unchecked((char)cur + Convert.ToChar(val)));
			}

			if (cur is sbyte)
			{
				return val is IConvertible && SetValue(o, unchecked((sbyte)cur + Convert.ToSByte(val)));
			}

			if (cur is byte)
			{
				return val is IConvertible && SetValue(o, unchecked((byte)cur + Convert.ToByte(val)));
			}

			if (cur is short)
			{
				return val is IConvertible && SetValue(o, unchecked((short)cur + Convert.ToInt16(val)));
			}

			if (cur is ushort)
			{
				return val is IConvertible && SetValue(o, unchecked((ushort)cur + Convert.ToUInt16(val)));
			}

			if (cur is int)
			{
				return val is IConvertible && SetValue(o, unchecked((int)cur + Convert.ToInt32(val)));
			}

			if (cur is uint)
			{
				return val is IConvertible && SetValue(o, unchecked((uint)cur + Convert.ToUInt32(val)));
			}

			if (cur is long)
			{
				return val is IConvertible && SetValue(o, unchecked((long)cur + Convert.ToInt64(val)));
			}

			if (cur is ulong)
			{
				return val is IConvertible && SetValue(o, unchecked((ulong)cur + Convert.ToUInt64(val)));
			}

			if (cur is float)
			{
				return val is IConvertible && SetValue(o, (float)cur + Convert.ToSingle(val));
			}

			if (cur is decimal)
			{
				return val is IConvertible && SetValue(o, (decimal)cur + Convert.ToDecimal(val));
			}

			if (cur is double)
			{
				return val is IConvertible && SetValue(o, (double)cur + Convert.ToDouble(val));
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
				return val is IConvertible && (!limit || (char)cur >= Convert.ToChar(val)) &&
					   SetValue(o, unchecked((char)cur - Convert.ToChar(val)));
			}

			if (cur is sbyte)
			{
				return val is IConvertible && (!limit || (sbyte)cur >= Convert.ToSByte(val)) &&
					   SetValue(o, unchecked((sbyte)cur - Convert.ToSByte(val)));
			}

			if (cur is byte)
			{
				return val is IConvertible && (!limit || (byte)cur >= Convert.ToByte(val)) &&
					   SetValue(o, unchecked((byte)cur - Convert.ToByte(val)));
			}

			if (cur is short)
			{
				return val is IConvertible && (!limit || (short)cur >= Convert.ToInt16(val)) &&
					   SetValue(o, unchecked((short)cur - Convert.ToInt16(val)));
			}

			if (cur is ushort)
			{
				return val is IConvertible && (!limit || (ushort)cur >= Convert.ToUInt16(val)) &&
					   SetValue(o, unchecked((ushort)cur - Convert.ToUInt16(val)));
			}

			if (cur is int)
			{
				return val is IConvertible && (!limit || (int)cur >= Convert.ToInt32(val)) &&
					   SetValue(o, unchecked((int)cur - Convert.ToInt32(val)));
			}

			if (cur is uint)
			{
				return val is IConvertible && (!limit || (uint)cur >= Convert.ToUInt32(val)) &&
					   SetValue(o, unchecked((uint)cur - Convert.ToUInt32(val)));
			}

			if (cur is long)
			{
				return val is IConvertible && (!limit || (long)cur >= Convert.ToInt64(val)) &&
					   SetValue(o, unchecked((long)cur - Convert.ToInt64(val)));
			}

			if (cur is ulong)
			{
				return val is IConvertible && (!limit || (ulong)cur >= Convert.ToUInt64(val)) &&
					   SetValue(o, unchecked((ulong)cur - Convert.ToUInt64(val)));
			}

			if (cur is float)
			{
				return val is IConvertible && (!limit || (float)cur >= Convert.ToSingle(val)) &&
					   SetValue(o, (float)cur - Convert.ToSingle(val));
			}

			if (cur is decimal)
			{
				return val is IConvertible && (!limit || (decimal)cur >= Convert.ToDecimal(val)) &&
					   SetValue(o, (decimal)cur - Convert.ToDecimal(val));
			}

			if (cur is double)
			{
				return val is IConvertible && (!limit || (double)cur >= Convert.ToDouble(val)) &&
					   SetValue(o, (double)cur - Convert.ToDouble(val));
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
					return (!limit || (DateTime)cur >= DateTime.MinValue + (TimeSpan)val) &&
						   SetValue(o, (DateTime)cur - (TimeSpan)val);
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

			if (cur is sbyte || cur is byte || cur is short || cur is ushort || cur is int || cur is uint || cur is long ||
				cur is ulong || cur is float || cur is decimal || cur is double)
			{
				return SetValue(o, 0);
			}

			if (cur is TimeSpan)
			{
				return SetValue(o, TimeSpan.Zero);
			}

			if (cur is DateTime)
			{
				return SetValue(o, DateTime.MinValue);
			}

			if (cur is char)
			{
				return SetValue(o, ' ');
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