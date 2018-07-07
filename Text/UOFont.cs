#region Header
//   Vorspire    _,-'/-'/  UOFont.cs
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using Server;
#endregion

namespace VitaNex.Text
{
	public enum UOEncoding : byte
	{
		Ascii,
		Unicode
	}

	public sealed class UOFont
	{
		public const PixelFormat PixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppArgb1555;

		public static Size DefaultCharSize = new Size(8, 10);

		public static UOFont[] Ascii { get; private set; }
		public static UOFont[] Unicode { get; private set; }

		private static Bitmap NewEmptyImage()
		{
			return new Bitmap(DefaultCharSize.Width, DefaultCharSize.Height, PixelFormat);
		}

		private static UOChar NewEmptyChar(UOEncoding enc)
		{
			return new UOChar(enc, 0, 0, NewEmptyImage());
		}

		private static readonly byte[] _EmptyBuffer = new byte[0];

		private static readonly UOChar[][][] _Chars;

		static UOFont()
		{
			_Chars = new UOChar[2][][];

			LoadAscii(out _Chars[0]);
			LoadUnicode(out _Chars[1]);

			Ascii = new UOFont[_Chars[0].Length];
			Unicode = new UOFont[_Chars[1].Length];

			Ascii.SetAll(i => Instantiate(UOEncoding.Ascii, (byte)i));
			Unicode.SetAll(i => Instantiate(UOEncoding.Unicode, (byte)i));
		}

		public static void Configure()
		{ }

		private static UOFont Instantiate(UOEncoding enc, byte id)
		{
			int charsWidth = 0, charsHeight = 0;

			foreach (var o in _Chars[(byte)enc][id])
			{
				charsWidth = Math.Max(charsWidth, o.XOffset + o.Width);
				charsHeight = Math.Max(charsHeight, o.YOffset + o.Height);
			}

			return new UOFont(enc, id, 1, 4, (byte)charsWidth, (byte)charsHeight);
		}

		private static void LoadAscii(out UOChar[][] fonts)
		{
			const UOEncoding enc = UOEncoding.Ascii;

			fonts = new UOChar[10][];
			fonts.SetAll(i => new UOChar[0x100]);

			var path = Core.FindDataFile("fonts.mul");

			if (path == null || !File.Exists(path))
			{
				foreach (var chars in fonts)
				{
					chars.SetAll(i => NewEmptyChar(enc));
				}

				return;
			}

			using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				using (var bin = new BinaryReader(fs))
				{
					foreach (var chars in fonts)
					{
						bin.ReadByte(); // header

						for (var c = 0; c < 32; c++)
						{
							chars[c] = NewEmptyChar(enc);
						}

						for (var c = 32; c < chars.Length; c++)
						{
							var width = bin.ReadByte();
							var height = bin.ReadByte();

							bin.ReadByte(); // unk

							var buffer = _EmptyBuffer;

							if (width * height > 0)
							{
								buffer = bin.ReadBytes((width * height) * 2);
							}

							chars[c] = new UOChar(enc, 0, 0, GetImage(width, height, buffer, enc));
						}
					}
				}
			}
		}

		private static void LoadUnicode(out UOChar[][] fonts)
		{
			const UOEncoding enc = UOEncoding.Unicode;

			fonts = new UOChar[13][];
			fonts.SetAll(i => new UOChar[0x10000]);

			for (var id = 0; id < fonts.Length; id++)
			{
				var chars = fonts[id];

				var filePath = Core.FindDataFile("unifont{0:#}.mul", id);

				if (filePath == null)
				{
					chars.SetAll(i => NewEmptyChar(enc));
					continue;
				}

				using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
				{
					using (var bin = new BinaryReader(fs))
					{
						for (int c = 0, o; c < chars.Length; c++)
						{
							fs.Seek(c * 4, SeekOrigin.Begin);

							o = bin.ReadInt32();

							if (o <= 0 || o >= fs.Length)
							{
								chars[c] = NewEmptyChar(enc);
								continue;
							}

							fs.Seek(o, SeekOrigin.Begin);

							var x = bin.ReadSByte(); // x-offset
							var y = bin.ReadSByte(); // y-offset

							var width = bin.ReadByte();
							var height = bin.ReadByte();

							var buffer = _EmptyBuffer;

							if (width * height > 0)
							{
								buffer = bin.ReadBytes(height * (((width - 1) / 8) + 1));
							}

							chars[c] = new UOChar(enc, x, y, GetImage(width, height, buffer, enc));
						}
					}
				}
			}
		}

