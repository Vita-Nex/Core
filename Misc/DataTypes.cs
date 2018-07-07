#region Header
//   Vorspire    _,-'/-'/  DataTypes.cs
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
using System.Globalization;
using System.Linq;

using Server;
#endregion

namespace System
{
	public enum DataType
	{
		Null = 0,
		Bool,
		Char,
		Byte,
		SByte,
		Short,
		UShort,
		Int,
		UInt,
		Long,
		ULong,
		Float,
		Decimal,
		Double,
		String,
		DateTime,
		TimeSpan
	}

	public struct SimpleType
	{
		public static SimpleType Null { get { return new SimpleType((object)null); } }

		public DataType Flag { get; private set; }
		public Type Type { get; private set; }
		public object Value { get; private set; }

		public bool HasValue { get { return Value != null; } }

		public SimpleType(object obj)
			: this()
		{
			Value = obj;
			Type = obj != null ? obj.GetType() : null;
			Flag = DataTypes.Lookup(Type);

			//Console.WriteLine("SimpleType: {0} ({1}) [{2}]", Value, Type, Flag);

			if (Flag == DataType.Null)
			{
				Value = null;
			}
		}

		public SimpleType(GenericReader reader)
			: this()
		{
			Deserialize(reader);
		}

		public T Cast<T>()
		{
			T value;

			return TryCast(out value) ? value : default(T);
		}

		public bool TryCast<T>(out T value)
		{
			if (this is T)
			{
				value = (T)(object)this;
				return true;
			}

			if (Value is T)
			{
				value = (T)Value;
				return true;
			}

			if (HasValue)
			{
				if (default(T) is string)
				{
					value = (T)(object)Value.ToString();
					return true;
				}

				try
				{
					// ReSharper disable once PossibleInvalidCastException
					value = (T)Value;
					return true;
				}
				catch
				{ }
			}

			value = default(T);
			return false;
		}

		public override string ToString()
		{
			return String.Format("{0} ({1})", Value != null ? Value.ToString() : "null", Flag);
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteFlag(Flag);

			switch (Flag)
			{
				case DataType.Null:
					break;
				case DataType.Bool:
					writer.Write(Convert.ToBoolean(Value));
					break;
				case DataType.Char:
					writer.Write(Convert.ToChar(Value));
					break;
				case DataType.Byte:
					writer.Write(Convert.ToByte(Value));
					break;
				case DataType.SByte:
					writer.Write(Convert.ToSByte(Value));
					break;
				case DataType.Short:
					writer.Write(Convert.ToInt16(Value));
					break;
				case DataType.UShort:
					writer.Write(Convert.ToUInt16(Value));
					break;
				case DataType.Int:
					writer.Write(Convert.ToInt32(Value));
					break;
				case DataType.UInt:
					writer.Write(Convert.ToUInt32(Value));
					break;
				case DataType.Long:
					writer.Write(Convert.ToInt64(Value));
					break;
				case DataType.ULong:
					writer.Write(Convert.ToUInt64(Value));
					break;
				case DataType.Float:
					writer.Write(Convert.ToSingle(Value));
					break;
				case DataType.Decimal:
					writer.Write(Convert.ToDecimal(Value));
					break;
				case DataType.Double:
					writer.Write(Convert.ToDouble(Value));
					break;
				case DataType.String:
					writer.Write(Convert.ToString(Value));
					break;
				case DataType.DateTime:
					writer.Write(Convert.ToDateTime(Value));
					break;
				case DataType.TimeSpan:
					writer.Write((TimeSpan)Value);
					break;
			}
		}

		public void Deserialize(GenericReader reader)
		{
			Flag = reader.ReadFlag<DataType>();
			Type = Flag.ToType();

			switch (Flag)
			{
				case DataType.Null:
					Value = null;
					break;
				case DataType.Bool:
					Value = reader.ReadBool();
					break;
				case DataType.Char:
					Value = reader.ReadChar();
					break;
				case DataType.Byte:
					Value = reader.ReadByte();
					break;
				case DataType.SByte:
					Value = reader.ReadSByte();
					break;
				case DataType.Short:
					Value = reader.ReadShort();
					break;
				case DataType.UShort:
					Value = reader.ReadUShort();
					break;
				case DataType.Int:
					Value = reader.ReadInt();
					break;
				case DataType.UInt:
					Value = reader.ReadUInt();
					break;
				case DataType.Long:
					Value = reader.ReadLong();
					break;
				case DataType.ULong:
					Value = reader.ReadULong();
					break;
				case DataType.Float:
					Value = reader.ReadFloat();
					break;
				case DataType.Decimal:
					Value = reader.ReadDecimal();
					break;
				case DataType.Double:
					Value = reader.ReadDouble();
					break;
				case DataType.String:
					Value = reader.ReadString() ?? String.Empty;
					break;
				case DataType.DateTime:
					Value = reader.ReadDateTime();
					break;
				case DataType.TimeSpan:
					Value = reader.ReadTimeSpan();
					break;
			}
		}

		public static bool IsSimpleType(object value)
		{
			return value != null && DataTypes.Lookup(value) != DataType.Null;
		}

		public static bool IsSimpleType(Type type)
		{
			return type != null && DataTypes.Lookup(type) != DataType.Null;
		}

		public static object ToObject(SimpleType value)
		{
			return value.Value;
		}

		public static SimpleType FromObject(object value)
		{
			return new SimpleType(value);
		}

