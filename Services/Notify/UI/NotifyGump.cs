#region Header
//   Vorspire    _,-'/-'/  NotifyGump.cs
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

using Server;
using Server.Gumps;

using VitaNex.SuperGumps;
using VitaNex.Text;
#endregion

namespace VitaNex.Notify
{
	public class NotifyGumpOption
	{
		public TextDefinition Label { get; set; }
		public Action<GumpButton> Callback { get; set; }

		public Color LabelColor { get; set; }
		public Color FillColor { get; set; }
		public Color BorderColor { get; set; }

		public int Width { get { return Label.GetString().ComputeWidth(UOFont.Font0); } }

		public NotifyGumpOption(TextDefinition label, Action<GumpButton> callback)
			: this(label, callback, Color.Empty)
		{ }

		public NotifyGumpOption(TextDefinition label, Action<GumpButton> callback, Color color)
			: this(label, callback, color, Color.Empty)
		{ }

		public NotifyGumpOption(TextDefinition label, Action<GumpButton> callback, Color color, Color fill)
			: this(label, callback, color, fill, Color.Empty)
		{ }

		public NotifyGumpOption(TextDefinition label, Action<GumpButton> callback, Color color, Color fill, Color border)
		{
			Label = label;
			Callback = callback;

			LabelColor = color;
			FillColor = fill;
			BorderColor = border;
		}
	}

	public abstract class NotifyGump : SuperGump
	{
		public static event Action<NotifyGump> OnNotify;

		public static TimeSpan DefaultAnimDuration = TimeSpan.FromMilliseconds(500.0);
		public static TimeSpan DefaultPauseDuration = TimeSpan.FromSeconds(5.0);

		public static Size SizeMin = new Size(100, 60);
		public static Size SizeMax = new Size(400, 200);

		public enum AnimState
		{
			Show,
			Hide,
			Pause
		}
		
		public TimeSpan AnimDuration { get; set; }
		public TimeSpan PauseDuration { get; set; }

		public string Html { get; set; }
		public Color HtmlColor { get; set; }
		public int HtmlIndent { get; set; }

		public int BorderID { get; set; }
		public int BorderSize { get; set; }
		public bool BorderAlpha { get; set; }

		public int BackgroundID { get; set; }
		public bool BackgroundAlpha { get; set; }

		public int WidthMax { get; set; }
		public int HeightMax { get; set; }

		public bool AutoClose { get; set; }

		public int Frame { get; private set; }
		public AnimState State { get; private set; }

		public int FrameCount { get { return (int)Math.Ceiling(Math.Max(100.0, AnimDuration.TotalMilliseconds) / 100.0); } }
		public int FrameHeight { get; private set; }
		public int FrameWidth { get; private set; }

		public List<NotifyGumpOption> Options { get; private set; }

		public int OptionsCols { get; private set; }
		public int OptionsRows { get; private set; }

		public override bool InitPolling { get { return true; } }

		public NotifyGump(Mobile user, string html)
			: this(user, html, null)
		{ }
		
		public NotifyGump(Mobile user, string html, IEnumerable<NotifyGumpOption> options)
			: base(user, null, 0, 140)
		{
			Options = options.Ensure().ToList();

			AnimDuration = DefaultAnimDuration;
			PauseDuration = DefaultPauseDuration;

			Html = html ?? String.Empty;
			HtmlColor = Color.White;
			HtmlIndent = 10;

			BorderSize = 4;
			BorderID = 9204;
			BorderAlpha = false;

			BackgroundID = 2624;
			BackgroundAlpha = true;

			AutoClose = true;

			Frame = 0;
			State = AnimState.Pause;

			CanMove = false;
			CanResize = false;

			CloseSound = -1;

			ForceRecompile = true;
			AutoRefreshRate = TimeSpan.FromMilliseconds(100.0);
			AutoRefresh = true;

			WidthMax = 250;
			HeightMax = 90;
		}

		public void AddOption(TextDefinition label, Action<GumpButton> callback)
		{
			AddOption(label, callback, Color.Empty);
		}

		public void AddOption(TextDefinition label, Action<GumpButton> callback, Color color)
		{
			AddOption(label, callback, color, Color.Empty);
		}

		public void AddOption(TextDefinition label, Action<GumpButton> callback, Color color, Color fill)
		{
			AddOption(label, callback, color, fill, Color.Empty);
		}

		public void AddOption(TextDefinition label, Action<GumpButton> callback, Color color, Color fill, Color border)
		{
			if (color.IsEmpty)
			{
				color = HtmlColor;
			}

			Options.Add(new NotifyGumpOption(label, callback, color, fill, border));
		}

