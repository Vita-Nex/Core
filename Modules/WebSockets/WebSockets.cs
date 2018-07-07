#region Header
//   Vorspire    _,-'/-'/  WebSockets.cs
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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

using Server;
using Server.Misc;
#endregion

namespace VitaNex.Modules.WebSockets
{
	public static partial class WebSockets
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		private static bool _Started;

		private static PollTimer _ActivityTimer;

		public static TcpListener Listener { get; private set; }
		public static List<WebSocketsClient> Clients { get; private set; }

		public static event Action<WebSocketsClient> OnConnected;
		public static event Action<WebSocketsClient> OnDisconnected;

		private static readonly MethodInfo _IsPrivateNetwork = //
			typeof(ServerList).GetMethod("IsPrivateNetwork", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

		private static void AcquireListener()
		{
			if (!CMOptions.ModuleEnabled)
			{
				ReleaseListener();
				return;
			}

			if (Listener != null && ((IPEndPoint)Listener.LocalEndpoint).Port != CMOptions.Port)
			{
				ReleaseListener();
			}

			if (Listener == null)
			{
				var address = NetworkInterface.GetAllNetworkInterfaces()
											  .Select(adapter => adapter.GetIPProperties())
											  .Select(
												  properties => properties.UnicastAddresses.Select(unicast => unicast.Address)
																		  .FirstOrDefault(
																			  ip => !IPAddress.IsLoopback(ip) && ip.AddressFamily != AddressFamily.InterNetworkV6 &&
																					(_IsPrivateNetwork == null || (bool)_IsPrivateNetwork.Invoke(null, new object[] {ip}))))
											  .FirstOrDefault() ?? IPAddress.Any;

				Listener = new TcpListener(address, CMOptions.Port);
			}

			if (!Listener.Server.IsBound)
			{
				Listener.Start(CMOptions.MaxConnections);

				CMOptions.ToConsole("Listening: {0}", Listener.LocalEndpoint);
			}

			_Listening = true;
		}

		private static void ReleaseListener()
		{
			if (Listener == null)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() =>
				{
					if (Listener.Server.IsBound)
					{
						Listener.Server.Disconnect(true);
					}
				});

			VitaNexCore.TryCatch(Listener.Stop);

			Listener = null;

			_Listening = false;
		}

		private static bool _Listening;

