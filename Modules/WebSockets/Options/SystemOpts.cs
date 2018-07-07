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

namespace VitaNex.Modules.WebSockets
{
	public class WebSocketsOptions : CoreModuleOptions
	{
		[CommandProperty(WebSockets.Access)]
		public int Port { get; set; }

		[CommandProperty(WebSockets.Access)]
		public int MaxConnections { get; set; }

		public WebSocketsOptions()
			: base(typeof(WebSockets))
		{
			Port = 2594;
			MaxConnections = 1000;
		}

		public WebSocketsOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			base.Clear();

			Port = 2594;
			MaxConnections = 1000;
		}

		public override void Reset()
		{
			base.Reset();

			Port = 2594;
			MaxConnections = 1000;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
				{
					writer.Write(Port);
					writer.Write(MaxConnections);
				}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.GetVersion();

			switch (version)
			{
				case 0:
				{
					Port = reader.ReadInt();
					MaxConnections = reader.ReadInt();
				}
					break;
			}
		}
	}
}