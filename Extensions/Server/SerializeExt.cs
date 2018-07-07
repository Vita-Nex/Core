#region Header
//   Vorspire    _,-'/-'/  SerializeExt.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Server.Accounting;
using Server.Items;

using VitaNex;
using VitaNex.Crypto;
#endregion

namespace Server
{
	public static class SerializeExtUtility
	{
		private static readonly ObjectProperty _ReaderStream = new ObjectProperty("BaseStream");
		private static readonly ObjectProperty _WriterStream = new ObjectProperty("UnderlyingStream");

		private static readonly List<IAsyncResult> _Tasks = new List<IAsyncResult>();

		private static readonly object _TaskRoot = ((ICollection)_Tasks).SyncRoot;

		private static readonly AutoResetEvent _Sync = new AutoResetEvent(true);

		private static readonly Action<FileInfo, Action<GenericWriter>, bool> _Serialize = Serialize;

		public static int PendingWriters
		{
			get
			{
				lock (_TaskRoot)
				{
					return _Tasks.Count - _Tasks.RemoveAll(t => t.IsCompleted);
				}
			}
		}

		static SerializeExtUtility()
		{
			VitaNexCore.OnSaved += WaitForWriteCompletion;
			VitaNexCore.OnDispose += WaitForWriteCompletion;
			VitaNexCore.OnDisposed += WaitForWriteCompletion;
		}

		[CallPriority(Int32.MaxValue)]
		private static void OnSaved()
		{
			WaitForWriteCompletion();
		}

		[CallPriority(Int32.MinValue)]
		private static void OnDispose()
		{
			WaitForWriteCompletion();
		}

		[CallPriority(Int32.MaxValue)]
		private static void OnDisposed()
		{
			WaitForWriteCompletion();
		}

		/// <summary>
		///     Prevent the core from exiting during a crash state while async writers are pending.
		/// </summary>
		private static void WaitForWriteCompletion()
		{
			if (!VitaNexCore.Crashed && !Core.Closing)
			{
				return;
			}

			var pending = PendingWriters;

			if (pending <= 0)
			{
				return;
			}

			VitaNexCore.ToConsole("Waiting for {0:#,0} pending write tasks...", pending);

			while (pending > 0)
			{
				_Sync.WaitOne(10);

				pending = PendingWriters;
			}

			VitaNexCore.ToConsole("All write tasks completed.", pending);
		}

		#region Initializers
		public static BinaryFileWriter GetBinaryWriter(this Stream stream)
		{
			return new BinaryFileWriter(stream, true);
		}

		public static BinaryFileReader GetBinaryReader(this Stream stream)
		{
			return new BinaryFileReader(new BinaryReader(stream));
		}

		public static FileStream GetStream(
			this FileInfo file,
			FileAccess access = FileAccess.ReadWrite,
			FileShare share = FileShare.ReadWrite)
		{
			return file != null ? file.Open(FileMode.OpenOrCreate, access, share) : null;
		}

		public static void SerializeAsync(this FileInfo file, Action<GenericWriter> handler)
		{
			SerializeAsync(file, handler, true);
		}

		public static void SerializeAsync(this FileInfo file, Action<GenericWriter> handler, bool truncate)
		{
			if (file == null || handler == null)
			{
				return;
			}

			// Do not use async writing during a crash state or when closing.
			if (VitaNexCore.Crashed || Core.Closing)
			{
				_Serialize.Invoke(file, handler, truncate);
				return;
			}

			var t = _Serialize.BeginInvoke(file, handler, truncate, OnSerializeAsync, file);

			lock (_TaskRoot)
			{
				_Tasks.Add(t);
			}

			_Sync.Reset();

			var dir = file.Directory != null ? file.Directory.Name : String.Empty;

			VitaNexCore.ToConsole("Async write started for '{0}/{1}'", dir, file.Name);
		}

		private static void OnSerializeAsync(IAsyncResult r)
		{
			_Serialize.EndInvoke(r);

			lock (_TaskRoot)
			{
				_Tasks.Remove(r);
			}

			_Sync.Set();

			var file = (FileInfo)r.AsyncState;
			var dir = file.Directory != null ? file.Directory.Name : String.Empty;

			VitaNexCore.ToConsole("Async write ended for '{0}/{1}'", dir, file.Name);
		}

		public static void Serialize(this FileInfo file, Action<GenericWriter> handler)
		{
			Serialize(file, handler, true);
		}

		public static void Serialize(this FileInfo file, Action<GenericWriter> handler, bool truncate)
		{
			if (file == null || handler == null)
			{
				return;
			}

			file = file.EnsureFile(truncate);

			using (var stream = GetStream(file))
			{
				var writer = GetBinaryWriter(stream);

				VitaNexCore.TryCatch(handler, writer);

				writer.Close();
			}
		}