		protected virtual Size GetSizeMin()
		{
			return SizeMin;
		}

		protected virtual Size GetSizeMax()
		{
			return SizeMax;
		}

		protected override void Compile()
		{
			base.Compile();

			var sMin = GetSizeMin();
			var sMax = GetSizeMax();

			WidthMax = Math.Max(sMin.Width, Math.Min(sMax.Width, WidthMax));
			HeightMax = Math.Max(sMin.Height, Math.Min(sMax.Height, HeightMax));

			var wm = WidthMax - (BorderSize * 2);

			if (Options.Count > 0)
			{
				var owm = Math.Max(30, Math.Min(wm, Options.Max(o => o.Width)));

				OptionsCols = (int)Math.Floor(wm / (double)owm);
				OptionsRows = (int)Math.Ceiling(Options.Count / (double)OptionsCols);
			}

			var h =
				Html.ParseBBCode(HtmlColor)
					.StripHtmlBreaks(true)
					.Split('\n')
					.Select(line => line.StripHtml())
					.Not(String.IsNullOrWhiteSpace)
					.Select(line => line.ComputeSize(UOFont.Font0))
					.Aggregate(
						OptionsRows * 30,
						(c, s) =>
						{
							var val = s.Height + UOFont.Font0.LineSpacing;
							val *= (s.Width <= wm ? 1 : (int)Math.Ceiling(s.Width / (double)wm));

							return c + val;
						});

			HeightMax = Math.Max(sMin.Height, Math.Min(!Initialized ? HeightMax : sMax.Height, Math.Min(sMax.Height, h)));

			HtmlIndent = Math.Max(0, Math.Min(10, HtmlIndent));
			BorderSize = Math.Max(0, Math.Min(10, BorderSize));

			var f = Frame / (double)FrameCount;

			FrameWidth = (int)Math.Ceiling(WidthMax * f);
			FrameHeight = (int)Math.Ceiling(HeightMax * f);
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add(
				"frame",
				() =>
				{
					if (BorderSize > 0 && BorderID >= 0)
					{
						AddImageTiled(0, 0, FrameWidth, FrameHeight, BorderID);

						if (BorderAlpha)
						{
							AddAlphaRegion(0, 0, FrameWidth, FrameHeight);
						}
					}

					if (FrameWidth > BorderSize * 2 && FrameHeight > BorderSize * 2 && BackgroundID >= 0)
					{
						AddImageTiled(
							BorderSize,
							BorderSize,
							FrameWidth - (BorderSize * 2),
							(FrameHeight - (BorderSize * 2)),
							BackgroundID);

						if (BackgroundAlpha)
						{
							AddAlphaRegion(BorderSize, BorderSize, FrameWidth - (BorderSize * 2), FrameHeight - (BorderSize * 2));
						}
					}

					if (Frame < FrameCount)
					{
						return;
					}

					var html = Html.ParseBBCode(HtmlColor).Replace("<br>", "\n").Replace("<BR>", "\n");

					AddHtml(
						BorderSize + HtmlIndent,
						BorderSize,
						(FrameWidth - (BorderSize * 2)) - HtmlIndent,
						(FrameHeight - (BorderSize * 2)) - (OptionsRows * 30),
						html.WrapUOHtmlColor(HtmlColor, false),
						false,
						FrameHeight >= GetSizeMax().Height);
				});

			if (Options.Count > 0)
			{
				var wm = WidthMax - (BorderSize * 2);
				var bw = OptionsCols > 1 ? Options.Max(o => o.Width) : wm;

				if (bw < wm)
				{
					bw += (int)Math.Floor((wm - (OptionsCols * bw)) / (double)OptionsCols);
				}

				bw = Math.Max(30, Math.Min(wm, bw));

				var x = BorderSize;
				var y = BorderSize + (FrameHeight - (BorderSize * 2)) - (OptionsRows * 30);
				var w = FrameWidth - (BorderSize * 2);
				var h = OptionsRows * 30;

				CompileOptionsLayout(layout, x, y, w, h, bw);
			}
		}

		protected virtual void CompileOptionsLayout(SuperGumpLayout layout, int x, int y, int w, int h, int bw)
		{
			if (Frame < FrameCount || Options.Count == 0 || OptionsCols * OptionsRows <= 0)
			{
				return;
			}

			layout.Add("opts", () => AddRectangle(x, y, w, h, Color.Black, true));

			var oi = 0;
			var ox = x;
			var oy = y;

			foreach (var ob in Options)
			{
				CompileOptionLayout(layout, oi, ox, oy, bw, 30, ob);

				if (++oi % OptionsCols == 0)
				{
					ox = x;
					oy += 30;
				}
				else
				{
					ox += bw;
				}
			}
		}

