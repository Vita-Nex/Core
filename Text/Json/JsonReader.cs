#region Header
//   Vorspire    _,-'/-'/  JsonReader.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Server;
using Server.Guilds;
#if ServUO
using CustomsFramework;
#endif
#endregion

namespace VitaNex.Text
{
	public class JsonReader : GenericReader, IDisposable
	{
		private int _NodeID;
		private int _ObjectID;

		public Stream BaseStream { get; private set; }
		public Encoding Encoding { get; private set; }
		public List<List<object>> Nodes { get; private set; }
		public List<object> CurrentNode { get; private set; }

		public bool IsDisposed { get; private set; }

		public JsonReader(byte[] buffer)
			: this(buffer, Json.DefaultEncoding)
		{ }

		public JsonReader(byte[] buffer, Encoding encoding)
			: this(new MemoryStream(buffer), encoding)
		{ }

		public JsonReader(string path)
			: this(path, Json.DefaultEncoding)
		{ }

		public JsonReader(string path, Encoding encoding)
			: this(File.OpenRead(path), encoding)
		{ }

		public JsonReader(Stream stream)
			: this(stream, Json.DefaultEncoding)
		{ }

		public JsonReader(StreamReader reader)
			: this(reader.BaseStream, reader.CurrentEncoding)
		{ }

		public JsonReader(Stream stream, Encoding encoding)
		{
			BaseStream = stream;
			Encoding = encoding;
			Nodes = new List<List<object>>(0x20);

			var length = (int)(BaseStream.Length - BaseStream.Position);
			var buffer = new byte[Math.Min(length, 65535)];

			var json = String.Empty;

			while (length > 0)
			{
				var c = Math.Min(length, buffer.Length);

				length -= BaseStream.Read(buffer, 0, c);
				json += Encoding.GetString(buffer, 0, c);
			}

			var o = Json.Decode(json) as List<object>;

			if (o == null)
			{
				return;
			}

			foreach (var e in o.OfType<List<object>>())
			{
				Nodes.Add(e);
			}
		}

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			BaseStream.Close();
			BaseStream = null;

			foreach (var n in Nodes)
			{
				n.Free(true);
			}

			Nodes.Free(true);
			Nodes = null;

			Encoding = null;

			IsDisposed = true;
		}

		public override bool End()
		{
			return IsDisposed || _NodeID >= Nodes.Count;
		}

		private T ReadValue<T>()
		{
			if (CurrentNode == null || _ObjectID >= CurrentNode.Count)
			{
				_NodeID = 0;
				_ObjectID = 0;

				CurrentNode = Nodes.Count > 0 ? Nodes[_NodeID++] : null;
			}

			if (CurrentNode == null || _ObjectID >= CurrentNode.Count)
			{
				return default(T);
			}

			return (T)CurrentNode[_ObjectID++];
		}

		private List<TObj> ReadArray<T, TObj>(Func<T, TObj> selector, Func<TObj, bool> predicate)
		{
			var list = ReadValue<List<object>>();

			return list.Cast<T>().Select(selector).Where(predicate).ToList();
		}

		public override string ReadString()
		{
			return ReadValue<string>();
		}

		public override DateTime ReadDateTime()
		{
			return DateTime.Parse(ReadValue<string>());
		}

		public override DateTimeOffset ReadDateTimeOffset()
		{
			return DateTimeOffset.Parse(ReadValue<string>());
		}

		public override TimeSpan ReadTimeSpan()
		{
			return TimeSpan.Parse(ReadValue<string>());
		}

		public override DateTime ReadDeltaTime()
		{
			return DateTime.Parse(ReadValue<string>());
		}

		public override decimal ReadDecimal()
		{
			return ReadValue<decimal>();
		}

		public override long ReadLong()
		{
			return ReadValue<long>();
		}

		public override ulong ReadULong()
		{
			return ReadValue<ulong>();
		}

		public override int ReadInt()
		{
			return ReadValue<int>();
		}

		public override uint ReadUInt()
		{
			return ReadValue<uint>();
		}

		public override short ReadShort()
		{
			return ReadValue<short>();
		}

		public override ushort ReadUShort()
		{
			return ReadValue<ushort>();
		}

		public override double ReadDouble()
		{
			return ReadValue<double>();
		}

		public override float ReadFloat()
		{
			return ReadValue<float>();
		}

		public override char ReadChar()
		{
			return ReadValue<char>();
		}

		public override byte ReadByte()
		{
			return ReadValue<byte>();
		}

		public override sbyte ReadSByte()
		{
			return ReadValue<sbyte>();
		}

		public override bool ReadBool()
		{
			return Boolean.Parse(ReadValue<string>());
		}

		public override int ReadEncodedInt()
		{
			return ReadValue<int>();
		}

		public override IPAddress ReadIPAddress()
		{
			return IPAddress.Parse(ReadValue<string>());
		}

		public override Point3D ReadPoint3D()
		{
			return Point3D.Parse(ReadValue<string>());
		}

		public override Point2D ReadPoint2D()
		{
			return Point2D.Parse(ReadValue<string>());
		}

		public override Rectangle2D ReadRect2D()
		{
			return Rectangle2D.Parse(ReadValue<string>());
		}