		public static void Deserialize(this FileInfo file, Action<GenericReader> handler)
		{
			if (file == null || !file.Exists || file.Length == 0 || handler == null)
			{
				return;
			}

			using (var stream = GetStream(file))
			{
				var reader = GetBinaryReader(stream);

				VitaNexCore.TryCatch(handler, reader);

				reader.Close();
			}
		}
		#endregion Initializers

		#region Operations
		public static int Skip(this GenericReader reader, int length)
		{
			var skipped = 0;

			while (--length >= 0)
			{
				if (reader.End())
				{
					break;
				}

				reader.ReadByte();
				++skipped;
			}

			return skipped;
		}

		public static long Seek(this GenericWriter writer, long offset, SeekOrigin origin)
		{
			if (writer != null)
			{
				if (writer is BinaryFileWriter)
				{
					var bin = (BinaryFileWriter)writer;

					return bin.UnderlyingStream.Seek(offset, origin);
				}

				if (writer is AsyncWriter)
				{
					var bin = (AsyncWriter)writer;

					return bin.MemStream.Seek(offset, origin);
				}

				var s = _WriterStream.GetValue(writer) as Stream;

				if (s != null && s.CanSeek)
				{
					return s.Seek(offset, origin);
				}
			}

			throw new InvalidOperationException("Can't perform seek operation");
		}

		public static long Seek(this GenericReader reader, long offset, SeekOrigin origin)
		{
			if (reader != null)
			{
				if (reader is BinaryFileReader)
				{
					var bin = (BinaryFileReader)reader;
					return bin.Seek(offset, origin);
				}

				var s = _ReaderStream.GetValue(reader) as Stream;

				if (s != null && s.CanSeek)
				{
					return s.Seek(offset, origin);
				}
			}

			throw new InvalidOperationException("Can't perform seek operation");
		}
		#endregion Operations

		#region Raw Data
		public static void Read(this GenericReader reader, byte[] buffer)
		{
			for (var i = 0; i < buffer.Length; i++)
			{
				buffer[i] = reader.ReadByte();
			}
		}

		public static void WriteBytes(this GenericWriter writer, byte[] buffer)
		{
			int length;
			WriteBytes(writer, buffer, 0, buffer.Length, out length);
		}

		public static void WriteBytes(this GenericWriter writer, byte[] buffer, int offset, int count, out int length)
		{
			var block = offset == 0 && count == buffer.Length ? buffer : buffer.Skip(offset).Take(count).ToArray();

			writer.Write(length = block.Length);

			for (var i = 0; i < length; i++)
			{
				writer.Write(block[i]);
			}
		}

		public static byte[] ReadBytes(this GenericReader reader, int length)
		{
			var buffer = new byte[length];

			Read(reader, buffer);

			return buffer;
		}

		public static byte[] ReadBytes(this GenericReader reader)
		{
			var length = reader.ReadInt();

			return ReadBytes(reader, length);
		}

		public static void WriteLongBytes(this GenericWriter writer, byte[] buffer)
		{
			long length;

			WriteLongBytes(writer, buffer, 0, buffer.Length, out length);
		}

		public static void WriteLongBytes(this GenericWriter writer, byte[] buffer, int offset, int count, out long length)
		{
			var block = offset == 0 && count == buffer.Length ? buffer : buffer.Skip(offset).Take(count).ToArray();

			writer.Write(length = block.Length);

			for (long i = 0; i < length; i++)
			{
				writer.Write(block[i]);
			}
		}

		public static byte[] ReadLongBytes(this GenericReader reader)
		{
			var length = reader.ReadLong();

			var block = new byte[length];

			for (long i = 0; i < length && !reader.End(); i++)
			{
				block[i] = reader.ReadByte();
			}

			return block;
		}
		#endregion Raw Data

		#region Block Data
		public static void WriteBlock<T>(this GenericWriter writer, Action<GenericWriter, T> onSerialize, T state)
		{
			using (var ms = new MemoryStream())
			{
				var bw = ms.GetBinaryWriter();

				VitaNexCore.TryCatch(w => onSerialize(w, state), bw);

				bw.Flush();

				ms.Seek(0, SeekOrigin.Begin);

				var length = ms.Length;
				var chunkSize = (int)Math.Min(0xFFFF, length);
				var chunk = new byte[chunkSize];

				writer.Write(length + 8);

				while (length > 0)
				{
					chunkSize = (int)Math.Min(chunkSize, length);

					ms.Read(chunk, 0, chunkSize);

					for (var i = 0; i < chunkSize; i++)
					{
						writer.Write(chunk[i]);
					}

					length -= chunkSize;
				}
			}
		}