		private static unsafe Bitmap GetImage(int width, int height, byte[] buffer, UOEncoding enc)
		{
			if (width * height <= 0 || buffer.IsNullOrEmpty())
			{
				return NewEmptyImage();
			}

			var image = new Bitmap(width, height, PixelFormat);
			var bound = new Rectangle(0, 0, width, height);
			var data = image.LockBits(bound, ImageLockMode.WriteOnly, PixelFormat);

			var index = 0;

			var line = (ushort*)data.Scan0;
			var delta = data.Stride >> 1;

			int x, y;
			ushort pixel;

			for (y = 0; y < height; y++, line += delta)
			{
				var cur = line;

				if (cur == null)
				{
					continue;
				}

				for (x = 0; x < width; x++)
				{
					pixel = 0;

					if (enc > 0)
					{
						index = x / 8 + y * ((width + 7) / 8);
					}

					if (index < buffer.Length)
					{
						if (enc > 0)
						{
							pixel = buffer[index];
						}
						else
						{
							pixel = (ushort)(buffer[index++] | (buffer[index++] << 8));
						}
					}

					if (enc > 0)
					{
						pixel &= (ushort)(1 << (7 - (x % 8)));
					}

					if (pixel == 0)
					{
						cur[x] = 0;
					}
					else if (enc > 0)
					{
						cur[x] = 0x8000;
					}
					else
					{
						cur[x] = (ushort)(pixel ^ 0x8000);
					}
				}
			}

			image.UnlockBits(data);

			return image;
		}

		public static Bitmap GetImage(UOFont font, char c)
		{
			return font[c].GetImage();
		}

		public static Bitmap GetAsciiImage(byte font, char c)
		{
			if (!Ascii.InBounds(font))
			{
				font = 3;
			}

			return GetImage(Ascii[font], c);
		}

		public static Bitmap GetUnicodeImage(byte font, char c)
		{
			if (!Unicode.InBounds(font))
			{
				font = 1;
			}

			return GetImage(Unicode[font], c);
		}

		public static int GetAsciiWidth(byte font, string text)
		{
			if (!Ascii.InBounds(font))
			{
				font = 3;
			}

			return Ascii[font].GetWidth(text);
		}

		public static int GetAsciiHeight(byte font, string text)
		{
			if (!Ascii.InBounds(font))
			{
				font = 3;
			}

			return Ascii[font].GetHeight(text);
		}

		public static Size GetAsciiSize(byte font, string text)
		{
			if (!Ascii.InBounds(font))
			{
				font = 3;
			}

			return Ascii[font].GetSize(text);
		}

		public static int GetAsciiWidth(byte font, params string[] lines)
		{
			if (!Ascii.InBounds(font))
			{
				font = 3;
			}

			return Ascii[font].GetWidth(lines);
		}

		public static int GetAsciiHeight(byte font, params string[] lines)
		{
			if (!Ascii.InBounds(font))
			{
				font = 3;
			}

			return Ascii[font].GetHeight(lines);
		}

		public static Size GetAsciiSize(byte font, params string[] lines)
		{
			if (!Ascii.InBounds(font))
			{
				font = 3;
			}

			return Ascii[font].GetSize(lines);
		}

		public static int GetUnicodeWidth(byte font, string text)
		{
			if (!Unicode.InBounds(font))
			{
				font = 1;
			}

			return Unicode[font].GetWidth(text);
		}

		public static int GetUnicodeHeight(byte font, string text)
		{
			if (!Unicode.InBounds(font))
			{
				font = 1;
			}

			return Unicode[font].GetHeight(text);
		}

		public static Size GetUnicodeSize(byte font, string text)
		{
			if (!Unicode.InBounds(font))
			{
				font = 1;
			}

			return Unicode[font].GetSize(text);
		}

		public static int GetUnicodeWidth(byte font, params string[] lines)
		{
			if (!Unicode.InBounds(font))
			{
				font = 1;
			}

			return Unicode[font].GetWidth(lines);
		}

		public static int GetUnicodeHeight(byte font, params string[] lines)
		{
			if (!Unicode.InBounds(font))
			{
				font = 1;
			}

			return Unicode[font].GetHeight(lines);
		}

		public static Size GetUnicodeSize(byte font, params string[] lines)
		{
			if (!Unicode.InBounds(font))
			{
				font = 1;
			}

			return Unicode[font].GetSize(lines);
		}

		public UOEncoding Encoding { get; private set; }

		public byte ID { get; private set; }

		public byte MaxCharWidth { get; private set; }
		public byte MaxCharHeight { get; private set; }