		public override Rectangle3D ReadRect3D()
		{
			var value = ReadValue<string>();

			var start = value.IndexOf('(');
			var end = value.IndexOf(',', start + 1);

			var param1 = value.Substring(start + 1, end - (start + 1)).Trim();

			start = end;
			end = value.IndexOf(',', start + 1);

			var param2 = value.Substring(start + 1, end - (start + 1)).Trim();

			start = end;
			end = value.IndexOf(',', start + 1);

			var param3 = value.Substring(start + 1, end - (start + 1)).Trim();

			start = end;
			end = value.IndexOf(',', start + 1);

			var param4 = value.Substring(start + 1, end - (start + 1)).Trim();

			start = end;
			end = value.IndexOf(',', start + 1);

			var param5 = value.Substring(start + 1, end - (start + 1)).Trim();

			start = end;
			end = value.IndexOf(')', start + 1);

			var param6 = value.Substring(start + 1, end - (start + 1)).Trim();

			return new Rectangle3D(
				Convert.ToInt32(param1),
				Convert.ToInt32(param2),
				Convert.ToInt32(param3),
				Convert.ToInt32(param4),
				Convert.ToInt32(param5),
				Convert.ToInt32(param6));
		}

		public override Map ReadMap()
		{
			return Map.Parse(ReadValue<string>());
		}

		public override Item ReadItem()
		{
			return World.FindItem(ReadValue<int>());
		}

		public override Mobile ReadMobile()
		{
			return World.FindMobile(ReadValue<int>());
		}

		public override BaseGuild ReadGuild()
		{
			return BaseGuild.FindByName(ReadValue<string>());
		}

		public override T ReadItem<T>()
		{
			return ReadItem() as T;
		}

		public override T ReadMobile<T>()
		{
			return ReadMobile() as T;
		}

		public override T ReadGuild<T>()
		{
			return ReadGuild() as T;
		}

		public override Race ReadRace()
		{
			return Race.Parse(ReadValue<string>());
		}

		public override ArrayList ReadItemList()
		{
			return new ArrayList(ReadArray<Serial, Item>(World.FindItem, o => o != null));
		}

		public override ArrayList ReadMobileList()
		{
			return new ArrayList(ReadArray<Serial, Mobile>(World.FindMobile, o => o != null));
		}

		public override ArrayList ReadGuildList()
		{
			return new ArrayList(ReadArray<string, BaseGuild>(BaseGuild.FindByName, o => o != null));
		}

		public override List<Item> ReadStrongItemList()
		{
			return ReadArray<Serial, Item>(World.FindItem, o => o != null);
		}

		public override List<T> ReadStrongItemList<T>()
		{
			return ReadArray<Serial, T>(o => World.FindItem(o) as T, o => o != null);
		}

		public override List<Mobile> ReadStrongMobileList()
		{
			return ReadArray<Serial, Mobile>(World.FindMobile, o => o != null);
		}

		public override List<T> ReadStrongMobileList<T>()
		{
			return ReadArray<Serial, T>(o => World.FindMobile(o) as T, o => o != null);
		}

		public override List<BaseGuild> ReadStrongGuildList()
		{
			return ReadArray<string, BaseGuild>(BaseGuild.FindByName, o => o != null);
		}

		public override List<T> ReadStrongGuildList<T>()
		{
			return ReadArray<string, T>(o => BaseGuild.FindByName(o) as T, o => o != null);
		}

		public override HashSet<Item> ReadItemSet()
		{
			return new HashSet<Item>(ReadArray<Serial, Item>(World.FindItem, o => o != null));
		}

		public override HashSet<T> ReadItemSet<T>()
		{
			return new HashSet<T>(ReadArray<Serial, T>(o => World.FindItem(o) as T, o => o != null));
		}

		public override HashSet<Mobile> ReadMobileSet()
		{
			return new HashSet<Mobile>(ReadArray<Serial, Mobile>(World.FindMobile, o => o != null));
		}

		public override HashSet<T> ReadMobileSet<T>()
		{
			return new HashSet<T>(ReadArray<Serial, T>(o => World.FindMobile(o) as T, o => o != null));
		}

		public override HashSet<BaseGuild> ReadGuildSet()
		{
			return new HashSet<BaseGuild>(ReadArray<string, BaseGuild>(BaseGuild.FindByName, o => o != null));
		}

		public override HashSet<T> ReadGuildSet<T>()
		{
			return new HashSet<T>(ReadArray<string, T>(o => BaseGuild.FindByName(o) as T, o => o != null));
		}

#if ServUO
		public override int PeekInt()
		{
			return -1;
		}

		public override SaveData ReadData()
		{
			return null;
		}

		public override T ReadData<T>()
		{
			return null;
		}

		public override ArrayList ReadDataList()
		{
			return null;
		}

		public override List<SaveData> ReadStrongDataList()
		{
			return null;
		}

		public override List<T> ReadStrongDataList<T>()
		{
			return null;
		}

		public override HashSet<SaveData> ReadDataSet()
		{
			return null;
		}

		public override HashSet<T> ReadDataSet<T>()
		{
			return null;
		}
#endif
	}
}