namespace Koobando.AntiCheat
{
	using System;

	[Serializable]
	public struct EncryptedUShort : IEncryptedType, IFormattable, IEquatable<EncryptedUShort>, IComparable<EncryptedUShort>, IComparable<ushort>, IComparable
	{
		private ushort currentCryptoKey;
		private ushort hiddenValue;
		private bool initialized;

		private ushort fakeValue;
		private bool fakeValueActive;

		private EncryptedUShort(ushort value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : (ushort)0;
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public static ushort Encrypt(ushort value, ushort key)
		{
			return (ushort)(value ^ key);
		}

		public static ushort Decrypt(ushort value, ushort key)
		{
			return (ushort)(value ^ key);
		}

		public static EncryptedUShort FromEncrypted(ushort encrypted, ushort key)
		{
			EncryptedUShort instance = new EncryptedUShort();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static ushort GenerateKey()
		{
			return BasicUtils.GenerateUShortKey();
		}

		public ushort GetEncrypted(out ushort key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(ushort encrypted, ushort key)
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

		public ushort GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			ushort decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private ushort InternalDecrypt()
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

			ushort decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedUShort(ushort i)
		{
			return new EncryptedUShort(i);
		}

		public static implicit operator ushort(EncryptedUShort i)
		{
			return i.InternalDecrypt();
		}

		public static EncryptedUShort operator ++(EncryptedUShort i)
		{
			return Increment(i, 1);
		}

		public static EncryptedUShort operator --(EncryptedUShort i)
		{
			return Increment(i, -1);
		}

		private static EncryptedUShort Increment(EncryptedUShort input, int increment)
		{
			ushort decrypted = (ushort)(input.InternalDecrypt() + increment);
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
			return obj is EncryptedUShort && Equals((EncryptedUShort)obj);
		}

		public bool Equals(EncryptedUShort obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(EncryptedUShort other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(ushort other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is ushort)) throw new ArgumentException("Argument must be ushort");
			return CompareTo((ushort)obj);
		}

		#endregion
	}
}
