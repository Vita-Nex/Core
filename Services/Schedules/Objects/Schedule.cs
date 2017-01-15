#region Header
//   Vorspire    _,-'/-'/  Schedule.cs
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
using System.Drawing;
using System.Text;

using Server;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Schedules
{
	[PropertyObject]
	public class Schedule : Timer
	{
		private ScheduleInfo _Info;

		private TimerPriority _DefaultPriority;

		private bool _Enabled;
		private string _Name;

		private DateTime? _LastGlobalTick;
		private DateTime? _CurrentGlobalTick;
		private DateTime? _NextGlobalTick;

		[CommandProperty(Schedules.Access)]
		public virtual string Name { get { return _Name ?? (_Name = String.Empty); } set { _Name = value ?? String.Empty; } }

		[CommandProperty(Schedules.Access)]
		public virtual bool Enabled
		{
			get { return _Enabled; }
			set
			{
				if (!_Enabled && value)
				{
					_Enabled = true;

					Priority = ComputePriority(Interval = TimeSpan.FromSeconds(1.0));

					if (OnEnabled != null)
					{
						OnEnabled(this);
					}
				}
				else if (_Enabled && !value)
				{
					_Enabled = false;

					Priority = ComputePriority(Interval = TimeSpan.FromMinutes(1.0));

					if (OnDisabled != null)
					{
						OnDisabled(this);
					}
				}
			}
		}

		[CommandProperty(Schedules.Access)]
		public virtual ScheduleInfo Info
		{
			get { return _Info ?? (_Info = new ScheduleInfo()); }
			set
			{
				_Info = value ?? new ScheduleInfo();
				InvalidateNextTick(DateTime.UtcNow);
			}
		}

		[CommandProperty(Schedules.Access)]
		public virtual DateTime? LastGlobalTick { get { return _LastGlobalTick; } }

		[CommandProperty(Schedules.Access)]
		public virtual DateTime? CurrentGlobalTick { get { return _CurrentGlobalTick; } }

		[CommandProperty(Schedules.Access)]
		public virtual DateTime? NextGlobalTick { get { return _NextGlobalTick; } }

		[CommandProperty(Schedules.Access)]
		public bool IsRegistered { get { return Schedules.IsRegistered(this); } }

		public event Action<Schedule> OnGlobalTick;
		public event Action<Schedule> OnEnabled;
		public event Action<Schedule> OnDisabled;

		public Schedule(
			string name,
			bool enabled,
			ScheduleMonths months = ScheduleMonths.None,
			ScheduleDays days = ScheduleDays.None,
			ScheduleTimes times = null,
			params Action<Schedule>[] onTick)
			: this(name, enabled, new ScheduleInfo(months, days, times), onTick)
		{ }

		public Schedule(string name, bool enabled, ScheduleInfo info, params Action<Schedule>[] onTick)
			: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
		{
			_Enabled = enabled;
			_Name = name ?? String.Empty;
			_Info = info ?? new ScheduleInfo();

			UpdateTicks(DateTime.UtcNow);

			if (onTick != null)
			{
				foreach (var a in onTick)
				{
					OnGlobalTick += a;
				}
			}

			Start();
		}

		public Schedule(GenericReader reader)
			: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
		{
			Deserialize(reader);
		}

		~Schedule()
		{
			Free();
		}

		public void Free()
		{
			OnGlobalTick = null;
			OnEnabled = null;
			OnDisabled = null;
		}

		public void Register()
		{
			if (!IsRegistered)
			{
				Schedules.Register(this);
			}
		}

		public void Unregister()
		{
			if (IsRegistered)
			{
				Schedules.Unregister(this);
			}
		}

		private void UpdateTicks(DateTime dt)
		{
			if (!dt.Kind.HasFlag(DateTimeKind.Utc))
			{
				dt = dt.ToUniversalTime();
			}

			_LastGlobalTick = dt;
			_NextGlobalTick = _Info.FindAfter(dt);
		}

		public void InvalidateNextTick(DateTime dt)
		{
			if (!dt.Kind.HasFlag(DateTimeKind.Utc))
			{
				dt = dt.ToUniversalTime();
			}

			_NextGlobalTick = _Info.FindAfter(dt);
		}

		protected override void OnTick()
		{
			base.OnTick();

			if (!_Enabled)
			{
				return;
			}

			var now = DateTime.UtcNow;

			if (_NextGlobalTick == null)
			{
				InvalidateNextTick(now);
				return;
			}

			if (now < _NextGlobalTick)
			{
				return;
			}

			_CurrentGlobalTick = now;

			InvalidateNextTick(now);

			if (OnGlobalTick != null)
			{
				OnGlobalTick(this);
			}

			_LastGlobalTick = now;
		}

		public virtual void Serialize(GenericWriter writer)
		{
			var version = writer.SetVersion(2);

			switch (version)
			{
				case 2:
				case 1:
				case 0:
				{
					if (version < 2)
					{
						writer.WriteType(_Info, t => _Info.Serialize(writer));
					}
					else
					{
						writer.WriteBlock(w => w.WriteType(_Info, t => _Info.Serialize(w)));
					}

					writer.Write(_Enabled);
					writer.Write(_Name);
					writer.WriteFlag(_DefaultPriority);

					if (_LastGlobalTick != null)
					{
						writer.Write(true);
						writer.Write(_LastGlobalTick.Value);
					}
					else
					{
						writer.Write(false);
					}

					if (_NextGlobalTick != null)
					{
						writer.Write(true);
						writer.Write(_NextGlobalTick.Value);
					}
					else
					{
						writer.Write(false);
					}

					writer.Write(Delay);
					writer.Write(Interval);
				}
					break;
			}

			if (version > 0)
			{
				writer.Write(Running);
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			var version = reader.GetVersion();

			switch (version)
			{
				case 2:
				case 1:
				case 0:
				{
					if (version < 2)
					{
						_Info = reader.ReadTypeCreate<ScheduleInfo>(reader) ?? new ScheduleInfo();
					}
					else
					{
						reader.ReadBlock(r => _Info = r.ReadTypeCreate<ScheduleInfo>(r) ?? new ScheduleInfo());
					}

					_Enabled = reader.ReadBool();
					_Name = reader.ReadString();
					_DefaultPriority = reader.ReadFlag<TimerPriority>();

					if (reader.ReadBool())
					{
						_LastGlobalTick = reader.ReadDateTime();
					}

					if (reader.ReadBool())
					{
						_NextGlobalTick = reader.ReadDateTime();
					}

					Delay = reader.ReadTimeSpan();
					Interval = reader.ReadTimeSpan();
				}
					break;
			}

			if (version > 0)
			{
				Running = reader.ReadBool();
			}
		}

		public override string ToString()
		{
			return _Name;
		}

		public virtual string ToHtmlString(bool big = true)
		{
			var now = DateTime.UtcNow;
			var html = new StringBuilder();

			html.AppendFormat("Current Date: {0}\n", Schedules.FormatDate(now));
			html.AppendFormat("Current Time: {0}\n", Schedules.FormatTime(now.TimeOfDay, true));

			html.AppendLine("\nSchedule Overview:\n");

			if (!_Enabled)
			{
				html.AppendLine("Schedule is currently disabled.".WrapUOHtmlColor(Color.OrangeRed));
				return html.ToString();
			}

			var print = false;
			string months = _Info.Months.ToString(), days = String.Empty, times = String.Empty;

			if (months == "All")
			{
				months = String.Join(" ", Enum.GetNames(typeof(ScheduleMonths)));
				months = months.Replace("All", String.Empty).Replace("None", String.Empty).Trim().Replace(" ", ", ");
			}

			if (months == "None")
			{
				html.AppendLine("Schedule requires at least one Month to be set.".WrapUOHtmlColor(Color.OrangeRed));
			}
			else
			{
				days = _Info.Days.ToString();

				if (days == "All")
				{
					days = String.Join(" ", Enum.GetNames(typeof(ScheduleDays)));
					days = days.Replace("All", String.Empty).Replace("None", String.Empty).Trim().Replace(" ", ", ");
				}

				if (days == "None")
				{
					html.AppendLine("Schedule requires at least one Day to be set.".WrapUOHtmlColor(Color.OrangeRed));
				}
				else
				{
					times = _Info.Times.ToString();

					if (String.IsNullOrWhiteSpace(times))
					{
						html.AppendLine("Schedule requires at least one Time to be set.".WrapUOHtmlColor(Color.OrangeRed));
					}
					else
					{
						var cc = 0;
						var wrap = String.Empty;

						foreach (var t in times)
						{
							if (t == ',')
							{
								cc++;
								wrap += ',';

								if (cc % 6 == 0)
								{
									wrap += '\n';
								}
								else
								{
									wrap += ' ';
								}
							}
							else if (t != ' ')
							{
								wrap += t;
							}
						}

						times = wrap;
						print = true;
					}
				}
			}

			if (print)
			{
				html.AppendFormat("<BASEFONT COLOR=#{0:X6}>", Color.Cyan.ToArgb());
				html.AppendLine("Schedule is set to perform an action:");
				html.AppendLine("\n<B>In...</B>");
				html.AppendLine(months);
				html.AppendLine("\n<B>On...</B>");
				html.AppendLine(days);
				html.AppendLine("\n<B>At...</B>");
				html.AppendLine(times);

				if (NextGlobalTick != null)
				{
					var today = (NextGlobalTick.Value.Day == DateTime.UtcNow.Day);

					html.AppendLine(
						String.Format(
							"\n\nThe next tick will be at {0} {1}.",
							NextGlobalTick.Value.TimeOfDay.ToSimpleString("h:m:s"),
							today ? "today." : "on " + NextGlobalTick.Value.ToSimpleString("D, M d")));
				}
			}

			return (big ? String.Format("<big>{0}</big>", html) : html.ToString()).WrapUOHtmlColor(
				SuperGump.DefaultHtmlColor,
				false);
		}
	}
}