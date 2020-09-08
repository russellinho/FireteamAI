namespace Koobando.AntiCheat
{
	using System;

	[Serializable]
	public struct EncryptedChar : IEncryptedType, IEquatable<EncryptedChar>, IComparable<EncryptedChar>, IComparable<char>, IComparable
	{
		private char currentCryptoKey;
		private char hiddenValue;
		private bool initialized;

		private char fakeValue;
		private bool fakeValueActive;

		private EncryptedChar(char value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : '\0';
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public static char Encrypt(char value, char key)
		{
			return (char)(value ^ key);
		}

		public static char Decrypt(char value, char key)
		{
			return (char)(value ^ key);
		}

		public static EncryptedChar FromEncrypted(char encrypted, char key)
		{
			EncryptedChar instance = new EncryptedChar();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static char GenerateKey()
		{
			return BasicUtils.GenerateCharKey();
		}

		public char GetEncrypted(out char key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(char encrypted, char key)
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

		public char GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			char decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private char InternalDecrypt()
		{
			if (!initialized)
			{
				currentCryptoKey = GenerateKey();
				hiddenValue = Encrypt('\0', currentCryptoKey);
				fakeValue = '\0';
				fakeValueActive = false;
				initialized = true;

				return '\0';
			}

			char decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedChar(char i)
		{
			return new EncryptedChar(i);
		}

		public static implicit operator char(EncryptedChar i)
		{
			return i.InternalDecrypt();
		}

		public static EncryptedChar operator ++(EncryptedChar i)
		{
			return Increment(i, 1);
		}

		public static EncryptedChar operator --(EncryptedChar i)
		{
			return Increment(i, -1);
		}

		private static EncryptedChar Increment(EncryptedChar input, int increment)
		{
			char decrypted = (char)(input.InternalDecrypt() + increment);
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

#if !UNITY_WINRT
		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}
#endif

		public override bool Equals(object obj)
		{
			return obj is EncryptedChar && Equals((EncryptedChar)obj);
		}

		public bool Equals(EncryptedChar obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(EncryptedChar other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(char other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is char)) throw new ArgumentException("Argument must be char");
			return CompareTo((char)obj);
		}

		#endregion
	}
}