		public static void ReadBlock<T>(this GenericReader reader, Action<GenericReader, T> onDeserialize, T state)
		{
			using (var ms = new MemoryStream())
			{
				var length = reader.ReadLong() - 8;
				var chunkSize = (int)Math.Min(0xFFFF, length);
				var chunk = new byte[chunkSize];

				while (length > 0)
				{
					chunkSize = (int)Math.Min(chunkSize, length);

					for (var i = 0; i < chunkSize; i++)
					{
						chunk[i] = reader.ReadByte();
					}

					ms.Write(chunk, 0, chunkSize);

					length -= chunkSize;
				}

				ms.Seek(0, SeekOrigin.Begin);

				VitaNexCore.TryCatch(r => onDeserialize(r, state), ms.GetBinaryReader());
			}
		}

		public static void WriteBlock(this GenericWriter writer, Action<GenericWriter> onSerialize)
		{
			using (var ms = new MemoryStream())
			{
				var bw = ms.GetBinaryWriter();

				VitaNexCore.TryCatch(onSerialize, bw);

				bw.Flush();

				ms.Seek(0, SeekOrigin.Begin);

				var length = ms.Length;
				var chunkSize = (int)Math.Min(0xFFFF, length);
				var chunk = new byte[chunkSize];

				writer.Write(length + 8);

				while (length > 0)
				{
					chunkSize = (int)Math.Min(chunkSize, length);

					ms.Read(chunk, 0, chunkSize);

					for (var i = 0; i < chunkSize; i++)
					{
						writer.Write(chunk[i]);
					}

					length -= chunkSize;
				}
			}
		}

		public static void ReadBlock(this GenericReader reader, Action<GenericReader> onDeserialize)
		{
			using (var ms = new MemoryStream())
			{
				var length = reader.ReadLong() - 8;
				var chunkSize = (int)Math.Min(0xFFFF, length);
				var chunk = new byte[chunkSize];

				while (length > 0)
				{
					chunkSize = (int)Math.Min(chunkSize, length);

					for (var i = 0; i < chunkSize; i++)
					{
						chunk[i] = reader.ReadByte();
					}

					ms.Write(chunk, 0, chunkSize);

					length -= chunkSize;
				}

				ms.Seek(0, SeekOrigin.Begin);

				VitaNexCore.TryCatch(onDeserialize, ms.GetBinaryReader());
			}
		}

		public static T ReadBlock<T>(this GenericReader reader, Func<GenericReader, T> onDeserialize)
		{
			using (var ms = new MemoryStream())
			{
				var length = reader.ReadLong() - 8;
				var chunkSize = (int)Math.Min(0xFFFF, length);
				var chunk = new byte[chunkSize];

				while (length > 0)
				{
					chunkSize = (int)Math.Min(chunkSize, length);

					for (var i = 0; i < chunkSize; i++)
					{
						chunk[i] = reader.ReadByte();
					}

					ms.Write(chunk, 0, chunkSize);

					length -= chunkSize;
				}

				ms.Seek(0, SeekOrigin.Begin);

				return VitaNexCore.TryCatchGet(onDeserialize, ms.GetBinaryReader());
			}
		}
		#endregion Block Data

		#region ICollection<T>
		public static void WriteBlockCollection<TObj>(
			this GenericWriter writer,
			ICollection<TObj> list,
			Action<GenericWriter, TObj> onSerialize)
		{
			list = list ?? new List<TObj>();

			writer.Write(list.Count);

			foreach (var obj in list)
			{
				WriteBlock(
					writer,
					w =>
					{
						if (obj == null)
						{
							w.Write(false);
						}
						else
						{
							w.Write(true);
							onSerialize(w, obj);
						}
					});
			}
		}

		public static IEnumerable<TObj> ReadBlockCollection<TObj>(
			this GenericReader reader,
			Func<GenericReader, TObj> onDeserialize)
		{
			var count = reader.ReadInt();

			for (var index = 0; index < count; index++)
			{
				yield return ReadBlock(
					reader,
					r =>
					{
						if (!r.ReadBool())
						{
							return default(TObj);
						}

						return onDeserialize(r);
					});
			}
		}

		public static void WriteCollection<TObj>(this GenericWriter writer, ICollection<TObj> list, Action<TObj> onSerialize)
		{
			WriteCollection(writer, list, (w, o) => onSerialize(o));
		}

		public static void WriteCollection<TObj>(
			this GenericWriter writer,
			ICollection<TObj> list,
			Action<GenericWriter, TObj> onSerialize)
		{
			list = list ?? new List<TObj>();

			writer.Write(list.Count);

			foreach (var obj in list)
			{
				if (obj == null)
				{
					writer.Write(false);
				}
				else
				{
					writer.Write(true);
					onSerialize(writer, obj);
				}
			}
		}

