#region Header
//   Vorspire    _,-'/-'/  TimeBoostsUI.cs
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
using System.Drawing;
using System.Linq;
using System.Text;

using Server;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.TimeBoosts
{
	public class TimeBoostsUI : SuperGumpList<ITimeBoost>
	{
		public static string DefaultTitle = "Time Boosts";
		public static string DefaultSubTitle = String.Empty;
		public static string DefaultEmptyText = "No available Time Boosts.";
		public static string DefaultSummaryText = "Time Left";

		public static Color DefaultTitleColor = Color.PaleGoldenrod;
		public static Color DefaultSubTitleColor = Color.Goldenrod;
		public static Color DefaultEmptyTextColor = Color.SaddleBrown;
		public static Color DefaultBoostTextColor = Color.Yellow;
		public static Color DefaultBoostCountColor = Color.LawnGreen;
		public static Color DefaultSummaryTextColor = Color.Goldenrod;

		public static void Update(PlayerMobile user)
		{
			var info = EnumerateInstances<TimeBoostsUI>(user).FirstOrDefault(g => g != null && !g.IsDisposed && g.IsOpen);

			if (info != null)
			{
				info.Refresh(true);
			}
		}

		public string Title { get; set; }
		public string SubTitle { get; set; }
		public string EmptyText { get; set; }
		public string SummaryText { get; set; }

		public Color TitleColor { get; set; }
		public Color SubTitleColor { get; set; }
		public Color EmptyTextColor { get; set; }
		public Color BoostTextColor { get; set; }
		public Color BoostCountColor { get; set; }
		public Color SummaryTextColor { get; set; }

		public TimeBoostProfile Profile { get; set; }

		public Func<ITimeBoost, bool> CanApply { get; set; }
		public Action<ITimeBoost> BoostUsed { get; set; }

		public Func<TimeSpan> GetTime { get; set; }
		public Action<TimeSpan> SetTime { get; set; }

		public TimeSpan? OldTime { get; private set; }

		public TimeSpan? Time
		{
			get
			{
				if (GetTime != null)
				{
					return GetTime();
				}

				return null;
			}
			set
			{
				if (SetTime != null)
				{
					SetTime(value ?? TimeSpan.Zero);
				}
			}
		}

		public TimeBoostsUI(
			Mobile user,
			Gump parent = null,
			TimeBoostProfile profile = null,
			Func<ITimeBoost, bool> canApply = null,
			Action<ITimeBoost> boostUsed = null,
			Func<TimeSpan> getTime = null,
			Action<TimeSpan> setTime = null)
			: base(user, parent)
		{
			Profile = profile;

			CanApply = canApply;
			BoostUsed = boostUsed;

			GetTime = getTime;
			SetTime = setTime;

			Title = DefaultTitle;
			SubTitle = DefaultSubTitle;
			EmptyText = DefaultEmptyText;
			SummaryText = DefaultSummaryText;

			TitleColor = DefaultTitleColor;
			SubTitleColor = DefaultSubTitleColor;
			EmptyTextColor = DefaultEmptyTextColor;
			BoostTextColor = DefaultBoostTextColor;
			BoostCountColor = DefaultBoostCountColor;
			SummaryTextColor = DefaultSummaryTextColor;

			EntriesPerPage = 4;

			Sorted = true;

			CanClose = true;
			CanDispose = true;
			CanMove = true;
			CanResize = false;

			AutoRefreshRate = TimeSpan.FromMinutes(1.0);
			AutoRefresh = true;

			ForceRecompile = true;
		}

		protected override void CompileList(List<ITimeBoost> list)
		{
			list.Clear();
			list.AddRange(TimeBoosts.AllTimes.Where(b => Profile == null || Profile[b] > 0));

			base.CompileList(list);
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add(
				"background",
				() =>
				{
					AddBackground(0, 0, 430, 225, 9270);
					//AddBackground(10, 10, 410, 25, 9350);
					AddBackground(15, 40, 400, 140, 9350);

					/*if (Time != null)
					{
						AddBackground(10, 185, 340, 28, 9350); //9350
					}*/
				});

			layout.Add(
				"title",
				() =>
				{
					if (!String.IsNullOrWhiteSpace(Title))
					{
						var text = Title;
						text = text.ToUpper();
						text = text.WrapUOHtmlBold();
						text = text.WrapUOHtmlColor(TitleColor, false);

						AddHtml(20, 15, 235, 40, text, false, false);
					}
				});

			layout.Add(
				"subtitle",
				() =>
				{
					if (!String.IsNullOrWhiteSpace(SubTitle))
					{
						var text = SubTitle;
						text = text.ToUpper();
						text = text.WrapUOHtmlBold();
						text = text.WrapUOHtmlColor(SubTitleColor, false);
						text = text.WrapUOHtmlRight();

						AddHtml(255, 15, 150, 40, text, false, false);
					}
				});

			layout.Add(
				"page/prev",
				() =>
				{
					if (HasPrevPage)
					{
						AddButton(7, 150, 9910, 9911, PreviousPage);
					}
					else
					{
						AddImage(7, 150, 9909);
					}
				});

			layout.Add(
				"page/next",
				() =>
				{
					if (HasNextPage)
					{
						AddButton(401, 150, 9904, 9905, NextPage);
					}
					else
					{
						AddImage(401, 150, 9903);
					}
				});

			var range = GetListRange();

			if (range.Count > 0)
			{
				CompileEntryLayout(layout, range);
			}
			else
			{
				layout.Add(
					"empty",
					() =>
					{
						var text = EmptyText;
						text = text.WrapUOHtmlColor(EmptyTextColor, false);
						text = text.WrapUOHtmlCenter();

						AddHtml(35, 150, 355, 40, text, false, false);
					});
			}

			layout.Add(
				"summary",
				() =>
				{
					if (Time != null)
					{
						var time = Time.Value;

						if (time < TimeSpan.Zero)
						{
							time = TimeSpan.Zero;
						}

						var text = SummaryText;
						text = text.ToUpper();
						text = text.WrapUOHtmlBold();
						text = text.WrapUOHtmlColor(SummaryTextColor, false);
						text = text.WrapUOHtmlCenter();

						AddHtml(20, 192, 87, 40, text, false, false);
						AddImage(112, 185, 30223);
						AddImageTime(140, 185, time, 0x33); // 175 x 28
						AddImage(315, 185, 30223);
					}
				});

			layout.Add("okay", () => AddButton(350, 187, 247, 248, Close));

			layout.Add(
				"tip",
				() =>
				{
					AddBackground(0, 225, 430, 40, 9270);

					var tip = "Tip: Click an available time boost to reduce the time left!";

					tip = tip.WrapUOHtmlSmall().WrapUOHtmlCenter().WrapUOHtmlColor(Color.Gold);

					AddHtml(20, 235, 390, 40, tip, false, false);
				});
		}

		protected virtual void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, ITimeBoost> range)
		{
			var i = 0;

			foreach (var kv in range)
			{
				CompileEntryLayout(layout, range.Count, kv.Key, i, 28 + (i * 93), kv.Value);

				++i;
			}
		}

		protected virtual void CompileEntryLayout(
			SuperGumpLayout layout,
			int length,
			int index,
			int pIndex,
			int xOffset,
			ITimeBoost boost)
		{
			layout.Add(
				"boosts/" + index,
				() =>
				{
					int hue = boost.Hue, bellID = 10850;
					var text = String.Empty;

					if (boost.Value.Hours != 0)
					{
						bellID = 10810; //Blue Gem
						text = "Hour";
					}
					else if (boost.Value.Minutes != 0)
					{
						bellID = 10830; //Green Gem
						text = "Min";
					}
					else
					{
						hue = 2999;
					}

					if (!String.IsNullOrWhiteSpace(text) && boost.RawValue != 1)
					{
						text += "s";
					}

					if (hue == 2999)
					{
						AddImage(xOffset + 7, 56, bellID, hue); //Left Bell
						AddImage(xOffset + 57, 56, bellID, hue); //Right Bell
					}
					else
					{
						AddImage(xOffset + 7, 56, bellID); //Left Bell
						AddImage(xOffset + 57, 56, bellID); //Right Bell
					}

					AddImage(xOffset, 45, 30058, hue); //Hammer

					if (Profile != null)
					{
						AddButton(xOffset + 5, 65, 1417, 1417, b => SelectBoost(boost)); //Disc
					}

					AddImage(xOffset + 5, 65, 1417, hue); //Disc
					AddImage(xOffset + 14, 75, 5577, 2999); //Blackout

					AddImageNumber(xOffset + 44, 99, boost.RawValue, hue, Axis.Both);

					if (!String.IsNullOrWhiteSpace(text))
					{
						text = text.WrapUOHtmlSmall();
						text = text.WrapUOHtmlCenter();
						text = text.WrapUOHtmlColor(BoostTextColor, false);

						AddHtml(xOffset + 20, 110, 50, 40, text, false, false);
					}

					if (hue == 2999)
					{
						AddImage(xOffset, 130, 30062, hue); //Feet
					}
					else
					{
						if (Profile != null)
						{
							AddBackground(xOffset + 10, 143, 70, 30, 9270); //9300
						}

						AddImage(xOffset, 130, 30062); //Feet

						if (Profile != null)
						{
							text = Profile[boost].ToString("#,0");
							text = text.WrapUOHtmlBold();
							text = text.WrapUOHtmlCenter();
							text = text.WrapUOHtmlColor(BoostCountColor, false);

							AddHtml(xOffset + 12, 150, 65, 40, text, false, false);
						}
					}
				});
		}

		public virtual void SelectBoost(ITimeBoost boost)
		{
			if (CanApplyBoost(boost))
			{
				ApplyBoost(boost, boost.Value.TotalHours >= 6);
			}
			else
			{
				Refresh(true);
			}
		}

		public virtual void ApplyBoost(ITimeBoost boost, bool confirm)
		{
			if (CanApplyBoost(boost))
			{
				if (confirm)
				{
					new ConfirmDialogGump(User, Hide(true))
					{
						Icon = 7057,
						Title = "Use " + boost.Name + "?",
						Html = String.Format(
							"{0}: {1}\n{2}\nClick OK to apply this Time Boost.",
							Title,
							SubTitle,
							String.Format(
								"Reduces time by {0} {1}{2}.",
								boost.RawValue,
								boost.Desc,
								boost.RawValue != 1 ? "s" : String.Empty)),
						AcceptHandler = b => ApplyBoost(boost, false),
						CancelHandler = b => Refresh(true)
					}.Send();
					return;
				}

				if (ApplyBoost(boost))
				{
					OnBoostUsed(boost);
				}
			}

			if (Hidden)
			{
				Refresh(true);
			}
		}

		public virtual bool ApplyBoost(ITimeBoost boost)
		{
			if (CanApplyBoost(boost) && Profile.Consume(boost, 1))
			{
				OldTime = Time;
				Time -= boost.Value;
				return true;
			}

			return false;
		}

		protected virtual bool CanApplyBoost(ITimeBoost boost)
		{
			return GetTime != null && SetTime != null && Time > TimeSpan.Zero && boost != null && boost.RawValue > 0 &&
				   Profile != null && Profile.Owner == User.Account && (CanApply == null || CanApply(boost));
		}

		protected virtual void OnBoostUsed(ITimeBoost boost)
		{
			if (BoostUsed != null)
			{
				BoostUsed(boost);
			}

			LogBoostUsed(boost);
		}

		protected virtual void LogBoostUsed(ITimeBoost boost)
		{
			var log = new StringBuilder();

			log.AppendLine();
			log.AppendLine("UI: '{0}' : '{1}' : '{2}'", Title, SubTitle, SummaryText);
			log.AppendLine("User: {0}", User);
			log.AppendLine("Boost: {0}", boost);
			log.AppendLine("Get: {0}", GetTime.Trace(false));
			log.AppendLine("Set: {0}", SetTime.Trace(false));
			log.AppendLine("Time: {0} > {1}", OldTime, Time);
			log.AppendLine();

			log.Log("/TimeBoosts/" + DateTime.Now.ToDirectoryName() + "/" + boost + ".log");
		}

		public override int SortCompare(ITimeBoost a, ITimeBoost b)
		{
			var res = 0;

			if (a.CompareNull(b, ref res))
			{
				return res;
			}

			if (a.Value < b.Value)
			{
				return -1;
			}

			if (a.Value > b.Value)
			{
				return 1;
			}

			return 0;
		}

		protected override void OnDisposed()
		{
			base.OnDisposed();

			Profile = null;
		}
	}
}