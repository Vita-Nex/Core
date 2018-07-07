#region Header
//   Vorspire    _,-'/-'/  SpellUtility.cs
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
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Spells;
#endregion

namespace VitaNex
{
	public static class SpellUtility
	{
		public static string[] CircleNames =
		{
			"First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Necromancy", "Chivalry", "Bushido",
			"Ninjitsu", "Spellweaving", "Mystic"
		};

		public static Dictionary<Type, SpellInfo> SpellsInfo { get; private set; }
		public static Dictionary<Type, int> ItemSpellIcons { get; private set; }
		public static Dictionary<string, Dictionary<Type, SpellInfo>> TreeStructure { get; private set; }

		static SpellUtility()
		{
			SpellsInfo = new Dictionary<Type, SpellInfo>();
			ItemSpellIcons = new Dictionary<Type, int>();
			TreeStructure = new Dictionary<string, Dictionary<Type, SpellInfo>>();
		}

		[CallPriority(Int16.MaxValue)]
		public static void Initialize()
		{
			Spell s;
			SpellInfo o;
			Type t;

			for (int i = 0, j = 8320; i < SpellRegistry.Types.Length; i++, j++)
			{
				s = SpellRegistry.NewSpell(i, null, null);

				if (s == null)
				{
					continue;
				}

				o = s.Info;

				if (o == null)
				{
					continue;
				}

				t = SpellRegistry.Types[i] ?? s.GetType();

				SpellsInfo[t] = new SpellInfo(
					o.Name,
					o.Mantra,
					o.Action,
					o.LeftHandEffect,
					o.RightHandEffect,
					o.AllowTown,
					o.Reagents);

				if (SpellIcons.IsItemIcon(j))
				{
					ItemSpellIcons[t] = j;
				}
			}

			InvalidateTreeStructure();
		}

		public static void InvalidateTreeStructure()
		{
			TreeStructure.Clear();

			foreach (var c in CircleNames)
			{
				var d = new Dictionary<Type, SpellInfo>();

				foreach (var o in SpellsInfo.Where(o => Insensitive.StartsWith(o.Key.FullName, "Server.Spells." + c)))
				{
					d[o.Key] = o.Value;
				}

				TreeStructure[c] = d;
			}
		}

		public static SpellInfo GetSpellInfo<TSpell>()
			where TSpell : ISpell
		{
			return GetSpellInfo(typeof(TSpell));
		}

		public static SpellInfo GetSpellInfo(int spellID)
		{
			return SpellRegistry.Types.InBounds(spellID) ? GetSpellInfo(SpellRegistry.Types[spellID]) : null;
		}

		public static SpellInfo GetSpellInfo(ISpell s)
		{
			return s != null ? GetSpellInfo(s.GetType()) : null;
		}

		public static SpellInfo GetSpellInfo(Type type)
		{
			return SpellsInfo.GetValue(type);
		}

		public static string GetSpellName<TSpell>()
			where TSpell : ISpell
		{
			return GetSpellName(typeof(TSpell));
		}

		public static string GetSpellName(int spellID)
		{
			return SpellRegistry.Types.InBounds(spellID) ? GetSpellName(SpellRegistry.Types[spellID]) : String.Empty;
		}

		public static string GetSpellName(ISpell s)
		{
			return s != null ? GetSpellName(s.GetType()) : String.Empty;
		}

		public static string GetSpellName(Type type)
		{
			if (type == null)
			{
				return String.Empty;
			}

			var o = GetSpellInfo(type);

			return o != null ? o.Name : String.Empty;
		}

		public static int GetItemIcon<TSpell>()
			where TSpell : ISpell
		{
			return GetItemIcon(typeof(TSpell));
		}

		public static int GetItemIcon(int spellID)
		{
			return SpellRegistry.Types.InBounds(spellID) ? GetItemIcon(SpellRegistry.Types[spellID]) : 0;
		}

		public static int GetItemIcon(ISpell s)
		{
			return s != null ? GetItemIcon(s.GetType()) : 0;
		}

		public static int GetItemIcon(Type type)
		{
			return ItemSpellIcons.GetValue(type);
		}

		public static string GetCircleName<TSpell>()
			where TSpell : ISpell
		{
			return GetCircleName(typeof(TSpell));
		}

		public static string GetCircleName(int spellID)
		{
			return SpellRegistry.Types.InBounds(spellID) ? GetCircleName(SpellRegistry.Types[spellID]) : String.Empty;
		}

		public static string GetCircleName(ISpell s)
		{
			return s != null ? GetCircleName(s.GetType()) : String.Empty;
		}

		public static string GetCircleName(Type type)
		{
			if (type == null)
			{
				return String.Empty;
			}

			if (TreeStructure == null || TreeStructure.Count == 0)
			{
				InvalidateTreeStructure();
			}

			if (TreeStructure == null || TreeStructure.Count == 0)
			{
				return String.Empty;
			}

			return TreeStructure.FirstOrDefault(o => o.Value.ContainsKey(type)).Key ?? String.Empty;
		}

		public static int GetCircle<TSpell>()
			where TSpell : ISpell
		{
			return GetCircle(typeof(TSpell));
		}

		public static int GetCircle(int spellID)
		{
			return SpellRegistry.Types.InBounds(spellID) ? GetCircle(SpellRegistry.Types[spellID]) : 0;
		}

		public static int GetCircle(ISpell s)
		{
			return s != null ? GetCircle(s.GetType()) : 0;
		}

		public static int GetCircle(Type type)
		{
			if (type == null)
			{
				return 0;
			}

			var circle = GetCircleName(type);

			return !String.IsNullOrWhiteSpace(circle) ? CircleNames.IndexOf(circle) + 1 : 0;
		}

		public static IEnumerable<Type> FindCircleSpells(int circleID)
		{
			if (!CircleNames.InBounds(--circleID))
			{
				return Enumerable.Empty<Type>();
			}

			return FindCircleSpells(CircleNames[circleID]);
		}

		public static IEnumerable<Type> FindCircleSpells(string circle)
		{
			if (SpellsInfo == null || SpellsInfo.Count == 0)
			{
				yield break;
			}

			if (TreeStructure == null || TreeStructure.Count == 0)
			{
				InvalidateTreeStructure();
			}

			if (TreeStructure == null || TreeStructure.Count == 0)
			{
				yield break;
			}

			circle = TreeStructure.Keys.FirstOrDefault(c => Insensitive.EndsWith(circle, c));

			if (circle == null)
			{
				yield break;
			}

			var spells = TreeStructure[circle];

			if (spells == null || spells.Count == 0)
			{
				yield break;
			}

			foreach (var t in spells.Keys)
			{
				yield return t;
			}
		}
	}
}