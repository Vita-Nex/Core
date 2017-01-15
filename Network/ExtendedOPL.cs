#region Header
//   Vorspire    _,-'/-'/  ExtendedOPL.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Server;
using Server.Network;
#endregion

namespace VitaNex.Network
{
	/// <summary>
	///     Provides methods for extending Item and Mobile ObjectPropertyLists by invoking event subscriptions - No more
	///     GetProperties overrides!
	/// </summary>
	public sealed class ExtendedOPL : IList<string>
	{
		/// <summary>
		///     Breaks EmptyClilocs every Nth entry. EG: When 5 entries have been added, use the next clilocID
		/// </summary>
		public static int ClilocBreak = 5;

		/// <summary>
		///     Breaks EmptyClilocs when the current string value length exceeds this threshold, regardless of the current
		///     ClilocBreak.
		/// </summary>
		public static int ClilocThreshold = 160;

		/// <summary>
		///     Empty clilocID list extended with ~1_VAL~ *support should be the same as ~1_NOTHING~
		///     The default settings for an ExtendedOPL instance allows for up to 65 custom cliloc entries.
		///     Capacity is equal to the number of available empty clilocs multiplied by the cliloc break value.
		///     Default: 65 == 13 * 5
		///     Clilocs with multiple argument support will be parsed accordingly.
		///     It is recommended to use clilocs that contain no characters other than the argument placeholders and whitespace.
		/// </summary>
		public static int[] EmptyClilocs =
		{
			//1042971, 1070722, // ~1_NOTHING~ (Reserved by ObjectPropertyList)
			1114057, 1114778, 1114779, // ~1_val~
			1150541, // ~1_TOKEN~
			1153153, // ~1_year~

			1041522, // ~1~~2~~3~
			1060847, // ~1_val~ ~2_val~
			1116560, // ~1_val~ ~2_val~
			1116690, // ~1_val~ ~2_val~ ~3_val~
			1116691, // ~1_val~ ~2_val~ ~3_val~
			1116692, // ~1_val~ ~2_val~ ~3_val~
			1116693, // ~1_val~ ~2_val~ ~3_val~
			1116694 // ~1_val~ ~2_val~ ~3_val~
		};

		public static bool Initialized { get; private set; }

		/// <summary>
		///     Gets a value representing the parent OPL PacketHandler that was overridden, if any
		/// </summary>
		public static PacketHandler ReqOplParent { get; private set; }

		/// <summary>
		///     Gets a value representing the parent BatchOPL PacketHandler that was overridden, if any
		/// </summary>
		public static PacketHandler ReqBatchOplParent { get; private set; }

		/// <summary>
		///     Gets a value represting the handler to use when decoding OPL packet 0xD6
		/// </summary>
		public static OutgoingPacketOverrideHandler OutParent0xD6 { get; private set; }

		/// <summary>
		///     Event called when an Item based OPL is requested
		/// </summary>
		public static event Action<Item, Mobile, ExtendedOPL> OnItemOPLRequest;

		/// <summary>
		///     Event called when a Mobile based OPL is requested
		/// </summary>
		public static event Action<Mobile, Mobile, ExtendedOPL> OnMobileOPLRequest;

		/// <summary>
		///     Event called when an IEntity based OPL is requested that doesn't match an Item or Mobile
		/// </summary>
		public static event Action<IEntity, Mobile, ExtendedOPL> OnEntityOPLRequest;
		
		public static void Init()
		{
			if (Initialized)
			{
				return;
			}

			ReqOplParent = PacketHandlers.GetExtendedHandler(0x10);
			PacketHandlers.RegisterExtended(ReqOplParent.PacketID, ReqOplParent.Ingame, OnQueryProperties);

			ReqBatchOplParent = PacketHandlers.GetHandler(0xD6);

			PacketHandlers.Register(
				ReqBatchOplParent.PacketID,
				ReqBatchOplParent.Length,
				ReqBatchOplParent.Ingame,
				OnBatchQueryProperties);

			PacketHandlers.Register6017(
				ReqBatchOplParent.PacketID,
				ReqBatchOplParent.Length,
				ReqBatchOplParent.Ingame,
				OnBatchQueryProperties);
			
			OutParent0xD6 = OutgoingPacketOverrides.GetHandler(0xD6);
			OutgoingPacketOverrides.Register(0xD6, OnEncode0xD6);

			Initialized = true;
		}

		public static ObjectPropertyList ResolveOPL(IEntity e)
		{
			return ResolveOPL(e, null);
		}

		public static ObjectPropertyList ResolveOPL(IEntity e, Mobile v)
		{
			if (e == null || e.Deleted)
			{
				return null;
			}

			var opl = new ObjectPropertyList(e);

			if (e is Item)
			{
				var item = (Item)e;

				item.GetProperties(opl);
				item.AppendChildProperties(opl);
			}
			else if (e is Mobile)
			{
				var mob = (Mobile)e;

				mob.GetProperties(opl);
			}

			var eopl = new ExtendedOPL(opl);

			InvokeOPLRequest(e, v, eopl);

			eopl.Apply();

			//opl = eopl.Opl ?? opl;

			opl.Terminate();
			opl.SetStatic();
			
			return opl;
		}
		
