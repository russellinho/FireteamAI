namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct EncryptedVector2Int : IEncryptedType
	{
		private static readonly Vector2Int Zero = Vector2Int.zero;

		[SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private RawEncryptedVector2Int hiddenValue;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private Vector2Int fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedVector2Int(Vector2Int value)
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

		public EncryptedVector2Int(int x, int y)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(x, y, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning)
			{
				fakeValue = new Vector2Int(x, y);
				fakeValueActive = true;
			}
			else
			{
				fakeValue = Zero;
				fakeValueActive = false;
			}

			initialized = true;
		}

		public int x
		{
			get
			{
				int decrypted = EncryptedInt.Decrypt(hiddenValue.x, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.x) > 0)
				{
					EncryptedCheatingDetector.Instance.OnCheatingDetected();
				}
				return decrypted;
			}

			set
			{
				hiddenValue.x = EncryptedInt.Encrypt(value, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning)
				{
					fakeValue.x = value;
					fakeValue.y = EncryptedInt.Decrypt(hiddenValue.y, currentCryptoKey);
					fakeValueActive = true;
				}
				else
				{
					fakeValueActive = false;
				}
			}
		}

		public int y
		{
			get
			{
				int decrypted = EncryptedInt.Decrypt(hiddenValue.y, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.y) > 0)
				{
					EncryptedCheatingDetector.Instance.OnCheatingDetected();
				}
				return decrypted;
			}

			set
			{
				hiddenValue.y = EncryptedInt.Encrypt(value, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning)
				{
					fakeValue.x = EncryptedInt.Decrypt(hiddenValue.x, currentCryptoKey);
					fakeValue.y = value;
					fakeValueActive = true;
				}
				else
				{
					fakeValueActive = false;
				}
			}
		}

		public int this[int index]
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
						throw new IndexOutOfRangeException("Invalid EncryptedVector2Int index!");
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
						throw new IndexOutOfRangeException("Invalid EncryptedVector2Int index!");
				}
			}
		}

		public static RawEncryptedVector2Int Encrypt(Vector2Int value, int key)
		{
			return Encrypt(value.x, value.y, key);
		}

		public static RawEncryptedVector2Int Encrypt(int x, int y, int key)
		{
			RawEncryptedVector2Int result;
			result.x = EncryptedInt.Encrypt(x, key);
			result.y = EncryptedInt.Encrypt(y, key);

			return result;
		}

		public static Vector2Int Decrypt(RawEncryptedVector2Int value, int key)
		{
			Vector2Int result = new Vector2Int
			{
				x = EncryptedInt.Decrypt(value.x, key),
				y = EncryptedInt.Decrypt(value.y, key)
			};

			return result;
		}

		public static EncryptedVector2Int FromEncrypted(RawEncryptedVector2Int encrypted, int key)
		{
			EncryptedVector2Int instance = new EncryptedVector2Int();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static int GenerateKey()
		{
			return BasicUtils.GenerateIntKey();
		}

		public RawEncryptedVector2Int GetEncrypted(out int key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(RawEncryptedVector2Int encrypted, int key)
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

		/// <summary>
		/// Alternative to the type cast, use if you wish to get decrypted value 
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value.</returns>
		public Vector2Int GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			Vector2Int decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private Vector2Int InternalDecrypt()
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

			Vector2Int decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedVector2Int(Vector2Int value)
		{
			return new EncryptedVector2Int(value);
		}

		public static implicit operator Vector2Int(EncryptedVector2Int value)
		{
			return value.InternalDecrypt();
		}

		public static implicit operator Vector2(EncryptedVector2Int value)
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

		#endregion

		/// <summary>
		/// Used to store encrypted Vector2.
		/// </summary>
		[Serializable]
		public struct RawEncryptedVector2Int
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