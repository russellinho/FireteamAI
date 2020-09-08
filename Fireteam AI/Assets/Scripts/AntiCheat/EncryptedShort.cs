namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct EncryptedShort : IEncryptedType, IFormattable, IEquatable<EncryptedShort>, IComparable<EncryptedShort>, IComparable<short>, IComparable
	{
		[SerializeField]
		private short currentCryptoKey;

		[SerializeField]
		private short hiddenValue;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private short fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedShort(short value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : (short)0;
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public static short Encrypt(short value, short key)
		{
			return (short)(value ^ key);
		}

		public static short Decrypt(short value, short key)
		{
			return (short)(value ^ key);
		}

		public static EncryptedShort FromEncrypted(short encrypted, short key)
		{
			EncryptedShort instance = new EncryptedShort();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static short GenerateKey()
		{
			return BasicUtils.GenerateShortKey();
		}

		public short GetEncrypted(out short key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(short encrypted, short key)
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

		public short GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			short decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private short InternalDecrypt()
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

			short decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedShort(short i)
		{
			return new EncryptedShort(i);
		}

		public static implicit operator short(EncryptedShort i)
		{
			return i.InternalDecrypt();
		}

		public static EncryptedShort operator ++(EncryptedShort i)
		{
			return Increment(i, 1);
		}

		public static EncryptedShort operator --(EncryptedShort i)
		{
			return Increment(i, -1);
		}

		private static EncryptedShort Increment(EncryptedShort input, int increment)
		{
			short decrypted = (short)(input.InternalDecrypt() + increment);
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
			return obj is EncryptedShort && Equals((EncryptedShort)obj);
		}

		public bool Equals(EncryptedShort obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(EncryptedShort other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(short other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is short)) throw new ArgumentException("Argument must be short");
			return CompareTo((short)obj);
		}

		#endregion
	}
}
