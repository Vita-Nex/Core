#region Header
//   Vorspire    _,-'/-'/  IconDefinition.cs
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

using Server;
using Server.Gumps;
#endregion

namespace VitaNex
{
	public enum IconType
	{
		GumpArt,
		ItemArt
	}

	[PropertyObject]
	public struct IconDefinition : IEquatable<IconDefinition>
	{
		public static readonly IconDefinition EmptyGumpIcon = new IconDefinition(IconType.GumpArt, 0);
		public static readonly IconDefinition EmptyItemIcon = new IconDefinition(IconType.ItemArt, 0);

		private IconType _AssetType;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public IconType AssetType
		{
			get { return _AssetType; }
			set
			{
				if (_AssetType == value)
				{
					return;
				}

				_AssetType = value;
				_AssetID = 0;
			}
		}

		private int _AssetID;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int AssetID
		{
			get { return _AssetID; }
			set
			{
				if (_AssetID == value)
				{
					return;
				}

				if (value < 0)
				{
					value = 0;
				}
				else if (_AssetType == IconType.ItemArt && value > TileData.MaxItemValue)
				{
					_AssetType = IconType.GumpArt;
				}

				_AssetID = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool IsEmpty { get { return _AssetID <= 0; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool IsGumpArt { get { return _AssetType == IconType.GumpArt; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool IsItemArt { get { return _AssetType == IconType.ItemArt; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Hue { get; set; }

		public int Width { get { return 0; } }
		public int Height { get { return 0; } }

		public IconDefinition(IconType assetType, int assetID)
			: this(assetType, assetID, 0)
		{ }

		public IconDefinition(IconType assetType, int assetID, int hue)
			: this()
		{
			_AssetType = assetType;

			if (assetID < 0)
			{
				assetID = 0;
			}
			else if (_AssetType == IconType.ItemArt && assetID > TileData.MaxItemValue)
			{
				_AssetType = IconType.GumpArt;
			}

			_AssetID = assetID;

			Hue = hue;
		}

		public IconDefinition(GenericReader reader)
			: this()
		{
			Deserialize(reader);
		}

		public void AddToGump(Gump g, int x, int y)
		{
			if (Hue > 0)
			{
				if (AssetType == IconType.ItemArt)
				{
					g.AddItem(x, y, AssetID, Hue);
				}
				else
				{
					g.AddImage(x, y, AssetID, Hue);
				}
			}
			else
			{
				if (AssetType == IconType.ItemArt)
				{
					g.AddItem(x, y, AssetID);
				}
				else
				{
					g.AddImage(x, y, AssetID);
				}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			var version = writer.SetVersion(1);

			if (version > 0)
			{
				writer.Write(Hue);
			}

			writer.WriteFlag(_AssetType);
			writer.Write(_AssetID);
		}

		public void Deserialize(GenericReader reader)
		{
			var version = reader.GetVersion();

			if (version > 0)
			{
				Hue = reader.ReadInt();
			}

			_AssetType = reader.ReadFlag<IconType>();
			_AssetID = reader.ReadInt();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = AssetID;
				hash = (hash * 397) ^ (int)AssetType;
				return hash;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is IconDefinition && Equals((IconDefinition)obj);
		}

		public bool Equals(IconDefinition other)
		{
			return Equals(_AssetType, other._AssetType) && Equals(_AssetID, other._AssetID);
		}

		public static bool operator ==(IconDefinition l, IconDefinition r)
		{
			return l.Equals(r);
		}

		public static bool operator !=(IconDefinition l, IconDefinition r)
		{
			return !l.Equals(r);
		}
	}
}