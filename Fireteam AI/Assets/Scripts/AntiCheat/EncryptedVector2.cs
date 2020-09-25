namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct EncryptedVector2 : IEncryptedType
	{
		private static readonly Vector2 Zero = Vector2.zero;

		[SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private RawEncryptedVector2 hiddenValue;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private Vector2 fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedVector2(Vector2 value)
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

		public EncryptedVector2(float x, float y)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(x, y, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning)
			{
				fakeValue = new Vector2(x, y);
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
				if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.x) > EncryptedCheatingDetector.Instance.vector2Epsilon)
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
				if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.y) > EncryptedCheatingDetector.Instance.vector2Epsilon)
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
					default:
						throw new IndexOutOfRangeException("Invalid EncryptedVector2 index!");
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
					default:
						throw new IndexOutOfRangeException("Invalid EncryptedVector2 index!");
				}
			}
		}

		public static RawEncryptedVector2 Encrypt(Vector2 value, int key)
		{
			return Encrypt(value.x, value.y, key);
		}

		public static RawEncryptedVector2 Encrypt(float x, float y, int key)
		{
			RawEncryptedVector2 result;
			result.x = EncryptedFloat.Encrypt(x, key);
			result.y = EncryptedFloat.Encrypt(y, key);

			return result;
		}

		public static Vector2 Decrypt(RawEncryptedVector2 value, int key)
		{
			Vector2 result;
			result.x = EncryptedFloat.Decrypt(value.x, key);
			result.y = EncryptedFloat.Decrypt(value.y, key);

			return result;
		}

		public static EncryptedVector2 FromEncrypted(RawEncryptedVector2 encrypted, int key)
		{
			EncryptedVector2 instance = new EncryptedVector2();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static int GenerateKey()
		{
			return BasicUtils.GenerateIntKey();
		}

		private static bool CompareVectorsWithTolerance(Vector2 vector1, Vector2 vector2)
		{
			float epsilon = EncryptedCheatingDetector.Instance.vector2Epsilon;
			return Math.Abs(vector1.x - vector2.x) < epsilon &&
			       Math.Abs(vector1.y - vector2.y) < epsilon;
		}

		public RawEncryptedVector2 GetEncrypted(out int key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(RawEncryptedVector2 encrypted, int key)
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

		public Vector2 GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			Vector2 decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private Vector2 InternalDecrypt()
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

			Vector2 decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && !CompareVectorsWithTolerance(decrypted, fakeValue))
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedVector2(Vector2 value)
		{
			return new EncryptedVector2(value);
		}

		public static implicit operator Vector2(EncryptedVector2 value)
		{
			return value.InternalDecrypt();
		}

		public static implicit operator Vector3(EncryptedVector2 value)
		{
			Vector2 v = value.InternalDecrypt();
			return new Vector3(v.x, v.y, 0.0f);
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
		/// Used to store encrypted Vector2.
		/// </summary>
		[Serializable]
		public struct RawEncryptedVector2
		{
			/// <summary>
			/// Encrypted value
			/// </summary>
			public int x;

			/// <summary>
			/// Encrypted value
			/// </summary>
			public int y;
		}
	
	}
}