#region Header
//   Vorspire    _,-'/-'/  CryptoHashType.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

namespace VitaNex.Crypto
{
	public enum CryptoHashType
	{
		MD5 = 0,
		SHA1 = 1,
		SHA256 = 2,
		SHA384 = 3,
		SHA512 = 4,
		RIPEMD160 = 5,
		Jenkins3 = 6
	}
}