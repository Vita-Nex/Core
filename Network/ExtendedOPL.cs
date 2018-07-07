#region Header
//   Vorspire    _,-'/-'/  ExtendedOPL.cs
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

using Server;
using Server.Network;

using VitaNex.Collections;
#endregion

namespace VitaNex.Network
{
	public delegate void OPLQueryValidator(Mobile viewer, IEntity target, ref bool allow);

	/// <summary>
	///     Provides methods for extending Item and Mobile ObjectPropertyLists by invoking event subscriptions - No more
	///     GetProperties overrides!
	/// </summary>
	public sealed class ExtendedOPL : IList<string>, IDisposable
	{
		private static readonly string[] _EmptyBuffer = new string[0];

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

		/// <summary>
		///     Event called when an IEntity based OPL is requested
		/// </summary>
		public static event OPLQueryValidator OnValidateQuery;

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

			using (var eopl = new ExtendedOPL(opl))
			{
				InvokeOPLRequest(e, v, eopl);
			}

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

			var serial = reader.ReadInt32();

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

			var length = pvSrc.Size - 3;

			if (length < 0 || (length % 4) != 0)
			{
				return;
			}

			var count = length / 4;

			Serial s;

			for (var i = 0; i < count; ++i)
			{
				s = pvSrc.ReadInt32();

				if (s.IsValid)
				{
					HandleQueryProperties(state.Mobile, World.FindEntity(s));
				}
			}
		}

		private static void OnQueryProperties(NetState state, PacketReader pvSrc)
		{
			if (!ObjectPropertyList.Enabled || state == null || pvSrc == null)
			{
				return;
			}

			var serial = (Serial)pvSrc.ReadInt32();

			if (serial.IsValid)
			{
				HandleQueryProperties(state.Mobile, World.FindEntity(serial));
			}
		}

		private static void HandleQueryProperties(Mobile viewer, IEntity e)
		{
			if (viewer == null || viewer.Deleted || e == null || e.Deleted || !viewer.CanSee(e))
			{
				return;
			}

			if (OnValidateQuery != null)
			{
				var allow = true;

				OnValidateQuery(viewer, e, ref allow);

				if (!allow)
				{
					return;
				}
			}

			if (e is Mobile)
			{
				var m = (Mobile)e;

				if (Utility.InUpdateRange(viewer, m))
				{
					SendPropertiesTo(viewer, m);
				}
			}
			else if (e is Item)
			{
				var item = (Item)e;

				if (Utility.InUpdateRange(viewer, item.GetWorldLocation()))
				{
					SendPropertiesTo(viewer, item);
				}
			}
		}

		/// <summary>
		///     Forces the compilation of a new Mobile based ObjectPropertyList and sends it to the specified Mobile
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
		///     Forces the compilation of a new Item based ObjectPropertyList and sends it to the specified Mobile
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

		public static void AddTo(ObjectPropertyList opl, params object[] args)
		{
			if (opl == null || args.IsNullOrEmpty())
			{
				return;
			}

			using (var o = new ExtendedOPL(opl))
			{
				foreach (var a in args)
				{
					if (a != null)
					{
						o.Add(a.ToString());
					}
					else
					{
						o.Add(String.Empty);
					}
				}
			}
		}

		public static void AddTo(ObjectPropertyList opl, IEnumerable<object> args)
		{
			if (opl == null || args == null)
			{
				return;
			}

			using (var o = new ExtendedOPL(opl))
			{
				foreach (var a in args)
				{
					if (a != null)
					{
						o.Add(a.ToString());
					}
					else
					{
						o.Add(String.Empty);
					}
				}
			}
		}

		public static void AddTo(ObjectPropertyList opl, string line, params object[] args)
		{
			if (opl == null)
			{
				return;
			}

			using (var o = new ExtendedOPL(opl))
			{
				if (!args.IsNullOrEmpty())
				{
					o.Add(line, args);
				}
				else
				{
					o.Add(line);
				}
			}
		}

		public static void AddTo(ObjectPropertyList opl, string[] lines)
		{
			if (opl == null || lines.IsNullOrEmpty())
			{
				return;
			}

			using (var o = new ExtendedOPL(opl))
			{
				o.AddRange(lines);
			}
		}

		private List<string> _Buffer;

		bool ICollection<string>.IsReadOnly { get { return _Buffer.IsNullOrEmpty(); } }

		public int Count { get { return _Buffer != null ? _Buffer.Count : 0; } }

		public string this[int index]
		{
			get { return _Buffer != null ? _Buffer[index] : null; }
			set
			{
				if (_Buffer != null)
				{
					_Buffer[index] = value;
				}
			}
		}

		/// <summary>
		///     Gets or sets the underlying ObjectPropertyList
		/// </summary>
		public ObjectPropertyList Opl { get; set; }

		public int LineBreak { get; set; }

		public bool IsDisposed { get; private set; }

		/// <summary>
		///     Create with pre-defined OPL
		/// </summary>
		/// <param name="opl">ObjectPropertyList object to wrap and extend</param>
		public ExtendedOPL(ObjectPropertyList opl)
		{
			_Buffer = ListPool<string>.AcquireObject();

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
			_Buffer = ListPool<string>.AcquireObject();
			_Buffer.Capacity = capacity;

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
			_Buffer = ListPool<string>.AcquireObject();
			_Buffer.AddRange(list);

			Opl = opl;
			LineBreak = ClilocBreak;
		}

