#region Header
//   Vorspire    _,-'/-'/  Notify.cs
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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Server;
using Server.Commands;
using Server.Network;

using VitaNex.IO;
using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Notify
{
	public static partial class Notify
	{
		public static CoreServiceOptions CSOptions { get; private set; }

		public static Type[] GumpTypes { get; private set; }

		public static Dictionary<Type, Type[]> NestedTypes { get; private set; }

		public static BinaryDataStore<Type, NotifySettings> Settings { get; private set; }

		[Usage("Notify <text | html | bbc>"), Description("Send a global notification gump to all online clients, " + //
														  "containing a message parsed from HTML, BBS or plain text.")]
		private static void HandleNotify(CommandEventArgs e)
		{
			if (ValidateCommand(e))
			{
				Broadcast(e.Mobile, e.ArgString, false, true);
			}
		}

		[Usage("NotifyAC <text | html | bbc>"), Description("Send a global notification gump to all online clients, " + //
															"containing a message parsed from HTML, BBS or plain text, " + //
															"which auto-closes after 10 seconds.")]
		private static void HandleNotifyAC(CommandEventArgs e)
		{
			if (ValidateCommand(e))
			{
				Broadcast(e.Mobile, e.ArgString, true, true);
			}
		}

		[Usage("NotifyNA <text | html | bbc>"), Description("Send a global notification gump to all online clients, " + //
															"containing a message parsed from HTML, BBS or plain text, " + //
															"which has no animation delay.")]
		private static void HandleNotifyNA(CommandEventArgs e)
		{
			if (ValidateCommand(e))
			{
				Broadcast(e.Mobile, e.ArgString, false, false);
			}
		}

		[Usage("NotifyACNA <text | html | bbc>"), Description("Send a global notification gump to all online clients, " + //
															  "containing a message parsed from HTML, BBS or plain text, " + //
															  "which auto-closes after 10 seconds and has no animation delay.")]
		private static void HandleNotifyACNA(CommandEventArgs e)
		{
			if (ValidateCommand(e))
			{
				Broadcast(e.Mobile, e.ArgString, true, false);
			}
		}

		private static bool ValidateCommand(CommandEventArgs e)
		{
			if (e == null || e.Mobile == null)
			{
				return false;
			}

			if (String.IsNullOrWhiteSpace(e.ArgString) ||
				String.IsNullOrWhiteSpace(Regex.Replace(e.ArgString, @"<[^>]*>", String.Empty)))
			{
				e.Mobile.SendMessage(0x22, "Html/BBC message must be at least 1 character and not all white-space after parsing.");
				e.Mobile.SendMessage(0x22, "Usage: {0}{1} <text | html | bbc>", CommandSystem.Prefix, e.Command);
				return false;
			}

			return true;
		}

		public static NotifySettings EnsureSettings<TGump>() where TGump : NotifyGump
		{
			return EnsureSettings(typeof(TGump));
		}

		public static NotifySettings EnsureSettings(Type t)
		{
			if (t == null || !t.IsEqualOrChildOf<NotifyGump>())
			{
				return null;
			}

			NotifySettings settings = null;
			var init = false;

			Settings.AddOrReplace(
				t,
				s =>
				{
					init = true;
					return settings = s ?? new NotifySettings(t);
				});

			if (init && settings != null)
			{
				var m = t.GetMethod("InitSettings", BindingFlags.Static | BindingFlags.NonPublic);

				if (m != null)
				{
					m.Invoke(null, new object[] {settings});
				}
			}

			return settings;
		}

		public static bool IsIgnored<TGump>(Mobile pm) where TGump : NotifyGump
		{
			return IsIgnored(typeof(TGump), pm);
		}

		public static bool IsIgnored(Type t, Mobile pm)
		{
			var settings = EnsureSettings(t);

			return settings != null && settings.IsIgnored(pm);
		}

		public static bool IsAnimated<TGump>(Mobile pm) where TGump : NotifyGump
		{
			return IsAnimated(typeof(TGump), pm);
		}

		public static bool IsAnimated(Type t, Mobile pm)
		{
			var settings = EnsureSettings(t);

			return settings == null || settings.IsAnimated(pm);
		}

		public static void Broadcast(Mobile m, string message, AccessLevel level = AccessLevel.Player)
		{
			Broadcast(m, message, true, level);
		}

		public static void Broadcast(Mobile m, string message, bool autoClose, AccessLevel level = AccessLevel.Player)
		{
			Broadcast(m, message, autoClose, true, level);
		}

		public static void Broadcast(
			Mobile m,
			string message,
			bool autoClose,
			bool animate,
			AccessLevel level = AccessLevel.Player)
		{
			if (m != null && !m.Deleted)
			{
				Broadcast(String.Format("{0}:\n{1}", m.RawName, message), autoClose, animate, level);
			}
		}

		public static void Broadcast(string message, AccessLevel level = AccessLevel.Player)
		{
			Broadcast(message, false, level);
		}

		public static void Broadcast(string message, bool autoClose, AccessLevel level = AccessLevel.Player)
		{
			Broadcast(message, autoClose, false, level);
		}

		public static void Broadcast(string message, bool autoClose, bool animate, AccessLevel level = AccessLevel.Player)
		{
			message = String.Format("[{0}]:\n{1}", DateTime.Now.ToSimpleString("t@h:m@"), message);

			Broadcast<WorldNotifyGump>(message, autoClose, animate ? 1.0 : 0.0, level: level);
		}

		public static void Broadcast<TGump>(
			string html,
			bool autoClose = true,
			double delay = 1.0,
			double pause = 3.0,
			Color? color = null,
			Action<TGump> beforeSend = null,
			Action<TGump> afterSend = null,
			AccessLevel level = AccessLevel.Player) where TGump : NotifyGump
		{
			var c = NetState.Instances.Count;

			while (--c >= 0)
			{
				if (!NetState.Instances.InBounds(c))
				{
					continue;
				}

				var ns = NetState.Instances[c];

				if (ns != null && ns.Running && ns.Mobile != null && ns.Mobile.AccessLevel >= level)
				{
					VitaNexCore.TryCatch(m => Send(m, html, autoClose, delay, pause, color, beforeSend, afterSend, level), ns.Mobile);
				}
			}
		}

		public static void Send(
			Mobile m,
			string html,
			bool autoClose = true,
			double delay = 1.0,
			double pause = 3.0,
			Color? color = null,
			Action<NotifyGump> beforeSend = null,
			Action<NotifyGump> afterSend = null,
			AccessLevel level = AccessLevel.Player)
		{
			Send<NotifyGump>(m, html, autoClose, delay, pause, color, beforeSend, afterSend, level);
		}

		public static void Send<TGump>(
			Mobile m,
			string html,
			bool autoClose = true,
			double delay = 1.0,
			double pause = 3.0,
			Color? color = null,
			Action<TGump> beforeSend = null,
			Action<TGump> afterSend = null,
			AccessLevel level = AccessLevel.Player) where TGump : NotifyGump
		{
			if (!m.IsOnline() || m.AccessLevel < level)
			{
				return;
			}

			var t = typeof(TGump);

			if (t.IsAbstract || m.HasGump(t))
			{
				Type[] subs;

				if (!NestedTypes.TryGetValue(t, out subs) || subs == null)
				{
					NestedTypes[t] = subs = t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic) //
											 .Where(st => st.IsChildOf<NotifyGump>()).ToArray();
				}

				var sub = subs.FirstOrDefault(st => !m.HasGump(st));

				if (sub != null)
				{
					t = sub;
				}
			}

			if (!t.IsAbstract)
			{
				if (IsIgnored(t.IsNested ? t.DeclaringType : t, m))
				{
					return;
				}

				if (!IsAnimated(t.IsNested ? t.DeclaringType : t, m))
				{
					delay = 0.0;
				}

				var ng = t.CreateInstanceSafe<TGump>(m, html);

				if (ng != null)
				{
					ng.AutoClose = autoClose;
					ng.AnimDuration = TimeSpan.FromSeconds(Math.Max(0, delay));
					ng.PauseDuration = TimeSpan.FromSeconds(Math.Max(0, pause));
					ng.HtmlColor = color ?? Color.White;

					if (beforeSend != null)
					{
						beforeSend(ng);
					}

					if (ng.IsDisposed)
					{
						return;
					}

					ng.Send();

					if (afterSend != null)
					{
						afterSend(ng);
					}

					return;
				}
			}

			foreach (var str in
				html.Split(new[] {"\n", "<br>", "<BR>"}, StringSplitOptions.RemoveEmptyEntries)
					.Select(s => Regex.Replace(s, @"<[^>]*>", String.Empty)))
			{
				m.SendMessage(str);
			}
		}
	}
}