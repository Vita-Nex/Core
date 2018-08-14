#region Header
//   Vorspire    _,-'/-'/  WebStats.cs
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

using Server;
using Server.Guilds;
using Server.Items;
using Server.Misc;
using Server.Network;

using VitaNex.IO;
using VitaNex.Text;
using VitaNex.Web;
#endregion

namespace VitaNex.Modules.WebStats
{
	public static partial class WebStats
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		private static DateTime _LastUpdate = DateTime.MinValue;
		private static WebStatsRequestFlags _LastFlags = WebStatsRequestFlags.None;

		private static readonly Dictionary<string, object> _Json;

		private static bool _UpdatingJson;
		private static bool _Updating;

		public static WebStatsOptions CMOptions { get; private set; }

		public static BinaryDataStore<string, WebStatsEntry> Stats { get; private set; }

		public static Dictionary<IPAddress, List<Mobile>> Snapshot { get; private set; }

		private static void HandleWebRequest(WebAPIContext context)
		{
			if (!CMOptions.ModuleEnabled)
			{
				return;
			}

			var flags = WebStatsRequestFlags.Server | WebStatsRequestFlags.Stats;

			if (context.Request.Queries.Count > 0)
			{
				if (context.Request.Queries["flags"] != null)
				{
					var f = context.Request.Queries["flags"];
					int v;

					if (f.StartsWith("0x"))
					{
						if (!Int32.TryParse(f.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v) || v < 0)
						{
							v = 0;
						}
					}
					else if (!Int32.TryParse(f, out v) || v < 0)
					{
						v = 0;
					}

					flags = (WebStatsRequestFlags)v;
				}
				else
				{
					bool? server = null, stats = null;

					foreach (var q in context.Request.Queries)
					{
						var value = !q.Value.EqualsAny(true, "false", "no", "off", "disabled", "0", String.Empty);

						if (Insensitive.Equals(q.Key, "server"))
						{
							server = value;
						}
						else if (Insensitive.Equals(q.Key, "stats"))
						{
							stats = value;
						}
					}

					if (server != null && !server.Value)
					{
						flags &= ~WebStatsRequestFlags.Server;
					}

					if (stats != null && !stats.Value)
					{
						flags &= ~WebStatsRequestFlags.Stats;
					}
				}
			}

			context.Response.Data = GetJson(flags, false);
		}

		public static bool UpdateStats(bool forceUpdate)
		{
			if (_Updating)
			{
				return false;
			}

			if (!forceUpdate && _LastFlags == CMOptions.RequestFlags && _LastUpdate + CMOptions.UpdateInterval > DateTime.UtcNow)
			{
				return false;
			}

			_Updating = true;

			_LastUpdate = DateTime.UtcNow;
			_LastFlags = CMOptions.RequestFlags;

			var states = NetState.Instances.Where(ns => ns != null && ns.Socket != null && ns.Mobile != null).ToArray();

			Snapshot.Clear();

			foreach (var ns in states)
			{
				var ep = (IPEndPoint)ns.Socket.RemoteEndPoint;

				List<Mobile> ch;

				if (!Snapshot.TryGetValue(ep.Address, out ch) || ch == null)
				{
					Snapshot[ep.Address] = ch = new List<Mobile>();
				}

				ch.Add(ns.Mobile);
			}

			TimeSpan time;
			long lnum;
			int num;

			#region Uptime
			var uptime = DateTime.UtcNow - Clock.ServerStart;

			Stats["uptime"].Value = uptime;
			Stats["uptime_peak"].Value = Stats["uptime_peak"].TryCast(out time)
				? TimeSpan.FromSeconds(Math.Max(time.TotalSeconds, uptime.TotalSeconds))
				: uptime;
			#endregion

			#region Online
			var connected = states.Length;

			Stats["online"].Value = connected;
			Stats["online_max"].Value = Stats["online_max"].TryCast(out num) ? Math.Max(num, connected) : connected;
			Stats["online_peak"].Value = Stats["online_peak"].TryCast(out num) ? Math.Max(num, connected) : connected;
			#endregion

			#region Unique
			var unique = Snapshot.Count;

			Stats["unique"].Value = unique;
			Stats["unique_max"].Value = Stats["unique_max"].TryCast(out num) ? Math.Max(num, unique) : unique;
			Stats["unique_peak"].Value = Stats["unique_peak"].TryCast(out num) ? Math.Max(num, unique) : unique;
			#endregion

			#region Items
			var items = World.Items.Count;

			Stats["items"].Value = items;
			Stats["items_max"].Value = Stats["items_max"].TryCast(out num) ? Math.Max(num, items) : items;
			Stats["items_peak"].Value = Stats["items_peak"].TryCast(out num) ? Math.Max(num, items) : items;
			#endregion

			#region Mobiles
			var mobiles = World.Mobiles.Count;

			Stats["mobiles"].Value = mobiles;
			Stats["mobiles_max"].Value = Stats["mobiles_max"].TryCast(out num) ? Math.Max(num, mobiles) : mobiles;
			Stats["mobiles_peak"].Value = Stats["mobiles_peak"].TryCast(out num) ? Math.Max(num, mobiles) : mobiles;
			#endregion

			#region Guilds
			var guilds = BaseGuild.List.Count;

			Stats["guilds"].Value = guilds;
			Stats["guilds_max"].Value = Stats["guilds_max"].TryCast(out num) ? Math.Max(num, guilds) : guilds;
			Stats["guilds_peak"].Value = Stats["guilds_peak"].TryCast(out num) ? Math.Max(num, guilds) : guilds;
			#endregion

			#region Misc
			var ram = GC.GetTotalMemory(false);

			Stats["memory"].Value = ram;
			Stats["memory_max"].Value = Stats["memory_max"].TryCast(out lnum) ? Math.Max(lnum, ram) : ram;
			Stats["memory_peak"].Value = Stats["memory_peak"].TryCast(out lnum) ? Math.Max(lnum, ram) : ram;
			#endregion

			_Updating = false;

			return true;
		}