		private static void OnEncode0xD6(NetState state, PacketReader reader, ref byte[] buffer, ref int length)
		{
			if (state == null || reader == null || buffer == null || length < 0)
			{
				return;
			}

			var pos = reader.Seek(0, SeekOrigin.Current);
			reader.Seek(5, SeekOrigin.Begin);
			Serial serial = reader.ReadInt32();
			reader.Seek(pos, SeekOrigin.Begin);

			var opl = ResolveOPL(World.FindEntity(serial), state.Mobile);

			if (opl != null)
			{
				buffer = opl.Compile(state.CompressionEnabled, out length);
			}
		}

		private static void OnBatchQueryProperties(NetState state, PacketReader pvSrc)
		{
			if (state == null || pvSrc == null || !ObjectPropertyList.Enabled)
			{
				return;
			}

			if (OnItemOPLRequest == null && OnMobileOPLRequest == null)
			{
				if (ReqBatchOplParent != null)
				{
					ReqBatchOplParent.OnReceive(state, pvSrc);
					return;
				}
			}

			var from = state.Mobile;

			var length = pvSrc.Size - 3;

			if (length < 0 || (length % 4) != 0)
			{
				return;
			}

			var count = length / 4;

			for (var i = 0; i < count; ++i)
			{
				Serial s = pvSrc.ReadInt32();

				if (s.IsMobile)
				{
					var m = World.FindMobile(s);

					if (m != null && from.CanSee(m) && Utility.InUpdateRange(from, m))
					{
						SendPropertiesTo(from, m);
					}
				}
				else if (s.IsItem)
				{
					var item = World.FindItem(s);

					if (item != null && !item.Deleted && from.CanSee(item) &&
						Utility.InUpdateRange(from.Location, item.GetWorldLocation()))
					{
						SendPropertiesTo(from, item);
					}
				}
			}
		}

		private static void OnQueryProperties(NetState state, PacketReader pvSrc)
		{
			if (!ObjectPropertyList.Enabled || state == null || pvSrc == null)
			{
				return;
			}

			var from = state.Mobile;
			Serial s = pvSrc.ReadInt32();

			if (s.IsMobile)
			{
				var m = World.FindMobile(s);

				if (m != null && from.CanSee(m) && Utility.InUpdateRange(from, m))
				{
					SendPropertiesTo(from, m);
				}
			}
			else if (s.IsItem)
			{
				var item = World.FindItem(s);

				if (item != null && !item.Deleted && from.CanSee(item) &&
					Utility.InUpdateRange(from.Location, item.GetWorldLocation()))
				{
					SendPropertiesTo(from, item);
				}
			}
		}

		/// <summary>
		///     Forces the comilation of a new Mobile based ObjectPropertyList and sends it to the specified Mobile
		/// </summary>
		/// <param name="to">Mobile viewer, the Mobile viewing the OPL</param>
		/// <param name="m">Mobile owner, the Mobile which owns the OPL</param>
		public static void SendPropertiesTo(Mobile to, Mobile m)
		{
			if (to == null || m == null)
			{
				return;
			}

			var opl = ResolveOPL(m, to);

			if (opl != null)
			{
				to.Send(opl);
			}
		}

		/// <summary>
		///     Forces the comilation of a new Item based ObjectPropertyList and sends it to the specified Mobile
		/// </summary>
		/// <param name="to">Mobile viewer, the Mobile viewing the OPL</param>
		/// <param name="item"></param>
		public static void SendPropertiesTo(Mobile to, Item item)
		{
			if (to == null || item == null)
			{
				return;
			}

			var opl = ResolveOPL(item, to);

			if (opl != null)
			{
				to.Send(opl);
			}
		}

		private static void InvokeOPLRequest(IEntity entity, Mobile viewer, ExtendedOPL eopl)
		{
			if (entity == null || entity.Deleted || eopl == null)
			{
				return;
			}

			if (entity is Mobile && OnMobileOPLRequest != null)
			{
				OnMobileOPLRequest((Mobile)entity, viewer, eopl);
			}

			if (entity is Item && OnItemOPLRequest != null)
			{
				OnItemOPLRequest((Item)entity, viewer, eopl);
			}

			if (OnEntityOPLRequest != null)
			{
				OnEntityOPLRequest(entity, viewer, eopl);
			}
		}

		public static void AddTo(ObjectPropertyList opl, string line, params object[] args)
		{
			if (args != null)
			{
				line = String.Format(line, args);
			}

			AddTo(opl, new[] {line});
		}

		public static void AddTo(ObjectPropertyList opl, string[] lines)
		{
			if (opl != null)
			{
				new ExtendedOPL(opl, lines).Apply();
			}
		}
		
		private List<string> _Buffer;

		public int Count { get { return _Buffer.Count; } }

		public bool IsReadOnly { get { return false; } }

		public string this[int index] { get { return _Buffer[index]; } set { _Buffer[index] = value; } }

