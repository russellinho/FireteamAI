namespace Koobando.AntiCheat
{
	using System;

 	[Serializable]
	public struct EncryptedByte : IEncryptedType, IFormattable, IEquatable<EncryptedByte>, IComparable<EncryptedByte>, IComparable<byte>, IComparable
	{
		private byte currentCryptoKey;
		private byte hiddenValue;
		private bool inited;

		private byte fakeValue;
		private bool fakeValueActive;

		private EncryptedByte(byte value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : (byte)0;
			fakeValueActive = detectorRunning;
#endif
			inited = true;
		}

		public static byte Encrypt(byte value, byte key)
		{
			return (byte)(value ^ key);
		}

		public static void Encrypt(byte[] value, byte key)
		{
			int len = value.Length;
			for (int i = 0; i < len; i++)
			{
				value[i] = Encrypt(value[i], key);
			}
		}

		public static byte Decrypt(byte value, byte key)
		{
			return (byte)(value ^ key);
		}

		public static void Decrypt(byte[] value, byte key)
		{
			int len = value.Length;
			for (int i = 0; i < len; i++)
			{
				value[i] = Decrypt(value[i], key);
			}
		}

		public static EncryptedByte FromEncrypted(byte encrypted, byte key)
		{
			EncryptedByte instance = new EncryptedByte();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static byte GenerateKey()
		{
			return BasicUtils.GenerateByteKey();
		}

		public byte GetEncrypted(out byte key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(byte encrypted, byte key)
		{
			inited = true;
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

		public byte GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			byte decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private byte InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = GenerateKey();
				hiddenValue = Encrypt(0, currentCryptoKey);
				fakeValue = 0;
				fakeValueActive = false;
				inited = true;

				return 0;
			}

			byte decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedByte(byte i)
		{
			return new EncryptedByte(i);
		}

		public static implicit operator byte(EncryptedByte i)
		{
			return i.InternalDecrypt();
		}

		public static EncryptedByte operator ++(EncryptedByte i)
		{
			return Increment(i, 1);
		}

		public static EncryptedByte operator --(EncryptedByte i)
		{
			return Increment(i, -1);
		}

		private static EncryptedByte Increment(EncryptedByte input, int increment)
		{
			byte decrypted = (byte)(input.InternalDecrypt() + increment);
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
			return obj is EncryptedByte && Equals((EncryptedByte)obj);
		}

		public bool Equals(EncryptedByte obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(EncryptedByte other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(byte other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is byte)) throw new ArgumentException("Argument must be byte");
			return CompareTo((byte)obj);
		}

		#endregion
	}
}
