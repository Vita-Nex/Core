#region Header
//   Vorspire    _,-'/-'/  EnumMenuGump.cs
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

using Server;
using Server.Gumps;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class EnumMenuGump<TEnum> : MenuGump
		where TEnum : struct, IComparable, IFormattable, IConvertible
	{
		public Action<TEnum> Callback { get; set; }

		public TEnum DefaultValue { get; set; }

		public EnumMenuGump(
			Mobile user,
			Gump parent = null,
			GumpButton clicked = null,
			TEnum def = default(TEnum),
			Action<TEnum> callback = null)
			: base(user, parent, null, clicked)
		{
			DefaultValue = def;
			Callback = callback;
		}

		protected virtual void OnSelected(TEnum val)
		{
			if (Callback != null)
			{
				Callback(val);
			}
		}

		protected override void CompileOptions(MenuGumpOptions list)
		{
			if (typeof(TEnum).IsEnum)
			{
				var vals = (default(TEnum) as Enum).EnumerateValues<TEnum>(false);

				foreach (var val in vals)
				{
					var e = new ListGumpEntry(val.ToString(), b => OnSelected(val));

					if (Equals(val, DefaultValue))
					{
						e.Hue = HighlightHue;
					}

					list.AppendEntry(e);
				}
			}

			base.CompileOptions(list);
		}
	}
}