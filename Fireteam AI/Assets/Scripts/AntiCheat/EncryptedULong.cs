namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct EncryptedULong : IEncryptedType, IFormattable, IEquatable<EncryptedULong>, IComparable<EncryptedULong>, IComparable<ulong>, IComparable
	{
		[SerializeField]
		private ulong currentCryptoKey;

		[SerializeField]
		private ulong hiddenValue;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private ulong fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedULong(ulong value)
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

		public static ulong Encrypt(ulong value, ulong key)
		{
			return value ^ key;
		}

		public static ulong Decrypt(ulong value, ulong key)
		{
			return value ^ key;
		}

		public static EncryptedULong FromEncrypted(ulong encrypted, ulong key)
		{
			EncryptedULong instance = new EncryptedULong();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static ulong GenerateKey()
		{
			return BasicUtils.GenerateULongKey();
		}

		public ulong GetEncrypted(out ulong key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(ulong encrypted, ulong key)
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

		public ulong GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			ulong decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private ulong InternalDecrypt()
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

			ulong decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedULong(ulong i)
		{
			return new EncryptedULong(i);
		}

		public static implicit operator ulong(EncryptedULong i)
		{
			return i.InternalDecrypt();
		}

		public static EncryptedULong operator ++(EncryptedULong i)
		{
			return Increment(i, 1);
		}

		public static EncryptedULong operator --(EncryptedULong i)
		{
			return Increment(i, -1);
		}

		private static EncryptedULong Increment(EncryptedULong input, int increment)
		{
			ulong decrypted = increment == 1 ? input.InternalDecrypt() + 1 : input.InternalDecrypt() - 1;
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
			return obj is EncryptedULong && Equals((EncryptedULong)obj);
		}

		public bool Equals(EncryptedULong obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(EncryptedULong other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(ulong other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is ulong)) throw new ArgumentException("Argument must be ulong");
			return CompareTo((ulong)obj);
		}

		#endregion
	}
}
