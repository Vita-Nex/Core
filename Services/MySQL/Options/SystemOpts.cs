#region Header
//   Vorspire    _,-'/-'/  SystemOpts.cs
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

namespace VitaNex.MySQL
{
	public class MySQLOptions : CoreServiceOptions
	{
		[CommandProperty(MySQL.Access)]
		public int MaxConnections { get; set; }

		[CommandProperty(MySQL.Access, AccessLevel.Owner)]
		public MySQLConnectionInfo Persistence { get; set; }

		public MySQLOptions()
			: base(typeof(MySQL))
		{
			MaxConnections = 100;
			Persistence = new MySQLConnectionInfo("localhost", 3306, "root", "", ODBCVersion.V_5_3_UNICODE);
		}

		public MySQLOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			base.Clear();

			MaxConnections = 1;
		}

		public override void Reset()
		{
			base.Reset();

			MaxConnections = 100;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					Persistence.Serialize(writer);
					goto case 0;
				case 0:
					writer.Write(MaxConnections);
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.GetVersion();

			switch (version)
			{
				case 1:
					Persistence = new MySQLConnectionInfo(reader);
					goto case 0;
				case 0:
					MaxConnections = reader.ReadInt();
					break;
			}
		}
	}
}