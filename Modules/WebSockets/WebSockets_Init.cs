#region Header
//   Vorspire    _,-'/-'/  WebSockets_Init.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;

using Server;
using Server.Network;
#endregion

namespace VitaNex.Modules.WebSockets
{
	[CoreModule("Web Sockets", "1.0.0.1")]
	public static partial class WebSockets
	{
		public static WebSocketsOptions CMOptions { get; private set; }

		static WebSockets()
		{
			CMOptions = new WebSocketsOptions();

			EventSink.ServerStarted += () => _Started = true;

			Clients = new List<WebSocketsClient>();

			OnConnected += HandleConnection;

			_ActivityTimer = PollTimer.FromSeconds(
				60.0,
				() =>
				{
					if (!_Listening || Listener == null || Listener.Server == null || !Listener.Server.IsBound)
					{
						_Listening = false;
						ListenAsync();
					}

					Clients.RemoveAll(c => !c.Connected);
				},
				() => CMOptions.ModuleEnabled && Clients.Count > 0);

			NetState.CreatedCallback += ns =>
			{
				if (ns is WebSocketsClient)
				{
					var client = (WebSocketsClient)ns;

					client.CompressionEnabled = false;
				}
			};
		}

		private static void CMInvoke()
		{
			ListenAsync();
		}

		private static void CMEnabled()
		{
			ListenAsync();
		}

		private static void CMDisabled()
		{
			ReleaseListener();
		}

		private static void CMSave()
		{ }

		private static void CMLoad()
		{ }

		private static void CMDisposed()
		{
			if (Listener == null)
			{
				return;
			}

			Listener.Stop();
			Listener = null;
		}
	}
}