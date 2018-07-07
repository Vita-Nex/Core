#region Header
//   Vorspire    _,-'/-'/  ResistBuffInfo.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Linq;

using Server;
#endregion

namespace VitaNex
{
	public class UniqueResistMod : ResistanceMod
	{
		public static bool ApplyTo(Mobile m, ResistanceType type, string name, int offset)
		{
			if (m == null || m.Deleted)
			{
				return false;
			}

			RemoveFrom(m, type, name);

			return new UniqueResistMod(type, name, offset).ApplyTo(m);
		}

		public static bool RemoveFrom(Mobile m, ResistanceType type, string name)
		{
			if (m == null || m.Deleted)
			{
				return false;
			}

			if (m.ResistanceMods != null)
			{
				var mod = m.ResistanceMods.OfType<UniqueResistMod>().FirstOrDefault(rm => rm.Type == type && rm.Name == name);

				if (mod != null)
				{
					return mod.RemoveFrom(m);
				}
			}

			return false;
		}

		public string Name { get; set; }

		public UniqueResistMod(ResistanceType res, string name, int value)
			: base(res, value)
		{
			Name = name;
		}

		public virtual bool ApplyTo(Mobile from)
		{
			if (from == null || from.Deleted)
			{
				return false;
			}

			from.AddResistanceMod(this);
			return true;
		}

		public virtual bool RemoveFrom(Mobile from)
		{
			if (from == null || from.Deleted)
			{
				return false;
			}

			from.RemoveResistanceMod(this);
			return true;
		}
	}

	public class ResistBuffInfo : PropertyObject, IEquatable<ResistBuffInfo>, IEquatable<ResistanceMod>
	{
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public ResistanceType Type { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Offset { get; set; }

		public ResistBuffInfo(ResistanceType type, int offset)
		{
			Type = type;
			Offset = offset;
		}

		public ResistBuffInfo(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{ }

		public override void Reset()
		{ }

		public ResistBuffInfo Clone()
		{
			return new ResistBuffInfo(Type, Offset);
		}

		public ResistanceMod ToResistMod()
		{
			return new ResistanceMod(Type, Offset);
		}

		public override string ToString()
		{
			return Type.ToString();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int)Type * 397) ^ Offset;
			}
		}

		public override bool Equals(object obj)
		{
			return (obj is ResistanceMod && Equals((ResistanceMod)obj) ||
					(obj is ResistBuffInfo && Equals((ResistBuffInfo)obj)));
		}

		public virtual bool Equals(ResistanceMod mod)
		{
			return !ReferenceEquals(mod, null) && Type == mod.Type && Offset == mod.Offset;
		}

		public virtual bool Equals(ResistBuffInfo info)
		{
			return !ReferenceEquals(info, null) && Type == info.Type && Offset == info.Offset;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
				{
					writer.WriteFlag(Type);
					writer.Write(Offset);
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
					Type = reader.ReadFlag<ResistanceType>();
					Offset = reader.ReadInt();
				}
					break;
			}
		}

		public static bool operator ==(ResistBuffInfo l, ResistBuffInfo r)
		{
			return ReferenceEquals(l, null) ? ReferenceEquals(r, null) : l.Equals(r);
		}

		public static bool operator !=(ResistBuffInfo l, ResistBuffInfo r)
		{
			return ReferenceEquals(l, null) ? !ReferenceEquals(r, null) : !l.Equals(r);
		}

		public static bool operator ==(ResistBuffInfo l, ResistanceMod r)
		{
			return ReferenceEquals(l, null) ? ReferenceEquals(r, null) : l.Equals(r);
		}

		public static bool operator !=(ResistBuffInfo l, ResistanceMod r)
		{
			return ReferenceEquals(l, null) ? !ReferenceEquals(r, null) : !l.Equals(r);
		}

		public static bool operator ==(ResistanceMod l, ResistBuffInfo r)
		{
			return r == l;
		}

		public static bool operator !=(ResistanceMod l, ResistBuffInfo r)
		{
			return r != l;
		}

		public static implicit operator ResistanceMod(ResistBuffInfo info)
		{
			return new ResistanceMod(info.Type, info.Offset);
		}
	}
}