		private static void ListenAsync()
		{
			AcquireListener();

			if (Listener == null)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() => Listener.BeginAcceptTcpClient(
					r =>
					{
						var client = VitaNexCore.TryCatchGet(() => Listener.EndAcceptTcpClient(r), CMOptions.ToConsole);

						if (client != null && client.Connected)
						{
							VitaNexCore.TryCatch(() => Connected(client), CMOptions.ToConsole);
						}

						ListenAsync();
					},
					null),
				e =>
				{
					_Listening = false;
					CMOptions.ToConsole(e);
				});
		}

		private static void Connected(TcpClient tcp)
		{
			if (tcp == null)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() =>
				{
					if (Listener != null && _Started)
					{
						Connected(new WebSocketsClient(tcp, Core.MessagePump));
					}
					else
					{
						tcp.Close();
					}
				},
				CMOptions.ToConsole);
		}

		private static void Connected(WebSocketsClient client)
		{
			lock (Clients)
			{
				if (!Clients.Contains(client))
				{
					Clients.Add(client);
				}
			}

			CMOptions.ToConsole("[{0}] Client connected: {1}", Clients.Count, client.Address);

			if (OnConnected != null)
			{
				VitaNexCore.TryCatch(
					() => OnConnected(client),
					e =>
					{
						CMOptions.ToConsole(e);

						client.Dispose();
						Disconnected(client);
					});
			}
		}

		private static void Disconnected(WebSocketsClient client)
		{
			if (OnDisconnected != null)
			{
				VitaNexCore.TryCatch(() => OnDisconnected(client), CMOptions.ToConsole);
			}

			lock (Clients)
			{
				Clients.Remove(client);
			}

			CMOptions.ToConsole("[{0}] Client disconnected: {1}", Clients.Count, client.Address);

			client.Dispose();
		}

		private static void Encode(string data, out byte[] buffer, out int length)
		{
			length = Encoding.UTF8.GetByteCount(data);
			buffer = new byte[length];

			Encoding.UTF8.GetBytes(data, 0, data.Length, buffer, 0);
		}

		private static void Decode(byte[] src, out string data)
		{
			data = Encoding.UTF8.GetString(src);
		}

		private static void Compress(ref byte[] buffer, ref int length)
		{
			using (MemoryStream inS = new MemoryStream(buffer.Take(length).ToArray()), outS = new MemoryStream())
			{
				using (var ds = new DeflateStream(outS, CompressionMode.Compress))
				{
					inS.CopyTo(ds);

					outS.Position = 0;
				}

				buffer = outS.ToArray();
				length = buffer.Length;
			}
		}

		private static void Decompress(ref byte[] buffer, ref int length)
		{
			using (MemoryStream inS = new MemoryStream(buffer.Take(length).ToArray()), outS = new MemoryStream())
			{
				using (var ds = new DeflateStream(inS, CompressionMode.Decompress))
				{
					ds.CopyTo(outS);

					outS.Position = 0;
				}

				buffer = outS.ToArray();
				length = buffer.Length;
			}
		}

		private static void Send(
			WebSocketsClient client,
			string data,
			bool encode,
			bool compress,
			Action<WebSocketsClient, byte[]> callback)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					int len;
					byte[] buffer;

					if (encode)
					{
						Encode(data, out buffer, out len);
					}
					else
					{
						buffer = data.Select(c => (byte)c).ToArray();
						len = buffer.Length;
					}

					Send(client, buffer, len, compress, callback);
				},
				CMOptions.ToConsole);
		}

		private static void Send(
			WebSocketsClient client,
			byte[] buffer,
			int len,
			bool compress,
			Action<WebSocketsClient, byte[]> callback)
		{
			var stream = client.TcpClient.GetStream();

			if (compress)
			{
				Compress(ref buffer, ref len);
			}

			var count = 0;

			while (count < len)
			{
				var block = buffer.Skip(count).Take(client.TcpClient.SendBufferSize).ToArray();

				stream.Write(block, 0, block.Length);

				count += block.Length;
			}

			if (callback != null)
			{
				callback(client, buffer);
			}
		}

		private static void Receive(
			WebSocketsClient client,
			bool decompress,
			bool decode,
			Action<WebSocketsClient, string, byte[]> callback)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					var stream = client.TcpClient.GetStream();

					var buffer = new byte[client.TcpClient.ReceiveBufferSize];
					var len = buffer.Length;

					stream.Read(buffer, 0, buffer.Length);

					if (decompress)
					{
						Decompress(ref buffer, ref len);
					}

					string data;

					if (decode)
					{
						Decode(buffer, out data);
					}
					else
					{
						data = new String(buffer.Select(b => (char)b).ToArray());
					}

					if (callback != null)
					{
						callback(client, data, buffer);
					}
				},
				CMOptions.ToConsole);
		}

		private static void HandleConnection(WebSocketsClient client)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					if (client.Seeded)
					{
						return;
					}

					var headers = new Dictionary<string, string>();

					Receive(
						client,
						false,
						true,
						(c, d, b) =>
						{
							if (d.Length == 0)
							{
								return;
							}

							var lines = d.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

							lines = lines.Take(lines.Length - 1).ToArray();

							if (CMOptions.ModuleDebug)
							{
								CMOptions.ToConsole(lines.Not(String.IsNullOrWhiteSpace).ToArray());
							}

							lines.ForEach(
								line =>
								{
									line = line.Trim();

									var header = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

									if (header.Length == 0)
									{
										return;
									}

									var hk = header[0].Replace(":", String.Empty);

									if (String.IsNullOrWhiteSpace(hk))
									{
										return;
									}

									var hv = header.Length > 1 ? String.Join(" ", header.Skip(1)) : String.Empty;

									if (!headers.ContainsKey(hk))
									{
										headers.Add(hk, hv);
									}
									else
									{
										headers[hk] = hv;
									}
								});
						});

					if (headers.Count > 0)
					{
						HandleHttpRequest(client, headers);
					}
					else
					{
						throw new Exception("No headers defined for WebSockets client handshake.", new SocketException());
					}
				},
				CMOptions.ToConsole);
		}

		private static void HandleHttpRequest(WebSocketsClient client, Dictionary<string, string> headers)
		{
			//var uri = headers["GET"];
			//var origin = headers["Origin"];

			var key = client.ResolveKey(headers["Sec-WebSocket-Key"]);

			var answer = Convert.ToBase64String(Encoding.ASCII.GetBytes(key.Value));

			var sendHeaders = new List<string>
			{
				"HTTP/1.1 101 Switching Protocols", //
				"Connection: Upgrade", //
				"Sec-WebSocket-Accept: " + answer, //
				"Upgrade: websocket" //
			};

			Send(client, String.Join("\r\n", sendHeaders) + "\r\n\r\n", false, false, (c, d) => client.Start());

			if (!CMOptions.ModuleDebug)
			{
				return;
			}

			CMOptions.ToConsole("HEADERS>>>\n");
			CMOptions.ToConsole(sendHeaders.ToArray());
		}
	}
}