#region Header
//   Vorspire    _,-'/-'/  Client.cs
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
using System.Net.Sockets;

using Server;
using Server.Network;

using VitaNex.Crypto;
#endregion

namespace VitaNex.Modules.WebSockets
{
	public sealed class WebSocketsClientKey : CryptoHashCode
	{
		public override string Value { get { return base.Value.Replace("-", String.Empty); } }

		public WebSocketsClientKey(string key)
			: base(CryptoHashType.SHA1, String.Concat(key, "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))
		{ }

		public WebSocketsClientKey(GenericReader reader)
			: base(reader)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public sealed class WebSocketsClient : NetState
	{
		public WebSocketsClientKey Key { get; private set; }

		public TcpClient TcpClient { get; private set; }

		public bool Connected { get { return TcpClient != null && TcpClient.Connected; } }

		public WebSocketsClient(TcpClient client, MessagePump p)
			: base(client.Client, p)
		{
			TcpClient = client;
		}

		public WebSocketsClientKey ResolveKey(string key)
		{
			return Key ?? (Key = new WebSocketsClientKey(key));
		}

		/*
		public override void Dispose(bool flush)
		{
			base.Dispose(flush);
		}

		public override void Send(Packet p)
		{
			base.Send(p);
		}

		public override string ToString()
		{
			return base.ToString();
		}*/
		/*
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}*/
		/*
		public void Send(Packet p)
		{
			if (Connected)
			{
				NetState.Send(p);
			}
		}

		public void Flush()
		{
			if (Connected)
			{
				NetState.Flush();
			}
		}

		public override void Dispose()
		{
			VitaNexCore.TryCatch(() =>
			{
				if (!Connected)
				{
					return;
				}

				WebSockets.Disconnected(this);

				NetState.Dispose();
				NetState = null;
			}, e =>
			{
				lock (WebSockets.Clients)
				{
					WebSockets.Clients.Remove(this);
				}

				WebSockets.CMOptions.ToConsole(e);
			});
		}*/
	}
}