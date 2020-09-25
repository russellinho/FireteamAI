namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct EncryptedLong : IEncryptedType, IFormattable, IEquatable<EncryptedLong>, IComparable<EncryptedLong>, IComparable<long>, IComparable
	{
		[SerializeField]
		private long currentCryptoKey;

		[SerializeField]
		private long hiddenValue;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private long fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedLong(long value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : 0;
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public static long Encrypt(long value, long key)
		{
			return value ^ key;
		}

		public static long Decrypt(long value, long key)
		{
			return value ^ key;
		}

		public static EncryptedLong FromEncrypted(long encrypted, long key)
		{
			EncryptedLong instance = new EncryptedLong();
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

		public long GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			long decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private long InternalDecrypt()
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

			long decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedLong(long i)
		{
			return new EncryptedLong(i);
		}

		public static implicit operator long(EncryptedLong i)
		{
			return i.InternalDecrypt();
		}

		public static EncryptedLong operator ++(EncryptedLong i)
		{
			return Increment(i, 1);
		}

		public static EncryptedLong operator --(EncryptedLong i)
		{
			return Increment(i, -1);
		}

		private static EncryptedLong Increment(EncryptedLong input, int increment)
		{
			long decrypted = input.InternalDecrypt() + increment;
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
			return obj is EncryptedLong && Equals((EncryptedLong)obj);
		}

		public bool Equals(EncryptedLong obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(EncryptedLong other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(long other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is long)) throw new ArgumentException("Argument must be long");
			return CompareTo((long)obj);
		}

		#endregion
	}
}
