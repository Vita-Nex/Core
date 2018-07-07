#region Header
//   Vorspire    _,-'/-'/  Handler.cs
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
#endregion

namespace VitaNex.Web
{
	public class WebAPIHandler
	{
		public string Uri { get; private set; }

		public Action<WebAPIContext> Handler { get; set; }

		public WebAPIHandler(string uri, Action<WebAPIContext> handler)
		{
			Uri = uri;
			Handler = handler;
		}

		public override int GetHashCode()
		{
			return Uri.GetHashCode();
		}
	}
}