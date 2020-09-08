namespace Koobando.AntiCheat
{
	using System;

	[Serializable]
	public struct EncryptedSByte : IEncryptedType, IFormattable, IEquatable<EncryptedSByte>, IComparable<EncryptedSByte>, IComparable<sbyte>, IComparable
	{
		private sbyte currentCryptoKey;
		private sbyte hiddenValue;
		private bool initialized;

		private sbyte fakeValue;
		private bool fakeValueActive;

		private EncryptedSByte(sbyte value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : (sbyte)0;
			fakeValueActive = detectorRunning;
#endif

			initialized = true;
		}

		public static sbyte Encrypt(sbyte value, sbyte key)
		{
			return (sbyte)(value ^ key);
		}

		public static sbyte Decrypt(sbyte value, sbyte key)
		{
			return (sbyte)(value ^ key);
		}

		public static EncryptedSByte FromEncrypted(sbyte encrypted, sbyte key)
		{
			EncryptedSByte instance = new EncryptedSByte();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static sbyte GenerateKey()
		{
			return BasicUtils.GenerateSByteKey();
		}

		public sbyte GetEncrypted(out sbyte key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(sbyte encrypted, sbyte key)
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

		public sbyte GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			sbyte decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private sbyte InternalDecrypt()
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

			sbyte decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedSByte(sbyte i)
		{
			return new EncryptedSByte(i);
		}

		public static implicit operator sbyte(EncryptedSByte i)
		{
			return i.InternalDecrypt();
		}

		public static EncryptedSByte operator ++(EncryptedSByte i)
		{
			return Increment(i, 1);
		}

		public static EncryptedSByte operator --(EncryptedSByte i)
		{
			return Increment(i, -1);
		}

		private static EncryptedSByte Increment(EncryptedSByte input, int increment)
		{
			sbyte decrypted = (sbyte)(input.InternalDecrypt() + increment);
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
			return obj is EncryptedSByte && Equals((EncryptedSByte)obj);
		}

		public bool Equals(EncryptedSByte obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(EncryptedSByte other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(sbyte other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is sbyte)) throw new ArgumentException("Argument must be sbyte");
			return CompareTo((sbyte)obj);
		}

		#endregion
	}
}
