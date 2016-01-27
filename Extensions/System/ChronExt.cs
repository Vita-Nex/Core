#region Header
//   Vorspire    _,-'/-'/  ChronExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;

using VitaNex;
#endregion

namespace System
{
	[Flags]
	public enum Months
	{
		None = 0x000,
		January = 0x001,
		Febuary = 0x002,
		March = 0x004,
		April = 0x008,
		May = 0x010,
		June = 0x020,
		July = 0x040,
		August = 0x080,
		September = 0x100,
		October = 0x200,
		November = 0x400,
		December = 0x800,

		All = January | Febuary | March | April | May | June | July | August | September | October | November | December
	}

	public static class ChronExtUtility
	{
		public static DateTime Interpolate(this DateTime start, DateTime end, double percent)
		{
			return new DateTime((long)(start.Ticks + ((end.Ticks - start.Ticks) * percent)));
		}

		public static TimeSpan Interpolate(this TimeSpan start, TimeSpan end, double percent)
		{
			return new TimeSpan((long)(start.Ticks + ((end.Ticks - start.Ticks) * percent)));
		}

		public static TimeStamp Interpolate(this TimeStamp start, TimeStamp end, double percent)
		{
			return new TimeStamp((long)(start.Ticks + ((end.Ticks - start.Ticks) * percent)));
		}

		public static TimeStamp ToTimeStamp(this DateTime date)
		{
			return new TimeStamp(date);
		}

		public static Months GetMonth(this DateTime date)
		{
			switch (date.Month)
			{
				case 1:
					return Months.January;
				case 2:
					return Months.Febuary;
				case 3:
					return Months.March;
				case 4:
					return Months.April;
				case 5:
					return Months.May;
				case 6:
					return Months.June;
				case 7:
					return Months.July;
				case 8:
					return Months.August;
				case 9:
					return Months.September;
				case 10:
					return Months.October;
				case 11:
					return Months.November;
				case 12:
					return Months.December;
				default:
					return Months.None;
			}
		}

		public static string ToSimpleString(this DateTime date, string format = "t D d M y")
		{
			var strs = new List<string>(format.Length);

			var noformat = false;

			for (var i = 0; i < format.Length; i++)
			{
				if (format[i] == '#')
				{
					noformat = !noformat;
					continue;
				}

				if (noformat)
				{
					strs.Add(Convert.ToString(format[i]));
					continue;
				}

				switch (format[i])
				{
					case '\\':
						strs.Add((i + 1 < format.Length) ? Convert.ToString(format[++i]) : String.Empty);
						break;
					case 'z':
					{
						var tzo = TimeZoneInfo.Local.DisplayName;
						var s = tzo.IndexOf(' ');

						if (s > 0)
						{
							tzo = tzo.Substring(0, s);
						}

						strs.Add(tzo);
					}
						break;
					case 'Z':
					{
						var tzo = date.IsDaylightSavingTime() ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.DisplayName;
						var s = tzo.IndexOf(' ');

						if (s > 0)
						{
							tzo = tzo.Substring(0, s);
						}

						strs.Add(tzo);
					}
						break;
					case 'D':
						strs.Add(Convert.ToString(date.DayOfWeek));
						break;
					case 'd':
						strs.Add(Convert.ToString(date.Day));
						break;
					case 'M':
						strs.Add(Convert.ToString(GetMonth(date)));
						break;
					case 'm':
						strs.Add(Convert.ToString(date.Month));
						break;
					case 'y':
						strs.Add(Convert.ToString(date.Year));
						break;
					case 't':
					{
						var tf = String.Empty;

						if (i + 1 < format.Length)
						{
							if (format[i + 1] == '@')
							{
								++i;

								while (++i < format.Length && format[i] != '@')
								{
									tf += format[i];
								}
							}
						}

						strs.Add(ToSimpleString(date.TimeOfDay, !String.IsNullOrWhiteSpace(tf) ? tf : "h-m-s"));
					}
						break;
					default:
						strs.Add(Convert.ToString(format[i]));
						break;
				}
			}

			var str = String.Join(String.Empty, strs);

			strs.Free(true);

			return str;
		}

