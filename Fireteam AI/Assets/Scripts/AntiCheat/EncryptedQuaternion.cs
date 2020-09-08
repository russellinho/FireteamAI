namespace Koobando.AntiCheat
{
	using System;
	using UnityEngine;

	[Serializable]
	public struct EncryptedQuaternion : IEncryptedType
	{
		private static readonly Quaternion Identity = Quaternion.identity;

		[SerializeField]
		private int currentCryptoKey;

		[SerializeField]
		private RawEncryptedQuaternion hiddenValue;

		[SerializeField]
		private bool initialized;

		[SerializeField]
		private Quaternion fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private EncryptedQuaternion(Quaternion value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			bool detectorRunning = EncryptedCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : Identity;
			fakeValueActive = detectorRunning;
#endif
			initialized = true;
		}

		public EncryptedQuaternion(float x, float y, float z, float w)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(x, y, z, w, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning)
			{
				fakeValue = new Quaternion(x, y, z, w);
				fakeValueActive = true;
			}
			else
			{
				fakeValue = Identity;
				fakeValueActive = false;
			}

			initialized = true;
		}

		public static RawEncryptedQuaternion Encrypt(Quaternion value, int key)
		{
			return Encrypt(value.x, value.y, value.z, value.w, key);
		}

		public static RawEncryptedQuaternion Encrypt(float x, float y, float z, float w, int key)
		{
			RawEncryptedQuaternion result;
			result.x = EncryptedFloat.Encrypt(x, key);
			result.y = EncryptedFloat.Encrypt(y, key);
			result.z = EncryptedFloat.Encrypt(z, key);
			result.w = EncryptedFloat.Encrypt(w, key);

			return result;
		}

		public static Quaternion Decrypt(RawEncryptedQuaternion value, int key)
		{
			Quaternion result;
			result.x = EncryptedFloat.Decrypt(value.x, key);
			result.y = EncryptedFloat.Decrypt(value.y, key);
			result.z = EncryptedFloat.Decrypt(value.z, key);
			result.w = EncryptedFloat.Decrypt(value.w, key);

			return result;
		}

		public static EncryptedQuaternion FromEncrypted(RawEncryptedQuaternion encrypted, int key)
		{
			EncryptedQuaternion instance = new EncryptedQuaternion();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		public static int GenerateKey()
		{
			return BasicUtils.GenerateIntKey();
		}

		private static bool CompareQuaternionsWithTolerance(Quaternion q1, Quaternion q2)
		{
			float epsilon = EncryptedCheatingDetector.Instance.quaternionEpsilon;
			return Math.Abs(q1.x - q2.x) < epsilon &&
			       Math.Abs(q1.y - q2.y) < epsilon &&
			       Math.Abs(q1.z - q2.z) < epsilon &&
			       Math.Abs(q1.w - q2.w) < epsilon;
		}

		public RawEncryptedQuaternion GetEncrypted(out int key)
		{
			key = currentCryptoKey;
			return hiddenValue;
		}

		public void SetEncrypted(RawEncryptedQuaternion encrypted, int key)
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

		public Quaternion GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			Quaternion decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private Quaternion InternalDecrypt()
		{
			if (!initialized)
			{
				currentCryptoKey = GenerateKey();
				hiddenValue = Encrypt(Identity, currentCryptoKey);
				fakeValue = Identity;
				fakeValueActive = false;
				initialized = true;

				return Identity;
			}

			Quaternion decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (EncryptedCheatingDetector.ExistsAndIsRunning && fakeValueActive && !CompareQuaternionsWithTolerance(decrypted, fakeValue))
			{
				EncryptedCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}

		#region operators, overrides, interface implementations

		//! @cond
		public static implicit operator EncryptedQuaternion(Quaternion value)
		{
			return new EncryptedQuaternion(value);
		}

		public static implicit operator Quaternion(EncryptedQuaternion value)
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

		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		#endregion

		/// <summary>
		/// Used to store encrypted Quaternion.
		/// </summary>
		[Serializable]
		public struct RawEncryptedQuaternion
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

			/// <summary>
			/// Encrypted value
			/// </summary>
			public int w;
		}
	}
}