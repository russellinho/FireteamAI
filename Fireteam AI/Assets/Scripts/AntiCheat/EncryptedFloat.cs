namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;
	using System.Runtime.InteropServices;
	using UnityEngine.Serialization;

	[Serializable]
	public struct EncryptedFloat : IEncryptedType, IFormattable, IEquatable<EncryptedFloat>, IComparable<EncryptedFloat>, IComparable<float>, IComparable
	{
		[SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private int hiddenValue;

		[SerializeField]
		[FormerlySerializedAs("hiddenValue")]
#pragma warning disable 414
		private Byte4 hiddenValueOldByte4;
#pragma warning restore 414

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private float fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedFloat(float value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);
			hiddenValueOldByte4 = default(Byte4);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : 0f;
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public static int Encrypt(float value, int key)
		{
			return FloatIntBytesUnion.XorFloatToInt(value, key);
		}

		public static float Decrypt(int value, int key)
		{
			return FloatIntBytesUnion.XorIntToFloat(value, key);
		}

		public static int MigrateEncrypted(int encrypted, byte fromVersion = 0, byte toVersion = 2)
		{
			return FloatIntBytesUnion.Migrate(encrypted, fromVersion, toVersion);
		}

		public static EncryptedFloat FromEncrypted(int encrypted, int key)
		{
			EncryptedFloat instance = new EncryptedFloat();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static int GenerateKey()
		{
			return BasicUtils.GenerateIntKey();
		}

		public int GetEncrypted(out int key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(int encrypted, int key)
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

		public float GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			float decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private float InternalDecrypt()
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

			if (hiddenValueOldByte4.b1 != 0 || 
			    hiddenValueOldByte4.b2 != 0 || 
				hiddenValueOldByte4.b3 != 0 || 
				hiddenValueOldByte4.b4 != 0)
			{
				FloatIntBytesUnion union = new FloatIntBytesUnion {b4 = hiddenValueOldByte4};
				union.b4.Shuffle();
				hiddenValue = union.i;

				hiddenValueOldByte4.b1 = 0;
				hiddenValueOldByte4.b2 = 0;
				hiddenValueOldByte4.b3 = 0;
				hiddenValueOldByte4.b4 = 0;
			}

			float decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue) > EncryptedCheatingDetector.Instance.floatEpsilon)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedFloat(float i)
		{
			return new EncryptedFloat(i);
		}

		public static implicit operator float(EncryptedFloat i)
		{
			return i.InternalDecrypt();
		}

		public static EncryptedFloat operator ++(EncryptedFloat i)
		{
			return Increment(i, 1);
		}

		public static EncryptedFloat operator --(EncryptedFloat i)
		{
			return Increment(i, -1);
		}

		private static EncryptedFloat Increment(EncryptedFloat input, int increment)
		{
			float decrypted = input.InternalDecrypt() + increment;
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
			return obj is EncryptedFloat && Equals((EncryptedFloat)obj);
		}

		public bool Equals(EncryptedFloat obj)
		{
			return obj.InternalDecrypt().Equals(InternalDecrypt());
		}

		public int CompareTo(EncryptedFloat other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(float other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			if (obj == null) return 1;
			if (!(obj is float)) throw new ArgumentException("Argument must be float");
			return CompareTo((float)obj);
		}

		#endregion

		[StructLayout(LayoutKind.Explicit)]
		internal struct FloatIntBytesUnion
		{
			[FieldOffset(0)]
			internal float f;

			[FieldOffset(0)]
			internal int i;

			[FieldOffset(0)]
			internal Byte4 b4;

			public static int Migrate(int value, byte fromVersion, byte toVersion)
			{
				FloatIntBytesUnion u = FromInt(value);

				if (fromVersion < 2 && toVersion == 2)
				{
					u.b4.Shuffle();
				}

				return u.i;
			}

			internal static int XorFloatToInt(float value, int key)
			{
				return FromFloat(value).Shuffle(key).i;
			}

			internal static float XorIntToFloat(int value, int key)
			{
				return FromInt(value).UnShuffle(key).f;
			}

			private static FloatIntBytesUnion FromFloat(float value)
			{
				return new FloatIntBytesUnion { f = value};
			}

			private static FloatIntBytesUnion FromInt(int value)
			{
				return new FloatIntBytesUnion { i = value};
			}

			private FloatIntBytesUnion Shuffle(int key)
			{
				i ^= key;
				b4.Shuffle();

				return this;
			}

			private FloatIntBytesUnion UnShuffle(int key)
			{
				b4.UnShuffle();
				i ^= key;

				return this;
			}
		}
	}
}