		public static string ToSimpleString(this TimeSpan time, string format = "h-m-s")
		{
			var strs = new string[format.Length];

			var noformat = false;
			var zeroValue = false;
			var zeroEmpty = 0;

			for (var i = 0; i < format.Length && i < strs.Length; i++)
			{
				if (format[i] == '#')
				{
					noformat = !noformat;
					continue;
				}

				if (noformat)
				{
					strs[i] = Convert.ToString(format[i]);
					continue;
				}

				var append = String.Empty;

				switch (format[i])
				{
					case '<':
						++zeroEmpty;
						break;
					case '>':
					{
						if (zeroEmpty == 0 || (zeroEmpty > 0 && --zeroEmpty == 0))
						{
							zeroValue = false;
						}
					}
						break;
					case '\\':
						append = ((i + 1 < format.Length) ? Convert.ToString(format[++i]) : String.Empty);
						break;
					case 'z':
					{
						var tzo = TimeZoneInfo.Local.DisplayName;
						var s = tzo.IndexOf(' ');

						if (s > 0)
						{
							tzo = tzo.Substring(0, s);
						}

						append = tzo;
					}
						break;
					case 'Z':
					{
						var tzo = DateTime.Now.IsDaylightSavingTime() ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.DisplayName;
						var s = tzo.IndexOf(' ');

						if (s > 0)
						{
							tzo = tzo.Substring(0, s);
						}

						append = tzo;
					}
						break;
					case 'D':
						append = String.Format(zeroEmpty > 0 ? "{0:#.##}" : "{0:F2}", time.TotalDays);
						zeroValue = String.IsNullOrWhiteSpace(append);
						break;
					case 'H':
						append = String.Format(zeroEmpty > 0 ? "{0:#.##}" : "{0:F2}", time.TotalHours);
						zeroValue = String.IsNullOrWhiteSpace(append);
						break;
					case 'M':
						append = String.Format(zeroEmpty > 0 ? "{0:#.##}" : "{0:F2}", time.TotalMinutes);
						zeroValue = String.IsNullOrWhiteSpace(append);
						break;
					case 'S':
						append = String.Format(zeroEmpty > 0 ? "{0:#.##}" : "{0:F2}", time.TotalSeconds);
						zeroValue = String.IsNullOrWhiteSpace(append);
						break;
					case 'd':
						append = String.Format(zeroEmpty > 0 ? "{0:#}" : "{0:D2}", time.Days);
						zeroValue = String.IsNullOrWhiteSpace(append);
						break;
					case 'h':
						append = String.Format(zeroEmpty > 0 ? "{0:#}" : "{0:D2}", time.Hours);
						zeroValue = String.IsNullOrWhiteSpace(append);
						break;
					case 'm':
						append = String.Format(zeroEmpty > 0 ? "{0:#}" : "{0:D2}", time.Minutes);
						zeroValue = String.IsNullOrWhiteSpace(append);
						break;
					case 's':
						append = String.Format(zeroEmpty > 0 ? "{0:#}" : "{0:D2}", time.Seconds);
						zeroValue = String.IsNullOrWhiteSpace(append);
						break;
					default:
						append = Convert.ToString(format[i]);
						break;
				}

				if (zeroValue && zeroEmpty > 0)
				{
					append = String.Empty;
				}

				strs[i] = append;
			}

			return String.Join(String.Empty, strs);
		}

		public static string ToDirectoryName(this DateTime date, string format = "D d M y")
		{
			return ToSimpleString(date, format);
		}

		public static string ToFileName(this DateTime date, string format = "D d M y")
		{
			return ToSimpleString(date, format);
		}

		public static string ToDirectoryName(this TimeSpan time, string format = "h-m")
		{
			return ToSimpleString(time, format);
		}

		public static string ToFileName(this TimeSpan time, string format = "h-m")
		{
			return ToSimpleString(time, format);
		}
	}
}