		protected virtual void CompileOptionLayout(
			SuperGumpLayout layout,
			int index,
			int x,
			int y,
			int w,
			int h,
			NotifyGumpOption option)
		{
			layout.Add(
				"opts/" + index,
				() =>
				{
					AddHtmlButton(
						x,
						y,
						w,
						h,
						b =>
						{
							if (option.Callback != null)
							{
								option.Callback(b);
							}

							Refresh();
						},
						option.Label.GetString(User),
						option.LabelColor,
						option.FillColor,
						option.BorderColor,
						1);
				});
		}

		protected override bool CanAutoRefresh()
		{
			return State == AnimState.Pause && Frame > 0 ? AutoClose && base.CanAutoRefresh() : base.CanAutoRefresh();
		}

		protected override void OnAutoRefresh()
		{
			base.OnAutoRefresh();

			AnimateList();

			switch (State)
			{
				case AnimState.Show:
				{
					if (Frame++ >= FrameCount)
					{
						AutoRefreshRate = PauseDuration;
						State = AnimState.Pause;
						Frame = FrameCount;
					}
				}
					break;
				case AnimState.Hide:
				{
					if (Frame-- <= 0)
					{
						AutoRefreshRate = TimeSpan.FromMilliseconds(100.0);
						State = AnimState.Pause;
						Frame = 0;
						Close(true);
					}
				}
					break;
				case AnimState.Pause:
				{
					AutoRefreshRate = TimeSpan.FromMilliseconds(100.0);
					State = Frame <= 0 ? AnimState.Show : AutoClose ? AnimState.Hide : AnimState.Pause;
				}
					break;
			}
		}

		protected override bool OnBeforeSend()
		{
			if (!Initialized)
			{
				if (Notify.IsIgnored(GetType(), User))
				{
					return false;
				}

				if (!Notify.IsAnimated(GetType(), User))
				{
					AnimDuration = TimeSpan.Zero;
				}

				if (OnNotify != null)
				{
					OnNotify(this);
				}
			}

			return base.OnBeforeSend();
		}

		public override void Close(bool all = false)
		{
			if (all)
			{
				base.Close(true);
			}
			else
			{
				AutoRefreshRate = TimeSpan.FromMilliseconds(100.0);
				AutoClose = true;
			}
		}

		private void AnimateList()
		{
			VitaNexCore.TryCatch(
				() =>
				{
					var p = this;

					foreach (var g in
						EnumerateInstances<NotifyGump>(User, true)
							.Where(g => g != this && g.IsOpen && !g.IsDisposed && g.Y >= p.Y)
							.OrderBy(g => g.Y))
					{
						g.Y = p.Y + p.FrameHeight;
						p = g;

						if (g.State != AnimState.Pause)
						{
							return;
						}

						var lr = g.LastAutoRefresh;
						g.Refresh(true);
						g.LastAutoRefresh = lr;
					}
				},
				e => e.ToConsole());
		}

		private class Sub0 : NotifyGump
		{
			public Sub0(Mobile user, string html)
				: base(user, html)
			{ }
		}

		private class Sub1 : NotifyGump
		{
			public Sub1(Mobile user, string html)
				: base(user, html)
			{ }
		}

		private class Sub2 : NotifyGump
		{
			public Sub2(Mobile user, string html)
				: base(user, html)
			{ }
		}

		private class Sub3 : NotifyGump
		{
			public Sub3(Mobile user, string html)
				: base(user, html)
			{ }
		}

		private class Sub4 : NotifyGump
		{
			public Sub4(Mobile user, string html)
				: base(user, html)
			{ }
		}

		private class Sub5 : NotifyGump
		{
			public Sub5(Mobile user, string html)
				: base(user, html)
			{ }
		}

		private class Sub6 : NotifyGump
		{
			public Sub6(Mobile user, string html)
				: base(user, html)
			{ }
		}

		private class Sub7 : NotifyGump
		{
			public Sub7(Mobile user, string html)
				: base(user, html)
			{ }
		}

		private class Sub8 : NotifyGump
		{
			public Sub8(Mobile user, string html)
				: base(user, html)
			{ }
		}

		private class Sub9 : NotifyGump
		{
			public Sub9(Mobile user, string html)
				: base(user, html)
			{ }
		}
	}
}