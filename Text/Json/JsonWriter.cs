#region Header
//   Vorspire    _,-'/-'/  JsonWriter.cs
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
using System.Globalization;
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
	public class JsonWriter : GenericWriter, IDisposable
	{
		private bool _StartWritten;
		private bool _EndWritten;

		private int _ObjectID;

		public Stream BaseStream { get; private set; }
		public Encoding Encoding { get; private set; }
		public List<object> CurrentNode { get; private set; }

		public bool IsDisposed { get; private set; }

		public override long Position { get { return IsDisposed ? -1 : BaseStream.Position; } }

		public JsonWriter()
			: this(new MemoryStream())
		{ }

		public JsonWriter(string path)
			: this(path, Json.DefaultEncoding)
		{ }

		public JsonWriter(string path, Encoding encoding)
			: this(File.OpenWrite(path), encoding)
		{ }

		public JsonWriter(Stream stream)
			: this(stream, Json.DefaultEncoding)
		{ }

		public JsonWriter(StreamWriter writer)
			: this(writer.BaseStream, writer.Encoding)
		{ }

		public JsonWriter(Stream stream, Encoding encoding)
		{
			BaseStream = stream;
			Encoding = encoding;
			CurrentNode = new List<object>(0x20);
		}

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			Close();

			_ObjectID = 0;

			CurrentNode = null;
			Encoding = null;
			BaseStream = null;

			IsDisposed = true;
		}

		public override void Close()
		{
			if (IsDisposed)
			{
				return;
			}

			Compile(true);

			BaseStream.Close();
		}

		public void Flush()
		{
			Flush(false);
		}

		public void Flush(bool reset)
		{
			Compile(reset);

			if (!reset)
			{
				return;
			}

			_ObjectID = 0;

			_StartWritten = false;
			_EndWritten = false;
		}

		private void Compile(bool end)
		{
			if (IsDisposed)
			{
				return;
			}

			WriteStart();

			if (CurrentNode.Count > 0)
			{
				var json = Json.Encode(CurrentNode);

				if (_ObjectID++ > 0)
				{
					json = ", " + json;
				}

				var buffer = Encoding.GetBytes(json);

				BaseStream.Write(buffer, 0, buffer.Length);

				CurrentNode.Clear();
			}

			if (end)
			{
				WriteEnd();
			}

			BaseStream.Flush();
		}

		private void WriteStart()
		{
			if (IsDisposed || _StartWritten)
			{
				return;
			}

			InternalWrite("[ ");
			_StartWritten = true;
		}

		private void WriteEnd()
		{
			if (IsDisposed || _EndWritten)
			{
				return;
			}

			InternalWrite(" ]");
			_EndWritten = true;
		}

		private void InternalWrite(string value)
		{
			var buffer = Encoding.GetBytes(value);

			BaseStream.Write(buffer, 0, buffer.Length);
		}

		private void AddString(string value)
		{
			if (!IsDisposed)
			{
				CurrentNode.Add(value ?? String.Empty);
			}
		}

		private void AddNumber(double value)
		{
			if (!IsDisposed)
			{
				CurrentNode.Add(value);
			}
		}

		private void AddBoolean(bool value)
		{
			if (!IsDisposed)
			{
				CurrentNode.Add(value);
			}
		}

		private void AddObject(IDictionary value)
		{
			if (!IsDisposed)
			{
				CurrentNode.Add(value ?? new Dictionary<string, object>());
			}
		}

		private void AddArray<T>(IEnumerable<T> value, Func<T, object> selector)
		{
			if (IsDisposed)
			{
				return;
			}

			var o = value.Select(selector).ToArray();

			CurrentNode.Add(o);
		}

		public override void Write(string value)
		{
			AddString(value);
		}

		public override void Write(DateTime value)
		{
			AddString(value.ToString(CultureInfo.InvariantCulture));
		}

		public override void Write(DateTimeOffset value)
		{
			AddString(value.ToString());
		}

		public override void WriteDeltaTime(DateTime value)
		{
			AddString((value - DateTime.UtcNow).ToString());
		}

		public override void Write(IPAddress value)
		{
			AddString(value.ToString());
		}

		public override void Write(TimeSpan value)
		{
			AddString(value.ToString());
		}

		public override void Write(decimal value)
		{
			AddNumber((double)value);
		}

		public override void Write(long value)
		{
			AddNumber(value);
		}

		public override void Write(ulong value)
		{
			AddNumber(value);
		}

		public override void Write(int value)
		{
			AddNumber(value);
		}

		public override void Write(uint value)
		{
			AddNumber(value);
		}

		public override void Write(short value)
		{
			AddNumber(value);
		}

		public override void Write(ushort value)
		{
			AddNumber(value);
		}

		public override void Write(double value)
		{
			AddNumber(value);
		}

		public override void Write(float value)
		{
			AddNumber(value);
		}

		public override void Write(char value)
		{
			AddNumber(value);
		}

		public override void Write(byte value)
		{
			AddNumber(value);
		}

		public override void Write(sbyte value)
		{
			AddNumber(value);
		}

		public override void Write(bool value)
		{
			AddBoolean(value);
		}

		public override void WriteEncodedInt(int value)
		{
			AddNumber(value);
		}

		public override void Write(Point3D value)
		{
			AddString(value.ToString());
		}

		public override void Write(Point2D value)
		{
			AddString(value.ToString());
		}

		public override void Write(Rectangle2D value)
		{
			AddString(value.ToString());
		}

		public override void Write(Rectangle3D value)
		{
			AddString(
				String.Format(
					"({0}, {1}, {2})+({3}, {4}, {5})",
					value.Start.X,
					value.Start.Y,
					value.Start.Z,
					value.Width,
					value.Height,
					value.Depth));
		}

		public override void Write(Map value)
		{
			AddString(value.Name);
		}

		public override void Write(Item value)
		{
			AddNumber(value.Serial.Value);
		}

		public override void Write(Mobile value)
		{
			AddNumber(value.Serial.Value);
		}

		public override void Write(BaseGuild value)
		{
			AddString(value.Name);
		}

		public override void WriteItem<T>(T value)
		{
			Write(value);
		}

		public override void WriteMobile<T>(T value)
		{
			Write(value);
		}

		public override void WriteGuild<T>(T value)
		{
			Write(value);
		}

		public override void Write(Race value)
		{
			AddString(value.ToString());
		}

		public override void WriteItemList(ArrayList list)
		{
			AddArray(list.OfType<Item>(), o => o.Serial.Value);
		}

		public override void WriteItemList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				var i = list.Count;

				while (--i >= 0)
				{
					var o = list[i] as Item;

					if (o == null || o.Deleted)
					{
						list.RemoveAt(i);
					}
				}
			}

			WriteItemList(list);
		}

		public override void WriteMobileList(ArrayList list)
		{
			AddArray(list.OfType<Mobile>(), o => o.Serial.Value);
		}

		public override void WriteMobileList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				var i = list.Count;

				while (--i >= 0)
				{
					var o = list[i] as Mobile;

					if (o == null || o.Deleted)
					{
						list.RemoveAt(i);
					}
				}
			}

			WriteMobileList(list);
		}

		public override void WriteGuildList(ArrayList list)
		{
			AddArray(list.OfType<BaseGuild>(), o => o.Id);
		}

		public override void WriteGuildList(ArrayList list, bool tidy)
		{
			if (tidy)
			{
				var i = list.Count;

				while (--i >= 0)
				{
					var o = list[i] as BaseGuild;

					if (o == null || o.Disbanded)
					{
						list.RemoveAt(i);
					}
				}
			}

			WriteGuildList(list);
		}

		public override void Write(List<Item> list)
		{
			AddArray(list, o => o.Serial.Value);
		}

		public override void Write(List<Item> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(o => o == null || o.Deleted);
			}

			Write(list);
		}

		public override void WriteItemList<T>(List<T> list)
		{
			AddArray(list, o => o.Serial.Value);
		}

		public override void WriteItemList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(o => o == null || o.Deleted);
			}

			WriteItemList(list);
		}

		public override void Write(HashSet<Item> list)
		{
			AddArray(list, o => o.Serial.Value);
		}

		public override void Write(HashSet<Item> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveWhere(o => o == null || o.Deleted);
			}

			Write(list);
		}

		public override void WriteItemSet<T>(HashSet<T> set)
		{
			AddArray(set, o => o.Serial.Value);
		}

		public override void WriteItemSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(o => o == null || o.Deleted);
			}

			WriteItemSet(set);
		}

		public override void Write(List<Mobile> list)
		{
			AddArray(list, o => o.Serial.Value);
		}

		public override void Write(List<Mobile> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(o => o == null || o.Deleted);
			}

			Write(list);
		}

		public override void WriteMobileList<T>(List<T> list)
		{
			AddArray(list, o => o.Serial.Value);
		}

		public override void WriteMobileList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(o => o == null || o.Deleted);
			}

			WriteMobileList(list);
		}

		public override void Write(HashSet<Mobile> list)
		{
			AddArray(list, o => o.Serial.Value);
		}

		public override void Write(HashSet<Mobile> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveWhere(o => o == null || o.Deleted);
			}

			Write(list);
		}

		public override void WriteMobileSet<T>(HashSet<T> set)
		{
			AddArray(set, o => o.Serial.Value);
		}

		public override void WriteMobileSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(o => o == null || o.Deleted);
			}

			WriteMobileSet(set);
		}

		public override void Write(List<BaseGuild> list)
		{
			AddArray(list, o => o.Id);
		}

		public override void Write(List<BaseGuild> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(o => o == null || o.Disbanded);
			}

			Write(list);
		}

		public override void WriteGuildList<T>(List<T> list)
		{
			AddArray(list, o => o.Id);
		}

		public override void WriteGuildList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(o => o == null || o.Disbanded);
			}

			WriteGuildList(list);
		}

		public override void Write(HashSet<BaseGuild> list)
		{
			AddArray(list, o => o.Id);
		}

		public override void Write(HashSet<BaseGuild> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveWhere(o => o == null || o.Disbanded);
			}

			Write(list);
		}

		public override void WriteGuildSet<T>(HashSet<T> set)
		{
			AddArray(set, o => o.Id);
		}

		public override void WriteGuildSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(o => o == null || o.Disbanded);
			}

			WriteGuildSet(set);
		}

#if ServUO
		public override void Write(SaveData value)
		{ }

		public override void WriteData<T>(T value)
		{ }

		public override void WriteDataList(ArrayList list)
		{ }

		public override void WriteDataList(ArrayList list, bool tidy)
		{ }

		public override void Write(List<SaveData> list)
		{ }

		public override void Write(List<SaveData> list, bool tidy)
		{ }

		public override void WriteDataList<T>(List<T> list)
		{ }

		public override void WriteDataList<T>(List<T> list, bool tidy)
		{ }

		public override void Write(HashSet<SaveData> set)
		{ }

		public override void Write(HashSet<SaveData> set, bool tidy)
		{ }

		public override void WriteDataSet<T>(HashSet<T> set)
		{ }

		public override void WriteDataSet<T>(HashSet<T> set, bool tidy)
		{ }
#endif
	}
}