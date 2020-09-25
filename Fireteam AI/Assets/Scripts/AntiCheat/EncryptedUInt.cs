namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct EncryptedUInt : IEncryptedType, IFormattable, IEquatable<EncryptedUInt>, IComparable<EncryptedUInt>, IComparable<uint>, IComparable
	{
		[SerializeField]
		private uint currentCryptoKey;

		[SerializeField]
		private uint hiddenValue;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private uint fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedUInt(uint value)
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

		public static uint Encrypt(uint value, uint key)
		{
			return value ^ key;
		}

		public static uint Decrypt(uint value, uint key)
		{
			return value ^ key;
		}

		public static EncryptedUInt FromEncrypted(uint encrypted, uint key)
		{
			EncryptedUInt instance = new EncryptedUInt();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static uint GenerateKey()
		{
			return BasicUtils.GenerateUIntKey();
		}

		public uint GetEncrypted(out uint key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(uint encrypted, uint key)
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

		public uint GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			uint decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private uint InternalDecrypt()
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

			uint decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations
		public static implicit operator EncryptedUInt(uint i)
		{
			return new EncryptedUInt(i);
		}

		public static implicit operator uint(EncryptedUInt i)
		{
			return i.InternalDecrypt();
		}

		public static explicit operator EncryptedInt(EncryptedUInt i)
		{
			return (int)i.InternalDecrypt();
		}

		public static EncryptedUInt operator ++(EncryptedUInt i)
		{
			return Increment(i, 1);
		}

		public static EncryptedUInt operator --(EncryptedUInt i)
		{
			return Increment(i, -1);
		}

		private static EncryptedUInt Increment(EncryptedUInt input, int increment)
		{
			uint decrypted = (uint)(input.InternalDecrypt() + increment);
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
			return obj is EncryptedUInt && Equals((EncryptedUInt)obj);
		}

		public bool Equals(EncryptedUInt obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(EncryptedUInt other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(uint other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is uint)) throw new ArgumentException("Argument must be uint");
			return CompareTo((uint)obj);
		}

		#endregion
	}
}