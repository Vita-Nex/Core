#region Header
//   Vorspire    _,-'/-'/  IGameUI.cs
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
#endregion

namespace VitaNex.Modules.Games
{
	public interface IGameUI : IDisposable
	{
		bool IsDisposed { get; }
		bool IsDisposing { get; }

		IGameEngine Engine { get; }

		Mobile User { get; }

		bool Validate();
	}
}