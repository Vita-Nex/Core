#region Header
//   Vorspire    _,-'/-'/  LayeredIconDefinition.cs
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
using System.Drawing;
using System.Linq;

using Server;
using Server.Gumps;
#endregion

namespace VitaNex
{
	[PropertyObject]
	public class LayeredIconDefinition : List<IconDefinition>
	{
		private static readonly Size _Zero = new Size(0, 0);

		public static void AddTo(Gump gump, int x, int y, LayeredIconDefinition icon)
		{
			if (gump != null && icon != null)
			{
				icon.AddToGump(gump, x, y);
			}
		}

		public static void AddTo(Gump gump, int x, int y, int hue, LayeredIconDefinition icon)
		{
			if (gump != null && icon != null)
			{
				icon.AddToGump(gump, x, y, hue);
			}
		}

		public static LayeredIconDefinition Empty()
		{
			return new LayeredIconDefinition();
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Layers { get { return Count; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Size Size
		{
			get
			{
				if (Count == 0)
				{
					return _Zero;
				}

				var w = this.Max(o => (o.ComputeOffset ? o.OffsetX : 0) + o.Size.Width);
				var h = this.Max(o => (o.ComputeOffset ? o.OffsetY : 0) + o.Size.Height);
				
				if (w > 0 && h > 0)
				{
					return new Size(w, h);
				}

				return _Zero;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool HasSpellIcon { get { return this.Any(o => o.IsSpellIcon); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool HasSmallSpellIcon { get { return this.Any(o => o.IsSmallSpellIcon); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool HasLargeSpellIcon { get { return this.Any(o => o.IsLargeSpellIcon); } }

		public LayeredIconDefinition()
		{ }

		public LayeredIconDefinition(IEnumerable<IconDefinition> collection)
			: base(collection)
		{ }

		public LayeredIconDefinition(int capacity)
			: base(capacity)
		{ }

		public LayeredIconDefinition(GenericReader reader)
		{
			Deserialize(reader);
		}

		#region Gumps
		public void AddGump(int gumpID)
		{
			Add(IconDefinition.FromGump(gumpID));
		}

		public void AddGump(int gumpID, int hue)
		{
			Add(IconDefinition.FromGump(gumpID, hue));
		}

		public void AddGump(int gumpID, int offsetX, int offsetY)
		{
			Add(IconDefinition.FromGump(gumpID, offsetX, offsetY));
		}

		public void AddGump(int gumpID, int hue, int offsetX, int offsetY)
		{
			Add(IconDefinition.FromGump(gumpID, hue, offsetX, offsetY));
		}

		public void ClearGumps()
		{
			RemoveAll(o => o.AssetType == IconType.GumpArt);
		}
		#endregion

		#region Items
		public void AddItem(int itemID)
		{
			Add(IconDefinition.FromItem(itemID));
		}

		public void AddItem(int itemID, int hue)
		{
			Add(IconDefinition.FromItem(itemID, hue));
		}

		public void AddItem(int itemID, int offsetX, int offsetY)
		{
			Add(IconDefinition.FromItem(itemID, offsetX, offsetY));
		}

		public void AddItem(int itemID, int hue, int offsetX, int offsetY)
		{
			Add(IconDefinition.FromItem(itemID, hue, offsetX, offsetY));
		}

		public void ClearItems()
		{
			RemoveAll(o => o.AssetType == IconType.ItemArt);
		}
		#endregion

		#region Spell Icons
		public void AddSpellIcon()
		{
			Add(IconDefinition.SpellIcon());
		}

		public void AddSpellIcon(int hue)
		{
			Add(IconDefinition.SpellIcon(hue));
		}

		public void AddSpellIcon(int offsetX, int offsetY)
		{
			Add(IconDefinition.SpellIcon(offsetX, offsetY));
		}

		public void AddSpellIcon(int hue, int offsetX, int offsetY)
		{
			Add(IconDefinition.SpellIcon(hue, offsetX, offsetY));
		}

		public void AddSmallSpellIcon()
		{
			Add(IconDefinition.SmallSpellIcon());
		}

		public void AddSmallSpellIcon(int hue)
		{
			Add(IconDefinition.SmallSpellIcon(hue));
		}

		public void AddSmallSpellIcon(int offsetX, int offsetY)
		{
			Add(IconDefinition.SmallSpellIcon(offsetX, offsetY));
		}

		public void AddSmallSpellIcon(int hue, int offsetX, int offsetY)
		{
			Add(IconDefinition.SmallSpellIcon(hue, offsetX, offsetY));
		}

		public void AddLargeSpellIcon()
		{
			Add(IconDefinition.LargeSpellIcon());
		}

		public void AddLargeSpellIcon(int hue)
		{
			Add(IconDefinition.LargeSpellIcon(hue));
		}

		public void AddLargeSpellIcon(int offsetX, int offsetY)
		{
			Add(IconDefinition.LargeSpellIcon(offsetX, offsetY));
		}

		public void AddLargeSpellIcon(int hue, int offsetX, int offsetY)
		{
			Add(IconDefinition.LargeSpellIcon(hue, offsetX, offsetY));
		}

		public void ClearSpellIcons()
		{
			RemoveAll(o => o.IsSpellIcon);
		}

		public void ClearSmallSpellIcons()
		{
			RemoveAll(o => o.IsSmallSpellIcon);
		}

		public void ClearLargeSpellIcons()
		{
			RemoveAll(o => o.IsLargeSpellIcon);
		}
		#endregion

		public int RemoveAll(int assetID)
		{
			return RemoveAll(o => o.AssetID == assetID);
		}

		public virtual void AddToGump(Gump g, int x, int y)
		{
			foreach (var icon in this)
			{
				icon.AddToGump(g, x, y);
			}
		}

		public virtual void AddToGump(Gump g, int x, int y, int hue)
		{
			foreach (var icon in this)
			{
				icon.AddToGump(g, x, y, hue);
			}
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.SetVersion(0);

			writer.WriteList(this, (w, o) => o.Serialize(w));
		}

		public virtual void Deserialize(GenericReader reader)
		{
			reader.GetVersion();

			reader.ReadList(r => new IconDefinition(r), this);
		}
	}
}