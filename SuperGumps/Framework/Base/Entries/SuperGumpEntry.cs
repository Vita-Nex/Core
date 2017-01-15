#region Header
//   Vorspire    _,-'/-'/  SuperGumpEntry.cs
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

using Server;
using Server.Gumps;
using Server.Network;
#endregion

namespace VitaNex.SuperGumps
{
	public interface IGumpEntryPoint
	{
		int X { get; set; }
		int Y { get; set; }
	}

	public interface IGumpEntrySize
	{
		int Width { get; set; }
		int Height { get; set; }
	}

	public interface IGumpEntryVector : IGumpEntryPoint, IGumpEntrySize
	{ }

	public abstract class SuperGumpEntry : GumpEntry, IDisposable
	{
		public virtual bool IgnoreModalOffset { get { return false; } }

		public virtual Mobile User
		{
			get
			{
				if (Parent is SuperGump)
				{
					return ((SuperGump)Parent).User;
				}

				return null;
			}
		}

		public virtual NetState UserState
		{
			get
			{
				var user = User;

				if (user != null)
				{
					return user.NetState;
				}

				return null;
			}
		}

		protected int FixHue(int hue)
		{
			hue = Math.Max(-1, Math.Min(2999, hue));

			if (UserState.IsEnhanced())
			{
				++hue;
			}

			return hue;
		}

		protected void Delta<T>(ref T var, T val)
		{
			if (ReferenceEquals(var, val) || Equals(var, val))
			{
				return;
			}

			var = val;

			if (Parent != null)
			{
				Parent.Invalidate();
			}
		}

		public virtual void Dispose()
		{
			Parent = null;
		}
	}
}