		/// <summary>
		///     Gets or sets the underlying ObjectPropertyList
		/// </summary>
		public ObjectPropertyList Opl { get; set; }
		
		public int LineBreak { get; set; }

		/// <summary>
		///     Create with pre-defined OPL
		/// </summary>
		/// <param name="opl">ObjectPropertyList object to wrap and extend</param>
		public ExtendedOPL(ObjectPropertyList opl)
		{
			_Buffer = new List<string>();

			Opl = opl;
			LineBreak = ClilocBreak;
		}

		/// <summary>
		///     Create with pre-defined OPL
		/// </summary>
		/// <param name="opl">ObjectPropertyList object to wrap and extend</param>
		/// <param name="capacity">Capacity of the extension</param>
		public ExtendedOPL(ObjectPropertyList opl, int capacity)
		{
			_Buffer = new List<string>(capacity);

			Opl = opl;
			LineBreak = ClilocBreak;
		}

		/// <summary>
		///     Create with pre-defined OPL
		/// </summary>
		/// <param name="opl">ObjectPropertyList object to wrap and extend</param>
		/// <param name="list">Pre-defined list to append to the specified OPL</param>
		public ExtendedOPL(ObjectPropertyList opl, IEnumerable<string> list)
		{
			_Buffer = new List<string>(list);

			Opl = opl;
			LineBreak = ClilocBreak;
		}

		~ExtendedOPL()
		{
			Opl = null;

			_Buffer.Free(true);
			_Buffer = null;
		}

		public bool NextEmpty(out ClilocInfo info)
		{
			info = null;

			if (Opl == null)
			{
				return false;
			}

			for (int i = 0, index; i < EmptyClilocs.Length; i++)
			{
				index = EmptyClilocs[i];

				if (!Opl.Contains(index))
				{
					info = ClilocLNG.NULL.Lookup(index);
					return true;
				}
			}

			return false;
		}

		public void Flush()
		{
			Apply();

			_Buffer.Free(true);
		}

		/// <summary>
		///     Applies all changes to the underlying ObjectPropertyList
		/// </summary>
		public void Apply()
		{
			if (Opl == null || _Buffer.Count == 0)
			{
				_Buffer.Clear();
				return;
			}

			ClilocInfo info;
			string final;
			int take;

			while (_Buffer.Count > 0)
			{
				if (!NextEmpty(out info))
				{
					break;
				}

				if (!info.HasArgs || Opl.Contains(info.Index))
				{
					continue;
				}

				final = String.Empty;
				take = 0;

				for (var i = 0; i < _Buffer.Count; i++)
				{
					var s = _Buffer[i];

					if (i > 0)
					{
						final += '\n';
					}

					if (String.IsNullOrWhiteSpace(s))
					{
						s = " ";
					}

					final += s;

					if (++take >= LineBreak || final.Length >= ClilocThreshold)
					{
						break;
					}
				}

				if (take == 0)
				{
					break;
				}

				_Buffer.RemoveRange(0, take);

				Opl.Add(info.Index, info.ToString(final));
			}

			_Buffer.Clear();
		}

		public void Add(string format, params object[] args)
		{
			_Buffer.Add(String.Format(format ?? String.Empty, args));
		}

		public void Add(int number, params object[] args)
		{
			var info = ClilocLNG.NULL.Lookup(number);

			if (info != null)
			{
				_Buffer.Add(info.ToString(args));
			}
		}

		public void AddRange(IEnumerable<string> lines)
		{
			_Buffer.AddRange(lines.Select(line => line ?? String.Empty));
		}

		public void Add(string line)
		{
			_Buffer.Add(line ?? String.Empty);
		}

		public bool Remove(string line)
		{
			return _Buffer.Remove(line ?? String.Empty);
		}

		public int RemoveAll(Predicate<string> match)
		{
			return _Buffer.RemoveAll(match);
		}

		public void RemoveRange(int index, int count)
		{
			_Buffer.RemoveRange(index, count);
		}

		public void RemoveAt(int index)
		{
			_Buffer.RemoveAt(index);
		}

		public int IndexOf(string line)
		{
			return _Buffer.IndexOf(line ?? String.Empty);
		}

		public void Insert(int index, string line)
		{
			_Buffer.Insert(index, line ?? String.Empty);
		}

		public void Insert(int index, string format, params object[] args)
		{
			_Buffer.Insert(index, String.Format(format ?? String.Empty, args));
		}

		public void Insert(int index, int number, params object[] args)
		{
			var info = ClilocLNG.NULL.Lookup(number);

			if (info != null)
			{
				_Buffer.Insert(index, info.ToString(args));
			}
		}

		public bool Contains(string line)
		{
			return _Buffer.Contains(line ?? String.Empty);
		}

		public void Clear()
		{
			_Buffer.Clear();
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			_Buffer.CopyTo(array, arrayIndex);
		}

		public IEnumerator<string> GetEnumerator()
		{
			return _Buffer.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _Buffer.GetEnumerator();
		}
	}
}