#region Header
//   Vorspire    _,-'/-'/  ArtworkSupport.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Server;
using Server.Items;
using Server.Network;

using VitaNex.Network;
#endregion

namespace VitaNex
{
	public static partial class ArtworkSupport
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public static CoreServiceOptions CSOptions { get; private set; }

		private static OutgoingPacketOverrideHandler _Parent0x1A;
		private static OutgoingPacketOverrideHandler _Parent0xF3;

		public static ClientVersion DefaultHighVersion = new ClientVersion(7, 0, 24, 3);
		public static ClientVersion DefaultLowVersion = new ClientVersion(0, 0, 0, 0);

		public static Dictionary<Type, List<ArtworkInfo>> Info { get; private set; }

		public static short[] LandTextures { get; private set; }
		public static short[] StaticAnimations { get; private set; }

		public static short LookupTexture(int landID)
		{
			return LandTextures.InBounds(landID) ? LandTextures[landID] : (short)0;
		}

		public static short LookupAnimation(int staticID)
		{
			return StaticAnimations.InBounds(staticID) ? StaticAnimations[staticID] : (short)0;
		}

		public static int LookupGump(int staticID, bool female)
		{
			int value = LookupAnimation(staticID);

			if (value > 0)
			{
				value += female ? 60000 : 50000;
			}

			return value;
		}

		public static int LookupGump(Body body)
		{
			switch (body)
			{
				case 183:
				case 185:
				case 400:
				case 402:
					return 12;
				case 184:
				case 186:
				case 401:
				case 403:
					return 13;
				case 605:
				case 607:
					return 14;
				case 606:
				case 608:
					return 15;
				case 666:
					return 666;
				case 667:
					return 665;
				case 694:
					return 666;
				case 695:
					return 665;
				case 750:
					return 12;
				case 751:
					return 13;
			}

			return -1;
		}

		private static void HandleWorldItem(NetState state, PacketReader reader, ref byte[] buffer, ref int length)
		{
			if (_Parent0x1A != null)
			{
				_Parent0x1A(state, reader, ref buffer, ref length);
			}

			if (state == null || reader == null || buffer == null)
			{
				return;
			}

			var pos = reader.Seek(0, SeekOrigin.Current);
			reader.Seek(3, SeekOrigin.Begin);
			Serial serial = reader.ReadInt32();
			reader.Seek(pos, SeekOrigin.Begin);

			if (!serial.IsValid || !serial.IsItem)
			{
				return;
			}

			var item = World.FindItem(serial);
			var info = Lookup(state.Version, item);

			if (info == null)
			{
				return;
			}

			if (CSOptions.ServiceDebug)
			{
				CSOptions.ToConsole(
					"Rewriting packet buffer ItemID for '{0}' -> \n(0x{1:X4} -> 0x{2:X4})",
					item.GetType().Name,
					info.HighItemID,
					info.LowItemID);
			}

			info.SwitchWorldItem(item is BaseMulti, ref buffer);
			length = buffer.Length;
		}

		private static void HandleWorldItemSAHS(NetState state, PacketReader reader, ref byte[] buffer, ref int length)
		{
			if (_Parent0xF3 != null)
			{
				_Parent0xF3(state, reader, ref buffer, ref length);
			}

			if (state == null || reader == null || buffer == null)
			{
				return;
			}

			var pos = reader.Seek(0, SeekOrigin.Current);
			reader.Seek(4, SeekOrigin.Begin);
			Serial serial = reader.ReadInt32();
			reader.Seek(pos, SeekOrigin.Begin);

			if (!serial.IsValid || !serial.IsItem)
			{
				return;
			}

			var item = World.FindItem(serial);
			var info = Lookup(state.Version, item);

			if (info == null)
			{
				return;
			}

			if (CSOptions.ServiceDebug)
			{
				CSOptions.ToConsole(
					"Rewriting packet buffer ItemID for '{0}' -> \n(0x{1:X4} -> 0x{2:X4})",
					item.GetType().Name,
					info.HighItemID,
					info.LowItemID);
			}

			info.SwitchWorldItemSAHS(item is BaseMulti, ref buffer);
			length = buffer.Length;
		}

		public static void Register<TItem>(ClientVersion highVersion, ClientVersion lowVersion, int highItemID, int lowItemID)
			where TItem : Item
		{
			Register(typeof(TItem), highVersion, lowVersion, highItemID, lowItemID);
		}

		public static void Register(
			Type type,
			ClientVersion highVersion,
			ClientVersion lowVersion,
			int highItemID,
			int lowItemID)
		{
			List<ArtworkInfo> infoList;

			if (!Info.TryGetValue(type, out infoList) || infoList == null)
			{
				Info[type] = infoList = new List<ArtworkInfo>();
			}

			var info =
				infoList.Find(i => i.HighVersion == highVersion && i.LowVersion == lowVersion && i.HighItemID == highItemID);

			if (info != null)
			{
				if (CSOptions.ServiceDebug)
				{
					CSOptions.ToConsole(
						"Replacing ArtworkInfo for '{0}' -> \n({1} -> {2}, 0x{3:X4} -> 0x{4:X4}) with ({5} -> {6}, 0x{7:X4} -> 0x{8:X4})",
						type.Name,
						info.HighVersion,
						info.LowVersion,
						info.HighItemID,
						info.LowItemID,
						highVersion,
						lowVersion,
						highItemID,
						lowItemID);
				}

				info.HighItemID = highItemID;
				info.LowItemID = lowItemID;
			}
			else
			{
				if (CSOptions.ServiceDebug)
				{
					CSOptions.ToConsole(
						"Adding ArtworkInfo for '{0}' -> \n({1} -> {2}, 0x{3:X4} -> 0x{4:X4})",
						type.Name,
						highVersion,
						lowVersion,
						highItemID,
						lowItemID);
				}

				infoList.Add(new ArtworkInfo(highVersion, lowVersion, highItemID, lowItemID));
			}
		}

		public static ArtworkInfo Lookup(ClientVersion version, Item item)
		{
			return item != null ? Lookup(version, item.GetType(), item.ItemID) : null;
		}

		public static ArtworkInfo Lookup(ClientVersion version, Type type, int itemID)
		{
			var list = Info.GetValue(type);

			if (list == null || list.Count == 0)
			{
				return null;
			}

			return list.OrderBy(i => i.HighVersion).ThenBy(i => i.LowVersion).FirstOrDefault(i => i.CanSwitch(version));
		}

		public static bool SupportsArtwork(ClientVersion version, Item item)
		{
			return item != null && SupportsArtwork(version, item.GetType(), item.ItemID);
		}

		public static bool SupportsArtwork(ClientVersion version, Type type, int itemID)
		{
			var list = Info.GetValue(type);

			if (list == null || list.Count == 0)
			{
				return true;
			}

			return !list.OrderBy(i => i.HighVersion).ThenBy(i => i.LowVersion).Any(i => i.CanSwitch(version));
		}
	}
}