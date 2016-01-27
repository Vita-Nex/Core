#region Header
//   Vorspire    _,-'/-'/  AntiAdverts_Init.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2016  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;

using Server;
using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.AntiAdverts
{
	[CoreModule("Anti Adverts", "1.0.0.1")]
	public static partial class AntiAdverts
	{
		static AntiAdverts()
		{
			Reports = new List<AntiAdvertsReport>(100);
		}

		private static void CMConfig()
		{
			EventSink.Speech += OnSpeech;
		}

		private static void CMEnabled()
		{
			EventSink.Speech += OnSpeech;
		}

		private static void CMDisabled()
		{
			EventSink.Speech -= OnSpeech;
		}

		private static void CMInvoke()
		{
			CommandUtility.Register(
				"AntiAds",
				Access,
				e =>
				{
					if (e.Mobile is PlayerMobile)
					{
						SuperGump.Send(new AntiAvertsReportsGump((PlayerMobile)e.Mobile));
					}
				});
		}

		private static void CMSave()
		{
			VitaNexCore.TryCatch(() => ReportsFile.Serialize(SerializeReports), CMOptions.ToConsole);
		}

		private static void CMLoad()
		{
			VitaNexCore.TryCatch(() => ReportsFile.Deserialize(DeserializeReports), CMOptions.ToConsole);
		}

		private static void CMDisposed()
		{ }

		private static void SerializeReports(GenericWriter writer)
		{
			writer.SetVersion(0);

			writer.WriteBlockList(Reports, (w, r) => r.Serialize(w));
		}

		private static void DeserializeReports(GenericReader reader)
		{
			reader.GetVersion();

			reader.ReadBlockList(r => new AntiAdvertsReport(r), Reports);
		}
	}
}