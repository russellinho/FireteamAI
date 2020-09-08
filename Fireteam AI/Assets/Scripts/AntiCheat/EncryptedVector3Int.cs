namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct EncryptedVector3Int : IEncryptedType
	{
		private static readonly Vector3Int Zero = Vector3Int.zero;

		[SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private RawEncryptedVector3Int hiddenValue;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private Vector3Int fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedVector3Int(Vector3Int value)
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

		public EncryptedVector3Int(int x, int y, int z)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(x, y, z, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning)
			{
				fakeValue = new Vector3Int
				{
					x = x,
					y = y,
					z = z
				};
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
					fakeValue.z = EncryptedInt.Decrypt(hiddenValue.z, currentCryptoKey);
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
					fakeValue.z = EncryptedInt.Decrypt(hiddenValue.z, currentCryptoKey);
					fakeValueActive = true;
				}
				else
				{
					fakeValueActive = false;
				}
			}
		}

		public int z
		{
			get
			{
				int decrypted = EncryptedInt.Decrypt(hiddenValue.z, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.z) > 0)
				{
					EncryptedCheatingDetector.Instance.OnCheatingDetected();
				}
				return decrypted;
			}

			set
			{
				hiddenValue.z = EncryptedInt.Encrypt(value, currentCryptoKey);
				if (EncryptedCheatingDetector.ExistsAndIsRunning)
				{
					fakeValue.x = EncryptedInt.Decrypt(hiddenValue.x, currentCryptoKey);
					fakeValue.y = EncryptedInt.Decrypt(hiddenValue.y, currentCryptoKey);
					fakeValue.z = value;
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
					case 2:
						return z;
					default:
						throw new IndexOutOfRangeException("Invalid EncryptedVector3Int index!");
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
						throw new IndexOutOfRangeException("Invalid EncryptedVector3Int index!");
				}
			}
		}

		public static RawEncryptedVector3Int Encrypt(Vector3Int value, int key)
		{
			return Encrypt(value.x, value.y, value.z, key);
		}

		public static RawEncryptedVector3Int Encrypt(int x, int y, int z, int key)
		{
			RawEncryptedVector3Int result;
			result.x = EncryptedInt.Encrypt(x, key);
			result.y = EncryptedInt.Encrypt(y, key);
			result.z = EncryptedInt.Encrypt(z, key);

			return result;
		}

		public static Vector3Int Decrypt(RawEncryptedVector3Int value, int key)
		{
			Vector3Int result = new Vector3Int
			{
				x = EncryptedInt.Decrypt(value.x, key),
				y = EncryptedInt.Decrypt(value.y, key),
				z = EncryptedInt.Decrypt(value.z, key)
			};

			return result;
		}

		public static EncryptedVector3Int FromEncrypted(RawEncryptedVector3Int encrypted, int key)
		{
			EncryptedVector3Int instance = new EncryptedVector3Int();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static int GenerateKey()
		{
			return BasicUtils.GenerateIntKey();
		}

		public RawEncryptedVector3Int GetEncrypted(out int key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(RawEncryptedVector3Int encrypted, int key)
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

		public Vector3Int GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			Vector3Int decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private Vector3Int InternalDecrypt()
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

			Vector3Int decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		public static implicit operator EncryptedVector3Int(Vector3Int value)
		{
			return new EncryptedVector3Int(value);
		}

		public static implicit operator Vector3Int(EncryptedVector3Int value)
		{
			return value.InternalDecrypt();
		}

		public static implicit operator Vector3(EncryptedVector3Int value)
		{
			return value.InternalDecrypt();
		}

		public static EncryptedVector3Int operator +(EncryptedVector3Int a, EncryptedVector3Int b)
		{
			return a.InternalDecrypt() + b.InternalDecrypt();
		}

		public static EncryptedVector3Int operator +(Vector3Int a, EncryptedVector3Int b)
		{
			return a + b.InternalDecrypt();
		}

		public static EncryptedVector3Int operator +(EncryptedVector3Int a, Vector3Int b)
		{
			return a.InternalDecrypt() + b;
		}

		public static EncryptedVector3Int operator -(EncryptedVector3Int a, EncryptedVector3Int b)
		{
			return a.InternalDecrypt() - b.InternalDecrypt();
		}

		public static EncryptedVector3Int operator -(Vector3Int a, EncryptedVector3Int b)
		{
			return a - b.InternalDecrypt();
		}

		public static EncryptedVector3Int operator -(EncryptedVector3Int a, Vector3Int b)
		{
			return a.InternalDecrypt() - b;
		}

		public static EncryptedVector3Int operator *(EncryptedVector3Int a, int d)
		{
			return a.InternalDecrypt() * d;
		}

		public static bool operator ==(EncryptedVector3Int lhs, EncryptedVector3Int rhs)
		{
			return lhs.InternalDecrypt() == rhs.InternalDecrypt();
		}

		public static bool operator ==(Vector3Int lhs, EncryptedVector3Int rhs)
		{
			return lhs == rhs.InternalDecrypt();
		}

		public static bool operator ==(EncryptedVector3Int lhs, Vector3Int rhs)
		{
			return lhs.InternalDecrypt() == rhs;
		}

		public static bool operator !=(EncryptedVector3Int lhs, EncryptedVector3Int rhs)
		{
			return lhs.InternalDecrypt() != rhs.InternalDecrypt();
		}

		public static bool operator !=(Vector3Int lhs, EncryptedVector3Int rhs)
		{
			return lhs != rhs.InternalDecrypt();
		}

		public static bool operator !=(EncryptedVector3Int lhs, Vector3Int rhs)
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
		/// Used to store encrypted Vector3Int.
		/// </summary>
		[Serializable]
		public struct RawEncryptedVector3Int
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