		public static IEnumerable<TObj> ReadCollection<TObj>(
			this GenericReader reader,
			Func<GenericReader, TObj> onDeserialize)
		{
			var count = reader.ReadInt();

			for (var index = 0; index < count; index++)
			{
				if (!reader.ReadBool())
				{
					yield return default(TObj);
				}

				yield return onDeserialize(reader);
			}
		}
		#endregion ICollection<T>

		#region T[]
		public static void WriteBlockArray<TObj>(
			this GenericWriter writer,
			TObj[] list,
			Action<GenericWriter, TObj> onSerialize)
		{
			list = list ?? new TObj[0];

			writer.Write(list.Length);

			foreach (var obj in list)
			{
				WriteBlock(
					writer,
					w =>
					{
						if (obj == null)
						{
							w.Write(false);
						}
						else
						{
							w.Write(true);
							onSerialize(w, obj);
						}
					});
			}
		}

		public static TObj[] ReadBlockArray<TObj>(
			this GenericReader reader,
			Func<GenericReader, TObj> onDeserialize,
			TObj[] list = null)
		{
			var count = reader.ReadInt();

			list = list ?? new TObj[count];

			if (list.Length < count)
			{
				list = new TObj[count];
			}

			for (var index = 0; index < count; index++)
			{
				ReadBlock(
					reader,
					r =>
					{
						if (!r.ReadBool())
						{
							if (index < list.Length)
							{
								list[index] = default(TObj);
							}

							return;
						}

						if (index < list.Length)
						{
							list[index] = onDeserialize(r);
						}
						else
						{
							onDeserialize(r);
						}
					});
			}

			return list;
		}

		public static void WriteArray<TObj>(this GenericWriter writer, TObj[] list, Action<TObj> onSerialize)
		{
			WriteArray(writer, list, (w, o) => onSerialize(o));
		}

		public static void WriteArray<TObj>(this GenericWriter writer, TObj[] list, Action<GenericWriter, TObj> onSerialize)
		{
			list = list ?? new TObj[0];

			writer.Write(list.Length);

			foreach (var obj in list)
			{
				if (obj == null)
				{
					writer.Write(false);
				}
				else
				{
					writer.Write(true);
					onSerialize(writer, obj);
				}
			}
		}

		public static TObj[] ReadArray<TObj>(this GenericReader reader, Func<TObj> onDeserialize, TObj[] list = null)
		{
			return ReadArray(reader, r => onDeserialize(), list);
		}

		public static TObj[] ReadArray<TObj>(
			this GenericReader reader,
			Func<GenericReader, TObj> onDeserialize,
			TObj[] list = null)
		{
			var count = reader.ReadInt();

			list = list ?? new TObj[count];

			if (list.Length < count)
			{
				list = new TObj[count];
			}

			for (var index = 0; index < count; index++)
			{
				if (!reader.ReadBool())
				{
					if (index < list.Length)
					{
						list[index] = default(TObj);
					}

					continue;
				}

				if (index < list.Length)
				{
					list[index] = onDeserialize(reader);
				}
				else
				{
					onDeserialize(reader);
				}
			}

			return list;
		}
		#endregion T[]

		#region List<T>
		public static void WriteBlockList<TObj>(
			this GenericWriter writer,
			List<TObj> list,
			Action<GenericWriter, TObj> onSerialize)
		{
			list = list ?? new List<TObj>();

			writer.Write(list.Count);

			foreach (var obj in list)
			{
				WriteBlock(
					writer,
					w =>
					{
						if (obj == null)
						{
							w.Write(false);
						}
						else
						{
							w.Write(true);
							onSerialize(w, obj);
						}
					});
			}
		}

		public static List<TObj> ReadBlockList<TObj>(
			this GenericReader reader,
			Func<GenericReader, TObj> onDeserialize,
			List<TObj> list = null)
		{
			var count = reader.ReadInt();

			list = list ?? new List<TObj>(count);

			if (list.Capacity < count)
			{
				list.Capacity = count;
			}

			for (var index = 0; index < count; index++)
			{
				ReadBlock(
					reader,
					r =>
					{
						if (!r.ReadBool())
						{
							return;
						}

						var obj = onDeserialize(r);

						if (obj != null)
						{
							list.Add(obj);
						}
					});
			}

			list.Free(false);

			return list;
		}

		public static void WriteList<TObj>(this GenericWriter writer, List<TObj> list, Action<TObj> onSerialize)
		{
			WriteList(writer, list, (w, o) => onSerialize(o));
		}

		public static void WriteList<TObj>(
			this GenericWriter writer,
			List<TObj> list,
			Action<GenericWriter, TObj> onSerialize)
		{
			list = list ?? new List<TObj>();

			writer.Write(list.Count);

			foreach (var obj in list)
			{
				if (obj == null)
				{
					writer.Write(false);
				}
				else
				{
					writer.Write(true);
					onSerialize(writer, obj);
				}
			}
		}

