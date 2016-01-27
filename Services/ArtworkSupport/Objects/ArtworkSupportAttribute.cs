#region Header
//   Vorspire    _,-'/-'/  ArtworkSupportAttribute.cs
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
#endregion

namespace VitaNex
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class ArtworkSupportAttribute : Attribute
	{
		public ClientVersion HighVersion { get; private set; }
		public ClientVersion LowVersion { get; private set; }

		public int HighItemID { get; private set; }
		public int LowItemID { get; private set; }

		public ArtworkSupportAttribute(int highItemID, int lowItemID)
			: this(ArtworkSupport.DefaultHighVersion, ArtworkSupport.DefaultLowVersion, highItemID, lowItemID)
		{ }

		public ArtworkSupportAttribute(string highVersion, int highItemID, int lowItemID)
			: this(new ClientVersion(highVersion), highItemID, lowItemID)
		{ }

		public ArtworkSupportAttribute(ClientVersion highVersion, int highItemID, int lowItemID)
			: this(highVersion, ArtworkSupport.DefaultLowVersion, highItemID, lowItemID)
		{ }

		public ArtworkSupportAttribute(string highVersion, string lowVersion, int highItemID, int lowItemID)
			: this(new ClientVersion(highVersion), new ClientVersion(lowVersion), highItemID, lowItemID)
		{ }

		public ArtworkSupportAttribute(ClientVersion highVersion, ClientVersion lowVersion, int highItemID, int lowItemID)
		{
			HighVersion = highVersion ?? ArtworkSupport.DefaultHighVersion;
			LowVersion = lowVersion ?? ArtworkSupport.DefaultLowVersion;

			HighItemID = highItemID;
			LowItemID = lowItemID;
		}
	}
}