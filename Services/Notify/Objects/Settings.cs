#region Header
//   Vorspire    _,-'/-'/  Settings.cs
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

using Server;
using Server.Accounting;
#endregion

namespace VitaNex.Notify
{
	public sealed class NotifySettings : PropertyObject
	{
		public Dictionary<IAccount, NotifySettingsState> States { get; private set; }

		public Type Type { get; private set; }

		public string Name { get; set; }
		public string Desc { get; set; }

		public AccessLevel Access { get; set; }

		public bool CanIgnore { get; set; }

		public NotifySettings(Type t)
		{
			Type = t;
			Name = t.Name.Replace("Notify", String.Empty).Replace("Gump", String.Empty).SpaceWords();
			Desc = String.Empty;
			Access = AccessLevel.Player;
			States = new Dictionary<IAccount, NotifySettingsState>();
		}

		public NotifySettings(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			States.Clear();
		}

		public override void Reset()
		{
			States.Clear();
		}

		public bool IsIgnored(Mobile m)
		{
			return m != null && m.Account != null && States.ContainsKey(m.Account) && States[m.Account].Ignore;
		}

		public bool IsAnimated(Mobile m)
		{
			return m == null || m.Account == null || !States.ContainsKey(m.Account) || States[m.Account].Animate;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					writer.Write(Desc);
					goto case 0;
				case 0:
				{
					writer.WriteType(Type);
					writer.Write(Name);
					writer.Write(CanIgnore);

					writer.WriteBlockDictionary(States, (w, k, v) => v.Serialize(w));
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
				case 1:
					Desc = reader.ReadString();
					goto case 0;
				case 0:
				{
					Type = reader.ReadType();
					Name = reader.ReadString();
					CanIgnore = reader.ReadBool();

					States = reader.ReadBlockDictionary(
						r =>
						{
							var state = new NotifySettingsState(this, r);

							return new KeyValuePair<IAccount, NotifySettingsState>(state.Owner.Account, state);
						});
				}
					break;
			}
		}
	}

	public sealed class NotifySettingsState : PropertyObject
	{
		public NotifySettings Settings { get; private set; }

		public Mobile Owner { get; private set; }

		public bool Ignore { get; set; }
		public bool Animate { get; set; }

		public NotifySettingsState(NotifySettings settings, Mobile owner)
		{
			Settings = settings;
			Owner = owner;

			Ignore = false;
			Animate = true;
		}

		public NotifySettingsState(NotifySettings settings, GenericReader reader)
			: base(reader)
		{
			Settings = settings;
		}

		public override void Clear()
		{ }

		public override void Reset()
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(Owner);

			writer.Write(Ignore);
			writer.Write(Animate);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			Owner = reader.ReadMobile<Mobile>();

			Ignore = reader.ReadBool();
			Animate = reader.ReadBool();
		}
	}
}