		public static List<TObj> ReadList<TObj>(this GenericReader reader, Func<TObj> onDeserialize, List<TObj> list = null)
		{
			return ReadList(reader, r => onDeserialize(), list);
		}

		public static List<TObj> ReadList<TObj>(
			this GenericReader reader,
			Func<GenericReader, TObj> onDeserialize,
			List<TObj> list = null)
		{
			var count = reader.ReadInt();

			list = list ?? new List<TObj>(count);

			if (list.Capacity < count)
			{
				list.Capacity = count;
			}

			for (var index = 0; index < count; index++)
			{
				if (!reader.ReadBool())
				{
					continue;
				}

				var obj = onDeserialize(reader);

				if (obj != null)
				{
					list.Add(obj);
				}
			}

			list.Free(false);

			return list;
		}
		#endregion List<T>

		#region Queue<T>
		public static void WriteBlockQueue<TObj>(
			this GenericWriter writer,
			Queue<TObj> queue,
			Action<GenericWriter, TObj> onSerialize)
		{
			queue = queue ?? new Queue<TObj>();

			writer.Write(queue.Count);

			foreach (var obj in queue)
			{
				WriteBlock(
					writer,
					w =>
					{
						if (obj == null)
						{
							w.Write(false);
						}
						else
						{
							w.Write(true);
							onSerialize(w, obj);
						}
					});
			}
		}

		public static Queue<TObj> ReadBlockQueue<TObj>(
			this GenericReader reader,
			Func<GenericReader, TObj> onDeserialize,
			Queue<TObj> queue = null)
		{
			var count = reader.ReadInt();

			queue = queue ?? new Queue<TObj>(count);

			for (var index = 0; index < count; index++)
			{
				ReadBlock(
					reader,
					r =>
					{
						if (!r.ReadBool())
						{
							return;
						}

						var obj = onDeserialize(r);

						if (obj != null)
						{
							queue.Enqueue(obj);
						}
					});
			}

			queue.Free(false);

			return queue;
		}

		public static void WriteQueue<TObj>(this GenericWriter writer, Queue<TObj> queue, Action<TObj> onSerialize)
		{
			WriteQueue(writer, queue, (w, o) => onSerialize(o));
		}

		public static void WriteQueue<TObj>(
			this GenericWriter writer,
			Queue<TObj> queue,
			Action<GenericWriter, TObj> onSerialize)
		{
			queue = queue ?? new Queue<TObj>();

			writer.Write(queue.Count);

			foreach (var obj in queue)
			{
				if (obj == null)
				{
					writer.Write(false);
				}
				else
				{
					writer.Write(true);
					onSerialize(writer, obj);
				}
			}
		}

		public static Queue<TObj> ReadQueue<TObj>(
			this GenericReader reader,
			Func<TObj> onDeserialize,
			Queue<TObj> queue = null)
		{
			return ReadQueue(reader, r => onDeserialize(), queue);
		}

		public static Queue<TObj> ReadQueue<TObj>(
			this GenericReader reader,
			Func<GenericReader, TObj> onDeserialize,
			Queue<TObj> queue = null)
		{
			var count = reader.ReadInt();

			queue = queue ?? new Queue<TObj>(count);

			for (var index = 0; index < count; index++)
			{
				if (!reader.ReadBool())
				{
					continue;
				}

				var obj = onDeserialize(reader);

				if (obj != null)
				{
					queue.Enqueue(obj);
				}
			}

			queue.Free(false);

			return queue;
		}
		#endregion Queue<T>

		#region Dictionary<TKey, TVal>
		public static void WriteBlockDictionary<TKey, TVal>(
			this GenericWriter writer,
			Dictionary<TKey, TVal> list,
			Action<GenericWriter, TKey, TVal> onSerialize)
		{
			list = list ?? new Dictionary<TKey, TVal>();

			writer.Write(list.Count);

			foreach (var kvp in list)
			{
				WriteBlock(
					writer,
					w =>
					{
						if (kvp.Key == null)
						{
							w.Write(false);
						}
						else
						{
							w.Write(true);
							onSerialize(w, kvp.Key, kvp.Value);
						}
					});
			}
		}

