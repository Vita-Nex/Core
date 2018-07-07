#region Header
//   Vorspire    _,-'/-'/  JsonToken.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

namespace VitaNex.Text
{
	public enum JsonToken
	{
		None = 0,
		ObjectOpen,
		ObjectClose,
		ArrayOpen,
		ArrayClose,
		Colon,
		Comma,
		String,
		Number,
		True,
		False,
		Null
	}
}