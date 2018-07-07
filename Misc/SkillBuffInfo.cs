#region Header
//   Vorspire    _,-'/-'/  SkillBuffInfo.cs
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
	public class UniqueSkillMod : DefaultSkillMod
	{
		public static bool ApplyTo(Mobile m, SkillName skill, string name, bool relative, double value)
		{
			if (m == null || m.Deleted)
			{
				return false;
			}

			RemoveFrom(m, skill, name);

			return new UniqueSkillMod(skill, name, relative, value).ApplyTo(m);
		}

		public static bool RemoveFrom(Mobile m, SkillName skill, string name)
		{
			if (m == null || m.Deleted)
			{
				return false;
			}

			if (m.SkillMods != null)
			{
				var mod = m.SkillMods.OfType<UniqueSkillMod>().FirstOrDefault(sm => sm.Skill == skill && sm.Name == name);

				if (mod != null)
				{
					return mod.RemoveFrom(m);
				}
			}

			return false;
		}

		public string Name { get; set; }

		public UniqueSkillMod(SkillName skill, string name, bool relative, double value)
			: base(skill, relative, value)
		{
			Name = name;
		}

		public virtual bool ApplyTo(Mobile from)
		{
			if (from == null || from.Deleted || from.Skills[Skill] == null)
			{
				return false;
			}

			from.AddSkillMod(this);
			return true;
		}

		public virtual bool RemoveFrom(Mobile from)
		{
			if (from == null || from.Deleted)
			{
				return false;
			}

			from.RemoveSkillMod(this);
			return true;
		}
	}

	public class SkillBuffInfo : PropertyObject, IEquatable<SkillBuffInfo>, IEquatable<SkillMod>
	{
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public SkillName Skill { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Relative { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public double Value { get; set; }

		public SkillBuffInfo(SkillName skill, bool relative, double value)
		{
			Skill = skill;
			Relative = relative;
			Value = value;
		}

		public SkillBuffInfo(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{ }

		public override void Reset()
		{ }

		public SkillBuffInfo Clone()
		{
			return new SkillBuffInfo(Skill, Relative, Value);
		}

		public SkillMod ToSkillMod()
		{
			return new DefaultSkillMod(Skill, Relative, Value);
		}

		public override string ToString()
		{
			return Skill.ToString();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = (int)Skill;
				hash = (hash * 397) ^ Relative.GetHashCode();
				hash = (hash * 397) ^ Value.GetHashCode();
				return hash;
			}
		}

		public override bool Equals(object obj)
		{
			return (obj is SkillMod && Equals((SkillMod)obj)) || (obj is SkillBuffInfo && Equals((SkillBuffInfo)obj));
		}

		public virtual bool Equals(SkillMod mod)
		{
			return !ReferenceEquals(mod, null) && Value == mod.Value && Relative == mod.Relative && Skill == mod.Skill;
		}

		public virtual bool Equals(SkillBuffInfo info)
		{
			return !ReferenceEquals(info, null) && Value == info.Value && Relative == info.Relative && Skill == info.Skill;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
				{
					writer.WriteFlag(Skill);
					writer.Write(Relative);
					writer.Write(Value);
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
					Skill = reader.ReadFlag<SkillName>();
					Relative = reader.ReadBool();
					Value = reader.ReadDouble();
				}
					break;
			}
		}

		public static bool operator ==(SkillBuffInfo l, SkillBuffInfo r)
		{
			return ReferenceEquals(l, null) ? ReferenceEquals(r, null) : l.Equals(r);
		}

		public static bool operator !=(SkillBuffInfo l, SkillBuffInfo r)
		{
			return ReferenceEquals(l, null) ? !ReferenceEquals(r, null) : !l.Equals(r);
		}

		public static bool operator ==(SkillBuffInfo l, SkillMod r)
		{
			return ReferenceEquals(l, null) ? ReferenceEquals(r, null) : l.Equals(r);
		}

		public static bool operator !=(SkillBuffInfo l, SkillMod r)
		{
			return ReferenceEquals(l, null) ? !ReferenceEquals(r, null) : !l.Equals(r);
		}

		public static bool operator ==(SkillMod l, SkillBuffInfo r)
		{
			return r == l;
		}

		public static bool operator !=(SkillMod l, SkillBuffInfo r)
		{
			return r != l;
		}

		public static implicit operator DefaultSkillMod(SkillBuffInfo info)
		{
			return new DefaultSkillMod(info.Skill, info.Relative, info.Value);
		}
	}
}