		public static Dictionary<TKey, TVal> ReadBlockDictionary<TKey, TVal>(
			this GenericReader reader,
			Func<GenericReader, KeyValuePair<TKey, TVal>> onDeserialize,
			Dictionary<TKey, TVal> list = null,
			bool replace = true)
		{
			var count = reader.ReadInt();

			list = list ?? new Dictionary<TKey, TVal>(count);

			for (var index = 0; index < count; index++)
			{
				ReadBlock(
					reader,
					r =>
					{
						if (!r.ReadBool())
						{
							return;
						}

						var kvp = onDeserialize(r);

						if (kvp.Key == null)
						{
							return;
						}

						if (!list.ContainsKey(kvp.Key))
						{
							list.Add(kvp.Key, kvp.Value);
						}
						else if (replace)
						{
							list[kvp.Key] = kvp.Value;
						}
					});
			}

			return list;
		}

		public static void WriteDictionary<TKey, TVal>(
			this GenericWriter writer,
			Dictionary<TKey, TVal> list,
			Action<TKey, TVal> onSerialize)
		{
			WriteDictionary(writer, list, (w, key, val) => onSerialize(key, val));
		}

		public static void WriteDictionary<TKey, TVal>(
			this GenericWriter writer,
			Dictionary<TKey, TVal> list,
			Action<GenericWriter, TKey, TVal> onSerialize)
		{
			list = list ?? new Dictionary<TKey, TVal>();

			writer.Write(list.Count);

			foreach (var kvp in list)
			{
				if (kvp.Key == null)
				{
					writer.Write(false);
				}
				else
				{
					writer.Write(true);
					onSerialize(writer, kvp.Key, kvp.Value);
				}
			}
		}

		public static Dictionary<TKey, TVal> ReadDictionary<TKey, TVal>(
			this GenericReader reader,
			Func<KeyValuePair<TKey, TVal>> onDeserialize,
			Dictionary<TKey, TVal> list = null,
			bool replace = true)
		{
			return ReadDictionary(reader, r => onDeserialize(), list, replace);
		}

		public static Dictionary<TKey, TVal> ReadDictionary<TKey, TVal>(
			this GenericReader reader,
			Func<GenericReader, KeyValuePair<TKey, TVal>> onDeserialize,
			Dictionary<TKey, TVal> list = null,
			bool replace = true)
		{
			var count = reader.ReadInt();

			list = list ?? new Dictionary<TKey, TVal>(count);

			for (var index = 0; index < count; index++)
			{
				if (!reader.ReadBool())
				{
					continue;
				}

				var kvp = onDeserialize(reader);

				if (kvp.Key == null)
				{
					continue;
				}

				if (!list.ContainsKey(kvp.Key))
				{
					list.Add(kvp.Key, kvp.Value);
				}
				else if (replace)
				{
					list[kvp.Key] = kvp.Value;
				}
			}

			return list;
		}
		#endregion Dictionary<TKey, TVal>

		#region BitArray
		public static void WriteBitArray(this GenericWriter writer, BitArray list)
		{
			list = list ?? new BitArray(0);

			writer.Write(list.Length);

			for (var index = 0; index < list.Length; index++)
			{
				writer.Write(list[index]);
			}
		}

		public static BitArray ReadBitArray(this GenericReader reader, BitArray list = null)
		{
			var length = reader.ReadInt();

			list = list ?? new BitArray(length);

			if (list.Length < length)
			{
				list.Length = length;
			}

			for (var index = 0; index < length; index++)
			{
				list[index] = reader.ReadBool();
			}

			return list;
		}
		#endregion BitArray

		#region Custom Types
		public static void Write(this GenericWriter writer, TimeStamp ts)
		{
			ts.Serialize(writer);
		}

		public static TimeStamp ReadTimeStamp(this GenericReader reader)
		{
			return new TimeStamp(reader);
		}

		public static void WriteBlock3D(this GenericWriter writer, Block3D b)
		{
			writer.Write(b.X);
			writer.Write(b.Y);
			writer.Write(b.Z);
			writer.Write(b.H);
		}

		public static Block3D ReadBlock3D(this GenericReader reader)
		{
			var x = reader.ReadInt();
			var y = reader.ReadInt();
			var z = reader.ReadInt();
			var h = reader.ReadInt();

			return new Block3D(x, y, z, h);
		}

		public static void WriteCoords(this GenericWriter writer, Coords c)
		{
			if (c != null)
			{
				writer.Write((Map)c);
				writer.Write((Point2D)c);
			}
			else
			{
				writer.Write(Map.Internal);
				writer.Write(Point2D.Zero);
			}
		}

		public static Coords ReadCoords(this GenericReader reader)
		{
			var map = reader.ReadMap();
			var x = reader.ReadInt();
			var y = reader.ReadInt();

			return new Coords(map, x, y);
		}

		public static void WriteLocation(this GenericWriter writer, MapPoint mp)
		{
			if (mp != null)
			{
				writer.Write((Map)mp);
				writer.Write((Point3D)mp);
			}
			else
			{
				writer.Write(Map.Internal);
				writer.Write(Point3D.Zero);
			}
		}

