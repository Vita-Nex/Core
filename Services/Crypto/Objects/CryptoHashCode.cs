#region Header
//   Vorspire    _,-'/-'/  CryptoHashCode.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
#endregion

namespace VitaNex.Crypto
{
	public class CryptoHashCode : IEquatable<CryptoHashCode>, IComparable<CryptoHashCode>, IEnumerable<char>, IDisposable
	{
		public bool IsDisposed { get; private set; }

		public bool IsExtended { get { return CryptoService.IsExtended(ProviderID); } }

		private int _ProviderID;

		public int ProviderID
		{
			get { return _ProviderID; }
			protected set
			{
				if (_ProviderID == value)
				{
					return;
				}

				_ProviderID = value;
				Invalidate();
			}
		}

		private string _Seed;

		public string Seed
		{
			get { return _Seed; }
			protected set
			{
				if (_Seed == value)
				{
					return;
				}

				_Seed = value;
				Invalidate();
			}
		}

		public virtual string Value { get; private set; }

		public int ValueHash { get { return GetValueHash(); } }

		public int Length { get { return Value.Length; } }

		public char this[int index] { get { return Value[index]; } }

		public CryptoHashCode(CryptoHashType type, string seed)
			: this((int)type, seed)
		{ }

		public CryptoHashCode(int providerID, string seed)
		{
			_ProviderID = providerID;
			_Seed = seed ?? String.Empty;

			Invalidate();
		}

		public CryptoHashCode(GenericReader reader)
		{
			Deserialize(reader);

			Invalidate();
		}

		~CryptoHashCode()
		{
			Dispose();
		}

		protected void Invalidate()
		{
			Value = CryptoGenerator.GenString(_ProviderID, _Seed);
		}

		public override string ToString()
		{
			return Value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<char> GetEnumerator()
		{
			return Value.GetEnumerator();
		}

		public virtual int CompareTo(CryptoHashCode code)
		{
			return !ReferenceEquals(code, null) ? Insensitive.Compare(Value, code.Value) : -1;
		}

		public override bool Equals(object obj)
		{
			return obj is CryptoHashCode && Equals((CryptoHashCode)obj);
		}

		public bool Equals(CryptoHashCode other)
		{
			return !ReferenceEquals(other, null) && Insensitive.Equals(Value, other.Value);
		}

		public override int GetHashCode()
		{
			return unchecked((ProviderID * 397) ^ GetValueHash());
		}

		public int GetValueHash()
		{
			var hash = Value.Aggregate(Value.Length, (h, c) => unchecked((h * 397) ^ (int)c));

			// It may be negative, so ensure it is positive, normally this wouldn't be the case but negative integers for 
			// almost unique id's should be positive for things like database keys.
			// Note this increases chance of unique collisions by 50%, though still extremely unlikely;
			// 1 : 2,147,483,647
			return Math.Abs(hash);
		}

		public virtual void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			IsDisposed = true;

			GC.SuppressFinalize(this);

			_Seed = null;
			Value = null;
		}

		public virtual void Serialize(GenericWriter writer)
		{
			var version = writer.SetVersion(1);

			writer.Write(_ProviderID);

			if (_Seed == null)
			{
				_Seed = String.Empty;
			}

			switch (version)
			{
				case 1:
				{
					// Compressing a string worth less than 256 bytes results in larger output
					if (Encoding.UTF32.GetByteCount(_Seed) > 256)
					{
						writer.Write(true);
						writer.WriteBytes(StringCompression.Pack(_Seed));
					}
					else
					{
						writer.Write(false);
						writer.WriteBytes(Encoding.UTF32.GetBytes(_Seed));
					}
				}
					break;
				case 0:
					writer.Write(_Seed);
					break;
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			var version = reader.GetVersion();

			_ProviderID = reader.ReadInt();

			switch (version)
			{
				case 1:
				{
					_Seed = reader.ReadBool()
						? StringCompression.Unpack(reader.ReadBytes())
						: Encoding.UTF32.GetString(reader.ReadBytes());
				}
					break;
				case 0:
					_Seed = reader.ReadString();
					break;
			}
		}

		public static bool operator ==(CryptoHashCode l, CryptoHashCode r)
		{
			return ReferenceEquals(l, null) ? ReferenceEquals(r, null) : l.Equals(r);
		}

		public static bool operator !=(CryptoHashCode l, CryptoHashCode r)
		{
			return ReferenceEquals(l, null) ? !ReferenceEquals(r, null) : !l.Equals(r);
		}
	}
}