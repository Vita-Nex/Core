#region Header
//   Vorspire    _,-'/-'/  PacketExt.cs
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

using Server.Network;
#endregion

namespace VitaNex.Network
{
	public static class PacketExtUtility
	{
		public static bool RewriteItemID(this WorldItem p, int itemID, bool reset = true)
		{
			return Rewrite(p, 7, (short)itemID, reset);
		}

		public static bool RewriteItemID(this WorldItemSA p, int itemID, bool reset = true)
		{
			return Rewrite(p, 8, (short)itemID, reset);
		}

		public static bool RewriteItemID(this WorldItemHS p, int itemID, bool reset = true)
		{
			return Rewrite(p, 8, (ushort)itemID, reset);
		}

		public static bool RewriteBody(this MobileIncomingOld p, int itemID, bool reset = true)
		{
			return Rewrite(p, 8, (ushort)itemID, reset);
		}

		public static bool Rewrite(this Packet p, int offset, bool value, bool reset = true)
		{
			if (p == null || p.UnderlyingStream == null || offset < 0)
			{
				return false;
			}

			var success = false;

			VitaNexCore.TryCatch(
				() =>
				{
					var o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}

					success = true;
				});

			return success;
		}

		public static bool Rewrite(this Packet p, int offset, byte value, bool reset = true)
		{
			if (p == null || p.UnderlyingStream == null || offset < 0)
			{
				return false;
			}

			var success = false;

			VitaNexCore.TryCatch(
				() =>
				{
					var o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}

					success = true;
				});

			return success;
		}

		public static bool Rewrite(this Packet p, int offset, sbyte value, bool reset = true)
		{
			if (p == null || p.UnderlyingStream == null || offset < 0)
			{
				return false;
			}

			var success = false;

			VitaNexCore.TryCatch(
				() =>
				{
					var o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}

					success = true;
				});

			return success;
		}

		public static bool Rewrite(this Packet p, int offset, short value, bool reset = true)
		{
			if (p == null || p.UnderlyingStream == null || offset < 0)
			{
				return false;
			}

			var success = false;

			VitaNexCore.TryCatch(
				() =>
				{
					var o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}

					success = true;
				});

			return success;
		}

		public static bool Rewrite(this Packet p, int offset, ushort value, bool reset = true)
		{
			if (p == null || p.UnderlyingStream == null || offset < 0)
			{
				return false;
			}

			var success = false;

			VitaNexCore.TryCatch(
				() =>
				{
					var o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}

					success = true;
				});

			return success;
		}

		public static bool Rewrite(this Packet p, int offset, int value, bool reset = true)
		{
			if (p == null || p.UnderlyingStream == null || offset < 0)
			{
				return false;
			}

			var success = false;

			VitaNexCore.TryCatch(
				() =>
				{
					var o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}

					success = true;
				});

			return success;
		}

		public static bool Rewrite(this Packet p, int offset, uint value, bool reset = true)
		{
			if (p == null || p.UnderlyingStream == null || offset < 0)
			{
				return false;
			}

			var success = false;

			VitaNexCore.TryCatch(
				() =>
				{
					var o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}

					success = true;
				});

			return success;
		}

		public static bool Rewrite(this Packet p, int offset, byte[] value, bool reset = true)
		{
			if (p == null || offset < 0)
			{
				return false;
			}

			var success = false;

			VitaNexCore.TryCatch(
				() =>
				{
					if (p.UnderlyingStream != null)
					{
						var o = p.UnderlyingStream.Position;

						p.UnderlyingStream.Position = offset;
						p.UnderlyingStream.Write(value, 0, value.Length);

						if (reset)
						{
							p.UnderlyingStream.Position = o;
						}
					}
					else
					{
						byte[] buffer;

						if (p.GetFieldValue("m_CompiledBuffer", out buffer) && buffer != null)
						{
							Buffer.BlockCopy(value, 0, buffer, offset, value.Length);

							success = p.SetFieldValue("m_CompiledBuffer", buffer);
						}
					}
				});

			return success;
		}
	}
}