		public static MapPoint ReadLocation(this GenericReader reader)
		{
			var map = reader.ReadMap();
			var p = reader.ReadPoint3D();

			return new MapPoint(map, p);
		}

		public static void WriteEntity(this GenericWriter writer, IEntity e)
		{
			if (e != null)
			{
				writer.Write(e.Serial);
			}
			else
			{
				writer.Write(-1);
			}
		}

		public static IEntity ReadEntity(this GenericReader reader)
		{
			Serial s = reader.ReadInt();

			if (!s.IsValid)
			{
				return null;
			}

			IEntity e = null;

			if (s.IsItem)
			{
				e = World.FindItem(s);
			}
			else if (s.IsMobile)
			{
				e = World.FindMobile(s);
			}

			return e;
		}

		public static TEntity ReadEntity<TEntity>(this GenericReader reader)
			where TEntity : IEntity
		{
			var e = ReadEntity(reader);

			// ReSharper disable once MergeConditionalExpression
			return e is TEntity ? (TEntity)e : default(TEntity);
		}

		public static void WriteTextDef(this GenericWriter writer, TextDefinition def)
		{
			if (def == null)
			{
				writer.WriteEncodedInt(0);
				return;
			}

			if (def.Number > 0)
			{
				writer.WriteEncodedInt(1);
				writer.WriteEncodedInt(def.Number);
			}
			else if (def.String != null)
			{
				writer.WriteEncodedInt(2);
				writer.Write(def.String);
			}
			else
			{
				writer.WriteEncodedInt(0);
			}
		}

		public static TextDefinition ReadTextDef(this GenericReader reader)
		{
			TextDefinition def;

			switch (reader.ReadEncodedInt())
			{
				case 1:
					def = reader.ReadEncodedInt();
					break;
				case 2:
					def = reader.ReadString();
					break;
				default:
					def = new TextDefinition();
					break;
			}

			return def;
		}
		#endregion

		#region Type
		public static void Write(this GenericWriter writer, Type type)
		{
			WriteType(writer, type, (Action<Type>)null);
		}

		public static void WriteType(this GenericWriter writer, object obj, Action<Type> onSerialize, bool full = true)
		{
			WriteType(
				writer,
				obj,
				(w, t) =>
				{
					if (onSerialize != null)
					{
						onSerialize(t);
					}
				},
				full);
		}

		public static void WriteType(
			this GenericWriter writer,
			object obj,
			Action<GenericWriter, Type> onSerialize = null,
			bool full = true)
		{
			Type type = null;

			if (obj != null)
			{
				if (obj is Type)
				{
					type = (Type)obj;
				}
				else if (obj is ITypeSelectProperty)
				{
					type = ((ITypeSelectProperty)obj).InternalType;
				}
				else
				{
					type = obj.GetType();
				}
			}

			if (type == null)
			{
				writer.Write(false);
			}
			else
			{
				writer.Write(true);
				writer.Write(full);
				writer.Write(full ? type.FullName : type.Name);
			}

			if (onSerialize != null)
			{
				onSerialize(writer, type);
			}
		}

		public static Type ReadType(this GenericReader reader)
		{
			if (!reader.ReadBool())
			{
				return null;
			}

			var full = reader.ReadBool();
			var name = reader.ReadString();

			if (String.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			var type = Type.GetType(name, false) ??
					   (full ? ScriptCompiler.FindTypeByFullName(name) : ScriptCompiler.FindTypeByName(name));

			return type;
		}

		public static object ReadTypeCreate(this GenericReader reader, params object[] args)
		{
			return ReadTypeCreate<object>(reader, args);
		}

		public static TObj ReadTypeCreate<TObj>(this GenericReader reader, params object[] args)
			where TObj : class
		{
			TObj obj = null;

			VitaNexCore.TryCatch(
				() =>
				{
					var t = ReadType(reader);

					if (t != null)
					{
						obj = t.CreateInstanceSafe<TObj>(args);
					}
				},
				VitaNexCore.ToConsole);

			return obj;
		}
		#endregion Type

		#region Enums
		public static void WriteFlag<TEnum>(this GenericWriter writer, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			WriteFlag(writer, (Enum)Enum.ToObject(typeof(TEnum), flag));
		}

		public static void WriteFlag(this GenericWriter writer, Enum flag)
		{
			var ut = Enum.GetUnderlyingType(flag.GetType());

			if (ut == typeof(byte))
			{
				writer.Write((byte)0x01);
				writer.Write(Convert.ToByte(flag));
			}
			else if (ut == typeof(short))
			{
				writer.Write((byte)0x02);
				writer.Write(Convert.ToInt16(flag));
			}
			else if (ut == typeof(ushort))
			{
				writer.Write((byte)0x03);
				writer.Write(Convert.ToUInt16(flag));
			}
			else if (ut == typeof(int))
			{
				writer.Write((byte)0x04);
				writer.Write(Convert.ToInt32(flag));
			}
			else if (ut == typeof(uint))
			{
				writer.Write((byte)0x05);
				writer.Write(Convert.ToUInt32(flag));
			}
			else if (ut == typeof(long))
			{
				writer.Write((byte)0x06);
				writer.Write(Convert.ToInt64(flag));
			}
			else if (ut == typeof(ulong))
			{
				writer.Write((byte)0x07);
				writer.Write(Convert.ToUInt64(flag));
			}
			else
			{
				writer.Write((byte)0x00);
			}
		}

		public static TEnum ReadFlag<TEnum>(this GenericReader reader)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var type = typeof(TEnum);
			var flag = default(TEnum);

			if (!type.IsEnum)
			{
				return flag;
			}

			switch (reader.ReadByte())
			{
				case 0x01:
					flag = ToEnum<TEnum>(reader.ReadByte());
					break;
				case 0x02:
					flag = ToEnum<TEnum>(reader.ReadShort());
					break;
				case 0x03:
					flag = ToEnum<TEnum>(reader.ReadUShort());
					break;
				case 0x04:
					flag = ToEnum<TEnum>(reader.ReadInt());
					break;
				case 0x05:
					flag = ToEnum<TEnum>(reader.ReadUInt());
					break;
				case 0x06:
					flag = ToEnum<TEnum>(reader.ReadLong());
					break;
				case 0x07:
					flag = ToEnum<TEnum>(reader.ReadULong());
					break;
			}

			return flag;
		}

