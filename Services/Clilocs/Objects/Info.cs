#region Header
//   Vorspire    _,-'/-'/  Info.cs
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
using System.Linq;

using Server;
#endregion

namespace VitaNex
{
	public sealed class ClilocInfo
	{
		public ClilocLNG Language { get; private set; }

		public int Index { get; private set; }
		public string Text { get; private set; }

		public int Count { get; private set; }
		public string Format { get; private set; }

		public bool HasArgs { get { return Count > 0 && !String.IsNullOrWhiteSpace(Format); } }

		public ClilocInfo(ClilocLNG lng, int index, string text)
		{
			Language = lng;
			Index = index;
			Text = text;

			InvalidateArgs();
		}

		public void InvalidateArgs()
		{
			if (String.IsNullOrWhiteSpace(Text))
			{
				Count = 0;
				Text = Format = String.Empty;
				return;
			}

			Format = Clilocs.VarPattern.Replace(Text, e => "{" + (Count++) + "}");

			/*
			Console.WriteLine("{0}: {1}", Index, Format);
			
			Count = Clilocs.VarPattern.Matches(Text).OfType<Match>().Count(m => m.Success);

			string[] format = new string[Count];

			format.SetAll(i => "{" + i + "}");

			Format = String.Join("\t", format);
			*/
		}

		private string ParseArgs(object value)
		{
			var s = value.ToString();

			int idx, oidx = -1;

			while ((idx = s.IndexOf('#')) > oidx)
			{
				oidx = idx;

				var sub = String.Empty;

				for (var si = idx + 1; si < s.Length; si++)
				{
					if (!Char.IsDigit(s[si]))
					{
						break;
					}

					sub += s[si];
				}

				int sid;

				if (!Int32.TryParse(sub, out sid) || sid < 500000 || sid > 3011032)
				{
					continue;
				}

				var inf = sid == Index ? this : Language.Lookup(sid);

				if (inf != null)
				{
					s = s.Substring(0, idx) + inf.Text + s.Substring(idx + 1 + sub.Length);
				}
			}

			return s;
		}

		public override string ToString()
		{
			return Text;
		}

		public string ToString(string args)
		{
			if (!HasArgs)
			{
				return ToString();
			}

			if (String.IsNullOrEmpty(args))
			{
				args = new String('\t', Count - 1);
			}
			else
			{
				var ac = args.Count(c => c == '\t');

				if (ac < Count - 1)
				{
					args += new String('\t', (Count - 1) - ac);
				}
			}

			var buffer = new object[Count];
			var split = args.Split('\t');

			buffer.SetAll(i => (i < split.Length ? split[i] ?? String.Empty : String.Empty) + (Count > 1 ? "\t" : String.Empty));

			if (split.Length > buffer.Length)
			{
				buffer[buffer.Length - 1] += String.Join(
					String.Empty,
					split.Skip(buffer.Length).Take(split.Length - buffer.Length));
			}

			buffer.SetAll((i, s) => ParseArgs(s));

			return String.Format(Format, buffer);
		}

		public string ToString(object[] args)
		{
			if (!HasArgs)
			{
				return ToString();
			}

			if (args == null)
			{
				args = new object[Count];
			}
			else if (args.Length < Count)
			{
				args = args.Merge(new object[Count - args.Length]);
			}

			var buffer = new object[Count];

			buffer.SetAll(i => (i < args.Length ? args[i] ?? String.Empty : String.Empty) + (Count > 1 ? "\t" : String.Empty));

			if (args.Length > buffer.Length)
			{
				buffer[buffer.Length - 1] += String.Join(String.Empty, args.Skip(buffer.Length).Take(args.Length - buffer.Length));
			}

			buffer.SetAll((i, s) => ParseArgs(s));

			return String.Format(Format, buffer);
		}

		public TextDefinition ToDefinition()
		{
			return new TextDefinition(Index, Text);
		}
	}
}