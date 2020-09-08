namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct EncryptedVector3 : IEncryptedType
	{
		private static readonly Vector3 Zero = Vector3.zero;

		[SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private RawEncryptedVector3 hiddenValue;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private Vector3 fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedVector3(Vector3 value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : Zero;
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public EncryptedVector3(float x, float y, float z)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(x,y,z, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning)
			{
				fakeValue = new Vector3(x, y, z);
				fakeValueActive = true;
			}
			else
			{
				fakeValue = Zero;
				fakeValueActive = false;
			}

			initialized = true;
		}

		public float x
		{
			get
			{
				float decrypted = EncryptedFloat.Decrypt(hiddenValue.x, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.x) > EncryptedCheatingDetector.Instance.vector3Epsilon)
				{
					EncryptedCheatingDetector.Instance.OnCheatingDetected();
				}
				return decrypted;
			}

			set
			{
				hiddenValue.x = EncryptedFloat.Encrypt(value, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning)
				{
					fakeValue.x = value;
					fakeValue.y = EncryptedFloat.Decrypt(hiddenValue.y, currentCryptoKey);
					fakeValue.z = EncryptedFloat.Decrypt(hiddenValue.z, currentCryptoKey);
					fakeValueActive = true;
				}
				else
				{
					fakeValueActive = false;
				}
			}
		}

		public float y
		{
			get
			{
				float decrypted = EncryptedFloat.Decrypt(hiddenValue.y, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.y) > EncryptedCheatingDetector.Instance.vector3Epsilon)
				{
					EncryptedCheatingDetector.Instance.OnCheatingDetected();
				}
				return decrypted;
			}

			set
			{
				hiddenValue.y = EncryptedFloat.Encrypt(value, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning)
				{
					fakeValue.x = EncryptedFloat.Decrypt(hiddenValue.x, currentCryptoKey);
					fakeValue.y = value;
					fakeValue.z = EncryptedFloat.Decrypt(hiddenValue.z, currentCryptoKey);
					fakeValueActive = true;
				}
				else
				{
					fakeValueActive = false;
				}
			}
		}

		public float z
		{
			get
			{
				float decrypted = EncryptedFloat.Decrypt(hiddenValue.z, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.z) > EncryptedCheatingDetector.Instance.vector3Epsilon)
				{
					EncryptedCheatingDetector.Instance.OnCheatingDetected();
				}
				return decrypted;
			}

			set
			{
				hiddenValue.z = EncryptedFloat.Encrypt(value, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning)
				{
					fakeValue.x = EncryptedFloat.Decrypt(hiddenValue.x, currentCryptoKey);
					fakeValue.y = EncryptedFloat.Decrypt(hiddenValue.y, currentCryptoKey);
					fakeValue.z = value;
					fakeValueActive = true;
				}
				else
				{
					fakeValueActive = false;
				}
			}
		}

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return x;
					case 1:
						return y;
					case 2:
						return z;
					default:
						throw new IndexOutOfRangeException("Invalid EncryptedVector3 index!");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						x = value;
						break;
					case 1:
						y = value;
						break;
					case 2:
						z = value;
						break;
					default:
						throw new IndexOutOfRangeException("Invalid EncryptedVector3 index!");
				}
			}
		}

		public static RawEncryptedVector3 Encrypt(Vector3 value, int key)
		{
			return Encrypt(value.x, value.y, value.z, key);
		}

		public static RawEncryptedVector3 Encrypt(float x, float y, float z, int key)
		{
			RawEncryptedVector3 result;
			result.x = EncryptedFloat.Encrypt(x, key);
			result.y = EncryptedFloat.Encrypt(y, key);
			result.z = EncryptedFloat.Encrypt(z, key);

			return result;
		}

		public static Vector3 Decrypt(RawEncryptedVector3 value, int key)
		{
			Vector3 result;
			result.x = EncryptedFloat.Decrypt(value.x, key);
			result.y = EncryptedFloat.Decrypt(value.y, key);
			result.z = EncryptedFloat.Decrypt(value.z, key);

			return result;
		}

		public static EncryptedVector3 FromEncrypted(RawEncryptedVector3 encrypted, int key)
		{
			EncryptedVector3 instance = new EncryptedVector3();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static int GenerateKey()
		{
			return BasicUtils.GenerateIntKey();
		}

		private static bool CompareVectorsWithTolerance(Vector3 vector1, Vector3 vector2)
		{
			float epsilon = EncryptedCheatingDetector.Instance.vector3Epsilon;
			return Math.Abs(vector1.x - vector2.x) < epsilon &&
			       Math.Abs(vector1.y - vector2.y) < epsilon &&
			       Math.Abs(vector1.z - vector2.z) < epsilon;
		}

		public RawEncryptedVector3 GetEncrypted(out int key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(RawEncryptedVector3 encrypted, int key)
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

		public Vector3 GetDecrypted() 
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			Vector3 decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private Vector3 InternalDecrypt()
		{
			if (!initialized)
			{
				currentCryptoKey = GenerateKey();
				hiddenValue = Encrypt(Zero, currentCryptoKey);
				fakeValue = Zero;
				fakeValueActive = false;
				initialized = true;

				return Zero;
			}

			Vector3 decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && !CompareVectorsWithTolerance(decrypted, fakeValue))
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedVector3(Vector3 value)
		{
			return new EncryptedVector3(value);
		}

		public static implicit operator Vector3(EncryptedVector3 value)
		{
			return value.InternalDecrypt();
		}

		public static EncryptedVector3 operator +(EncryptedVector3 a, EncryptedVector3 b)
		{
			return a.InternalDecrypt() + b.InternalDecrypt();
		}

		public static EncryptedVector3 operator +(Vector3 a, EncryptedVector3 b)
		{
			return a + b.InternalDecrypt();
		}

		public static EncryptedVector3 operator +(EncryptedVector3 a, Vector3 b)
		{
			return a.InternalDecrypt() + b;
		}

		public static EncryptedVector3 operator -(EncryptedVector3 a, EncryptedVector3 b)
		{
			return a.InternalDecrypt() - b.InternalDecrypt();
		}

		public static EncryptedVector3 operator -(Vector3 a, EncryptedVector3 b)
		{
			return a - b.InternalDecrypt();
		}

		public static EncryptedVector3 operator -(EncryptedVector3 a, Vector3 b)
		{
			return a.InternalDecrypt() - b;
		}

		public static EncryptedVector3 operator -(EncryptedVector3 a)
		{
			return -a.InternalDecrypt();
		}

		public static EncryptedVector3 operator *(EncryptedVector3 a, float d)
		{
			return a.InternalDecrypt() * d;
		}

		public static EncryptedVector3 operator *(float d, EncryptedVector3 a)
		{
			return d * a.InternalDecrypt();
		}

		public static EncryptedVector3 operator /(EncryptedVector3 a, float d)
		{
			return a.InternalDecrypt() / d;
		}

		public static bool operator ==(EncryptedVector3 lhs, EncryptedVector3 rhs)
		{
			return lhs.InternalDecrypt() == rhs.InternalDecrypt();
		}

		public static bool operator ==(Vector3 lhs, EncryptedVector3 rhs)
		{
			return lhs == rhs.InternalDecrypt();
		}

		public static bool operator ==(EncryptedVector3 lhs, Vector3 rhs)
		{
			return lhs.InternalDecrypt() == rhs;
		}

		public static bool operator !=(EncryptedVector3 lhs, EncryptedVector3 rhs)
		{
			return lhs.InternalDecrypt() != rhs.InternalDecrypt();
		}

		public static bool operator !=(Vector3 lhs, EncryptedVector3 rhs)
		{
			return lhs != rhs.InternalDecrypt();
		}

		public static bool operator !=(EncryptedVector3 lhs, Vector3 rhs)
		{
			return lhs.InternalDecrypt() != rhs;
		}

		public override bool Equals(object other)
		{
			return InternalDecrypt().Equals(other);
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

		#endregion

		/// <summary>
		/// Used to store encrypted Vector3.
		/// </summary>
		[Serializable]
		public struct RawEncryptedVector3
		{
			/// <summary>
			/// Encrypted value
			/// </summary>
			public int x;

			/// <summary>
			/// Encrypted value
			/// </summary>
			public int y;

			/// <summary>
			/// Encrypted value
			/// </summary>
			public int z;
		}
	}
}