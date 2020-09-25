namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;

	[Serializable]
	public sealed class EncryptedString : IEncryptedType, IComparable<EncryptedString>, IComparable<string>, IComparable
	{

		[SerializeField]
		private char[] cryptoKey;

		[SerializeField]
		private char[] hiddenChars;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private string fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		// for serialization purposes
		private EncryptedString(){}

		private EncryptedString(string value)
		{
			cryptoKey = new char[7];
			GenerateKey(cryptoKey);
			hiddenChars = InternalEncryptDecrypt(value.ToCharArray(), cryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : null;
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public static char[] Encrypt(string value, string key)
		{
			return Encrypt(value, key.ToCharArray());
		}

		public static char[] Encrypt(string value, char[] key)
		{
			return Encrypt(value.ToCharArray(), key);
		}

		public static char[] Encrypt(char[] value, char[] key)
		{
			return InternalEncryptDecrypt(value, key);
		}

		public static string Decrypt(char[] value, string key)
		{
			return Decrypt(value, key.ToCharArray());
		}

		public static string Decrypt(char[] value, char[] key)
		{
			return new string(InternalEncryptDecrypt(value, key));
		}

		public static EncryptedString FromEncrypted(char[] encrypted, char[] key)
		{
			EncryptedString instance = new EncryptedString();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static char[] GenerateKey()
		{
			return BasicUtils.GenerateCharArrayKey();
		}

		public static char[] GenerateKey(char[] arrayToFill)
		{
			return BasicUtils.GenerateCharArrayKey(arrayToFill);
		}

		internal static char[] InternalEncryptDecrypt(char[] value, char[] key)
		{
			if (value == null || value.Length == 0)
			{
				return value;
			}

			if (key.Length == 0)
			{
				return value;
			}

			int keyLength = key.Length;
			int valueLength = value.Length;

			char[] result = new char[valueLength];

			for (int i = 0; i < valueLength; i++)
			{
				result[i] = (char)(value[i] ^ key[i % keyLength]);
			}

			return result;
		}

		public char[] GetEncrypted(out char[] key)
		{
			key = cryptoKey;
			return hiddenChars;
		}

		public void SetEncrypted(char[] encrypted, char[] key)
		{
			initialized = true;
			hiddenChars = encrypted;
			cryptoKey = key;

			if (EncryptedCheatingDetector.ExistsAndIsRunning)
			{
				fakeValueActive = false;
				fakeValue = InternalDecryptToString();
				fakeValueActive = true;
			}
			else
			{
				fakeValueActive = false;
			}
		}

		public string GetDecrypted()
		{
			return InternalDecryptToString();
		}

		public char[] GetDecryptedToChars()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			char[] decrypted = InternalDecrypt();
			GenerateKey(cryptoKey);
			hiddenChars = InternalEncryptDecrypt(decrypted, cryptoKey); // encrypting
		}

		private string InternalDecryptToString()
		{
			return new string(InternalDecrypt());
		}

		private char[] InternalDecrypt()
		{
			if (!initialized)
			{
				cryptoKey = new char[7];
				GenerateKey(cryptoKey);
				hiddenChars = InternalEncryptDecrypt(new char[0], cryptoKey); // encrypting
				fakeValue = string.Empty;
				fakeValueActive = false;
				initialized = true;

				return new char[0];
			}

			char[] decrypted = InternalEncryptDecrypt(hiddenChars, cryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && !CompareCharsToString(decrypted, fakeValue))
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		private bool CompareCharsToString(char[] chars, string s)
		{
			if (chars.Length != s.Length) return false;

			for (int i = 0; i < chars.Length; i++)
			{
				if (chars[i] != s[i])
				{
					return false;
				}
			}

			return true;
		}

		#region operators, overrides, interface implementations

		public int Length
		{
			get { return hiddenChars.Length; }
		}

		public char this[int index]
		{
			get
			{
				if (index < 0 || index >= Length)
				{
					throw new IndexOutOfRangeException();
				}

				return InternalDecrypt()[index];
			}
		}

		public static implicit operator EncryptedString(string value)
		{
			return value == null ? null : new EncryptedString(value);
		}

		public static implicit operator string(EncryptedString value)
		{
			return value == null ? null : value.InternalDecryptToString();
		}

		public static bool operator ==(EncryptedString a, EncryptedString b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if ((object)a == null || (object)b == null)
			{
				return false;
			}

			if (a.cryptoKey == b.cryptoKey)
			{
				return ArraysEquals(a.hiddenChars, b.hiddenChars);
			}

			return ArraysEquals(a.InternalDecrypt(), b.InternalDecrypt());
		}

		public static bool operator !=(EncryptedString a, EncryptedString b)
		{
			return !(a == b);
		}

		public string Substring(int startIndex)
		{
			return Substring(startIndex, Length - startIndex);
		}

		public string Substring(int startIndex, int length)
		{
			return InternalDecryptToString().Substring(startIndex, length);
		}

		public bool StartsWith(string value, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			return InternalDecryptToString().StartsWith(value, comparisonType);
		}

		public bool EndsWith(string value, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			return InternalDecryptToString().EndsWith(value, comparisonType);
		}

		public override int GetHashCode()
		{
			return InternalDecryptToString().GetHashCode();
		}

		public override string ToString()
		{
			return new string(InternalDecrypt());
		}

		public override bool Equals(object obj)
		{
			EncryptedString EncryptedString = obj as EncryptedString;
			return EncryptedString != null && Equals(EncryptedString);
		}

		public bool Equals(EncryptedString value)
		{
			if (value == null) return false;

			if (cryptoKey == value.cryptoKey)
			{
				return ArraysEquals(hiddenChars, value.hiddenChars);
			}

			return ArraysEquals(InternalDecrypt(), value.InternalDecrypt());
		}

		public bool Equals(EncryptedString value, StringComparison comparisonType)
		{
			return value != null && string.Equals(InternalDecryptToString(), value.InternalDecryptToString(), comparisonType);
		}

		public int CompareTo(EncryptedString other)
		{
			return InternalDecryptToString().CompareTo(other.InternalDecryptToString());
		}

		public int CompareTo(string other)
		{
			return InternalDecryptToString().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is string)) throw new ArgumentException("Argument must be string");
			return CompareTo((string)obj);
		}

		#endregion

		private static bool ArraysEquals(char[] a1, char[] a2)
		{
			if (a1 == a2) return true;
			if (a1 == null || a2 == null) return false;
			if (a1.Length != a2.Length) return false;

			for (int i = 0; i < a1.Length; i++)
			{
				if (a1[i] != a2[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}