		private static TEnum ToEnum<TEnum>(object val)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var type = typeof(TEnum);
			var flag = default(TEnum);

			if (!type.IsEnum)
			{
				return flag;
			}

			if (!Enum.TryParse(val.ToString(), out flag) ||
				(!type.HasCustomAttribute<FlagsAttribute>(true) && !Enum.IsDefined(type, flag)))
			{
				flag = default(TEnum);
			}

			return flag;
		}
		#endregion Enums

		#region Simple Types
		public static void WriteSimpleType(this GenericWriter writer, object obj)
		{
			SimpleType.FromObject(obj).Serialize(writer);
		}

		public static SimpleType ReadSimpleType(this GenericReader reader)
		{
			return new SimpleType(reader);
		}

		public static TObj ReadSimpleType<TObj>(this GenericReader reader)
		{
			var value = new SimpleType(reader).Value;

			// ReSharper disable once MergeConditionalExpression
			return value is TObj ? (TObj)value : default(TObj);
		}
		#endregion Simple types

		#region Accounts
		public static void Write(this GenericWriter writer, IAccount a)
		{
			writer.Write(a == null ? String.Empty : a.Username);
		}

		public static TAcc ReadAccount<TAcc>(this GenericReader reader, bool defaultToOwner = false)
			where TAcc : IAccount
		{
			return (TAcc)ReadAccount(reader, defaultToOwner);
		}

		public static IAccount ReadAccount(this GenericReader reader, bool defaultToOwner = false)
		{
			var username = reader.ReadString();
			var a = Accounts.GetAccount(username ?? String.Empty);

			if (a == null && defaultToOwner)
			{
				a = Accounts.GetAccounts().FirstOrDefault(ac => ac.AccessLevel == AccessLevel.Owner);
			}

			return a;
		}
		#endregion Accounts

		#region Versioning
		public static int SetVersion(this GenericWriter writer, int version)
		{
			writer.Write(version);
			return version;
		}

		public static int GetVersion(this GenericReader reader)
		{
			return reader.ReadInt();
		}
		#endregion Versioning

		#region Crypto
		public static void Write(this GenericWriter writer, CryptoHashCode hash)
		{
			WriteType(
				writer,
				hash,
				t =>
				{
					if (t != null)
					{
						hash.Serialize(writer);
					}
				});
		}

		public static THashCode ReadHashCode<THashCode>(this GenericReader reader)
			where THashCode : CryptoHashCode
		{
			return ReadTypeCreate<THashCode>(reader, reader);
		}

		public static CryptoHashCode ReadHashCode(this GenericReader reader)
		{
			return ReadTypeCreate<CryptoHashCode>(reader, reader);
		}
		#endregion Crypto

		#region Misc
		public static void Write(this GenericWriter writer, WeaponAbility a)
		{
			writer.Write(WeaponAbility.Abilities.IndexOf(a));
		}

		public static WeaponAbility ReadAbility(this GenericReader reader)
		{
			var i = reader.ReadInt();

			if (WeaponAbility.Abilities.InBounds(i))
			{
				return WeaponAbility.Abilities[i];
			}

			return null;
		}
		#endregion Misc
	}
}