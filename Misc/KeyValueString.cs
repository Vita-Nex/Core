#region Header
//   Vorspire    _,-'/-'/  KeyValueString.cs
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

using Server;
#endregion

namespace System
{
	[PropertyObject]
	public struct KeyValueString : IEquatable<KeyValuePair<string, string>>, IEquatable<KeyValueString>
	{
		[CommandProperty(AccessLevel.Counselor, true)]
		public string Key { get; private set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public string Value { get; set; }

		public KeyValueString(KeyValueString kvs)
			: this(kvs.Key, kvs.Value)
		{ }

		public KeyValueString(KeyValuePair<string, string> kvp)
			: this(kvp.Key, kvp.Value)
		{ }

		public KeyValueString(string key, string value)
			: this()
		{
			Key = key;
			Value = value;
		}

		public KeyValueString(GenericReader reader)
			: this()
		{
			Deserialize(reader);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int k = Key != null ? Key.GetHashCode() : 0, v = Value != null ? Value.GetHashCode() : 0;

				return (k * 397) ^ v;
			}
		}

		public override bool Equals(object obj)
		{
			return (obj is KeyValueString && Equals((KeyValueString)obj)) ||
				   (obj is KeyValuePair<string, string> && Equals((KeyValuePair<string, string>)obj));
		}

		public bool Equals(KeyValueString other)
		{
			return Key == other.Key && Value == other.Value;
		}

		public bool Equals(KeyValuePair<string, string> other)
		{
			return Key == other.Key && Value == other.Value;
		}

		public void Serialize(GenericWriter writer)
		{
			writer.SetVersion(0);

			writer.Write(Key);
			writer.Write(Value);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.GetVersion();

			Key = reader.ReadString();
			Value = reader.ReadString();
		}

		public static bool operator ==(KeyValueString l, KeyValueString r)
		{
			return Equals(l, r);
		}

		public static bool operator !=(KeyValueString l, KeyValueString r)
		{
			return !Equals(l, r);
		}

		public static implicit operator KeyValuePair<string, string>(KeyValueString kv)
		{
			return new KeyValuePair<string, string>(kv.Key, kv.Value);
		}

		public static implicit operator KeyValueString(KeyValuePair<string, string> kv)
		{
			return new KeyValueString(kv.Key, kv.Value);
		}
	}
}