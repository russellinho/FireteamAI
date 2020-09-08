namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;

	[Serializable]
	public struct EncryptedBool : IEncryptedType, IEquatable<EncryptedBool>, IComparable<EncryptedBool>, IComparable<bool>, IComparable
	{
		[SerializeField]
		private byte currentCryptoKey;

		[SerializeField]
		private int hiddenValue;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private bool fakeValue;

		[SerializeField]
		[FormerlySerializedAs("fakeValueChanged")]
		private bool fakeValueActive;

		private EncryptedBool(bool value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : false;
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public static int Encrypt(bool value, byte key)
		{
			return (value ? 213 : 181) ^ key;
		}

		public static bool Decrypt(int value, byte key)
		{
			return (value ^ key) != 181;
		}

		public static EncryptedBool FromEncrypted(int encrypted, byte key)
		{
			var instance = new EncryptedBool();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static byte GenerateKey()
		{
			return BasicUtils.GenerateByteKey();
		}

		public int GetEncrypted(out byte key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(int encrypted, byte key)
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

		public bool GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private bool InternalDecrypt()
		{
			if (!initialized)
			{
				currentCryptoKey = GenerateKey();
				hiddenValue = Encrypt(false, currentCryptoKey);
				fakeValue = false;
				fakeValueActive = false;
				initialized = true;

				return false;
			}

			var decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedBool(bool value)
		{
			return new EncryptedBool(value);
		}

		public static implicit operator bool(EncryptedBool value)
		{
			return value.InternalDecrypt();
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		public override bool Equals(object obj)
		{
			return obj is EncryptedBool && Equals((EncryptedBool)obj);
		}

		public bool Equals(EncryptedBool obj)
		{
			if (currentCryptoKey == obj.currentCryptoKey)
			{
				return hiddenValue.Equals(obj.hiddenValue);
			}

			return Decrypt(hiddenValue, currentCryptoKey).Equals(Decrypt(obj.hiddenValue, obj.currentCryptoKey));
		}

		public int CompareTo(EncryptedBool other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(bool other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{

			if (obj == null) return 1;
			if (!(obj is bool)) throw new ArgumentException("Argument must be boolean");
			return CompareTo((bool)obj);
		}

		#endregion
	}
}