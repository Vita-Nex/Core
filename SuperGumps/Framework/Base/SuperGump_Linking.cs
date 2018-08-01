#region Header
//   Vorspire    _,-'/-'/  SuperGump_Linking.cs
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
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		private List<SuperGump> _Linked;

		public List<SuperGump> Linked { get { return _Linked; } protected set { _Linked = value; } }

		public bool IsLinked { get { return Linked.Count > 0; } }

		public virtual void Link(SuperGump gump)
		{
			if (gump == null)
			{
				return;
			}

			Linked.AddOrReplace(gump);

			if (!gump.Linked.Contains(this))
			{
				gump.Link(this);
			}
		}

		public virtual void Unlink(SuperGump gump)
		{
			if (gump == null)
			{
				return;
			}

			Linked.Remove(gump);

			if (gump.Linked.Contains(this))
			{
				gump.Unlink(this);
			}
		}

		public virtual bool IsLinkedWith(SuperGump gump)
		{
			return gump != null && Linked.Contains(gump);
		}

		protected virtual void OnLinkRefreshed(SuperGump link)
		{ }

		protected virtual void OnLinkHidden(SuperGump link)
		{ }

		protected virtual void OnLinkClosed(SuperGump link)
		{ }

		protected virtual void OnLinkSend(SuperGump link)
		{ }

		protected virtual void OnLinkSendFail(SuperGump link)
		{ }
	}
}