#region Header
//   Vorspire    _,-'/-'/  ArcadeOptions.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server;
#endregion

namespace VitaNex.Modules.Games
{
	public sealed class ArcadeOptions : CoreModuleOptions
	{
		public ArcadeOptions()
			: base(typeof(Arcade))
		{ }

		/*
		public override void Clear()
		{
			base.Clear();
		}

		public override void Reset()
		{
			base.Reset();
		}
		*/

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}
}