		private static Dictionary<string, object> GetJson(WebStatsRequestFlags flags, bool forceUpdate)
		{
			var root = new Dictionary<string, object>();

			if (UpdateJson(forceUpdate))
			{
				if (flags != WebStatsRequestFlags.None && flags != WebStatsRequestFlags.All)
				{
					if (flags.HasFlag(WebStatsRequestFlags.Server))
					{
						root["server"] = _Json.GetValue("server");
					}

					if (flags.HasFlag(WebStatsRequestFlags.Stats))
					{
						root["stats"] = _Json.GetValue("stats");
					}
				}
			}

			return root;
		}

		private static bool UpdateJson(bool forceUpdate)
		{
			if (_UpdatingJson || !UpdateStats(forceUpdate))
			{
				return false;
			}

			_UpdatingJson = true;

			VitaNexCore.TryCatch(
				() =>
				{
					_Json["vnc_version"] = VitaNexCore.Version.Value;
					_Json["mod_version"] = CMOptions.ModuleVersion;

					_Json["last_update"] = _LastUpdate.ToString(CultureInfo.InvariantCulture);
					_Json["last_update_stamp"] = _LastUpdate.ToTimeStamp().Stamp;

					if (CMOptions.DisplayServer)
					{
						var server = _Json.Intern("server", o => o as Dictionary<string, object> ?? new Dictionary<string, object>());

						server["name"] = ServerList.ServerName;

						var ipep = Listener.EndPoints.LastOrDefault();

						if (ipep != null)
						{
							if (ipep.Address.Equals(IPAddress.Any) || ipep.Address.Equals(IPAddress.IPv6Any))
							{
								foreach (var ip in ipep.Address.FindInternal())
								{
									server["host"] = ip.ToString();
									server["port"] = ipep.Port;
									break;
								}
							}
							else
							{
								server["host"] = ipep.Address.ToString();
								server["port"] = ipep.Port;
							}
						}

						server["os"] = Environment.OSVersion.VersionString;
						server["net"] = Environment.Version.ToString();

#if SERVUO
						server["core"] = "ServUO";
#elif JUSTUO
						server["core"] = "JustUO";
#else
						server["core"] = "RunUO";
#endif

						var an = Core.Assembly.GetName();

						server["assembly"] = an.Name;
						server["assembly_version"] = an.Version.ToString();

						_Json["server"] = server;
					}
					else
					{
						var server = _Json.Intern("server", o => o as Dictionary<string, object>);

						if (server != null)
						{
							server.Clear();
						}

						_Json.Remove("server");
					}

					if (CMOptions.DisplayStats)
					{
						var stats = _Json.Intern("stats", o => o as Dictionary<string, object> ?? new Dictionary<string, object>());

						foreach (var kv in Stats)
						{
							var o = kv.Value.Value;

							if (o is DateTime)
							{
								var dt = (DateTime)o;

								stats[kv.Key] = dt.ToString(CultureInfo.InvariantCulture);
								stats[kv.Key + "_stamp"] = Math.Floor(dt.ToTimeStamp().Stamp);
							}
							else if (o is TimeSpan)
							{
								var ts = (TimeSpan)o;

								stats[kv.Key] = ts.ToString();
								stats[kv.Key + "_stamp"] = ts.TotalSeconds;
							}
							else
							{
								stats[kv.Key] = o;
							}
						}

						_Json["stats"] = stats;
					}
					else
					{
						var stats = _Json.Intern("stats", o => o as Dictionary<string, object>);

						if (stats != null)
						{
							stats.Clear();
						}

						_Json.Remove("stats");
					}

					File.WriteAllText(
						IOUtility.GetSafeFilePath(VitaNexCore.CacheDirectory + "/WebStats.json", true),
						Json.Encode(_Json));
				},
				CMOptions.ToConsole);

			_UpdatingJson = false;

			return true;
		}
	}
}