		~ExtendedOPL()
		{
			Dispose(true);
		}

		void IDisposable.Dispose()
		{
			Dispose(false);
		}

		private void Dispose(bool gc)
		{
			if (IsDisposed)
			{
				return;
			}

			try
			{
				if (!gc)
				{
					GC.SuppressFinalize(this);
				}
			}
			catch
			{ }

			try
			{
				if (Opl != null)
				{
					Flush();

					Opl = null;
				}
			}
			catch
			{ }

			ObjectPool.Free(ref _Buffer);

			IsDisposed = true;
		}

		public bool NextEmpty(out ClilocInfo info)
		{
			info = null;

			if (IsDisposed || Opl == null)
			{
				return false;
			}

			for (int i = 0, index; i < EmptyClilocs.Length; i++)
			{
				index = EmptyClilocs[i];

				if (!Opl.Contains(index))
				{
					info = ClilocLNG.NULL.Lookup(index);

					if (info != null)
					{
						return true;
					}
				}
			}

			return false;
		}

		public void Flush()
		{
			if (IsDisposed)
			{
				return;
			}

			Apply();

			if (_Buffer != null)
			{
				_Buffer.Free(true);
			}
		}

		/// <summary>
		///     Applies all changes to the underlying ObjectPropertyList
		/// </summary>
		public void Apply()
		{
			if (IsDisposed)
			{
				return;
			}

			if (Opl == null || _Buffer == null || _Buffer.Count == 0)
			{
				if (_Buffer != null)
				{
					_Buffer.Clear();
				}

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

					if (String.IsNullOrWhiteSpace(s))
					{
						s = " ";
					}

					if (i > 0)
					{
						final += '\n';
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

				if (!String.IsNullOrEmpty(final))
				{
					Opl.Add(info.Index, info.ToString(final));
				}
			}

			Clear();
		}

		public void Add(string format, params object[] args)
		{
			if (_Buffer != null)
			{
				_Buffer.Add(String.Format(format ?? String.Empty, args));
			}
		}

		public void Add(int number, params object[] args)
		{
			if (_Buffer == null)
			{
				return;
			}

			var info = ClilocLNG.NULL.Lookup(number);

			if (info != null)
			{
				_Buffer.Add(info.ToString(args));
			}
		}

		public void Add(ObjectPropertyList opl)
		{
			Add(opl, ClilocLNG.NULL);
		}

		public void Add(ObjectPropertyList opl, ClilocLNG lng)
		{
			if (opl != null && opl != Opl && opl.Entity != Opl.Entity)
			{
				AddRange(opl.GetAllLines(lng));
			}
		}

		public void AddRange(IEnumerable<string> lines)
		{
			if (_Buffer != null)
			{
				_Buffer.AddRange(lines.Select(line => line ?? String.Empty));
			}
		}

		public void Add(string line)
		{
			if (_Buffer != null)
			{
				_Buffer.Add(line ?? String.Empty);
			}
		}

		public bool Remove(string line)
		{
			if (_Buffer != null)
			{
				return _Buffer.Remove(line ?? String.Empty);
			}

			return false;
		}

		public int RemoveAll(Predicate<string> match)
		{
			if (_Buffer != null && match != null)
			{
				return _Buffer.RemoveAll(match);
			}

			return 0;
		}

		public void RemoveRange(int index, int count)
		{
			if (_Buffer != null)
			{
				_Buffer.RemoveRange(index, count);
			}
		}

		public void RemoveAt(int index)
		{
			if (_Buffer != null)
			{
				_Buffer.RemoveAt(index);
			}
		}

		public void Insert(int index, string line)
		{
			if (_Buffer != null)
			{
				_Buffer.Insert(index, line ?? String.Empty);
			}
		}

		public void Insert(int index, string format, params object[] args)
		{
			if (_Buffer != null)
			{
				_Buffer.Insert(index, String.Format(format ?? String.Empty, args));
			}
		}

		public void Insert(int index, int number, params object[] args)
		{
			if (_Buffer == null)
			{
				return;
			}

			var info = ClilocLNG.NULL.Lookup(number);

			if (info != null)
			{
				_Buffer.Insert(index, info.ToString(args));
			}
		}

		public int IndexOf(string line)
		{
			if (_Buffer != null)
			{
				return _Buffer.IndexOf(line ?? String.Empty);
			}

			return -1;
		}

		public bool Contains(string line)
		{
			return _Buffer != null && _Buffer.Contains(line ?? String.Empty);
		}

		public void Clear()
		{
			if (_Buffer != null)
			{
				_Buffer.Clear();
			}
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			if (_Buffer != null)
			{
				_Buffer.CopyTo(array, arrayIndex);
			}
		}

		public IEnumerator<string> GetEnumerator()
		{
			if (_Buffer != null)
			{
				return _Buffer.GetEnumerator();
			}

			return _EmptyBuffer.GetEnumerator<string>();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}