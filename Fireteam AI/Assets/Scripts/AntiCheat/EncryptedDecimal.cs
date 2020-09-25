namespace Koobando.AntiCheat
{
	using System;
	using System.Runtime.InteropServices;
	using UnityEngine;

	[Serializable]
	public struct EncryptedDecimal : IEncryptedType, IFormattable, IEquatable<EncryptedDecimal>, IComparable<EncryptedDecimal>, IComparable<decimal>, IComparable
	{
		[SerializeField]
		private long currentCryptoKey;

		[SerializeField]
		private Byte16 hiddenValue;

		[SerializeField]
		private bool initialized;

		private decimal fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedDecimal(decimal value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = InternalEncrypt(value, currentCryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : 0m;
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public static decimal Encrypt(decimal value, long key)
		{
			return DecimalLongBytesUnion.XorDecimalToDecimal(value, key);
		}

		public static decimal Decrypt(decimal value, long key)
		{
			return DecimalLongBytesUnion.XorDecimalToDecimal(value, key);
		}

		public static EncryptedDecimal FromEncrypted(decimal encrypted, long key)
		{
			EncryptedDecimal instance = new EncryptedDecimal();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static long GenerateKey()
		{
			return BasicUtils.GenerateLongKey();
		}

		public decimal GetEncrypted(out long key)
		{
			key = currentCryptoKey;
			return DecimalLongBytesUnion.ConvertB16ToDecimal(hiddenValue);
		}

		public void SetEncrypted(decimal encrypted, long key)
		{
			initialized = true;
			hiddenValue = DecimalLongBytesUnion.ConvertDecimalToB16(encrypted);
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

		public decimal GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			decimal decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = InternalEncrypt(decrypted, currentCryptoKey);
		}

		private static Byte16 InternalEncrypt(decimal value, long key)
		{
			return DecimalLongBytesUnion.XorDecimalToB16(value, key);
		}

		private decimal InternalDecrypt()
		{
			if (!initialized)
			{
				currentCryptoKey = GenerateKey();
				hiddenValue = InternalEncrypt(0m, currentCryptoKey);
				fakeValue = 0m;
				fakeValueActive = false;
				initialized = true;

				return 0m;
			}

			decimal decrypted = DecimalLongBytesUnion.XorB16ToDecimal(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedDecimal(decimal i)
		{
			return new EncryptedDecimal(i);
		}

		public static implicit operator decimal(EncryptedDecimal i)
		{
			return i.InternalDecrypt();
		}

		public static explicit operator EncryptedDecimal(EncryptedFloat f)
		{
			return (decimal)(float)f;
		}

		public static EncryptedDecimal operator ++(EncryptedDecimal i)
		{
			return Increment(i, 1);
		}

		public static EncryptedDecimal operator --(EncryptedDecimal i)
		{
			return Increment(i, -1);
		}

		private static EncryptedDecimal Increment(EncryptedDecimal input, decimal increment)
		{
			decimal decrypted = input.InternalDecrypt() + increment;
			input.hiddenValue = InternalEncrypt(decrypted, input.currentCryptoKey);

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
			return obj is EncryptedDecimal && Equals((EncryptedDecimal)obj);
		}

		public bool Equals(EncryptedDecimal obj)
		{
			return obj.InternalDecrypt().Equals(InternalDecrypt());
		}

		public int CompareTo(EncryptedDecimal other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(decimal other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is decimal)) throw new ArgumentException("Argument must be decimal");
			return CompareTo((decimal)obj);
		}

		#endregion

		[StructLayout(LayoutKind.Explicit)]
		private struct DecimalLongBytesUnion
		{
			[FieldOffset(0)]
			private decimal d;

			[FieldOffset(0)]
			private long l1;

			[FieldOffset(8)]
			private long l2;

			[FieldOffset(0)]
			private Byte16 b16;

			internal static decimal XorDecimalToDecimal(decimal value, long key)
			{
				return FromDecimal(value).XorLongs(key).d;
			}

			internal static Byte16 XorDecimalToB16(decimal value, long key)
			{
				return FromDecimal(value).XorLongs(key).b16;
			}

			internal static decimal XorB16ToDecimal(Byte16 value, long key)
			{
				return FromB16(value).XorLongs(key).d;
			}

			internal static decimal ConvertB16ToDecimal(Byte16 value)
			{
				return FromB16(value).d;
			}

			internal static Byte16 ConvertDecimalToB16(decimal value)
			{
				return FromDecimal(value).b16;
			}

			private static DecimalLongBytesUnion FromDecimal(decimal value)
			{
				return new DecimalLongBytesUnion {d = value};
			}

			private static DecimalLongBytesUnion FromB16(Byte16 value)
			{
				return new DecimalLongBytesUnion {b16 = value};
			}

			private DecimalLongBytesUnion XorLongs(long key)
			{
				l1 ^= key;
				l2 ^= key;
				return this;
			}
		}
	}
}