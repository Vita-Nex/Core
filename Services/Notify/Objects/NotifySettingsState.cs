#region Header
//   Vorspire    _,-'/-'/  NotifySettingsState.cs
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
using Server.Accounting;
#endregion

namespace VitaNex.Notify
{
	public sealed class NotifySettingsState : SettingsObject<NotifyFlags>
	{
		[CommandProperty(Notify.Access)]
		public NotifySettings Settings { get; private set; }

		[CommandProperty(Notify.Access, true)]
		public IAccount Owner { get; private set; }

		[CommandProperty(Notify.Access)]
		public override NotifyFlags Flags { get { return base.Flags; } set { base.Flags = value; } }

		[CommandProperty(Notify.Access)]
		public bool AutoClose
		{
			get { return GetFlag(NotifyFlags.AutoClose); }
			set { SetFlag(NotifyFlags.AutoClose, value); }
		}

		[CommandProperty(Notify.Access)]
		public bool Ignore { get { return GetFlag(NotifyFlags.Ignore); } set { SetFlag(NotifyFlags.Ignore, value); } }

		[CommandProperty(Notify.Access)]
		public bool TextOnly { get { return GetFlag(NotifyFlags.TextOnly); } set { SetFlag(NotifyFlags.TextOnly, value); } }

		[CommandProperty(Notify.Access)]
		public bool Animate { get { return GetFlag(NotifyFlags.Animate); } set { SetFlag(NotifyFlags.Animate, value); } }

		[CommandProperty(Notify.Access)]
		public byte Speed { get; set; }

		public NotifySettingsState(NotifySettings settings, IAccount owner)
		{
			Settings = settings;
			Owner = owner;

			SetDefaults();
		}

		public NotifySettingsState(NotifySettings settings, GenericReader reader)
			: base(reader)
		{
			Settings = settings;
		}

		public override void Clear()
		{
			SetDefaults();
		}

		public override void Reset()
		{
			SetDefaults();
		}

		public void SetDefaults()
		{
			AutoClose = false;
			Ignore = false;
			TextOnly = false;
			Animate = true;

			Speed = 100;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(Owner);

			writer.Write(Speed);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			Owner = reader.ReadAccount();

			Speed = reader.ReadByte();
		}
	}
}