#region Header
//   Vorspire    _,-'/-'/  ArtworkSupport_Init.cs
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
using System.IO;

using Server;

using VitaNex.Network;
#endregion

namespace VitaNex
{
	[CoreService("Artwork Support", "1.0.0.0", TaskPriority.High)]
	public static partial class ArtworkSupport
	{
		static ArtworkSupport()
		{
			CSOptions = new CoreServiceOptions(typeof(ArtworkSupport));

			Info = new Dictionary<Type, List<ArtworkInfo>>();

			LandTextures = new short[TileData.MaxLandValue];
			LandTextures.SetAll((short)-1);

			StaticAnimations = new short[TileData.MaxItemValue];
			StaticAnimations.SetAll((short)-1);

			var filePath = Core.FindDataFile("tiledata.mul");

			if (String.IsNullOrWhiteSpace(filePath))
			{
				return;
			}

			var file = new FileInfo(filePath);

			if (!file.Exists)
			{
				return;
			}

			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var bin = new BinaryReader(fs);
				var buffer = new byte[25];

				if (fs.Length == 3188736)
				{
					// 7.0.9.0
					LandTextures = new short[0x4000];

					for (var i = 0; i < 0x4000; ++i)
					{
						if (i == 1 || (i > 0 && (i & 0x1F) == 0))
						{
							bin.Read(buffer, 0, 4);
						}

						bin.Read(buffer, 0, 8);

						var texture = bin.ReadInt16();

						bin.Read(buffer, 0, 20);

						LandTextures[i] = texture;
					}

					StaticAnimations = new short[0x10000];

					for (var i = 0; i < 0x10000; ++i)
					{
						if ((i & 0x1F) == 0)
						{
							bin.Read(buffer, 0, 4);
						}

						bin.Read(buffer, 0, 14);

						var anim = bin.ReadInt16();

						bin.Read(buffer, 0, 25);

						StaticAnimations[i] = anim;
					}
				}
				else
				{
					LandTextures = new short[0x4000];

					for (var i = 0; i < 0x4000; ++i)
					{
						if ((i & 0x1F) == 0)
						{
							bin.Read(buffer, 0, 4);
						}

						bin.Read(buffer, 0, 4);

						var texture = bin.ReadInt16();

						bin.Read(buffer, 0, 20);

						LandTextures[i] = texture;
					}

					if (fs.Length == 1644544)
					{
						// 7.0.0.0
						StaticAnimations = new short[0x8000];

						for (var i = 0; i < 0x8000; ++i)
						{
							if ((i & 0x1F) == 0)
							{
								bin.Read(buffer, 0, 4);
							}

							bin.Read(buffer, 0, 10);

							var anim = bin.ReadInt16();

							bin.Read(buffer, 0, 25);

							StaticAnimations[i] = anim;
						}
					}
					else
					{
						StaticAnimations = new short[0x4000];

						for (var i = 0; i < 0x4000; ++i)
						{
							if ((i & 0x1F) == 0)
							{
								bin.Read(buffer, 0, 4);
							}

							bin.Read(buffer, 0, 10);

							var anim = bin.ReadInt16();

							bin.Read(buffer, 0, 25);

							StaticAnimations[i] = anim;
						}
					}
				}
			}
		}

		private static void CSConfig()
		{
			_Parent0x1A = OutgoingPacketOverrides.GetHandler(0x1A);
			_Parent0xF3 = OutgoingPacketOverrides.GetHandler(0xF3);

			OutgoingPacketOverrides.Register(0x1A, HandleWorldItem);
			OutgoingPacketOverrides.Register(0xF3, HandleWorldItemSAHS);
		}

		private static void CSInvoke()
		{
			ArtworkSupportAttribute[] attrs;

			foreach (var child in typeof(Item).FindChildren())
			{
				if (!child.GetCustomAttributes(false, out attrs))
				{
					continue;
				}

				foreach (var attr in attrs)
				{
					Register(child, attr.HighVersion, attr.LowVersion, attr.HighItemID, attr.LowItemID);
				}
			}
		}
	}
}