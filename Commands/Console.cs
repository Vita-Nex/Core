#region Header
//   Vorspire    _,-'/-'/  Console.cs
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
using System.Drawing;
using System.IO;
using System.Linq;

using Server;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.Text;
#endregion

namespace VitaNex.Commands
{
	public static class ConsoleCommand
	{
		private static bool _Configured;

		[CallPriority(Int32.MinValue)]
		public static void Configure()
		{
			if (_Configured)
			{
				return;
			}

			_Configured = true;

			ConsoleBuffer.Instance = new ConsoleBuffer(0x4000);

			CommandUtility.Register(
				"Console",
				AccessLevel.Administrator,
				e =>
				{
					if (e.Mobile is PlayerMobile)
					{
						new ConsoleGump((PlayerMobile)e.Mobile).Send();
					}
				});
		}

		private class ConsoleBuffer : StreamWriter
		{
			private static ConsoleBuffer _Instance;

			public static ConsoleBuffer Instance
			{
				get { return _Instance; }
				set
				{
					if (_Instance == value)
					{
						return;
					}

					if (_Instance != null)
					{
						_Instance.Close();
						_Instance.Dispose();
					}

					_Instance = value;
				}
			}

			public string Output { get; private set; }

			public int Limit { get; private set; }

			public ConsoleBuffer(int limit)
				: base(new HookStream())
			{
				Limit = limit;

				((HookStream)BaseStream).Buffer = this;
				
				//AutoFlush = true;

				Core.MultiConsoleOut.Add(this);
			}

			public override void Close()
			{
				Core.MultiConsoleOut.Remove(this);

				base.Close();
			}

			protected override void Dispose(bool disposing)
			{
				Core.MultiConsoleOut.Remove(this);

				base.Dispose(disposing);
			}

			private class HookStream : Stream
			{
				public override bool CanRead { get { return true; } }
				public override bool CanSeek { get { return true; } }
				public override bool CanWrite { get { return true; } }

				public override long Length { get { return 0; } }
				public override long Position { get; set; }

				public ConsoleBuffer Buffer { get; set; }

				public override long Seek(long offset, SeekOrigin origin)
				{
					return 0;
				}

				public override void SetLength(long value)
				{ }

				public override int ReadByte()
				{
					return 0;
				}

				public override int Read(byte[] buffer, int offset, int count)
				{
					return 0;
				}

				public override void WriteByte(byte value)
				{
					Buffer.Output += Convert.ToChar(value);
				}

				public override void Write(byte[] buffer, int offset, int count)
				{
					Buffer.Output += System.Text.Encoding.UTF8.GetString(buffer, offset, count);

					if (Buffer.Output.Length > Buffer.Limit)
					{
						Buffer.Output = Buffer.Output.Substring(Buffer.Output.Length - Buffer.Limit);
					}
				}

				public override void Flush()
				{ }
			}
		}

		public class ConsoleGump : SuperGump
		{
			private List<string> _Buffer;
			private int _Index;

			public string Output { get; set; }

			public int Width { get; set; }
			public int Height { get; set; }

			public ConsoleGump(PlayerMobile user, int width = 700, int height = 500)
				: base(user, null, null)
			{
				_Buffer = new List<string>(0x200);

				Output = ConsoleBuffer.Instance.Output;

				Width = width;
				Height = height;

				CanClose = true;
				CanDispose = true;
				CanMove = true;
				CanResize = true;

				AutoRefreshRate = TimeSpan.FromSeconds(1.0);
				AutoRefresh = true;
			}

			protected override bool CanAutoRefresh()
			{
				return base.CanAutoRefresh() && Output != ConsoleBuffer.Instance.Output;
			}

			protected override void Compile()
			{
				Output = ConsoleBuffer.Instance.Output;

				Width = Math.Max(100, Math.Min(800, Width));
				Height = Math.Max(100, Math.Min(600, Height));
				
				var w = Width - 10;
				var rw = w - 24;

				var lines = Output.Split('\r', '\n');

				_Buffer.Clear();

				foreach (string line in lines)
				{
					var l = line;

					while (UOFont.Font2.GetWidth(l) > rw - 5)
					{
						var len = l.Length / 2;

						_Buffer.Add(l.Substring(0, len));

						l = l.Substring(len);
					}

					_Buffer.Add(l);
				}


				base.Compile();
			}

			protected override void CompileLayout(SuperGumpLayout layout)
			{
				base.CompileLayout(layout);

				layout.Add("bg", () => AddBackground(0, 0, Width, Height, 2620));

				layout.Add(
					"output",
					() =>
					{
						var w = Width - 10;
						var h = Height - 10;

						var rw = w - 24;
						var rh = h / 20;

						var rc = h / rh;

						var x = 5;
						var y = 5;

						foreach (var line in _Buffer.Skip(_Index).Take(rc))
						{
							AddHtml(x, y, rw, rh, line, false, false);

							y += rh;
						}

						x += rw;
						y = 5;

						AddScrollbarV(
							x,
							y,
							_Buffer.Count,
							_Buffer.Count,
							b =>
							{
								--_Index;
								Refresh(true);
							},
							b =>
							{
								++_Index;
								Refresh(true);
							},
							new Rectangle2D(6, 42, 13, h - 56),
							new Rectangle2D(6, 10, 13, 28),
							new Rectangle2D(6, 42 + (h - 56), 13, 28),
							Tuple.Create(10740, 10742),
							Tuple.Create(10701, 10702, 10700),
							Tuple.Create(10721, 10722, 10720));
					});
			}

			protected override void OnDisposed()
			{
				_Buffer.Free(true);
				_Buffer = null;

				base.OnDisposed();
			}
		}
	}
}