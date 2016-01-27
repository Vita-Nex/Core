#region Header
//   Vorspire    _,-'/-'/  CastBars_Init.cs
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
using System.Collections.Generic;
using System.Drawing;

using Server;
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.CastBars
{
	[CoreModule("Spell Cast Bars", "1.0.0.0")]
	public static partial class SpellCastBars
	{
		static SpellCastBars()
		{
			States.OnSerialize = Serialize;
			States.OnDeserialize = Deserialize;

			_InternalTimer = PollTimer.CreateInstance(
				TimeSpan.FromMilliseconds(100.0),
				PollCastBarQueue,
				() => CMOptions.ModuleEnabled,
				false);
		}

		private static void CMInvoke()
		{
			_InternalTimer.Start();
		}

		private static void CMEnabled()
		{
			_InternalTimer.Start();
		}

		private static void CMDisabled()
		{
			_InternalTimer.Stop();
		}

		private static void CMSave()
		{
			var result = States.Export();
			CMOptions.ToConsole("{0} profiles saved, {1}", States.Count.ToString("#,0"), result);
		}

		private static void CMLoad()
		{
			var result = States.Import();
			CMOptions.ToConsole("{0} profiles loaded, {1}.", States.Count.ToString("#,0"), result);
		}

		private static bool Serialize(GenericWriter writer)
		{
			var version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
				{
					writer.WriteBlockDictionary(
						_States,
						(w, k, v) =>
						{
							w.Write(k);
							w.Write(v.Item1);
							w.Write(v.Item2.X);
							w.Write(v.Item2.Y);
						});
				}
					break;
			}

			return true;
		}

		private static bool Deserialize(GenericReader reader)
		{
			var version = reader.GetVersion();

			switch (version)
			{
				case 0:
				{
					reader.ReadBlockDictionary(
						r =>
						{
							var k = r.ReadMobile<PlayerMobile>();
							var v1 = r.ReadBool();
							var v2 = new Point(r.ReadInt(), r.ReadInt());

							return new KeyValuePair<PlayerMobile, Tuple<bool, Point>>(k, new Tuple<bool, Point>(v1, v2));
						},
						_States);
				}
					break;
			}

			return true;
		}
	}
}