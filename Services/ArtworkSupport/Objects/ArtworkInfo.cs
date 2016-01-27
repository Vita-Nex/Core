#region Header
//   Vorspire    _,-'/-'/  ArtworkInfo.cs
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

using Server;
using Server.Network;

using VitaNex.Network;
#endregion

namespace VitaNex
{
	public sealed class ArtworkInfo : PropertyObject
	{
		public ClientVersion HighVersion { get; set; }
		public ClientVersion LowVersion { get; set; }

		public int HighItemID { get; set; }
		public int LowItemID { get; set; }

		public ArtworkInfo(int highItemID, int lowItemID)
			: this(ArtworkSupport.DefaultHighVersion, highItemID, lowItemID)
		{ }

		public ArtworkInfo(ClientVersion highVersion, int highItemID, int lowItemID)
			: this(highVersion, ArtworkSupport.DefaultLowVersion, highItemID, lowItemID)
		{ }

		public ArtworkInfo(ClientVersion highVersion, ClientVersion lowVersion, int highItemID, int lowItemID)
		{
			HighVersion = highVersion ?? ArtworkSupport.DefaultHighVersion;
			LowVersion = lowVersion ?? ArtworkSupport.DefaultLowVersion;

			HighItemID = highItemID;
			LowItemID = lowItemID;
		}

		public ArtworkInfo(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			HighVersion = ArtworkSupport.DefaultHighVersion;
			LowVersion = ArtworkSupport.DefaultLowVersion;
		}

		public override void Reset()
		{
			HighVersion = ArtworkSupport.DefaultHighVersion;
			LowVersion = ArtworkSupport.DefaultLowVersion;
		}

		public bool RewriteID(bool multi, Packet p, int offset)
		{
			if (p == null || offset >= p.UnderlyingStream.Length)
			{
				return false;
			}

			var success = false;

			VitaNexCore.TryCatch(
				() =>
				{
					var v = p is WorldItem ? 0 : p is WorldItemSA ? 1 : p is WorldItemHS ? 2 : -1;
					var id = LowItemID;

					switch (v)
					{
						case 0: //Old
						{
							id &= 0x3FFF;

							if (multi)
							{
								id |= 0x4000;
							}

							success = p.Rewrite(offset, (short)id);
						}
							break;
						case 1: //SA
						{
							id &= multi ? 0x3FFF : 0x7FFF;
							success = p.Rewrite(offset, (short)id);
						}
							break;
						case 2: //HS
						{
							id &= multi ? 0x3FFF : 0xFFFF;
							success = p.Rewrite(offset, (ushort)id);
						}
							break;
						default:
							return;
					}
				},
				ArtworkSupport.CSOptions.ToConsole);

			return success;
		}

		public bool RewriteID(bool multi, ref byte[] buffer, int offset)
		{
			if (buffer == null || offset >= buffer.Length)
			{
				return false;
			}

			var b = buffer;
			var success = false;

			VitaNexCore.TryCatch(
				() =>
				{
					var v = b.Length == 23 ? 0 : b.Length == 24 ? 1 : b.Length == 26 ? 2 : -1;
					var id = LowItemID;

					switch (v)
					{
						case 0: //Old
						{
							id &= 0x3FFF;

							if (multi)
							{
								id |= 0x4000;
							}

							BitConverter.GetBytes((short)id).CopyTo(b, offset);
							success = true;
						}
							break;
						case 1: //SA
						{
							id &= multi ? 0x3FFF : 0x7FFF;
							BitConverter.GetBytes((short)id).CopyTo(b, offset);
							success = true;
						}
							break;
						case 2: //HS
						{
							id &= multi ? 0x3FFF : 0xFFFF;
							BitConverter.GetBytes((ushort)id).CopyTo(b, offset);
							success = true;
						}
							break;
					}
				},
				ArtworkSupport.CSOptions.ToConsole);

			return success;
		}

		public bool SwitchID(bool multi, WorldItem p)
		{
			return RewriteID(multi, p, 7);
		}

		public bool SwitchID(bool multi, WorldItemSA p)
		{
			return RewriteID(multi, p, 8);
		}

		public bool SwitchID(bool multi, WorldItemHS p)
		{
			return RewriteID(multi, p, 8);
		}

		public bool SwitchWorldItem(bool multi, ref byte[] buffer)
		{
			return RewriteID(multi, ref buffer, 7);
		}

		public bool SwitchWorldItemSAHS(bool multi, ref byte[] buffer)
		{
			return RewriteID(multi, ref buffer, 8);
		}

		public bool CanSwitch(ClientVersion query)
		{
			return query < HighVersion && query >= LowVersion;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
				{
					writer.Write(HighVersion.SourceString);
					writer.Write(LowVersion.SourceString);

					writer.Write(HighItemID);
					writer.Write(LowItemID);
				}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.GetVersion();

			switch (version)
			{
				case 0:
				{
					HighVersion = new ClientVersion(reader.ReadString());
					LowVersion = new ClientVersion(reader.ReadString());

					HighItemID = reader.ReadInt();
					LowItemID = reader.ReadInt();
				}
					break;
			}
		}
	}
}