		public static bool TryParse<T>(string data, out T value)
		{
			value = default(T);

			var flag = DataTypes.Lookup(value);

			object val;

			if (flag == DataType.Null)
			{
				var i = 0;

				while (i < DataTypes.NumericFlags.Length)
				{
					flag = DataTypes.NumericFlags[i++];

					if (TryConvert(data, flag, out val))
					{
						try
						{
							value = (T)val;
							return true;
						}
						catch
						{
							return false;
						}
					}
				}

				return false;
			}

			if (TryConvert(data, flag, out val))
			{
				try
				{
					value = (T)val;
					return true;
				}
				catch
				{
					return false;
				}
			}

			return false;
		}

		public static bool TryParse(string data, DataType flag, out SimpleType value)
		{
			value = Null;

			object val;

			if (flag == DataType.Null)
			{
				var i = 0;

				while (i < DataTypes.NumericFlags.Length)
				{
					flag = DataTypes.NumericFlags[i++];

					if (TryConvert(data, flag, out val))
					{
						try
						{
							value = new SimpleType(val);
							return true;
						}
						catch
						{
							return false;
						}
					}
				}

				return false;
			}

			if (TryConvert(data, flag, out val))
			{
				try
				{
					value = new SimpleType(val);
					return true;
				}
				catch
				{
					return false;
				}
			}

			return false;
		}

		public static bool TryConvert(string data, DataType flag, out object val)
		{
			val = null;

			if (flag == DataType.Null)
			{
				return false;
			}

			try
			{
				var numStyle = Insensitive.StartsWith(data.Trim(), "0x") ? NumberStyles.HexNumber : NumberStyles.Any;

				if (numStyle == NumberStyles.HexNumber)
				{
					data = data.Substring(data.IndexOf("0x", StringComparison.OrdinalIgnoreCase) + 2);
				}

				switch (flag)
				{
					case DataType.Bool:
						val = Boolean.Parse(data);
						return true;
					case DataType.Char:
						val = Char.Parse(data);
						return true;
					case DataType.Byte:
						val = Byte.Parse(data, numStyle);
						return true;
					case DataType.SByte:
						val = SByte.Parse(data, numStyle);
						return true;
					case DataType.Short:
						val = Int16.Parse(data, numStyle);
						return true;
					case DataType.UShort:
						val = UInt16.Parse(data, numStyle);
						return true;
					case DataType.Int:
						val = Int32.Parse(data, numStyle);
						return true;
					case DataType.UInt:
						val = UInt32.Parse(data, numStyle);
						return true;
					case DataType.Long:
						val = Int64.Parse(data, numStyle);
						return true;
					case DataType.ULong:
						val = UInt64.Parse(data, numStyle);
						return true;
					case DataType.Float:
						val = Single.Parse(data, numStyle);
						return true;
					case DataType.Decimal:
						val = Decimal.Parse(data, numStyle);
						return true;
					case DataType.Double:
						val = Double.Parse(data, numStyle);
						return true;
					case DataType.String:
						val = data;
						return true;
					case DataType.DateTime:
						val = DateTime.Parse(data, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces);
						return true;
					case DataType.TimeSpan:
						val = TimeSpan.Parse(data);
						return true;
					default:
						return false;
				}
			}
			catch
			{
				return false;
			}
		}

		public static implicit operator SimpleType(bool value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(char value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(byte value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(sbyte value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(short value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(ushort value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(int value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(uint value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(long value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(ulong value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(float value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(decimal value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(double value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(string value)
		{
			return new SimpleType(value ?? String.Empty);
		}

		public static implicit operator SimpleType(DateTime value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(TimeSpan value)
		{
			return new SimpleType(value);
		}
	}

	public static class DataTypes
	{
		private static readonly Dictionary<DataType, Type> _DataTypeTable = new Dictionary<DataType, Type>
		{
			{DataType.Null, null},
			{DataType.Bool, typeof(Boolean)},
			{DataType.Char, typeof(Char)},
			{DataType.Byte, typeof(Byte)},
			{DataType.SByte, typeof(SByte)},
			{DataType.Short, typeof(Int16)},
			{DataType.UShort, typeof(UInt16)},
			{DataType.Int, typeof(Int32)},
			{DataType.UInt, typeof(UInt32)},
			{DataType.Long, typeof(Int64)},
			{DataType.ULong, typeof(UInt64)},
			{DataType.Float, typeof(Single)},
			{DataType.Decimal, typeof(Decimal)},
			{DataType.Double, typeof(Double)},
			{DataType.String, typeof(String)},
			{DataType.DateTime, typeof(DateTime)},
			{DataType.TimeSpan, typeof(TimeSpan)}
		};

		public static DataType[] Flags = _DataTypeTable.Keys.ToArray();

		public static DataType[] IntegralNumericFlags =
		{
			DataType.Byte, DataType.SByte, DataType.Short, DataType.UShort, DataType.Int, DataType.UInt, DataType.Long,
			DataType.ULong
		};

		public static DataType[] RealNumericFlags = {DataType.Float, DataType.Decimal, DataType.Double};

		public static DataType[] NumericFlags = IntegralNumericFlags.Merge(RealNumericFlags);

		public static Type ToType(this DataType f)
		{
			return Lookup(f);
		}

		public static DataType FromType(Type t)
		{
			return Lookup(t);
		}

		public static DataType Lookup(object o)
		{
			return o != null ? Lookup(o.GetType()) : DataType.Null;
		}

		public static DataType Lookup(Type t)
		{
			return _DataTypeTable.GetKey(t);
		}

		public static Type Lookup(DataType f)
		{
			return _DataTypeTable.GetValue(f);
		}

		public static bool IsSimple(this Type t)
		{
			return Lookup(t) != DataType.Null;
		}

		public static DataType GetDataType(this Type t)
		{
			return Lookup(t);
		}
	}
}