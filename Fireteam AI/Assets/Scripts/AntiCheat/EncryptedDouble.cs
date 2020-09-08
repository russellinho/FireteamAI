namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;
	using System.Runtime.InteropServices;
	using UnityEngine.Serialization;

	[Serializable]
	public struct EncryptedDouble : IEncryptedType, IFormattable, IEquatable<EncryptedDouble>, IComparable<EncryptedDouble>, IComparable<double>, IComparable
	{
		[SerializeField]
		private long currentCryptoKey;

		[SerializeField]
		private long hiddenValue;

		[SerializeField]
		[FormerlySerializedAs("hiddenValue")]
#pragma warning disable 414
		private Byte8 hiddenValueOldByte8;
#pragma warning restore 414

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private double fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedDouble(double value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);
			hiddenValueOldByte8 = default(Byte8);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : 0L;
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public static long Encrypt(double value, long key)
		{
			return DoubleLongBytesUnion.XorDoubleToLong(value, key);
		}

		public static double Decrypt(long value, long key)
		{
			return DoubleLongBytesUnion.XorLongToDouble(value, key);
		}

		public static long MigrateEncrypted(long encrypted, byte fromVersion = 0, byte toVersion = 2)
		{
			return DoubleLongBytesUnion.Migrate(encrypted, fromVersion, toVersion);
		}

		public static EncryptedDouble FromEncrypted(long encrypted, long key)
		{
			EncryptedDouble instance = new EncryptedDouble();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static long GenerateKey()
		{
			return BasicUtils.GenerateLongKey();
		}

		public long GetEncrypted(out long key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(long encrypted, long key)
		{
			initialized = true;
			hiddenValue = encrypted;
			currentCryptoKey = key;

			if (EncryptedCheatingDetector.ExistsAndIsRunning)
			{
				fakeValueActive = false;
				fakeValue = InternalDecrypt();
				fakeValueActive = true;
			}
			else
			{
				fakeValueActive = false;
			}
		}

		public double GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			double decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private double InternalDecrypt()
		{
			if (!initialized)
			{
				currentCryptoKey = GenerateKey();
				hiddenValue = Encrypt(0, currentCryptoKey);
				fakeValue = 0;
				fakeValueActive = false;
				initialized = true;

				return 0;
			}

			if (hiddenValueOldByte8.b1 != 0 ||
			    hiddenValueOldByte8.b2 != 0 ||
			    hiddenValueOldByte8.b3 != 0 ||
			    hiddenValueOldByte8.b4 != 0 ||
			    hiddenValueOldByte8.b5 != 0 ||
			    hiddenValueOldByte8.b6 != 0 ||
			    hiddenValueOldByte8.b7 != 0 ||
			    hiddenValueOldByte8.b8 != 0)
			{
				DoubleLongBytesUnion union = new DoubleLongBytesUnion { b8 = hiddenValueOldByte8 };
				union.b8.Shuffle();
				hiddenValue = union.l;

				hiddenValueOldByte8.b1 = 0;
				hiddenValueOldByte8.b2 = 0;
				hiddenValueOldByte8.b3 = 0;
				hiddenValueOldByte8.b4 = 0;
				hiddenValueOldByte8.b5 = 0;
				hiddenValueOldByte8.b6 = 0;
				hiddenValueOldByte8.b7 = 0;
				hiddenValueOldByte8.b8 = 0;
			}

			double decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue) > EncryptedCheatingDetector.Instance.doubleEpsilon)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		//! @cond
		public static implicit operator EncryptedDouble(double i)
		{
			return new EncryptedDouble(i);
		}

		public static implicit operator double(EncryptedDouble i)
		{
			return i.InternalDecrypt();
		}

		public static explicit operator EncryptedDouble(EncryptedFloat f)
		{
			return (float)f;
		}

		public static EncryptedDouble operator ++(EncryptedDouble i)
		{
			return Increment(i, 1);
		}

		public static EncryptedDouble operator --(EncryptedDouble i)
		{
			return Increment(i, -1);
		}

		private static EncryptedDouble Increment(EncryptedDouble input, double increment)
		{
			double decrypted = input.InternalDecrypt() + increment;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning)
			{
				input.fakeValue = decrypted;
				input.fakeValueActive = true;
			}
			else
			{
				input.fakeValueActive = false;
			}

			return input;
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}

		public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecrypt().ToString(format, provider);
		}

		public override bool Equals(object obj)
		{
			return obj is EncryptedDouble && Equals((EncryptedDouble)obj);
		}

		public bool Equals(EncryptedDouble obj)
		{
			return obj.InternalDecrypt().Equals(InternalDecrypt());
		}

		public int CompareTo(EncryptedDouble other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(double other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is double)) throw new ArgumentException("Argument must be double");
			return CompareTo((double)obj);
		}

		#endregion

		[StructLayout(LayoutKind.Explicit)]
		private struct DoubleLongBytesUnion
		{
			[FieldOffset(0)]
			internal double d;

			[FieldOffset(0)]
			internal long l;

			[FieldOffset(0)]
			internal Byte8 b8;

			internal static long Migrate(long value, byte fromVersion, byte toVersion)
			{
				DoubleLongBytesUnion u = FromLong(value);

				if (fromVersion < 2 && toVersion == 2)
				{
					u.b8.Shuffle();
				}

				return u.l;
			}

			internal static long XorDoubleToLong(double value, long key)
			{
				return FromDouble(value).Shuffle(key).l;
			}

			internal static double XorLongToDouble(long value, long key)
			{
				return FromLong(value).UnShuffle(key).d;
			}

			private static DoubleLongBytesUnion FromDouble(double value)
			{
				return new DoubleLongBytesUnion { d = value};
			}

			private static DoubleLongBytesUnion FromLong(long value)
			{
				return new DoubleLongBytesUnion { l = value};
			}

			private DoubleLongBytesUnion Shuffle(long key)
			{
				l ^= key;
				b8.Shuffle();

				return this;
			}

			private DoubleLongBytesUnion UnShuffle(long key)
			{
				b8.UnShuffle();
				l ^= key;

				return this;
			}
		}
	}
}