		public byte CharSpacing { get; private set; }
		public byte LineSpacing { get; private set; }

		public byte LineHeight { get; private set; }

		public UOChar[] Chars { get; private set; }

		public int Length { get { return Chars.Length; } }

		public UOChar this[char c] { get { return Chars[c % Length]; } }
		public UOChar this[int i] { get { return Chars[i % Length]; } }

		private UOFont(UOEncoding enc, byte id, byte charSpacing, byte lineSpacing, byte charsWidth, byte charsHeight)
		{
			Encoding = enc;

			ID = id;

			CharSpacing = charSpacing;
			LineSpacing = lineSpacing;
			MaxCharWidth = charsWidth;
			MaxCharHeight = charsHeight;

			Chars = _Chars[(byte)Encoding][ID];
		}

		public int GetWidth(string value)
		{
			return GetSize(value).Width;
		}

		public int GetHeight(string value)
		{
			return GetSize(value).Height;
		}

		public Size GetSize(string value)
		{
			var lines = value.Split('\n');

			if (lines.Length == 0)
			{
				lines = new[] {value};
			}

			return GetSize(lines);
		}

		public int GetWidth(params string[] lines)
		{
			return GetSize(lines).Width;
		}

		public int GetHeight(params string[] lines)
		{
			return GetSize(lines).Height;
		}

		public Size GetSize(params string[] lines)
		{
			var w = 0;
			var h = 0;

			var space = Chars[' '];

			UOChar ci;

			foreach (var line in lines.SelectMany(o => o.Contains('\n') ? o.Split('\n') : o.ToEnumerable()))
			{
				var lw = 0;
				var lh = 0;

				foreach (var c in line)
				{
					if (c == '\t')
					{
						lw += (CharSpacing + space.Width) * 4;
						continue;
					}

					ci = this[c];

					if (ci == null)
					{
						lw += (CharSpacing + space.Width);
						continue;
					}

					lw += (CharSpacing + ci.XOffset + ci.Width);
					lh = Math.Max(lh, ci.YOffset + ci.Height);
				}

				w = Math.Max(w, lw);
				h += lh + LineSpacing;
			}

			return new Size(w, h);
		}

		public override string ToString()
		{
			return String.Format("({0}, {1}, {2})", Encoding, ID, Length);
		}
	}

	public sealed class UOChar
	{
		public Bitmap Image { get; private set; }

		public UOEncoding Encoding { get; private set; }

		public sbyte XOffset { get; private set; }
		public sbyte YOffset { get; private set; }

		public byte Width { get; private set; }
		public byte Height { get; private set; }

		public UOChar(UOEncoding enc, sbyte ox, sbyte oy, Bitmap image)
		{
			Encoding = enc;

			XOffset = ox;
			YOffset = oy;

			Image = image;

			Width = (byte)Image.Width;
			Height = (byte)Image.Height;
		}

		public Bitmap GetImage()
		{
			return GetImage(false);
		}

		public Bitmap GetImage(bool fill)
		{
			return GetImage(fill, Color555.White);
		}

		public Bitmap GetImage(bool fill, Color555 bgColor)
		{
			return GetImage(fill, bgColor, Color555.Black);
		}

		public unsafe Bitmap GetImage(bool fill, Color555 bgColor, Color555 textColor)
		{
			if (Width * Height <= 0)
			{
				return null;
			}

			var image = new Bitmap(Width, Height, UOFont.PixelFormat);

			var bound = new Rectangle(0, 0, Width, Height);

			var dataSrc = Image.LockBits(bound, ImageLockMode.ReadOnly, UOFont.PixelFormat);
			var lineSrc = (ushort*)dataSrc.Scan0;
			var deltaSrc = dataSrc.Stride >> 1;

			var dataTrg = image.LockBits(bound, ImageLockMode.WriteOnly, UOFont.PixelFormat);
			var lineTrg = (ushort*)dataTrg.Scan0;
			var deltaTrg = dataTrg.Stride >> 1;

			int x, y;

			for (y = 0; y < Height; y++, lineSrc += deltaSrc, lineTrg += deltaTrg)
			{
				var source = lineSrc;
				var target = lineTrg;

				if (source == null || target == null)
				{
					continue;
				}

				for (x = 0; x < Width; x++)
				{
					if (source[x] != 0)
					{
						target[x] = textColor;
					}
					else if (fill)
					{
						target[x] = bgColor;
					}
				}
			}

			Image.UnlockBits(dataSrc);
			image.UnlockBits(dataTrg);

			return image;
		}
	}
}